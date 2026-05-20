using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace DebugHeroFileDungeonRPG
{
    public enum ScreenMode
    {
        Boot,
        AssistantIntro,
        ProfileSetup,
        Desktop,
        Shop,
        Stage,
        StageClearDialog,
        FinalInput,
        Ending,
        Help
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

        public bool IsCastingPattern = false; // 특수 기믹 시전 여부 (true일 때 4번 특수 모션 스프라이트로 고정)
        public int Facing = 1;                // 바라보는 방향 (1: 오른쪽, -1: 왼쪽)
        public int HitFlash = 0;              // 피격 시 반짝임 효과 틱 (타격감 및 아우라 렌더링용)
        public int AttackCooldown = 0;        // 보스 기본 공격 쿨타임 관리

        // 보스 체력 페이즈별 기믹이 정확히 1번씩만 발동하도록 제어하는 스위치
        public bool Pattern75Used = false;
        public bool Pattern50Used = false;
        public bool Pattern25Used = false;
        public bool Pattern10Used = false;

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
        public int Hp = 100;
        public int MaxHp = 100;
        public int Mp = 60;
        public int MaxMp = 60;
        public int SystemStability = 100;
        public int CpuLoad = 15;
        public int Coins = 0;
        public int HpPotions = 2;
        public int MpPotions = 2;
        public int Level = 1;
        public int WeaponLevel = 1;
        public int Exp = 0;
        public int Facing = 1;
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
            s.Add(new StageInfo
            {
                Index = 1,
                Name = "File Explorer 숲",
                FileName = "File_Explorer_Forest.exe",
                Kind = StageKind.Normal,
                Background = "Windows XP 기본 바탕화면",
                CombatStyle = "일반 RPG에 가까운 액션 전투",
                CombatSpace = "별도 전투창이 아니라 바탕화면 전체",
                PlayerCharacter = "Recovery Program",
                Npc = "Windows Recovery Assistant",
                Mood = "밝고 유쾌한 Windows XP 패러디 감성",
                TwistHintLevel = "매우 낮음",
                Summary = "Windows XP 바탕화면 자체가 전투 필드가 되고, 파일과 폴더 아이콘을 정리한 뒤 Driver-K 보스 던전으로 자동 전환되는 첫 스테이지입니다.",
                MustKeep = "전투창을 따로 열지 않는다. 바탕화면 전체가 필드가 된다. 일반 파일 몬스터 정리 후 Driver-K 보스 패턴으로 자동 전환한다. 후반 반전을 직접 암시하지 않는다.",
                Flow = "부팅 화면 → XP 바탕화면 → Recovery Assistant 등장 → 파일 개체 정리 → Driver-K 보스 던전 자동 진입 → Driver-K 격리.",
                Objective = "바탕화면의 파일 개체를 정리한 뒤 Driver-K 보스를 격리한다.",
                Enemies = new string[] { "새 폴더 무리", "진짜최종 파일", "깨진 바로가기", "휴지통 과부하" },
                BossName = "Driver-K",
                BossRole = "Stage 1로 이동된 첫 번째 보스. 오래된 장치 드라이버를 지키는 경비형 보스",
                Dialogs = new string[]
                {
                    "안녕하세요! Windows Recovery Assistant입니다. 현재 바탕화면에 정리되지 않은 파일 개체가 많아 보여요.",
                    "먼저 간단한 복구 테스트를 시작해볼까요? 위험한 건 아니에요. 가벼운 정리 작업이라고 생각하시면 됩니다!",
                    "좋아요! 이제 직접 움직여볼까요? 바탕화면 위의 파일 개체들을 정리하려면 먼저 이동이 필요합니다."
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
                Kind = StageKind.Normal,
                Background = "Windows XP 장치 관리자 / 드라이버 보관소",
                CombatStyle = "일반 RPG에 가까운 보스전",
                CombatSpace = "장치 관리자 화면이 확장된 드라이버 격납고",
                PlayerCharacter = "Recovery Program",
                Npc = "Windows Recovery Assistant",
                Mood = "장치 충돌 개그 / 첫 보스전 / 낡은 드라이버의 적대감",
                TwistHintLevel = "낮음",
                Summary = "Driver-K가 Stage 01에서 격리된 뒤 남은 장치 충돌 잔여 파일을 정리하는 일반 스테이지입니다.",
                MustKeep = "Windows XP 장치 관리자 UI를 적극 활용한다. Driver-K 중복 보스전은 제거하고 잔여 드라이버 충돌 몬스터를 정리한다.",
                Flow = "Driver Vault.exe 실행 → 장치 목록 확인 → Unknown Device 잔여 충돌 확인 → 드라이버 파편 정리 → 드라이버 충돌 복구.",
                Objective = "Driver-K 격리 후 남은 드라이버 충돌 잔여 몬스터를 정리한다.",
                Enemies = new string[] { "Unknown Device", "Broken Driver Icon", "IRQ Conflict", "Driver Cache Fragment" },
                BossName = "Driver Cache Trace",
                BossRole = "Driver-K 격리 후 남은 드라이버 충돌 흔적",
                Dialogs = new string[]
                {
                    "Driver Vault에 도착했습니다! 이곳은 장치 드라이버 정보를 보관하는 공간이에요.",
                    "앗. 알 수 없는 장치가 응답하고 있어요. Driver-K가 복구 명령을 거부하고 있습니다.",
                    "Driver-K가 활성화되었습니다. 드라이버 충돌의 중심으로 보여요. 조심해서 접근해주세요!"
                },
                Accent = Color.FromArgb(236, 184, 52),
                BackColor = Color.FromArgb(94, 82, 54),
                NpcMood = NpcMood.Question
            });
            s.Add(new StageInfo
            {
                Index = 3,
                Name = "Windows Update 연구소",
                FileName = "Windows_Update_Lab.exe",
                Kind = StageKind.Normal,
                Background = "Windows XP 업데이트 창 / 업데이트 연구소",
                CombatStyle = "일반 RPG에 가까운 액션 전투",
                CombatSpace = "Windows Update 창과 진행률 UI가 확장된 공간",
                PlayerCharacter = "Recovery Program",
                Npc = "Windows Recovery Assistant",
                Mood = "업데이트 지옥 / 진행바 스트레스 / 반복되는 재시작 알림",
                TwistHintLevel = "낮음",
                Summary = "업데이트 진행률이 멈추고 UI 요소들이 적으로 변형되는 일반 스테이지입니다.",
                MustKeep = "Windows Update 창, 진행률 표시줄, 재시작 알림, 업데이트 실패 메시지를 적극 활용한다.",
                Flow = "Windows Update Lab.exe 실행 → 업데이트 준비 중 → 진행률 UI 적 변형 → 재시작 알림 이벤트 → 실패 구간 → 재시도 후 클리어.",
                Objective = "진행률 UI 적을 정리하고 업데이트 재시도를 완료한다.",
                Enemies = new string[] { "Update Patch 조각", "Loading Bar Slime", "Restart Reminder", "Failed Update" },
                BossName = "Failed Update",
                BossRole = "진행률 실패 구간의 마무리 적",
                Dialogs = new string[]
                {
                    "업데이트 준비 중입니다. 진행률이 멈추더라도 당황하지 마세요.",
                    "재시작 알림이 반복됩니다. 지금은 복구 절차가 우선입니다.",
                    "업데이트 실패 메시지를 정리하면 다음 보호 영역으로 이동할 수 있어요."
                },
                Accent = Color.FromArgb(68, 132, 226),
                BackColor = Color.FromArgb(44, 72, 124),
                NpcMood = NpcMood.Progress
            });
            s.Add(new StageInfo
            {
                Index = 4,
                Name = "System32 금지구역",
                FileName = "System32_Check.exe",
                Kind = StageKind.Boss,
                Background = "Windows XP 탐색기 / C:\\WINDOWS\\system32",
                CombatStyle = "일반 RPG에 가까운 보스전",
                CombatSpace = "System32 탐색기 창 내부가 확장된 시스템 핵심 구역",
                PlayerCharacter = "Recovery Program",
                Npc = "Windows Recovery Assistant",
                Mood = "조용한 긴장감 / 접근 제한 / 권한 확인 / 시스템 보호",
                TwistHintLevel = "낮음~중간 이하",
                Summary = "보호된 System32 내부에서 High-Kernel이 핵심 파일을 보호하며 등장하는 보스 스테이지입니다.",
                MustKeep = "일반 몹전은 넣지 않는다. Windows XP 탐색기 / 시스템 폴더 느낌을 유지한다. 직접적인 반전 암시는 금지한다.",
                Flow = "System32 Check.exe 실행 → C:\\WINDOWS\\system32 진입 → 접근 제한 경고 → High-Kernel 보스전 → 무결성 검사 완료.",
                Objective = "High-Kernel과 싸우고 핵심 파일 무결성 검사를 완료한다.",
                Enemies = new string[] { "High-Kernel" },
                BossName = "High-Kernel",
                BossRole = "System32 핵심 파일을 보호하는 경비 프로그램",
                Dialogs = new string[]
                {
                    "보호된 영역에 진입했습니다. 불필요한 조작은 최소화해주세요.",
                    "핵심 파일 보호 절차가 작동했습니다. High-Kernel이 접근을 차단합니다.",
                    "시스템 안정성을 위해 필요한 검사만 수행하겠습니다."
                },
                Accent = Color.FromArgb(210, 80, 58),
                BackColor = Color.FromArgb(82, 40, 46),
                NpcMood = NpcMood.Warning
            });
            s.Add(new StageInfo
            {
                Index = 5,
                Name = "Network Port 항구",
                FileName = "Network_Port.exe",
                Kind = StageKind.Normal,
                Background = "Windows XP 네트워크 연결 / 방화벽 / 포트 관리 화면",
                CombatStyle = "디펜스 요소가 섞인 액션 RPG 일반 전투",
                CombatSpace = "Network Connections 창이 확장된 네트워크 항구",
                PlayerCharacter = "Recovery Program",
                Npc = "Windows Recovery Assistant",
                Mood = "차가운 네트워크 화면 / 연결 차단 / 패킷 흐름 / 약한 불편함",
                TwistHintLevel = "낮음~중간 이하",
                Summary = "열린 포트와 이상 패킷을 정리하는 네트워크 항구 일반 스테이지입니다.",
                MustKeep = "Network Connections 창, 방화벽 설정창, 포트 번호, 패킷 표시를 적극 활용한다.",
                Flow = "Network Port.exe 실행 → 열린 포트 정리 → 이상 패킷 제거 → 방화벽 게이트 → 연결 과부하 안정화.",
                Objective = "포트와 패킷 흐름을 정리하고 네트워크 안정화를 완료한다.",
                Enemies = new string[] { "Open Port Buoy", "Packet Minnow", "Request Crab", "Firewall Barnacle" },
                BossName = "Firewall Barnacle",
                BossRole = "네트워크 안정화 구간의 마무리 적",
                Dialogs = new string[]
                {
                    "Network Connections가 항구처럼 확장되었습니다.",
                    "열린 포트와 이상 패킷을 확인하세요. 불필요한 연결은 정리해야 합니다.",
                    "외부 연결을 안정화하면 다음 시스템 안정성 검사로 이동할 수 있습니다."
                },
                Accent = Color.FromArgb(52, 174, 210),
                BackColor = Color.FromArgb(34, 72, 98),
                NpcMood = NpcMood.Thinking
            });
            s.Add(new StageInfo
            {
                Index = 6,
                Name = "Blue Screen Tower",
                FileName = "System_Stability_Check.exe",
                Kind = StageKind.Boss,
                Background = "Windows XP 블루스크린 / 시스템 안정성 검사 화면",
                CombatStyle = "생존형 보스전",
                CombatSpace = "블루스크린 문장과 STOP 코드가 탑처럼 쌓인 공간",
                PlayerCharacter = "Recovery Program",
                Npc = "Windows Recovery Assistant",
                Mood = "시스템 과부하 / 재부팅 직전 / 실제 위험감",
                TwistHintLevel = "중간 이하",
                Summary = "시스템이 정말 무너질 수 있음을 처음 체감하는 생존형 보스 스테이지입니다.",
                MustKeep = "일반 몹전은 넣지 않는다. 블루스크린 화면 자체가 보스이자 전투 공간처럼 느껴져야 한다.",
                Flow = "System Stability Check.exe 실행 → 상태 확인 → 응답 지연 → Blue Screen Tower 진입 → BSOD 등장 → 생존형 보스전.",
                Objective = "Crash Dump가 완료되기 전에 BSOD 충돌 파동을 견디고 안정화한다.",
                Enemies = new string[] { "BSOD / Blue Screen Sentinel" },
                BossName = "BSOD",
                BossRole = "시스템이 더 이상 버티기 어렵다고 보내는 마지막 경고",
                Dialogs = new string[]
                {
                    "시스템 안정성 검사가 시작되었습니다. CPU Load와 Memory 응답을 확인합니다.",
                    "응답 지연이 감지되었습니다. 화면이 일시적으로 멈출 수 있습니다.",
                    "Blue Screen Tower가 활성화되었습니다. 충돌 파동을 피하며 안정화하세요."
                },
                Accent = Color.FromArgb(36, 110, 245),
                BackColor = Color.FromArgb(10, 26, 120),
                NpcMood = NpcMood.Bsod
            });
            s.Add(new StageInfo
            {
                Index = 7,
                Name = "Registry Hive 보관소",
                FileName = "Registry_Hive.exe",
                Kind = StageKind.Normal,
                Background = "Windows XP 레지스트리 편집기 / Registry Editor",
                CombatStyle = "탐색형 일반 전투 / 기록 조사 / 선택 기반 정리",
                CombatSpace = "레지스트리 트리 구조가 미로처럼 확장된 보관소",
                PlayerCharacter = "Recovery Program",
                Npc = "Windows Recovery Assistant",
                Mood = "조용한 의심 / 기록 확인 / 불편한 흔적",
                TwistHintLevel = "중간",
                Summary = "이전 행동 기록이 레지스트리 값과 Recent Trace로 남아 있음을 보여주는 탐색형 스테이지입니다.",
                MustKeep = "암시는 레지스트리 값, 최근 실행 기록, 프로필 이름, 선택 기록으로 보여준다. 직접 설명은 금지한다.",
                Flow = "Registry Hive.exe 실행 → HKEY 트리 확인 → Broken Key 정리 → Recent Trace 조사 → 레지스트리 백업 생성.",
                Objective = "정리할 값과 보관할 값을 구분하고 남은 예외 메시지를 추적한다.",
                Enemies = new string[] { "Broken Key", "Duplicate Value", "Orphan Entry", "Recent Trace" },
                BossName = "Recent Trace",
                BossRole = "이전 스테이지 행동 기록의 잔상",
                Dialogs = new string[]
                {
                    "Registry Editor가 열렸습니다. 설정값과 실행 기록을 확인합니다.",
                    "일부 값에서 이전 복구 작업 기록이 감지됩니다.",
                    "값을 삭제하기 전에 정리할 항목과 보관할 항목을 구분해주세요."
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
                Background = "Windows XP 오류창 / 팝업 미궁 / 작업 표시줄",
                CombatStyle = "UI 조작형 보스전",
                CombatSpace = "오류창들이 겹겹이 쌓여 만들어진 미궁",
                PlayerCharacter = "Recovery Program",
                Npc = "Windows Recovery Assistant",
                Mood = "팝업 개그 -> UI 혼란 -> 통제 상실",
                TwistHintLevel = "중간~높음, 단 직접 공개 금지",
                Summary = "닫고 무시했던 오류창들이 Exception Queen으로 되돌아오는 UI 붕괴형 보스전입니다.",
                MustKeep = "처음에는 팝업 개그처럼 시작하되, 점점 조작이 통제되지 않는 느낌으로 바뀐다. 직접 공개는 금지한다.",
                Flow = "Exception Report.exe 실행 → 오류창 폭주 → Popup Error 미궁 형성 → Exception Queen 등장 → 중심 오류창 격리.",
                Objective = "잘못된 버튼을 피하고 Exception Queen의 중심 오류창을 찾아 격리한다.",
                Enemies = new string[] { "Exception Queen" },
                BossName = "Exception Queen",
                BossRole = "무시된 경고와 닫힌 오류창들의 집합체",
                Dialogs = new string[]
                {
                    "처리되지 않은 오류 기록을 확인합니다.",
                    "닫은 창이 다시 열리고 있습니다. 오류창 수가 비정상적으로 증가합니다.",
                    "중심 오류창을 찾아 격리하세요. 잘못된 버튼은 피해야 합니다."
                },
                Accent = Color.FromArgb(236, 58, 70),
                BackColor = Color.FromArgb(84, 36, 48),
                NpcMood = NpcMood.Error
            });
            s.Add(new StageInfo
            {
                Index = 9,
                Name = "Temp Cache 동굴",
                FileName = "Temp_Cache.exe",
                Kind = StageKind.Normal,
                Background = "Windows XP 임시파일 폴더 / 캐시 저장소 / 최근 기록",
                CombatStyle = "탐색형 일반 전투 + 선택형 파일 처리",
                CombatSpace = "임시파일과 캐시 더미가 동굴처럼 쌓인 저장소",
                PlayerCharacter = "Recovery Program",
                Npc = "Windows Recovery Assistant",
                Mood = "현실감 / 죄책감 / 삭제되지 않은 흔적 / 최종장 직전의 불편함",
                TwistHintLevel = "높음, 단 정체 직접 공개 금지",
                Summary = "삭제했다고 생각한 기록과 오류가 임시파일 형태로 남아 있음을 확인하는 최종 직전 스테이지입니다.",
                MustKeep = "탐색과 기록 확인 비중을 높인다. 실제 사용자 이름은 표시하지 않는다. 정체 직접 공개는 금지한다.",
                Flow = "Temp Cache 진입 → 임시파일 더미 탐색 → 미전송 오류 보고서 발견 → 복구 프로필 캐시 발견 → Cache Heap 정리.",
                Objective = "열기, 삭제, 보관 선택을 통해 임시파일과 캐시 흔적을 정리한다.",
                Enemies = new string[] { "Temp Fragment", "Cache Leech", "Unsent Report", "Recent Ghost", "Cache Heap" },
                BossName = "Cache Heap",
                BossRole = "임시파일 더미가 뭉친 마지막 정리 대상",
                Dialogs = new string[]
                {
                    "임시 저장소에 남은 항목이 많습니다. 불필요한 항목은 삭제해주세요.",
                    "열어보지 않아도 됩니다. 삭제하면 정리됩니다.",
                    "남은 기록은 복구를 지연시킵니다. 기록을 줄여야 합니다."
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
                Background = "Windows XP 휴지통 내부 / 삭제된 항목 저장소 / 최종 정리 구역",
                CombatStyle = "최종 보스전 + 직접 입력 기반 엔딩 분기",
                CombatSpace = "휴지통 내부가 던전처럼 확장된 삭제 항목 보관소",
                PlayerCharacter = "Recovery Program",
                Npc = "Windows Recovery Assistant",
                Mood = "최종 정리 / 자각 / 책임 / 종료 / 루프 분기",
                TwistHintLevel = "최종 공개",
                Summary = "지금까지 정리·삭제·격리·차단·닫기 행동이 모두 모이고, 플레이어가 직접 삭제할 대상을 입력해 엔딩이 갈리는 최종 스테이지입니다.",
                MustKeep = "Stage 1~9에서 쌓아온 파일명, 기록, 보스 이름, 입력한 이름을 모두 회수한다. Illegal_Binny 이후 최종 입력창으로 넘어간다.",
                Flow = "Recycle Bin 진입 → 삭제된 항목 확인 → Illegal_Binny 최종 전투 → 최종 입력창 → 엔딩 분기.",
                Objective = "Illegal_Binny를 격리한 뒤 삭제할 프로세스 이름을 직접 입력한다.",
                Enemies = new string[] { "Illegal_Binny" },
                BossName = "Illegal_Binny",
                BossRole = "삭제된 항목들의 집합이자 표면상 최종 보스",
                Dialogs = new string[]
                {
                    "이제 마지막 정리만 남았습니다. 휴지통을 비우면 복구 절차가 완료됩니다.",
                    "삭제된 것들은 사라지지 않습니다. 보관될 뿐입니다.",
                    "무엇을 비울 건가요? 최종 입력이 필요합니다."
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
