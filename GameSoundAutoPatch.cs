using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace DebugHeroFileDungeonRPG
{
    /// <summary>
    /// 기존 게임 파일을 수정하지 않고 추가만 해서 사운드를 자동 적용하는 패치입니다.
    ///
    /// 적용 방식:
    /// - ModuleInitializer가 앱 실행 후 열린 MainForm을 찾아 사운드 훅을 설치합니다.
    /// - MainForm.cs / Renderer.cs / PlayerMovementSystem.cs / .csproj를 직접 수정하지 않습니다.
    /// - 사운드는 Assets\SoundPatch 아래 MP3 파일을 사용합니다.
    /// - Directory.Build.targets가 빌드 출력 폴더로 MP3를 복사하지만,
    ///   복사가 누락되어도 이 파일이 프로젝트 루트의 Assets\SoundPatch를 위쪽 폴더에서 자동 탐색합니다.
    ///
    /// 재생 규칙:
    /// - 기본 화면/상점/대화 후 화면: 기본 맵 배경음 반복
    /// - 일반/잡몹 스테이지: 잡몹 배경음 반복
    /// - 보스 스테이지: 최종 보스 배경음 반복 + 보스 몬스터 사운드 반복
    /// - Q 공격: 키 입력으로 실제 공격 애니메이션이 시작될 때만 재생
    /// - W/R 스킬: 실제 발동 성공 후에만 재생
    /// - E 방어막: 실제 방어막 생성 성공 후에만 재생
    /// - 데미지: 플레이어 HP가 실제로 감소했을 때만 즉시 재생
    /// - 방어 성공: 방어/실드 상태에서 몬스터 공격을 막거나 줄였을 때만 재생
    /// - NPC 대화: NPC 대화창이 열려 있는 동안 반복
    /// - 상점 결제: 코인이 차감되고 아이템이 증가한 성공 구매 때만 재생
    /// </summary>
    internal static class GameSoundAutoPatchBootstrap
    {
        [ModuleInitializer]
        internal static void InitializeSoundPatch()
        {
            try
            {
                if (Environment.GetEnvironmentVariable("DEBUGHERO_DISABLE_SOUND_PATCH") == "1")
                    return;

                Application.Idle += TryInstallOnIdle;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[GameSoundAutoPatch] bootstrap failed: " + ex.Message);
            }
        }

        private static void TryInstallOnIdle(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < Application.OpenForms.Count; i++)
                {
                    MainForm form = Application.OpenForms[i] as MainForm;
                    if (form == null) continue;

                    form.GameSoundPatchInstall();
                    Application.Idle -= TryInstallOnIdle;
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[GameSoundAutoPatch] idle install failed: " + ex.Message);
            }
        }
    }

    public sealed partial class MainForm
    {
        private const string GameSoundPatchFolderName = "SoundPatch";
        private const int GameSoundPatchBgmVolume = 310;
        private const int GameSoundPatchBossAmbientVolume = 230;
        private const int GameSoundPatchNpcVolume = 260;
        private const int GameSoundPatchFxVolume = 850;

        private bool gameSoundPatchInstalled = false;
        private string gameSoundPatchCurrentBgmKey = "";
        private string gameSoundPatchCurrentBgmAlias = "";
        private string gameSoundPatchBossLoopAlias = "";
        private string gameSoundPatchNpcLoopAlias = "";
        private int gameSoundPatchFxCounter = 0;
        private int gameSoundPatchLastHp = -1;
        private int gameSoundPatchLastShield = -1;
        private int gameSoundPatchLastDefenseTicks = 0;
        private int gameSoundPatchLastCoins = -1;
        private int gameSoundPatchLastHpPotions = -1;
        private int gameSoundPatchLastMpPotions = -1;
        private int gameSoundPatchLastSkySwordCount = 0;
        private int gameSoundPatchLastSkillIndex = -1;
        private int gameSoundPatchLastSkillTick = -9999;
        private DateTime gameSoundPatchLastMouseClickUtc = DateTime.MinValue;
        private readonly Dictionary<string, string> gameSoundPatchPathCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DateTime> gameSoundPatchCooldowns = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        private readonly List<GameSoundPatchFxAlias> gameSoundPatchFxAliases = new List<GameSoundPatchFxAlias>();

        [DllImport("winmm.dll", EntryPoint = "mciSendStringW", CharSet = CharSet.Unicode, SetLastError = false)]
        private static extern int GameSoundPatchMciSendString(string command, StringBuilder returnValue, int returnLength, IntPtr hwndCallback);

        internal void GameSoundPatchInstall()
        {
            if (gameSoundPatchInstalled) return;
            gameSoundPatchInstalled = true;

            try
            {
                gameSoundPatchLastHp = player.Hp;
                gameSoundPatchLastShield = playerShield;
                gameSoundPatchLastDefenseTicks = player.DefenseTicks;
                gameSoundPatchLastCoins = player.Coins;
                gameSoundPatchLastHpPotions = player.HpPotions;
                gameSoundPatchLastMpPotions = player.MpPotions;
                gameSoundPatchLastSkySwordCount = playerSkySwords.Count;

                EnsureSoundPatchOutputFolder();

                KeyDown += GameSoundPatch_KeyDown;
                MouseDown += GameSoundPatch_MouseDown;
                FormClosing += GameSoundPatch_FormClosing;

                if (timer != null)
                    timer.Tick += GameSoundPatch_Tick;

                GameSoundPatch_Tick(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[GameSoundAutoPatch] install failed: " + ex.Message);
            }
        }

        private void GameSoundPatch_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (screen != ScreenMode.Stage || currentStage <= 0 || IsStageNpcHintOpen())
                    return;

                if (e.KeyCode == Keys.Q)
                {
                    // 원본 KeyDown 처리 뒤에 실행되므로 실제 Q 공격 애니메이션이 시작된 경우에만 재생됩니다.
                    if (player.ActionState == PlayerActionState.Skill && player.SkillIndex == 0 && player.ActionTick <= 1)
                    {
                        PlaySoundPatchEffect("player_attack.mp3", "attack", 70, GameSoundPatchFxVolume);
                    }
                }
                else if (e.KeyCode == Keys.W)
                {
                    // W 오버클럭이 성공하면 원본 코드가 wBuffTicks=300, wCooldownTicks=900으로 설정합니다.
                    if (wBuffTicks >= 295 && wCooldownTicks >= 890)
                    {
                        PlaySoundPatchEffect("skill_cast.mp3", "skill_w", 220, GameSoundPatchFxVolume);
                    }
                }
                else if (e.KeyCode == Keys.E)
                {
                    // E 방어막이 성공하면 원본 코드가 eShieldDurationTicks/playerShield/eCooldownTicks를 설정합니다.
                    if (playerShield > 0 && eShieldDurationTicks >= 295 && eCooldownTicks >= 1190)
                    {
                        PlaySoundPatchEffect("shield_cast.mp3", "shield_cast", 260, GameSoundPatchFxVolume);
                    }
                }
                else if (e.KeyCode == Keys.R)
                {
                    // R 궁극기가 성공하면 원본 코드가 rCooldownTicks=1500으로 설정하고 검 투하 객체를 추가합니다.
                    if (rCooldownTicks >= 1490 || playerSkySwords.Count > gameSoundPatchLastSkySwordCount)
                    {
                        PlaySoundPatchEffect("skill_cast.mp3", "skill_r", 300, GameSoundPatchFxVolume);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[GameSoundAutoPatch] key sound failed: " + ex.Message);
            }
        }

        private void GameSoundPatch_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                // 스테이지 우클릭은 캐릭터 이동 명령이므로 별도 클릭음을 내지 않습니다.
                if (screen == ScreenMode.Stage && e.Button == MouseButtons.Right)
                    return;

                DateTime now = DateTime.UtcNow;
                if ((now - gameSoundPatchLastMouseClickUtc).TotalMilliseconds < 120)
                    return;

                gameSoundPatchLastMouseClickUtc = now;
                PlaySoundPatchEffect("ui_click.mp3", "mouse_click", 100, 720);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[GameSoundAutoPatch] mouse sound failed: " + ex.Message);
            }
        }

        private void GameSoundPatch_Tick(object sender, EventArgs e)
        {
            try
            {
                CleanupFinishedSoundPatchEffects();
                UpdateSoundPatchLoopingTracks();
                UpdateSoundPatchStateDrivenEffects();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[GameSoundAutoPatch] tick failed: " + ex.Message);
            }
        }

        private void UpdateSoundPatchLoopingTracks()
        {
            string desiredBgm = GetDesiredSoundPatchBgmKey();

            if (desiredBgm != gameSoundPatchCurrentBgmKey)
            {
                StopSoundPatchLoop(ref gameSoundPatchCurrentBgmAlias);
                gameSoundPatchCurrentBgmKey = desiredBgm;

                if (desiredBgm == "base")
                {
                    gameSoundPatchCurrentBgmAlias = StartSoundPatchLoop("sp_loop_bgm_base", "bgm_base_map.mp3", GameSoundPatchBgmVolume);
                }
                else if (desiredBgm == "mob")
                {
                    gameSoundPatchCurrentBgmAlias = StartSoundPatchLoop("sp_loop_bgm_mob", "bgm_mob_dungeon.mp3", GameSoundPatchBgmVolume);
                }
                else if (desiredBgm == "boss")
                {
                    gameSoundPatchCurrentBgmAlias = StartSoundPatchLoop("sp_loop_bgm_boss", "bgm_final_boss.mp3", GameSoundPatchBgmVolume);
                }
            }

            bool shouldPlayBossAmbient = screen == ScreenMode.Stage && stageBossPhase && currentStage > 0 && HasAliveSoundPatchBoss();
            if (shouldPlayBossAmbient)
            {
                if (string.IsNullOrEmpty(gameSoundPatchBossLoopAlias))
                    gameSoundPatchBossLoopAlias = StartSoundPatchLoop("sp_loop_boss_monster", "boss_monster_loop.mp3", GameSoundPatchBossAmbientVolume);
            }
            else
            {
                StopSoundPatchLoop(ref gameSoundPatchBossLoopAlias);
            }

            bool shouldPlayNpc = IsSoundPatchNpcTalking();
            if (shouldPlayNpc)
            {
                if (string.IsNullOrEmpty(gameSoundPatchNpcLoopAlias))
                    gameSoundPatchNpcLoopAlias = StartSoundPatchLoop("sp_loop_npc_dialogue", "npc_dialogue_loop.mp3", GameSoundPatchNpcVolume);
            }
            else
            {
                StopSoundPatchLoop(ref gameSoundPatchNpcLoopAlias);
            }
        }

        private void UpdateSoundPatchStateDrivenEffects()
        {
            if (screen == ScreenMode.Stage && currentStage > 0)
            {
                if (gameSoundPatchLastHp >= 0 && player.Hp < gameSoundPatchLastHp)
                {
                    bool defended = gameSoundPatchLastDefenseTicks > 0 || player.DefenseTicks > 0;
                    if (defended)
                    {
                        PlaySoundPatchEffect("defense_success.mp3", "defense_success", 180, GameSoundPatchFxVolume);
                    }
                    else
                    {
                        string damageFile = random.Next(0, 2) == 0 ? "damage_light.mp3" : "damage_heavy.mp3";
                        PlaySoundPatchEffect(damageFile, "player_damage", 150, GameSoundPatchFxVolume);
                    }
                }

                // E 방어막이 실제로 데미지를 상쇄한 경우: HP가 유지되어도 shield 값이 감소합니다.
                if (gameSoundPatchLastShield > 0 && playerShield < gameSoundPatchLastShield)
                {
                    PlaySoundPatchEffect("defense_success.mp3", "shield_block_success", 190, GameSoundPatchFxVolume);
                }

                // 혹시 UI/버튼 등에서 CastSkill(1~3)이 호출되는 경우도 스킬 발동음이 빠지지 않게 보조 감지합니다.
                if (player.ActionState == PlayerActionState.Skill && player.ActionTick <= 1)
                {
                    if (player.SkillIndex != gameSoundPatchLastSkillIndex || tick - gameSoundPatchLastSkillTick > 4)
                    {
                        if (player.SkillIndex == 1 || player.SkillIndex == 2)
                        {
                            PlaySoundPatchEffect("skill_cast.mp3", "skill_slot_" + player.SkillIndex, 260, GameSoundPatchFxVolume);
                        }
                        // 방어키는 키를 눌렀을 때가 아니라 몬스터 공격 방어에 성공했을 때만 defense_success.mp3를 재생합니다.

                        gameSoundPatchLastSkillIndex = player.SkillIndex;
                        gameSoundPatchLastSkillTick = tick;
                    }
                }
            }

            if (gameSoundPatchLastCoins >= 0 && player.Coins < gameSoundPatchLastCoins)
            {
                bool boughtItem = player.HpPotions > gameSoundPatchLastHpPotions || player.MpPotions > gameSoundPatchLastMpPotions;
                if (boughtItem)
                    PlaySoundPatchEffect("shop_purchase.mp3", "shop_purchase", 250, GameSoundPatchFxVolume);
            }

            gameSoundPatchLastHp = player.Hp;
            gameSoundPatchLastShield = playerShield;
            gameSoundPatchLastDefenseTicks = player.DefenseTicks;
            gameSoundPatchLastCoins = player.Coins;
            gameSoundPatchLastHpPotions = player.HpPotions;
            gameSoundPatchLastMpPotions = player.MpPotions;
            gameSoundPatchLastSkySwordCount = playerSkySwords.Count;
        }

        private string GetDesiredSoundPatchBgmKey()
        {
            if (screen == ScreenMode.Stage && currentStage > 0)
            {
                if (stageBossPhase)
                    return "boss";

                return "mob";
            }

            if (screen == ScreenMode.Cutscene)
                return "";

            return "base";
        }

        private bool HasAliveSoundPatchBoss()
        {
            try
            {
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (enemies[i] != null && enemies[i].IsBoss && enemies[i].Hp > 0)
                        return true;
                }
            }
            catch { }

            return false;
        }

        private bool IsSoundPatchNpcTalking()
        {
            try
            {
                if (screen == ScreenMode.AssistantIntro) return true;
                if (screen == ScreenMode.StageClearDialog) return true;
                if (screen == ScreenMode.Ending) return true;
                if (screen == ScreenMode.Stage && IsStageNpcHintOpen()) return true;
            }
            catch { }

            return false;
        }

        private void PlaySoundPatchEffect(string fileName, string cooldownKey, int cooldownMs, int volume)
        {
            DateTime now = DateTime.UtcNow;
            DateTime last;
            if (gameSoundPatchCooldowns.TryGetValue(cooldownKey, out last))
            {
                if ((now - last).TotalMilliseconds < cooldownMs)
                    return;
            }
            gameSoundPatchCooldowns[cooldownKey] = now;

            string path = ResolveSoundPatchPath(fileName);
            if (string.IsNullOrEmpty(path))
                return;

            string alias = "sp_fx_" + (++gameSoundPatchFxCounter).ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (OpenSoundPatchAlias(alias, path))
            {
                SetSoundPatchVolume(alias, volume);
                SendSoundPatchMci("play " + alias + " from 0");
                gameSoundPatchFxAliases.Add(new GameSoundPatchFxAlias(alias, now.AddSeconds(7)));
            }
        }

        private string StartSoundPatchLoop(string alias, string fileName, int volume)
        {
            string path = ResolveSoundPatchPath(fileName);
            if (string.IsNullOrEmpty(path))
                return "";

            SendSoundPatchMci("stop " + alias);
            SendSoundPatchMci("close " + alias);

            if (!OpenSoundPatchAlias(alias, path))
                return "";

            SetSoundPatchVolume(alias, volume);
            SendSoundPatchMci("play " + alias + " repeat");
            return alias;
        }

        private void StopSoundPatchLoop(ref string alias)
        {
            if (string.IsNullOrEmpty(alias))
                return;

            try
            {
                SendSoundPatchMci("stop " + alias);
                SendSoundPatchMci("close " + alias);
            }
            catch { }
            alias = "";
        }

        private bool OpenSoundPatchAlias(string alias, string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return false;

            string safePath = path.Replace("\"", "");
            int result = SendSoundPatchMci("open \"" + safePath + "\" type mpegvideo alias " + alias);
            return result == 0;
        }

        private void SetSoundPatchVolume(string alias, int volume)
        {
            if (volume < 0) volume = 0;
            if (volume > 1000) volume = 1000;
            SendSoundPatchMci("setaudio " + alias + " volume to " + volume.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        private int SendSoundPatchMci(string command)
        {
            try
            {
                return GameSoundPatchMciSendString(command, null, 0, IntPtr.Zero);
            }
            catch
            {
                return -1;
            }
        }

        private void CleanupFinishedSoundPatchEffects()
        {
            DateTime now = DateTime.UtcNow;
            for (int i = gameSoundPatchFxAliases.Count - 1; i >= 0; i--)
            {
                if (now >= gameSoundPatchFxAliases[i].CloseAtUtc)
                {
                    SendSoundPatchMci("stop " + gameSoundPatchFxAliases[i].Alias);
                    SendSoundPatchMci("close " + gameSoundPatchFxAliases[i].Alias);
                    gameSoundPatchFxAliases.RemoveAt(i);
                }
            }
        }

        private void GameSoundPatch_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (timer != null)
                    timer.Tick -= GameSoundPatch_Tick;

                StopSoundPatchLoop(ref gameSoundPatchCurrentBgmAlias);
                StopSoundPatchLoop(ref gameSoundPatchBossLoopAlias);
                StopSoundPatchLoop(ref gameSoundPatchNpcLoopAlias);

                for (int i = gameSoundPatchFxAliases.Count - 1; i >= 0; i--)
                {
                    SendSoundPatchMci("stop " + gameSoundPatchFxAliases[i].Alias);
                    SendSoundPatchMci("close " + gameSoundPatchFxAliases[i].Alias);
                }
                gameSoundPatchFxAliases.Clear();
            }
            catch { }
        }

        private string ResolveSoundPatchPath(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "";

            string cached;
            if (gameSoundPatchPathCache.TryGetValue(fileName, out cached) && File.Exists(cached))
                return cached;

            List<string> candidates = new List<string>();
            AddSoundPatchCandidates(candidates, AppDomain.CurrentDomain.BaseDirectory, fileName);
            AddSoundPatchCandidates(candidates, Directory.GetCurrentDirectory(), fileName);

            for (int i = 0; i < candidates.Count; i++)
            {
                if (File.Exists(candidates[i]))
                {
                    gameSoundPatchPathCache[fileName] = candidates[i];
                    return candidates[i];
                }
            }

            Debug.WriteLine("[GameSoundAutoPatch] sound file missing: " + fileName);
            return "";
        }

        private void AddSoundPatchCandidates(List<string> candidates, string startDir, string fileName)
        {
            if (string.IsNullOrEmpty(startDir))
                return;

            try
            {
                DirectoryInfo dir = new DirectoryInfo(startDir);
                for (int depth = 0; dir != null && depth < 9; depth++, dir = dir.Parent)
                {
                    candidates.Add(Path.Combine(dir.FullName, "Assets", GameSoundPatchFolderName, fileName));
                }
            }
            catch { }
        }

        private void EnsureSoundPatchOutputFolder()
        {
            // 빌드 출력 폴더에 MP3 복사가 누락된 경우에도, 프로젝트 루트의 Assets\SoundPatch를 찾아 출력 폴더로 복사합니다.
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string outputSoundDir = Path.Combine(baseDir, "Assets", GameSoundPatchFolderName);
                Directory.CreateDirectory(outputSoundDir);

                string[] files = new string[]
                {
                    "bgm_base_map.mp3",
                    "bgm_mob_dungeon.mp3",
                    "bgm_final_boss.mp3",
                    "boss_monster_loop.mp3",
                    "player_attack.mp3",
                    "skill_cast.mp3",
                    "shield_cast.mp3",
                    "defense_success.mp3",
                    "damage_light.mp3",
                    "damage_heavy.mp3",
                    "npc_dialogue_loop.mp3",
                    "shop_purchase.mp3",
                    "ui_click.mp3"
                };

                for (int i = 0; i < files.Length; i++)
                {
                    string target = Path.Combine(outputSoundDir, files[i]);
                    if (File.Exists(target))
                        continue;

                    string source = ResolveSoundPatchPath(files[i]);
                    if (string.IsNullOrEmpty(source) || string.Equals(Path.GetFullPath(source), Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase))
                        continue;

                    File.Copy(source, target, true);
                    gameSoundPatchPathCache[files[i]] = target;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[GameSoundAutoPatch] output copy skipped: " + ex.Message);
            }
        }

        private sealed class GameSoundPatchFxAlias
        {
            public readonly string Alias;
            public readonly DateTime CloseAtUtc;

            public GameSoundPatchFxAlias(string alias, DateTime closeAtUtc)
            {
                Alias = alias;
                CloseAtUtc = closeAtUtc;
            }
        }
    }
}
