using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace DebugHeroFileDungeonRPG
{
    public sealed class MainForm : Form
    {
        private const int TaskbarHeight = 38;
        private readonly Timer timer = new Timer();
        private readonly Random random = new Random();
        private readonly List<UiButton> buttons = new List<UiButton>();
        private readonly List<Effect> effects = new List<Effect>();
        private readonly List<DungeonInfo> dungeons = GameData.CreateDungeons();
        private readonly List<Monster> monsters = new List<Monster>();
        private readonly List<DroppedItem> drops = new List<DroppedItem>();
        private readonly string saveFile;

        private Player player = new Player();
        private GameScreen screen = GameScreen.CinematicIntro;
        private DungeonInfo currentDungeon;
        private int selectedFile = 0;
        private int selectedPart = 0;
        private float cameraX = 0;
        private float cameraY = 0;
        private int tick = 0;
        private int introTick = 0;
        private string pendingResultAction = "desktop";
        private bool trueDungeonUnlocked = false;
        private int truthScore = 0;
        private int clearedDungeonCount = 0;
        private int qCd = 0;
        private int wCd = 0;
        private int eCd = 0;
        private int rCd = 0;
        private bool moving = false;
        private bool rightMouseDown = false;
        private DroppedItem draggingItem;
        private DroppedItem pendingItem;
        private string message = "";
        private string resultText = "";
        private Rectangle lastHeroScreenRect;
        private string playerNameBuffer = "";
        private const int NameUnlockScore = 5;
        private int npcRelationScore = 0;
        private int npcTalkIndex = 0;
        private bool heroNameUnlocked = false;
        private string adminNpcLine = "NPC 404호: 로그가 충분히 쌓이면 시스템이 당신을 다시 부를 겁니다.";
        private readonly string[] adminNpcDialog = new string[]
        {
            "NPC 404호: 로그를 분석하는 중입니다.",
            "NPC 404호: 파일 던전의 반응을 더 확인해야 합니다.",
            "NPC 404호: 아직 결론을 내리기엔 이릅니다."
        };
        private readonly List<string> quarantinedBosses = new List<string>();
        private string relationNpcName = "NPC 404호";
        private string relationQuestion = "";
        private string relationContext = "";
        private string relationNextAction = "desktop";
        private string nameUnlockReturnAction = "desktop";
        private bool relationChoiceReady = false;
        private readonly string[] relationAnswers = new string[3];
        private readonly int[] relationDeltas = new int[3];
        private readonly string[] relationReactions = new string[3];
        private int lastRelationDelta = 0;
        private BossPatternManager bossManager = new BossPatternManager();


        public MainForm()
        {
            Text = "디버그 용사: Windows File Dungeon DX - Story Arena";
            ClientSize = new Size(1366, 768);
            MinimumSize = new Size(1120, 680);
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;
            KeyPreview = true;
            BackColor = Color.Black;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            saveFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DebugHeroFileDungeonRPG_StoryArena_save.txt");
            player.ApplyCustomization(0, 0, 0, 0);
            timer.Interval = 16;
            timer.Tick += Timer_Tick;
            timer.Start();
            KeyDown += MainForm_KeyDown;
            KeyPress += MainForm_KeyPress;
            MouseDown += MainForm_MouseDown;
            MouseMove += MainForm_MouseMove;
            MouseUp += MainForm_MouseUp;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            tick++;
            if (screen == GameScreen.CinematicIntro)
            {
                introTick++;
                if (introTick > 1320) screen = GameScreen.Title;
            }
            if (qCd > 0) qCd--;
            if (wCd > 0) wCd--;
            if (eCd > 0) eCd--;
            if (rCd > 0) rCd--;
            if (player.ShieldTicks > 0) player.ShieldTicks--;
            if (player.InvincibleTicks > 0) player.InvincibleTicks--;
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                effects[i].Ticks--;
                if (effects[i].Ticks <= 0) effects.RemoveAt(i);
            }
            if (screen == GameScreen.Dungeon)
            {
                UpdatePlayerMove();
                UpdateMonsters();
                UpdateCamera();
                CheckDungeonClear();
            }
            Invalidate();
        }

        private void UpdatePlayerMove()
        {
            if (player.Hp <= 0 || screen == GameScreen.Result)
            {
                moving = false;
                return;
            }

            float dx = player.TargetX - player.X;
            float dy = player.TargetY - player.Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            float speed = 4.2f + player.Speed * 0.22f;
            moving = false;
            if (dist > 3f)
            {
                player.X += dx / dist * Math.Min(speed, dist);
                player.Y += dy / dist * Math.Min(speed, dist);
                player.Facing = dx >= 0 ? 1 : -1;
                moving = true;
            }
            KeepPlayerInLane();
        }

        private void KeepPlayerInLane()
        {
            if (currentDungeon == null) return;
            player.X = Math.Max(60, Math.Min(currentDungeon.MapWidth - 80, player.X));
            player.Y = Math.Max(205, Math.Min(470, player.Y));
        }

        private void UpdateMonsters()
        {
            // 플레이어 사망 체크
            if (player.Hp <= 0)
            {
                player.Hp = 0; // UI 표시용 0 고정
                screen = GameScreen.Result; // 결과 화면으로 전환
                resultText = "STAGE FAILED\n\n플레이어 프로세스가 강제 종료되었습니다.\n데이터 손상을 방지하기 위해 바탕화면으로 이동합니다.";
                pendingResultAction = "desktop";
                return; // 즉시 메서드 종료 (아래 몬스터 로직 실행 안 함)
            }

            for (int i = 0; i < monsters.Count; i++)
            {
                Monster m = monsters[i];
                if (m.Hp <= 0) continue;
                if (m.Boss && currentDungeon != null && currentDungeon.Boss)
                {
                    // 보스 매니저를 통해 기믹 및 일반 공격 실행
                    bossManager.Update(m, player, effects, cameraX, cameraY, currentDungeon.MapWidth);
                }
                if (m.HitFlash > 0) m.HitFlash--;
                if (m.AttackCooldown > 0) m.AttackCooldown--;
                float dx = player.X - m.X;
                float dy = player.Y - m.Y;
                float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                if (dist < 540)
                {
                    float speed = m.Boss ? 1.25f : 1.65f;
                    if (dist > 72)
                    {
                        m.X += dx / Math.Max(1, dist) * speed;
                        m.Y += dy / Math.Max(1, dist) * speed * 0.65f;
                    }
                    else if (m.AttackCooldown <= 0 && player.InvincibleTicks <= 0)
                    {
                        int dmg = Math.Max(2, m.Attack - player.Defense / 2);
                        if (player.ShieldTicks > 0) dmg = Math.Max(1, dmg / 2);
                        player.Hp -= dmg;
                        player.InvincibleTicks = 28;
                        m.AttackCooldown = m.Boss ? 45 : 60;
                        AddEffect("text", player.X, player.Y - 70, player.X, player.Y - 70, 32, Color.OrangeRed, "-" + dmg);
                        AddEffect("burst", player.X, player.Y - 30, player.X, player.Y - 30, 18, Color.Red, "");
                        if (player.Hp <= 0)
                        {
                            player.Hp = Math.Max(1, player.MaxHp / 2);
                            screen = GameScreen.Desktop;
                            message = "프로세스가 강제 복구되었습니다. 바탕화면으로 돌아갑니다.";
                        }
                    }
                }
                if (m.X < 80) m.X = 80;
                if (currentDungeon != null && m.X > currentDungeon.MapWidth - 120) m.X = currentDungeon.MapWidth - 120;
                m.Y = Math.Max(205, Math.Min(470, m.Y));
            }
        }

        private void UpdateCamera()
        {
            if (currentDungeon == null) return;
            cameraX = player.X - ClientSize.Width * 0.36f;
            cameraY = player.Y - ClientSize.Height * 0.52f;
            if (cameraX < 0) cameraX = 0;
            if (cameraX > currentDungeon.MapWidth - ClientSize.Width + 120) cameraX = Math.Max(0, currentDungeon.MapWidth - ClientSize.Width + 120);
            if (cameraY < 0) cameraY = 0;
            if (cameraY > 180) cameraY = 180;
        }

        private void CheckDungeonClear()
        {
            bool allDead = true;
            for (int i = 0; i < monsters.Count; i++) if (monsters[i].Hp > 0) allDead = false;
            if (allDead)
            {
                FinishDungeon();
            }
        }

        private void FinishDungeon()
        {
            if (currentDungeon == null) return;
            int stageNo = Math.Max(1, selectedFile + 1);
            int gold = 150 + currentDungeon.RecommendedLevel * 42 + (currentDungeon.Boss ? 220 : 0);
            int patch = currentDungeon.Boss ? 32 + currentDungeon.RecommendedLevel * 2 : 16 + currentDungeon.RecommendedLevel;
            player.Gold += gold;
            player.PatchShards += patch;
            bool lv = player.AddExp(120 + currentDungeon.RecommendedLevel * 80 + (currentDungeon.Boss ? 180 : 0));
            if (selectedFile == clearedDungeonCount && clearedDungeonCount < dungeons.Count) clearedDungeonCount++;
            if (!currentDungeon.Boss) truthScore++;

            string quarantine = "";
            if (currentDungeon.Boss)
            {
                string bossName = GameData.GetDungeonNpc(currentDungeon);
                if (!quarantinedBosses.Contains(bossName)) quarantinedBosses.Add(bossName);
                quarantine = "\n\n격리 처리: NPC 404호가 " + bossName + "을(를) Recycle Bin 격리 구역으로 강제 이동했습니다." +
                             "\n현재 격리된 보스 수: " + quarantinedBosses.Count;
            }

            resultText = "Stage " + stageNo.ToString("00") + " 클리어!\n" + currentDungeon.Name + "\n\n" +
                         GameData.GetDungeonClearLog(currentDungeon, truthScore) + "\n\n" +
                         "NPC: " + GameData.GetDungeonNpc(currentDungeon) + "\n" +
                         "획득 단서: " + GameData.GetHintText(currentDungeon, truthScore) + "\n" +
                         quarantine + "\n\n" +
                         "Gold +" + gold + "   패치 조각 +" + patch + "   EXP 획득";
            if (lv)
            {
                player.WeaponLevel++;
                player.Attack += 3;
                resultText += "\n레벨업! 무기 자동 강화 성공: " + player.WeaponName;
            }

            relationNextAction = "desktop";
            if (selectedFile == dungeons.Count - 1) relationNextAction = "suspect";
            SetupRelationChoice(currentDungeon);
            pendingResultAction = "relation";
            screen = GameScreen.Result;
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (screen == GameScreen.CinematicIntro)
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space || e.KeyCode == Keys.E || e.KeyCode == Keys.Escape) screen = GameScreen.Title;
                return;
            }
            if (screen == GameScreen.Title)
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space) BeginAdminIntro();
                if (e.KeyCode == Keys.L) LoadGame();
                if (e.KeyCode == Keys.H) screen = GameScreen.Help;
                return;
            }
            if (screen == GameScreen.AdminIntro)
            {
                if (!heroNameUnlocked)
                {
                    if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space || e.KeyCode == Keys.E) StartAdminRun();
                    if (e.KeyCode == Keys.Escape) screen = GameScreen.Title;
                    return;
                }
                if (e.KeyCode == Keys.Back && playerNameBuffer.Length > 0) playerNameBuffer = playerNameBuffer.Substring(0, playerNameBuffer.Length - 1);
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space) ConfirmAdminName();
                if (e.KeyCode == Keys.Escape)
                {
                    if (clearedDungeonCount > 0) screen = GameScreen.Desktop;
                    else screen = GameScreen.Title;
                }
                return;
            }

            if (screen == GameScreen.Customize)
            {
                if (e.KeyCode == Keys.Up) selectedPart = (selectedPart + 3) % 4;
                if (e.KeyCode == Keys.Down) selectedPart = (selectedPart + 1) % 4;
                if (e.KeyCode == Keys.Left) AdjustCustomization(-1);
                if (e.KeyCode == Keys.Right) AdjustCustomization(1);
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space) StartGameFromCustomization();
                if (e.KeyCode == Keys.Escape) screen = GameScreen.Title;
                return;
            }
            if (screen == GameScreen.Desktop)
            {
                if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Up) selectedFile = (selectedFile + dungeons.Count - 1) % dungeons.Count;
                if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Down) selectedFile = (selectedFile + 1) % dungeons.Count;
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.E) EnterDungeon(selectedFile);
                if (e.KeyCode == Keys.S) SaveGame();
                if (e.KeyCode == Keys.H) screen = GameScreen.Help;
                if (e.KeyCode == Keys.Escape) screen = GameScreen.Title;
                return;
            }
            if (screen == GameScreen.Dungeon)
            {
                if (e.KeyCode == Keys.Space) Leap();
                if (e.KeyCode == Keys.Q) CastQ();
                if (e.KeyCode == Keys.W) CastW();
                if (e.KeyCode == Keys.E) CastE();
                if (e.KeyCode == Keys.R) CastR();
                if (e.KeyCode == Keys.D) UsePotion();
                if (e.KeyCode == Keys.F) UseMpPotion();
                if (e.KeyCode == Keys.Escape) screen = GameScreen.Desktop;
                return;
            }
            if (screen == GameScreen.EquipPrompt)
            {
                if (e.KeyCode == Keys.Y) EquipPendingItem();
                if (e.KeyCode == Keys.N || e.KeyCode == Keys.Escape) StorePendingItem();
                return;
            }
            if (screen == GameScreen.Result)
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space || e.KeyCode == Keys.Escape)
                {
                    ContinueAfterResult();
                }
                return;
            }
            if (screen == GameScreen.RelationChoice)
            {
                if (e.KeyCode == Keys.D1 || e.KeyCode == Keys.NumPad1) ResolveRelationChoice(0);
                if (e.KeyCode == Keys.D2 || e.KeyCode == Keys.NumPad2) ResolveRelationChoice(1);
                if (e.KeyCode == Keys.D3 || e.KeyCode == Keys.NumPad3) ResolveRelationChoice(2);
                if (e.KeyCode == Keys.Escape) ResolveRelationChoice(1);
                return;
            }

            if (screen == GameScreen.SuspectSelect)
            {
                if (e.KeyCode == Keys.D1 || e.KeyCode == Keys.NumPad1) ChooseSuspect(0);
                if (e.KeyCode == Keys.D2 || e.KeyCode == Keys.NumPad2) ChooseSuspect(1);
                if (e.KeyCode == Keys.D3 || e.KeyCode == Keys.NumPad3) ChooseSuspect(2);
                if (e.KeyCode == Keys.D4 || e.KeyCode == Keys.NumPad4) ChooseSuspect(3);
                if (e.KeyCode == Keys.D5 || e.KeyCode == Keys.NumPad5) ChooseSuspect(4);
                if (e.KeyCode == Keys.D6 || e.KeyCode == Keys.NumPad6) ChooseSuspect(5);
                if (e.KeyCode == Keys.Escape) screen = GameScreen.Desktop;
                return;
            }
            if (screen == GameScreen.Ending)
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape) screen = GameScreen.Title;
                return;
            }
            if (screen == GameScreen.Help)
            {
                if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Enter) screen = GameScreen.Title;
            }
        }


        private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (screen != GameScreen.AdminIntro || !heroNameUnlocked) return;
            char c = e.KeyChar;
            if (!char.IsControl(c) && playerNameBuffer.Length < 12)
            {
                if (char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == ' ')
                    playerNameBuffer += c;
            }
        }



private void BeginAdminIntro()
{
    player = new Player();
    player.ApplyCustomization(0, 0, 0, 0);
    player.Name = "UNKNOWN_PROCESS";
    playerNameBuffer = "";
    selectedPart = 0;
    selectedFile = 0;
    npcRelationScore = 0;
    npcTalkIndex = 0;
    heroNameUnlocked = false;
    quarantinedBosses.Clear();
    truthScore = 0;
    clearedDungeonCount = 0;
    trueDungeonUnlocked = false;
    relationChoiceReady = false;
    relationNextAction = "desktop";
    nameUnlockReturnAction = "customize";
    message = "새 프로세스가 생성되었습니다. 장비를 선택한 뒤 파일 던전에 진입하세요.";
    adminNpcLine = "NPC 404호: 초기 식별 정보는 아직 불안정합니다. 로그가 충분히 쌓이면 시스템이 당신을 다시 부를 겁니다.";
    screen = GameScreen.Customize;
}



private void TalkWithNpc404()
{
    if (!heroNameUnlocked)
    {
        adminNpcLine = "NPC 404호: 지금은 처리할 로그가 없습니다. 던전에서 돌아온 뒤 다시 대화합시다.";
        message = "NPC 404호가 아직 당신을 제대로 인식하지 못했습니다.";
        TryBeep(520, 55);
        return;
    }
    adminNpcLine = "NPC 404호: 이제 임시 프로세스명 대신 직접 이름을 등록할 수 있습니다.";
}



private void ConfirmAdminName()
{
    if (!heroNameUnlocked)
    {
        adminNpcLine = "NPC 404호: 아직 이 명령은 열리지 않았습니다.";
        message = "현재 사용할 수 없는 명령입니다.";
        TryBeep(320, 70);
        return;
    }
    string n = playerNameBuffer.Trim();
    if (n.Length < 2)
    {
        adminNpcLine = "NPC 404호: 이름이 너무 짧습니다. 최소 두 글자는 있어야 로그에 남습니다.";
        message = "이름 입력 실패: 최소 2글자 이상 입력하세요.";
        TryBeep(320, 70);
        return;
    }
    if (n.ToLower() == "admin")
    {
        adminNpcLine = "NPC 404호: 그 이름은 임시 계정명으로 이미 사용 중입니다. 다른 이름을 입력하세요.";
        message = "admin은 예약된 임시 이름입니다. 다른 이름을 입력하세요.";
        TryBeep(260, 90);
        return;
    }
    player.Name = n;
    message = "NPC 404호: 새 이름 등록 완료 - " + player.Name + ".";
    TryBeep(960, 90);
    if (nameUnlockReturnAction == "suspect") screen = GameScreen.SuspectSelect;
    else if (nameUnlockReturnAction == "ending") screen = GameScreen.Ending;
    else screen = GameScreen.Desktop;
}



private void StartAdminRun()
{
    // 이전 버전 호환용. 새 게임은 이름 입력 없이 바로 커스터마이징으로 진입합니다.
    screen = GameScreen.Customize;
}

        private void TryBeep(int frequency, int duration)
        {
            try { Console.Beep(frequency, duration); } catch { }
        }

        private void AdjustCustomization(int delta)
        {
            if (selectedPart == 0) player.Outfit = (player.Outfit + GameData.OutfitNames.Length + delta) % GameData.OutfitNames.Length;
            else if (selectedPart == 1) player.Weapon = (player.Weapon + GameData.WeaponNames.Length + delta) % GameData.WeaponNames.Length;
            else if (selectedPart == 2) player.Armor = (player.Armor + GameData.ArmorNames.Length + delta) % GameData.ArmorNames.Length;
            else player.Cape = (player.Cape + GameData.CapeNames.Length + delta) % GameData.CapeNames.Length;
            player.ApplyCustomization(player.Outfit, player.Weapon, player.Armor, player.Cape);
        }

        private void StartGameFromCustomization()
        {
            player.ApplyCustomization(player.Outfit, player.Weapon, player.Armor, player.Cape);
            screen = GameScreen.Desktop;
            message = "장비 설정 완료. 10개의 파일 던전 로그를 순서대로 추적하세요.";
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (screen == GameScreen.CinematicIntro) { screen = GameScreen.Title; return; }
            if (screen == GameScreen.RelationChoice && e.Button == MouseButtons.Left)
            {
                for (int i = buttons.Count - 1; i >= 0; i--)
                {
                    if (buttons[i].Action.StartsWith("rel") && buttons[i].Bounds.Contains(e.Location))
                    {
                        HandleButton(buttons[i].Action);
                        return;
                    }
                }
                return;
            }
            if (e.Button == MouseButtons.Left)
            {
                for (int i = buttons.Count - 1; i >= 0; i--)
                {
                    if (buttons[i].Bounds.Contains(e.Location))
                    {
                        HandleButton(buttons[i].Action);
                        return;
                    }
                }
            }
            if (screen == GameScreen.Dungeon)
            {
                if (e.Button == MouseButtons.Right)
                {
                    rightMouseDown = true;
                    SetMoveTarget(e.Location, true);
                }
                if (e.Button == MouseButtons.Left)
                {
                    bossManager.HandleClick(e.Location);
                    DroppedItem item = FindDropAt(e.Location);
                    if (item != null)
                    {
                        draggingItem = item;
                        item.Dragging = true;
                        item.DragPoint = e.Location;
                    }
                }
            }
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (screen == GameScreen.Dungeon)
            {
                if (draggingItem != null)
                {
                    draggingItem.DragPoint = e.Location;
                }
                if ((e.Button & MouseButtons.Right) == MouseButtons.Right)
                {
                    SetMoveTarget(e.Location, false);
                }
            }
        }

        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) rightMouseDown = false;
            if (screen == GameScreen.Dungeon && e.Button == MouseButtons.Left && draggingItem != null)
            {
                DroppedItem item = draggingItem;
                draggingItem = null;
                item.Dragging = false;
                if (lastHeroScreenRect.Contains(e.Location))
                {
                    pendingItem = item;
                    screen = GameScreen.EquipPrompt;
                }
            }
        }

        private void HandleButton(string action)
        {
            if (action == "skipIntro") screen = GameScreen.Title;
            else if (action == "new") BeginAdminIntro();
            else if (action == "help") screen = GameScreen.Help;
            else if (action == "load") LoadGame();
            else if (action == "title") screen = GameScreen.Title;
            else if (action == "desktop") screen = GameScreen.Desktop;
            else if (action == "startGame") StartGameFromCustomization();
            else if (action == "talk404") TalkWithNpc404();
            else if (action == "startAdminRun") StartAdminRun();
            else if (action == "confirmName") ConfirmAdminName();
            else if (action == "equip") EquipPendingItem();
            else if (action == "store") StorePendingItem();
            else if (action == "resultContinue") ContinueAfterResult();
            else if (action == "endingTitle") screen = GameScreen.Title;
            else if (action.StartsWith("rel")) { int ri; if (int.TryParse(action.Substring(3), out ri)) ResolveRelationChoice(ri); }
            else if (action.StartsWith("suspect")) { int si; if (int.TryParse(action.Substring(7), out si)) ChooseSuspect(si); }
            else if (action.StartsWith("file"))
            {
                int idx;
                if (int.TryParse(action.Substring(4), out idx)) EnterDungeon(idx);
            }
            else if (action.StartsWith("outfit")) { player.Outfit = int.Parse(action.Substring(6)); player.ApplyCustomization(player.Outfit, player.Weapon, player.Armor, player.Cape); }
            else if (action.StartsWith("weapon")) { player.Weapon = int.Parse(action.Substring(6)); player.ApplyCustomization(player.Outfit, player.Weapon, player.Armor, player.Cape); }
            else if (action.StartsWith("armor")) { player.Armor = int.Parse(action.Substring(5)); player.ApplyCustomization(player.Outfit, player.Weapon, player.Armor, player.Cape); }
            else if (action.StartsWith("cape")) { player.Cape = int.Parse(action.Substring(4)); player.ApplyCustomization(player.Outfit, player.Weapon, player.Armor, player.Cape); }
        }


        private string GetStageLockMessage(int idx)
        {
            if (idx <= 0) return "잠긴 파일입니다.";
            int requiredStage = idx;
            string gate = "이전 스테이지";
            if (idx >= 2 && idx <= 3) gate = "Boss 1 Driver-K";
            else if (idx >= 4 && idx <= 5) gate = "Boss 2 High-Kernel";
            else if (idx >= 6 && idx <= 7) gate = "Boss 3 BSOD";
            else if (idx >= 8 && idx <= 9) gate = "Boss 4 Exception Queen";
            return "잠금: " + gate + " 격리 기록이 필요합니다. 현재 진행 가능 단계: " + (clearedDungeonCount + 1);
        }

        private void EnterDungeon(int idx)
        {
            if (idx < 0 || idx >= dungeons.Count) return;
            DungeonInfo d = dungeons[idx];
            if (idx > clearedDungeonCount)
            {
                message = GetStageLockMessage(idx);
                AddEffect("text", player.X, player.Y - 70, player.X, player.Y - 70, 40, Color.OrangeRed, "STAGE LOCK");
                return;
            }
            if (d.FileName == "UserCore_TrueFault.exe" && !trueDungeonUnlocked && player.PatchShards < 999)
            {
                message = "최종 로그가 아직 암호화되어 있습니다. Blue Screen Tower 이후 용의자 추리를 진행하세요.";
                AddEffect("text", player.X, player.Y - 70, player.X, player.Y - 70, 40, Color.Magenta, "LOCKED");
                return;
            }
            if (player.PatchShards < d.RequiredPatch)
            {
                message = "패치 조각이 부족합니다. 필요한 패치: " + d.RequiredPatch;
                AddEffect("text", player.X, player.Y - 70, player.X, player.Y - 70, 40, Color.OrangeRed, "LOCKED");
                return;
            }
            selectedFile = idx;
            currentDungeon = d;
            // [수정 사항 1] 스테이지 진입 시 HP와 MP를 최대치로 회복
            player.Hp = player.MaxHp;
            player.Mp = player.MaxMp;

            monsters.Clear();
            drops.Clear();
            effects.Clear();
            monsters.AddRange(GameData.CreateMonsters(d));
            player.X = 160;
            player.Y = 330;
            player.TargetX = player.X;
            player.TargetY = player.Y;
            cameraX = 0;
            cameraY = 0;
            qCd = wCd = eCd = rCd = 0;
            screen = GameScreen.Dungeon;
            message = "우클릭으로 이동하고, Q/W/E/R로 스킬을 사용하세요. 드롭 파일은 용사에게 드래그합니다.";
        }

        private void SetMoveTarget(Point point, bool show)
        {
            PointF world = Renderer.ScreenToWorld(point, cameraX, cameraY);
            player.TargetX = Math.Max(40, Math.Min(currentDungeon == null ? 9999 : currentDungeon.MapWidth - 80, world.X));
            player.TargetY = Math.Max(205, Math.Min(470, world.Y));
            if (show) AddEffect("target", player.TargetX, player.TargetY, player.TargetX, player.TargetY, 32, Color.Cyan, "");
        }

        private void Leap()
        {
            float dx = player.TargetX - player.X;
            float dy = player.TargetY - player.Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            if (dist < 5)
            {
                dx = player.Facing;
                dy = 0;
                dist = 1;
            }
            player.X += dx / dist * 115;
            player.Y += dy / dist * 70;
            KeepPlayerInLane();
            AddEffect("burst", player.X, player.Y, player.X, player.Y, 26, player.OutfitColor, "SPACE");
        }

        private void CastQ()
        {
            if (qCd > 0) return;
            qCd = 18;
            PointF dir = AimVector();
            float sx = player.X + dir.X * 35;
            float sy = player.Y - 22 + dir.Y * 20;
            float tx = player.X + dir.X * 270;
            float ty = player.Y - 22 + dir.Y * 110;
            AddEffect("projectile", sx, sy, tx, ty, 26, player.WeaponColor, "Q");
            AddEffect("slash", tx, ty, tx, ty, 24, Color.White, "");
            HitMonstersLine(player.X, player.Y, tx, ty, 72, player.Attack + player.Level * 3 + player.WeaponLevel * 2);
        }

        private void CastW()
        {
            if (wCd > 0 || player.Mp < 14) return;
            player.Mp -= 14;
            wCd = 44;
            PointF dir = AimVector();
            float tx = player.X + dir.X * 360;
            float ty = player.Y + dir.Y * 170;
            AddEffect("projectile", player.X, player.Y - 20, tx, ty - 20, 34, Renderer.Lighten(player.WeaponColor, 40), "W");
            AddEffect("burst", tx, ty - 20, tx, ty - 20, 34, player.WeaponColor, "");
            HitMonstersLine(player.X, player.Y, tx, ty, 115, player.Attack * 2 + player.Level * 5 + player.WeaponLevel * 4);
        }

        private void CastE()
        {
            if (eCd > 0 || player.Mp < 20) return;
            player.Mp -= 20;
            eCd = 66;
            player.ShieldTicks = 180;
            int heal = Math.Min(player.MaxHp - player.Hp, 26 + player.Level * 4);
            player.Hp += heal;
            AddEffect("burst", player.X, player.Y - 20, player.X, player.Y - 20, 45, Color.LimeGreen, "E");
            AddEffect("text", player.X, player.Y - 88, player.X, player.Y - 88, 36, Color.LimeGreen, "HP +" + heal);
        }

        private void CastR()
        {
            if (rCd > 0 || player.Mp < 38) return;
            player.Mp -= 38;
            rCd = 140;
            PointF dir = AimVector();
            AddEffect("burst", player.X, player.Y - 25, player.X, player.Y - 25, 54, player.WeaponColor, "R");
            for (int i = 0; i < 6; i++)
            {
                float spread = (i - 2.5f) * 38f;
                float sx = player.X + dir.X * 35;
                float sy = player.Y - 30 + spread * .35f;
                float tx = player.X + dir.X * (360 + i * 80);
                float ty = player.Y - 30 + dir.Y * 100 + spread;
                AddEffect("projectile", sx, sy, tx, ty, 40 + i * 2, player.WeaponColor, "R");
                AddEffect("slash", tx, ty, tx, ty, 38, Color.White, "");
                HitMonstersLine(sx, sy, tx, ty, 100, player.Attack * 2 + player.Level * 8 + 42);
            }
        }

        private PointF AimVector()
        {
            float dx = player.TargetX - player.X;
            float dy = player.TargetY - player.Y;
            float d = (float)Math.Sqrt(dx * dx + dy * dy);
            if (d < 12)
            {
                return new PointF(player.Facing, 0);
            }
            player.Facing = dx >= 0 ? 1 : -1;
            return new PointF(dx / d, dy / d);
        }

        private void HitMonstersLine(float x1, float y1, float x2, float y2, float radius, int damage)
        {
            for (int i = 0; i < monsters.Count; i++)
            {
                Monster m = monsters[i];
                if (m.Hp <= 0) continue;
                float dist = DistancePointToSegment(m.X, m.Y, x1, y1, x2, y2);
                if (dist <= radius)
                {
                    int dmg = Math.Max(1, damage + random.Next(-5, 8));
                    m.Hp -= dmg;
                    m.HitFlash = 12;
                    AddEffect("text", m.X, m.Y - 80, m.X, m.Y - 80, 32, Color.Yellow, dmg.ToString());
                    AddEffect("burst", m.X, m.Y - 22, m.X, m.Y - 22, 18, Color.White, "");
                    if (m.Hp <= 0)
                    {
                        player.Gold += m.Gold;
                        player.AddExp(m.Exp);
                        TryDropItem(m);
                    }
                }
            }
        }

        private float DistancePointToSegment(float px, float py, float x1, float y1, float x2, float y2)
        {
            float vx = x2 - x1;
            float vy = y2 - y1;
            float wx = px - x1;
            float wy = py - y1;
            float c1 = vx * wx + vy * wy;
            if (c1 <= 0) return Dist(px, py, x1, y1);
            float c2 = vx * vx + vy * vy;
            if (c2 <= c1) return Dist(px, py, x2, y2);
            float b = c1 / c2;
            return Dist(px, py, x1 + b * vx, y1 + b * vy);
        }

        private float Dist(float x1, float y1, float x2, float y2)
        {
            float dx = x2 - x1;
            float dy = y2 - y1;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        private void TryDropItem(Monster m)
        {
            int roll = random.Next(100);
            DroppedItem item;
            if (m.Boss || roll < 15)
                item = new DroppedItem("LegendCore.bsod", "장착: 공격 +4, 방어 +3, 패치 +10", ItemKind.Weapon, Color.FromArgb(100, 180, 255), m.X, m.Y, 4, 3, 0, 0, 10);
            else if (roll < 38)
                item = new DroppedItem("DebugSword.exe", "장착: 공격 +3", ItemKind.Weapon, Color.FromArgb(90, 220, 255), m.X, m.Y, 3, 0, 0, 0, 0);
            else if (roll < 60)
                item = new DroppedItem("FirewallArmor.dll", "장착: 방어 +3, HP +12", ItemKind.Armor, Color.FromArgb(90, 140, 255), m.X, m.Y, 0, 3, 12, 0, 0);
            else if (roll < 82)
                item = new DroppedItem("MemoryBoost.sys", "보관: MP +8", ItemKind.Material, Color.FromArgb(120, 210, 255), m.X, m.Y, 0, 0, 0, 8, 0);
            else
                item = new DroppedItem("PatchShard.pkg", "보관: 패치 조각 +5", ItemKind.Material, Color.Gold, m.X, m.Y, 0, 0, 0, 0, 5);
            drops.Add(item);
            AddEffect("text", item.X, item.Y - 70, item.X, item.Y - 70, 36, item.Color, "DROP");
        }

        private DroppedItem FindDropAt(Point p)
        {
            for (int i = drops.Count - 1; i >= 0; i--)
            {
                if (drops[i].ScreenRect(cameraX, cameraY).Contains(p)) return drops[i];
            }
            return null;
        }

        private void EquipPendingItem()
        {
            if (pendingItem == null) return;
            ApplyItem(pendingItem, true);
            drops.Remove(pendingItem);
            pendingItem = null;
            screen = GameScreen.Dungeon;
        }

        private void StorePendingItem()
        {
            if (pendingItem == null) return;
            ApplyItem(pendingItem, false);
            drops.Remove(pendingItem);
            pendingItem = null;
            screen = GameScreen.Dungeon;
        }

        private void ApplyItem(DroppedItem item, bool equip)
        {
            if (equip && (item.Kind == ItemKind.Weapon || item.Kind == ItemKind.Armor || item.Kind == ItemKind.Outfit))
            {
                player.Attack += item.AttackBonus;
                player.Defense += item.DefenseBonus;
                player.MaxHp += item.HpBonus;
                player.MaxMp += item.MpBonus;
                player.PatchShards += item.PatchBonus;
                player.EquippedItems++;
                player.Hp = Math.Min(player.MaxHp, player.Hp + item.HpBonus);
                AddEffect("text", player.X, player.Y - 96, player.X, player.Y - 96, 40, Color.Cyan, "장착 완료");
            }
            else
            {
                player.MaxHp += item.HpBonus;
                player.MaxMp += item.MpBonus;
                player.Mp = Math.Min(player.MaxMp, player.Mp + item.MpBonus);
                player.PatchShards += item.PatchBonus;
                player.StoredItems++;
                AddEffect("text", player.X, player.Y - 96, player.X, player.Y - 96, 40, Color.LightGreen, "보관 완료");
            }
        }

        private void UsePotion()
        {
            if (player.Potion <= 0) return;
            int heal = Math.Min(player.MaxHp - player.Hp, 70);
            player.Hp += heal;
            player.Potion--;
            AddEffect("text", player.X, player.Y - 85, player.X, player.Y - 85, 34, Color.LimeGreen, "HP +" + heal);
        }

        private void UseMpPotion()
        {
            if (player.MpPotion <= 0) return;
            int heal = Math.Min(player.MaxMp - player.Mp, 45);
            player.Mp += heal;
            player.MpPotion--;
            AddEffect("text", player.X, player.Y - 85, player.X, player.Y - 85, 34, Color.DeepSkyBlue, "MP +" + heal);
        }

        private void AddEffect(string kind, float x, float y, float x2, float y2, int ticks, Color color, string text)
        {
            effects.Add(new Effect(kind, x, y, x2, y2, ticks, color, text));
        }

        private void SaveGame()
        {
            try
            {
                string data = string.Join("|", new string[] {
                    player.Name, player.Level.ToString(), player.Exp.ToString(), player.Gold.ToString(), player.PatchShards.ToString(),
                    player.Hp.ToString(), player.MaxHp.ToString(), player.Mp.ToString(), player.MaxMp.ToString(), player.Attack.ToString(), player.Defense.ToString(), player.Speed.ToString(),
                    player.Outfit.ToString(), player.Weapon.ToString(), player.Armor.ToString(), player.Cape.ToString(), player.WeaponLevel.ToString(), player.Potion.ToString(), player.MpPotion.ToString(),
                    clearedDungeonCount.ToString(), npcRelationScore.ToString(), heroNameUnlocked ? "1" : "0", trueDungeonUnlocked ? "1" : "0", truthScore.ToString()
                });
                File.WriteAllText(saveFile, data);
                message = "저장 완료: " + saveFile;
            }
            catch (Exception ex) { message = "저장 실패: " + ex.Message; }
        }

        private void LoadGame()
        {
            try
            {
                if (!File.Exists(saveFile)) { message = "저장 파일이 없습니다."; return; }
                string[] s = File.ReadAllText(saveFile).Split('|');
                if (s.Length < 19) { message = "저장 파일 형식이 맞지 않습니다."; return; }
                player.Name = s[0];
                player.Level = int.Parse(s[1]); player.Exp = int.Parse(s[2]); player.Gold = int.Parse(s[3]); player.PatchShards = int.Parse(s[4]);
                player.Hp = int.Parse(s[5]); player.MaxHp = int.Parse(s[6]); player.Mp = int.Parse(s[7]); player.MaxMp = int.Parse(s[8]);
                player.Attack = int.Parse(s[9]); player.Defense = int.Parse(s[10]); player.Speed = int.Parse(s[11]);
                player.Outfit = int.Parse(s[12]); player.Weapon = int.Parse(s[13]); player.Armor = int.Parse(s[14]); player.Cape = int.Parse(s[15]); player.WeaponLevel = int.Parse(s[16]);
                player.Potion = int.Parse(s[17]); player.MpPotion = int.Parse(s[18]);
                if (s.Length >= 24)
                {
                    clearedDungeonCount = int.Parse(s[19]);
                    npcRelationScore = int.Parse(s[20]);
                    heroNameUnlocked = s[21] == "1";
                    trueDungeonUnlocked = s[22] == "1";
                    truthScore = int.Parse(s[23]);
                }
                else
                {
                    if (player.PatchShards >= 999) trueDungeonUnlocked = true;
                    heroNameUnlocked = player.Name.ToLower() != "admin";
                    npcRelationScore = heroNameUnlocked ? NameUnlockScore : 0;
                }
                screen = GameScreen.Desktop;
                message = "불러오기 완료";
            }
            catch (Exception ex) { message = "불러오기 실패: " + ex.Message; }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            buttons.Clear();
            if (screen == GameScreen.CinematicIntro) DrawCinematicIntro(g);
            else if (screen == GameScreen.Title) DrawTitle(g);
            else if (screen == GameScreen.AdminIntro) DrawAdminIntro(g);
            else if (screen == GameScreen.Customize) Renderer.DrawCustomization(g, ClientRectangle, player, selectedPart, buttons);
            else if (screen == GameScreen.Desktop) Renderer.DrawDesktop(g, ClientRectangle, dungeons, selectedFile, player, buttons);
            else if (screen == GameScreen.Dungeon) DrawDungeon(g);
            else if (screen == GameScreen.EquipPrompt) { DrawDungeon(g); DrawEquipPrompt(g); }
            else if (screen == GameScreen.Result) DrawResult(g);
            else if (screen == GameScreen.RelationChoice) DrawRelationChoice(g);
            else if (screen == GameScreen.SuspectSelect) DrawSuspectSelect(g);
            else if (screen == GameScreen.Ending) DrawEnding(g);
            else if (screen == GameScreen.Help) DrawHelp(g);
        }



private void ContinueAfterResult()
{
    if (pendingResultAction == "relation" && relationChoiceReady)
    {
        screen = GameScreen.RelationChoice;
        return;
    }
    if (pendingResultAction == "suspect") screen = GameScreen.SuspectSelect;
    else if (pendingResultAction == "ending") screen = GameScreen.Ending;
    else screen = GameScreen.Desktop;
}


private void SetupRelationChoice(DungeonInfo dungeon)
{
    relationChoiceReady = true;
    lastRelationDelta = 0;
    relationNpcName = GameData.GetDungeonNpc(dungeon);
    string f = dungeon.FileName;
    relationContext = "던전 클리어 후 " + relationNpcName + "와 마주했습니다. 당신의 대답은 시스템 로그에 조용히 기록됩니다.";

    if (f.Contains("Explorer"))
    {
        relationQuestion = "Searchy.exe가 말합니다. '바로가기만 찾았다고 끝난 게 아니야. 원본 파일까지 찾아줄 거야?'";
        SetRelationAnswers("원본 경로를 끝까지 추적한다", 2, "NPC 404호: 데이터의 뿌리를 보는군요.",
                           "일단 보관하고 로그를 확인한다", 1, "NPC 404호: 최소한 휴지통행은 아니네요.",
                           "바로가기니까 삭제한다", -2, "NPC 404호: 와, 원본도 못 찾고 바로 삭제라니.");
    }
    else if (f.Contains("Driver"))
    {
        relationQuestion = "Driver-K가 항변합니다. '충돌은 내가 만든 게 아니라 누가 장치 로그를 꼬아놨어.' 어떻게 처리할까?";
        SetRelationAnswers("증거 로그를 보존하고 격리한다", 2, "NPC 404호: 억울한 드라이버에게도 절차는 필요하죠.",
                           "일단 격리하되 기록은 남긴다", 1, "NPC 404호: 빠르진 않아도 안전합니다.",
                           "시끄러우니 바로 삭제한다", -2, "NPC 404호: 정상 드라이버까지 날릴 기세네요.");
    }
    else if (f.Contains("Update"))
    {
        relationQuestion = "PatchMan이 묻습니다. '99%에서 멈춘 업데이트, 기다릴래? 강제 종료할래?'";
        SetRelationAnswers("완료 로그가 뜰 때까지 기다린다", 2, "NPC 404호: 인내심이 업데이트되었습니다.",
                           "패치 로그만 백업하고 재시도한다", 1, "NPC 404호: 현실적인 선택입니다.",
                           "전원 버튼을 길게 누른다", -2, "NPC 404호: 그건 해결이 아니라 재앙 버튼입니다.");
    }
    else if (f.Contains("System32"))
    {
        relationQuestion = "High-Kernel이 경고합니다. '핵심 파일을 건드릴 권한이 있나?'";
        SetRelationAnswers("정상 파일은 보호하고 감염 파일만 격리한다", 2, "NPC 404호: 드디어 System32 앞에서 손이 떨리는 법을 배웠군요.",
                           "권한 로그를 확인한 뒤 진행한다", 1, "NPC 404호: 무난합니다.",
                           "다 지우면 빨라질 것 같다", -3, "NPC 404호: 그 말 한 사람 대부분 포맷했습니다.");
    }
    else if (f.Contains("Network"))
    {
        relationQuestion = "Ping 선장이 말합니다. '외부 침입처럼 보이지만 시작점은 내부야. 누구를 의심할래?'";
        SetRelationAnswers("내부 조작 로그부터 확인한다", 2, "NPC 404호: 남 탓보다 로그 탓. 좋은 자세입니다.",
                           "외부 패킷도 함께 추적한다", 1, "NPC 404호: 시야는 넓군요.",
                           "무조건 해커 탓으로 한다", -2, "NPC 404호: 편리한 결론은 보통 틀립니다.");
    }
    else if (f.Contains("BSOD"))
    {
        relationQuestion = "BSOD가 쓰러진 뒤 파란 로그가 남습니다. '나는 원인인가, 결과인가?'";
        SetRelationAnswers("블루스크린은 결과일 수 있으니 원인을 더 찾는다", 2, "NPC 404호: 파란 화면 너머를 보셨군요.",
                           "격리하되 오류 코드를 기록한다", 1, "NPC 404호: 적어도 코드는 읽었네요.",
                           "파란색이면 다 악당이다", -2, "NPC 404호: 색깔로 재판하면 제어판도 위험합니다.");
    }
    else if (f.Contains("Registry"))
    {
        relationQuestion = "Regi가 레지스트리 키를 보여줍니다. '실행 경로를 바꾼 자는 권한을 가진 존재야.'";
        SetRelationAnswers("권한 로그와 실행 경로를 대조한다", 2, "NPC 404호: 추리가 드디어 사람 구실을 합니다.",
                           "의심 NPC 목록에 추가한다", 1, "NPC 404호: 적당히 합리적입니다.",
                           "복잡하니 레지스트리 전체 삭제", -3, "NPC 404호: 그건 추리가 아니라 자폭입니다.");
    }
    else if (f.Contains("Popup"))
    {
        relationQuestion = "Exception Queen이 묻습니다. '오류창을 읽지도 않고 닫은 자, 누구라고 생각해?'";
        SetRelationAnswers("오류 메시지 내용을 끝까지 읽는다", 2, "NPC 404호: 기적입니다. 오류창을 읽는 인간이 존재했군요.",
                           "일단 캡처하고 로그와 비교한다", 1, "NPC 404호: 증거 보존은 좋습니다.",
                           "X 버튼을 연타한다", -2, "NPC 404호: 그 손가락이 사건 현장입니다.");
    }
    else if (f.Contains("Temp"))
    {
        relationQuestion = "Temp 청소부가 말합니다. '임시파일은 지워져도 로그는 남아. 어떻게 할래?'";
        SetRelationAnswers("필요한 로그를 백업하고 정리한다", 2, "NPC 404호: 정리와 삭제의 차이를 아는군요.",
                           "용량 큰 것부터 확인한다", 1, "NPC 404호: 최소한 무작정 삭제는 아니네요.",
                           "전체 선택 후 Delete", -2, "NPC 404호: 당신의 정리 습관이 보스를 키웁니다.");
    }
    else if (f.Contains("Recycle"))
    {
        relationQuestion = "Illegal_Binny가 마지막으로 묻습니다. '버려진 파일들도 증언할 권리가 있을까?'";
        SetRelationAnswers("격리된 기록을 모두 확인한다", 2, "NPC 404호: 버려진 로그까지 들어주는군요.",
                           "중요한 기록만 복구한다", 1, "NPC 404호: 효율은 좋지만 놓치는 게 있을지도요.",
                           "휴지통은 비우라고 있는 것이다", -3, "NPC 404호: 네, 그리고 사건도 영원히 묻히겠죠.");
    }
    else
    {
        relationQuestion = "NPC가 묻습니다. '이 로그를 어떻게 처리할까요?'";
        SetRelationAnswers("보존한다", 1, "NPC 404호: 로그를 보존했습니다.", "보류한다", 0, "NPC 404호: 변화 없음", "삭제한다", -1, "NPC 404호: 로그가 불안정해졌습니다.");
    }
}

private void SetRelationAnswers(string a0, int d0, string r0, string a1, int d1, string r1, string a2, int d2, string r2)
{
    relationAnswers[0] = a0;
    relationAnswers[1] = a1;
    relationAnswers[2] = a2;
    relationDeltas[0] = d0;
    relationDeltas[1] = d1;
    relationDeltas[2] = d2;
    relationReactions[0] = r0;
    relationReactions[1] = r1;
    relationReactions[2] = r2;
}


private void ResolveRelationChoice(int index)
{
    if (!relationChoiceReady) { screen = GameScreen.Desktop; return; }
    if (index < 0) index = 0;
    if (index > 2) index = 2;
    int delta = relationDeltas[index];
    int before = npcRelationScore;
    npcRelationScore = Math.Max(0, Math.Min(12, npcRelationScore + delta));
    lastRelationDelta = delta;

    if (delta > 0) truthScore += delta >= 2 ? 2 : 1;
    else if (delta < 0) truthScore = Math.Max(0, truthScore + delta);

    // 첫 NPC 대화 이후에만 시스템이 플레이어를 admin으로 임시 등록한다.
    bool firstAdminAssigned = false;
    if (player.Name != "admin" && !heroNameUnlocked)
    {
        player.Name = "admin";
        firstAdminAssigned = true;
    }

    relationChoiceReady = false;
    pendingResultAction = relationNextAction;

    string changeText = delta > 0 ? "관계 변화: 상승" : (delta < 0 ? "관계 변화: 하락" : "관계 변화: 없음");
    if (delta != 0) changeText += " (" + (delta > 0 ? "+" : "") + delta + ")";
    message = relationReactions[index] + "  /  " + changeText;
    if (firstAdminAssigned)
        message += "  /  NPC 404호가 당신을 임시 프로세스명 'admin'으로 기록했습니다.";
    TryBeep(delta >= 2 ? 960 : (delta >= 0 ? 680 : 280), delta < 0 ? 100 : 70);

    if (!heroNameUnlocked && before < NameUnlockScore && npcRelationScore >= NameUnlockScore && player.Name.ToLower() == "admin")
    {
        heroNameUnlocked = true;
        playerNameBuffer = "";
        nameUnlockReturnAction = relationNextAction;
        adminNpcLine = "NPC 404호: 충분한 로그가 쌓였습니다. 임시 프로세스명을 계속 쓰면 추적이 꼬입니다. 지금 새 이름을 등록하세요.";
        message = "새 이름 등록 명령이 열렸습니다.";
        screen = GameScreen.AdminIntro;
        return;
    }

    if (pendingResultAction == "suspect") screen = GameScreen.SuspectSelect;
    else if (pendingResultAction == "ending") screen = GameScreen.Ending;
    else screen = GameScreen.Desktop;
}

        private void ChooseSuspect(int index)
        {
            string[] names = new string[] { "Driver-K", "High-Kernel", "BSOD", "Exception Queen", "Illegal_Binny", "Task Manager(Admin)" };
            if (index == 5)
            {
                resultText = "최종 추리 성공\n\n모든 격리 기록은 한 방향을 가리킵니다.\nDriver-K, High-Kernel, BSOD, Exception Queen, Illegal_Binny는 모두 체포된 피의자였지만,\n처음 권한을 부여하고 모든 격리를 실행한 존재는 Task Manager(Admin)였습니다.\n\n단, 최종 로그는 아직 완전히 열리지 않았습니다.\nNPC 404호: '정답을 맞혔지만, 시스템은 더 깊은 로그를 감추고 있습니다.'";
                pendingResultAction = "ending";
                screen = GameScreen.Result;
            }
            else
            {
                player.X = 180;
                player.Y = 330;
                player.TargetX = player.X;
                player.TargetY = player.Y;
                resultText = "오답: " + names[Math.Max(0, Math.Min(index, names.Length - 1))] + "\n\nNPC 404호: 보스가 수상하다고 다 범인은 아닙니다.\n격리 기록과 관리자 권한 로그를 다시 보세요.\n\n태초마을, 즉 바탕화면으로 돌아갑니다.";
                pendingResultAction = "desktop";
                screen = GameScreen.Result;
            }
        }

        private void DrawCinematicIntro(Graphics g)
        {
            using (LinearGradientBrush bg = new LinearGradientBrush(ClientRectangle, Color.FromArgb(5, 10, 30), Color.FromArgb(18, 58, 120), 90f))
                g.FillRectangle(bg, ClientRectangle);
            using (Pen grid = new Pen(Color.FromArgb(28, 110, 210, 255)))
            {
                for (int x = 0; x < ClientSize.Width; x += 46) g.DrawLine(grid, x, 0, x, ClientSize.Height);
                for (int y = 0; y < ClientSize.Height; y += 36) g.DrawLine(grid, 0, y, ClientSize.Width, y);
            }
            int scene = Math.Min(6, introTick / 190);
            float fade = Math.Min(1f, (introTick % 190) / 50f);
            Rectangle panel = new Rectangle(92, 78, ClientSize.Width - 184, ClientSize.Height - 156);
            Renderer.Panel(g, panel, Color.FromArgb(230, 238, 248));
            Renderer.Header(g, new Rectangle(panel.X + 8, panel.Y + 8, panel.Width - 16, 40), "TASK MANAGER SYSTEM BRIEFING");

            string title = "";
            string body = "";
            string code = "";
            if (scene == 0)
            {
                title = "무너져가는 OS";
                body = "바탕화면 평원에 원인 불명의 재앙이 감지되었습니다.\n응답하지 않는 창, 깨진 아이콘, 사라진 프로젝트 파일...\n관리자 권한은 임시 용사를 호출합니다.";
                code = "CPU: 83%   RAM: 91%   BSOD RISK: RISING";
            }
            else if (scene == 1)
            {
                title = "Final_Project_REAL_LAST.exe 실종";
                body = "사라진 파일의 흔적은 평범한 폴더가 아니라 던전화된 파일 안에 남아 있습니다.\n파일을 실행하면, 그 내부가 전장이 됩니다.";
                code = "TARGET FILE: <MISSING>   LOCATION: ENCRYPTED";
            }
            else if (scene == 2)
            {
                title = "10개의 파일 던전";
                body = "Explorer, Popup, Recycle Bin, Update, Control Panel, Network, Registry, Driver, System32, BSOD.\n각 던전은 다른 로그와 다른 용의자를 숨깁니다.";
                code = "STAGE COUNT: 10   FINAL LOG: LOCKED";
            }
            else if (scene == 3)
            {
                title = "단일 용사 커스터마이징";
                body = "직업은 하나. 대신 의상, 무기, 갑옷, 망토로 전투 스타일을 완성하세요.\n마우스 우클릭으로 이동하고 Q/W/E/R로 시스템을 돌파합니다.";
                code = "INPUT: RIGHT CLICK + Q/W/E/R";
            }
            else if (scene == 4)
            {
                title = "힌트는 선택에 따라 달라진다";
                body = "파일을 데려갈 것인가, 버릴 것인가.\n무기를 강화할 것인가, 로그를 읽을 것인가.\n가벼운 선택처럼 보여도 진실의 방향은 조금씩 바뀝니다.";
                code = "HINT QUALITY: VARIABLE   TRUTH SCORE: UNKNOWN";
            }
            else if (scene == 5)
            {
                title = "범인은 아직 잠겨 있다";
                body = "Task Manager는 모든 정보를 말하지 않습니다.\n몬스터가 정말 몬스터인지, NPC가 정말 아군인지, 최종 로그를 열기 전까지는 알 수 없습니다.";
                code = "CULPRIT: <LOCKED UNTIL FINAL LOG>";
            }
            else
            {
                title = "디버그 용사: Windows File Dungeon DX";
                body = "파일을 실행하라. 로그를 읽어라.\n그리고 아무도 믿지 마라.\n단, 결말은 직접 실행해야 열린다.";
                code = "PRESS ENTER / SPACE TO START";
            }

            int alpha = Math.Max(0, Math.Min(255, (int)(255 * fade)));
            using (Font ft = Renderer.Font(33f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(alpha, 20, 45, 85)))
                g.DrawString(title, ft, b, new Rectangle(panel.X + 50, panel.Y + 88, panel.Width - 100, 60), Renderer.Center());
            using (Font fb = Renderer.Font(14f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(alpha, 35, 55, 82)))
                g.DrawString(body, fb, b, new Rectangle(panel.X + 78, panel.Y + 172, panel.Width - 156, 150), Renderer.Center());
            Rectangle error = new Rectangle(panel.X + 95, panel.Bottom - 190, panel.Width - 190, 92);
            Renderer.Inset(g, error, Color.FromArgb(8, 18, 42));
            using (Font fc = Renderer.Font(13f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(alpha, 120, 240, 255)))
                g.DrawString(code, fc, b, error, Renderer.Center());
            Rectangle skip = new Rectangle(panel.Right - 155, panel.Bottom - 58, 120, 34);
            Renderer.Button(g, skip, "SKIP", false);
            buttons.Add(new UiButton(skip, "skipIntro"));
            using (Font f = Renderer.Font(8.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(80, 40, 55, 80)))
                g.DrawString("결말 스포일러 없음 · 모든 단서는 게임 내부 로그에서 해금", f, b, new Rectangle(panel.X + 30, panel.Bottom - 58, 480, 34), Renderer.LeftMiddle());
        }





        private void DrawRelationChoice(Graphics g)
        {
            Renderer.DrawDesktop(g, ClientRectangle, dungeons, selectedFile, player, buttons);
            using (SolidBrush overlay = new SolidBrush(Color.FromArgb(170, 0, 0, 0))) g.FillRectangle(overlay, ClientRectangle);
            Rectangle box = new Rectangle(ClientSize.Width / 2 - 460, ClientSize.Height / 2 - 285, 920, 570);
            Renderer.Panel(g, box, Color.FromArgb(238, 244, 252));
            Renderer.Header(g, new Rectangle(box.X + 8, box.Y + 8, box.Width - 16, 42), "던전 클리어 후 NPC 대화");

            Rectangle npc = new Rectangle(box.X + 42, box.Y + 76, 210, 260);
            Renderer.Inset(g, npc, Color.FromArgb(12, 22, 48));
            using (Font f = Renderer.Font(42f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(120, 220, 255)))
                g.DrawString("404", f, b, new Rectangle(npc.X, npc.Y + 20, npc.Width, 80), Renderer.Center());
            using (Font f = Renderer.Font(10.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.White))
                g.DrawString("NPC 404호\n로그 기록 관리자\n당신의 대답은 조용히 기록됩니다.", f, b, new Rectangle(npc.X + 14, npc.Y + 118, npc.Width - 28, 90), Renderer.Center());
            using (Font f = Renderer.Font(8.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(180, 230, 255)))
                g.DrawString("선택 후 로그 반응이 표시됩니다.", f, b, new Rectangle(npc.X + 10, npc.Bottom - 48, npc.Width - 20, 22), Renderer.Center());

            Rectangle dialog = new Rectangle(box.X + 280, box.Y + 76, box.Width - 322, 185);
            Renderer.Inset(g, dialog, Color.White);
            using (Font f = Renderer.Font(11f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(36, 54, 82)))
            {
                string dialogText = relationContext + "\n\n" + relationQuestion;
                g.DrawString(dialogText, f, b, new Rectangle(dialog.X + 18, dialog.Y + 16, dialog.Width - 36, dialog.Height - 32), Renderer.LeftTop());
            }

            for (int i = 0; i < 3; i++)
            {
                Rectangle r = new Rectangle(box.X + 280, box.Y + 285 + i * 62, box.Width - 322, 48);
                Renderer.Button(g, r, (i + 1) + ". " + relationAnswers[i], i == 0);
                buttons.Add(new UiButton(r, "rel" + i));
            }

            using (Font f = Renderer.Font(9f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(80, 90, 110)))
                g.DrawString("1/2/3 키 또는 버튼 클릭 · 결과는 선택 후 로그로만 확인됩니다.", f, b, new Rectangle(box.X + 42, box.Bottom - 42, box.Width - 84, 24), Renderer.Center());
        }

        private void DrawSuspectSelect(Graphics g)
        {
            Renderer.DrawDesktop(g, ClientRectangle, dungeons, selectedFile, player, buttons);
            using (SolidBrush overlay = new SolidBrush(Color.FromArgb(165, 0, 0, 0))) g.FillRectangle(overlay, ClientRectangle);
            Rectangle box = new Rectangle(ClientSize.Width / 2 - 440, 64, 880, ClientSize.Height - 128);
            Renderer.Panel(g, box, Color.FromArgb(238, 244, 252));
            Renderer.Header(g, new Rectangle(box.X + 8, box.Y + 8, box.Width - 16, 40), "범인 추리 - 격리 구역 로그 분석");
            string desc = "10단계 스테이지가 끝났습니다. 모든 보스는 NPC 404호에 의해 Recycle Bin 격리 구역으로 이동했습니다.\n누가 이 흐름을 설계했는지 최종 권한 로그를 근거로 지목하세요.\n오답을 고르면 태초마을로 돌아갑니다.";
            using (Font f = Renderer.Font(11.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(32, 44, 68)))
                g.DrawString(desc, f, b, new Rectangle(box.X + 42, box.Y + 66, box.Width - 84, 82), Renderer.Center());
            string[] names = new string[] { "1. Driver-K", "2. High-Kernel", "3. BSOD", "4. Exception Queen", "5. Illegal_Binny", "6. Task Manager(Admin)" };
            for (int i = 0; i < names.Length; i++)
            {
                Rectangle r = new Rectangle(box.X + 130, box.Y + 165 + i * 50, box.Width - 260, 40);
                Renderer.Button(g, r, names[i], i == 5);
                buttons.Add(new UiButton(r, "suspect" + i));
            }
            using (Font f = Renderer.Font(9f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(80, 90, 110)))
                g.DrawString("힌트: 범인은 단순히 강한 보스가 아니라, 모든 격리와 권한 부여를 실행한 존재입니다.", f, b, new Rectangle(box.X + 40, box.Bottom - 50, box.Width - 80, 24), Renderer.Center());
        }

        private void DrawEnding(Graphics g)
        {
            using (LinearGradientBrush bg = new LinearGradientBrush(ClientRectangle, Color.FromArgb(5, 8, 26), Color.FromArgb(45, 12, 68), 90f))
                g.FillRectangle(bg, ClientRectangle);
            using (Pen p = new Pen(Color.FromArgb(35, 210, 90, 255)))
            {
                for (int x = 0; x < ClientSize.Width; x += 50) g.DrawLine(p, x, 0, x, ClientSize.Height);
                for (int y = 0; y < ClientSize.Height; y += 40) g.DrawLine(p, 0, y, ClientSize.Width, y);
            }
            Rectangle box = new Rectangle(ClientSize.Width / 2 - 430, 86, 860, ClientSize.Height - 172);
            Renderer.Panel(g, box, Color.FromArgb(238, 244, 252));
            Renderer.Header(g, new Rectangle(box.X + 8, box.Y + 8, box.Width - 16, 42), "SYSTEM RESTORED");
            using (Font title = Renderer.Font(26f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(35, 45, 75)))
                g.DrawString("REAL_LAST_FINAL_FIXED.exe 복구 완료", title, b, new Rectangle(box.X + 34, box.Y + 80, box.Width - 68, 50), Renderer.Center());
            string ending = "최종 로그가 복구되었습니다.\n\n오류는 누군가의 악의만으로 만들어진 것이 아니었습니다.\n무시한 경고, 미룬 업데이트, 정리하지 않은 파일, 덮어쓴 최종본이\n시스템 전체를 하나의 던전으로 바꾸었습니다.\n\nNPC 404호: 최고의 디버깅은 남을 의심하는 것이 아니라,\n자기 시스템을 점검하는 것입니다.\n\n클리어!";
            using (Font f = Renderer.Font(13f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(34, 48, 72)))
                g.DrawString(ending, f, b, new Rectangle(box.X + 70, box.Y + 150, box.Width - 140, 250), Renderer.Center());
            Rectangle ok = new Rectangle(box.X + box.Width / 2 - 115, box.Bottom - 72, 230, 42);
            Renderer.Button(g, ok, "타이틀로", true);
            buttons.Add(new UiButton(ok, "endingTitle"));
        }



        private void DrawAdminIntro(Graphics g)
        {
            using (LinearGradientBrush bg = new LinearGradientBrush(ClientRectangle, Color.FromArgb(7, 10, 30), Color.FromArgb(30, 70, 130), 90f))
                g.FillRectangle(bg, ClientRectangle);
            Rectangle box = new Rectangle(ClientSize.Width / 2 - 450, ClientSize.Height / 2 - 240, 900, 480);
            Renderer.Panel(g, box, Color.FromArgb(236, 243, 252));
            Renderer.Header(g, new Rectangle(box.X + 8, box.Y + 8, box.Width - 16, 42), "PROCESS IDENTITY REWRITE");

            using (Font title = Renderer.Font(22f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(24, 45, 82)))
                g.DrawString("NPC 404호가 새 이름 등록을 요구합니다", title, b, new Rectangle(box.X + 34, box.Y + 70, box.Width - 68, 42), Renderer.Center());

            Rectangle npc = new Rectangle(box.X + 52, box.Y + 145, 220, 220);
            Renderer.Inset(g, npc, Color.FromArgb(12, 24, 52));
            using (Font f = Renderer.Font(58f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(110, 220, 255)))
                g.DrawString("404", f, b, new Rectangle(npc.X, npc.Y + 18, npc.Width, 92), Renderer.Center());
            using (Font f = Renderer.Font(10f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.White))
                g.DrawString("NPC 404호\n임시 프로세스명을\n재등록하려 합니다", f, b, new Rectangle(npc.X + 12, npc.Bottom - 82, npc.Width - 24, 70), Renderer.Center());

            Rectangle talk = new Rectangle(box.X + 305, box.Y + 145, box.Width - 365, 145);
            Renderer.Inset(g, talk, Color.White);
            using (Font f = Renderer.Font(12f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(32, 46, 70)))
                g.DrawString(adminNpcLine, f, b, new Rectangle(talk.X + 18, talk.Y + 16, talk.Width - 36, talk.Height - 32), Renderer.LeftTop());

            Rectangle input = new Rectangle(box.X + 305, box.Y + 320, box.Width - 365, 54);
            Renderer.Inset(g, input, Color.FromArgb(248, 252, 255));
            using (Font f = Renderer.Font(16f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(20, 80, 170)))
                g.DrawString(playerNameBuffer + "_", f, b, new Rectangle(input.X + 18, input.Y, input.Width - 36, input.Height), Renderer.LeftMiddle());

            Rectangle ok = new Rectangle(box.Right - 292, box.Bottom - 70, 230, 42);
            Renderer.Button(g, ok, "이름 등록", true);
            buttons.Add(new UiButton(ok, "confirmName"));
            Rectangle back = new Rectangle(box.X + 70, box.Bottom - 70, 210, 42);
            Renderer.Button(g, back, "나중에", false);
            buttons.Add(new UiButton(back, "desktop"));

            using (Font f = Renderer.Font(9f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(80, 90, 110)))
                g.DrawString("키보드로 이름 입력 · Backspace 수정 · Enter 등록 · admin은 사용할 수 없습니다.", f, b, new Rectangle(box.X + 42, box.Bottom - 105, box.Width - 84, 24), Renderer.Center());
        }

        private void DrawTitle(Graphics g)
        {
            using (LinearGradientBrush bg = new LinearGradientBrush(ClientRectangle, Color.FromArgb(10, 20, 58), Color.FromArgb(35, 105, 185), 90f)) g.FillRectangle(bg, ClientRectangle);
            using (SolidBrush glow = new SolidBrush(Color.FromArgb(52, 110, 220, 255))) g.FillEllipse(glow, ClientSize.Width / 2 - 380, 110, 760, 260);
            using (Font title = Renderer.Font(40f, FontStyle.Bold))
            using (SolidBrush white = new SolidBrush(Color.White))
            using (SolidBrush cyan = new SolidBrush(Color.FromArgb(120, 235, 255)))
            {
                g.DrawString("디버그 용사", title, white, new Rectangle(0, 120, ClientSize.Width, 70), Renderer.Center());
                g.DrawString("Windows File Dungeon DX", title, cyan, new Rectangle(0, 184, ClientSize.Width, 70), Renderer.Center());
            }
            using (Font sub = Renderer.Font(13f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(230, 245, 255)))
                g.DrawString("결말은 잠겨 있다 · 10개의 파일 던전 · 숨겨진 로그와 용의자 추리 · 단일 용사 커스터마이징", sub, b, new Rectangle(0, 260, ClientSize.Width, 34), Renderer.Center());
            Player demo = new Player();
            demo.ApplyCustomization(player.Outfit, player.Weapon, player.Armor, player.Cape);
            demo.X = ClientSize.Width / 2;
            demo.Y = 430;
            Renderer.DrawHero(g, demo, 0, 0, tick, true, true);
            Rectangle start = new Rectangle(ClientSize.Width / 2 - 160, ClientSize.Height - 190, 320, 52);
            Rectangle load = new Rectangle(ClientSize.Width / 2 - 160, ClientSize.Height - 128, 150, 42);
            Rectangle help = new Rectangle(ClientSize.Width / 2 + 10, ClientSize.Height - 128, 150, 42);
            Renderer.Button(g, start, "시작 / 커스터마이징", true);
            Renderer.Button(g, load, "불러오기", false);
            Renderer.Button(g, help, "도움말", false);
            buttons.Add(new UiButton(start, "new"));
            buttons.Add(new UiButton(load, "load"));
            buttons.Add(new UiButton(help, "help"));
        }

        // 1. 메인 던전 그리기 메서드
        private void DrawDungeon(Graphics g)
        {
            if (currentDungeon == null) { screen = GameScreen.Desktop; return; }

            // 배경 및 전장 렌더링
            Renderer.DrawArena(g, ClientRectangle, currentDungeon, cameraX, cameraY, tick);

            // 드롭 아이템 및 몬스터/영웅 렌더링
            for (int i = 0; i < drops.Count; i++) if (drops[i] != draggingItem) Renderer.DrawDroppedItem(g, drops[i], cameraX, cameraY);
            for (int i = 0; i < monsters.Count; i++) Renderer.DrawMonster(g, monsters[i], cameraX, cameraY, tick);
            Renderer.DrawHero(g, player, cameraX, cameraY, tick, moving, false);

            // 히어로 좌표 계산 및 이펙트 렌더링
            PointF hs = Renderer.WorldToScreen(player.X, player.Y, cameraX, cameraY);
            lastHeroScreenRect = new Rectangle((int)hs.X - 46, (int)hs.Y - 92, 92, 122);
            for (int i = 0; i < effects.Count; i++) Renderer.DrawEffect(g, effects[i], cameraX, cameraY);
            if (draggingItem != null) Renderer.DrawDroppedItem(g, draggingItem, cameraX, cameraY);

            // --- [수정된 위치] 보스 스테이지일 때 기믹 렌더링 호출 ---
            if (currentDungeon.Boss)
            {
                DrawBossGimmicks(g);
            }

            // 최상단 UI 렌더링
            DrawHud(g);
        }

        // 2. 보스 기믹 전용 렌더링 메서드 (별도로 분리)
        private void DrawBossGimmicks(Graphics g)
        {
            // 리소스 부족 패턴 UI (보스 체력 50% 패턴) 
            if (bossManager.IsResourcePatternActive)
            {
                Rectangle overlay = new Rectangle(ClientSize.Width / 2 - 400, ClientSize.Height / 2 - 150, 800, 300);
                Renderer.Panel(g, overlay, Color.FromArgb(180, 20, 20, 20));

                using (Font f = Renderer.Font(16f, FontStyle.Bold))
                    g.DrawString("!!! RESOURCE EXHAUSTED !!!\nCLICK ALL DEBUG BUTTONS FAST!", f, Brushes.Yellow, overlay, Renderer.Center());

                foreach (var btn in bossManager.DebugButtons)
                    Renderer.Button(g, btn, "DEBUG.EXE", true);
            }

            // 드라이브 조각 렌더링 (보스 체력 75%, 25% 패턴) [cite: 12]
            if (bossManager.IsShardPatternActive)
            {
                PointF sPos = Renderer.WorldToScreen(bossManager.CurrentShardPos.X, bossManager.CurrentShardPos.Y, cameraX, cameraY);
                Rectangle shardRect = new Rectangle((int)sPos.X - 25, (int)sPos.Y - 25, 50, 50);

                // 조각 아이콘 및 남은 시간 표시 [cite: 12, 13]
                Renderer.DrawLargeFileSymbol(g, shardRect, Color.LimeGreen, false);

                using (Font f = Renderer.Font(11f, FontStyle.Bold))
                {
                    string timerText = $"FIND DRIVE SHARD! ({(bossManager.ShardTimer / 60.0):0.0}s)"; // 5초 카운트다운 
                    g.DrawString(timerText, f, Brushes.Lime, new Point(ClientSize.Width / 2 - 100, 80));
                }
            }
        }


        private void DrawHud(Graphics g)
        {
            Rectangle top = new Rectangle(12, 12, 392, 160);
            Renderer.Panel(g, top, Color.FromArgb(232, 239, 248));
            Renderer.Header(g, new Rectangle(top.X + 5, top.Y + 5, top.Width - 10, 28), "용사 상태");
            using (Font f = Renderer.Font(9f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(26, 40, 64)))
            {
                g.DrawString(player.Name + " Lv." + player.Level + "  " + player.WeaponName, f, b, new Rectangle(top.X + 16, top.Y + 42, top.Width - 32, 18), Renderer.LeftMiddle());
            }
            DrawBarWithLabel(g, new Rectangle(top.X + 58, top.Y + 68, 210, 15), "HP", player.Hp, player.MaxHp, Color.LimeGreen);
            DrawBarWithLabel(g, new Rectangle(top.X + 58, top.Y + 92, 210, 15), "MP", player.Mp, player.MaxMp, Color.DeepSkyBlue);
            DrawBarWithLabel(g, new Rectangle(top.X + 58, top.Y + 116, 210, 15), "EXP", player.Exp, player.NextExp, Color.Cyan);
            using (Font f = Renderer.Font(8f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(42, 55, 76)))
                g.DrawString("Gold " + player.Gold + "   패치 " + player.PatchShards + "   드롭 " + drops.Count + "   장착 " + player.EquippedItems + "   보관 " + player.StoredItems, f, b, new Rectangle(top.X + 16, top.Y + 137, top.Width - 32, 18), Renderer.LeftMiddle());

            Rectangle skill = new Rectangle(ClientSize.Width / 2 - 280, ClientSize.Height - 88, 560, 72);
            Renderer.Panel(g, skill, Color.FromArgb(226, 233, 244));
            DrawSkill(g, new Rectangle(skill.X + 14, skill.Y + 12, 86, 48), "Q", "검기", qCd);
            DrawSkill(g, new Rectangle(skill.X + 108, skill.Y + 12, 86, 48), "W", "파동", wCd);
            DrawSkill(g, new Rectangle(skill.X + 202, skill.Y + 12, 86, 48), "E", "보호막", eCd);
            DrawSkill(g, new Rectangle(skill.X + 296, skill.Y + 12, 86, 48), "R", "궁극", rCd);
            DrawSkill(g, new Rectangle(skill.X + 400, skill.Y + 12, 68, 48), "D", "HP " + player.Potion, 0);
            DrawSkill(g, new Rectangle(skill.X + 476, skill.Y + 12, 68, 48), "F", "MP " + player.MpPotion, 0);

            Rectangle guide = new Rectangle(ClientSize.Width - 395, 12, 380, 92);
            Renderer.Panel(g, guide, Color.FromArgb(232, 239, 248));
            using (Font f = Renderer.Font(8.8f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(32, 46, 70)))
            {
                string s = "우클릭: 이동 목표 지정 / 유지 이동\nSpace: 도약   Q/W/E/R: 스킬\n드롭 파일: 마우스로 용사에게 드래그";
                g.DrawString(s, f, b, new Rectangle(guide.X + 14, guide.Y + 12, guide.Width - 28, guide.Height - 24), Renderer.LeftTop());
            }
        }

        private void DrawBarWithLabel(Graphics g, Rectangle r, string label, int value, int max, Color c)
        {
            using (Font f = Renderer.Font(7.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(24, 34, 52))) g.DrawString(label, f, b, new Rectangle(r.X - 42, r.Y - 1, 38, r.Height + 2), Renderer.LeftMiddle());
            Renderer.Bar(g, r, value, max, c);
            using (Font f = Renderer.Font(7.5f, FontStyle.Regular))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(24, 34, 52))) g.DrawString(value + "/" + max, f, b, new Rectangle(r.Right + 8, r.Y - 1, 90, r.Height + 2), Renderer.LeftMiddle());
        }

        private void DrawSkill(Graphics g, Rectangle r, string key, string name, int cd)
        {
            Renderer.Panel(g, r, cd > 0 ? Color.FromArgb(190, 195, 205) : Color.FromArgb(232, 240, 255));
            using (Font f = Renderer.Font(13f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(cd > 0 ? Color.Gray : Color.FromArgb(32, 80, 170))) g.DrawString(key, f, b, new Rectangle(r.X, r.Y + 2, r.Width, 20), Renderer.Center());
            using (Font f = Renderer.Font(7.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(32, 42, 60))) g.DrawString(cd > 0 ? cd.ToString() : name, f, b, new Rectangle(r.X + 2, r.Y + 25, r.Width - 4, 18), Renderer.Center());
        }

        private void DrawEquipPrompt(Graphics g)
        {
            if (pendingItem == null) return;
            using (SolidBrush overlay = new SolidBrush(Color.FromArgb(130, 0, 0, 0))) g.FillRectangle(overlay, ClientRectangle);
            Rectangle box = new Rectangle(ClientSize.Width / 2 - 260, ClientSize.Height / 2 - 120, 520, 240);
            Renderer.Panel(g, box, Color.FromArgb(236, 242, 250));
            Renderer.Header(g, new Rectangle(box.X + 6, box.Y + 6, box.Width - 12, 34), "드롭 파일 처리");
            Renderer.DrawDroppedItem(g, pendingItem, pendingItem.X - box.X - 78, pendingItem.Y - box.Y - 80);
            using (Font f = Renderer.Font(11f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(30, 42, 66)))
                g.DrawString(pendingItem.Name + "\n" + pendingItem.Description, f, b, new Rectangle(box.X + 140, box.Y + 58, box.Width - 166, 74), Renderer.LeftTop());
            Rectangle equip = new Rectangle(box.X + 92, box.Bottom - 70, 150, 42);
            Rectangle store = new Rectangle(box.Right - 242, box.Bottom - 70, 150, 42);
            bool equippable = pendingItem.Kind == ItemKind.Weapon || pendingItem.Kind == ItemKind.Armor || pendingItem.Kind == ItemKind.Outfit;
            Renderer.Button(g, equip, equippable ? "장착(Y)" : "자동보관", equippable);
            Renderer.Button(g, store, "보관(N)", true);
            if (equippable) buttons.Add(new UiButton(equip, "equip"));
            buttons.Add(new UiButton(store, "store"));
        }

        private void DrawResult(Graphics g)
        {
            Renderer.DrawDesktop(g, ClientRectangle, dungeons, selectedFile, player, buttons);
            using (SolidBrush overlay = new SolidBrush(Color.FromArgb(150, 0, 0, 0))) g.FillRectangle(overlay, ClientRectangle);
            Rectangle box = new Rectangle(ClientSize.Width / 2 - 290, ClientSize.Height / 2 - 170, 580, 340);
            Renderer.Panel(g, box, Color.FromArgb(238, 244, 252));
            Renderer.Header(g, new Rectangle(box.X + 6, box.Y + 6, box.Width - 12, 38), "던전 클리어");
            using (Font f = Renderer.Font(13f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(28, 42, 68)))
                g.DrawString(resultText, f, b, new Rectangle(box.X + 36, box.Y + 70, box.Width - 72, 175), Renderer.Center());
            Rectangle ok = new Rectangle(box.X + 190, box.Bottom - 72, 200, 42);
            Renderer.Button(g, ok, pendingResultAction == "relation" ? "NPC 대화 선택으로" : (pendingResultAction == "suspect" ? "용의자 추리로" : (pendingResultAction == "ending" ? "엔딩 보기" : "바탕화면으로")), true);
            buttons.Add(new UiButton(ok, "resultContinue"));
        }

        private void DrawHelp(Graphics g)
        {
            using (LinearGradientBrush bg = new LinearGradientBrush(ClientRectangle, Color.FromArgb(18, 32, 72), Color.FromArgb(48, 98, 165), 90f)) g.FillRectangle(bg, ClientRectangle);
            Rectangle box = new Rectangle(120, 80, ClientSize.Width - 240, ClientSize.Height - 160);
            Renderer.Panel(g, box, Color.FromArgb(238, 244, 252));
            Renderer.Header(g, new Rectangle(box.X + 8, box.Y + 8, box.Width - 16, 36), "도움말");
            string txt = "이번 버전 변경점\n\n" +
                         "• 직업 선택 제거: 용사는 하나입니다.\n" +
                         "• 시작 시 의상, 무기, 갑옷, 망토를 커스터마이징합니다.\n" +
                         "• 던전 전투는 단일 라인 아레나 시야로 변경했습니다.\n" +
                         "• 스토리: 10단계 순차 스테이지, 5개 보스 격리, 최종 범인 추리를 반영했습니다.\n" +
                         "• 이동: 마우스 우클릭 / 우클릭 유지\n" +
                         "• 점프/도약: Space\n" +
                         "• 스킬: Q W E R, 포션: D F\n" +
                         "• 드롭 파일을 마우스로 용사에게 드래그하면 장착 또는 보관을 선택할 수 있습니다.\n\n" +
                         "주의: 특정 상용 게임의 리소스나 이름을 사용하지 않고, 단일 라인 아레나 전투 느낌만 C# WinForms로 직접 구현했습니다.";
            using (Font f = Renderer.Font(12f, FontStyle.Regular))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(28, 42, 68))) g.DrawString(txt, f, b, new Rectangle(box.X + 38, box.Y + 70, box.Width - 76, box.Height - 120), Renderer.LeftTop());
        }
    }
}
