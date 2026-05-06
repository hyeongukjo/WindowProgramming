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
        private readonly Timer timer;
        private readonly Random random = new Random();
        private readonly List<UiButton> buttons = new List<UiButton>();
        private readonly List<Effect> effects = new List<Effect>();
        private readonly List<string> log = new List<string>();
        private readonly string saveFile;

        private GameScreen screen = GameScreen.Title;
        private Player player = new Player();
        private DungeonInfo currentDungeon;
        private List<Platform> platforms = new List<Platform>();
        private List<Monster> monsters = new List<Monster>();
        private int selectedJob = 0;
        private int selectedFile = 0;
        private int tick;
        private float cameraX;
        private bool left;
        private bool right;
        private bool jumpHeld;
        private bool moving;
        private int attackCooldown;
        private int skillCooldown;
        private int skill2Cooldown;
        private int ultimateCooldown;
        private int itemCooldown;
        private int stageClearTicks;
        private int comboCount;
        private int comboTimer;
        private int maxCombo;
        private int dungeonStartTick;
        private int dungeonHitCount;
        private int dungeonKillCount;
        private bool hiddenFolderFound;
        private string lastDropText = "없음";
        private readonly HashSet<string> achievements = new HashSet<string>();
        private int introShakeTicks;
        private int npcToastTicks;
        private string npcToast = "";
        private int npcLineSeed;
        private string resultText = "";
        private string playerName = "디버그용사";
        private DungeonType lastClearedDungeonType = DungeonType.FileExplorerForest;
        private string lastClearGrade = "";
        private string storyTitle = "";
        private string storyBody = "";
        private string endingTitle = "";
        private string endingBody = "";
        private bool trueFinalUnlocked;
        private bool normalEndingSeen;
        private bool trueEndingSeen;
        private int clueCount;
        private int companionCount;
        private int abandonedCount;
        private int deletedCount;
        private int suspectFailCount;
        private string companionHistory = "";
        private string hintHistory = "";

        public MainForm()
        {
            Text = "디버그 용사: Windows File Dungeon DX";
            ClientSize = new Size(1366, 768);
            MinimumSize = new Size(1100, 680);
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;
            KeyPreview = true;
            BackColor = Color.Black;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);

            saveFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DebugHeroFileDungeonRPG_save.txt");
            player.ApplyJob(JobType.DebugWarrior);
            AddLog("이동: ←/→, 점프: Space/↑, 공격/스킬: Q/W/E/R, 아이템: D/F");
            AddLog("파일을 선택해서 던전에 진입하세요.");
            AddLog("추가 재미 요소: 콤보, 아이템 드롭, 숨겨진 폴더, 클리어 등급, 보스 패턴");
            AddLog("병맛 NPC, 에러창 용사 소개, 전직, 무기 강화가 추가되었습니다.");
            AddLog("스토리 추가: 오류창 대화, 감정 선택, 범인 추리, 진범 반전, 찐 최종 던전.");

            timer = new Timer();
            timer.Interval = 16;
            timer.Tick += Timer_Tick;
            timer.Start();

            KeyDown += MainForm_KeyDown;
            KeyUp += MainForm_KeyUp;
            KeyPress += MainForm_KeyPress;
            MouseDown += MainForm_MouseDown;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            tick++;
            if (attackCooldown > 0) attackCooldown--;
            if (skillCooldown > 0) skillCooldown--;
            if (skill2Cooldown > 0) skill2Cooldown--;
            if (ultimateCooldown > 0) ultimateCooldown--;
            if (itemCooldown > 0) itemCooldown--;
            if (introShakeTicks > 0) introShakeTicks--;
            if (npcToastTicks > 0) npcToastTicks--;
            if (comboTimer > 0)
            {
                comboTimer--;
                if (comboTimer <= 0) comboCount = 0;
            }
            if (player.InvincibleTicks > 0) player.InvincibleTicks--;
            if (player.ShieldTicks > 0) player.ShieldTicks--;

            for (int i = effects.Count - 1; i >= 0; i--)
            {
                effects[i].Ticks--;
                if (effects[i].Ticks <= 0) effects.RemoveAt(i);
            }

            if (screen == GameScreen.Dungeon)
            {
                UpdateDungeon();
            }
            Invalidate();
        }

        private void UpdateDungeon()
        {
            moving = false;
            float accel = player.OnGround ? 0.85f : 0.42f;
            float maxSpeed = 5.6f + player.Speed * 0.07f;
            if (left)
            {
                player.VX -= accel;
                player.Facing = -1;
                moving = true;
            }
            if (right)
            {
                player.VX += accel;
                player.Facing = 1;
                moving = true;
            }
            if (!left && !right)
            {
                player.VX *= player.OnGround ? 0.78f : 0.94f;
                if (Math.Abs(player.VX) < 0.05f) player.VX = 0;
            }
            if (player.VX > maxSpeed) player.VX = maxSpeed;
            if (player.VX < -maxSpeed) player.VX = -maxSpeed;

            player.VY += 0.58f;
            if (player.VY > 16f) player.VY = 16f;

            float oldX = player.X;
            float oldY = player.Y;
            player.X += player.VX;
            player.Y += player.VY;
            if (player.X < 40) player.X = 40;
            if (currentDungeon != null && player.X > currentDungeon.MapWidth - 40) player.X = currentDungeon.MapWidth - 40;

            ResolvePlatformCollision(oldX, oldY);

            for (int i = 0; i < monsters.Count; i++)
            {
                UpdateMonster(monsters[i]);
            }
            CheckMonsterCollision();
            CheckHiddenFolder();
            UpdateCamera();
            CheckClearCondition();
        }

        private void ResolvePlatformCollision(float oldX, float oldY)
        {
            player.OnGround = false;
            RectangleF pb = player.Bounds;
            for (int i = 0; i < platforms.Count; i++)
            {
                Platform p = platforms[i];
                if (pb.IntersectsWith(p.Bounds))
                {
                    RectangleF oldB = new RectangleF(oldX - 24, oldY - 56, 48, 56);
                    if (oldB.Bottom <= p.Bounds.Top + 8 && player.VY >= 0)
                    {
                        player.Y = p.Bounds.Top;
                        player.VY = 0;
                        player.OnGround = true;
                        pb = player.Bounds;
                    }
                    else if (oldB.Top >= p.Bounds.Bottom - 4 && player.VY < 0)
                    {
                        player.Y = p.Bounds.Bottom + 56;
                        player.VY = 0;
                        pb = player.Bounds;
                    }
                    else if (oldB.Right <= p.Bounds.Left && player.VX > 0)
                    {
                        player.X = p.Bounds.Left - 25;
                        player.VX = 0;
                        pb = player.Bounds;
                    }
                    else if (oldB.Left >= p.Bounds.Right && player.VX < 0)
                    {
                        player.X = p.Bounds.Right + 25;
                        player.VX = 0;
                        pb = player.Bounds;
                    }
                }
            }
            if (player.Y > 720)
            {
                DamagePlayer(30);
                player.X = 120;
                player.Y = 300;
                player.VX = 0;
                player.VY = 0;
            }
        }

        private void UpdateMonster(Monster m)
        {
            if (m.Hp <= 0) return;
            if (m.HitFlash > 0) m.HitFlash--;
            if (m.AttackCooldown > 0) m.AttackCooldown--;
            if (m.IsBoss)
            {
                float dist = player.X - m.X;
                if (Math.Abs(dist) < 480) m.VX += dist > 0 ? 0.04f : -0.04f;
                if (m.VX > 2.0f) m.VX = 2.0f;
                if (m.VX < -2.0f) m.VX = -2.0f;
                m.X += m.VX;
                TryBossPattern(m, dist);
            }
            else
            {
                m.X += m.VX;
                if (m.X < 140 || (currentDungeon != null && m.X > currentDungeon.MapWidth - 160)) m.VX = -m.VX;
                if (random.Next(0, 240) == 0) m.VX = -m.VX;
            }
            m.Facing = m.VX >= 0 ? 1 : -1;
        }

        private void CheckMonsterCollision()
        {
            RectangleF pb = player.Bounds;
            for (int i = 0; i < monsters.Count; i++)
            {
                Monster m = monsters[i];
                if (m.Hp <= 0) continue;
                if (pb.IntersectsWith(m.Bounds) && player.InvincibleTicks <= 0)
                {
                    DamagePlayer(Math.Max(3, m.Attack - player.Defense / 2));
                    player.VX = player.X < m.X ? -8f : 8f;
                    player.VY = -5f;
                    AddEffect(EffectKind.HitSpark, player.X, player.Y - 32, player.X, player.Y - 32, 18, Color.Red, "", player.Facing);
                    AddEffect(EffectKind.Text, player.X, player.Y - 86, player.X, player.Y - 86, 35, Color.OrangeRed, "HIT", player.Facing);
                }
            }
        }

        private void DamagePlayer(int damage)
        {
            if (player.ShieldTicks > 0) damage = Math.Max(1, damage / 2);
            player.Hp -= damage;
            player.InvincibleTicks = 45;
            if (screen == GameScreen.Dungeon)
            {
                dungeonHitCount++;
                comboCount = 0;
                comboTimer = 0;
            }
            if (player.Hp <= 0)
            {
                player.Hp = Math.Max(1, player.MaxHp / 2);
                player.Mp = Math.Max(0, player.MaxMp / 2);
                player.X = 120;
                player.Y = 300;
                player.VX = 0;
                player.VY = 0;
                AddLog("시스템 복구 지점으로 되돌아왔습니다.");
            }
        }

        private void UpdateCamera()
        {
            if (currentDungeon == null) return;
            int viewW = ClientSize.Width;
            cameraX = player.X - viewW * 0.42f;
            if (cameraX < 0) cameraX = 0;
            if (cameraX > currentDungeon.MapWidth - viewW) cameraX = Math.Max(0, currentDungeon.MapWidth - viewW);
        }

        private void CheckClearCondition()
        {
            bool allDead = true;
            for (int i = 0; i < monsters.Count; i++)
            {
                if (monsters[i].Hp > 0) allDead = false;
            }
            if (allDead)
            {
                stageClearTicks++;
                if (stageClearTicks == 1)
                {
                    AddLog("던전 클리어! 오른쪽 끝 포털로 이동하거나 ESC로 나갈 수 있습니다.");
                }
                if (player.X > currentDungeon.MapWidth - 130)
                {
                    FinishDungeon();
                }
            }
            else
            {
                stageClearTicks = 0;
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (screen == GameScreen.Title)
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space) screen = GameScreen.JobSelect;
                if (e.KeyCode == Keys.L) LoadGame();
                return;
            }
            if (screen == GameScreen.JobSelect)
            {
                if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Up) selectedJob = (selectedJob + 3) % 4;
                if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Down) selectedJob = (selectedJob + 1) % 4;
                if (e.KeyCode == Keys.D1 || e.KeyCode == Keys.NumPad1) selectedJob = 0;
                if (e.KeyCode == Keys.D2 || e.KeyCode == Keys.NumPad2) selectedJob = 1;
                if (e.KeyCode == Keys.D3 || e.KeyCode == Keys.NumPad3) selectedJob = 2;
                if (e.KeyCode == Keys.D4 || e.KeyCode == Keys.NumPad4) selectedJob = 3;
                if (e.KeyCode == Keys.Back && playerName.Length > 0) playerName = playerName.Substring(0, playerName.Length - 1);
                if (e.KeyCode == Keys.Enter) ConfirmJob();
                if (e.KeyCode == Keys.Escape) screen = GameScreen.Title;
                return;
            }
            if (screen == GameScreen.HeroIntro)
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space || e.KeyCode == Keys.E) AgreeHeroIntro();
                if (e.KeyCode == Keys.Escape) screen = GameScreen.Title;
                return;
            }
            if (screen == GameScreen.FileSelect)
            {
                if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Up) selectedFile = (selectedFile + GameData.Dungeons.Count - 1) % GameData.Dungeons.Count;
                if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Down) selectedFile = (selectedFile + 1) % GameData.Dungeons.Count;
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.E) EnterSelectedDungeon();
                if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.B) screen = GameScreen.ItemShop;
                if (e.KeyCode == Keys.T) TryJobChange();
                if (e.KeyCode == Keys.U) TryManualWeaponUpgrade();
                if (e.KeyCode == Keys.S) SaveGame();
                if (e.KeyCode == Keys.Escape) screen = GameScreen.Title;
                return;
            }
            if (screen == GameScreen.ItemShop)
            {
                if (e.KeyCode == Keys.D1 || e.KeyCode == Keys.NumPad1) BuyShopItem("hp");
                if (e.KeyCode == Keys.D2 || e.KeyCode == Keys.NumPad2) BuyShopItem("mp");
                if (e.KeyCode == Keys.D3 || e.KeyCode == Keys.NumPad3) BuyShopItem("patch");
                if (e.KeyCode == Keys.D4 || e.KeyCode == Keys.NumPad4) BuyShopItem("repair");
                if (e.KeyCode == Keys.Escape) screen = GameScreen.FileSelect;
                return;
            }
            if (screen == GameScreen.Dungeon)
            {
                if (e.KeyCode == Keys.Left) left = true;
                if (e.KeyCode == Keys.Right) right = true;
                if ((e.KeyCode == Keys.Space || e.KeyCode == Keys.Up) && !jumpHeld)
                {
                    jumpHeld = true;
                    TryJump();
                }
                if (e.KeyCode == Keys.Q) UseSkillSlot(0);
                if (e.KeyCode == Keys.W) UseSkillSlot(1);
                if (e.KeyCode == Keys.E) UseSkillSlot(2);
                if (e.KeyCode == Keys.R) UseSkillSlot(3);
                if (e.KeyCode == Keys.D) UseHpPotion();
                if (e.KeyCode == Keys.F) UseMpPotion();
                if (e.KeyCode == Keys.Escape) screen = GameScreen.FileSelect;
                return;
            }
            if (screen == GameScreen.Result)
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape) StartPostDungeonStory();
                return;
            }
            if (screen == GameScreen.StoryDialog)
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space || e.KeyCode == Keys.E) ContinueStoryDialog();
                if (e.KeyCode == Keys.Escape) ContinueStoryDialog();
                return;
            }
            if (screen == GameScreen.CompanionChoice)
            {
                if (e.KeyCode == Keys.D1 || e.KeyCode == Keys.NumPad1) ResolveCompanionChoice("take");
                if (e.KeyCode == Keys.D2 || e.KeyCode == Keys.NumPad2) ResolveCompanionChoice("leave");
                if (e.KeyCode == Keys.D3 || e.KeyCode == Keys.NumPad3) ResolveCompanionChoice("delete");
                if (e.KeyCode == Keys.Escape) ResolveCompanionChoice("leave");
                return;
            }
            if (screen == GameScreen.RewardChoice)
            {
                if (e.KeyCode == Keys.D1 || e.KeyCode == Keys.NumPad1 || e.KeyCode == Keys.U) ResolveRewardChoice(true);
                if (e.KeyCode == Keys.D2 || e.KeyCode == Keys.NumPad2 || e.KeyCode == Keys.H) ResolveRewardChoice(false);
                return;
            }
            if (screen == GameScreen.SuspectSelect)
            {
                if (e.KeyCode == Keys.D1 || e.KeyCode == Keys.NumPad1) ResolveSuspect(0);
                if (e.KeyCode == Keys.D2 || e.KeyCode == Keys.NumPad2) ResolveSuspect(1);
                if (e.KeyCode == Keys.D3 || e.KeyCode == Keys.NumPad3) ResolveSuspect(2);
                if (e.KeyCode == Keys.D4 || e.KeyCode == Keys.NumPad4) ResolveSuspect(3);
                if (e.KeyCode == Keys.D5 || e.KeyCode == Keys.NumPad5) ResolveSuspect(4);
                if (e.KeyCode == Keys.Escape) ReturnToTaechoVillage();
                return;
            }
            if (screen == GameScreen.Ending)
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space || e.KeyCode == Keys.Escape) CloseEndingScreen();
                return;
            }
            if (e.KeyCode == Keys.Escape) screen = GameScreen.FileSelect;
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left) left = false;
            if (e.KeyCode == Keys.Right) right = false;
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Up) jumpHeld = false;
        }

        private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (screen != GameScreen.JobSelect) return;
            if (char.IsLetterOrDigit(e.KeyChar) || e.KeyChar == '_' || (e.KeyChar >= '가' && e.KeyChar <= '힣'))
            {
                if (playerName.Length < 12) playerName += e.KeyChar;
            }
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            for (int i = buttons.Count - 1; i >= 0; i--)
            {
                if (buttons[i].Bounds.Contains(e.Location))
                {
                    HandleAction(buttons[i].Action);
                    break;
                }
            }
        }

        private void HandleAction(string action)
        {
            if (action == "new") screen = GameScreen.JobSelect;
            else if (action == "load") LoadGame();
            else if (action == "help") screen = GameScreen.Help;
            else if (action == "quit") Close();
            else if (action == "backTitle") screen = GameScreen.Title;
            else if (action == "confirmJob") ConfirmJob();
            else if (action == "introAgree") AgreeHeroIntro();
            else if (action == "introNo") RefuseHeroIntro();
            else if (action == "storyContinue") ContinueStoryDialog();
            else if (action == "companionTake") ResolveCompanionChoice("take");
            else if (action == "companionLeave") ResolveCompanionChoice("leave");
            else if (action == "companionDelete") ResolveCompanionChoice("delete");
            else if (action == "rewardUpgrade") ResolveRewardChoice(true);
            else if (action == "rewardHint") ResolveRewardChoice(false);
            else if (action.StartsWith("suspect")) { int n; if (int.TryParse(action.Substring(7), out n)) ResolveSuspect(n); }
            else if (action == "endingOk") CloseEndingScreen();
            else if (action == "shopRecycle") screen = GameScreen.ItemShop;
            else if (action == "buyHp") BuyShopItem("hp");
            else if (action == "buyMp") BuyShopItem("mp");
            else if (action == "buyPatch") BuyShopItem("patch");
            else if (action == "buyRepair") BuyShopItem("repair");
            else if (action == "desktopSearch") UseDesktopSearch();
            else if (action == "desktopUpdate") UseDesktopUpdate();
            else if (action == "desktopControl") TryManualWeaponUpgrade();
            else if (action == "desktopNotebook") ShowQuestMemo();
            else if (action == "desktopMyComputer") ShowSystemStatusToast();
            else if (action == "jobChange") TryJobChange();
            else if (action == "upgradeWeapon") TryManualWeaponUpgrade();
            else if (action == "taecho") ReturnToTaechoVillage();
            else if (action.StartsWith("job"))
            {
                int n;
                if (int.TryParse(action.Substring(3), out n)) selectedJob = n;
            }
            else if (action.StartsWith("file"))
            {
                int n;
                if (int.TryParse(action.Substring(4), out n))
                {
                    selectedFile = n;
                    EnterSelectedDungeon();
                }
            }
            else if (action == "save") SaveGame();
            else if (action == "resultOk") StartPostDungeonStory();
        }

        private void ConfirmJob()
        {
            if (string.IsNullOrWhiteSpace(playerName)) playerName = "디버그용사";
            player.Name = playerName;
            player.ApplyJob((JobType)selectedJob);
            introShakeTicks = 80;
            PlayStrongSound();
            AddLog(player.Profile.Name + " 선택 완료. 오류 창이 당신을 심사합니다.");
            screen = GameScreen.HeroIntro;
        }

        private void EnterSelectedDungeon()
        {
            DungeonInfo d = GameData.Dungeons[selectedFile];
            if (IsDungeonLocked(d))
            {
                AddLog("잠긴 파일입니다. " + GetDungeonLockReason(d));
                NpcMock(GetDungeonLockReason(d) + " 그래도 더블클릭한 용기는 인정합니다.");
                return;
            }
            currentDungeon = d;
            platforms = GameData.CreatePlatforms(d);
            monsters = GameData.CreateMonsters(d);
            effects.Clear();
            player.X = 120;
            player.Y = 320;
            player.VX = 0;
            player.VY = 0;
            cameraX = 0;
            stageClearTicks = 0;
            comboCount = 0;
            comboTimer = 0;
            maxCombo = 0;
            dungeonStartTick = tick;
            dungeonHitCount = 0;
            dungeonKillCount = 0;
            hiddenFolderFound = false;
            lastDropText = "없음";
            screen = GameScreen.Dungeon;
            AddLog(d.FileName + " 실행: 던전에 입장했습니다.");
            NpcMock(GameData.GetDungeonNpcName(d.Type) + ": " + GameData.GetDungeonNpcLine(d.Type));
        }



        private bool IsDungeonLocked(DungeonInfo d)
        {
            if (d.Type == DungeonType.UserCoreTrueFault && !trueFinalUnlocked) return true;
            return player.PatchShards < d.RequiredPatch;
        }

        private string GetDungeonLockReason(DungeonInfo d)
        {
            if (d.Type == DungeonType.UserCoreTrueFault && !trueFinalUnlocked)
                return "1차 범인 추리 성공 후 User_Action_Log.sys를 복구해야 합니다.";
            return "패치 조각 " + d.RequiredPatch + "개 필요";
        }

        private void StartPostDungeonStory()
        {
            if (lastClearedDungeonType == DungeonType.UserCoreTrueFault)
            {
                ShowEnding(true);
                return;
            }
            PrepareStoryDialog(lastClearedDungeonType);
            PlayStrongSound();
            screen = GameScreen.StoryDialog;
        }

        private void PrepareStoryDialog(DungeonType type)
        {
            if (type == DungeonType.FileExplorerForest)
            {
                storyTitle = "[파일 검색 실패] 파일이 어디 갔지?";
                storyBody = "Final_Project_REAL_LAST.exe의 흔적을 찾았습니다.\n\n" +
                            "오류 메시지: '파일은 사라진 게 아닙니다. 사용자의 폴더 정리 능력에서 삭제되었을 뿐입니다.'\n\n" +
                            "NPC: " + GameData.GetDungeonNpcName(type) + "\n" + GameData.GetDungeonNpcLine(type);
            }
            else if (type == DungeonType.PopupErrorZone)
            {
                storyTitle = "[예외 발생] 오류창 왜 뜨지?";
                storyBody = "오류창들이 드디어 말을 하기 시작했습니다.\n\n" +
                            "오류 메시지: '나를 읽지도 않고 X만 누른 사람은 누구인가?'\n\n" +
                            "NPC: " + GameData.GetDungeonNpcName(type) + "\n" + GameData.GetDungeonNpcLine(type);
            }
            else if (type == DungeonType.RecycleBinDungeon)
            {
                storyTitle = "[삭제 경고] 휴지통에 버린 파일 복구해야 하나?";
                storyBody = "삭제된 파일들이 감정을 되찾았습니다.\n\n" +
                            "오류 메시지: '정말 삭제하시겠습니까?'\n'정말이었네?'\n\n" +
                            "NPC: " + GameData.GetDungeonNpcName(type) + "\n" + GameData.GetDungeonNpcLine(type);
            }
            else if (type == DungeonType.UpdateLab)
            {
                storyTitle = "[업데이트 중] 99%에서 멈췄습니다";
                storyBody = "업데이트 연구소에서 멈춘 진행률과 조작된 패치 로그를 발견했습니다.\n\n" +
                            "오류 메시지: '기다리면 해결될 수도 있습니다. 하지만 언제인지는 모릅니다.'\n\n" +
                            "NPC: " + GameData.GetDungeonNpcName(type) + "\n" + GameData.GetDungeonNpcLine(type);
            }
            else if (type == DungeonType.ControlPanelCastle)
            {
                storyTitle = "[설정 충돌] 제어판이 삐졌습니다";
                storyBody = "설정 패널 안에서 오류창 여왕 Exception의 로그를 발견했습니다.\n\n" +
                            "오류 메시지: '설정을 바꿨으면 책임도 같이 바꾸세요.'\n\n" +
                            "NPC: " + GameData.GetDungeonNpcName(type) + "\n" + GameData.GetDungeonNpcLine(type);
            }
            else if (type == DungeonType.System32Forbidden)
            {
                storyTitle = "[접근 거부] System32는 건드리면 안 된다던데?";
                storyBody = "System32의 커널 수호자가 정상 파일과 감염 파일을 구분하라고 합니다.\n\n" +
                            "오류 메시지: '관리자 권한보다 필요한 것은 근거입니다.'\n\n" +
                            "NPC: " + GameData.GetDungeonNpcName(type) + "\n" + GameData.GetDungeonNpcLine(type);
            }
            else if (type == DungeonType.BlueScreenTower)
            {
                storyTitle = "[BLUE SCREEN] 블루스크린 뜨면 끝난 건가?";
                storyBody = "Blue Screen Dragon이 쓰러졌지만 화면 한쪽에 숨겨진 로그가 남았습니다.\n\n" +
                            "오류 메시지: 'STOP: 0xUSER_FAULT_TRACE'\n\n" +
                            "NPC: " + GameData.GetDungeonNpcName(type) + "\n이제 범인을 찍어야 합니다. 틀리면 태초마을입니다.";
            }
            else
            {
                storyTitle = "[파일 내부 로그] " + GameData.GetDungeonNpcName(type);
                storyBody = "던전 파일 안에서 새로운 감정 로그가 출력되었습니다.\n\n" +
                            "오류 메시지: '이 파일도 그냥 아이콘이 아니라 사연 있는 던전입니다.'\n\n" +
                            "NPC: " + GameData.GetDungeonNpcName(type) + "\n" + GameData.GetDungeonNpcLine(type);
            }
        }

        private void ContinueStoryDialog()
        {
            screen = GameScreen.CompanionChoice;
        }

        private string GetCompanionName(DungeonType type)
        {
            if (type == DungeonType.FileExplorerForest) return "Searchy.exe";
            if (type == DungeonType.PopupErrorZone) return "ExceptionQueen.dll";
            if (type == DungeonType.RecycleBinDungeon) return "Binny.bak";
            if (type == DungeonType.UpdateLab) return "PatchMan.sys";
            if (type == DungeonType.ControlPanelCastle) return "PanelButler.cpl";
            if (type == DungeonType.TempCacheCave) return "TempCleaner.tmp";
            if (type == DungeonType.NetworkPort) return "CaptainPing.net";
            if (type == DungeonType.RegistryHive) return "RegiKey.reg";
            if (type == DungeonType.DriverVault) return "DriverK.drv";
            if (type == DungeonType.System32Forbidden) return "KernelGuard.sys";
            if (type == DungeonType.BlueScreenTower) return "BlueScreenWisp.log";
            return "MirrorUser.tmp";
        }

        private void ResolveCompanionChoice(string choice)
        {
            string name = GetCompanionName(lastClearedDungeonType);
            if (choice == "take")
            {
                companionCount++;
                companionHistory += name + " 동행\n";
                player.Luck += 1;
                AddLog(name + "을(를) 데리고 갑니다. 행운 +1");
                NpcMock("감정 있는 파일을 데려갑니다. 이제 폴더 정리 난이도도 같이 상승합니다.");
            }
            else if (choice == "delete")
            {
                deletedCount++;
                companionHistory += name + " 삭제\n";
                player.PatchShards += 2;
                AddLog(name + "을(를) 삭제했습니다. 패치 조각 +2");
                NpcMock("삭제 버튼을 또 눌렀군요. 휴지통이 당신 얼굴을 외웠습니다.");
            }
            else
            {
                abandonedCount++;
                companionHistory += name + " 두고 감\n";
                AddLog(name + "을(를) 두고 갑니다. 나중에 삐져도 본인 책임입니다.");
                NpcMock("두고 가기 선택. 합리적인 척하는 회피입니다.");
            }
            screen = GameScreen.RewardChoice;
        }

        private string GetHintText(DungeonType type)
        {
            if (type == DungeonType.FileExplorerForest) return "힌트 1: 범인은 파일 위치와 바로가기 구조를 잘 아는 자다.";
            if (type == DungeonType.PopupErrorZone) return "힌트 2: 범인은 오류를 만든 게 아니라 오류창을 무시하게 만들었다.";
            if (type == DungeonType.RecycleBinDungeon) return "힌트 3: 범인은 버려진 기록과 복구 로그를 두려워한다.";
            if (type == DungeonType.UpdateLab) return "힌트 4: 범인은 패치를 일부러 늦췄다.";
            if (type == DungeonType.ControlPanelCastle) return "힌트 5: 범인은 설정값을 바꿀 권한이 있었다.";
            if (type == DungeonType.TempCacheCave) return "힌트 6: 범인은 임시 파일을 증거 은닉에 사용했다.";
            if (type == DungeonType.NetworkPort) return "힌트 7: 범인은 외부 패킷처럼 보이게 흔적을 위장했다.";
            if (type == DungeonType.RegistryHive) return "힌트 8: 범인은 레지스트리 키를 바꿔 실행 경로를 숨겼다.";
            if (type == DungeonType.DriverVault) return "힌트 9: 범인은 드라이버 충돌을 핑계로 로그를 지웠다.";
            if (type == DungeonType.System32Forbidden) return "힌트 10: 사건 당시 시스템 권한을 가진 존재는 패치 관리자였다.";
            if (type == DungeonType.BlueScreenTower) return "힌트 11: Blue Screen Dragon은 범인이 아니라 누군가 미룬 패치의 결과다.";
            return "힌트 ?: User_Action_Log에는 더 불편한 진실이 남아 있다.";
        }

        private void ResolveRewardChoice(bool upgrade)
        {
            if (upgrade)
            {
                int chance = Math.Max(42, 78 - player.WeaponLevel * 3 + player.Luck / 2);
                if (random.Next(0, 100) < chance)
                {
                    player.ApplyWeaponUpgrade();
                    PlayStrongSound();
                    AddLog("스토리 보상: 무기 강화 성공 → " + player.WeaponName);
                    NpcMock("오, 성공했습니다. 제 농락 대사가 또 실직했습니다.");
                }
                else
                {
                    player.FailedUpgrades++;
                    NpcMock("스토리 보상 강화 실패! 진실 대신 힘을 골랐는데 힘도 안 왔습니다.");
                    if (player.FailedUpgrades % 3 == 0)
                    {
                        player.PatchShards += 3;
                        AddLog("불쌍 보정: 패치 조각 +3");
                    }
                }
            }
            else
            {
                string hint = GetHintText(lastClearedDungeonType);
                clueCount++;
                hintHistory += hint + "\n";
                AddLog("범인 힌트 획득: " + hint);
                AddEffect(EffectKind.Text, player.X, player.Y - 130, player.X, player.Y - 130, 70, Color.Gold, "CLUE +1", player.Facing);
                if (clueCount >= 5) UnlockAchievement("진실 추적자");
            }

            if (lastClearedDungeonType == DungeonType.BlueScreenTower && !normalEndingSeen)
            {
                screen = GameScreen.SuspectSelect;
                PlayStrongSound();
            }
            else
            {
                ReturnToTaechoVillage();
            }
        }

        private void ResolveSuspect(int index)
        {
            string[] suspects = new string[] { "NPC 404호", "오류창 여왕 Exception", "Update 아저씨", "Kernel 수호자", "휴지통 소녀 Bin" };
            if (index == 2)
            {
                normalEndingSeen = true;
                trueFinalUnlocked = true;
                UnlockAchievement("가짜 범인 추리 성공");
                ShowEnding(false);
            }
            else
            {
                suspectFailCount++;
                AddLog("오답 추리: " + suspects[Math.Max(0, Math.Min(index, suspects.Length - 1))] + " 지목 실패.");
                NpcMock("추리는 자신감이 아니라 근거로 하는 겁니다. 태초마을 리셋~");
                ReturnToTaechoVillage();
            }
        }

        private void ShowEnding(bool trueEnding)
        {
            if (trueEnding)
            {
                trueEndingSeen = true;
                UnlockAchievement("진짜 디버그 완료");
                endingTitle = "[진엔딩] 완전한 디버그";
                endingBody = "UserCore_TrueFault.exe가 정화되었습니다.\n\n" +
                             "[시스템 복구 완료]\n오류의 원인을 확인했습니다.\n\n" +
                             "원인: 사용자 본인\n조치: 파일 정리, 오류 메시지 확인, 업데이트 수행, 중요 파일 백업\n\n" +
                             "NPC 404호: 결국 최고의 디버깅은 남을 의심하는 게 아니라 자기 자신을 점검하는 거였네요.\n\n" +
                             "생성된 파일: Final_Project_REAL_LAST_REAL_FINAL_FIXED.exe";
            }
            else
            {
                endingTitle = "[1차 엔딩] 범인 체포 완료?";
                endingBody = "Update 아저씨가 패치를 일부러 늦춘 가짜 범인으로 밝혀졌습니다.\n\n" +
                             "하지만 복구된 User_Action_Log.sys가 더 불편한 로그를 출력합니다.\n\n" +
                             "- 오류창 무시\n- 업데이트 미룸\n- 파일명 FINAL 남발\n- 휴지통 정리 실패\n- 수상한 파일 실행\n\n" +
                             "최초 원인 제공자: USER\n\n" +
                             "NPC 404호: 축하합니다. 범인을 잡았네요. 그런데 찐 범인은 당신이었어요.\n\n" +
                             "찐 최종 던전 UserCore_TrueFault.exe가 바탕화면에 해금되었습니다.";
            }
            PlayStrongSound();
            screen = GameScreen.Ending;
        }

        private void CloseEndingScreen()
        {
            if (trueEndingSeen)
            {
                selectedFile = 0;
                screen = GameScreen.FileSelect;
            }
            else
            {
                int idx = GameData.Dungeons.FindIndex(delegate(DungeonInfo d) { return d.Type == DungeonType.UserCoreTrueFault; });
                if (idx >= 0) selectedFile = idx;
                screen = GameScreen.FileSelect;
            }
        }


        private void BuyShopItem(string item)
        {
            int price = 0;
            string name = "";
            if (item == "hp") { price = 120; name = "HP 패치 포션"; }
            else if (item == "mp") { price = 140; name = "MP 복구 포션"; }
            else if (item == "patch") { price = 220; name = "PatchShard.pkg x5"; }
            else { price = 260; name = "무기 수리 키트"; }

            if (player.Gold < price)
            {
                NpcMock("휴지통 상점: 돈이 부족합니다. 버릴 건 많은데 살 돈은 없군요.");
                PlayShortBeep();
                return;
            }
            player.Gold -= price;
            if (item == "hp") player.Potion += 2;
            else if (item == "mp") player.MpPotion += 2;
            else if (item == "patch") player.PatchShards += 5;
            else
            {
                player.FailedUpgrades = Math.Max(0, player.FailedUpgrades - 2);
                player.Hp = Math.Min(player.MaxHp, player.Hp + 40);
            }
            AddLog("휴지통 상점 구매: " + name + " / Gold -" + price);
            NpcMock("휴지통 상점: 버려진 아이템도 손님 앞에서는 신상품입니다.");
            PlayShortBeep();
        }

        private void UseDesktopSearch()
        {
            clueCount++;
            player.Luck += 1;
            hintHistory += "검색창 단서: 파일명보다 중요한 건 마지막으로 저장한 위치다.\n";
            AddLog("Windows 검색 사용: 범인 힌트 +1, 행운 +1");
            NpcMock("검색창: 방금 검색한 건 파일이 아니라 본인의 기억력입니다.");
        }

        private void UseDesktopUpdate()
        {
            if (player.Gold < 180)
            {
                NpcMock("Windows Update: Gold 180이 필요합니다. 업데이트도 공짜로는 인내심만 줍니다.");
                return;
            }
            player.Gold -= 180;
            player.PatchShards += 4;
            AddLog("Windows Update 실행: 패치 조각 +4 / Gold -180");
            NpcMock("업데이트 완료! 99%에서 멈추지 않은 것만으로도 기적입니다.");
        }

        private void ShowQuestMemo()
        {
            string msg = "메모장: 현재 힌트 " + clueCount + "개 / 동행 파일 " + companionCount + "개 / 삭제 파일 " + deletedCount + "개";
            AddLog(msg);
            NpcMock("메모장: 중요한 건 적었는데 어디 저장했는지는 또 까먹겠죠?");
        }

        private void ShowSystemStatusToast()
        {
            AddLog("내 컴퓨터: Lv." + player.Level + " / " + player.DisplayJobName + " / " + player.WeaponName);
            NpcMock("내 컴퓨터: 시스템 상태는 정상입니다. 사용자 상태는 보류입니다.");
        }

        private void TryJump()
        {
            if (player.OnGround)
            {
                player.VY = -12.2f - Math.Min(2.5f, player.Speed * 0.08f);
                player.OnGround = false;
                AddEffect(EffectKind.HitSpark, player.X, player.Y - 5, player.X, player.Y - 5, 12, Color.White, "", player.Facing);
            }
        }

        private int GetSkillCooldown(int slot)
        {
            if (slot == 0) return attackCooldown;
            if (slot == 1) return skillCooldown;
            if (slot == 2) return skill2Cooldown;
            return ultimateCooldown;
        }

        private void SetSkillCooldown(int slot, int value)
        {
            if (slot == 0) attackCooldown = value;
            else if (slot == 1) skillCooldown = value;
            else if (slot == 2) skill2Cooldown = value;
            else ultimateCooldown = value;
        }

        private void UseSkillSlot(int slot)
        {
            if (GetSkillCooldown(slot) > 0)
            {
                AddEffect(EffectKind.Text, player.X, player.Y - 96, player.X, player.Y - 96, 24, Color.LightGray, "쿨타임", player.Facing);
                return;
            }
            int cost = 0;
            int cooldown = 18;
            if (slot == 1) { cost = 14; cooldown = 44; }
            if (slot == 2) { cost = 22; cooldown = 64; }
            if (slot == 3) { cost = 38; cooldown = 130; }
            if (player.Mp < cost)
            {
                AddLog("MP가 부족합니다.");
                AddEffect(EffectKind.Text, player.X, player.Y - 92, player.X, player.Y - 92, 40, Color.LightSkyBlue, "MP 부족", player.Facing);
                return;
            }
            player.Mp -= cost;
            SetSkillCooldown(slot, cooldown);
            if (slot == 0) CastBasicAttack();
            else if (slot == 1) CastClassSkill();
            else if (slot == 2) CastSecondSkill();
            else CastUltimateSkill();
        }

        private void CastBasicAttack()
        {
            int dir = player.Facing;
            float startX = player.X + dir * 28;
            float targetX = player.X + dir * 185;
            float y = player.Y - 38;
            Color c = player.Profile.MainColor;
            AddEffect(EffectKind.Projectile, startX, y, targetX, y - 12, 18, c, "Q", dir);
            AddEffect(EffectKind.Projectile, startX + dir * 10, y - 6, targetX - dir * 18, y + 6, 15, Color.White, "", dir);
            AddEffect(EffectKind.Slash, targetX, y, targetX, y, 22, c, "", dir);
            AddEffect(EffectKind.HitSpark, targetX, y - 4, targetX, y - 4, 18, Color.White, "", dir);
            RectangleF hit = new RectangleF(dir > 0 ? player.X : player.X - 205, player.Y - 82, 205, 92);
            HitMonsters(hit, player.Attack + player.Level * 3 + random.Next(4, 12), false);
        }

        private void CastClassSkill()
        {
            int dir = player.Facing;
            Color c = player.Profile.MainColor;
            if (player.Job == JobType.VaccineMage)
            {
                int heal = Math.Min(player.MaxHp - player.Hp, 24 + player.Level * 3);
                player.Hp += heal;
                AddEffect(EffectKind.Heal, player.X, player.Y - 34, player.X, player.Y - 34, 36, c, "W", dir);
                AddEffect(EffectKind.SkillBurst, player.X, player.Y - 42, player.X, player.Y - 42, 26, Color.LimeGreen, "", dir);
                AddEffect(EffectKind.Projectile, player.X + dir * 28, player.Y - 45, player.X + dir * 245, player.Y - 50, 28, c, "W", dir);
                RectangleF hit = new RectangleF(dir > 0 ? player.X : player.X - 260, player.Y - 95, 260, 120);
                HitMonsters(hit, player.Attack + player.Level * 5 + 28, true);
                if (heal > 0) AddEffect(EffectKind.Text, player.X, player.Y - 100, player.X, player.Y - 100, 42, Color.LimeGreen, "HP +" + heal, dir);
            }
            else if (player.Job == JobType.FirewallKnight)
            {
                player.ShieldTicks = 180;
                AddEffect(EffectKind.Guard, player.X, player.Y - 36, player.X, player.Y - 36, 48, Color.DeepSkyBlue, "W", dir);
                AddEffect(EffectKind.SkillBurst, player.X + dir * 110, player.Y - 44, player.X + dir * 110, player.Y - 44, 24, c, "", dir);
                AddEffect(EffectKind.Projectile, player.X + dir * 34, player.Y - 42, player.X + dir * 260, player.Y - 42, 30, c, "W", dir);
                RectangleF hit = new RectangleF(dir > 0 ? player.X : player.X - 285, player.Y - 105, 285, 125);
                HitMonsters(hit, player.Attack + player.Defense + player.Level * 4, true);
            }
            else if (player.Job == JobType.FileExplorer)
            {
                AddEffect(EffectKind.ScanLine, player.X + dir * 180, player.Y - 44, player.X + dir * 180, player.Y - 44, 38, c, "W", dir);
                AddEffect(EffectKind.ScanLine, player.X + dir * 260, player.Y - 58, player.X + dir * 260, player.Y - 58, 28, Color.Gold, "", dir);
                AddEffect(EffectKind.Projectile, player.X + dir * 30, player.Y - 48, player.X + dir * 320, player.Y - 48, 18, Color.Gold, "W", dir);
                RectangleF hit = new RectangleF(dir > 0 ? player.X : player.X - 330, player.Y - 105, 330, 125);
                HitMonsters(hit, player.Attack + player.Speed + player.Luck / 2 + player.Level * 3, true);
            }
            else
            {
                AddEffect(EffectKind.Projectile, player.X + dir * 32, player.Y - 45, player.X + dir * 310, player.Y - 55, 24, c, "W", dir);
                AddEffect(EffectKind.Projectile, player.X + dir * 20, player.Y - 56, player.X + dir * 285, player.Y - 28, 18, Color.White, "", dir);
                AddEffect(EffectKind.SkillBurst, player.X + dir * 300, player.Y - 55, player.X + dir * 300, player.Y - 55, 36, c, "W", dir);
                RectangleF hit = new RectangleF(dir > 0 ? player.X : player.X - 330, player.Y - 125, 330, 150);
                HitMonsters(hit, player.Attack * 2 + player.Level * 6, true);
            }
        }

        private void CastSecondSkill()
        {
            int dir = player.Facing;
            Color c = player.Profile.MainColor;
            if (player.Job == JobType.DebugWarrior)
            {
                for (int i = 0; i < 3; i++)
                {
                    float y = player.Y - 58 + i * 18;
                    AddEffect(EffectKind.Projectile, player.X + dir * 30, y, player.X + dir * (260 + i * 55), y - 6, 28, Color.FromArgb(70, 220, 255), "E", dir);
                    AddEffect(EffectKind.Slash, player.X + dir * (245 + i * 52), y, player.X + dir * (245 + i * 52), y, 20, Color.White, "", dir);
                }
                RectangleF hit = new RectangleF(dir > 0 ? player.X : player.X - 460, player.Y - 118, 460, 150);
                HitMonsters(hit, player.Attack * 2 + player.Level * 8 + 25, true);
            }
            else if (player.Job == JobType.VaccineMage)
            {
                int heal = Math.Min(player.MaxHp - player.Hp, 42 + player.Level * 5);
                player.Hp += heal;
                player.InvincibleTicks = Math.Max(player.InvincibleTicks, 35);
                AddEffect(EffectKind.Heal, player.X, player.Y - 40, player.X, player.Y - 40, 48, Color.LimeGreen, "E", dir);
                AddEffect(EffectKind.SkillBurst, player.X, player.Y - 42, player.X, player.Y - 42, 34, Color.LimeGreen, "", dir);
                AddEffect(EffectKind.SkillBurst, player.X + dir * 230, player.Y - 56, player.X + dir * 230, player.Y - 56, 42, c, "E", dir);
                RectangleF hit = new RectangleF(dir > 0 ? player.X : player.X - 300, player.Y - 120, 300, 150);
                HitMonsters(hit, player.Attack + player.Level * 7 + 42, true);
                if (heal > 0) AddEffect(EffectKind.Text, player.X, player.Y - 110, player.X, player.Y - 110, 46, Color.LimeGreen, "HP +" + heal, dir);
            }
            else if (player.Job == JobType.FirewallKnight)
            {
                player.ShieldTicks = 260;
                AddEffect(EffectKind.Guard, player.X, player.Y - 38, player.X, player.Y - 38, 58, c, "E", dir);
                AddEffect(EffectKind.SkillBurst, player.X, player.Y - 44, player.X, player.Y - 44, 30, Color.DeepSkyBlue, "", dir);
                AddEffect(EffectKind.SkillBurst, player.X + dir * 170, player.Y - 54, player.X + dir * 170, player.Y - 54, 42, c, "E", dir);
                RectangleF hit = new RectangleF(dir > 0 ? player.X - 20 : player.X - 330, player.Y - 125, 350, 160);
                HitMonsters(hit, player.Attack + player.Defense * 2 + player.Level * 5, true);
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    AddEffect(EffectKind.ScanLine, player.X + dir * (95 + i * 70), player.Y - 64 + i * 12, player.X + dir * (95 + i * 70), player.Y - 64 + i * 12, 38, Color.Gold, "E", dir);
                    AddEffect(EffectKind.Projectile, player.X + dir * 20, player.Y - 48, player.X + dir * (110 + i * 68), player.Y - 58 + i * 10, 18, Color.Gold, "", dir);
                }
                RectangleF hit = new RectangleF(dir > 0 ? player.X : player.X - 430, player.Y - 120, 430, 160);
                HitMonsters(hit, player.Attack + player.Speed * 2 + player.Luck + player.Level * 5, true);
            }
        }

        private void CastUltimateSkill()
        {
            int dir = player.Facing;
            Color c = player.Profile.MainColor;
            AddEffect(EffectKind.SkillBurst, player.X, player.Y - 60, player.X, player.Y - 60, 54, c, "R", dir);
            AddEffect(EffectKind.Guard, player.X, player.Y - 46, player.X, player.Y - 46, 32, Color.White, "", dir);
            for (int i = 0; i < 5; i++)
            {
                float x = player.X + dir * (80 + i * 95);
                AddEffect(EffectKind.Projectile, player.X + dir * 24, player.Y - 50 - i * 8, x, player.Y - 58 + (i % 2) * 18, 34 + i * 3, c, "R", dir);
                AddEffect(EffectKind.Projectile, player.X + dir * 12, player.Y - 44 + i * 4, x - dir * 18, player.Y - 48 + (i % 2) * 18, 24 + i * 2, Color.White, "", dir);
                AddEffect(EffectKind.Slash, x, player.Y - 58 + (i % 2) * 18, x, player.Y - 58, 38, Color.White, "", dir);
                AddEffect(EffectKind.HitSpark, x, player.Y - 56 + (i % 2) * 18, x, player.Y - 56 + (i % 2) * 18, 22, Color.White, "", dir);
            }
            RectangleF hit = new RectangleF(dir > 0 ? player.X - 60 : player.X - 620, player.Y - 160, 620, 220);
            int damage = player.Attack * 3 + player.Level * 14 + player.Speed + player.Defense + 55;
            HitMonsters(hit, damage, true);
            AddLog("R 시스템 오버라이드 발동!");
        }

        private void TryAttack()
        {
            UseSkillSlot(0);
        }

        private void TrySkill()
        {
            UseSkillSlot(1);
        }

        private void HitMonsters(RectangleF hit, int damage, bool skill)
        {
            int hitCount = 0;
            for (int i = 0; i < monsters.Count; i++)
            {
                Monster m = monsters[i];
                if (m.Hp <= 0) continue;
                if (hit.IntersectsWith(m.Bounds))
                {
                    int comboBonus = comboCount >= 5 ? Math.Min(40, comboCount * 2) : 0;
                    int finalDamage = Math.Max(1, damage + random.Next(-6, 8) + comboBonus);
                    m.Hp -= finalDamage;
                    m.HitFlash = 18;
                    m.X += player.Facing * (skill ? 28 : 16);
                    hitCount++;
                    RegisterCombo(m.X, m.Y - 92, finalDamage);
                    AddEffect(EffectKind.HitSpark, m.X, m.Y - 48, m.X, m.Y - 48, 22, skill ? Color.Cyan : Color.Orange, "", player.Facing);
                    AddEffect(EffectKind.Text, m.X, m.Y - 92, m.X, m.Y - 92, 38, Color.White, finalDamage.ToString(), player.Facing);
                    if (comboCount >= 5)
                    {
                        AddEffect(EffectKind.Text, m.X, m.Y - 118, m.X, m.Y - 118, 30, Color.FromArgb(80, 220, 255), "+COMBO DMG", player.Facing);
                    }
                    if (m.Hp <= 0)
                    {
                        dungeonKillCount++;
                        player.Gold += m.Gold;
                        player.PatchShards += m.IsBoss ? 18 : 2 + random.Next(0, 3);
                        bool levelUp = player.AddExp(m.Exp);
                        AddEffect(EffectKind.Text, m.X, m.Y - 115, m.X, m.Y - 115, 52, Color.Gold, "CLEAR", player.Facing);
                        TryDropItem(m);
                        if (dungeonKillCount == 1) UnlockAchievement("첫 디버깅");
                        if (m.IsBoss) UnlockAchievement("블루스크린 파괴자");
                        if (levelUp)
                        {
                            AddLog("레벨 업! Lv." + player.Level + "  직업 성장치가 적용되었습니다.");
                            HandleLevelUpWeaponGrowth(m.X, m.Y);
                        }
                    }
                }
            }
            if (hitCount == 0)
            {
                AddEffect(EffectKind.Text, player.X + player.Facing * 120, player.Y - 82, player.X, player.Y, 25, Color.LightGray, "MISS", player.Facing);
            }
        }

        private void RegisterCombo(float x, float y, int damage)
        {
            comboCount++;
            comboTimer = 95;
            if (comboCount > maxCombo) maxCombo = comboCount;
            if (comboCount >= 2)
            {
                Color comboColor = comboCount >= 10 ? Color.Gold : Color.FromArgb(80, 220, 255);
                AddEffect(EffectKind.Text, x, y - 28, x, y - 28, 42, comboColor, comboCount + " COMBO", player.Facing);
            }
            if (comboCount == 10) UnlockAchievement("콤보 장인");
        }

        private void TryDropItem(Monster monster)
        {
            int dropChance = monster.IsBoss ? 100 : 16 + Math.Min(30, comboCount * 2) + player.Luck / 2;
            if (random.Next(0, 100) >= dropChance) return;
            int roll = random.Next(0, 100);
            string itemName;
            Color color;
            if (monster.IsBoss || roll >= 92)
            {
                itemName = "전설 BlueScreenCore.bsod";
                player.Attack += 3;
                player.Defense += 2;
                player.PatchShards += 10;
                color = Color.FromArgb(100, 190, 255);
            }
            else if (roll >= 76)
            {
                itemName = "희귀 DebugSword.exe";
                player.Attack += 2;
                color = Color.FromArgb(80, 220, 255);
            }
            else if (roll >= 58)
            {
                itemName = "FirewallShield.dll";
                player.Defense += 2;
                color = Color.FromArgb(90, 140, 255);
            }
            else if (roll >= 35)
            {
                itemName = "MemoryBoost.sys";
                player.MaxMp += 5;
                player.Mp = Math.Min(player.MaxMp, player.Mp + 5);
                color = Color.FromArgb(120, 200, 255);
            }
            else if (roll >= 15)
            {
                itemName = "VaccinePotion.dat";
                player.Potion++;
                color = Color.LimeGreen;
            }
            else
            {
                itemName = "PatchShard.pkg x5";
                player.PatchShards += 5;
                color = Color.Gold;
            }
            lastDropText = itemName;
            AddLog("아이템 드롭: " + itemName);
            AddEffect(EffectKind.Text, monster.X, monster.Y - 138, monster.X, monster.Y - 138, 64, color, "DROP " + itemName, player.Facing);
            if (itemName.StartsWith("희귀") || itemName.StartsWith("전설")) UnlockAchievement("희귀 파일 수집가");
        }

        private void CheckHiddenFolder()
        {
            if (currentDungeon == null || hiddenFolderFound) return;
            float hx = GetHiddenFolderX();
            float hy = GetHiddenFolderY();
            if (Math.Abs(player.X - hx) < 72 && Math.Abs(player.Y - hy) < 130)
            {
                hiddenFolderFound = true;
                int bonusGold = 250 + currentDungeon.RecommendedLevel * 30;
                int bonusPatch = 8 + currentDungeon.RecommendedLevel / 2;
                player.Gold += bonusGold;
                player.PatchShards += bonusPatch;
                player.Potion++;
                lastDropText = "SecretFolder.zip 보상";
                AddLog("숨겨진 폴더 발견! Gold +" + bonusGold + ", 패치 +" + bonusPatch);
                AddEffect(EffectKind.SkillBurst, hx, hy - 42, hx, hy - 42, 55, Color.Gold, "SECRET", player.Facing);
                AddEffect(EffectKind.Text, hx, hy - 112, hx, hy - 112, 70, Color.Gold, "HiddenFolder 발견!", player.Facing);
                UnlockAchievement("숨겨진 폴더 탐색자");
            }
        }

        private float GetHiddenFolderX()
        {
            if (currentDungeon == null) return 900;
            return Math.Max(620, Math.Min(currentDungeon.MapWidth - 460, currentDungeon.MapWidth * 0.58f + ((int)currentDungeon.Type - 2) * 70));
        }

        private float GetHiddenFolderY()
        {
            return 570f;
        }

        private void TryBossPattern(Monster boss, float dist)
        {
            if (boss.AttackCooldown > 0 || Math.Abs(dist) > 650) return;
            boss.AttackCooldown = 92;
            int dir = boss.X > player.X ? -1 : 1;
            int pattern = random.Next(0, 4);
            if (pattern == 0)
            {
                AddLog("보스 패턴: BSOD Breath 준비!");
                AddEffect(EffectKind.Projectile, boss.X - dir * 60, boss.Y - 92, player.X, player.Y - 50, 46, Color.FromArgb(90, 180, 255), "BSOD", dir);
                if (Math.Abs(player.X - boss.X) < 520 && Math.Abs(player.Y - boss.Y) < 190) DamagePlayer(Math.Max(12, boss.Attack + 8 - player.Defense / 2));
            }
            else if (pattern == 1)
            {
                AddLog("보스 패턴: Error Rain");
                for (int i = 0; i < 5; i++)
                {
                    float x = player.X - 180 + i * 90;
                    AddEffect(EffectKind.Projectile, x, 80, x, player.Y - 45, 52, Color.Cyan, "ERR", dir);
                }
                if (random.Next(0, 100) < 70) DamagePlayer(Math.Max(8, boss.Attack - player.Defense / 2));
            }
            else if (pattern == 2)
            {
                AddLog("보스 패턴: Forced Reboot");
                AddEffect(EffectKind.SkillBurst, boss.X, boss.Y - 68, boss.X, boss.Y - 68, 48, Color.White, "REBOOT", dir);
                player.VX = player.X < boss.X ? -10f : 10f;
                player.VY = -5f;
                DamagePlayer(Math.Max(8, boss.Attack - 5 - player.Defense / 2));
            }
            else
            {
                AddLog("보스 패턴: Memory Crash");
                AddEffect(EffectKind.ScanLine, player.X, player.Y - 40, player.X, player.Y - 40, 52, Color.MediumPurple, "SLOW", dir);
                player.VX *= 0.2f;
                DamagePlayer(Math.Max(6, boss.Attack - 10 - player.Defense / 2));
            }
        }

        private void UnlockAchievement(string title)
        {
            if (achievements.Contains(title)) return;
            achievements.Add(title);
            AddLog("업적 달성: " + title);
            AddEffect(EffectKind.Text, player.X, player.Y - 135, player.X, player.Y - 135, 82, Color.Gold, "업적: " + title, player.Facing);
        }

        private void UseHpPotion()
        {
            if (itemCooldown > 0) return;
            if (player.Potion <= 0)
            {
                AddLog("HP 패치 포션이 없습니다.");
                return;
            }
            int heal = Math.Min(player.MaxHp - player.Hp, 70 + player.Level * 8);
            if (heal <= 0)
            {
                AddEffect(EffectKind.Text, player.X, player.Y - 92, player.X, player.Y - 92, 28, Color.LightGray, "HP FULL", player.Facing);
                return;
            }
            itemCooldown = 24;
            player.Potion--;
            player.Hp += heal;
            AddEffect(EffectKind.Heal, player.X, player.Y - 35, player.X, player.Y - 35, 36, Color.LimeGreen, "D", player.Facing);
            AddEffect(EffectKind.Text, player.X, player.Y - 100, player.X, player.Y - 100, 42, Color.LimeGreen, "HP +" + heal, player.Facing);
        }

        private void UseMpPotion()
        {
            if (itemCooldown > 0) return;
            if (player.MpPotion <= 0)
            {
                AddLog("MP 복구 포션이 없습니다.");
                return;
            }
            int recover = Math.Min(player.MaxMp - player.Mp, 38 + player.Level * 5);
            if (recover <= 0)
            {
                AddEffect(EffectKind.Text, player.X, player.Y - 92, player.X, player.Y - 92, 28, Color.LightGray, "MP FULL", player.Facing);
                return;
            }
            itemCooldown = 24;
            player.MpPotion--;
            player.Mp += recover;
            AddEffect(EffectKind.Heal, player.X, player.Y - 35, player.X, player.Y - 35, 36, Color.DodgerBlue, "F", player.Facing);
            AddEffect(EffectKind.Text, player.X, player.Y - 100, player.X, player.Y - 100, 42, Color.SkyBlue, "MP +" + recover, player.Facing);
        }

        private void FinishDungeon()
        {
            int clearSeconds = Math.Max(1, (tick - dungeonStartTick) / 60);
            int score = 1000 - clearSeconds * 4 - dungeonHitCount * 70 + maxCombo * 25 + dungeonKillCount * 35 + (hiddenFolderFound ? 120 : 0);
            string grade = score >= 1050 ? "S" : score >= 820 ? "A" : score >= 610 ? "B" : "C";
            string gradeName = grade == "S" ? "시스템 최적화 완료" : grade == "A" ? "안정화 성공" : grade == "B" ? "오류 일부 잔존" : "긴급 복구 필요";
            lastClearedDungeonType = currentDungeon.Type;
            lastClearGrade = grade;

            int rewardGold = 200 + currentDungeon.RecommendedLevel * 80;
            int rewardExp = 100 + currentDungeon.RecommendedLevel * 90;
            int rewardPatch = 6 + currentDungeon.RecommendedLevel;
            if (grade == "S") { rewardGold += 180; rewardPatch += 8; }
            else if (grade == "A") { rewardGold += 90; rewardPatch += 4; }
            if (maxCombo >= 10) rewardPatch += 3;

            player.Gold += rewardGold;
            player.PatchShards += rewardPatch;
            bool leveled = player.AddExp(rewardExp);
            if (leveled) HandleLevelUpWeaponGrowth(player.X, player.Y);
            resultText = currentDungeon.DisplayName + " 클리어!\n\n" +
                         "최종 등급: " + grade + "  - " + gradeName + "\n" +
                         "클리어 시간: " + clearSeconds + "초\n" +
                         "처치 몬스터: " + dungeonKillCount + "마리\n" +
                         "피격 횟수: " + dungeonHitCount + "회\n" +
                         "최대 콤보: " + maxCombo + " COMBO\n" +
                         "숨겨진 폴더: " + (hiddenFolderFound ? "발견" : "미발견") + "\n" +
                         "획득 드롭: " + lastDropText + "\n\n" +
                         "Gold +" + rewardGold + "\n" +
                         "EXP +" + rewardExp + "\n" +
                         "패치 조각 +" + rewardPatch + "\n" +
                         (leveled ? "\n레벨 업! Lv." + player.Level + "\n" : "") +
                         "Enter를 누르면 오류 메시지 스토리 창으로 이동합니다.";
            if (grade == "S") UnlockAchievement("S급 최적화");
            if (maxCombo >= 10) UnlockAchievement("콤보 장인");
            screen = GameScreen.Result;
            AddLog(currentDungeon.DisplayName + " 클리어 보상 획득. 등급: " + grade);
        }


        private void PlayStrongSound()
        {
            try { Console.Beep(880, 110); Console.Beep(440, 140); } catch { }
        }

        private void PlayNpcSound()
        {
            try { Console.Beep(660, 90); Console.Beep(520, 90); } catch { }
        }

        private void PlayShortBeep()
        {
            try { Console.Beep(720, 70); } catch { }
        }

        private string GetNpcLine()
        {
            string[] lines = new string[]
            {
                "NPC 404호: 파일 실행은 인생과 같습니다. 대충 누르면 대충 터집니다.",
                "NPC 404호: 패치 조각 없으면 입구컷입니다. 이게 바로 윈도우식 매너.",
                "NPC 404호: T 누르면 전직합니다. 단, 레벨 낮으면 제가 비웃습니다.",
                "NPC 404호: U는 무기 강화입니다. 실패하면 멘탈도 같이 강화됩니다.",
                "NPC 404호: 태초마을 버튼은 도망이 아니라 전략적 회귀라고 우겨봅시다.",
                "NPC 404호: QWER 누르다 보면 버그도 울고 교수님도 끄덕일 겁니다. 아마도요."
            };
            return lines[(tick / 180 + npcLineSeed) % lines.Length];
        }

        private void NpcMock(string message)
        {
            npcToast = "NPC 404호: " + message;
            npcToastTicks = 210;
            npcLineSeed = random.Next(0, 1000);
            PlayNpcSound();
            AddLog(npcToast);
        }

        private void AgreeHeroIntro()
        {
            PlayStrongSound();
            if (random.Next(0, 2) == 0)
            {
                int w = 1180 + random.Next(0, 160);
                int h = 680 + random.Next(0, 90);
                ClientSize = new Size(w, h);
                NpcMock("동의했으니 창 크기를 살짝 흔들었습니다. 이것이 계약서의 무서움.");
            }
            else
            {
                ReturnToTaechoVillage();
                NpcMock("동의했으니 태초마을로 보냅니다. 모든 용사의 국룰입니다.");
                return;
            }
            screen = GameScreen.FileSelect;
        }

        private void RefuseHeroIntro()
        {
            PlayNpcSound();
            selectedFile = 0;
            screen = GameScreen.FileSelect;
            NpcMock("거절 버튼을 눌렀지만 이미 용사 등록은 완료됐습니다. 축하합니다 피해자님.");
        }

        private void ReturnToTaechoVillage()
        {
            selectedFile = 0;
            currentDungeon = null;
            cameraX = 0;
            left = false;
            right = false;
            jumpHeld = false;
            screen = GameScreen.FileSelect;
        }

        private void TryJobChange()
        {
            if (player.JobTier > 0)
            {
                NpcMock("이미 전직했는데 또요? 욕심이 System32급입니다.");
                return;
            }
            if (player.Level < 4)
            {
                NpcMock("전직은 Lv.4부터입니다. 지금은 튜토리얼 폴더에서 좀 더 구르세요.");
                return;
            }
            player.JobTier = 1;
            player.Attack += 8;
            player.Defense += 5;
            player.MaxHp += 28;
            player.MaxMp += 12;
            player.Speed += 2;
            player.Luck += 2;
            player.Hp = player.MaxHp;
            player.Mp = player.MaxMp;
            UnlockAchievement("전직 성공");
            PlayStrongSound();
            AddLog("전직 완료: " + player.DisplayJobName + " / NPC가 갑자기 존댓말을 하기 시작합니다.");
        }

        private void TryManualWeaponUpgrade()
        {
            int cost = 90 + player.WeaponLevel * 55;
            if (player.Gold < cost)
            {
                NpcMock("강화 비용 " + cost + "G도 없네요. 지갑이 read-only 속성입니다.");
                return;
            }
            player.Gold -= cost;
            int chance = Math.Max(35, 82 - player.WeaponLevel * 5 + player.Luck / 3);
            if (random.Next(0, 100) < chance)
            {
                player.ApplyWeaponUpgrade();
                PlayStrongSound();
                AddLog("무기 강화 성공: " + player.WeaponName + " / 공격력이 증가했습니다.");
                npcToast = "NPC 404호: 성공이라니... 제 농락 대본이 취소됐습니다.";
                npcToastTicks = 180;
            }
            else
            {
                player.FailedUpgrades++;
                NpcMock(GetUpgradeMockLine());
                if (player.FailedUpgrades % 3 == 0)
                {
                    player.PatchShards += 2;
                    AddLog("불쌍 보상: 패치 조각 +2");
                }
            }
        }

        private string GetUpgradeMockLine()
        {
            string[] mocks = new string[]
            {
                "강화 실패! 무기가 방금 블루스크린 보고 휴가 갔습니다.",
                "실패! 그 클릭은 용감했지만 결과는 휴지통행입니다.",
                "강화 실패! NPC도 예상했는데 본인만 몰랐습니다.",
                "실패! 무기가 '+1은 싫다'며 실행을 거부했습니다.",
                "실패! 이것이 바로 운빨 알고리즘의 참교육입니다."
            };
            return mocks[random.Next(0, mocks.Length)];
        }

        private void HandleLevelUpWeaponGrowth(float x, float y)
        {
            int chance = 72 + Math.Min(18, player.Luck / 2);
            if (random.Next(0, 100) < chance)
            {
                player.ApplyWeaponUpgrade();
                AddLog("레벨업 보너스: 무기 자동 강화 → " + player.WeaponName);
                AddEffect(EffectKind.Text, x, y - 160, x, y - 160, 64, Color.FromArgb(120, 230, 255), "WEAPON UP!", player.Facing);
            }
            else
            {
                player.FailedUpgrades++;
                NpcMock("레벨업했는데 무기 강화는 실패. 성장과 현실은 별개입니다.");
                AddEffect(EffectKind.Text, x, y - 160, x, y - 160, 56, Color.OrangeRed, "UPGRADE FAIL", player.Facing);
            }
        }

        private void SaveGame()
        {
            try
            {
                List<string> lines = new List<string>();
                lines.Add(player.Name);
                lines.Add(((int)player.Job).ToString());
                lines.Add(player.Level.ToString());
                lines.Add(player.Exp.ToString());
                lines.Add(player.NextExp.ToString());
                lines.Add(player.Gold.ToString());
                lines.Add(player.PatchShards.ToString());
                lines.Add(player.Hp.ToString());
                lines.Add(player.MaxHp.ToString());
                lines.Add(player.Mp.ToString());
                lines.Add(player.MaxMp.ToString());
                lines.Add(player.Attack.ToString());
                lines.Add(player.Defense.ToString());
                lines.Add(player.Speed.ToString());
                lines.Add(player.Luck.ToString());
                lines.Add(player.Potion.ToString());
                lines.Add(player.MpPotion.ToString());
                lines.Add(player.JobTier.ToString());
                lines.Add(player.WeaponLevel.ToString());
                lines.Add(player.FailedUpgrades.ToString());
                lines.Add(trueFinalUnlocked ? "1" : "0");
                lines.Add(normalEndingSeen ? "1" : "0");
                lines.Add(trueEndingSeen ? "1" : "0");
                lines.Add(clueCount.ToString());
                lines.Add(companionCount.ToString());
                lines.Add(abandonedCount.ToString());
                lines.Add(deletedCount.ToString());
                lines.Add(suspectFailCount.ToString());
                lines.Add(companionHistory.Replace("\n", "|").Replace("\r", ""));
                lines.Add(hintHistory.Replace("\n", "|").Replace("\r", ""));
                File.WriteAllLines(saveFile, lines.ToArray());
                AddLog("저장 완료: " + saveFile);
            }
            catch
            {
                AddLog("저장 실패: 문서 폴더 권한을 확인하세요.");
            }
        }

        private void LoadGame()
        {
            try
            {
                if (!File.Exists(saveFile))
                {
                    AddLog("저장 파일이 없습니다.");
                    screen = GameScreen.JobSelect;
                    return;
                }
                string[] l = File.ReadAllLines(saveFile);
                if (l.Length < 16) throw new InvalidDataException();
                player.Name = l[0];
                playerName = player.Name;
                player.ApplyJob((JobType)int.Parse(l[1]));
                player.Level = int.Parse(l[2]);
                player.Exp = int.Parse(l[3]);
                player.NextExp = int.Parse(l[4]);
                player.Gold = int.Parse(l[5]);
                player.PatchShards = int.Parse(l[6]);
                player.Hp = int.Parse(l[7]);
                player.MaxHp = int.Parse(l[8]);
                player.Mp = int.Parse(l[9]);
                player.MaxMp = int.Parse(l[10]);
                player.Attack = int.Parse(l[11]);
                player.Defense = int.Parse(l[12]);
                player.Speed = int.Parse(l[13]);
                player.Luck = int.Parse(l[14]);
                player.Potion = int.Parse(l[15]);
                player.MpPotion = l.Length > 16 ? int.Parse(l[16]) : 3;
                player.JobTier = l.Length > 17 ? int.Parse(l[17]) : 0;
                player.WeaponLevel = l.Length > 18 ? int.Parse(l[18]) : 1;
                player.FailedUpgrades = l.Length > 19 ? int.Parse(l[19]) : 0;
                trueFinalUnlocked = l.Length > 20 && l[20] == "1";
                normalEndingSeen = l.Length > 21 && l[21] == "1";
                trueEndingSeen = l.Length > 22 && l[22] == "1";
                clueCount = l.Length > 23 ? int.Parse(l[23]) : 0;
                companionCount = l.Length > 24 ? int.Parse(l[24]) : 0;
                abandonedCount = l.Length > 25 ? int.Parse(l[25]) : 0;
                deletedCount = l.Length > 26 ? int.Parse(l[26]) : 0;
                suspectFailCount = l.Length > 27 ? int.Parse(l[27]) : 0;
                companionHistory = l.Length > 28 ? l[28].Replace("|", "\n") : "";
                hintHistory = l.Length > 29 ? l[29].Replace("|", "\n") : "";
                AddLog("불러오기 완료.");
                screen = GameScreen.FileSelect;
            }
            catch
            {
                AddLog("불러오기 실패: 저장 파일이 손상되었습니다.");
            }
        }

        private void AddLog(string msg)
        {
            log.Add(msg);
            while (log.Count > 6) log.RemoveAt(0);
        }

        private void AddEffect(EffectKind kind, float x, float y, float x2, float y2, int ticks, Color color, string text, int dir)
        {
            effects.Add(new Effect(kind, x, y, x2, y2, ticks, color, text, dir));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            buttons.Clear();
            if (screen == GameScreen.Title) DrawTitle(g);
            else if (screen == GameScreen.JobSelect) DrawJobSelect(g);
            else if (screen == GameScreen.HeroIntro) DrawHeroIntro(g);
            else if (screen == GameScreen.FileSelect) DrawFileSelect(g);
            else if (screen == GameScreen.ItemShop) DrawItemShop(g);
            else if (screen == GameScreen.Dungeon) DrawDungeon(g);
            else if (screen == GameScreen.Result) DrawResult(g);
            else if (screen == GameScreen.StoryDialog) DrawStoryDialog(g);
            else if (screen == GameScreen.CompanionChoice) DrawCompanionChoice(g);
            else if (screen == GameScreen.RewardChoice) DrawRewardChoice(g);
            else if (screen == GameScreen.SuspectSelect) DrawSuspectSelect(g);
            else if (screen == GameScreen.Ending) DrawEnding(g);
            else if (screen == GameScreen.Help) DrawHelp(g);
        }

        private Rectangle AppRect()
        {
            return new Rectangle(108, 28, ClientSize.Width - 136, ClientSize.Height - TaskbarHeight - 56);
        }

        private void DrawDesktop(Graphics g, string activeTitle)
        {
            Renderer.DrawDesktopWallpaper(g, ClientRectangle, cameraX);
            DrawDesktopIcons(g);
            DrawTaskbar(g, activeTitle);
        }

        private void DrawDesktopIcons(Graphics g)
        {
            DrawDesktopIcon(g, "내 컴퓨터", 24, 20, Color.FromArgb(80, 160, 210));
            DrawDesktopIcon(g, "내 문서", 24, 118, Color.FromArgb(240, 190, 45));
            DrawDesktopIcon(g, "3.5 플로피", 24, 216, Color.FromArgb(100, 115, 135));
            DrawDesktopIcon(g, "제어판", 24, 314, Color.FromArgb(230, 200, 70));
            DrawDesktopIcon(g, "시스템 카드", 24, 412, Color.FromArgb(65, 145, 210));
            DrawDesktopIcon(g, "휴지통", 24, 510, Color.FromArgb(205, 215, 215));
        }

        private void DrawDesktopIcon(Graphics g, string label, int x, int y, Color c)
        {
            Rectangle r = new Rectangle(x + 10, y, 56, 56);
            using (SolidBrush b = new SolidBrush(Color.FromArgb(60, 255, 255, 255))) g.FillRectangle(b, r);
            Renderer.DrawLargeFileSymbol(g, r, c, false);
            using (Font f = Renderer.Font(8.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.White))
                g.DrawString(label, f, b, new Rectangle(x - 4, y + 58, 88, 34), Renderer.Center());
        }

        private void DrawTaskbar(Graphics g, string title)
        {
            Rectangle r = new Rectangle(0, ClientSize.Height - TaskbarHeight, ClientSize.Width, TaskbarHeight);
            Renderer.Panel(g, r, Color.FromArgb(210, 216, 226));
            Rectangle start = new Rectangle(8, r.Y + 4, 94, TaskbarHeight - 8);
            Renderer.Button(g, start, "시작", false);
            buttons.Add(new UiButton(start, "backTitle"));
            Rectangle app = new Rectangle(118, r.Y + 4, Math.Min(470, ClientSize.Width - 530), TaskbarHeight - 8);
            Renderer.Inset(g, app, Color.FromArgb(238, 244, 250));
            using (Font f = Renderer.Font(9f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(22, 34, 56)))
                g.DrawString(title, f, b, new Rectangle(app.X + 12, app.Y, app.Width - 24, app.Height), Renderer.LeftMiddle());
            Rectangle tray = new Rectangle(ClientSize.Width - 180, r.Y + 4, 170, TaskbarHeight - 8);
            Renderer.Inset(g, tray, Color.FromArgb(235, 240, 245));
            using (Font f = Renderer.Font(9f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(22, 34, 56)))
                g.DrawString(DateTime.Now.ToString("tt hh:mm"), f, b, tray, Renderer.Center());
        }

        private void DrawWindowFrame(Graphics g, Rectangle app, string title)
        {
            Renderer.Panel(g, app, Color.FromArgb(205, 210, 220));
            Rectangle tb = new Rectangle(app.X + 5, app.Y + 5, app.Width - 10, 30);
            using (LinearGradientBrush b = new LinearGradientBrush(tb, Color.FromArgb(24, 60, 150), Color.FromArgb(68, 148, 230), 0f))
                g.FillRectangle(b, tb);
            using (Font f = Renderer.Font(10.5f, FontStyle.Bold))
            using (SolidBrush br = new SolidBrush(Color.White))
                g.DrawString(title, f, br, new Rectangle(tb.X + 12, tb.Y, tb.Width - 120, tb.Height), Renderer.LeftMiddle());
            Renderer.Button(g, new Rectangle(tb.Right - 78, tb.Y + 4, 22, 22), "_", false);
            Renderer.Button(g, new Rectangle(tb.Right - 52, tb.Y + 4, 22, 22), "□", false);
            Renderer.Button(g, new Rectangle(tb.Right - 26, tb.Y + 4, 22, 22), "X", false);
            using (Font f = Renderer.Font(8.5f, FontStyle.Regular))
            using (SolidBrush br = new SolidBrush(Color.FromArgb(20, 20, 20)))
                g.DrawString("파일(F)    보기(V)    이동(M)    스킬(K)    도움말(H)", f, br, new Rectangle(app.X + 18, app.Y + 40, app.Width - 36, 22), Renderer.LeftMiddle());
            using (Pen p = new Pen(Color.Gray)) g.DrawLine(p, app.X + 8, app.Y + 64, app.Right - 8, app.Y + 64);
        }

        private void DrawTitle(Graphics g)
        {
            DrawDesktop(g, "디버그 용사: Windows File Dungeon DX");
            Rectangle app = AppRect();
            DrawWindowFrame(g, app, "디버그 용사: Windows File Dungeon DX");
            Rectangle c = new Rectangle(app.X + 10, app.Y + 70, app.Width - 20, app.Height - 80);
            using (LinearGradientBrush b = new LinearGradientBrush(c, Color.FromArgb(12, 28, 84), Color.FromArgb(50, 104, 184), 90f)) g.FillRectangle(b, c);
            using (Pen grid = new Pen(Color.FromArgb(36, 190, 230, 255)))
            {
                for (int x = c.X; x < c.Right; x += 42) g.DrawLine(grid, x, c.Y, x, c.Bottom);
                for (int y = c.Y; y < c.Bottom; y += 38) g.DrawLine(grid, c.X, y, c.Right, y);
            }
            using (Font f = Renderer.Font(38f, FontStyle.Bold))
            using (SolidBrush w = new SolidBrush(Color.White))
            using (SolidBrush cyan = new SolidBrush(Color.FromArgb(140, 240, 255)))
            {
                g.DrawString("디버그 용사", f, w, new Rectangle(c.X + 20, c.Y + 45, c.Width - 40, 70), Renderer.Center());
                g.DrawString("Windows File Dungeon DX", f, cyan, new Rectangle(c.X + 20, c.Y + 102, c.Width - 40, 70), Renderer.Center());
            }
            using (Font f = Renderer.Font(13f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(220, 240, 255)))
                g.DrawString("파일을 선택해 던전에 진입하고, 키보드로 이동/점프/공격하는 횡스크롤 액션 RPG", f, b, new Rectangle(c.X + 20, c.Y + 172, c.Width - 40, 28), Renderer.Center());

            Rectangle menu = new Rectangle(c.X + 80, c.Y + 240, 300, 250);
            DrawMenuButton(g, new Rectangle(menu.X, menu.Y, 270, 58), "새 게임", "new", true);
            DrawMenuButton(g, new Rectangle(menu.X, menu.Y + 72, 270, 58), "불러오기", "load", false);
            DrawMenuButton(g, new Rectangle(menu.X, menu.Y + 144, 270, 58), "도움말", "help", false);

            Rectangle preview = new Rectangle(c.Right - 440, c.Y + 235, 360, 270);
            Renderer.Panel(g, preview, Color.FromArgb(230, 238, 248));
            Renderer.Header(g, new Rectangle(preview.X + 6, preview.Y + 6, preview.Width - 12, 32), "게임 미리보기");
            Renderer.DrawLargeFileSymbol(g, new Rectangle(preview.X + 28, preview.Y + 60, 100, 100), Color.FromArgb(70, 140, 240), false);
            Renderer.DrawLargeFileSymbol(g, new Rectangle(preview.X + 142, preview.Y + 76, 86, 86), Color.FromArgb(80, 190, 110), false);
            Renderer.DrawLargeFileSymbol(g, new Rectangle(preview.X + 242, preview.Y + 62, 92, 92), Color.FromArgb(220, 85, 70), false);
            using (Font f = Renderer.Font(10f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(32, 48, 70)))
                g.DrawString("파일 안에 숨겨진 던전\n→ 파일 선택\n→ 횡스크롤 전투\n→ 레벨업과 보스전", f, b, new Rectangle(preview.X + 26, preview.Y + 172, preview.Width - 52, 90), Renderer.Center());
        }

        private void DrawMenuButton(Graphics g, Rectangle r, string text, string action, bool selected)
        {
            Renderer.Button(g, r, text, selected);
            buttons.Add(new UiButton(r, action));
        }


        private void DrawHeroIntro(Graphics g)
        {
            DrawDesktop(g, "긴급 오류: 용사 등록 프로세스");
            int shakeX = introShakeTicks > 0 ? (int)(Math.Sin(tick * 0.8) * 8) : 0;
            int shakeY = introShakeTicks > 0 ? (int)(Math.Cos(tick * 0.9) * 5) : 0;
            Rectangle app = new Rectangle(ClientSize.Width / 2 - 430 + shakeX, ClientSize.Height / 2 - 250 + shakeY, 860, 500);
            Renderer.Panel(g, app, Color.FromArgb(238, 238, 238));
            Rectangle title = new Rectangle(app.X + 5, app.Y + 5, app.Width - 10, 34);
            using (LinearGradientBrush b = new LinearGradientBrush(title, Color.FromArgb(150, 10, 10), Color.FromArgb(255, 60, 45), 0f))
                g.FillRectangle(b, title);
            using (Font f = Renderer.Font(11f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.White))
                g.DrawString("치명적 오류 - 용사 소개.exe", f, b, new Rectangle(title.X + 12, title.Y, title.Width - 90, title.Height), Renderer.LeftMiddle());
            Renderer.Button(g, new Rectangle(title.Right - 72, title.Y + 5, 22, 24), "_", false);
            Renderer.Button(g, new Rectangle(title.Right - 48, title.Y + 5, 22, 24), "□", false);
            Renderer.Button(g, new Rectangle(title.Right - 24, title.Y + 5, 22, 24), "X", false);

            Rectangle warning = new Rectangle(app.X + 28, app.Y + 68, 130, 130);
            using (SolidBrush red = new SolidBrush(Color.FromArgb(235, 60, 40)))
                g.FillEllipse(red, warning);
            using (Font f = Renderer.Font(64f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.White))
                g.DrawString("!", f, b, warning, Renderer.Center());

            using (Font f = Renderer.Font(24f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(120, 20, 20)))
                g.DrawString("ERROR: 평범한 학생을 용사로 변환 중", f, b, new Rectangle(app.X + 180, app.Y + 62, app.Width - 220, 50), Renderer.LeftMiddle());

            string intro = "대상: " + player.Name + "\n" +
                "직업: " + player.Profile.Name + " → " + player.DisplayJobName + " 예약 대기\n" +
                "무기: " + player.WeaponName + "\n\n" +
                "NPC 404호: 반갑습니다. 저는 진행 담당 오류창입니다.\n" +
                "지금부터 당신은 파일을 실행하다가 던전에 빠지는 사람입니다.\n" +
                "동의하면 창 크기가 변하거나 태초마을로 돌아갈 수 있습니다.\n" +
                "거절해도 별 의미는 없습니다. 이미 프로그램이 웃고 있습니다.";
            using (Font f = Renderer.Font(12.2f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(32, 38, 54)))
                g.DrawString(intro, f, b, new Rectangle(app.X + 184, app.Y + 126, app.Width - 220, 210), Renderer.Left());

            Rectangle heroBox = new Rectangle(app.X + 42, app.Y + 230, 190, 170);
            Renderer.Inset(g, heroBox, Color.White);
            Player preview = new Player();
            preview.ApplyJob(player.Job);
            preview.Name = player.Name;
            preview.X = heroBox.X + heroBox.Width / 2;
            preview.Y = heroBox.Y + 130;
            Renderer.DrawPlayer(g, preview, 0, tick, false);
            using (Font f = Renderer.Font(9f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(100, 20, 20)))
                g.DrawString("용사 등록 중...\n삐빅, 병맛 감지", f, b, new Rectangle(heroBox.X + 8, heroBox.Y + 8, heroBox.Width - 16, 44), Renderer.Center());

            Rectangle agree = new Rectangle(app.Right - 335, app.Bottom - 70, 150, 42);
            Rectangle nope = new Rectangle(app.Right - 174, app.Bottom - 70, 130, 42);
            Renderer.Button(g, agree, "동의", true);
            Renderer.Button(g, nope, "싫은데요", false);
            buttons.Add(new UiButton(agree, "introAgree"));
            buttons.Add(new UiButton(nope, "introNo"));

            using (Font f = Renderer.Font(9f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.DarkRed))
                g.DrawString("※ Enter/E로 동의 가능. 소리는 시스템 오류음으로 대체됩니다.", f, b, new Rectangle(app.X + 30, app.Bottom - 64, 430, 30), Renderer.LeftMiddle());
        }

        private void DrawNpcGuide(Graphics g, Rectangle r)
        {
            Renderer.Panel(g, r, Color.FromArgb(246, 241, 225));
            Renderer.Header(g, new Rectangle(r.X + 6, r.Y + 6, r.Width - 12, 28), "NPC 404호의 막장 안내");
            Rectangle face = new Rectangle(r.X + 18, r.Y + 42, 58, 58);
            using (SolidBrush b = new SolidBrush(Color.FromArgb(255, 236, 120))) g.FillEllipse(b, face);
            using (Pen p = new Pen(Color.FromArgb(80, 60, 20), 3f)) g.DrawEllipse(p, face);
            using (SolidBrush b = new SolidBrush(Color.FromArgb(20, 20, 20)))
            {
                g.FillEllipse(b, face.X + 17, face.Y + 20, 6, 6);
                g.FillEllipse(b, face.X + 35, face.Y + 20, 6, 6);
            }
            using (Pen p = new Pen(Color.FromArgb(130, 40, 40), 2f)) g.DrawArc(p, face.X + 17, face.Y + 30, 24, 15, 10, 160);
            string line = npcToastTicks > 0 ? npcToast : GetNpcLine();
            using (Font f = Renderer.Font(9.3f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(50, 42, 28)))
                g.DrawString(line, f, b, new Rectangle(r.X + 90, r.Y + 42, r.Width - 105, 54), Renderer.Left());
            using (Font f = Renderer.Font(8.5f, FontStyle.Regular))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(95, 78, 45)))
                g.DrawString("T 전직 / U 강화 / 동의하면 가끔 창 크기 변경 / 실패하면 제가 놀립니다.", f, b, new Rectangle(r.X + 90, r.Bottom - 28, r.Width - 105, 20), Renderer.LeftMiddle());
        }

        private void DrawJobSelect(Graphics g)
        {
            DrawDesktop(g, "직업 선택");
            Rectangle app = AppRect();
            DrawWindowFrame(g, app, "디버그 용사: 직업 선택");
            Rectangle c = new Rectangle(app.X + 10, app.Y + 70, app.Width - 20, app.Height - 80);
            Renderer.Panel(g, c, Color.FromArgb(228, 234, 242));
            using (Font f = Renderer.Font(28f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(24, 60, 150)))
                g.DrawString("직업 선택", f, b, new Rectangle(c.X, c.Y + 12, c.Width, 50), Renderer.Center());
            int cardW = (c.Width - 70) / 4;
            int cardH = Math.Min(355, c.Height - 160);
            for (int i = 0; i < GameData.Jobs.Count; i++)
            {
                Rectangle card = new Rectangle(c.X + 20 + i * (cardW + 10), c.Y + 88, cardW, cardH);
                DrawJobCard(g, card, GameData.Jobs[i], i == selectedJob, i);
                buttons.Add(new UiButton(card, "job" + i));
            }
            Rectangle input = new Rectangle(c.X + 70, c.Bottom - 58, c.Width - 330, 40);
            Renderer.Inset(g, input, Color.White);
            using (Font f = Renderer.Font(11f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(30, 40, 60)))
                g.DrawString("이름: " + playerName + "_", f, b, new Rectangle(input.X + 12, input.Y, input.Width - 24, input.Height), Renderer.LeftMiddle());
            Rectangle ok = new Rectangle(input.Right + 18, input.Y, 120, 40);
            Rectangle back = new Rectangle(ok.Right + 12, input.Y, 100, 40);
            Renderer.Button(g, ok, "선택", true);
            Renderer.Button(g, back, "뒤로", false);
            buttons.Add(new UiButton(ok, "confirmJob"));
            buttons.Add(new UiButton(back, "backTitle"));
        }

        private void DrawJobCard(Graphics g, Rectangle r, JobProfile job, bool selected, int index)
        {
            Renderer.Panel(g, r, selected ? Color.FromArgb(245, 248, 255) : Color.FromArgb(232, 236, 242));
            Renderer.Header(g, new Rectangle(r.X + 6, r.Y + 6, r.Width - 12, 32), (index + 1) + ". " + job.Name);
            if (selected)
            {
                using (Pen p = new Pen(job.MainColor, 4f)) g.DrawRectangle(p, r.X + 2, r.Y + 2, r.Width - 5, r.Height - 5);
            }
            Rectangle portrait = new Rectangle(r.X + 22, r.Y + 52, r.Width - 44, 125);
            Renderer.Inset(g, portrait, Color.FromArgb(246, 250, 255));
            Player dummy = new Player();
            dummy.ApplyJob(job.Type);
            dummy.X = portrait.X + portrait.Width / 2;
            dummy.Y = portrait.Y + 104;
            Renderer.DrawPlayer(g, dummy, 0, tick, false);
            using (Font f = Renderer.Font(11f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(job.MainColor))
                g.DrawString(job.Role, f, b, new Rectangle(r.X + 10, portrait.Bottom + 8, r.Width - 20, 26), Renderer.Center());
            using (Font f = Renderer.Font(8.5f, FontStyle.Regular))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(42, 52, 72)))
                g.DrawString(job.Description, f, b, new Rectangle(r.X + 16, portrait.Bottom + 34, r.Width - 32, 54), Renderer.Center());
            using (Font f = Renderer.Font(8.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(36, 48, 70)))
            {
                int y = r.Bottom - 88;
                g.DrawString("HP " + job.StartHp + " / MP " + job.StartMp, f, b, new Rectangle(r.X + 16, y, r.Width - 32, 18), Renderer.LeftMiddle());
                g.DrawString("공격 " + job.StartAttack + " / 방어 " + job.StartDefense, f, b, new Rectangle(r.X + 16, y + 20, r.Width - 32, 18), Renderer.LeftMiddle());
                g.DrawString("스킬: " + job.SkillName, f, new SolidBrush(job.MainColor), new Rectangle(r.X + 16, y + 43, r.Width - 32, 20), Renderer.LeftMiddle());
            }
        }


        private void DrawFileSelect(Graphics g)
        {
            // 파일 선택 화면은 더 이상 단순 목록이 아니라, 윈도우 바탕화면 위에 던전 파일들이 놓인 구조입니다.
            DrawDesktop(g, "던전 파일 바탕화면");

            Rectangle desktopTitle = new Rectangle(128, 24, Math.Min(720, ClientSize.Width - 520), 44);
            Renderer.Panel(g, desktopTitle, Color.FromArgb(235, 242, 250));
            using (Font f = Renderer.Font(11f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(24, 44, 78)))
            {
                string title = "C:\\WindowsKingdom\\DungeonDesktop";
                g.DrawString(title, f, b, new Rectangle(desktopTitle.X + 16, desktopTitle.Y, desktopTitle.Width - 32, desktopTitle.Height), Renderer.LeftMiddle());
            }

            using (Font f = Renderer.Font(9.2f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(235, 255, 255, 255)))
            using (SolidBrush shadow = new SolidBrush(Color.FromArgb(90, 0, 0, 0)))
            {
                Rectangle hint = new Rectangle(135, 72, Math.Min(760, ClientSize.Width - 520), 24);
                g.DrawString("←/→/↑/↓로 파일 선택, Enter/E로 실행 · 잠긴 파일은 패치 조각 필요", f, shadow, new Rectangle(hint.X + 2, hint.Y + 2, hint.Width, hint.Height), Renderer.LeftMiddle());
                g.DrawString("←/→/↑/↓로 파일 선택, Enter/E로 실행 · 잠긴 파일은 패치 조각 필요", f, b, hint, Renderer.LeftMiddle());
            }

            Rectangle workArea = new Rectangle(112, 96, ClientSize.Width - 485, ClientSize.Height - TaskbarHeight - 184);
            int cols = Math.Max(3, Math.Min(5, workArea.Width / 138));
            int cellW = Math.Max(126, workArea.Width / cols);
            int cellH = 124;
            int iconW = 112;
            int iconH = 102;

            for (int i = 0; i < GameData.Dungeons.Count; i++)
            {
                DungeonInfo d = GameData.Dungeons[i];
                bool locked = IsDungeonLocked(d);
                int col = i % cols;
                int row = i / cols;
                Rectangle icon = new Rectangle(workArea.X + 18 + col * cellW, workArea.Y + 16 + row * cellH, iconW, iconH);
                if (icon.Bottom > ClientSize.Height - TaskbarHeight - 178) continue;
                Renderer.DrawDungeonDesktopIcon(g, icon, d, i == selectedFile, locked, i);
                buttons.Add(new UiButton(icon, "file" + i));
            }

            DrawDesktopSystemShortcuts(g, new Rectangle(112, ClientSize.Height - TaskbarHeight - 170, ClientSize.Width - 505, 60));

            // 오른쪽에는 선택한 파일의 속성 창을 띄워 실제 파일을 선택하는 느낌을 강화합니다.
            DungeonInfo selected = GameData.Dungeons[selectedFile];
            bool selectedLocked = IsDungeonLocked(selected);
            Rectangle prop = new Rectangle(ClientSize.Width - 388, 24, 360, ClientSize.Height - TaskbarHeight - 48);
            Renderer.Panel(g, prop, Color.FromArgb(232, 238, 247));
            Renderer.Header(g, new Rectangle(prop.X + 6, prop.Y + 6, prop.Width - 12, 32), "파일 속성 / 던전 정보");
            Rectangle preview = new Rectangle(prop.X + 24, prop.Y + 56, 118, 118);
            Renderer.DrawLargeFileSymbol(g, preview, selected.Accent, selectedLocked);

            using (Font f = Renderer.Font(11f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(selectedLocked ? Color.Gray : Color.FromArgb(20, 36, 64)))
                g.DrawString(selected.FileName, f, b, new Rectangle(prop.X + 154, prop.Y + 56, prop.Width - 174, 44), Renderer.Left());
            using (Font f = Renderer.Font(8.8f, FontStyle.Regular))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(54, 68, 92)))
            {
                string path = "위치: C:\\WindowsKingdom\\Dungeons\\" + selected.FileName + "\n" +
                              "종류: 실행 가능한 던전 파일\n" +
                              "권장 레벨: Lv. " + selected.RecommendedLevel + "\n" +
                              "필요 패치 조각: " + selected.RequiredPatch + "\n" +
                              "크기: " + (selected.MapWidth / 10) + " KB";
                g.DrawString(path, f, b, new Rectangle(prop.X + 154, prop.Y + 102, prop.Width - 174, 88), Renderer.Left());
            }

            Rectangle desc = new Rectangle(prop.X + 22, prop.Y + 214, prop.Width - 44, 118);
            Renderer.Inset(g, desc, Color.White);
            using (Font f = Renderer.Font(9.2f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(26, 42, 68)))
                g.DrawString("파일 설명", f, b, new Rectangle(desc.X + 12, desc.Y + 8, desc.Width - 24, 20), Renderer.LeftMiddle());
            using (Font f = Renderer.Font(8.7f, FontStyle.Regular))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(54, 68, 92)))
                g.DrawString(selected.Description + "\n\nNPC: " + GameData.GetDungeonNpcLine(selected.Type), f, b, new Rectangle(desc.X + 12, desc.Y + 34, desc.Width - 24, desc.Height - 44), Renderer.Left());

            Rectangle playerBox = new Rectangle(prop.X + 22, desc.Bottom + 18, prop.Width - 44, 142);
            Renderer.Panel(g, playerBox, Color.FromArgb(242, 246, 250));
            Renderer.Header(g, new Rectangle(playerBox.X + 5, playerBox.Y + 5, playerBox.Width - 10, 26), "플레이어 상태");
            Renderer.DrawPlayer(g, MakePreviewPlayer(playerBox.X + 64, playerBox.Y + 104), 0, tick, false);
            using (Font f = Renderer.Font(8.6f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(32, 45, 70)))
            {
                string s = player.Name + "\n" + player.Profile.Name + "  Lv. " + player.Level +
                    "\nHP " + player.Hp + "/" + player.MaxHp + "   MP " + player.Mp + "/" + player.MaxMp +
                    "\nGold " + player.Gold + " G   패치 " + player.PatchShards;
                g.DrawString(s, f, b, new Rectangle(playerBox.X + 126, playerBox.Y + 44, playerBox.Width - 138, 86), Renderer.Left());
            }

            Rectangle open = new Rectangle(prop.X + 24, prop.Bottom - 140, 150, 34);
            Rectangle save = new Rectangle(open.Right + 12, open.Y, 128, 34);
            Rectangle job = new Rectangle(prop.X + 24, prop.Bottom - 98, 96, 34);
            Rectangle upgrade = new Rectangle(job.Right + 10, job.Y, 112, 34);
            Rectangle taecho = new Rectangle(upgrade.Right + 10, job.Y, 90, 34);
            Rectangle back = new Rectangle(prop.X + 24, prop.Bottom - 48, prop.Width - 48, 34);
            Renderer.Button(g, open, selectedLocked ? "잠김" : "파일 실행", !selectedLocked);
            Renderer.Button(g, save, "저장", false);
            Renderer.Button(g, job, "T 전직", player.Level >= 4 && player.JobTier == 0);
            Renderer.Button(g, upgrade, "U 무기강화", true);
            Renderer.Button(g, taecho, "태초마을", false);
            Renderer.Button(g, back, "타이틀로", false);
            if (!selectedLocked) buttons.Add(new UiButton(open, "file" + selectedFile));
            buttons.Add(new UiButton(save, "save"));
            buttons.Add(new UiButton(job, "jobChange"));
            buttons.Add(new UiButton(upgrade, "upgradeWeapon"));
            buttons.Add(new UiButton(taecho, "taecho"));
            buttons.Add(new UiButton(back, "backTitle"));

            DrawNpcGuide(g, new Rectangle(128, ClientSize.Height - TaskbarHeight - 132, Math.Min(760, ClientSize.Width - 520), 108));
        }


        private void DrawDesktopSystemShortcuts(Graphics g, Rectangle area)
        {
            string[] names = new string[] { "휴지통 상점", "Windows 검색", "Windows Update", "제어판 강화", "메모장 로그" };
            string[] actions = new string[] { "shopRecycle", "desktopSearch", "desktopUpdate", "desktopControl", "desktopNotebook" };
            Color[] colors = new Color[] { Color.FromArgb(190, 205, 210), Color.FromArgb(70, 145, 230), Color.FromArgb(80, 190, 240), Color.FromArgb(230, 200, 70), Color.FromArgb(245, 245, 210) };
            int w = Math.Max(108, area.Width / names.Length - 10);
            for (int i = 0; i < names.Length; i++)
            {
                Rectangle r = new Rectangle(area.X + i * (w + 8), area.Y, w, 56);
                Renderer.Panel(g, r, Color.FromArgb(235, 240, 247));
                Renderer.DrawLargeFileSymbol(g, new Rectangle(r.X + 7, r.Y + 7, 38, 38), colors[i], false);
                using (Font f = Renderer.Font(7.6f, FontStyle.Bold))
                using (SolidBrush b = new SolidBrush(Color.FromArgb(22, 36, 58)))
                    g.DrawString(names[i], f, b, new Rectangle(r.X + 48, r.Y + 6, r.Width - 52, 34), Renderer.Left());
                using (Font f = Renderer.Font(6.8f, FontStyle.Bold))
                using (SolidBrush b = new SolidBrush(Color.FromArgb(80, 90, 110)))
                    g.DrawString(i == 0 ? "B/Delete" : "클릭", f, b, new Rectangle(r.X + 48, r.Bottom - 18, r.Width - 52, 14), Renderer.LeftMiddle());
                buttons.Add(new UiButton(r, actions[i]));
            }
        }

        private void DrawDungeonNpc(Graphics g)
        {
            if (currentDungeon == null) return;
            float worldX = 215;
            float sx = worldX - cameraX;
            if (sx < -180 || sx > ClientSize.Width + 180) return;
            int baseY = 570;
            Color c = currentDungeon.Accent;
            Rectangle body = new Rectangle((int)sx - 25, baseY - 92, 50, 82);
            using (SolidBrush glow = new SolidBrush(Color.FromArgb(55, c))) g.FillEllipse(glow, body.X - 24, body.Y - 20, body.Width + 48, body.Height + 44);
            using (SolidBrush coat = new SolidBrush(Color.FromArgb(230, c))) g.FillRectangle(coat, body.X + 8, body.Y + 34, body.Width - 16, 42);
            using (SolidBrush face = new SolidBrush(Color.FromArgb(242, 205, 155))) g.FillEllipse(face, body.X + 11, body.Y + 4, 28, 28);
            using (Pen p = new Pen(Color.FromArgb(30, 40, 60), 2f))
            {
                g.DrawEllipse(p, body.X + 11, body.Y + 4, 28, 28);
                g.DrawRectangle(p, body.X + 8, body.Y + 34, body.Width - 16, 42);
            }
            using (Font f = Renderer.Font(7f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.White))
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(145, 0, 0, 0)))
            {
                Rectangle name = new Rectangle((int)sx - 80, baseY - 124, 160, 18);
                g.FillRectangle(bg, name);
                g.DrawString(GameData.GetDungeonNpcName(currentDungeon.Type), f, b, name, Renderer.Center());
            }
            Rectangle bubble = new Rectangle((int)sx + 42, baseY - 138, 280, 58);
            if (bubble.Right > ClientSize.Width - 12) bubble.X = (int)sx - 322;
            Renderer.Panel(g, bubble, Color.FromArgb(255, 252, 230));
            using (Font f = Renderer.Font(7.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(42, 38, 26)))
                g.DrawString(GameData.GetDungeonNpcLine(currentDungeon.Type), f, b, new Rectangle(bubble.X + 8, bubble.Y + 7, bubble.Width - 16, bubble.Height - 14), Renderer.Left());
        }

        private void DrawItemShop(Graphics g)
        {
            DrawDesktop(g, "휴지통 아이템 상점");
            Rectangle app = AppRect();
            DrawWindowFrame(g, app, "$Recycle.Bin_ItemShop.exe");
            Rectangle c = new Rectangle(app.X + 18, app.Y + 82, app.Width - 36, app.Height - 104);
            Renderer.Panel(g, c, Color.FromArgb(238, 242, 247));
            Renderer.Header(g, new Rectangle(c.X + 18, c.Y + 18, c.Width - 36, 42), "휴지통 상점: 버린 아이템도 가격표가 붙으면 신상품");
            Rectangle npc = new Rectangle(c.X + 40, c.Y + 96, 240, c.Height - 140);
            Renderer.Panel(g, npc, Color.FromArgb(226, 232, 238));
            Renderer.DrawLargeFileSymbol(g, new Rectangle(npc.X + 70, npc.Y + 35, 100, 100), Color.FromArgb(190, 205, 210), false);
            using (Font f = Renderer.Font(10f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(32, 44, 62)))
                g.DrawString("상점 NPC: Binny\n\n'복구할 건 복구하고, 살 건 사세요. 단, 환불은 휴지통행입니다.'\n\nGold: " + player.Gold + " G", f, b, new Rectangle(npc.X + 20, npc.Y + 150, npc.Width - 40, 180), Renderer.Center());

            Rectangle list = new Rectangle(npc.Right + 28, c.Y + 96, c.Width - npc.Width - 88, c.Height - 140);
            Renderer.Panel(g, list, Color.White);
            string[] itemNames = new string[] { "1 HP 패치 포션 x2", "2 MP 복구 포션 x2", "3 PatchShard.pkg x5", "4 무기 수리 키트" };
            string[] descs = new string[] { "HP 포션을 2개 구매합니다.", "MP 포션을 2개 구매합니다.", "패치 조각을 5개 구매합니다.", "강화 실패 누적을 줄이고 HP를 회복합니다." };
            string[] prices = new string[] { "120 G", "140 G", "220 G", "260 G" };
            string[] actions = new string[] { "buyHp", "buyMp", "buyPatch", "buyRepair" };
            for (int i = 0; i < itemNames.Length; i++)
            {
                Rectangle row = new Rectangle(list.X + 18, list.Y + 18 + i * 78, list.Width - 36, 64);
                Renderer.Panel(g, row, Color.FromArgb(238, 244, 250));
                Renderer.DrawLargeFileSymbol(g, new Rectangle(row.X + 12, row.Y + 9, 42, 42), i == 0 ? Color.Red : i == 1 ? Color.DodgerBlue : i == 2 ? Color.Gold : Color.FromArgb(120, 150, 190), false);
                using (Font f = Renderer.Font(9.2f, FontStyle.Bold))
                using (SolidBrush b = new SolidBrush(Color.FromArgb(24, 34, 56)))
                    g.DrawString(itemNames[i], f, b, new Rectangle(row.X + 64, row.Y + 7, row.Width - 160, 20), Renderer.LeftMiddle());
                using (Font f = Renderer.Font(8.2f, FontStyle.Regular))
                using (SolidBrush b = new SolidBrush(Color.FromArgb(70, 80, 100)))
                    g.DrawString(descs[i], f, b, new Rectangle(row.X + 64, row.Y + 30, row.Width - 160, 20), Renderer.LeftMiddle());
                Rectangle buy = new Rectangle(row.Right - 104, row.Y + 14, 88, 36);
                Renderer.Button(g, buy, prices[i], player.Gold >= (i == 0 ? 120 : i == 1 ? 140 : i == 2 ? 220 : 260));
                buttons.Add(new UiButton(buy, actions[i]));
            }
            Rectangle back = new Rectangle(c.Right - 170, c.Bottom - 54, 140, 38);
            Renderer.Button(g, back, "ESC / 뒤로", false);
            buttons.Add(new UiButton(back, "taecho"));
        }

        private Player MakePreviewPlayer(float x, float y)
        {
            Player preview = new Player();
            preview.ApplyJob(player.Job);
            preview.X = x;
            preview.Y = y;
            preview.Facing = 1;
            return preview;
        }

        private void DrawDungeon(Graphics g)
        {
            Rectangle view = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height - TaskbarHeight);
            Renderer.DrawDungeonBackground(g, view, currentDungeon, cameraX, tick);
            using (Font f = Renderer.Font(9f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(200, 230, 255)))
                g.DrawString("C:\\WindowsKingdom\\Dungeons\\" + currentDungeon.FileName, f, b, new Rectangle(18, 12, ClientSize.Width - 36, 24), Renderer.LeftMiddle());

            for (int i = 0; i < platforms.Count; i++) Renderer.DrawPlatform(g, platforms[i], cameraX);
            DrawHiddenFolder(g);
            DrawDungeonNpc(g);
            for (int i = 0; i < monsters.Count; i++) if (monsters[i].Hp > 0) Renderer.DrawMonster(g, monsters[i], cameraX, tick);
            Renderer.DrawPlayer(g, player, cameraX, tick, moving);
            for (int i = 0; i < effects.Count; i++) Renderer.DrawEffect(g, effects[i], cameraX);
            DrawComboOverlay(g);
            DrawDungeonHud(g);
            DrawDungeonGuide(g);
            DrawTaskbar(g, currentDungeon.DisplayName + " - 던전 진행 중");
            bool allDead = true;
            for (int i = 0; i < monsters.Count; i++) if (monsters[i].Hp > 0) allDead = false;
            if (allDead) DrawExitPortal(g);
        }

        private void DrawComboOverlay(Graphics g)
        {
            if (comboCount < 2 || comboTimer <= 0) return;
            Rectangle box = new Rectangle(ClientSize.Width / 2 - 145, 74, 290, 56);
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(105, 0, 0, 0))) g.FillRectangle(bg, box);
            using (Pen p = new Pen(comboCount >= 10 ? Color.Gold : Color.FromArgb(80, 220, 255), 2f)) g.DrawRectangle(p, box);
            using (Font f = Renderer.Font(22f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(comboCount >= 10 ? Color.Gold : Color.White))
                g.DrawString(comboCount + " COMBO", f, b, new Rectangle(box.X, box.Y + 4, box.Width, 30), Renderer.Center());
            using (Font f = Renderer.Font(8f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(220, 230, 255)))
                g.DrawString("연속 디버깅 보너스 적용 중", f, b, new Rectangle(box.X, box.Y + 35, box.Width, 18), Renderer.Center());
        }

        private void DrawHiddenFolder(Graphics g)
        {
            if (currentDungeon == null || hiddenFolderFound) return;
            float hx = GetHiddenFolderX() - cameraX;
            float hy = GetHiddenFolderY() - 78;
            if (hx < -120 || hx > ClientSize.Width + 120) return;
            Rectangle r = new Rectangle((int)hx - 45, (int)hy, 90, 64);
            using (SolidBrush glow = new SolidBrush(Color.FromArgb(55, Color.Gold))) g.FillEllipse(glow, r.X - 28, r.Y - 18, r.Width + 56, r.Height + 50);
            Renderer.Panel(g, r, Color.FromArgb(250, 242, 186));
            using (SolidBrush tab = new SolidBrush(Color.FromArgb(255, 218, 76))) g.FillRectangle(tab, r.X + 10, r.Y + 10, 36, 14);
            using (SolidBrush body = new SolidBrush(Color.FromArgb(255, 205, 58))) g.FillRectangle(body, r.X + 8, r.Y + 20, r.Width - 16, r.Height - 24);
            using (Pen p = new Pen(Color.FromArgb(120, 84, 10), 2f)) g.DrawRectangle(p, r.X + 8, r.Y + 20, r.Width - 16, r.Height - 24);
            using (Font f = Renderer.Font(7.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(80, 48, 0)))
                g.DrawString("HiddenFolder", f, b, new Rectangle(r.X - 10, r.Bottom + 2, r.Width + 20, 16), Renderer.Center());
        }

        private void DrawDungeonHud(Graphics g)
        {
            Rectangle hud = new Rectangle(12, 42, 330, 132);
            Renderer.Panel(g, hud, Color.FromArgb(235, 240, 247));
            Renderer.Header(g, new Rectangle(hud.X + 5, hud.Y + 5, hud.Width - 10, 26), "용사 상태");
            using (Font f = Renderer.Font(9f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(30, 42, 64)))
            {
                g.DrawString(player.Name + " / " + player.DisplayJobName + "  Lv." + player.Level + "  " + player.WeaponName, f, b, new Rectangle(hud.X + 15, hud.Y + 38, hud.Width - 30, 18), Renderer.LeftMiddle());
            }
            DrawHudBar(g, "HP", player.Hp, player.MaxHp, Color.LimeGreen, hud.X + 52, hud.Y + 62);
            DrawHudBar(g, "MP", player.Mp, player.MaxMp, Color.DodgerBlue, hud.X + 52, hud.Y + 84);
            DrawHudBar(g, "EXP", player.Exp, player.NextExp, Color.Cyan, hud.X + 52, hud.Y + 106);

            Rectangle item = new Rectangle(ClientSize.Width - 310, 42, 296, 146);
            Renderer.Panel(g, item, Color.FromArgb(235, 240, 247));
            Renderer.Header(g, new Rectangle(item.X + 5, item.Y + 5, item.Width - 10, 24), "스킬 / 아이템");
            using (Font f = Renderer.Font(8.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(30, 42, 64)))
            {
                string skills = "Q 기본" + CoolText(attackCooldown) + "   W 대표" + CoolText(skillCooldown) + "\n" +
                                "E 보조" + CoolText(skill2Cooldown) + "   R 궁극" + CoolText(ultimateCooldown);
                string items = "D HP포션: " + player.Potion + "   F MP포션: " + player.MpPotion;
                string money = "Gold " + player.Gold + " G   패치 " + player.PatchShards;
                string fun = "Combo " + comboCount + " / Max " + maxCombo + "   업적 " + achievements.Count;
                g.DrawString(skills + "\n" + items + "\n" + money + "\n" + fun, f, b, new Rectangle(item.X + 14, item.Y + 36, item.Width - 28, item.Height - 42), Renderer.Left());
            }
        }

        private string CoolText(int cooldown)
        {
            return cooldown <= 0 ? "" : "(" + Math.Max(1, cooldown / 10).ToString() + ")";
        }

        private void DrawHudBar(Graphics g, string label, int val, int max, Color c, int x, int y)
        {
            using (Font f = Renderer.Font(8f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(30, 42, 64)))
                g.DrawString(label, f, b, new Rectangle(x - 38, y - 2, 36, 16), Renderer.LeftMiddle());
            Renderer.Bar(g, new Rectangle(x, y, 168, 14), val, max, c);
            using (Font f = Renderer.Font(7.5f, FontStyle.Regular))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(30, 42, 64)))
                g.DrawString(val + "/" + max, f, b, new Rectangle(x + 174, y - 2, 80, 16), Renderer.LeftMiddle());
        }

        private void DrawDungeonGuide(Graphics g)
        {
            Rectangle guide = new Rectangle(ClientSize.Width / 2 - 390, ClientSize.Height - TaskbarHeight - 55, 780, 38);
            Renderer.Panel(g, guide, Color.FromArgb(238, 242, 248));
            using (Font f = Renderer.Font(9f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(32, 46, 70)))
                g.DrawString("←/→ 이동   Space/↑ 점프   Q/W/E/R 공격·스킬   D HP포션   F MP포션   ESC 파일 선택   |   Windows 파일 내부 던전", f, b, guide, Renderer.Center());
        }

        private void DrawExitPortal(Graphics g)
        {
            float x = currentDungeon.MapWidth - 95 - cameraX;
            float y = 492;
            using (SolidBrush aura = new SolidBrush(Color.FromArgb(80, currentDungeon.Accent))) g.FillEllipse(aura, x - 50, y - 92, 100, 120);
            using (Pen p = new Pen(Color.White, 4f)) g.DrawEllipse(p, x - 36, y - 82, 72, 100);
            using (Pen p = new Pen(currentDungeon.Accent, 8f)) g.DrawEllipse(p, x - 28, y - 74, 56, 86);
            using (Font f = Renderer.Font(9f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.White))
                g.DrawString("CLEAR PORTAL", f, b, new RectangleF(x - 70, y + 20, 140, 22), Renderer.Center());
        }

        private void DrawResult(Graphics g)
        {
            DrawDesktop(g, "던전 클리어 보상");
            Rectangle app = AppRect();
            DrawWindowFrame(g, app, "전투 보상 / 던전 클리어");
            Rectangle c = new Rectangle(app.X + 10, app.Y + 70, app.Width - 20, app.Height - 80);
            Renderer.Panel(g, c, Color.FromArgb(232, 238, 246));
            using (Font f = Renderer.Font(32f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(24, 60, 150)))
                g.DrawString("던전 클리어!", f, b, new Rectangle(c.X + 20, c.Y + 40, c.Width - 40, 70), Renderer.Center());
            Rectangle reward = new Rectangle(c.X + c.Width / 2 - 310, c.Y + 140, 620, 300);
            Renderer.Panel(g, reward, Color.FromArgb(245, 248, 252));
            Renderer.DrawLargeFileSymbol(g, new Rectangle(reward.X + 60, reward.Y + 72, 110, 110), currentDungeon.Accent, false);
            using (Font f = Renderer.Font(11.2f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(30, 42, 64)))
                g.DrawString(resultText, f, b, new Rectangle(reward.X + 190, reward.Y + 32, reward.Width - 230, reward.Height - 48), Renderer.Left());
            Rectangle ok = new Rectangle(c.X + c.Width / 2 - 90, reward.Bottom + 30, 180, 44);
            Renderer.Button(g, ok, "확인", true);
            buttons.Add(new UiButton(ok, "resultOk"));
        }



        private void DrawStoryDialog(Graphics g)
        {
            DrawDesktop(g, "오류 메시지 대화");
            Rectangle app = AppRect();
            DrawWindowFrame(g, app, "ErrorDialog_Story.exe");
            Rectangle c = new Rectangle(app.X + 18, app.Y + 82, app.Width - 36, app.Height - 104);
            Renderer.Panel(g, c, Color.FromArgb(236, 240, 248));
            Rectangle title = new Rectangle(c.X + 22, c.Y + 22, c.Width - 44, 44);
            Renderer.Header(g, title, storyTitle);
            Rectangle body = new Rectangle(c.X + 42, c.Y + 92, c.Width - 84, c.Height - 190);
            Renderer.Inset(g, body, Color.White);
            using (Font f = Renderer.Font(11.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(30, 42, 64)))
                g.DrawString(storyBody, f, b, new Rectangle(body.X + 18, body.Y + 18, body.Width - 36, body.Height - 36), Renderer.Left());
            using (Font f = Renderer.Font(9f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.Firebrick))
                g.DrawString("※ 이 대화는 오류 메시지입니다. 닫아도 감정은 남습니다.", f, b, new Rectangle(c.X + 44, body.Bottom + 16, c.Width - 88, 24), Renderer.Center());
            Rectangle ok = new Rectangle(c.Right - 210, c.Bottom - 62, 170, 40);
            Renderer.Button(g, ok, "계속", true);
            buttons.Add(new UiButton(ok, "storyContinue"));
        }

        private void DrawCompanionChoice(Graphics g)
        {
            DrawDesktop(g, "파일 감정 선택");
            Rectangle app = AppRect();
            DrawWindowFrame(g, app, "CompanionChoice.properties");
            Rectangle c = new Rectangle(app.X + 18, app.Y + 82, app.Width - 36, app.Height - 104);
            Renderer.Panel(g, c, Color.FromArgb(236, 240, 248));
            string name = GetCompanionName(lastClearedDungeonType);
            Renderer.Header(g, new Rectangle(c.X + 22, c.Y + 22, c.Width - 44, 42), "감정 있는 파일 발견: " + name);
            Rectangle icon = new Rectangle(c.X + 60, c.Y + 110, 150, 150);
            DungeonInfo d = GameData.Dungeons[(int)Math.Min((int)lastClearedDungeonType, GameData.Dungeons.Count - 1)];
            Renderer.DrawLargeFileSymbol(g, icon, d.Accent, false);
            Rectangle text = new Rectangle(icon.Right + 40, c.Y + 104, c.Width - icon.Width - 130, 190);
            Renderer.Inset(g, text, Color.White);
            using (Font f = Renderer.Font(11f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(32, 46, 70)))
            {
                string msg = name + "이(가) 당신을 바라봅니다.\n\n" +
                             "오류 메시지: '나는 쓸모 없는 파일인가요, 아니면 다음 단서인가요?'\n\n" +
                             "선택은 후반 진최종 던전의 분위기와 보상 기록에 남습니다.";
                g.DrawString(msg, f, b, new Rectangle(text.X + 18, text.Y + 18, text.Width - 36, text.Height - 36), Renderer.Left());
            }
            Rectangle b1 = new Rectangle(c.X + 110, c.Bottom - 92, 220, 46);
            Rectangle b2 = new Rectangle(c.X + c.Width / 2 - 110, c.Bottom - 92, 220, 46);
            Rectangle b3 = new Rectangle(c.Right - 330, c.Bottom - 92, 220, 46);
            Renderer.Button(g, b1, "1 데리고 간다", true);
            Renderer.Button(g, b2, "2 두고 간다", false);
            Renderer.Button(g, b3, "3 삭제한다", false);
            buttons.Add(new UiButton(b1, "companionTake"));
            buttons.Add(new UiButton(b2, "companionLeave"));
            buttons.Add(new UiButton(b3, "companionDelete"));
        }

        private void DrawRewardChoice(Graphics g)
        {
            DrawDesktop(g, "보상 선택");
            Rectangle app = AppRect();
            DrawWindowFrame(g, app, "RewardChoice.dialog");
            Rectangle c = new Rectangle(app.X + 18, app.Y + 82, app.Width - 36, app.Height - 104);
            Renderer.Panel(g, c, Color.FromArgb(236, 240, 248));
            Renderer.Header(g, new Rectangle(c.X + 22, c.Y + 22, c.Width - 44, 42), "던전 클리어 보상 선택");
            using (Font f = Renderer.Font(12f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(32, 46, 70)))
            {
                string msg = "힘을 얻을지, 진실에 가까워질지 선택하세요.\n\n" +
                             "현재 무기: " + player.WeaponName + "\n" +
                             "수집한 범인 힌트: " + clueCount + "개\n" +
                             "동행 파일: " + companionCount + " / 버림: " + abandonedCount + " / 삭제: " + deletedCount + "\n\n" +
                             "NPC 404호: 둘 다 갖고 싶죠? 그럼 과제를 두 번 하세요.";
                g.DrawString(msg, f, b, new Rectangle(c.X + 60, c.Y + 100, c.Width - 120, 220), Renderer.Left());
            }
            Rectangle up = new Rectangle(c.X + 160, c.Bottom - 104, 300, 54);
            Rectangle hint = new Rectangle(c.Right - 460, c.Bottom - 104, 300, 54);
            Renderer.Button(g, up, "1 무기 강화 시도", true);
            Renderer.Button(g, hint, "2 범인 힌트 획득", false);
            buttons.Add(new UiButton(up, "rewardUpgrade"));
            buttons.Add(new UiButton(hint, "rewardHint"));
        }

        private void DrawSuspectSelect(Graphics g)
        {
            DrawDesktop(g, "범인 추리");
            Rectangle app = AppRect();
            DrawWindowFrame(g, app, "SuspectFinder.exe");
            Rectangle c = new Rectangle(app.X + 18, app.Y + 82, app.Width - 36, app.Height - 104);
            Renderer.Panel(g, c, Color.FromArgb(236, 240, 248));
            Renderer.Header(g, new Rectangle(c.X + 22, c.Y + 22, c.Width - 44, 42), "범인으로 의심되는 NPC를 선택하세요");
            using (Font f = Renderer.Font(10.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(32, 46, 70)))
            {
                string msg = "수집한 힌트 " + clueCount + "개\n" +
                             (hintHistory.Length == 0 ? "힌트를 안 모았습니다. 감으로 찍으면 태초마을입니다." : hintHistory) +
                             "\n오답을 고르면 태초마을로 돌아갑니다.";
                g.DrawString(msg, f, b, new Rectangle(c.X + 48, c.Y + 84, c.Width - 96, 150), Renderer.Left());
            }
            string[] suspects = new string[] { "NPC 404호", "오류창 여왕 Exception", "Update 아저씨", "Kernel 수호자", "휴지통 소녀 Bin" };
            for (int i = 0; i < suspects.Length; i++)
            {
                Rectangle r = new Rectangle(c.X + 110 + (i % 2) * 390, c.Y + 255 + (i / 2) * 70, 340, 48);
                Renderer.Button(g, r, (i + 1) + " " + suspects[i], i == 2 && clueCount >= 3);
                buttons.Add(new UiButton(r, "suspect" + i));
            }
        }

        private void DrawEnding(Graphics g)
        {
            DrawDesktop(g, endingTitle);
            Rectangle app = AppRect();
            DrawWindowFrame(g, app, "Ending_Log.sys");
            Rectangle c = new Rectangle(app.X + 18, app.Y + 82, app.Width - 36, app.Height - 104);
            Renderer.Panel(g, c, Color.FromArgb(236, 240, 248));
            Renderer.Header(g, new Rectangle(c.X + 22, c.Y + 22, c.Width - 44, 44), endingTitle);
            Rectangle body = new Rectangle(c.X + 60, c.Y + 96, c.Width - 120, c.Height - 180);
            Renderer.Inset(g, body, Color.White);
            using (Font f = Renderer.Font(11f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(32, 46, 70)))
                g.DrawString(endingBody, f, b, new Rectangle(body.X + 18, body.Y + 18, body.Width - 36, body.Height - 36), Renderer.Left());
            Rectangle ok = new Rectangle(c.Right - 230, c.Bottom - 66, 190, 42);
            Renderer.Button(g, ok, trueEndingSeen ? "클리어 확인" : "찐 던전으로", true);
            buttons.Add(new UiButton(ok, "endingOk"));
        }

        private void DrawHelp(Graphics g)
        {
            DrawDesktop(g, "도움말");
            Rectangle app = AppRect();
            DrawWindowFrame(g, app, "도움말");
            Rectangle c = new Rectangle(app.X + 10, app.Y + 70, app.Width - 20, app.Height - 80);
            Renderer.Panel(g, c, Color.FromArgb(234, 238, 246));
            using (Font f = Renderer.Font(14f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(24, 60, 150)))
                g.DrawString("조작법", f, b, new Rectangle(c.X + 30, c.Y + 30, c.Width - 60, 40), Renderer.LeftMiddle());
            using (Font f = Renderer.Font(11f, FontStyle.Regular))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(30, 42, 64)))
            {
                string s = "파일 선택 화면\n- 방향키: 파일 선택\n- Enter/E: 선택한 파일 실행\n\n던전 화면\n- ←/→ 방향키: 좌우 이동\n- Space/↑: 점프\n- Q: 기본 공격\n- W: 직업 대표 스킬\n- E: 보조 스킬\n- R: 궁극기\n- D: HP 포션\n- F: MP 포션\n- ESC: 파일 선택 화면으로 이동\n\n목표\n파일 안에 숨겨진 던전을 돌며 몬스터를 처치하고 패치 조각을 모아 Blue Screen Tower를 해금하세요.";
                g.DrawString(s, f, b, new Rectangle(c.X + 40, c.Y + 80, c.Width - 80, c.Height - 150), Renderer.Left());
            }
            Rectangle back = new Rectangle(c.Right - 180, c.Bottom - 62, 140, 40);
            Renderer.Button(g, back, "뒤로", false);
            buttons.Add(new UiButton(back, "backTitle"));
        }
    }
}
