using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
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
        private ScreenMode screen = ScreenMode.StartMenu;
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
        private string endingTitle = "";
        private string endingBody = "";
        private bool firstDesktopNotice = true;
        private bool stageNpcHintClosed = false;
        private int stageNpcHintIndex = 0;
        private bool stageBossPhase = false;
        private bool stage1BossPhase = false;
        private bool lastClearWasBoss = false;
        private readonly BossRuntime bossRuntime = new BossRuntime();
        private TaskbarUI taskbarUI;
        private BossPatternManager bossManager;
        private bool showStageClearPopup = false;
        private int popupBonusCoins = 0;
        private Rectangle popupConfirmBtnBounds;




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

            stages = GameData.CreateStages();
            NpcDialogueData.ApplyToStages(stages);
            timer = new Timer();
            timer.Interval = 16;
            timer.Tick += Timer_Tick;
            timer.Start();

            BackgroundRenderer.InitializeBackgrounds();
            taskbarUI = new TaskbarUI();

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
            else if (screen == ScreenMode.Stage)
            {
                if (tick % 45 == 0 && player.Mp < player.MaxMp) player.Mp = Math.Min(player.MaxMp, player.Mp + 1);
                UpdateStage();
            }
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

            // ==========================================================
            // 💡 [통합 사망 처리 엔진] 어떤 원인으로든 HP 유실 시 스테이지 튕김 처리
            // ==========================================================
            if (screen == ScreenMode.Stage && player.Hp <= 0)
            {
                // 윈도우 에러 알림 비프음 작동 (컨셉 강화)
                TryBeep(440, 300);

                // 진행 중이던 스테이지 상태 및 몬스터/이펙트 풀 완전 리셋
                currentStage = 0;
                enemies.Clear();
                effects.Clear();
                weaponDrops.Clear();
                stageBossPhase = false;
                stage1BossPhase = false;

                // 1% 보스 패턴 관련 내부 데스링크 플래그도 안전하게 완전 청소
                bossRuntime.patternManager.IsIllusionActive = false;
                bossRuntime.patternManager.BinnyClone = null;

                // ⚠️ 스폰되지 않고 즉시 바탕화면(스테이지 선택 화면)으로 강제 퇴장
                screen = ScreenMode.Desktop;
                return;
            }
            // ==========================================================
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

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
    }
}
