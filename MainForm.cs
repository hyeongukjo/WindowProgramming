using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DebugHeroFileDungeonRPG
{
    public sealed partial class MainForm : Form
    {
        private readonly Timer timer;
        private readonly Random random = new Random();
        private readonly List<StageInfo> stages;
        private readonly List<GameEntity> enemies = new List<GameEntity>();
        private readonly List<Effect> effects = new List<Effect>();
        private readonly List<UiButton> buttons = new List<UiButton>();
        private readonly List<WeaponUpgradeFile> weaponDrops = new List<WeaponUpgradeFile>();
        private WeaponUpgradeFile draggedWeaponDrop;

        //private ScreenMode screen = ScreenMode.Boot;
        private readonly PlayerState player = new PlayerState();
        private ScreenMode _screen = ScreenMode.StartMenu;
        private ScreenMode screen
        {
            get => _screen;
            set
            {
                bool isChanged = _screen != value;
                _screen = value;

              
                if (isChanged && _screen == ScreenMode.Desktop)
                {
                    UpdateAllBossRankings(); // 즉시 서버 최신 랭킹판 강제 동기화 트리거
                }
                if (isChanged && _screen != ScreenMode.Desktop)
                {
                    showOldGoogleWindow = false;
                    oldGoogleSearchFocused = false;

                }

            }
        }
        private int tick;
        private int bootTicks;
        private int introIndex;
        private string profileInput = "";
        private string finalInput = "";
        private int unlockedStage = 10;
        private int selectedStage = 1;
        private string selectedShopItem = "hp";
        private int currentStage = 0;
        private int clearStage = 0;
        private float cameraX;
        private int stageTime;
        private int totalGameTime = 0;
        private bool showLeaderboardWindow = false;
        private readonly string leaderboardCloseKey = "leaderboardWinClose";
        private string[] stageRankJsonCache = { "[]", "[]", "[]", "[]", "[]" };
        private readonly int[] trackingBossStages = { 2, 4, 6, 8, 10 };
        private string endingTitle = "";
        private string endingBody = "";
        private bool firstDesktopNotice = true;
        private bool stageNpcHintClosed = false;
        private int stageNpcHintIndex = 0;
        private bool ignoreEnterUntilKeyUp = false;
        private bool stageBossPhase = false;
        private bool stage1BossPhase = false;
        private bool lastClearWasBoss = false;
        private readonly BossRuntime bossRuntime = new BossRuntime();
        private TaskbarUI taskbarUI;
        private BossPatternManager bossManager;
        private bool showStageClearPopup = false;
        private int popupBonusCoins = 0;
        private Rectangle popupConfirmBtnBounds;
       
        private int wBuffTicks = 0;       // W 버프 남은 시간 (60틱 = 1초)
        private int playerShield = 0;     // E 보호막 현재 내구도
        private int eShieldDurationTicks = 0;
        private readonly List<PlayerSkySword> playerSkySwords = new List<PlayerSkySword>();


        private int lastPlayerHpForMotion = -1;
        private int playerHitMotionCooldown = 0;
        private bool playerDeathSequenceActive = false;
        private int playerDeathSequenceTicks = 0;


        // 스킬별 독립 쿨타임 카운터 변수
        private int wCooldownTicks = 0;   // W 쿨타임 (15초 = 900틱)
        private int eCooldownTicks = 0;   // E 쿨타임 (15초 = 900틱)
        private int rCooldownTicks = 0;   // R 쿨타임 (25초 = 1500틱)

        // R 궁극기 투하 연산을 위한 내부 클래스 정의
        public class PlayerSkySword
        {
            public float X, Y;
            public int Timer;
            public int MaxTimer;
            public string SwordType; // "dark" 또는 "cold"
        }

        [System.Runtime.InteropServices.DllImport("winmm.dll")]
        private static extern int mciSendString(string strCommand, System.Text.StringBuilder strReturn, int iReturnLength, IntPtr hwndCallback);

        private int cutsceneTicks = 0; // 컷씬이 상영될 총 프레임 시간 (60틱 = 1초)

        public MainForm()
        {
            bossManager = bossRuntime.patternManager;
            Text = "DebugHero File Dungeon - Player.exe AntiVirus Agent";
            ClientSize = new Size(1366, 768);
            MinimumSize = new Size(1100, 680);
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;
            KeyPreview = true;
            BackColor = Color.Black;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();
            stages = GameData.CreateStages();
            NpcDialogueData.ApplyToStages(stages);
            timer = new Timer();
            timer.Interval = 16;
            timer.Tick += Timer_Tick;
            timer.Start();

            BackgroundRenderer.InitializeBackgrounds();
            taskbarUI = new TaskbarUI();
            InitializeOldGoogleWindow();
            KeyDown += MainForm_KeyDown;
            KeyPress += MainForm_KeyPress;
            MouseDown += MainForm_MouseDown;
            MouseMove += MainForm_MouseMove;
            MouseUp += MainForm_MouseUp;

            string bossPath = System.IO.Path.Combine(Application.StartupPath, "Resources", "boss");
            if (!System.IO.Directory.Exists(bossPath)) System.IO.Directory.CreateDirectory(bossPath);

            // [추가] Driver-K 로드
            string dkPath = System.IO.Path.Combine(bossPath, "driver_k.png");
            if (System.IO.File.Exists(dkPath)) Renderer.ImgBoss_DriverK = Image.FromFile(dkPath);

            // [추가] BSOD 로드
            string bsodPath = System.IO.Path.Combine(bossPath, "bsod_dragon.png");
            if (System.IO.File.Exists(bsodPath)) Renderer.ImgBoss_BSOD = Image.FromFile(bsodPath);
            // [추가] High Kernel 로드
            string hkPath = System.IO.Path.Combine(bossPath, "high_kernel.png");
            if (System.IO.File.Exists(hkPath))
            {
                Renderer.ImgBoss_HighKernel = Image.FromFile(hkPath);
            }
            // [추가] Exception Queen 로드
            string eqPath = System.IO.Path.Combine(bossPath, "Exception_Queen.png");
            if (System.IO.File.Exists(eqPath))
            {
                Renderer.ImgBoss_ExceptionQueen = Image.FromFile(eqPath);
            }
            // [추가] Illegal Binny 로드
            string binnyPath = System.IO.Path.Combine(bossPath, "Illegal_Binny.png");
            if (System.IO.File.Exists(binnyPath))
            {
                Renderer.ImgBoss_IllegalBinny = Image.FromFile(binnyPath);
            }
            string diskPath = Path.Combine(bossPath, "disk_sprites.png");
            if (File.Exists(diskPath))
            {
                // 외부 사진을 GDI+ 이미지 객체로 변환하여 렌더러에 적재합니다.
                Renderer.Img_DiskSprite = Image.FromFile(diskPath);
            }
            // High-Kernel 기믹 이미지 로드
            string meteorPath = Path.Combine(bossPath, "Meteor.png");
            if (File.Exists(meteorPath)) Renderer.Img_Meteor = Image.FromFile(meteorPath);

            string meteor2Path = Path.Combine(bossPath, "Meteor2.png");
            if (File.Exists(meteor2Path)) Renderer.Img_Meteor2 = Image.FromFile(meteor2Path);

            string safePath = Path.Combine(bossPath, "safezone.png");
            if (File.Exists(safePath)) Renderer.Img_Safezone = Image.FromFile(safePath);

            string iceSwordPath = Path.Combine(bossPath, "ice_sword.png");
            if (File.Exists(iceSwordPath)) Renderer.Img_IceSword = Image.FromFile(iceSwordPath);

            string fireSwordPath = Path.Combine(bossPath, "fire_sword.png");
            if (File.Exists(fireSwordPath)) Renderer.Img_FireSword = Image.FromFile(fireSwordPath);

            string lightSwordPath = Path.Combine(bossPath, "lightning_sword.png");
            if (File.Exists(lightSwordPath)) Renderer.Img_LightningSword = Image.FromFile(lightSwordPath);
            
            
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            tick++;
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                effects[i].Ticks--;
                if (effects[i].Ticks <= 0) effects.RemoveAt(i);
            }
            if (effects.Count > 60) effects.RemoveRange(0, effects.Count - 60);

            if (screen == ScreenMode.Boot)
            {
                bootTicks++;
                if (bootTicks > 170) screen = ScreenMode.AssistantIntro;
            }
            else if (screen == ScreenMode.Cutscene)
            {
                if (cutsceneTicks > 0)
                {
                    cutsceneTicks--;
                    if (cutsceneTicks <= 0)
                    {
                        EndStage10Cutscene(); // 영상 시간이 끝나면 자동으로 보스전 이관
                    }
                }
            
                return;
                // ==========================================================
            }
            else if (screen == ScreenMode.Stage)
            {
                if (playerDeathSequenceActive)
                {
                    PlayerMovementSystem.UpdateActionAnimation(player);
                    playerDeathSequenceTicks++;

                    if (playerDeathSequenceTicks >= 55)
                    {
                        FinishPlayerDeathSequence();
                    }

                    Invalidate();
                    return;
                }

                if (IsStageNpcHintOpen())
                {
                    Invalidate();
                    return;
                }

                if (tick % 45 == 0 && player.Mp < player.MaxMp)
                    player.Mp = Math.Min(player.MaxMp, player.Mp + 1);

                UpdateStage();
            }


            if (screen == ScreenMode.Desktop && tick % 180 == 0)
            {
                UpdateAllBossRankings();
            }


            // 일반 인게임 상태에서만 도화지를 새로 고침 (컷씬 모드일 땐 위에서 return되어 실행 안 됨)
            Invalidate();

            if (bossRuntime.patternManager.PlayerSlowTicks > 0)
            {
                bossRuntime.patternManager.PlayerSlowTicks--;
            }
            if (bossRuntime.patternManager.PlayerBurnTicks > 0)
            {
                bossRuntime.patternManager.PlayerBurnTicks--;
                if (bossRuntime.patternManager.PlayerBurnTicks % 30 == 0) // 1초당(30틱) 최대 체력의 5% 도트 연산
                {
                    player.Hp -= (int)(player.MaxHp * 0.05f);
                    effects.Add(new Effect("text", player.X, player.Y - 60, player.X, player.Y - 60, 20, Color.DarkRed, "화상 피해"));
                }
            }

            HandlePlayerHpMotion(); //사망처리 코드 수정

            if (playerDeathSequenceActive)
            {
                PlayerMovementSystem.UpdateActionAnimation(player);
                playerDeathSequenceTicks++;


                if (playerDeathSequenceTicks >= 55)
                {
                    FinishPlayerDeathSequence();
                }
                Invalidate();
                return;
            }
        }
        private void HandlePlayerHpMotion()
        {
            if (screen != ScreenMode.Stage)
            {
                lastPlayerHpForMotion = player.Hp;
                return;
            }

            if (lastPlayerHpForMotion < 0)
                lastPlayerHpForMotion = player.Hp;

            if (playerHitMotionCooldown > 0)
                playerHitMotionCooldown--;

            if (player.Hp < lastPlayerHpForMotion && player.Hp > 0)
            {
                if (playerHitMotionCooldown <= 0 && player.ActionState != PlayerActionState.Die)
                {
                    PlayerMovementSystem.StartHitAnimation(player);
                    playerHitMotionCooldown = 12;
                }
            }

            if (player.Hp <= 0 && !playerDeathSequenceActive)
            {
                player.Hp = 0;
                playerDeathSequenceActive = true;
                playerDeathSequenceTicks = 0;

                player.TotalDeaths++;

                player.ActionState = PlayerActionState.Die; // 상태를 사망으로 강제 전환
                player.ActionFrame = 0;                     // 프레임 리셋
                player.ActionTick = 0;
                player.SkillIndex = -1;

                player.MoveVelocityX = 0f;
                player.MoveVelocityY = 0f;
                player.TargetX = player.X;
                player.TargetY = player.Y;


                PlayerMovementSystem.StartDeathAnimation(player);

                effects.Add(new Effect(
                    "text",
                    player.X,
                    player.Y - 92,
                    player.X,
                    player.Y - 92,
                    70,
                    Color.DarkRed,
                    "PROCESS TERMINATED"
                ));

                TryBeep(440, 300);
            }

            lastPlayerHpForMotion = player.Hp;
        }
        private void FinishPlayerDeathSequence()
        {
            currentStage = 0;

            enemies.Clear();
            effects.Clear();
            weaponDrops.Clear();
            draggedWeaponDrop = null;

            stageBossPhase = false;
            stage1BossPhase = false;
            showStageClearPopup = false;

            bossRuntime.patternManager.IsIllusionActive = false;
            bossRuntime.patternManager.BinnyClone = null;

            playerDeathSequenceActive = false;
            playerDeathSequenceTicks = 0;
            playerHitMotionCooldown = 0;

            player.Hp = player.MaxHp;
            lastPlayerHpForMotion = player.Hp;

            player.ActionState = PlayerActionState.Idle;
            player.ActionFrame = 0;
            player.ActionTick = 0;
            player.SkillIndex = -1;

            screen = ScreenMode.Desktop;
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (playerDeathSequenceActive) return;

            // 인게임 스테이지 작동 중이면서 보스 페이즈일 때 실시간 타이핑 해킹 입력 연동
            if (screen == ScreenMode.Stage && stageBossPhase)
            {
                bossRuntime.patternManager.HandleTypingInput(
                    e.KeyCode,
                    player,
                    effects,
                    StageFlowRules.GetStageMapWidth(stages[currentStage - 1], true, ClientSize.Width)
                );
            }
        }


        private void TryBeep(int f, int d)
        {
            try { Console.Beep(f, d); } catch { }
        }

        private async void UpdateAllBossRankings()
        {
            Task<string>[] tasks = new Task<string>[5];
            for (int i = 0; i < 5; i++)
            {
                tasks[i] = SupabaseManager.GetStageRankingsAsync(trackingBossStages[i]);
            }

            stageRankJsonCache = await Task.WhenAll(tasks);
            Invalidate(); // 도화지 실시간 다시 그리기 트리거
        }
    }
}
