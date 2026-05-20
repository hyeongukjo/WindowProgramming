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

            UpdateProjectiles(player, effects, mapWidth);
            UpdateSkyMissiles(player, effects);

            if (boss.Name == "Driver-K") UpdateDriverK(boss, player, effects, mapWidth);
            else if (boss.Name == "High-Kernel") UpdateHighKernel(boss, player, effects, mapWidth);
            else if (boss.Name == "BSOD") UpdateBSOD(boss, player, effects, mapWidth);
            else if (boss.Name == "Exception Queen") UpdateExceptionQueen(boss, player, effects, mapWidth);
            else if (boss.Name == "Illegal_Binny") UpdateBinny(boss, player, effects, mapWidth);
        }

        // ==========================================
        // 5번 보스 (Illegal_Binny) 메인 로직
        // ==========================================
        private void UpdateBinny(GameEntity boss, PlayerState player, List<Effect> effects, float mapWidth)
        {
            float hpPercent = (float)boss.Hp / boss.MaxHp * 100;

            if (IsBinny1Used && IsIllusionActive)
            {
                UpdateBinnyIllusion(boss, player, effects, mapWidth);
                return;
            }
            if (hpPercent <= 1.0f && !IsBinny1Used && !IsDPSCheckActive)
            {
                IsBinny1Used = true;
                IsIllusionActive = true;
                boss.MaxHp = 50; boss.Hp = 50;

                // [수정 4] 1% 분신 패턴 시 양 끝이 아닌 화면 중앙에서 35%, 65% 떨어진 좁은 구역으로 소환
                boss.X = mapWidth * 0.35f; boss.Y = 330;
                BinnyClone = new BossClone { X = mapWidth * 0.65f, Y = 330, Hp = 50, MaxHp = 50 };

                IllusionTimer = 900;
                SyncTimer = 180;
                effects.Add(new Effect("text", mapWidth / 2, 200, mapWidth / 2, 200, 100, Color.Red, "!!! 환영 동기화 프로토콜 가동 !!!"));
                return;
            }

            if (((hpPercent <= 75 && !IsBinny75Used) || (hpPercent <= 25 && boss.Pattern25Used == false)) && !IsScannerActive && !IsDPSCheckActive)
            {
                IsBlackholeActive = true;
                if (hpPercent <= 25) boss.Pattern25Used = true; else IsBinny75Used = true;
                boss.X = mapWidth / 2; boss.Y = 330;
                AnchorPos = new PointF(boss.X + (rand.Next(0, 2) == 0 ? -400 : 400), 330);
                AnchorTicks = 0;
            }

            if (hpPercent <= 50 && !IsBinny50Used && !IsBlackholeActive)
            {
                IsScannerActive = true;
                IsBinny50Used = true;
                ScannerPassCount = 0; ScannerDir = 1f; ScannerSpeed = 8f; ScannerX = 0f;
                SafeHoleY = rand.Next(200, 550);
            }

            if (hpPercent <= 10 && !IsBinny10Used && !IsBlackholeActive && !IsScannerActive)
            {
                IsDPSCheckActive = true;
                IsBinny10Used = true;
                BinnyShield = 800;
                DPSCheckTimer = 900;
                effects.Add(new Effect("text", boss.X, boss.Y - 100, boss.X, boss.Y - 100, 80, Color.Red, "강제 휴지통 비우기 진행 중!"));
            }

            if (IsBlackholeActive) UpdateBinnyBlackhole(boss, player, effects);
            else if (IsScannerActive) UpdateBinnyScanner(boss, player, effects, mapWidth);
            else if (IsDPSCheckActive) UpdateBinnyDPSCheck(boss, player, effects);
            else
            {
                basicAttackTimer++;
                if (basicAttackTimer >= 70)
                {
                    PerformBinnyNormalAttack(boss, player);
                    basicAttackTimer = 0;
                }
            }

            UpdateBinnyAttacks(boss, player, effects);
        }

        private void UpdateBinnyIllusion(GameEntity boss, PlayerState player, List<Effect> effects, float mapWidth)
        {
            IllusionTimer--;
            if (IllusionTimer <= 0) { player.Hp = 0; effects.Add(new Effect("text", player.X, player.Y, player.X, player.Y, 60, Color.Red, "TIME OVER")); return; }

            IsMainDead = boss.Hp <= 0;
            IsCloneDead = BinnyClone == null || BinnyClone.Hp <= 0;

            if (IsMainDead && IsCloneDead) return;

            if ((IsMainDead && !IsCloneDead) || (!IsMainDead && IsCloneDead))
            {
                SyncTimer--;
                if (SyncTimer <= 0)
                {
                    boss.Hp = 50;
                    if (BinnyClone != null) BinnyClone.Hp = 50;
                    SyncTimer = 180;
                    effects.Add(new Effect("text", mapWidth / 2, 250, mapWidth / 2, 250, 60, Color.Red, "동기화 복구 완료!"));
                }
            }
            else
            {
                SyncTimer = 180;
            }

            basicAttackTimer++;
            if (basicAttackTimer >= 50)
            {
                if (!IsMainDead) PerformBinnyNormalAttack(boss, player);
                if (!IsCloneDead)
                {
                    BinnyAoEs.Add(new BinnyAoE(player.X, player.Y));
                }
                basicAttackTimer = 0;
            }
            UpdateBinnyAttacks(boss, player, effects);
        }

        // [5번 보스 75%] 블랙홀: 우클릭 명령을 압도하는 강력한 인력 부여
        private void UpdateBinnyBlackhole(GameEntity boss, PlayerState player, List<Effect> effects)
        {
            float dx = boss.X - player.X; float dy = boss.Y - player.Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);

            // [수정 1] 인력 계수를 2.8에서 5.5로 대폭 올려 확실히 빨려들어가게 하고, 마우스 목표 지점(TargetX/Y)도 강제로 당겨 조작감을 마비시킴
            if (dist > 10)
            {
                player.X += (dx / dist) * 5.5f;
                player.Y += (dy / dist) * 5.5f;
                player.TargetX += (boss.X - player.TargetX) * 0.05f;
                player.TargetY += (boss.Y - player.TargetY) * 0.05f;
            }
            if (dist < 40) { player.Hp = 0; effects.Add(new Effect("text", player.X, player.Y, player.X, player.Y, 60, Color.Red, "SHIFT + DELETE")); return; }

            float aDist = (float)Math.Sqrt(Math.Pow(player.X - AnchorPos.X, 2) + Math.Pow(player.Y - AnchorPos.Y, 2));
            if (aDist < 85) AnchorTicks++;
            else AnchorTicks = Math.Max(0, AnchorTicks - 2);

            if (AnchorTicks >= 180)
            {
                IsBlackholeActive = false;
                effects.Add(new Effect("text", boss.X, boss.Y - 100, boss.X, boss.Y - 100, 60, Color.Lime, "블랙홀 저항 성공"));
            }
        }

        // [5번 보스 50%] 스캐너 포맷: 무적 프레임 무시 즉사
        private void UpdateBinnyScanner(GameEntity boss, PlayerState player, List<Effect> effects, float mapWidth)
        {
            ScannerX += ScannerSpeed * ScannerDir;

            if (ScannerDir > 0 && ScannerX > mapWidth) { ScannerDir = -1; ScannerPassCount++; SafeHoleY = rand.Next(200, 550); ScannerSpeed += 2.5f; ScannerX = mapWidth; }
            else if (ScannerDir < 0 && ScannerX < 0) { ScannerDir = 1; ScannerPassCount++; SafeHoleY = rand.Next(200, 550); ScannerSpeed += 2.5f; ScannerX = 0; }

            if (ScannerPassCount >= 4) { IsScannerActive = false; return; }

            // [수정 2] 무적(InvincibleTicks) 체크 삭제, 레이저에 닿으면 즉시 사망 처리
            if (Math.Abs(player.X - ScannerX) < 22)
            {
                if (Math.Abs(player.Y - SafeHoleY) > 55)
                {
                    player.Hp = 0;
                    effects.Add(new Effect("text", player.X, player.Y, player.X, player.Y, 60, Color.Red, "FORMATTED"));
                }
            }
        }

        // [5번 보스 10%] DPS 체크 지뢰 폭격 난이도 상향
        private void UpdateBinnyDPSCheck(GameEntity boss, PlayerState player, List<Effect> effects)
        {
            DPSCheckTimer--;
            if (DPSCheckTimer <= 0)
            {
                if (BinnyShield > 0) { player.Hp = 0; effects.Add(new Effect("text", player.X, player.Y, player.X, player.Y, 60, Color.Red, "RECYCLE BIN EMPTIED")); }
                IsDPSCheckActive = false;
                return;
            }
            if (BinnyShield <= 0) { IsDPSCheckActive = false; effects.Add(new Effect("text", boss.X, boss.Y - 100, boss.X, boss.Y - 100, 60, Color.Lime, "SHIELD BROKEN")); return; }

            // [수정 3] 기존 24틱 주기를 6틱으로 대폭 줄여 지뢰가 기관총처럼 발밑과 주변에 터지게 변경
            if (DPSCheckTimer % 6 == 0)
            {
                BinnyAoEs.Add(new BinnyAoE(player.X, player.Y));
                BinnyAoEs.Add(new BinnyAoE(player.X + rand.Next(-150, 150), player.Y + rand.Next(-80, 80)));
            }
        }

        private void PerformBinnyNormalAttack(GameEntity boss, PlayerState player)
        {
            int atk = rand.Next(0, 3);
            if (atk == 0) BinnyAoEs.Add(new BinnyAoE(player.X, player.Y));
            else if (atk == 1)
            {
                float dx = player.X - boss.X; float dy = (player.Y - 20) - (boss.Y - 20);
                float dist = (float)Math.Sqrt(dx * dx + dy * dy); if (dist < 1) dist = 1;
                BinnyBoomerangs.Add(new BinnyBoomerang { StartX = boss.X, StartY = boss.Y - 20, X = boss.X, Y = boss.Y - 20, VX = (dx / dist) * 9f, VY = (dy / dist) * 9f });
            }
            else if (atk == 2) BinnyRings.Add(new BinnyRing { X = boss.X, Y = boss.Y, Radius = 10f });
        }

        private void UpdateBinnyAttacks(GameEntity boss, PlayerState player, List<Effect> effects)
        {
            for (int i = BinnyAoEs.Count - 1; i >= 0; i--)
            {
                BinnyAoEs[i].Timer--;
                if (BinnyAoEs[i].Timer <= 0)
                {
                    float dist = (float)Math.Sqrt(Math.Pow(player.X - BinnyAoEs[i].X, 2) + Math.Pow(player.Y - BinnyAoEs[i].Y, 2));
                    if (dist < 45 && player.InvincibleTicks <= 0) { player.Hp -= 15; player.InvincibleTicks = 20; effects.Add(new Effect("text", player.X, player.Y - 50, player.X, player.Y - 50, 30, Color.Red, "-15")); }
                    effects.Add(new Effect("burst", BinnyAoEs[i].X, BinnyAoEs[i].Y, BinnyAoEs[i].X, BinnyAoEs[i].Y, 20, Color.OrangeRed, ""));
                    BinnyAoEs.RemoveAt(i);
                }
            }
            for (int i = BinnyBoomerangs.Count - 1; i >= 0; i--)
            {
                var b = BinnyBoomerangs[i]; b.Timer--;
                if (b.Timer < 45 && !b.Returning) { b.Returning = true; b.VX = 0; b.VY = 0; }
                if (b.Timer < 35 && b.Returning)
                {
                    float dx = boss.X - b.X; float dy = (boss.Y - 20) - b.Y; float dist = (float)Math.Sqrt(dx * dx + dy * dy); if (dist < 1) dist = 1;
                    b.VX = (dx / dist) * 12f; b.VY = (dy / dist) * 12f;
                    if (dist < 30) { BinnyBoomerangs.RemoveAt(i); continue; }
                }
                b.X += b.VX; b.Y += b.VY;
                if (Math.Sqrt(Math.Pow(player.X - b.X, 2) + Math.Pow((player.Y - 25) - b.Y, 2)) < 25 && player.InvincibleTicks <= 0)
                {
                    player.Hp -= 15; player.InvincibleTicks = 20; effects.Add(new Effect("text", player.X, player.Y - 50, player.X, player.Y - 50, 30, Color.Red, "-15"));
                }
            }
            for (int i = BinnyRings.Count - 1; i >= 0; i--)
            {
                var r = BinnyRings[i]; r.Radius += 7.5f;
                if (!r.HasHit && Math.Abs(Math.Sqrt(Math.Pow(player.X - r.X, 2) + Math.Pow(player.Y - r.Y, 2)) - r.Radius) < 15)
                {
                    if (player.InvincibleTicks <= 0) { player.Hp -= 20; player.InvincibleTicks = 20; effects.Add(new Effect("text", player.X, player.Y - 50, player.X, player.Y - 50, 30, Color.Red, "파동 피격!")); }
                    r.HasHit = true;
                }
                if (r.Radius > 1200) BinnyRings.RemoveAt(i);
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
            float hpPercent = (float)boss.Hp / boss.MaxHp * 100;

            // 1. 75%, 25% 패턴: NullReference 연결
            if (((hpPercent <= 75 && !boss.Pattern75Used) || (hpPercent <= 25 && !boss.Pattern25Used)) && !IsTryCatchActive)
            {
                IsNullRefActive = true;
                NullRefGauge = 0f;
                nullRefBarrageTimer = 0;
                NullRefNode = new PointF(rand.Next(200, (int)mapWidth - 200), rand.Next(150, 550));

                if (hpPercent <= 25) boss.Pattern25Used = true; else boss.Pattern75Used = true;
            }

            // 2. 50% 패턴: Try-Catch Block
            if (hpPercent <= 50 && !boss.Pattern50Used && !IsNullRefActive)
            {
                IsTryCatchActive = true;
                boss.Pattern50Used = true;
                boss.X = mapWidth / 2; boss.Y = 300;
                StartTryCatch(mapWidth);
            }

            // 3. 10% 패턴: StackOverflow (가비지 컬렉션)
            if (hpPercent <= 10 && !IsQueenPattern10Used)
            {
                IsStackOverflowActive = true;
                IsQueenPattern10Used = true;
                StackGauge = 0f;
                TargetSwitch = 0;
                SwitchLeftPos = new PointF(120, 330);
                SwitchRightPos = new PointF(mapWidth - 120, 330);
                IsNullRefActive = false; IsTryCatchActive = false;
            }

            if (IsNullRefActive) UpdateNullRef(boss, player, effects, mapWidth);
            else if (IsTryCatchActive) UpdateTryCatch(boss, player, effects, mapWidth); // [수정] mapWidth 매개변수 추가
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
        }

        // [4번 보스] 75%, 25% : NullReferenceException
        private void UpdateNullRef(GameEntity boss, PlayerState player, List<Effect> effects, float mapWidth)
        {
            float dx = player.X - NullRefNode.X;
            float dy = player.Y - NullRefNode.Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);

            if (dist >= 120 && dist <= 280)
            {
                NullRefGauge += 0.35f;
            }
            else
            {
                NullRefGauge -= 0.1f;
                if (NullRefGauge < 0) NullRefGauge = 0;

                if (basicAttackTimer % 15 == 0)
                {
                    player.Hp -= 2;
                    effects.Add(new Effect("text", player.X, player.Y, player.X, player.Y, 20, Color.Red, "Null Ref"));
                }
            }

            if (NullRefGauge >= 100f)
            {
                IsNullRefActive = false;
                effects.Add(new Effect("text", boss.X, boss.Y - 100, boss.X, boss.Y - 100, 60, Color.Lime, "REFERENCE RESTORED"));
            }

            // [수정] 갤러그식 사방향 탄막 발사
            nullRefBarrageTimer++;
            if (nullRefBarrageTimer >= 40) // 40틱마다 방사형 발사
            {
                nullRefBarrageTimer = 0;
                int count = 8;
                float baseAngle = (float)(rand.NextDouble() * Math.PI);
                for (int i = 0; i < count; i++)
                {
                    float angle = baseAngle + (float)(i * Math.PI * 2 / count);
                    float speed = 4f + rand.Next(0, 2);
                    Projectiles.Add(new BossProjectile(boss.X, boss.Y - 20, (float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed, 12, false, false, true));
                }
            }
            basicAttackTimer++; // 거리 페널티용 타이머 유지
        }

        // [4번 보스] 50% : Try-Catch 블록 준비
        private void StartTryCatch(float mapWidth)
        {
            TryCatchGauge = 0f;
            TryCatchTimer = 1200; // 20초 제한
            IsTypingPhaseActive = false;
            TryCatchSuccessCount = 0;
            SpawnTryCatchZones(mapWidth);
        }

        private void SpawnTryCatchZones(float mapWidth)
        {
            CatchZones.Clear();
            string[] exceptions = { "NullReference", "Format", "IndexOutOfRange" };
            TargetException = exceptions[rand.Next(exceptions.Length)];

            CatchZones.Add(new CatchZone("NullReference", mapWidth / 2 - 300, 450));
            CatchZones.Add(new CatchZone("Format", mapWidth / 2, 450));
            CatchZones.Add(new CatchZone("IndexOutOfRange", mapWidth / 2 + 300, 450));
        }

        // [4번 보스] 50% : Try-Catch 타이핑 게임 연산
        private void UpdateTryCatch(GameEntity
            
            boss, PlayerState player, List<Effect> effects, float mapWidth)
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
                // 플레이어가 올바른 장판 안에 들어갔는지 확인
                foreach (var zone in CatchZones)
                {
                    float dx = player.X - zone.X;
                    float dy = player.Y - zone.Y;
                    if (Math.Sqrt(dx * dx + dy * dy) <= 70f)
                    {
                        if (zone.ExceptionName == TargetException)
                        {
                            // 정답 장판에 진입 -> 타이핑 페이즈 시작 (5초 부여)
                            IsTypingPhaseActive = true;
                            TypingTimer = 300;
                            CurrentTypingTarget = GenerateTypingString(8);
                            effects.Add(new Effect("text", player.X, player.Y - 80, player.X, player.Y - 80, 40, Color.Lime, "HACKING STARTED"));
                        }
                        else
                        {
                            // 오답 장판 진입 시 대미지 페널티
                            if (TryCatchTimer % 30 == 0)
                            {
                                player.Hp -= 5;
                                effects.Add(new Effect("text", player.X, player.Y - 80, player.X, player.Y - 80, 30, Color.Red, "WRONG ZONE"));
                            }
                        }
                    }
                }
            }
            else
            {
                TypingTimer--;

                // 타이핑 중 장판을 벗어나면 취소 처리
                bool inZone = false;
                foreach (var zone in CatchZones)
                {
                    if (zone.ExceptionName == TargetException)
                    {
                        float dx = player.X - zone.X; float dy = player.Y - zone.Y;
                        if (Math.Sqrt(dx * dx + dy * dy) <= 70f) inZone = true;
                    }
                }

                if (!inZone)
                {
                    IsTypingPhaseActive = false;
                    effects.Add(new Effect("text", player.X, player.Y - 80, player.X, player.Y - 80, 40, Color.Orange, "CANCELED"));
                    return;
                }

                // 5초 타임오버 시 페널티 및 리셋
                if (TypingTimer <= 0)
                {
                    IsTypingPhaseActive = false;
                    player.Hp -= (int)(player.MaxHp * 0.15f);
                    effects.Add(new Effect("text", player.X, player.Y - 80, player.X, player.Y - 80, 40, Color.Red, "TIMEOUT"));
                }
            }
        }

        private string GenerateTypingString(int length)
        {
            char[] arr = new char[length];
            for (int i = 0; i < length; i++) arr[i] = allowedChars[rand.Next(allowedChars.Length)];
            return new string(arr);
        }

        // [외부 호출] 폼의 키 이벤트를 받아서 타이핑 미니게임을 처리합니다.
        public void HandleTypingInput(System.Windows.Forms.Keys key, PlayerState player, List<Effect> effects, float mapWidth)
        {
            if (!IsTryCatchActive || !IsTypingPhaseActive) return;

            char typedChar = '\0';

            // 키보드 입력을 문자로 변환
            if (key >= System.Windows.Forms.Keys.A && key <= System.Windows.Forms.Keys.Z) typedChar = key.ToString()[0];
            else if (key >= System.Windows.Forms.Keys.D0 && key <= System.Windows.Forms.Keys.D9) typedChar = (char)('0' + (key - System.Windows.Forms.Keys.D0));
            else if (key >= System.Windows.Forms.Keys.NumPad0 && key <= System.Windows.Forms.Keys.NumPad9) typedChar = (char)('0' + (key - System.Windows.Forms.Keys.NumPad0));

            if (typedChar != '\0' && CurrentTypingTarget.Length > 0)
            {
                if (CurrentTypingTarget[0] == typedChar)
                {
                    // 정답: 첫 글자 제거
                    CurrentTypingTarget = CurrentTypingTarget.Substring(1);

                    // 모두 입력 성공 시
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
                            SpawnTryCatchZones(mapWidth); // 성공 시 다음 타겟 장판 생성
                        }
                    }
                }
                else
                {
                    effects.Add(new Effect("text", player.X, player.Y - 60, player.X, player.Y - 60, 20, Color.Red, "TYPO!"));
                }
            }
        }

        // [4번 보스] 10% : 스택 오버플로우
        private void UpdateStackOverflow(GameEntity boss, PlayerState player, List<Effect> effects)
        {
            StackGauge += 0.084f;

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
            if (Math.Sqrt(dx * dx + dy * dy) < 60f)
            {
                StackGauge = 0f;
                TargetSwitch = TargetSwitch == 0 ? 1 : 0;
                effects.Add(new Effect("burst", currentTarget.X, currentTarget.Y, currentTarget.X, currentTarget.Y, 30, Color.Lime, "FLUSH"));
                effects.Add(new Effect("text", player.X, player.Y - 60, player.X, player.Y - 60, 40, Color.Lime, "GARBAGE COLLECTED"));
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
            Projectiles.Add(new BossProjectile(boss.X, boss.Y - 20, (dx / dist) * 10f, (dy / dist) * 10f, 15, false, false, true));
        }

        // --- 기존 보스들 Update 유지 ---
        private void UpdateHighKernel(GameEntity boss, PlayerState player, List<Effect> effects, float mapWidth)
        {
            float hpPercent = (float)boss.Hp / boss.MaxHp * 100;

            if (hpPercent <= 10 && !IsPattern10Used)
            {
                IsPattern10Used = true;
                StartEnrage(boss, mapWidth, effects);
            }
            else if (hpPercent <= 50 && !boss.Pattern50Used && !IsEnrageActive)
            {
                boss.Pattern50Used = true;
                StartSystemWipe(player, mapWidth);
            }
            else if (((hpPercent <= 75 && !boss.Pattern75Used) || (hpPercent <= 25 && !boss.Pattern25Used)) && !IsSystemWipeActive)
            {
                if (hpPercent <= 25) boss.Pattern25Used = true; else boss.Pattern75Used = true;
                IsAccessDeniedActive = true;
                accessDeniedBarrageTimer = 240;
            }

            if (IsEnrageActive) UpdateEnrage(boss, player, effects, mapWidth);
            else if (IsSystemWipeActive) UpdateSystemWipe(player, effects, mapWidth);
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

        private void UpdateBSOD(GameEntity boss, PlayerState player, List<Effect> effects, float mapWidth)
        {
            float hpPercent = (float)boss.Hp / boss.MaxHp * 100;

            if (((hpPercent <= 75 && !boss.Pattern75Used) || (hpPercent <= 25 && !boss.Pattern25Used)) && !IsLeakActive)
            {
                IsLotusActive = true;
                LotusTimer = 600;
                if (hpPercent <= 25) boss.Pattern25Used = true; else boss.Pattern75Used = true;
                boss.X = mapWidth / 2; boss.Y = 330;
            }

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

            if (hpPercent <= 10 && !IsMagnusActive)
            {
                IsMagnusActive = true;
                MagnusWidth = 750f;
                MagnusHeight = 240f;
                MagnusTimer = 0;
                IsLotusActive = false; IsLeakActive = false;
            }

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

        private void SpawnNextPatch(float mapWidth)
        {
            CurrentPatchPos = new PointF(
                rand.Next(200, (int)mapWidth - 200),
                rand.Next(150, 600)
            );
            StandTicks = 0;
        }

        private void UpdateLeak(GameEntity boss, PlayerState player, List<Effect> effects, float mapWidth)
        {
            LeakTimer--;
            boss.X = mapWidth / 2; boss.Y = 330;

            if (LeakTimer <= 0)
            {
                player.Hp = 0;
                effects.Add(new Effect("text", player.X, player.Y, player.X, player.Y, 60, Color.Red, "FATAL ERROR: BLUE SCREEN"));
                IsLeakActive = false;
                return;
            }

            leakBarrageTimer++;
            if (leakBarrageTimer >= 15)
            {
                leakBarrageTimer = 0;
                int count = 8;
                float baseAngle = (float)(rand.NextDouble() * Math.PI);
                for (int i = 0; i < count; i++)
                {
                    float angle = baseAngle + (float)(i * Math.PI * 2 / count);
                    float speed = 4.5f + rand.Next(0, 3);
                    Projectiles.Add(new BossProjectile(boss.X, boss.Y - 20, (float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed, 12));
                }
            }

            float dx = player.X - CurrentPatchPos.X;
            float dy = player.Y - CurrentPatchPos.Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);

            if (dist <= PatchRadius)
            {
                StandTicks++;
                if (StandTicks % 15 == 0)
                    effects.Add(new Effect("text", player.X, player.Y - 50, player.X, player.Y - 50, 20, Color.Lime, "PATCHING..."));

                if (StandTicks >= 60)
                {
                    PatchCount++;
                    effects.Add(new Effect("burst", CurrentPatchPos.X, CurrentPatchPos.Y, CurrentPatchPos.X, CurrentPatchPos.Y, 25, Color.Lime, ""));

                    if (PatchCount >= 3)
                    {
                        IsLeakActive = false;
                        effects.Add(new Effect("text", boss.X, boss.Y - 100, boss.X, boss.Y - 100, 60, Color.Lime, "DEBUG COMPLETE"));
                    }
                    else
                    {
                        SpawnNextPatch(mapWidth);
                    }
                }
            }
            else
            {
                StandTicks = 0;
            }
        }

        private void UpdateLotus(GameEntity boss, PlayerState player, List<Effect> effects)
        {
            LotusTimer--;
            float speed = (LotusTimer > 300) ? 0.013f : -0.013f;
            LotusAngle += speed;

            for (int i = 0; i < 4; i++)
            {
                float checkAngle = LotusAngle + (float)(i * Math.PI / 2);

                float nx = -(float)Math.Sin(checkAngle);
                float ny = (float)Math.Cos(checkAngle);

                float dx = player.X - boss.X;
                float dy = (player.Y - 25) - (boss.Y - 20);

                float distanceToLine = Math.Abs(dx * nx + dy * ny);

                if (distanceToLine < 15f)
                {
                    if (player.InvincibleTicks <= 0)
                    {
                        player.Hp -= (int)(player.MaxHp * 0.1f);
                        player.InvincibleTicks = 30;
                        effects.Add(new Effect("text", player.X, player.Y - 60, player.X, player.Y - 60, 30, Color.Cyan, "-10% HP"));
                    }
                }
            }
            if (LotusTimer % 45 == 0) PerformBSODBasicAttack(boss, player);
            if (LotusTimer <= 0) IsLotusActive = false;
        }

        private void UpdateMagnus(GameEntity boss, PlayerState player, List<Effect> effects)
        {
            MagnusTimer++;
            float moveX = (player.X - boss.X) * 0.05f;
            float moveY = (player.Y - boss.Y) * 0.05f;
            boss.X += moveX; boss.Y += moveY;

            if (MagnusTimer % 300 == 0) MagnusWidth = Math.Max(100f, MagnusWidth - 150f);

            RectangleF safeZone = new RectangleF(boss.X - MagnusWidth / 2, boss.Y - MagnusHeight / 2, MagnusWidth, MagnusHeight);
            if (!safeZone.Contains(player.X, player.Y) || MagnusWidth <= 0)
            {
                if (MagnusTimer % 60 == 0)
                {
                    player.Hp -= (int)(player.MaxHp * 0.3f);
                    effects.Add(new Effect("text", player.X, player.Y, player.X, player.Y, 40, Color.DeepSkyBlue, "SYSTEM HALT"));
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
            galagaTimer++;
            if (galagaTimer >= 20)
            {
                galagaTimer = 0;
                for (int i = 0; i < 3; i++) Projectiles.Add(new BossProjectile(boss.X, 220 + (i * 100), -12f, 0, 0, true));
            }
        }

        private void StartSystemWipe(PlayerState player, float mapWidth) { IsSystemWipeActive = true; SystemWipeCount = 0; NextSystemWipe(player, mapWidth); }
        private void NextSystemWipe(PlayerState player, float mapWidth)
        {
            SystemWipeTimer = 300; SystemWipeWaitTimer = 0;
            SafeZoneCenter = new PointF(player.X + rand.Next(-200, 200), rand.Next(250, 450));
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
            if (SystemWipeTimer <= 0)
            {
                float dx = player.X - SafeZoneCenter.X; float dy = player.Y - SafeZoneCenter.Y;
                if (Math.Sqrt(dx * dx + dy * dy) > SafeZoneRadius) player.Hp = 0;
                effects.Add(new Effect("burst", SafeZoneCenter.X, SafeZoneCenter.Y, player.X, player.Y, 40, Color.Red, "WIPE"));
                SystemWipeWaitTimer = 180;
            }
        }

        private void UpdateDriverK(GameEntity boss, PlayerState player, List<Effect> effects, float mapWidth)
        {
            float hpPercent = (float)boss.Hp / boss.MaxHp * 100;
            if (((hpPercent <= 75 && !boss.Pattern75Used) || (hpPercent <= 25 && !boss.Pattern25Used)) && !IsShardPatternActive && !IsResourcePatternActive)
            {
                StartShardPattern(mapWidth);
                if (hpPercent <= 25) boss.Pattern25Used = true; else boss.Pattern75Used = true;
            }
            if (hpPercent <= 50 && !boss.Pattern50Used && !IsResourcePatternActive && !IsShardPatternActive)
            {
                StartResourcePattern(); boss.Pattern50Used = true;
            }

            if (IsResourcePatternActive) UpdateResourcePattern(player, effects);
            else if (IsShardPatternActive) UpdateShardPattern(player, effects, mapWidth);
            else
            {
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
                    if (Math.Sqrt(Math.Pow(player.X - sm.X, 2) + Math.Pow(player.Y - sm.Y, 2)) < 60 && player.InvincibleTicks <= 0) { player.Hp -= (int)(player.MaxHp * 0.15f); player.InvincibleTicks = 20; }
                    effects.Add(new Effect("burst", sm.X, sm.Y, sm.X, sm.Y, 20, Color.OrangeRed, ""));
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
                    if (DebugButtons.Count == 0) IsResourcePatternActive = false;
                    return true;
                }
            }
            return false;
        }
    }
}