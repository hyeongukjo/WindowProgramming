using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading;

namespace DebugHeroFileDungeonRPG
{
    public class BossProjectile
    {
        public float X, Y, VX, VY;
        public int Damage;
        public bool IsEnrageMissile = false;
        public bool IsStunBullet = false;
        public bool IsExceptionBullet = false;
        public int NoticeTicks = 0;
        public string NoticeText = "";

        public void ShowNotice(string text) { NoticeText = text; NoticeTicks = 170; }

        public BossProjectile(float x, float y, float vx, float vy, int dmg, bool isEnrage = false, bool isStun = false, bool isException = false)
        {
            X = x; Y = y; VX = vx; VY = vy; Damage = dmg; IsEnrageMissile = isEnrage; IsStunBullet = isStun; IsExceptionBullet = isException;
        }
    }

    public class SkyMissile
    {
        public float X, Y;
        public int Timer;
    }

    public class CatchZone
    {
        public string ExceptionName;
        public float X, Y;
        public CatchZone(string name, float x, float y) { ExceptionName = name; X = x; Y = y; }
    }
    public class BossClone { public float X, Y; public int Hp = 50; public int MaxHp = 50; }
    public class BinnyAoE { public float X, Y; public int Timer = 45; public BinnyAoE(float x, float y) { X = x; Y = y; } }
    public class BinnyBoomerang { public float X, Y, VX, VY, StartX, StartY; public int Timer = 90; public bool Returning = false; }
    public class BinnyRing { public float X, Y, Radius = 10f; public bool HasHit = false; }

    public class BossPatternManager
    {
        private Random rand = new Random();
        public List<BossProjectile> Projectiles = new List<BossProjectile>();
        public List<SkyMissile> SkyMissiles = new List<SkyMissile>();

        private int basicAttackTimer = 0;

        public int NoticeTicks = 0;
        public string NoticeText = "";

        // --- 1번 보스 (Driver-K) 변수 ---
        public bool IsResourcePatternActive = false;
        public List<Rectangle> DebugButtons = new List<Rectangle>();
        public int ResourceTimer = 0;
        public bool IsShardPatternActive = false;
        public int ShardSequence = 0;
        public int ShardTimer = 0;
        public PointF CurrentShardPos;

        // --- 2번 보스 (High-Kernel) 변수 ---
        public bool IsAccessDeniedActive = false;
        private int accessDeniedBarrageTimer = 0;
        public bool IsSystemWipeActive = false;
        public int SystemWipeTimer = 0;
        public int SystemWipeWaitTimer = 0;
        public int SystemWipeCount = 0;
        public PointF SafeZoneCenter;
        public float SafeZoneRadius = 80f;
        public bool IsEnrageActive = false;
        public int EnrageTimer = 600;
        public int EnrageHitCount = 0;
        private int galagaTimer = 0;

        // --- 3번 보스 (BSOD) 변수 ---
        public bool IsLotusActive = false;
        public float LotusAngle = 0f;
        public int LotusTimer = 0;

        public bool IsLeakActive = false;
        public int LeakTimer = 0;
        public int PatchCount = 0;
        public PointF CurrentPatchPos;
        public float PatchRadius = 55f;
        public int StandTicks = 0;
        private int leakBarrageTimer = 0;

        public bool IsMagnusActive = false;
        public float MagnusWidth = 750f;
        public float MagnusHeight = 240f;
        public int MagnusTimer = 0;
        public PointF BossPos;

       
        public bool IsPattern10Used = false;

        // --- 4번 보스 (Exception Queen) 변수 ---
        public bool IsNullRefActive = false;
        public PointF NullRefNode;
        public float NullRefGauge = 0f;
        private int nullRefBarrageTimer = 0; // [추가] 75% 갤러그 탄막 타이머

        public bool IsTryCatchActive = false;
        public int TryCatchTimer = 0;
        public float TryCatchGauge = 0f;
        public string TargetException = "";
        public List<CatchZone> CatchZones = new List<CatchZone>();

        // [추가] 50% 패턴 타이핑 미니게임 변수
        public bool IsTypingPhaseActive = false;
        public int TypingTimer = 0;
        public string CurrentTypingTarget = "";
        public int TryCatchSuccessCount = 0;
        private char[] allowedChars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'Z', 'X', 'C', 'V', 'B', 'N', 'M', 'G', 'H', 'J', 'K', 'L' };

        public bool IsStackOverflowActive = false;
        public float StackGauge = 0f;
        public int TargetSwitch = 0;
        public PointF SwitchLeftPos, SwitchRightPos;
        public bool IsQueenPattern10Used = false;

        // --- 5번 보스 (Illegal_Binny) 변수 ---
        public bool IsBinny75Used = false, IsBinny50Used = false, IsBinny10Used = false, IsBinny1Used = false;

        // 75/25% (블랙홀)
        public bool IsBlackholeActive = false; public int BlackholeTimer = 0; public PointF AnchorPos; public int AnchorTicks = 0;
        // 50% (스캐너 줄넘기)
        public bool IsScannerActive = false; public float ScannerX = 0f, ScannerDir = 1f, ScannerSpeed = 8f, SafeHoleY = 0f; public int ScannerPassCount = 0;
        // 10% (DPS 체크)
        public bool IsDPSCheckActive = false; public int DPSCheckTimer = 0; public int BinnyShield = 0;
        // 1% (분신 발악)
        public bool IsIllusionActive = false; public int IllusionTimer = 0; public int SyncTimer = 180; public BossClone BinnyClone = null;
        public bool IsMainDead = false; public bool IsCloneDead = false;

        // 5번 보스 전용 일반공격 리스트
        public List<BinnyAoE> BinnyAoEs = new List<BinnyAoE>();
        public List<BinnyBoomerang> BinnyBoomerangs = new List<BinnyBoomerang>();
        public List<BinnyRing> BinnyRings = new List<BinnyRing>();
        public PointF PlayerPos;

        public void Reset()
        {
            Projectiles.Clear();
            SkyMissiles.Clear();
            DebugButtons.Clear();

            basicAttackTimer = 0;
            accessDeniedBarrageTimer = 0;
            SystemWipeTimer = 0;
            SystemWipeWaitTimer = 0;
            SystemWipeCount = 0;
            EnrageTimer = 600;
            EnrageHitCount = 0;
            galagaTimer = 0;

            IsResourcePatternActive = false;
            IsShardPatternActive = false;
            IsAccessDeniedActive = false;
            IsSystemWipeActive = false;
            IsEnrageActive = false;
            IsPattern10Used = false;

            IsLotusActive = false;
            IsLeakActive = false;
            IsMagnusActive = false;
            LeakTimer = 0;
            PatchCount = 0;
            StandTicks = 0;
            leakBarrageTimer = 0;

            MagnusWidth = 750f;
            MagnusHeight = 240f;

            // 4번 보스 초기화
            IsNullRefActive = false;
            IsTryCatchActive = false;
            IsStackOverflowActive = false;
            IsQueenPattern10Used = false;
            NullRefGauge = 0f;
            TryCatchGauge = 0f;
            StackGauge = 0f;
            IsTypingPhaseActive = false;
            nullRefBarrageTimer = 0;



            //5번 보스 초기화
            IsBinny75Used = false; IsBinny50Used = false; IsBinny10Used = false; IsBinny1Used = false;
            IsBlackholeActive = false; IsScannerActive = false; IsDPSCheckActive = false; IsIllusionActive = false; BinnyClone = null; IsMainDead = false; IsCloneDead = false;
        }

        public void Update(GameEntity boss, PlayerState player, List<Effect> effects, float mapWidth)
        {
            if (boss.Hp <= 0 && !IsIllusionActive) return;

            PlayerPos = new PointF(player.X, player.Y);

            UpdateProjectiles(player, effects, mapWidth);
            UpdateSkyMissiles(player, effects);

            if (boss.Name.Contains("Driver-K")) UpdateDriverK(boss, player, effects, mapWidth);
            else if (boss.Name.Contains("High-Kernel")) UpdateHighKernel(boss, player, effects, mapWidth);
            else if (boss.Name.Contains("BSOD")) UpdateBSOD(boss, player, effects, mapWidth);
            else if (boss.Name.Contains("Exception Queen") || boss.Name.Contains("Exception_Queen")) UpdateExceptionQueen(boss, player, effects, mapWidth);
            else if (boss.Name.Contains("Illegal_Binny") || boss.Name.Contains("Binny")) UpdateBinny(boss, player, effects, mapWidth);
        }

        // ==========================================
        // 5번 보스 (Illegal_Binny) 메인 업데이트
        // ==========================================
        private void UpdateBinny(GameEntity boss, PlayerState player, List<Effect> effects, float mapWidth)
        {
            // 기믹 상태에 따른 모션 인덱스 결정
            if (IsBlackholeActive || IsScannerActive || IsDPSCheckActive || IsIllusionActive)
            {
                boss.MotionIndex = 2; // 특수 기믹 시 인덱스 2번
            }
            else
            {
                // 플레이어 위치에 따른 인덱스 할당 (오른쪽 0, 왼쪽 1)
                boss.MotionIndex = (player.X > boss.X) ? 0 : 1;
            }
            // -----------------------
            // 💡 [방향 전환 추가] 최종 보스가 플레이어의 위치를 항상 추적하도록 세팅합니다.
            if (player.X < boss.X) boss.Facing = -1;
            else boss.Facing = 1;

            // 💡 렌더러 오버레이 동기화용 실시간 플레이어 좌표 백업
            PlayerPos = new PointF(player.X, player.Y);

            float hpPercent = (float)boss.Hp / boss.MaxHp * 100;

            // 1. 75% / 25% 패턴: 영구 소거 블랙홀 기믹
            if (((hpPercent <= 75 && !IsBinny75Used) || (hpPercent <= 25 && !IsBinny50Used)) && !IsBlackholeActive && !IsScannerActive && !IsDPSCheckActive)
            {
                IsBlackholeActive = true;
                BlackholeTimer = 480; // 8초 동안 유지
                AnchorPos = new PointF(mapWidth / 2, 330);
                if (hpPercent <= 25) IsBinny50Used = true; else IsBinny75Used = true;
            }

            // 2. 50% 패턴: 디스크 포맷 레이저 스캐너 (줄넘기 기믹)
            if (hpPercent <= 50 && !IsBinny50Used && !IsBlackholeActive && !IsScannerActive && !IsDPSCheckActive)
            {
                IsScannerActive = true;
                ScannerX = 100f; ScannerDir = 1f; ScannerSpeed = 8.5f; ScannerPassCount = 0;
                SafeHoleY = rand.Next(250, 480); // 안전지대 틈새 실시간 연산
            }

            // 3. 10% 패턴: 메모리 완전 비우기 강제 통제 (DPS 체크 쉴드)
            if (hpPercent <= 10 && !IsBinny10Used && !IsBlackholeActive && !IsScannerActive)
            {
                IsDPSCheckActive = true;
                IsBinny10Used = true;
                DPSCheckTimer = 600; // 10초 타임어택 제한시간 가동
                BinnyShield = 1500;   // 1500의 가공할 데이터 쉴드 적재
            }

            // 4. 1% 패턴: 가비지 컬렉터 메모리 누수 분신 폭주 (Final Apocalypse)
            if (hpPercent <= 1 && !IsBinny1Used && !IsDPSCheckActive)
            {
                IsIllusionActive = true;
                IsBinny1Used = true;
                BinnyClone = new BossClone { X = boss.X - 300, Y = boss.Y, Hp = boss.MaxHp / 4, MaxHp = boss.MaxHp / 4 };
            }

            // 기믹에 따른 물리 연산 핸들러 연결
            if (IsBlackholeActive) UpdateBlackhole(boss, player, effects);
            if (IsScannerActive) UpdateScanner(boss, player, effects, mapWidth);
            if (IsDPSCheckActive) UpdateDPSCheck(boss, player, effects);
            if (IsIllusionActive) UpdateIllusion(boss, player, effects);

            // 보스 기믹 캐스팅 바 활성화 플래그 연동
            boss.IsCastingPattern = (IsBlackholeActive || IsScannerActive || IsDPSCheckActive || IsIllusionActive);
        }

        // 수치 가독성 및 기존 연산 유지를 위한 가이드 브릿지 메서드들
        private void UpdateBlackhole(GameEntity boss, PlayerState player, List<Effect> effects)
        {
            BlackholeTimer--;
            float dx = AnchorPos.X - player.X; float dy = AnchorPos.Y - player.Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            if (dist > 10)
            {
                // 중심으로 서서히 빨아들이는 흡입력 물리 엔진 연산 적용
                player.X += (dx / dist) * 2.8f;
                player.Y += (dy / dist) * 2.8f;
            }
            if (dist < 45 && BlackholeTimer % 20 == 0)
            {
                player.Hp -= 80; // 블랙홀 코어 대미지
                effects.Add(new Effect("text", player.X, player.Y - 60, player.X, player.Y - 60, 25, Color.DarkRed, "데이터 소실 중!"));
            }
            if (BlackholeTimer <= 0) IsBlackholeActive = false;
        }

        private void UpdateScanner(GameEntity boss, PlayerState player, List<Effect> effects, float mapWidth)
        {
            ScannerX += ScannerSpeed * ScannerDir;
            if (ScannerX > mapWidth - 100 || ScannerX < 100) { ScannerDir *= -1f; ScannerPassCount++; }

            // 플레이어가 레이저 선에 부딪혔는지 판정
            if (Math.Abs(player.X - ScannerX) < 18f)
            {
                // 플레이어가 안전 구역(SafeHoleY) 범위 밖에 있다면 디스크 충돌 치명상 대미지
                if (player.Y < SafeHoleY - 45 || player.Y > SafeHoleY + 45)
                {
                    player.Hp -= 150;
                    effects.Add(new Effect("text", player.X, player.Y - 60, player.X, player.Y - 60, 20, Color.Red, "스캐너 충돌: 포맷 프로세스 가동!"));
                }
            }
            if (ScannerPassCount >= 4) IsScannerActive = false;
        }

        private void UpdateDPSCheck(GameEntity boss, PlayerState player, List<Effect> effects)
        {
            DPSCheckTimer--;
            // 보스 주위에 배리어가 쳐져 있는 동안 유저가 보스를 타격하면 쉴드가 먼저 깎이도록 설계
            if (BinnyShield <= 0)
            {
                IsDPSCheckActive = false;
                effects.Add(new Effect("text", boss.X, boss.Y - 120, boss.X, boss.Y - 120, 60, Color.Lime, "SHIELD BROKEN"));
                return;
            }
            if (DPSCheckTimer <= 0)
            {
                player.Hp = 0; // 타임오버: 휴지통 영구 강제 비우기 전멸
                effects.Add(new Effect("text", player.X, player.Y, player.X, player.Y, 60, Color.DarkRed, "RECYCLE BIN EMPTIED: SYSTEM ERASED"));
                IsDPSCheckActive = false;
            }
        }

        private void UpdateIllusion(GameEntity boss, PlayerState player, List<Effect> effects)
        {
            if (BinnyClone != null && BinnyClone.Hp <= 0) { BinnyClone = null; IsIllusionActive = false; }
        }

        public bool TrySkillInterrupt(PlayerState player, List<Effect> effects)
        {
            if (IsAccessDeniedActive)
            {
                effects.Add(new Effect("text", player.X, player.Y - 80, player.X, player.Y - 80, 60, Color.Red, "ACCESS DENIED"));
                SkyMissiles.Add(new SkyMissile { X = player.X + rand.Next(-50, 50), Y = player.Y + rand.Next(-50, 50), Timer = 60 });
                return true;
            }
            return false;
        }
       
        // ==========================================
        // 4번 보스 (Exception Queen) 메인 업데이트
        // ==========================================
        private void UpdateExceptionQueen(GameEntity boss, PlayerState player, List<Effect> effects, float mapWidth)
        {
            if (boss.Hp <= 0) return;

            // 플레이어의 위치를 실시간 추적하여 Facing 설정
            if (player.X < boss.X) boss.Facing = -1; // 왼쪽 대기
            else boss.Facing = 1;                  // 오른쪽 대기

            float hpPercent = (float)boss.Hp / boss.MaxHp * 100;

            // 1. 75%, 25% 패턴: NullReference 연결 기믹
            if (((hpPercent <= 75 && !boss.Pattern75Used) || (hpPercent <= 25 && !boss.Pattern25Used)) && !IsTryCatchActive && !IsNullRefActive && !IsStackOverflowActive)
            {
                IsNullRefActive = true;
                NullRefGauge = 0f;
                nullRefBarrageTimer = 0;
                NullRefNode = new PointF(rand.Next(350, (int)mapWidth - 350), rand.Next(220, 520));

                if (hpPercent <= 25) boss.Pattern25Used = true; else boss.Pattern75Used = true;
            }

            // 2. 50% 패턴: Try-Catch Block 구역 해킹 기믹
            if (hpPercent <= 50 && !boss.Pattern50Used && !IsNullRefActive && !IsTryCatchActive && !IsStackOverflowActive)
            {
                IsTryCatchActive = true;
                boss.Pattern50Used = true;
                boss.X = mapWidth / 2; boss.Y = 330;
                StartTryCatch(mapWidth);
            }

            // 3. 10% 패턴: StackOverflow (가비지 컬렉션 스위치 격파)
            if (hpPercent <= 10 && !IsQueenPattern10Used && !IsNullRefActive && !IsTryCatchActive)
            {
                IsStackOverflowActive = true;
                IsQueenPattern10Used = true;
                StackGauge = 0f;
                TargetSwitch = 0;
                SwitchLeftPos = new PointF(200, 450);
                SwitchRightPos = new PointF(mapWidth - 200, 450);
                boss.X = mapWidth / 2; boss.Y = 330;
            }

            if (IsNullRefActive) UpdateNullRef(boss, player, effects, mapWidth);
            else if (IsTryCatchActive) UpdateTryCatch(boss, player, effects, mapWidth);
            else if (IsStackOverflowActive) UpdateStackOverflow(boss, player, effects);
            else
            {
                basicAttackTimer++;
                if (basicAttackTimer >= 60)
                {
                    PerformExceptionBasicAttack(boss, player);
                    basicAttackTimer = 0;
                }
            }

            boss.IsCastingPattern = (IsNullRefActive || IsTryCatchActive || IsStackOverflowActive);
        }

        private void UpdateNullRef(GameEntity boss, PlayerState player, List<Effect> effects, float mapWidth)
        {
            nullRefBarrageTimer++;
            float dx = player.X - NullRefNode.X;
            float dy = player.Y - NullRefNode.Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);

            if (dist >= 120 && dist <= 280)
            {
                NullRefGauge += 0.35f;
            }
            else
            {
                NullRefGauge = Math.Max(0f, NullRefGauge - 0.15f);
                if (nullRefBarrageTimer % 15 == 0)
                {
                    player.Hp -= 40;
                    effects.Add(new Effect("text", player.X, player.Y - 50, player.X, player.Y - 50, 20, Color.Red, "궤도 이탈: 코어 오염 중!"));
                }
            }

            if (NullRefGauge >= 100f)
            {
                IsNullRefActive = false;
                effects.Add(new Effect("text", boss.X, boss.Y - 100, boss.X, boss.Y - 100, 60, Color.Lime, "REFERENCE RESTORED"));
            }

            if (nullRefBarrageTimer % 40 == 0)
            {
                int count = 8;
                float baseAngle = (float)(rand.NextDouble() * Math.PI);
                for (int i = 0; i < count; i++)
                {
                    float angle = baseAngle + (float)(i * Math.PI * 2 / count);
                    float speed = 5.5f;
                    Projectiles.Add(new BossProjectile(boss.X, boss.Y - 30, (float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed, 15, false, false, true));
                }
            }
        }

        private void StartTryCatch(float mapWidth)
        {
            TryCatchGauge = 0f;
            TryCatchTimer = 1200;
            IsTypingPhaseActive = false;
            TryCatchSuccessCount = 0;
            SpawnTryCatchZones(mapWidth);
        }

        private void SpawnTryCatchZones(float mapWidth)
        {
            CatchZones.Clear();
            string[] exceptions = { "NullReference", "Format", "IndexOutOfRange" };
            TargetException = exceptions[rand.Next(exceptions.Length)];

            CatchZones.Add(new CatchZone("NullReference", mapWidth / 2 - 320, 480));
            CatchZones.Add(new CatchZone("Format", mapWidth / 2, 480));
            CatchZones.Add(new CatchZone("IndexOutOfRange", mapWidth / 2 + 320, 480));
        }

        private void UpdateTryCatch(GameEntity boss, PlayerState player, List<Effect> effects, float mapWidth)
        {
            TryCatchTimer--;
            if (TryCatchTimer <= 0)
            {
                player.Hp = 0;
                effects.Add(new Effect("text", player.X, player.Y, player.X, player.Y, 60, Color.Red, "UNHANDLED EXCEPTION"));
                IsTryCatchActive = false;
                return;
            }

            if (!IsTypingPhaseActive)
            {
                foreach (var zone in CatchZones)
                {
                    float dx = player.X - zone.X;
                    float dy = player.Y - zone.Y;
                    if (Math.Sqrt(dx * dx + dy * dy) <= 75f)
                    {
                        if (zone.ExceptionName == TargetException)
                        {
                            IsTypingPhaseActive = true;
                            TypingTimer = 300;
                            CurrentTypingTarget = GenerateTypingString(6);
                            effects.Add(new Effect("text", player.X, player.Y - 80, player.X, player.Y - 80, 40, Color.Lime, "해킹 통로 개방!"));
                        }
                        else
                        {
                            if (TryCatchTimer % 25 == 0)
                            {
                                player.Hp -= 30;
                                effects.Add(new Effect("text", player.X, player.Y - 80, player.X, player.Y - 80, 20, Color.Red, "치명적 코어 예외 충돌!"));
                            }
                        }
                    }
                }
            }
            else
            {
                TypingTimer--;
                bool inZone = false;
                foreach (var zone in CatchZones)
                {
                    if (zone.ExceptionName == TargetException)
                    {
                        float dx = player.X - zone.X; float dy = player.Y - zone.Y;
                        if (Math.Sqrt(dx * dx + dy * dy) <= 75f) inZone = true;
                    }
                }

                if (!inZone)
                {
                    IsTypingPhaseActive = false;
                    effects.Add(new Effect("text", player.X, player.Y - 80, player.X, player.Y - 80, 40, Color.Orange, "해킹 세션 중단됨"));
                    return;
                }

                if (TypingTimer <= 0)
                {
                    IsTypingPhaseActive = false;
                    player.Hp -= 120;
                    effects.Add(new Effect("text", player.X, player.Y - 80, player.X, player.Y - 80, 40, Color.Red, "타임아웃 백도어 역류!"));
                    SpawnTryCatchZones(mapWidth);
                }
            }
        }

        // 💡 [해결 마법] 누락되었던 문자열 생성기 추가!
        private string GenerateTypingString(int length)
        {
            char[] arr = new char[length];
            for (int i = 0; i < length; i++) arr[i] = allowedChars[rand.Next(allowedChars.Length)];
            return new string(arr);
        }

        private void UpdateStackOverflow(GameEntity boss, PlayerState player, List<Effect> effects)
        {
            StackGauge += 0.135f;
            if (StackGauge >= 100f)
            {
                player.Hp = 0;
                effects.Add(new Effect("text", player.X, player.Y, player.X, player.Y, 60, Color.Red, "STACK OVERFLOW"));
                IsStackOverflowActive = false;
                return;
            }

            PointF currentTarget = TargetSwitch == 0 ? SwitchLeftPos : SwitchRightPos;
            float dx = player.X - currentTarget.X;
            float dy = player.Y - currentTarget.Y;
            if (Math.Sqrt(dx * dx + dy * dy) < 65f)
            {
                StackGauge = 0f;
                TargetSwitch = TargetSwitch == 0 ? 1 : 0;
                effects.Add(new Effect("burst", currentTarget.X, currentTarget.Y, currentTarget.X, currentTarget.Y, 30, Color.Lime, "FLUSH"));
                effects.Add(new Effect("text", player.X, player.Y - 60, player.X, player.Y - 60, 40, Color.Lime, "GC 가동 완료!"));
            }

            basicAttackTimer++;
            if (basicAttackTimer >= 40)
            {
                PerformExceptionBasicAttack(boss, player);
                basicAttackTimer = 0;
            }
        }

        private void PerformExceptionBasicAttack(GameEntity boss, PlayerState player)
        {
            float dx = player.X - boss.X; float dy = (player.Y - 20) - (boss.Y - 20);
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            if (dist < 1) dist = 1;
            Projectiles.Add(new BossProjectile(boss.X, boss.Y - 20, (dx / dist) * 9.5f, (dy / dist) * 9.5f, 15, false, false, true));
        }

        public void HandleTypingInput(System.Windows.Forms.Keys key, PlayerState player, List<Effect> effects, float mapWidth)
        {
            if (!IsTryCatchActive || !IsTypingPhaseActive) return;

            char typedChar = '\0';
            if (key >= System.Windows.Forms.Keys.A && key <= System.Windows.Forms.Keys.Z) typedChar = key.ToString()[0];
            else if (key >= System.Windows.Forms.Keys.D0 && key <= System.Windows.Forms.Keys.D9) typedChar = (char)('0' + (key - System.Windows.Forms.Keys.D0));
            else if (key >= System.Windows.Forms.Keys.NumPad0 && key <= System.Windows.Forms.Keys.NumPad9) typedChar = (char)('0' + (key - System.Windows.Forms.Keys.NumPad0));

            if (typedChar != '\0' && CurrentTypingTarget.Length > 0)
            {
                if (CurrentTypingTarget[0] == typedChar)
                {
                    CurrentTypingTarget = CurrentTypingTarget.Substring(1);
                    if (CurrentTypingTarget.Length == 0)
                    {
                        IsTypingPhaseActive = false;
                        TryCatchSuccessCount++;
                        TryCatchGauge = (TryCatchSuccessCount / 3f) * 100f;
                        effects.Add(new Effect("burst", player.X, player.Y, player.X, player.Y, 30, Color.Lime, ""));

                        if (TryCatchSuccessCount >= 3)
                        {
                            IsTryCatchActive = false;
                            effects.Add(new Effect("text", player.X, player.Y - 120, player.X, player.Y - 120, 60, Color.Lime, "BLOCK RESOLVED"));
                        }
                        else
                        {
                            SpawnTryCatchZones(mapWidth);
                        }
                    }
                }
                else
                {
                    effects.Add(new Effect("text", player.X, player.Y - 60, player.X, player.Y - 60, 20, Color.Red, "구문 오류!"));
                }
            }
        }

        // ==========================================
        // 2번 보스 (High-Kernel) 메인 로직 이식
        // ==========================================
        private void UpdateHighKernel(GameEntity boss, PlayerState player, List<Effect> effects, float mapWidth)
        {
            if (boss.Hp <= 0 && !IsIllusionActive) return;

            // 💡 [방향 전환 추가] 플레이어 위치를 실시간으로 추적하여 바라보는 방향 설정
            if (player.X < boss.X) boss.Facing = -1; // 플레이어가 왼쪽에 있을 때
            else boss.Facing = 1;                  // 플레이어가 오른쪽에 있을 때

            float hpPercent = (float)boss.Hp / boss.MaxHp * 100;

            // 1. 10% 이하 발악 패턴 (Enrage)
            if (hpPercent <= 10 && !IsPattern10Used)
            {
                IsPattern10Used = true;
                StartEnrage(boss, mapWidth, effects);
            }
            // 2. 50% 이하 전멸기 패턴 (System Wipe)
            else if (hpPercent <= 50 && !boss.Pattern50Used && !IsEnrageActive)
            {
                boss.Pattern50Used = true;
                StartSystemWipe(player, mapWidth);
            }
            // 3. 75% 및 25% 구간 장벽 패턴 (Access Denied)
            else if (((hpPercent <= 75 && !boss.Pattern75Used) || (hpPercent <= 25 && !boss.Pattern25Used)) && !IsSystemWipeActive)
            {
                if (hpPercent <= 25) boss.Pattern25Used = true; else boss.Pattern75Used = true;
                IsAccessDeniedActive = true;
                accessDeniedBarrageTimer = 240;
            }

            // 패턴 실행 분기 루프
            if (IsEnrageActive) UpdateEnrage(boss, player, effects, mapWidth);
            else if (IsSystemWipeActive) UpdateSystemWipe(player, effects, mapWidth);
            else
            {
                // Access Denied 패턴 시 하늘에서 미사일 투하
                if (accessDeniedBarrageTimer > 0)
                {
                    accessDeniedBarrageTimer--;
                    if (accessDeniedBarrageTimer % 15 == 0)
                        SkyMissiles.Add(new SkyMissile { X = player.X + rand.Next(-150, 150), Y = player.Y + rand.Next(-100, 100), Timer = 60 });
                    if (accessDeniedBarrageTimer == 0) IsAccessDeniedActive = false;
                }

                // 기본 90틱 주기 타겟팅 공격
                basicAttackTimer++;
                if (basicAttackTimer >= 90)
                {
                    float dx = player.X - boss.X; float dy = (player.Y - 20) - (boss.Y - 40);
                    float angle = (float)Math.Atan2(dy, dx);
                    Projectiles.Add(new BossProjectile(boss.X, boss.Y - 40, (float)Math.Cos(angle) * 8f, (float)Math.Sin(angle) * 8f, boss.Attack / 3));
                    basicAttackTimer = 0;
                }
            }

            // 💡 렌더러와 완벽 연동: 특수 기믹 패턴이 하나라도 켜져 있으면 true가 되며 4번 사진으로 전환됩니다.
            boss.IsCastingPattern = (IsEnrageActive || IsSystemWipeActive || IsAccessDeniedActive || accessDeniedBarrageTimer > 0);
        }

        // ==========================================
        // 3번 보스 (BSOD 드래곤) 핵심 로직 이식
        // ==========================================
        private void UpdateBSOD(GameEntity boss, PlayerState player, List<Effect> effects, float mapWidth)
        {
            if (boss.Hp <= 0) return;

            // 플레이어 방향 추적 (왼쪽: -1, 오른쪽: 1)
            if (player.X < boss.X) boss.Facing = -1;
            else boss.Facing = 1;

            // 💡 실시간 보스 위치 백업 (BossRuntime에서 안전구역을 그릴 때 참조함)
            BossPos = new PointF(boss.X, boss.Y);

            float hpPercent = (float)boss.Hp / boss.MaxHp * 100;

            // 1. 75% 및 25% 구간: Lotus (환각 전깃줄 레이저)
            if (((hpPercent <= 75 && !boss.Pattern75Used) || (hpPercent <= 25 && !boss.Pattern25Used)) && !IsLeakActive)
            {
                IsLotusActive = true;
                LotusTimer = 600;
                if (hpPercent <= 25) boss.Pattern25Used = true; else boss.Pattern75Used = true;
                boss.X = mapWidth / 2; boss.Y = 330;
            }

            // 2. 50% 구간: Leak (메모리 누수 패치 장판 + 다방향 탄막)
            if (hpPercent <= 50 && !boss.Pattern50Used && !IsLotusActive)
            {
                IsLeakActive = true;
                LeakTimer = 900;
                PatchCount = 0;
                StandTicks = 0;
                boss.Pattern50Used = true;
                boss.X = mapWidth / 2; boss.Y = 330;
                SpawnNextPatch(mapWidth);
            }

            // 3. 10% 이하 광폭화: Magnus (축소형 시스템 정지 안전구역)
            if (hpPercent <= 10 && !IsMagnusActive)
            {
                IsMagnusActive = true;
                MagnusWidth = 750f;
                MagnusHeight = 240f;
                MagnusTimer = 0;
                IsLotusActive = false; IsLeakActive = false;
            }

            // 패턴 실행 분기 루프
            if (IsLotusActive) UpdateLotus(boss, player, effects);
            else if (IsLeakActive) UpdateLeak(boss, player, effects, mapWidth);
            else if (IsMagnusActive) UpdateMagnus(boss, player, effects);
            else
            {
                basicAttackTimer++;
                if (basicAttackTimer >= 45)
                {
                    PerformBSODBasicAttack(boss, player);
                    basicAttackTimer = 0;
                }
            }

            boss.IsCastingPattern = (IsLotusActive || IsLeakActive || IsMagnusActive);
        }

        private void UpdateLotus(GameEntity boss, PlayerState player, List<Effect> effects)
        {
            LotusTimer--;
            float speed = (LotusTimer > 300) ? 0.013f : -0.013f;
            LotusAngle += speed;

            // 십자 방향으로 뻗어나가는 4개의 전깃줄 회전 레이저 충돌 판정
            for (int i = 0; i < 4; i++)
            {
                float checkAngle = LotusAngle + (float)(i * Math.PI / 2);

                float nx = -(float)Math.Sin(checkAngle);
                float ny = (float)Math.Cos(checkAngle);

                float dx = player.X - boss.X;
                float dy = (player.Y - 25) - (boss.Y - 20);

                float distanceToLine = Math.Abs(dx * nx + dy * ny);

                if (distanceToLine < 16f) // 전깃줄 판정 범위
                {
                    if (player.InvincibleTicks <= 0)
                    {
                        player.Hp -= (int)(player.MaxHp * 0.08f); // 8% 대미지
                        player.InvincibleTicks = 30;
                        effects.Add(new Effect("text", player.X, player.Y - 60, player.X, player.Y - 60, 30, Color.Cyan, "전깃줄 감전! -8%"));
                    }
                }
            }
            if (LotusTimer % 45 == 0) PerformBSODBasicAttack(boss, player);
            if (LotusTimer <= 0) IsLotusActive = false;
        }

        private void UpdateLeak(GameEntity boss, PlayerState player, List<Effect> effects, float mapWidth)
        {
            LeakTimer--;
            boss.X = mapWidth / 2; boss.Y = 330;

            if (LeakTimer <= 0)
            {
                player.Hp = 0; // 타임오버 전멸
                effects.Add(new Effect("text", player.X, player.Y, player.X, player.Y, 60, Color.Red, "FATAL ERROR: BLUE SCREEN"));
                IsLeakActive = false;
                return;
            }

            // 💡 15틱마다 보스 중심 사방으로 탄막 대량 살포 (다방향 탄막 기믹)
            leakBarrageTimer++;
            if (leakBarrageTimer >= 15)
            {
                leakBarrageTimer = 0;
                int count = 8; // 8방향 방사형 발사
                float baseAngle = (float)(rand.NextDouble() * Math.PI);
                for (int i = 0; i < count; i++)
                {
                    float angle = baseAngle + (float)(i * Math.PI * 2 / count);
                    float speed = 4.5f + rand.Next(0, 3);
                    Projectiles.Add(new BossProjectile(boss.X, boss.Y - 20, (float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed, 12));
                }
            }

            // 패치 장판 범위 체크
            float dx = player.X - CurrentPatchPos.X;
            float dy = player.Y - CurrentPatchPos.Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);

            if (dist <= PatchRadius)
            {
                StandTicks++; // 장판 위에 서있으면 로딩바 게이지 상승
                if (StandTicks % 15 == 0)
                    effects.Add(new Effect("text", player.X, player.Y - 50, player.X, player.Y - 50, 20, Color.Lime, "DOWNLOADING PATCH..."));

                if (StandTicks >= 60) // 게이지 만땅 (1초) 충족 시 패치 완료
                {
                    PatchCount++;
                    effects.Add(new Effect("burst", CurrentPatchPos.X, CurrentPatchPos.Y, CurrentPatchPos.X, CurrentPatchPos.Y, 25, Color.Lime, ""));

                    if (PatchCount >= 3) // 총 3번 해결 시 기믹 파쇄
                    {
                        IsLeakActive = false;
                        effects.Add(new Effect("text", boss.X, boss.Y - 100, boss.X, boss.Y - 100, 60, Color.Lime, "SYSTEM REPAIRED"));
                    }
                    else
                    {
                        SpawnNextPatch(mapWidth); // 다음 패치 장판 생성
                    }
                }
            }
            else
            {
                StandTicks = 0; // 장판에서 벗어나면 로딩 게이지 리셋
            }
        }

        private void SpawnNextPatch(float mapWidth)
        {
            // 맵 좌우 여백 200을 제외한 안전한 범위 내에 랜덤하게 패치 장판 좌표를 잡고,
            // 플레이어가 서 있어야 하는 시간인 로딩 게이지(StandTicks)를 0으로 초기화합니다.
            CurrentPatchPos = new PointF(
                rand.Next(200, (int)mapWidth - 200),
                rand.Next(150, 600)
            );
            StandTicks = 0;
        }

        private void UpdateMagnus(GameEntity boss, PlayerState player, List<Effect> effects)
        {
            MagnusTimer++;
            // 보스가 플레이어를 향해 압박하며 서서히 조여옴
            float moveX = (player.X - boss.X) * 0.02f;
            float moveY = (player.Y - boss.Y) * 0.02f;
            boss.X += moveX; boss.Y += moveY;

            // 300틱마다 안전구역이 단계별로 축소됨
            if (MagnusTimer % 300 == 0) MagnusWidth = Math.Max(150f, MagnusWidth - 150f);

            RectangleF safeZone = new RectangleF(boss.X - MagnusWidth / 2, boss.Y - MagnusHeight / 2, MagnusWidth, MagnusHeight);

            // 안전구역 박스 밖에 위치할 시 무자비한 대미지 페널티
            if (!safeZone.Contains(player.X, player.Y) || MagnusWidth <= 0)
            {
                if (MagnusTimer % 60 == 0)
                {
                    player.Hp -= (int)(player.MaxHp * 0.25f); // 25% 즉사급 페널티
                    effects.Add(new Effect("text", player.X, player.Y, player.X, player.Y, 40, Color.DeepSkyBlue, "SYSTEM HALT: OUT OF ZONE"));
                }
            }

            if (MagnusTimer % 45 == 0)
            {
                PerformBSODBasicAttack(boss, player);
            }
        }

        private void PerformBSODBasicAttack(GameEntity boss, PlayerState player)
        {
            float dx = player.X - boss.X; float dy = (player.Y - 20) - (boss.Y - 20);
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            if (dist < 1) dist = 1;
            Projectiles.Add(new BossProjectile(boss.X, boss.Y - 20, (dx / dist) * 9f, (dy / dist) * 9f, 15, false, true));

            boss.AttackCooldown = 60;
        }

        private void StartEnrage(GameEntity boss, float mapWidth, List<Effect> effects)
        {
            IsEnrageActive = true; boss.X = mapWidth - 150; boss.Y = 330;
            effects.Add(new Effect("text", boss.X, boss.Y - 100, boss.X, boss.Y - 100, 100, Color.Red, "!!! SYSTEM ENRAGE !!!"));
        }

        private void UpdateEnrage(GameEntity boss, PlayerState player, List<Effect> effects, float mapWidth)
        {
            EnrageTimer--; boss.X = mapWidth - 150;
            if (EnrageTimer <= 0) { IsEnrageActive = false; return; }

            // 💡 광폭화 전용 상단 알림 멘트 가동
            NoticeText = "☄️ 커널 임계 오버클록: 맵 전체에 산발적인 융단 폭격이 시작됩니다! (3번 피격 시 즉사)";
            NoticeTicks = 2;

            galagaTimer++;
            if (galagaTimer >= 18) // 발사 속도를 약간 더 타이트하게 상향 조율
            {
                galagaTimer = 0;

                // 💡 [기믹 수정] 3줄 정렬 방식을 파괴하고 한 웨이브당 3~4개의 운석을 스크린 전역 무작위 스폰
                int spawnCount = rand.Next(3, 5);
                for (int i = 0; i < spawnCount; i++)
                {
                    // 맵 전체 가로축(X) 범위 중 무작위 포인트를 타겟팅하여 하늘(Y=0)에 배치
                    float rx = rand.Next(100, (int)mapWidth - 100);

                    // 수직 중력 낙하 물리 가속도 주입 (IsEnrageMissile 플래그를 true로 세팅하여 즉사 카운터 연동)
                    float fallSpeed = 7f + (float)rand.NextDouble() * 4f;
                    Projectiles.Add(new BossProjectile(rx, 0, rand.Next(-1, 2), fallSpeed, boss.Attack / 4, true));
                }
            }
        }

        private void StartSystemWipe(PlayerState player, float mapWidth) { IsSystemWipeActive = true; SystemWipeCount = 0; NextSystemWipe(player, mapWidth); }
        private void NextSystemWipe(PlayerState player, float mapWidth)
        {
            // 💡 [밸런스 패치] 30 FPS 기준 5초는 정확히 150틱입니다! (기존 300틱에서 단축)
            SystemWipeTimer = 150;
            SystemWipeWaitTimer = 0;
            SafeZoneCenter = new PointF(player.X + rand.Next(-200, 200), rand.Next(250, 450));

            // 💡 패턴 시작 시 상단 멘트 실시간 동기화
            NoticeText = "🚨 커널 무결성 검사: 5초 뒤 안전구역을 제외한 전 구역이 폭발합니다! 안전구역 내부로 대피하세요!";
            NoticeTicks = 150;
        }
        private void UpdateSystemWipe(PlayerState player, List<Effect> effects, float mapWidth)
        {
            if (SystemWipeWaitTimer > 0)
            {
                SystemWipeWaitTimer--;
                if (SystemWipeWaitTimer <= 0) { if (SystemWipeCount < 3) { SystemWipeCount++; NextSystemWipe(player, mapWidth); } else IsSystemWipeActive = false; }
                return;
            }
            SystemWipeTimer--;
            if (SystemWipeTimer > 0)
            {
                NoticeText = "🚨 커널 무결성 검사: 5초 뒤 안전구역을 제외한 전 구역이 폭발합니다! 안전구역 내부로 대피하세요!";
                NoticeTicks = 2;
            }

            if (SystemWipeTimer <= 0)
            {
                float dx = player.X - SafeZoneCenter.X; float dy = player.Y - SafeZoneCenter.Y;

                // 💡 [즉사 메커니즘] 범위(SafeZoneRadius = 80) 밖에 있으면 즉사 대미지 판정
                if (Math.Sqrt(dx * dx + dy * dy) > SafeZoneRadius)
                {
                    player.Hp = 0; // 즉사
                }
                effects.Add(new Effect("burst", SafeZoneCenter.X, SafeZoneCenter.Y, player.X, player.Y, 40, Color.Red, "WIPE"));
                SystemWipeWaitTimer = 180;
            }
        }
        //==========================================
        // 1번 보스 (Driver K) 핵심 로직 이식
        //==========================================
        private void UpdateDriverK(GameEntity boss, PlayerState player, List<Effect> effects, float mapWidth)
        {
            // 💡 [버그 1 해결] 잃어버린 방향 전환 로직 복구!
            if (player.X < boss.X) boss.Facing = -1;
            else boss.Facing = 1;

            float hpPercent = (float)boss.Hp / boss.MaxHp * 100;

            // 75%, 25% 패턴 진입
            if (((hpPercent <= 75 && !boss.Pattern75Used) || (hpPercent <= 25 && !boss.Pattern25Used)) && !IsShardPatternActive && !IsResourcePatternActive)
            {
                StartShardPattern(mapWidth);
                if (hpPercent <= 25) boss.Pattern25Used = true; else boss.Pattern75Used = true;
            }
            // --------------------------------------------------------
            // 2. 50% 패턴: 디버그 팝업 지옥 (맵 전체 확장 + 13개 + 4.5초 타임어택)
            // --------------------------------------------------------
            if (hpPercent <= 50 && !boss.Pattern50Used && !IsShardPatternActive && !IsResourcePatternActive)
            {
                boss.Pattern50Used = true;
                IsResourcePatternActive = true;

                // 💡 [타임어택 설정] 게임이 30 FPS(틱당 33ms)로 작동하므로, 4.5초는 정확히 135틱입니다.
                ResourceTimer = 240;

                DebugButtons.Clear();

                // 💡 [개수 및 범위 수정] 10개의 버튼을 맵 전체에 무작위로 산발적 배치합니다.
                for (int i = 0; i < 13; i++)
                {
                    int btnW = 90;
                    int btnH = 32;

                    // 💡 mapWidth 대신 최대 해상도 1366을 기준으로 범위를 제한합니다.
                    // 우측 끝 여백과 버튼 크기(90px)를 고려하여 최대 1150px 안쪽에서만 나오도록 안전하게 통제합니다.
                    int rx = rand.Next(100, 1150);
                    int ry = rand.Next(160, 520); // 상단 보스 체력바와 하단 UI를 침범하지 않는 포지션

                    DebugButtons.Add(new Rectangle(rx, ry, btnW, btnH));
                }




            }

            // 패턴 중일 땐 패턴 전용 업데이트 실행
            if (IsResourcePatternActive) UpdateResourcePattern(player, effects);
            else if (IsShardPatternActive) UpdateShardPattern(player, effects, mapWidth);
            else
            {
                // 대기 상태일 때 일반 공격 발사
                basicAttackTimer++;
                if (basicAttackTimer >= 120)
                {
                    float dx = player.X - boss.X; float dy = (player.Y - 20) - (boss.Y - 30);
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy); float speed = 6.0f;
                    Projectiles.Add(new BossProjectile(boss.X, boss.Y - 30, (dx / dist) * speed, (dy / dist) * speed, boss.Attack / 5));
                    effects.Add(new Effect("projectile", boss.X, boss.Y - 30, player.X, player.Y - 30, 40, Color.MediumPurple, "ERR"));

                    boss.AttackCooldown = 45;
                    basicAttackTimer = 0;
                }
            }

            // 💡 [버그 2,3 방어] 상태 강제 동기화 (패턴이 끝나면 무조건 false로 돌아감)
            boss.IsCastingPattern = (IsShardPatternActive || IsResourcePatternActive);
        }


        private void StartResourcePattern() { IsResourcePatternActive = true; ResourceTimer = 270; DebugButtons.Clear(); for (int i = 0; i < 3; i++) DebugButtons.Add(new Rectangle(rand.Next(400, 900), rand.Next(250, 500), 110, 40)); }
        private void UpdateResourcePattern(PlayerState player, List<Effect> effects) { ResourceTimer--; if (ResourceTimer <= 0) { if (DebugButtons.Count > 0) { player.Hp /= 2; effects.Add(new Effect("text", player.X, player.Y, player.X, player.Y, 60, Color.Red, "SYSTEM OVERLOAD: HP HALVED")); } IsResourcePatternActive = false; } }
        private void StartShardPattern(float mapWidth) { IsShardPatternActive = true; ShardSequence = 0; SpawnNextShard(mapWidth); }
        private void SpawnNextShard(float mapWidth) { ShardTimer = 300; int minX = 300; int maxX = (int)Math.Max(minX + 100, mapWidth - 300); CurrentShardPos = new PointF(rand.Next(minX, maxX), rand.Next(250, 420)); }
        private void UpdateShardPattern(PlayerState player, List<Effect> effects, float mapWidth) { ShardTimer--; float dx = player.X - CurrentShardPos.X; float dy = player.Y - CurrentShardPos.Y; if (Math.Sqrt(dx * dx + dy * dy) < 80) { effects.Add(new Effect("burst", CurrentShardPos.X, CurrentShardPos.Y, CurrentShardPos.X, CurrentShardPos.Y, 20, Color.Lime, "FIXED")); ProgressShardPattern(mapWidth); return; } if (ShardTimer <= 0) { player.Hp -= (int)(player.MaxHp * 0.15f); effects.Add(new Effect("text", player.X, player.Y, player.X, player.Y, 40, Color.OrangeRed, "PATCH FAILED: -15% HP")); ProgressShardPattern(mapWidth); } }
        private void ProgressShardPattern(float mapWidth) { ShardSequence++; if (ShardSequence < 3) SpawnNextShard(mapWidth); else IsShardPatternActive = false; }

        private void UpdateProjectiles(PlayerState player, List<Effect> effects, float mapWidth)
        {
            for (int i = Projectiles.Count - 1; i >= 0; i--)
            {
                var p = Projectiles[i]; p.X += p.VX; p.Y += p.VY;
                float dist = (float)Math.Sqrt(Math.Pow(player.X - p.X, 2) + Math.Pow((player.Y - 25) - p.Y, 2));
                if (dist < 20 && player.InvincibleTicks <= 0)
                {
                    if (p.IsEnrageMissile) { EnrageHitCount++; if (EnrageHitCount >= 3) player.Hp = 0; effects.Add(new Effect("text", player.X, player.Y - 70, player.X, player.Y - 70, 40, Color.Red, $"HIT {EnrageHitCount}/3")); }
                    else
                    {
                        player.Hp -= p.Damage;
                        effects.Add(new Effect("text", player.X, player.Y - 60, player.X, player.Y - 60, 30, Color.HotPink, "-" + p.Damage));

                        if (p.IsStunBullet)
                        {
                            player.StunTicks = 30;
                            effects.Add(new Effect("text", player.X, player.Y - 90, player.X, player.Y - 90, 40, Color.Orange, "STUNNED (2s)"));
                        }
                        if (p.IsExceptionBullet && IsStackOverflowActive)
                        {
                            StackGauge += 15f;
                            effects.Add(new Effect("text", player.X, player.Y - 110, player.X, player.Y - 110, 40, Color.Red, "STACK +15%"));
                        }
                    }
                    player.InvincibleTicks = 25; Projectiles.RemoveAt(i); continue;
                }
                if (p.X < 0 || p.X > mapWidth + 500) Projectiles.RemoveAt(i);
            }
        }

        private void UpdateSkyMissiles(PlayerState player, List<Effect> effects)
        {
            for (int i = SkyMissiles.Count - 1; i >= 0; i--)
            {
                var sm = SkyMissiles[i]; sm.Timer--;
                if (sm.Timer <= 0)
                {
                    if (Math.Sqrt(Math.Pow(player.X - sm.X, 2) + Math.Pow(player.Y - sm.Y, 2)) < 60 && player.InvincibleTicks <= 0)
                    {
                        // 💡 [밸런스 패치] 한 발당 플레이어 최대 체력의 정확히 20% 파괴 연산 적용!
                        player.Hp -= (int)(player.MaxHp * 0.20f);
                        player.InvincibleTicks = 20;
                    }

                    // 💡 [버그 해결 핵심] 폭발이 끝난 미사일 객체를 리스트에서 삭제하여 빨간 범위선 잔상을 화면에서 완전히 소멸시킵니다!
                    SkyMissiles.RemoveAt(i);
                }
            }
        }

        public bool HandleClick(Point mousePos)
        {
            if (!IsResourcePatternActive) return false;
            for (int i = DebugButtons.Count - 1; i >= 0; i--)
            {
                if (DebugButtons[i].Contains(mousePos))
                {
                    DebugButtons.RemoveAt(i);
                    if (DebugButtons.Count == 0)
                    {
                        IsResourcePatternActive = false;
                        ResourceTimer = 0; // 💡 [핵심 수정] 버튼을 다 누르면 멈춰있던 타이머를 0으로 초기화!
                    }
                    return true;
                }
            }
            return false;
        }
    }
}