using System;
using System.Collections.Generic;
using System.Drawing;

namespace DebugHeroFileDungeonRPG
{
    public enum GameScreen
    {
        Title,
        JobSelect,
        HeroIntro,
        FileSelect,
        ItemShop,
        Dungeon,
        Result,
        StoryDialog,
        CompanionChoice,
        RewardChoice,
        SuspectSelect,
        Ending,
        Help
    }

    public enum JobType
    {
        DebugWarrior = 0,
        VaccineMage = 1,
        FirewallKnight = 2,
        FileExplorer = 3
    }

    public enum DungeonType
    {
        FileExplorerForest,
        PopupErrorZone,
        RecycleBinDungeon,
        UpdateLab,
        ControlPanelCastle,
        TempCacheCave,
        NetworkPort,
        RegistryHive,
        DriverVault,
        System32Forbidden,
        BlueScreenTower,
        UserCoreTrueFault
    }

    public enum MonsterKind
    {
        Slime,
        Goblin,
        Bat,
        Wolf,
        Skeleton,
        Ghost,
        Spider,
        Golem,
        Hound,
        Serpent,
        Dragon
    }

    public enum EffectKind
    {
        Slash,
        Projectile,
        SkillBurst,
        HitSpark,
        Heal,
        Guard,
        Text,
        ScanLine
    }

    public sealed class JobProfile
    {
        public JobType Type;
        public string Name;
        public string Role;
        public string SkillName;
        public string Description;
        public Color MainColor;
        public int StartHp;
        public int StartMp;
        public int StartAttack;
        public int StartDefense;
        public int StartSpeed;
        public int StartLuck;
        public int GrowthHp;
        public int GrowthMp;
        public int GrowthAttack;
        public int GrowthDefense;
        public int GrowthSpeed;
        public int GrowthLuck;

        public JobProfile(JobType type, string name, string role, string skillName, string description, Color mainColor,
            int startHp, int startMp, int startAttack, int startDefense, int startSpeed, int startLuck,
            int growthHp, int growthMp, int growthAttack, int growthDefense, int growthSpeed, int growthLuck)
        {
            Type = type;
            Name = name;
            Role = role;
            SkillName = skillName;
            Description = description;
            MainColor = mainColor;
            StartHp = startHp;
            StartMp = startMp;
            StartAttack = startAttack;
            StartDefense = startDefense;
            StartSpeed = startSpeed;
            StartLuck = startLuck;
            GrowthHp = growthHp;
            GrowthMp = growthMp;
            GrowthAttack = growthAttack;
            GrowthDefense = growthDefense;
            GrowthSpeed = growthSpeed;
            GrowthLuck = growthLuck;
        }
    }

    public sealed class Player
    {
        public string Name = "디버그용사";
        public JobType Job = JobType.DebugWarrior;
        public int Level = 1;
        public int Exp = 0;
        public int NextExp = 100;
        public int Gold = 300;
        public int PatchShards = 0;
        public int Potion = 5;
        public int MpPotion = 3;
        public int Hp = 120;
        public int MaxHp = 120;
        public int Mp = 40;
        public int MaxMp = 40;
        public int Attack = 20;
        public int Defense = 8;
        public int Speed = 8;
        public int Luck = 8;
        public float X = 120;
        public float Y = 300;
        public float VX = 0;
        public float VY = 0;
        public int Facing = 1;
        public bool OnGround = false;
        public int InvincibleTicks = 0;
        public int ShieldTicks = 0;
        public int JobTier = 0;
        public int WeaponLevel = 1;
        public int FailedUpgrades = 0;
        public int LastGainHp = 0;
        public int LastGainMp = 0;
        public int LastGainAttack = 0;
        public int LastGainDefense = 0;
        public int LastGainSpeed = 0;
        public int LastGainLuck = 0;

        public RectangleF Bounds
        {
            get { return new RectangleF(X - 24, Y - 56, 48, 56); }
        }

        public JobProfile Profile
        {
            get { return GameData.GetJob(Job); }
        }

        public void ApplyJob(JobType job)
        {
            Job = job;
            JobProfile p = Profile;
            Level = 1;
            Exp = 0;
            NextExp = 100;
            Gold = 300;
            PatchShards = 0;
            Potion = 5;
            MpPotion = 3;
            MaxHp = p.StartHp;
            MaxMp = p.StartMp;
            Hp = MaxHp;
            Mp = MaxMp;
            Attack = p.StartAttack;
            Defense = p.StartDefense;
            Speed = p.StartSpeed;
            Luck = p.StartLuck;
            X = 120;
            Y = 300;
            VX = 0;
            VY = 0;
            Facing = 1;
            ShieldTicks = 0;
            InvincibleTicks = 0;
            JobTier = 0;
            WeaponLevel = 1;
            FailedUpgrades = 0;
        }


        public string AdvancedJobName
        {
            get
            {
                if (Job == JobType.DebugWarrior) return "코드 파괴자";
                if (Job == JobType.VaccineMage) return "백신 대마법사";
                if (Job == JobType.FirewallKnight) return "커널 방화벽 기사";
                return "레어파일 추적자";
            }
        }

        public string DisplayJobName
        {
            get { return JobTier > 0 ? AdvancedJobName : Profile.Name; }
        }

        public string WeaponName
        {
            get
            {
                string prefix;
                if (Job == JobType.DebugWarrior) prefix = "DebugBlade";
                else if (Job == JobType.VaccineMage) prefix = "VaccineStaff";
                else if (Job == JobType.FirewallKnight) prefix = "FirewallLance";
                else prefix = "ExplorerScanner";

                string ext;
                if (WeaponLevel >= 9) ext = ".legend";
                else if (WeaponLevel >= 6) ext = ".dll";
                else if (WeaponLevel >= 3) ext = ".exe";
                else ext = ".tmp";
                return prefix + "+" + WeaponLevel + ext;
            }
        }

        public void ApplyWeaponUpgrade()
        {
            WeaponLevel++;
            Attack += 2 + WeaponLevel / 3;
            if (WeaponLevel % 3 == 0) Defense += 1;
            if (WeaponLevel % 4 == 0)
            {
                MaxMp += 3;
                Mp = Math.Min(MaxMp, Mp + 3);
            }
        }

        public bool AddExp(int amount)
        {
            bool leveled = false;
            LastGainHp = 0;
            LastGainMp = 0;
            LastGainAttack = 0;
            LastGainDefense = 0;
            LastGainSpeed = 0;
            LastGainLuck = 0;
            Exp += Math.Max(0, amount);
            while (Exp >= NextExp)
            {
                Exp -= NextExp;
                Level++;
                JobProfile p = Profile;
                MaxHp += p.GrowthHp;
                MaxMp += p.GrowthMp;
                Attack += p.GrowthAttack;
                Defense += p.GrowthDefense;
                Speed += p.GrowthSpeed;
                Luck += p.GrowthLuck;
                LastGainHp += p.GrowthHp;
                LastGainMp += p.GrowthMp;
                LastGainAttack += p.GrowthAttack;
                LastGainDefense += p.GrowthDefense;
                LastGainSpeed += p.GrowthSpeed;
                LastGainLuck += p.GrowthLuck;
                Hp = MaxHp;
                Mp = MaxMp;
                NextExp = 110 + Level * 85;
                leveled = true;
            }
            return leveled;
        }
    }

    public sealed class DungeonInfo
    {
        public DungeonType Type;
        public string DisplayName;
        public string FileName;
        public string Description;
        public int RecommendedLevel;
        public int RequiredPatch;
        public int MapWidth;
        public Color Accent;
        public Color BackColor;

        public DungeonInfo(DungeonType type, string displayName, string fileName, string description,
            int recommendedLevel, int requiredPatch, int mapWidth, Color accent, Color backColor)
        {
            Type = type;
            DisplayName = displayName;
            FileName = fileName;
            Description = description;
            RecommendedLevel = recommendedLevel;
            RequiredPatch = requiredPatch;
            MapWidth = mapWidth;
            Accent = accent;
            BackColor = backColor;
        }
    }

    public sealed class Platform
    {
        public RectangleF Bounds;
        public Color Color;
        public string Label;

        public Platform(float x, float y, float w, float h, Color color, string label)
        {
            Bounds = new RectangleF(x, y, w, h);
            Color = color;
            Label = label;
        }
    }

    public sealed class Monster
    {
        public MonsterKind Kind;
        public string Name;
        public string KoreanName;
        public int Level;
        public int Hp;
        public int MaxHp;
        public int Attack;
        public int Exp;
        public int Gold;
        public bool IsBoss;
        public float X;
        public float Y;
        public float VX;
        public int Facing = -1;
        public int HitFlash = 0;
        public int AttackCooldown = 0;
        public Color MainColor;

        public Monster(MonsterKind kind, string name, string koreanName, int level, int hp, int attack, int exp, int gold, bool boss, float x, float y, Color mainColor)
        {
            Kind = kind;
            Name = name;
            KoreanName = koreanName;
            Level = level;
            Hp = hp;
            MaxHp = hp;
            Attack = attack;
            Exp = exp;
            Gold = gold;
            IsBoss = boss;
            X = x;
            Y = y;
            VX = boss ? 1.1f : 1.3f;
            MainColor = mainColor;
        }

        public RectangleF Bounds
        {
            get
            {
                if (IsBoss) return new RectangleF(X - 86, Y - 118, 172, 118);
                return new RectangleF(X - 32, Y - 58, 64, 58);
            }
        }
    }

    public sealed class Effect
    {
        public EffectKind Kind;
        public float X;
        public float Y;
        public float X2;
        public float Y2;
        public int Ticks;
        public int MaxTicks;
        public Color Color;
        public string Text;
        public int Direction;

        public Effect(EffectKind kind, float x, float y, float x2, float y2, int ticks, Color color, string text, int direction)
        {
            Kind = kind;
            X = x;
            Y = y;
            X2 = x2;
            Y2 = y2;
            Ticks = ticks;
            MaxTicks = Math.Max(1, ticks);
            Color = color;
            Text = text;
            Direction = direction;
        }
    }

    public sealed class UiButton
    {
        public Rectangle Bounds;
        public string Action;

        public UiButton(Rectangle bounds, string action)
        {
            Bounds = bounds;
            Action = action;
        }
    }

    public static class GameData
    {
        private static List<JobProfile> jobs;
        private static List<DungeonInfo> dungeons;

        public static List<JobProfile> Jobs
        {
            get
            {
                if (jobs == null) jobs = CreateJobs();
                return jobs;
            }
        }

        public static List<DungeonInfo> Dungeons
        {
            get
            {
                if (dungeons == null) dungeons = CreateDungeons();
                return dungeons;
            }
        }

        public static JobProfile GetJob(JobType job)
        {
            for (int i = 0; i < Jobs.Count; i++)
            {
                if (Jobs[i].Type == job) return Jobs[i];
            }
            return Jobs[0];
        }

        private static List<JobProfile> CreateJobs()
        {
            List<JobProfile> list = new List<JobProfile>();
            list.Add(new JobProfile(JobType.DebugWarrior, "디버그 전사", "근접 공격형", "강타 디버깅", "검기를 날려 버그 코드를 직접 삭제합니다.", Color.FromArgb(40, 120, 240), 150, 42, 27, 12, 9, 7, 24, 7, 7, 3, 2, 1));
            list.Add(new JobProfile(JobType.VaccineMage, "백신 마법사", "회복/마법형", "백신 파동", "초록 백신 파동으로 적을 정화하고 HP를 회복합니다.", Color.FromArgb(50, 190, 90), 115, 72, 18, 8, 10, 9, 17, 14, 4, 2, 2, 2));
            list.Add(new JobProfile(JobType.FirewallKnight, "방화벽 기사", "방어형", "방화벽 돌진", "푸른 방화벽 파동으로 적을 밀어내고 피해를 줄입니다.", Color.FromArgb(70, 115, 220), 190, 35, 22, 20, 7, 6, 32, 5, 4, 7, 1, 1));
            list.Add(new JobProfile(JobType.FileExplorer, "파일 탐색자", "속도/전리품형", "취약점 스캔", "스캔 레이저로 적의 약점을 찾아 빠르게 공격합니다.", Color.FromArgb(220, 160, 50), 128, 48, 22, 9, 16, 16, 19, 8, 5, 2, 4, 4));
            return list;
        }



        public static string GetDungeonNpcName(DungeonType type)
        {
            if (type == DungeonType.FileExplorerForest) return "탐색요정 Searchy";
            if (type == DungeonType.PopupErrorZone) return "오류창 여왕 Exception";
            if (type == DungeonType.RecycleBinDungeon) return "휴지통 소녀 Binny";
            if (type == DungeonType.UpdateLab) return "업데이트 아저씨 PatchMan";
            if (type == DungeonType.ControlPanelCastle) return "설정 집사 Panel";
            if (type == DungeonType.TempCacheCave) return "임시파일 청소부 Temp";
            if (type == DungeonType.NetworkPort) return "패킷 선장 Ping";
            if (type == DungeonType.RegistryHive) return "레지스트리 사서 Regi";
            if (type == DungeonType.DriverVault) return "드라이버 정비공 Driver-K";
            if (type == DungeonType.System32Forbidden) return "커널 수호자 Kernel";
            if (type == DungeonType.BlueScreenTower) return "블루스크린 관측자 STOP";
            return "거울 속 사용자 Shadow";
        }

        public static string GetDungeonNpcLine(DungeonType type)
        {
            if (type == DungeonType.FileExplorerForest) return "파일이 안 보이면 일단 검색부터 하세요. 바탕화면에 다 던져두지 말고요.";
            if (type == DungeonType.PopupErrorZone) return "오류창은 닫으라고 만든 게 아니라 읽으라고 만든 겁니다. 물론 저도 닫고 싶긴 합니다.";
            if (type == DungeonType.RecycleBinDungeon) return "상점은 바탕화면 휴지통에서 열립니다. 버린 물건도 가격표가 붙으면 상품입니다.";
            if (type == DungeonType.UpdateLab) return "99%에서 멈춘 건 오류가 아닙니다. 인내심 벤치마크입니다.";
            if (type == DungeonType.ControlPanelCastle) return "설정은 건드리기 전에는 쉬워 보이고, 건드린 후에는 왜 그랬나 싶습니다.";
            if (type == DungeonType.TempCacheCave) return "임시 파일도 쌓이면 역사가 됩니다. 그리고 렉도 됩니다.";
            if (type == DungeonType.NetworkPort) return "패킷은 길을 잃고, 플레이어는 방향키를 잃습니다. 둘 다 자주 일어납니다.";
            if (type == DungeonType.RegistryHive) return "레지스트리는 기억의 벌집입니다. 아무 키나 뽑으면 벌이 아니라 오류가 나옵니다.";
            if (type == DungeonType.DriverVault) return "드라이버 충돌은 차 사고가 아닙니다. 그래도 수리비는 나갑니다.";
            if (type == DungeonType.System32Forbidden) return "여긴 만지지 말라는 말을 듣고 들어온 사람만 오는 곳입니다.";
            if (type == DungeonType.BlueScreenTower) return "파란 화면은 끝이 아니라 고급 경고문입니다. 멘탈은 별도 백업하세요.";
            return "이 던전의 NPC는 당신입니다. 도망가도 저장기록은 남습니다.";
        }

        public static string GetFileKind(DungeonInfo d)
        {
            string ext = ".file";
            int dot = d.FileName.LastIndexOf('.');
            if (dot >= 0 && dot < d.FileName.Length - 1) ext = d.FileName.Substring(dot).ToLowerInvariant();
            if (ext == ".exe") return "응용 프로그램 던전";
            if (ext == ".dll") return "동적 링크 라이브러리 던전";
            if (ext == ".zip") return "압축 파일 던전";
            if (ext == ".patch") return "패치 패키지 던전";
            if (ext == ".cpl") return "제어판 항목 던전";
            if (ext == ".tmp") return "임시 파일 던전";
            if (ext == ".net") return "네트워크 파일 던전";
            if (ext == ".reg") return "레지스트리 파일 던전";
            if (ext == ".drv") return "드라이버 파일 던전";
            if (ext == ".sys") return "시스템 파일 던전";
            if (ext == ".bsod") return "블루스크린 오류 던전";
            return "알 수 없는 파일 던전";
        }

        private static List<DungeonInfo> CreateDungeons()
        {
            List<DungeonInfo> list = new List<DungeonInfo>();
            list.Add(new DungeonInfo(DungeonType.FileExplorerForest, "File Explorer 숲", "Explorer_FolderForest.exe", "탐색기 창과 폴더 트리가 거대한 숲처럼 펼쳐진 초반 파일 던전입니다.", 1, 0, 2550, Color.FromArgb(38, 170, 95), Color.FromArgb(24, 86, 72)));
            list.Add(new DungeonInfo(DungeonType.PopupErrorZone, "Popup Error 미궁", "Popup_ErrorZone.dll", "살아 움직이는 오류창과 예외 메시지가 층층이 쌓인 에러 대화 던전입니다.", 3, 5, 2650, Color.FromArgb(238, 92, 70), Color.FromArgb(82, 38, 44)));
            list.Add(new DungeonInfo(DungeonType.RecycleBinDungeon, "Recycle Bin 던전", "$Recycle.Bin_Cache.zip", "삭제된 파일, 임시 폴더, 복구 조각이 떠다니는 보라색 지하 던전입니다. 바탕화면 휴지통은 아이템 상점입니다.", 4, 10, 2780, Color.FromArgb(136, 90, 218), Color.FromArgb(48, 42, 82)));
            list.Add(new DungeonInfo(DungeonType.UpdateLab, "Windows Update 연구소", "Windows_Update_Lab.patch", "99%에서 멈춘 업데이트와 패치 조각을 수집하는 진행률 던전입니다.", 6, 18, 2860, Color.FromArgb(70, 170, 235), Color.FromArgb(35, 68, 105)));
            list.Add(new DungeonInfo(DungeonType.ControlPanelCastle, "Control Panel 성", "ControlPanel_Settings.cpl", "제어판 설정 창, 슬라이더, 장치 아이콘이 성벽처럼 쌓인 중급 던전입니다.", 7, 25, 2920, Color.FromArgb(52, 128, 228), Color.FromArgb(40, 62, 104)));
            list.Add(new DungeonInfo(DungeonType.TempCacheCave, "Temp Cache 동굴", "Temp_CacheCleaner.tmp", "임시 파일과 캐시 찌꺼기가 몬스터가 된 가벼운 파밍 던전입니다.", 8, 34, 2800, Color.FromArgb(232, 170, 65), Color.FromArgb(80, 62, 34)));
            list.Add(new DungeonInfo(DungeonType.NetworkPort, "Network Port 항구", "Network_PortTunnel.net", "패킷과 포트가 물결처럼 흐르는 네트워크 항구 던전입니다.", 9, 42, 3000, Color.FromArgb(38, 195, 190), Color.FromArgb(25, 70, 88)));
            list.Add(new DungeonInfo(DungeonType.RegistryHive, "Registry Hive 보관소", "Registry_HiveArchive.reg", "레지스트리 키와 값이 미로처럼 얽힌 선택형 단서 던전입니다.", 10, 52, 3040, Color.FromArgb(190, 90, 210), Color.FromArgb(58, 36, 78)));
            list.Add(new DungeonInfo(DungeonType.DriverVault, "Driver Vault 격납고", "Driver_VaultDevice.drv", "드라이버 충돌과 장치 오류가 폭주하는 기계식 던전입니다.", 11, 60, 3100, Color.FromArgb(92, 108, 142), Color.FromArgb(36, 44, 62)));
            list.Add(new DungeonInfo(DungeonType.System32Forbidden, "System32 금지구역", "System32_KernelGuard.sys", "경고창과 시스템 파일이 뒤섞인 접근 금지 구역입니다. 강력한 커널 몬스터가 출현합니다.", 12, 70, 3260, Color.FromArgb(216, 82, 55), Color.FromArgb(78, 34, 42)));
            list.Add(new DungeonInfo(DungeonType.BlueScreenTower, "Blue Screen Tower", "BSOD_DragonFault.bsod", "블루스크린 패널과 오류 코드가 하늘을 뒤덮은 최종 보스 파일입니다.", 14, 90, 2700, Color.FromArgb(56, 132, 255), Color.FromArgb(14, 24, 82)));
            list.Add(new DungeonInfo(DungeonType.UserCoreTrueFault, "UserCore True Fault", "UserCore_TrueFault.exe", "숨겨진 User_Action_Log와 플레이어 자신의 오류가 구현된 찐 최종 던전입니다.", 17, 0, 2850, Color.FromArgb(245, 90, 180), Color.FromArgb(54, 22, 66)));
            return list;
        }


        public static List<Platform> CreatePlatforms(DungeonInfo dungeon)
        {
            List<Platform> list = new List<Platform>();
            int w = dungeon.MapWidth;
            Color baseColor = dungeon.Accent;
            list.Add(new Platform(0, 570, w, 70, Darken(baseColor, 78), "C:\\ROOT"));

            if (dungeon.Type == DungeonType.FileExplorerForest)
            {
                list.Add(new Platform(250, 455, 300, 34, Darken(baseColor, 20), "Desktop.folder"));
                list.Add(new Platform(650, 405, 280, 34, Darken(baseColor, 8), "Downloads.folder"));
                list.Add(new Platform(1030, 480, 340, 34, Darken(baseColor, 34), "cache.tmp"));
                list.Add(new Platform(1440, 375, 310, 34, Darken(baseColor, 2), "Explorer_patch.dll"));
                list.Add(new Platform(1845, 460, 350, 34, Darken(baseColor, 22), "preview.module"));
                list.Add(new Platform(2250, 335, 300, 34, Darken(baseColor, 32), "hidden.ini"));
            }
            else if (dungeon.Type == DungeonType.PopupErrorZone)
            {
                list.Add(new Platform(240, 458, 300, 34, Darken(baseColor, 24), "ErrorBox_001.dll"));
                list.Add(new Platform(620, 404, 300, 34, Darken(baseColor, 8), "exception.log"));
                list.Add(new Platform(1000, 482, 350, 34, Darken(baseColor, 32), "popup_stack.bin"));
                list.Add(new Platform(1420, 378, 320, 34, Darken(baseColor, 5), "dialog_loop.dll"));
                list.Add(new Platform(1820, 462, 340, 34, Darken(baseColor, 22), "warning_cache.tmp"));
                list.Add(new Platform(2240, 338, 300, 34, Darken(baseColor, 28), "message.final"));
            }
            else if (dungeon.Type == DungeonType.RecycleBinDungeon)
            {
                list.Add(new Platform(260, 460, 310, 34, Darken(baseColor, 24), "$Recycle.Bin"));
                list.Add(new Platform(680, 405, 290, 34, Darken(baseColor, 8), "deleted.log"));
                list.Add(new Platform(1060, 485, 340, 34, Darken(baseColor, 35), "restore.tmp"));
                list.Add(new Platform(1490, 382, 315, 34, Darken(baseColor, 5), "corrupt.zip"));
                list.Add(new Platform(1910, 462, 340, 34, Darken(baseColor, 22), "trash_cache.bin"));
                list.Add(new Platform(2320, 340, 320, 34, Darken(baseColor, 28), "recovery.ini"));
            }
            else if (dungeon.Type == DungeonType.UpdateLab)
            {
                list.Add(new Platform(240, 455, 320, 34, Darken(baseColor, 20), "progress_07.patch"));
                list.Add(new Platform(650, 405, 300, 34, Darken(baseColor, 8), "download_queue.sys"));
                list.Add(new Platform(1040, 480, 350, 34, Darken(baseColor, 34), "restart_pending.ini"));
                list.Add(new Platform(1470, 376, 320, 34, Darken(baseColor, 4), "PatchCore.sys"));
                list.Add(new Platform(1890, 462, 340, 34, Darken(baseColor, 18), "update_99.tmp"));
                list.Add(new Platform(2300, 338, 310, 34, Darken(baseColor, 28), "complete.flag"));
            }
            else if (dungeon.Type == DungeonType.ControlPanelCastle)
            {
                list.Add(new Platform(260, 455, 310, 34, Darken(baseColor, 18), "control.cpl"));
                list.Add(new Platform(660, 405, 300, 34, Darken(baseColor, 6), "device.panel"));
                list.Add(new Platform(1050, 480, 350, 34, Darken(baseColor, 30), "settings.dll"));
                list.Add(new Platform(1470, 376, 320, 34, Darken(baseColor, 4), "registry.reg"));
                list.Add(new Platform(1880, 462, 360, 34, Darken(baseColor, 18), "firewall.cpl"));
                list.Add(new Platform(2280, 338, 310, 34, Darken(baseColor, 28), "admin.panel"));
            }
            else if (dungeon.Type == DungeonType.TempCacheCave)
            {
                list.Add(new Platform(250, 455, 310, 34, Darken(baseColor, 24), "temp_001.tmp"));
                list.Add(new Platform(640, 408, 300, 34, Darken(baseColor, 8), "browser_cache.dat"));
                list.Add(new Platform(1040, 485, 350, 34, Darken(baseColor, 35), "old_thumbnail.db"));
                list.Add(new Platform(1460, 382, 320, 34, Darken(baseColor, 5), "cleanup.log"));
                list.Add(new Platform(1880, 462, 360, 34, Darken(baseColor, 22), "temp_boss.tmp"));
                list.Add(new Platform(2320, 340, 320, 34, Darken(baseColor, 28), "cache_clear.cmd"));
            }
            else if (dungeon.Type == DungeonType.NetworkPort)
            {
                list.Add(new Platform(250, 455, 320, 34, Darken(baseColor, 18), "port_80.net"));
                list.Add(new Platform(650, 405, 300, 34, Darken(baseColor, 6), "packet_stream.bin"));
                list.Add(new Platform(1040, 480, 350, 34, Darken(baseColor, 30), "ping_reply.log"));
                list.Add(new Platform(1480, 376, 320, 34, Darken(baseColor, 4), "router_table.tbl"));
                list.Add(new Platform(1900, 462, 360, 34, Darken(baseColor, 18), "firewall_port.cfg"));
                list.Add(new Platform(2320, 338, 310, 34, Darken(baseColor, 28), "tunnel_exit.net"));
            }
            else if (dungeon.Type == DungeonType.RegistryHive)
            {
                list.Add(new Platform(260, 455, 320, 34, Darken(baseColor, 22), "HKEY_USER.reg"));
                list.Add(new Platform(690, 405, 300, 34, Darken(baseColor, 10), "RunOnce.key"));
                list.Add(new Platform(1080, 485, 360, 34, Darken(baseColor, 36), "value_stack.dat"));
                list.Add(new Platform(1500, 376, 330, 34, Darken(baseColor, 5), "broken_key.reg"));
                list.Add(new Platform(1920, 462, 360, 34, Darken(baseColor, 18), "hive_backup.bak"));
                list.Add(new Platform(2350, 338, 320, 34, Darken(baseColor, 32), "root_value.bin"));
            }
            else if (dungeon.Type == DungeonType.DriverVault)
            {
                list.Add(new Platform(260, 455, 320, 34, Darken(baseColor, 22), "gpu_driver.drv"));
                list.Add(new Platform(690, 405, 300, 34, Darken(baseColor, 10), "audio_stack.sys"));
                list.Add(new Platform(1080, 485, 360, 34, Darken(baseColor, 36), "device_conflict.log"));
                list.Add(new Platform(1500, 376, 330, 34, Darken(baseColor, 5), "usb_bridge.dll"));
                list.Add(new Platform(1920, 462, 360, 34, Darken(baseColor, 18), "driver_patch.pkg"));
                list.Add(new Platform(2380, 338, 320, 34, Darken(baseColor, 32), "device_vault.map"));
            }
            else if (dungeon.Type == DungeonType.System32Forbidden)
            {
                list.Add(new Platform(270, 455, 320, 34, Darken(baseColor, 22), "kernel32.sys"));
                list.Add(new Platform(690, 405, 300, 34, Darken(baseColor, 10), "driver_stack.dll"));
                list.Add(new Platform(1080, 485, 360, 34, Darken(baseColor, 36), "access_denied.sys"));
                list.Add(new Platform(1500, 376, 330, 34, Darken(baseColor, 5), "memory_map.bin"));
                list.Add(new Platform(1920, 462, 360, 34, Darken(baseColor, 18), "boot_config.ini"));
                list.Add(new Platform(2350, 338, 320, 34, Darken(baseColor, 32), "kernel_guard.map"));
                list.Add(new Platform(2760, 485, 300, 34, Darken(baseColor, 15), "driver.log"));
                list.Add(new Platform(3000, 395, 230, 34, Darken(baseColor, 5), "ntos.map"));
            }
            else if (dungeon.Type == DungeonType.BlueScreenTower)
            {
                list.Add(new Platform(260, 455, 310, 34, Darken(baseColor, 18), "BSOD_core.sys"));
                list.Add(new Platform(650, 405, 300, 34, Darken(baseColor, 5), "stop_error.dll"));
                list.Add(new Platform(1030, 480, 340, 34, Darken(baseColor, 34), "dragon_fault.log"));
                list.Add(new Platform(1420, 375, 320, 34, Darken(baseColor, 2), "crash_dump.dmp"));
                list.Add(new Platform(1810, 460, 340, 34, Darken(baseColor, 22), "blue_screen.sys"));
                list.Add(new Platform(2180, 335, 300, 34, Darken(baseColor, 30), "final_patch.bin"));
            }
            else
            {
                list.Add(new Platform(260, 455, 310, 34, Darken(baseColor, 18), "user_action.log"));
                list.Add(new Platform(650, 405, 300, 34, Darken(baseColor, 5), "ignored_warning.err"));
                list.Add(new Platform(1030, 480, 340, 34, Darken(baseColor, 34), "forgot_save.bak"));
                list.Add(new Platform(1420, 375, 320, 34, Darken(baseColor, 2), "self_debug.exe"));
                list.Add(new Platform(1810, 460, 340, 34, Darken(baseColor, 22), "trash_regret.tmp"));
                list.Add(new Platform(2180, 335, 300, 34, Darken(baseColor, 30), "true_fault.sys"));
            }
            return list;
        }

        public static List<Monster> CreateMonsters(DungeonInfo dungeon)
        {
            List<Monster> list = new List<Monster>();
            if (dungeon.Type == DungeonType.FileExplorerForest)
            {
                list.Add(new Monster(MonsterKind.Slime, "Popup Slime", "팝업 슬라임", 1, 52, 7, 25, 18, false, 430, 570, Color.FromArgb(40, 180, 255)));
                list.Add(new Monster(MonsterKind.Goblin, "Error Goblin", "에러 고블린", 2, 68, 9, 32, 22, false, 850, 405, Color.FromArgb(75, 165, 65)));
                list.Add(new Monster(MonsterKind.Bat, "Lag Bat", "렉 박쥐", 3, 58, 8, 30, 20, false, 1240, 480, Color.FromArgb(130, 70, 210)));
                list.Add(new Monster(MonsterKind.Wolf, "Virus Wolf", "바이러스 울프", 4, 95, 12, 45, 35, false, 1780, 570, Color.FromArgb(70, 70, 90)));
            }
            else if (dungeon.Type == DungeonType.PopupErrorZone)
            {
                list.Add(new Monster(MonsterKind.Slime, "Popup Slime", "팝업 슬라임", 3, 78, 11, 42, 28, false, 430, 570, Color.FromArgb(255, 110, 90)));
                list.Add(new Monster(MonsterKind.Goblin, "Exception Imp", "예외 임프", 4, 92, 13, 55, 36, false, 850, 405, Color.FromArgb(220, 80, 65)));
                list.Add(new Monster(MonsterKind.Bat, "Dialog Bat", "대화상자 박쥐", 4, 86, 12, 50, 34, false, 1280, 480, Color.FromArgb(180, 90, 140)));
                list.Add(new Monster(MonsterKind.Ghost, "Warning Wraith", "경고 망령", 5, 120, 16, 66, 48, false, 1900, 570, Color.FromArgb(255, 160, 120)));
            }
            else if (dungeon.Type == DungeonType.RecycleBinDungeon)
            {
                list.Add(new Monster(MonsterKind.Skeleton, "Recycle Skeleton", "리사이클 스켈레톤", 5, 105, 15, 60, 45, false, 430, 570, Color.FromArgb(190, 190, 185)));
                list.Add(new Monster(MonsterKind.Slime, "Corrupt Slime", "손상된 슬라임", 5, 120, 16, 64, 42, false, 820, 405, Color.FromArgb(110, 60, 160)));
                list.Add(new Monster(MonsterKind.Ghost, "Deleted File Wraith", "삭제 파일 망령", 6, 125, 18, 72, 50, false, 1520, 375, Color.FromArgb(150, 220, 255)));
                list.Add(new Monster(MonsterKind.Hound, "Trojan Hound", "트로이 목마 하운드", 7, 155, 21, 86, 62, false, 2150, 570, Color.FromArgb(120, 40, 40)));
            }
            else if (dungeon.Type == DungeonType.UpdateLab)
            {
                list.Add(new Monster(MonsterKind.Slime, "Progress Slime", "진행률 슬라임", 6, 130, 18, 75, 55, false, 450, 570, Color.FromArgb(70, 190, 255)));
                list.Add(new Monster(MonsterKind.Bat, "Restart Bat", "재부팅 박쥐", 6, 118, 17, 72, 52, false, 890, 405, Color.FromArgb(95, 160, 220)));
                list.Add(new Monster(MonsterKind.Ghost, "Loading Ghost", "로딩 고스트", 7, 155, 21, 92, 68, false, 1510, 376, Color.FromArgb(140, 225, 255)));
                list.Add(new Monster(MonsterKind.Golem, "Patch Golem", "패치 골렘", 7, 210, 24, 120, 86, false, 2300, 570, Color.FromArgb(60, 135, 210)));
            }
            else if (dungeon.Type == DungeonType.ControlPanelCastle)
            {
                list.Add(new Monster(MonsterKind.Spider, "Registry Spider", "레지스트리 스파이더", 8, 145, 22, 92, 70, false, 620, 405, Color.FromArgb(75, 65, 135)));
                list.Add(new Monster(MonsterKind.Golem, "Firewall Golem", "방화벽 골렘", 9, 215, 25, 120, 88, false, 1260, 570, Color.FromArgb(75, 120, 160)));
                list.Add(new Monster(MonsterKind.Goblin, "Setting Imp", "설정 임프", 8, 130, 20, 80, 64, false, 1890, 460, Color.FromArgb(80, 160, 90)));
                list.Add(new Monster(MonsterKind.Golem, "Device Golem", "장치 골렘", 10, 250, 27, 140, 100, false, 2450, 570, Color.FromArgb(80, 100, 120)));
            }
            else if (dungeon.Type == DungeonType.TempCacheCave)
            {
                list.Add(new Monster(MonsterKind.Slime, "Cache Slime", "캐시 슬라임", 8, 150, 22, 100, 76, false, 430, 570, Color.FromArgb(235, 178, 70)));
                list.Add(new Monster(MonsterKind.Goblin, "Temp Goblin", "임시파일 고블린", 8, 145, 21, 96, 72, false, 880, 405, Color.FromArgb(210, 150, 55)));
                list.Add(new Monster(MonsterKind.Ghost, "Thumbnail Ghost", "썸네일 고스트", 9, 180, 25, 125, 90, false, 1520, 570, Color.FromArgb(255, 205, 110)));
                list.Add(new Monster(MonsterKind.Hound, "Cache Hound", "캐시 하운드", 9, 210, 27, 140, 104, false, 2180, 462, Color.FromArgb(180, 120, 60)));
            }
            else if (dungeon.Type == DungeonType.NetworkPort)
            {
                list.Add(new Monster(MonsterKind.Serpent, "Packet Serpent", "패킷 서펀트", 9, 190, 26, 135, 96, false, 480, 570, Color.FromArgb(50, 205, 190)));
                list.Add(new Monster(MonsterKind.Bat, "Ping Bat", "핑 박쥐", 10, 160, 24, 120, 88, false, 980, 405, Color.FromArgb(90, 220, 210)));
                list.Add(new Monster(MonsterKind.Goblin, "Port Imp", "포트 임프", 10, 185, 27, 145, 110, false, 1600, 570, Color.FromArgb(65, 180, 170)));
                list.Add(new Monster(MonsterKind.Golem, "Router Golem", "라우터 골렘", 11, 260, 31, 180, 135, false, 2360, 570, Color.FromArgb(45, 140, 160)));
            }
            else if (dungeon.Type == DungeonType.RegistryHive)
            {
                list.Add(new Monster(MonsterKind.Spider, "Registry Spider", "레지스트리 스파이더", 10, 185, 28, 150, 110, false, 520, 570, Color.FromArgb(110, 70, 160)));
                list.Add(new Monster(MonsterKind.Serpent, "Key Serpent", "키 서펀트", 11, 220, 30, 175, 130, false, 1100, 405, Color.FromArgb(190, 90, 210)));
                list.Add(new Monster(MonsterKind.Ghost, "Value Ghost", "값 고스트", 11, 210, 29, 170, 128, false, 1760, 570, Color.FromArgb(220, 120, 235)));
                list.Add(new Monster(MonsterKind.Golem, "Hive Guardian", "하이브 가디언", 12, 300, 34, 215, 160, false, 2500, 570, Color.FromArgb(140, 90, 190)));
            }
            else if (dungeon.Type == DungeonType.DriverVault)
            {
                list.Add(new Monster(MonsterKind.Golem, "Driver Golem", "드라이버 골렘", 11, 260, 32, 190, 145, false, 520, 570, Color.FromArgb(110, 120, 140)));
                list.Add(new Monster(MonsterKind.Hound, "Device Hound", "장치 하운드", 12, 230, 33, 185, 140, false, 1040, 405, Color.FromArgb(120, 110, 100)));
                list.Add(new Monster(MonsterKind.Bat, "USB Bat", "USB 박쥐", 12, 200, 31, 170, 128, false, 1680, 570, Color.FromArgb(150, 160, 170)));
                list.Add(new Monster(MonsterKind.Golem, "Conflict Guardian", "충돌 가디언", 13, 340, 38, 240, 190, false, 2520, 570, Color.FromArgb(90, 100, 130)));
            }
            else if (dungeon.Type == DungeonType.System32Forbidden)
            {
                list.Add(new Monster(MonsterKind.Ghost, "Memory Leak Ghost", "메모리 릭 고스트", 12, 210, 29, 160, 115, false, 540, 570, Color.FromArgb(120, 220, 255)));
                list.Add(new Monster(MonsterKind.Serpent, "Packet Serpent", "패킷 서펀트", 12, 230, 31, 170, 125, false, 1160, 405, Color.FromArgb(50, 190, 170)));
                list.Add(new Monster(MonsterKind.Hound, "Trojan Hound", "트로이 하운드", 13, 250, 34, 190, 140, false, 1830, 570, Color.FromArgb(160, 60, 60)));
                list.Add(new Monster(MonsterKind.Golem, "Kernel Guardian", "커널 가디언", 14, 330, 38, 230, 170, false, 2600, 485, Color.FromArgb(110, 110, 140)));
            }
            else if (dungeon.Type == DungeonType.BlueScreenTower)
            {
                list.Add(new Monster(MonsterKind.Dragon, "Blue Screen Dragon", "블루스크린 드래곤", 15, 980, 45, 680, 600, true, 2050, 570, Color.FromArgb(40, 95, 220)));
            }
            else
            {
                list.Add(new Monster(MonsterKind.Ghost, "Forgotten Save", "잊힌 저장본", 16, 260, 36, 190, 150, false, 520, 570, Color.FromArgb(250, 160, 230)));
                list.Add(new Monster(MonsterKind.Goblin, "Ignored Warning", "무시된 경고창", 16, 280, 38, 210, 160, false, 950, 405, Color.FromArgb(245, 110, 170)));
                list.Add(new Monster(MonsterKind.Hound, "Trash Regret", "휴지통 후회", 17, 330, 42, 240, 180, false, 1540, 570, Color.FromArgb(210, 70, 120)));
                list.Add(new Monster(MonsterKind.Dragon, "True User Shadow", "진짜 사용자 그림자", 18, 1250, 52, 900, 800, true, 2280, 570, Color.FromArgb(245, 90, 180)));
            }
            return list;
        }

        public static Color Darken(Color c, int amount)
        {
            return Color.FromArgb(c.A, Math.Max(0, c.R - amount), Math.Max(0, c.G - amount), Math.Max(0, c.B - amount));
        }
    }
}
