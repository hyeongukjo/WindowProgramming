using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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

        private ScreenMode screen = ScreenMode.Boot;
        private readonly PlayerState player = new PlayerState();
        private int tick;
        private int bootTicks;
        private int introIndex;
        private string profileInput = "";
        private string finalInput = "";
        private int unlockedStage = 1;
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
        private bool stageBossPhase = false;
        private bool stage1BossPhase = false;
        private bool lastClearWasBoss = false;
        private readonly BossRuntime bossRuntime = new BossRuntime();
        private TaskbarUI taskbarUI;


        public MainForm()
        {
            Text = "DebugHero File Dungeon - Player.exe AntiVirus Agent";
            ClientSize = new Size(1366, 768);
            MinimumSize = new Size(1100, 680);
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;
            KeyPreview = true;
            BackColor = Color.Black;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);

            stages = GameData.CreateStages();
            timer = new Timer();
            timer.Interval = 33; // 렉 방지를 위해 60FPS -> 30FPS로 조정
            timer.Tick += Timer_Tick;
            timer.Start();

            BackgroundRenderer.InitializeBackgrounds();
            taskbarUI = new TaskbarUI();

            KeyDown += MainForm_KeyDown;
            KeyPress += MainForm_KeyPress;
            MouseDown += MainForm_MouseDown;
            MouseMove += MainForm_MouseMove;
            MouseUp += MainForm_MouseUp;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            tick++;
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                effects[i].Ticks--;
                if (effects[i].Ticks <= 0) effects.RemoveAt(i);
            }
            // 이펙트가 과도하게 쌓이면 GDI+ 그리기 부하가 커져 렉이 발생하므로 상한을 둡니다.
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
        }


        private void TryBeep(int f, int d)
        {
            try { Console.Beep(f, d); } catch { }
        }
    }
}
