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
        public bool IsBinny25Used = false;

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

        private int binnyBasicAttackPattern = 0; // 1: 얼음, 2: 화염, 3: 번개
        private int binnyBasicAttackTimer = 0;
        private float binnyStrikeTargetX = 0;
        private float binnyStrikeTargetY = 0;
        private int fireSequenceCount = 0;      // 화염 연속 3타 트래킹 카운터
        public bool IsBinnyAttackShaking { get; private set; } // 메인 프레임 연동 흔들림 변수

        // 플레이어 디버프 관리용 시간 변수 (MainForm Update에서 차감)
        public int PlayerSlowTicks = 0;
        public int PlayerBurnTicks = 0;

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
          
            if (boss == null || boss.Hp <= 0)
            {
                if (IsIllusionActive) UpdateIllusion(boss, player, effects);
                return;
            }

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
            // 기믹 상태에 따른 모션 인덱스 결정 (기존 코드 유지)
            if (IsBlackholeActive || IsScannerActive || IsDPSCheckActive || IsIllusionActive)
            {
                boss.MotionIndex = 2;
            }
            else
            {
                boss.MotionIndex = (player.X > boss.X) ? 0 : 1;
            }

            if (player.X < boss.X) boss.Facing = -1;
            else boss.Facing = 1;

            PlayerPos = new PointF(player.X, player.Y);
            float hpPercent = (float)boss.Hp / boss.MaxHp * 100;

            // ----------------------------------------------------------
            // 1. 75% / 25% 패턴: 영구 소거 블랙홀 기믹 (변수 교정)
            // ----------------------------------------------------------
           
            if (((hpPercent <= 75 && !IsBinny75Used) || (hpPercent <= 25 && !IsBinny25Used)) && !IsBlackholeActive && !IsScannerActive && !IsDPSCheckActive)
            {
                IsBlackholeActive = true;
                BlackholeTimer = 480;
                AnchorPos = new PointF(mapWidth / 2, 330);
                if (hpPercent <= 25) IsBinny25Used = true; else IsBinny75Used = true;

               
                float spawnOffset = rand.Next(280, 450);
                float patchX = (rand.Next(0, 2) == 0) ? (mapWidth / 2) - spawnOffset : (mapWidth / 2) + spawnOffset;

                CurrentPatchPos = new PointF(patchX, rand.Next(250, 520));
                StandTicks = 0;
            }

            // ----------------------------------------------------------
            // 2. 50% 패턴: 디스크 포맷 레이저 스캐너 (줄넘기 기믹 오작동 해결)
            // ----------------------------------------------------------
            if (hpPercent <= 50 && !IsBinny50Used && !IsBlackholeActive && !IsScannerActive && !IsDPSCheckActive)
            {
                IsScannerActive = true;
                IsBinny50Used = true; // 💡 [오류 해결 핵심] 패턴이 시작되었으므로 자신의 플래그를 즉시 true로 잠금!

                // 왼쪽 끝(100f)에서 시작하여 오른쪽(Dir = 1f)으로 최초 주행 셋업
                ScannerX = 100f;
                ScannerDir = 1f;
                ScannerSpeed = 8.5f;
                ScannerPassCount = 0;
                SafeHoleY = rand.Next(250, 480);
            }

            // 3. 10% 패턴 (기존 코드 유지)
            if (hpPercent <= 10 && !IsBinny10Used && !IsBlackholeActive && !IsScannerActive)
            {
                IsDPSCheckActive = true;
                IsBinny10Used = true;
                DPSCheckTimer = 600;
                BinnyShield = 1500;
            }

            // 4. 1% 패턴 (기존 코드 유지 구역)
            if (hpPercent <= 1 && !IsBinny1Used && !IsDPSCheckActive)
            {
                IsIllusionActive = true;
                IsBinny1Used = true;
                int calculatedPlayerAttack = 30 + player.Level * 8 + (player.WeaponLevel - 1) * 7;
                int fifteenHitsHp = calculatedPlayerAttack * 15;
                boss.Hp = fifteenHitsHp;
                boss.MaxHp = fifteenHitsHp;
                BinnyClone = new BossClone { Hp = fifteenHitsHp, MaxHp = fifteenHitsHp };
                BinnyClone.X = player.X - 280f;
                boss.X = player.X + 280f;
                BinnyClone.Y = player.Y;
                boss.Y = player.Y;

             
                IllusionTimer = 360;  // 12초 제한시간 정상 가동 (30 FPS * 12 = 360틱)
                DualDeathTimer = -1;  // 3초 동시 처치 링크 타이머 대기
                IsMainDead = false;   // 본체 생존 등록
                IsCloneDead = false;  // 분신 생존 등록

                effects.Add(new Effect("text", player.X, player.Y - 120, player.X, player.Y - 120, 100, Color.Purple, "FINAL APOCALYPSE: 분신 폭주 개시!"));
            }
            // 기믹에 따른 물리 연산 핸들러 연결
            if (IsBlackholeActive) UpdateBlackhole(boss, player, effects);
            if (IsScannerActive) UpdateScanner(boss, player, effects, mapWidth);
            if (IsDPSCheckActive) UpdateDPSCheck(boss, player, effects);
            if (IsIllusionActive) UpdateIllusion(boss, player, effects);

            if (!IsBlackholeActive && !IsScannerActive && !IsDPSCheckActive && !IsIllusionActive)
            {
                // 평상시 기본 상태: 화면 하단 축(660px)을 기준으로 위아래 50px 범위 내를 부드럽게 유영
                boss.Y = 660f + (float)Math.Sin(Environment.TickCount * 0.0015);
            }
            else if (IsIllusionActive)
            {
                // 1% 최종 페이즈: 플레이어와 동일 선상 밀착 전투를 위해 680px 높이에서 미세 플로팅 기동
                boss.Y = 680f + (float)Math.Sin(Environment.TickCount * 0.003);
                if (BinnyClone != null)
                {
                    BinnyClone.Y = boss.Y; // 분신도 본체와 높이를 완전히 대칭 링크 일치시킵니다.
                }
            }

            // 보스 기믹 캐스팅 바 활성화 플래그 연동
            boss.IsCastingPattern = (IsBlackholeActive || IsScannerActive || IsDPSCheckActive || IsIllusionActive);

            // ==========================================================
            //  5번 보스 3색 검 기본 공격 패턴 세분화 루프
            // ==========================================================
            if (!boss.IsCastingPattern)
            {
                IsBinnyAttackShaking = false; // 매 프레임 흔들림 상태 초기화

                // 공격 타이머가 0 이하일 때 새로운 타겟팅 세션 시작
                if (binnyBasicAttackTimer <= 0)
                {
                    // 화염 검 연속 3타 연격 중이 아닐 때만 다음 속성을 랜덤하게 셋팅 (1:얼음, 2:화염, 3:번개)
                    if (fireSequenceCount == 0)
                    {
                        binnyBasicAttackPattern = rand.Next(1, 4);
                    }

                    // 내려찍을 시점의 플레이어 실시간 X, Y 위치 좌표 락온(Lock-on)
                    binnyStrikeTargetX = player.X;
                    binnyStrikeTargetY = player.Y;

                    // 1초 선행 경고 시간 부여 (30 FPS 제한 조건이므로 30틱 = 정확히 1초)
                    binnyBasicAttackTimer = 30;
                }

                // 1초 경고 충전 시간 동안 작동하는 연산
                if (binnyBasicAttackTimer > 0)
                {
                    binnyBasicAttackTimer--;

                    // 충전 타임 동안 바닥 내려찍기 예고 범위 표시 (속성별 색상 지정 표시 연출)
                    Color indicatorColor = binnyBasicAttackPattern == 1 ? Color.Cyan : binnyBasicAttackPattern == 2 ? Color.Red : Color.Gold;
                    effects.Add(new Effect("text", binnyStrikeTargetX, binnyStrikeTargetY - 20, binnyStrikeTargetX, binnyStrikeTargetY - 20, 2, indicatorColor, "▼"));

                    // 1초 시간이 다 되어 지면에 격돌("깡!")하는 강타 타이밍 판정
                    if (binnyBasicAttackTimer == 0)
                    {
                        IsBinnyAttackShaking = true; // 화면 흔들림 플래그 가동

                        float dist = (float)Math.Sqrt(Math.Pow(player.X - binnyStrikeTargetX, 2) + Math.Pow(player.Y - binnyStrikeTargetY, 2));
                        bool isHit = dist <= 85f;

                        // [1번 패턴 : ice_sword]
                        if (binnyBasicAttackPattern == 1)
                        {
                            // 💡 [수정] 단순 spark와 text 외에, 진짜 얼음검 이미지 효과를 화면에 생성하도록 추가!
                            effects.Add(new Effect("binnyIce", binnyStrikeTargetX, binnyStrikeTargetY, binnyStrikeTargetX, binnyStrikeTargetY, 25, Color.Cyan, ""));
                            effects.Add(new Effect("spark", binnyStrikeTargetX, binnyStrikeTargetY, binnyStrikeTargetX, binnyStrikeTargetY, 25, Color.Cyan, ""));
                            effects.Add(new Effect("text", binnyStrikeTargetX, binnyStrikeTargetY - 40, binnyStrikeTargetX, binnyStrikeTargetY - 40, 34, Color.Cyan, "ICE SLAM"));

                            if (isHit)
                            {
                                player.Hp -= (int)(player.MaxHp * 0.35f); // 💡 5% -> 35% 상향
                                PlayerSlowTicks = 120;
                            }
                            fireSequenceCount = 0;
                        }
                        // [2번 패턴 : fire_sword]
                        else if (binnyBasicAttackPattern == 2)
                        {
                            // 💡 [수정] 진짜 화염검 이미지 효과를 화면에 생성하도록 추가!
                            effects.Add(new Effect("binnyFire", binnyStrikeTargetX, binnyStrikeTargetY, binnyStrikeTargetX, binnyStrikeTargetY, 25, Color.Red, ""));
                            effects.Add(new Effect("spark", binnyStrikeTargetX, binnyStrikeTargetY, binnyStrikeTargetX, binnyStrikeTargetY, 25, Color.Red, ""));
                            effects.Add(new Effect("text", binnyStrikeTargetX, binnyStrikeTargetY - 40, binnyStrikeTargetX, binnyStrikeTargetY - 40, 34, Color.Red, $"FIRE COMBO ({fireSequenceCount + 1}/3)"));

                            if (isHit)
                            {
                                player.Hp -= (int)(player.MaxHp * 0.25f); // 💡 타당 25% 도합 75% 점사
                                PlayerBurnTicks = 150; // 화상 지속 시간 대폭 연장
                            }

                            fireSequenceCount++;
                            if (fireSequenceCount < 3) binnyBasicAttackTimer = 30;
                            else fireSequenceCount = 0;
                        }
                        else if (binnyBasicAttackPattern == 3)
                        {
                            // 💡 진짜 번개검 이미지 효과를 화면에 생성하도록 추가!
                            effects.Add(new Effect("binnyLight", binnyStrikeTargetX, binnyStrikeTargetY, binnyStrikeTargetX, binnyStrikeTargetY, 25, Color.Gold, ""));
                            effects.Add(new Effect("spark", binnyStrikeTargetX, binnyStrikeTargetY, binnyStrikeTargetX, binnyStrikeTargetY, 25, Color.Gold, ""));
                            effects.Add(new Effect("text", binnyStrikeTargetX, binnyStrikeTargetY - 40, binnyStrikeTargetX, binnyStrikeTargetY - 40, 34, Color.Gold, "LIGHTNING HEAL"));

                            if (isHit)
                            {
                                player.Hp -= (int)(player.MaxHp * 0.40f); // 💡 5% -> 40% 아웃
                                //boss.Hp = Math.Min(boss.MaxHp, boss.Hp + (int)(boss.MaxHp * 0.25f)); // 보스 힐량 25% 상향
                            }
                            fireSequenceCount = 0;
                        }
                    }
                }
            }
            else
            {
                // 특수 기믹 패턴 시전 중일 때는 모든 기본공격 카운터 초기화 차단
                IsBinnyAttackShaking = false;
                fireSequenceCount = 0;
                binnyBasicAttackTimer = 0;
            }
            
        } 

        // 수치 가독성 및 기존 연산 유지를 위한 가이드 브릿지 메서드들
        private void UpdateBlackhole(GameEntity boss, PlayerState player, List<Effect> effects)
        {
            BlackholeTimer--;
            float dx = AnchorPos.X - player.X; float dy = AnchorPos.Y - player.Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);

            // 3.5배 강화된 강력한 중력으로 플레이어를 아이스 소드 중심으로 상시 견인
            if (dist > 5)
            {
                player.X += (dx / dist) * 9.8f;
                player.Y += (dy / dist) * 9.8f;
            }

            // 중심부 코어에 완전히 닿아버리면 시스템 즉사 포맷 처리
            if (dist < 45 && BlackholeTimer % 20 == 0)
            {
                player.Hp = 0;
                effects.Add(new Effect("text", player.X, player.Y - 60, player.X, player.Y - 60, 25, Color.DarkRed, "데이터 영구 소실!"));
            }

            // ==========================================================
            // 💡 [추가] 실시간 패치파일 범위 체크 및 게이지 로딩 연산
            // ==========================================================
            float pdx = player.X - CurrentPatchPos.X;
            float pdy = player.Y - CurrentPatchPos.Y;
            float patchDist = (float)Math.Sqrt(pdx * pdx + pdy * pdy);

            if (patchDist <= 65f) // 패치 장판 반경 65px 내부일 때
            {
                StandTicks++; // 로딩바 게이지 충전 증가
                if (StandTicks % 15 == 0)
                    effects.Add(new Effect("text", player.X, player.Y - 50, player.X, player.Y - 50, 20, Color.DeepSkyBlue, "DOWNLOADING PATCH..."));

                // 30 FPS 규격 기준 90틱(정확히 3초) 동안 빨려 들어가지 않고 무빙치며 버티면 해킹 완료
                if (StandTicks >= 90)
                {
                    IsBlackholeActive = false; // 블랙홀 소멸 및 패턴 파쇄 종료
                    effects.Add(new Effect("burst", CurrentPatchPos.X, CurrentPatchPos.Y, CurrentPatchPos.X, CurrentPatchPos.Y, 30, Color.Lime, ""));
                    effects.Add(new Effect("text", boss.X, boss.Y - 100, boss.X, boss.Y - 100, 60, Color.Lime, "DEBUGER SUCCESS"));
                }
            }
            else
            {
                // 패치 구역을 이탈하면 충전 중이던 로딩바 게이지가 서서히 유실됨
                if (StandTicks > 0) StandTicks--;
            }

            // 해킹을 완수하지 못하고 제한시간 타임아웃 도달 시 전멸
            if (BlackholeTimer <= 0 && IsBlackholeActive)
            {
                player.Hp = 0;
                IsBlackholeActive = false;
            }
        }

        private void UpdateScanner(GameEntity boss, PlayerState player, List<Effect> effects, float mapWidth)
        {
            ScannerX += ScannerSpeed * ScannerDir;

            // 벽면 끝과 끝 충돌 감지 연산 파트
            if (ScannerX > mapWidth - 100 || ScannerX < 100)
            {
                ScannerDir *= -1f;
                ScannerPassCount++; // 맵 끝에서 끝으로 편도 왕복 이동 횟수 카운트 증가

                // 튕겨나갈 때마다 점진적 속도 오버클록 가속
                ScannerSpeed += 2.5f;

                // 매번 가변 예측이 불가능하도록 안전 구역 틈새 위치 랜덤 가변화
                SafeHoleY = rand.Next(250, 480);

                effects.Add(new Effect("text", ScannerX, SafeHoleY, ScannerX, SafeHoleY, 20, Color.Orange, "스캐너 과부하 가속!"));
            }

            // 플레이어 신체 중심축이 스캐너 수직 전깃줄 레이저 선에 접촉했는지 판정
            if (Math.Abs(player.X - ScannerX) < 18f)
            {
                // 💡 [수정] 안전 구역 구멍(SafeHoleY 틈새) 범위 밖인 전깃줄 영역에 단 1픽셀이라도 닿을 시 무조건 즉사
                if (player.Y < SafeHoleY - /*45*/ 180 || player.Y > SafeHoleY + /*45*/ 180)
                {
                    player.Hp = 0; // 체력을 0으로 강제 고정하여 즉사 처리 파괴 약속 이행
                    effects.Add(new Effect("text", player.X, player.Y - 60, player.X, player.Y - 60, 20, Color.Red, "스캐너 강제 포맷: 즉사!"));
                }
            }

            // 5회 반복
            if (ScannerPassCount >= 5)
            {
                IsScannerActive = false;
            }
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

        public int DualDeathTimer = -1; // 💡 [추가] 3초 타임어택용 제어 퓨즈 변수


        private void UpdateIllusion(GameEntity boss, PlayerState player, List<Effect> effects)
        {
            IllusionTimer--;

            // 1. 12초 타임아웃 제한시간 초과 시 플레이어 전멸 즉사 처리
            if (IllusionTimer <= 0)
            {
                player.Hp = 0;
                IsIllusionActive = false;
                effects.Add(new Effect("text", player.X, player.Y - 60, player.X, player.Y - 60, 60, Color.Red, "시간 초과: 시스템 강제 붕괴!"));
                return;
            }

            // 2. 메인 보스 본체 사망 실시간 추적 등록
            if (boss == null || boss.Hp <= 0)
            {
                if (!IsMainDead)
                {
                    IsMainDead = true;
                    SyncTimer = 90; // 한쪽이 무너지면 즉시 3초(90틱) 동시 처치 타이머 스타트!
                }
            }

            // 3. 분신 개체 사망 실시간 추적 등록
            if (BinnyClone != null && BinnyClone.Hp <= 0)
            {
                if (!IsCloneDead)
                {
                    IsCloneDead = true;
                    SyncTimer = 90; // 분신이 먼저 무너져도 동일하게 3초 링크 가동!
                }
            }

            // 4. [핵심] 한쪽 유실 시 3초 듀얼 타임어택 상호 카운트다운 연산
            if (IsMainDead || IsCloneDead)
            {
                SyncTimer--;
                if (SyncTimer <= 0)
                {
                    // 3초가 지났음에도 아직 한쪽이 살아남아 있다면 체력 조절 실패 전멸 처단
                    if (!IsMainDead || !IsCloneDead)
                    {
                        player.Hp = 0;
                        IsIllusionActive = false;
                        effects.Add(new Effect("text", player.X, player.Y - 60, player.X, player.Y - 60, 60, Color.Red, "동기화 실패: 잔여 가비지 코드 소멸 실패!"));
                        return;
                    }
                }
            }

            // 5. 💡 [승리 조건] 본체와 분신 '둘 다' 완벽히 누워 플래그가 성립되었을 때만 발악 기믹 최종 소멸!
            if (IsMainDead && IsCloneDead)
            {
                IsIllusionActive = false;
                BinnyClone = null;
            }
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

        // ==========================================================
        // 2번 High Kernel 보스 메인 업데이트 및 패턴 핸들러
        // ==========================================================
        private void UpdateHighKernel(GameEntity boss, PlayerState player, List<Effect> effects, float mapWidth)
        {
            if (boss.Hp <= 0 && !IsIllusionActive) return;

            if (player.X < boss.X) boss.Facing = -1;
            else boss.Facing = 1;

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

                // [수정] 패턴 시작 즉시 보스를 화면 중앙 고정축으로 픽싱
                boss.X = mapWidth / 2;
                boss.Y = 330;

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
            else if (IsSystemWipeActive)
            {
                // [수정] 기믹 진행 시간 동안 보스가 밀려나지 않도록 실시간 좌표 강제 잠금
                boss.X = mapWidth / 2;
                boss.Y = 330;
                UpdateSystemWipe(player, effects, mapWidth);
            }
            else
            {
                if (accessDeniedBarrageTimer > 0)
                {
                    accessDeniedBarrageTimer--;
                    if (accessDeniedBarrageTimer % 15 == 0)
                        SkyMissiles.Add(new SkyMissile { X = player.X + rand.Next(-150, 150), Y = player.Y + rand.Next(-100, 100), Timer = 60 });
                    if (accessDeniedBarrageTimer == 0) IsAccessDeniedActive = false;
                }

                basicAttackTimer++;
                if (basicAttackTimer >= 90)
                {
                    float dx = player.X - boss.X; float dy = (player.Y - 20) - (boss.Y - 40);
                    float angle = (float)Math.Atan2(dy, dx);
                    Projectiles.Add(new BossProjectile(boss.X, boss.Y - 40, (float)Math.Cos(angle) * 8f, (float)Math.Sin(angle) * 8f, boss.Attack / 3));
                    basicAttackTimer = 0;
                }
            }

            boss.IsCastingPattern = (IsEnrageActive || IsSystemWipeActive || IsAccessDeniedActive || accessDeniedBarrageTimer > 0);
        }

        private void StartSystemWipe(PlayerState player, float mapWidth)
        {
            IsSystemWipeActive = true;
            SystemWipeCount = 1; // 1번째 웨이브 세션 스타트
            NextSystemWipe(player, mapWidth);
        }

        private void NextSystemWipe(PlayerState player, float mapWidth)
        {
            SystemWipeTimer = 120; // 3초 후 폭발 타이머 스타트
            SystemWipeWaitTimer = 0;

            float bossX = mapWidth / 2;
            float bossY = 330;
            float szX = 0, szY = 0;
            int attempts = 0;

            // 💡 [수정] 보스 근처 반경 280px 이내에는 안전구역 스폰을 절대 거부하는 최소 안전거리 필터 알고리즘
            do
            {
                szX = rand.Next(200, (int)mapWidth - 200);
                szY = rand.Next(220, 620);
                attempts++;
            } while (Math.Sqrt(Math.Pow(szX - bossX, 2) + Math.Pow(szY - bossY, 2)) < 280f && attempts < 100);

            SafeZoneCenter = new PointF(szX, szY);
            //NoticeText = $"🚨 커널 무결성 검사 ({SystemWipeCount}/3): 안전구역 외부 포맷 대기 중!";
            //NoticeTicks = 90;
        }

        private void UpdateSystemWipe(PlayerState player, List<Effect> effects, float mapWidth)
        {
            // 한 파트가 터진 뒤 다음 스폰까지 쉬어가는 안전 대기 시간 연산
            if (SystemWipeWaitTimer > 0)
            {
                SystemWipeWaitTimer--;
                if (SystemWipeWaitTimer <= 0)
                {
                    if (SystemWipeCount < 3)
                    {
                        SystemWipeCount++;
                        NextSystemWipe(player, mapWidth); // 다음 안전구역 스폰
                    }
                    else
                    {
                        IsSystemWipeActive = false; // 3번 완수 시 기믹 최종 종료
                    }
                }
                return;
            }

            SystemWipeTimer--;

            // 폭발 타이머 제로 도달 시점 (데토네이션 판정)
            if (SystemWipeTimer <= 0)
            {
                float dx = player.X - SafeZoneCenter.X;
                float dy = player.Y - SafeZoneCenter.Y;

                // 💡 [수정] 안전구역(반경 80) 밖에 단 1픽셀이라도 벗어나 있으면 피격 무적 상관없이 무조건 즉사 및 강퇴
                if (Math.Sqrt(dx * dx + dy * dy) > SafeZoneRadius)
                {
                    player.Hp = 0; // 즉각 전역 사망 필터 작동 -> 바탕화면 강제 추방
                }

                effects.Add(new Effect("burst", SafeZoneCenter.X, SafeZoneCenter.Y, player.X, player.Y, 40, Color.Red, "SYSTEM WIPE"));
                SystemWipeWaitTimer = 90; // 폭발 후 3초(90틱) 여유 시간을 준 뒤 다음 구역 생성
            }
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

            // 💡 [수정] 전체 600틱 유지 시간 중 정확히 절반인 300틱을 기준으로 회전 방향을 1회 전격 반전!
            // 소울류에 걸맞게 레이저 회전 기본 속도 자체도 기존 0.013f에서 0.026f로 2배 가속화합니다.
            float speed = (LotusTimer > 300) ? 0.026f : -0.026f;
            LotusAngle += speed;

            // 십자 방향 회전 레이저 충돌 판정
            for (int i = 0; i < 4; i++)
            {
                float checkAngle = LotusAngle + (float)(i * Math.PI / 2);
                float nx = -(float)Math.Sin(checkAngle);
                float ny = (float)Math.Cos(checkAngle);
                float dx = player.X - boss.X;
                float dy = (player.Y - 25) - (boss.Y - 20);

                float distanceToLine = Math.Abs(dx * nx + dy * ny);

                if (distanceToLine < 18f) // 💡 판정 범위를 18f로 미세 상향
                {
                    if (player.InvincibleTicks <= 0)
                    {
                        // 💡 소울류 밸런스: 스치면 체력의 30%가 유실되는 흉악한 데미지 부여
                        player.Hp -= (int)(player.MaxHp * 0.30f);
                        player.InvincibleTicks = 20; // 무적 프레임도 단축하여 연격 위험성 상향
                        effects.Add(new Effect("text", player.X, player.Y - 60, player.X, player.Y - 60, 30, Color.Cyan, "시스템 과전류 감전! -30%"));
                    }
                }
            }
            if (LotusTimer % 35 == 0) PerformBSODBasicAttack(boss, player);
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

            NoticeText = "☄️ 커널 임계 오버클록: 무자비한 속도의 융단 폭격이 시작됩니다! ";
            NoticeTicks = 2;

            galagaTimer++;
            if (galagaTimer >= 14) // 💡 생성 주기를 더 타이트하게 상향 (18 -> 14)
            {
                galagaTimer = 0;
                int spawnCount = rand.Next(4, 6); // 💡 한 웨이브당 운석 개수 증가
                for (int i = 0; i < spawnCount; i++)
                {
                    float rx = rand.Next(100, (int)mapWidth - 100);

                    // 💡 [수정] 운석 낙하 속도를 광속 수준으로 대폭 증가 (기존 7f -> 최소 18f ~ 최대 26f)
                    float fallSpeed = 18f + (float)rand.NextDouble() * 8f;

                    // 💡 소울류 밸런스: 운석 하나당 데미지를 무지막지하게 대폭 강화
                    Projectiles.Add(new BossProjectile(rx, 0, rand.Next(-2, 3), fallSpeed, boss.Attack * 2, true));
                }
            }
        }

        //private void StartSystemWipe(PlayerState player, float mapWidth) { IsSystemWipeActive = true; SystemWipeCount = 0; NextSystemWipe(player, mapWidth); }
        //private void NextSystemWipe(PlayerState player, float mapWidth)
        //{
        //    // 💡 [밸런스 패치] 30 FPS 기준 5초는 정확히 150틱입니다! (기존 300틱에서 단축)
        //    SystemWipeTimer = 150;
        //    SystemWipeWaitTimer = 0;
        //    SafeZoneCenter = new PointF(player.X + rand.Next(-200, 200), rand.Next(250, 450));

        //    // 💡 패턴 시작 시 상단 멘트 실시간 동기화
        //    NoticeText = "🚨 커널 무결성 검사: 5초 뒤 안전구역을 제외한 전 구역이 폭발합니다! 안전구역 내부로 대피하세요!";
        //    NoticeTicks = 150;
        //}
        //private void UpdateSystemWipe(PlayerState player, List<Effect> effects, float mapWidth)
        //{
        //    if (SystemWipeWaitTimer > 0)
        //    {
        //        SystemWipeWaitTimer--;
        //        if (SystemWipeWaitTimer <= 0) { if (SystemWipeCount < 3) { SystemWipeCount++; NextSystemWipe(player, mapWidth); } else IsSystemWipeActive = false; }
        //        return;
        //    }
        //    SystemWipeTimer--;
        //    if (SystemWipeTimer > 0)
        //    {
        //        NoticeText = "🚨 커널 무결성 검사: 5초 뒤 안전구역을 제외한 전 구역이 폭발합니다! 안전구역 내부로 대피하세요!";
        //        NoticeTicks = 2;
        //    }

        //    if (SystemWipeTimer <= 0)
        //    {
        //        float dx = player.X - SafeZoneCenter.X; float dy = player.Y - SafeZoneCenter.Y;

        //        // 💡 [즉사 메커니즘] 범위(SafeZoneRadius = 80) 밖에 있으면 즉사 대미지 판정
        //        if (Math.Sqrt(dx * dx + dy * dy) > SafeZoneRadius)
        //        {
        //            player.Hp = 0; // 즉사
        //        }
        //        effects.Add(new Effect("burst", SafeZoneCenter.X, SafeZoneCenter.Y, player.X, player.Y, 40, Color.Red, "WIPE"));
        //        SystemWipeWaitTimer = 180;
        //    }
        //}
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
            // 2. 50% 패턴: 디버그 팝업 지옥
            if (hpPercent <= 50 && !boss.Pattern50Used && !IsShardPatternActive && !IsResourcePatternActive)
            {
                boss.Pattern50Used = true;
                IsResourcePatternActive = true;

                // 5초 150틱
                ResourceTimer = 150;

                DebugButtons.Clear();
                for (int i = 0; i < 13; i++)
                {
                    int btnW = 90; int btnH = 32;
                    int rx = rand.Next(100, 1150);
                    int ry = rand.Next(160, 520);
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
        private void UpdateResourcePattern(PlayerState player, List<Effect> effects)
        {
            ResourceTimer--;
            if (ResourceTimer <= 0)
            {
                if (DebugButtons.Count > 0)
                {
                    // 💡 [수정] 시간 내에 13개를 다 누르지 못하면 무조건 즉사 분기 실행 및 자동 퇴장 조치
                    player.Hp = 0;
                    effects.Add(new Effect("text", player.X, player.Y, player.X, player.Y, 60, Color.Red, "SYSTEM FATAL CRASH: TIME OVER"));
                }
                IsResourcePatternActive = false;
            }
        }
        private void StartShardPattern(float mapWidth) { IsShardPatternActive = true; ShardSequence = 0; SpawnNextShard(mapWidth); }
        private void SpawnNextShard(float mapWidth) { ShardTimer = 300; int minX = 300; int maxX = (int)Math.Max(minX + 100, mapWidth - 300); CurrentShardPos = new PointF(rand.Next(minX, maxX), rand.Next(250, 420)); }
        private void UpdateShardPattern(PlayerState player, List<Effect> effects, float mapWidth) { ShardTimer--; float dx = player.X - CurrentShardPos.X; float dy = player.Y - CurrentShardPos.Y; if (Math.Sqrt(dx * dx + dy * dy) < 80) { effects.Add(new Effect("burst", CurrentShardPos.X, CurrentShardPos.Y, CurrentShardPos.X, CurrentShardPos.Y, 20, Color.Lime, "FIXED")); ProgressShardPattern(mapWidth); return; } if (ShardTimer <= 0) { player.Hp -= (int)(player.MaxHp * 0.15f); effects.Add(new Effect("text", player.X, player.Y, player.X, player.Y, 40, Color.OrangeRed, "PATCH FAILED: -15% HP")); ProgressShardPattern(mapWidth); } }
        private void ProgressShardPattern(float mapWidth) { ShardSequence++; if (ShardSequence < 3) SpawnNextShard(mapWidth); else IsShardPatternActive = false; }

        // ==========================================================
        // 💡 [패치] 2번 보스 10% 발악 운석 3회 피격 즉사 및 강퇴 연동 엔진
        // ==========================================================
        private void UpdateProjectiles(PlayerState player, List<Effect> effects, float mapWidth)
        {
            for (int i = Projectiles.Count - 1; i >= 0; i--)
            {
                var p = Projectiles[i];
                p.X += p.VX;
                p.Y += p.VY;

                float dist = (float)Math.Sqrt(Math.Pow(player.X - p.X, 2) + Math.Pow((player.Y - 25) - p.Y, 2));

                // 일반 탄막 반경은 20px이지만, 발악 운석은 45px로 판정 범위를 대폭 확대!
                float checkRadius = p.IsEnrageMissile ? 45f : 20f;

                // 꼼수 방지: 10% 발악 운석은 플레이어의 피격 무적 프레임을 완전히 무시하고 실시간 누적 타격 판정을 가집니다.
                bool isHit = p.IsEnrageMissile ? (dist < checkRadius) : (dist < checkRadius && player.InvincibleTicks <= 0);

                if (isHit)
                {
                    if (p.IsEnrageMissile)
                    {
                        EnrageHitCount++;
                        effects.Add(new Effect("text", player.X, player.Y - 70, player.X, player.Y - 70, 40, Color.Red, $"운석 충돌! ({EnrageHitCount}/3)"));

                        // 정확히 3회 충돌 시 즉사 시퀀스 가동
                        if (EnrageHitCount >= 3)
                        {
                            player.Hp = 0; // HP를 즉시 0으로 만들어 MainForm의 전역 퇴장 핸들러를 트리거합니다.
                            effects.Add(new Effect("text", player.X, player.Y - 110, player.X, player.Y - 110, 60, Color.DarkRed, "LIMIT OVER: 3회 피격 즉사"));
                        }
                    }
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

                    // 운석은 투사체 자체를 즉시 소멸시키므로 단일 객체 다중 히트를 방지하며, 무적 시간 계산을 독립 제어합니다.
                    if (!p.IsEnrageMissile) player.InvincibleTicks = 25;

                    Projectiles.RemoveAt(i);
                    continue;
                }

                if (p.X < 0 || p.X > mapWidth + 500)
                {
                    Projectiles.RemoveAt(i);
                }
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