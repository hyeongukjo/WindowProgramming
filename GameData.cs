using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace DebugHeroFileDungeonRPG
{
    public enum ScreenMode
    {
        StartMenu,
        Boot,
        AssistantIntro,
        ProfileSetup,
        Desktop,
        Shop,
        Stage,
        StageClearDialog,
        FinalInput,
        Ending,
        Help,
        Cutscene
    }

    public enum StageKind
    {
        Normal,
        Boss,
        Final
    }

    public enum NpcMood
    {
        Basic,
        Welcome,
        Thinking,
        Happy,
        Question,
        Error,
        Bsod,
        Progress,
        Loading,
        Damaged,
        Log,
        Warning
    }

    public enum PlayerActionState
    {
        Idle,
        Walk,
        Skill,
        Hit,
        Die
    }

    public sealed class StageInfo
    {
        public int Index;
        public string Name;
        public string FileName;
        public StageKind Kind;
        public string Background;
        public string CombatStyle;
        public string CombatSpace;
        public string PlayerCharacter;
        public string Npc;
        public string Mood;
        public string TwistHintLevel;
        public string Summary;
        public string MustKeep;
        public string Flow;
        public string Objective;
        public string[] Enemies;
        public string BossName;
        public string BossRole;
        public string[] Dialogs;
        public Color Accent;
        public Color BackColor;
        public NpcMood NpcMood;
        public bool IsBossStage { get { return Kind == StageKind.Boss || Kind == StageKind.Final; } }
        public string PlanFile { get { return "StagePlans\\STAGE" + Index.ToString("00") + ".txt"; } }
    }

    public sealed class GameEntity
    {
        public string Name;
        public string DisplayName;
        public float X;
        public float Y;
        public float VX;
        public float VY;
        public int Hp;
        public int MaxHp;
        public int Attack;
        public bool IsBoss;
        public Color Color;
        public string Kind;

        public bool RewardGiven;
        public int CoinReward;

        public bool IsCastingPattern = false; // 특수 기믹 시전 여부
        public int Facing = 1;                // 보스가 바라보는 방향 (1: 오른쪽, -1: 왼쪽)
        public int HitFlash = 0;              // 피격 반짝임 효과
        public int AttackCooldown = 0;        // 보스 기본 공격 쿨타임

        public bool Pattern75Used = false;
        public bool Pattern50Used = false;
        public bool Pattern25Used = false;
        public bool Pattern10Used = false;

        public int MotionIndex = 0;

        // 💡 [STAGE 01 몬스터 기믹용 확장 변수 추가]
        public int StateTimer = 0;       // 상태 유지 및 쿨타임용 타이머 (틱 단위)
        public int MonsterState = 0;      // 0: 기본/대기, 1: 작동/돌진 중
        public float TargetPosX = 0f;    // 순간이동이나 돌진 목적지 X
        public float TargetPosY = 0f;    // 순간이동이나 돌진 목적지 Y

        public RectangleF Bounds
        {
            get
            {
                if (IsBoss) return new RectangleF(X - 95, Y - 130, 190, 130);
                return new RectangleF(X - 36, Y - 62, 72, 62);
            }
        }
    }

    public sealed class PlayerState
    {
        public string ProfileName = "";
        public string ProgramName = "Recovery Program";
        public float X = 180;
        public float Y = 520;
        public float TargetX = 180;
        public float TargetY = 520;
        public float MoveVelocityX = 0;
        public float MoveVelocityY = 0;
        public float WalkCycle = 0f;
        public int LastMoveTicks = 0;
        public int DefenseTicks = 0;
        public int Hp = 10000;  // 테스트용
        public int MaxHp = 10000;
        public int Mp = 60000;  //테스트용
        public int MaxMp = 60000;
        public int SystemStability = 100;
        public int CpuLoad = 15;
        public int Coins = 0;
        public int HpPotions = 2;
        public int MpPotions = 2;
        public int Level = 1;
        public int WeaponLevel = 1;
        public int Exp = 0;
        public int Facing = 1;
        public int Direction = 0;
        public PlayerActionState ActionState = PlayerActionState.Idle;
        public int ActionFrame = 0;
        public int ActionTick = 0;
        public int SkillIndex = -1;
        public int RelationLog = 0;
        public int ClearedStages = 0;
        public int QuarantinedBosses = 0;
        public int ProfileTruthScore = 0;
        public int InvincibleTicks = 0; // 피격 후 무적 시간
        public int StunTicks = 0;       // 기절 시간

        public RectangleF Bounds { get { return new RectangleF(X - 26, Y - 56, 52, 56); } }
    }

    public sealed class WeaponUpgradeFile
    {
        public float X;
        public float Y;
        public int StageIndex;
        public int UpgradeLevel;
        public bool Dragging;

        public RectangleF Bounds
        {
            get { return new RectangleF(X - 30, Y - 38, 60, 76); }
        }
    }

    public sealed class Effect
    {
        public string Kind;
        public float X;
        public float Y;
        public float X2;
        public float Y2;
        public int Ticks;
        public int MaxTicks;
        public string Text;
        public Color Color;

        public Effect(string kind, float x, float y, float x2, float y2, int ticks, Color color, string text)
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
        public const int RelationMax = 12;

        public static List<StageInfo> CreateStages()
        {
            List<StageInfo> s = new List<StageInfo>();

            // --------------------------------------------------------
            // [PAIR 1] 스테이지 1 (일반) ➡️ 스테이지 2 (Driver-K 보스)
            // --------------------------------------------------------
            s.Add(new StageInfo
            {
                Index = 1,
                Name = "File Explorer 숲",
                FileName = "File_Explorer_Forest.exe",
                Kind = StageKind.Normal,
                Background = "Windows XP 기본 바탕화면",
                Enemies = new string[] { "새 폴더 무리", "진짜최종 파일", "깨진 바로가기", "휴지통 과부하" },
                BossName = "",
                BossRole = "",
                Dialogs = new string[]
                {
                    "안녕하세요! Windows Recovery Assistant입니다. 바탕화면에 정리되지 않은 파일 개체가 많아요.",
                    "가벼운 정리 작업이라고 생각하시고 주변의 개체들을 정돈해 볼까요?",
                    "파일 몬스터들을 모두 물리치면 드라이버 보관소로 이동할 수 있는 링크가 열립니다!"
                },
                Accent = Color.FromArgb(48, 154, 84),
                BackColor = Color.FromArgb(88, 184, 90),
                NpcMood = NpcMood.Welcome
            });

            s.Add(new StageInfo
            {
                Index = 2,
                Name = "Driver Vault 격납고",
                FileName = "Driver_Vault.exe",
                Kind = StageKind.Boss,
                Background = "Windows XP 장치 관리자",
                Enemies = new string[] { "Driver-K" },
                BossName = "Driver-K",
                BossRole = "오래된 장치 드라이버를 지키는 경비형 보스",
                Dialogs = new string[]
                {
                    "Driver Vault 격납고에 진입했습니다! 경비 프로그램인 Driver-K가 감지되었습니다.",
                    "오래된 잔여 드라이버 장치들과 충돌을 일으키며 시스템 복구를 완강히 거부하고 있어요.",
                    "보스의 체력 기믹 패턴을 무력화시키고 안전하게 격리 절차를 진행해 주세요!"
                },
                Accent = Color.FromArgb(236, 184, 52),
                BackColor = Color.FromArgb(94, 82, 54),
                NpcMood = NpcMood.Question
            });

            // --------------------------------------------------------
            // [PAIR 2] 스테이지 3 (일반) ➡️ 스테이지 4 (High Kernel 보스)
            // --------------------------------------------------------
            s.Add(new StageInfo
            {
                Index = 3,
                Name = "System32 잔여 충돌 구역",
                FileName = "System32_Mobs.exe",
                Kind = StageKind.Normal,
                Background = "Windows XP 탐색기 기본",
                Enemies = new string[] { "Unknown Device", "Broken Driver Icon", "IRQ Conflict", "Driver Cache Fragment" },
                BossName = "",
                BossRole = "",
                Dialogs = new string[]
                {
                    "핵심 시스템 영역인 System32 부근에 도달했습니다. 접근 권한 경고가 발생 중입니다.",
                    "주변에 꼬여있는 알 수 없는 장치들과 캐시 파편들이 통행을 방해하고 있어요.",
                    "위험 개체들을 소거하여 커널 게이트로 향하는 보안 통로를 확보하세요."
                },
                Accent = Color.FromArgb(70, 130, 180),
                BackColor = Color.FromArgb(34, 50, 80),
                NpcMood = NpcMood.Thinking
            });

            s.Add(new StageInfo
            {
                Index = 4,
                Name = "System32 핵심 금지구역",
                FileName = "System32_Check.exe",
                Kind = StageKind.Boss,
                Background = "C:\\WINDOWS\\system32",
                Enemies = new string[] { "High-Kernel" },
                BossName = "High-Kernel",
                BossRole = "System32 핵심 파일을 보호하는 커널 가드 프로그램",
                Dialogs = new string[]
                {
                    "🚨 WARNING: 보호된 시스템 핵심 권한 영역에 무단 진입했습니다.",
                    "커널 영역 가디언인 High-Kernel이 무결성 검사를 강제 구사하며 차단 벽을 세웁니다.",
                    "사방에서 낙하하는 미사일 장벽 패턴을 격파하고 파일 무결성 검사를 완료하세요!"
                },
                Accent = Color.FromArgb(210, 80, 58),
                BackColor = Color.FromArgb(82, 40, 46),
                NpcMood = NpcMood.Warning
            });

            // --------------------------------------------------------
            // [PAIR 3] 스테이지 5 (일반) ➡️ 스테이지 6 (BSOD Dragon 보스)
            // --------------------------------------------------------
            s.Add(new StageInfo
            {
                Index = 5,
                Name = "Windows Update 연구소",
                FileName = "Windows_Update_Lab.exe",
                Kind = StageKind.Normal,
                Background = "Windows XP 업데이트 창",
                Enemies = new string[] { "Update Patch 조각", "Loading Bar Slime", "Restart Reminder" },
                BossName = "",
                BossRole = "",
                Dialogs = new string[]
                {
                    "Windows Update 연구소입니다. 진행률이 강제로 고정되어 시스템 과부하가 오고 있습니다.",
                    "반복되는 재시작 알림과 슬라임 개체들이 메모리 가용량을 위협하네요.",
                    "업데이트 실패 오류 조각들을 제거하고 상위 메모리 안정성 검사 탑으로 향하세요."
                },
                Accent = Color.FromArgb(68, 132, 226),
                BackColor = Color.FromArgb(44, 72, 124),
                NpcMood = NpcMood.Progress
            });

            s.Add(new StageInfo
            {
                Index = 6,
                Name = "Blue Screen Tower",
                FileName = "System_Stability_Check.exe",
                Kind = StageKind.Boss,
                Background = "Windows XP 블루스크린 화면",
                Enemies = new string[] { "BSOD" },
                BossName = "BSOD",
                BossRole = "시스템이 무너지기 직전 보내는 마지막 재앙 코드",
                Dialogs = new string[]
                {
                    "⚠️ CRITICAL ERROR: 시스템 응답 지연 및 메모리 누수가 발생했습니다.",
                    "누적된 시스템 모순의 결정체인 BSOD 드래곤이 치명적인 파동을 발산합니다.",
                    "환각 레이저 전깃줄과 메모리 장판을 밟아 복구 패치를 다운로드하여 생존하세요!"
                },
                Accent = Color.FromArgb(36, 110, 245),
                BackColor = Color.FromArgb(10, 26, 120),
                NpcMood = NpcMood.Bsod
            });

            // --------------------------------------------------------
            // [PAIR 4] 스테이지 7 (일반) ➡️ 스테이지 8 (Exception Queen 보스)
            // --------------------------------------------------------
            s.Add(new StageInfo
            {
                Index = 7,
                Name = "Registry Hive 보관소",
                FileName = "Registry_Hive.exe",
                Kind = StageKind.Normal,
                Background = "Windows XP 레지스트리 편집기",
                Enemies = new string[] { "Broken Key", "Duplicate Value", "Orphan Entry" },
                BossName = "",
                BossRole = "",
                Dialogs = new string[]
                {
                    "Registry Hive 미로 보관소입니다. 이전 행동들의 심층 기록이 남겨진 장소예요.",
                    "꼬여버린 중복 설정값들과 깨진 레지스트리 키 유령들이 탐색을 방해합니다.",
                    "오류 잔상들을 소거하여 최종 소거 제어 장치 팝업 미궁으로 나아가세요."
                },
                Accent = Color.FromArgb(150, 92, 218),
                BackColor = Color.FromArgb(56, 38, 82),
                NpcMood = NpcMood.Log
            });

            s.Add(new StageInfo
            {
                Index = 8,
                Name = "Popup Error 미궁",
                FileName = "Exception_Report.exe",
                Kind = StageKind.Boss,
                Background = "Windows XP 오류창 미궁",
                Enemies = new string[] { "Exception Queen" },
                BossName = "Exception Queen",
                BossRole = "무시된 경고와 닫힌 오류창들의 집합체",
                Dialogs = new string[]
                {
                    "닫고 외면했던 경고 오류창들이 융합해 탄생한 Exception Queen의 미궁입니다.",
                    "여왕이 폭주하며 시스템 크래시를 유도하고 있습니다. 통제 권한을 잃기 직전입니다!",
                    "포인터 연결선 거리를 유지하고 Try-Catch 구역의 보안 예외 코드를 완벽히 타이핑하세요!"
                },
                Accent = Color.FromArgb(236, 58, 70),
                BackColor = Color.FromArgb(84, 36, 48),
                NpcMood = NpcMood.Error
            });

            // --------------------------------------------------------
            // [PAIR 5] 스테이지 9 (일반) ➡️ 스테이지 10 (Illegal_Binny 최종 보스)
            // --------------------------------------------------------
            s.Add(new StageInfo
            {
                Index = 9,
                Name = "Temp Cache 동굴",
                FileName = "Temp_Cache.exe",
                Kind = StageKind.Normal,
                Background = "임시파일 저장소",
                Enemies = new string[] { "Temp Fragment", "Cache Leech", "Unsent Report" },
                BossName = "",
                BossRole = "",
                Dialogs = new string[]
                {
                    "최종 휴지통 입구 직전의 Temp Cache 동굴입니다. 완전히 지워지지 못한 캐시 더미입니다.",
                    "미전송 오류 보고서와 캐시 거머리들이 마지막 복구 승인을 차단하려 날뜁니다.",
                    "지우개 연산을 작동시켜 임시 흔적들을 마지막 한 조각까지 완벽히 정화하세요."
                },
                Accent = Color.FromArgb(210, 176, 54),
                BackColor = Color.FromArgb(86, 76, 52),
                NpcMood = NpcMood.Damaged
            });

            s.Add(new StageInfo
            {
                Index = 10,
                Name = "Recycle Bin 던전",
                FileName = "Recycle_Bin_Dungeon.exe",
                Kind = StageKind.Final,
                Background = "Windows XP 휴지통 내부",
                Enemies = new string[] { "Illegal_Binny" },
                BossName = "Illegal_Binny",
                BossRole = "삭제된 항목들의 집합이자 표면상 최종 보스",
                Dialogs = new string[]
                {
                    "복구 대장정의 종착지인 최후의 휴지통 내부 비우기 제어실입니다.",
                    "구금되었던 프로세스 파편들의 집합체인 최종 수호자 Illegal_Binny가 앞을 가로막습니다.",
                    "용사여, 최후의 데이터 격전에서 승리하고 시스템 엔딩 분기를 쟁취하십시오!"
                },
                Accent = Color.FromArgb(86, 180, 90),
                BackColor = Color.FromArgb(42, 78, 48),
                NpcMood = NpcMood.Warning
            });

            return s;
        }

        public static string ReadStagePlan(int index)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StagePlans", "STAGE" + index.ToString("00") + ".txt");
            try
            {
                if (File.Exists(path)) return File.ReadAllText(path);
            }
            catch { }
            return "STAGE" + index.ToString("00") + " 문서 원문을 찾을 수 없습니다.";
        }
    }
}
