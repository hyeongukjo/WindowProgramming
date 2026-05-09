using System;
using System.Collections.Generic;
using System.Drawing;

namespace DebugHeroFileDungeonRPG
{
    public class BossProjectile
    {
        public float X, Y, VX, VY;
        public int Damage;
        public bool Active = true;
        public BossProjectile(float x, float y, float vx, float vy, int dmg)
        {
            X = x; Y = y; VX = vx; VY = vy; Damage = dmg;
        }
    }

    public class BossPatternManager
    {
        private Random rand = new Random();
        public List<BossProjectile> Projectiles = new List<BossProjectile>();
        private int basicAttackTimer = 0;

        // 50% 패턴: 리소스 부족
        public bool IsResourcePatternActive = false;
        public List<Rectangle> DebugButtons = new List<Rectangle>();
        public int ResourceTimer = 0;

        // 75%/25% 패턴: 드라이브 조각 찾기
        public bool IsShardPatternActive = false;
        public int ShardSequence = 0;
        public int ShardTimer = 0;
        public PointF CurrentShardPos;

        // 고정 스폰 위치 (75% 체력 시: 0, 1, 2번 / 25% 체력 시: 3, 4, 5번 사용)
        private PointF[] fixedShardPositions = new PointF[]
        {
            new PointF(550, 310),   // 75% - 1번 조각
            new PointF(1450, 410),  // 75% - 2번 조각
            new PointF(950, 260),   // 75% - 3번 조각
            new PointF(650, 430),   // 25% - 1번 조각
            new PointF(1300, 280),  // 25% - 2번 조각
            new PointF(1600, 350)   // 25% - 3번 조각
        };
        private bool isSecondPhase = false; // 25% 패턴인지 확인용 플래그

        public void Update(Monster boss, Player player, List<Effect> effects, float cameraX, float cameraY, float mapWidth)
        {
            if (boss.Hp <= 0) return;

            // 투사체 로직
            for (int i = Projectiles.Count - 1; i >= 0; i--)
            {
                var p = Projectiles[i];
                p.X += p.VX;
                p.Y += p.VY;

                if (p.X < 0 || p.X > 5000) { Projectiles.RemoveAt(i); continue; }

                float dx = player.X - p.X;
                float dy = (player.Y - 20) - p.Y;
                float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                if (distance < 40 && player.InvincibleTicks <= 0)
                {
                    player.Hp -= p.Damage;
                    player.InvincibleTicks = 20;
                    effects.Add(new Effect("text", player.X, player.Y - 60, player.X, player.Y - 60, 30, Color.HotPink, "-" + p.Damage));
                    Projectiles.RemoveAt(i);
                    continue;
                }
            }

            float hpPercent = (float)boss.Hp / boss.MaxHp * 100;

            // --- 1. 특수 기믹 체크 ---
            if (((hpPercent <= 75 && !boss.Pattern75Used) || (hpPercent <= 25 && !boss.Pattern25Used)) && !IsShardPatternActive && !IsResourcePatternActive)
            {
                // 25% 체력 패턴인지 여부를 기록
                isSecondPhase = hpPercent <= 25;
                StartShardPattern(mapWidth);
                if (isSecondPhase) boss.Pattern25Used = true; else boss.Pattern75Used = true;
            }

            if (hpPercent <= 50 && !boss.Pattern50Used && !IsResourcePatternActive && !IsShardPatternActive)
            {
                StartResourcePattern();
                boss.Pattern50Used = true;
            }

            // --- 2. 패턴 실행 로직 ---
            if (IsResourcePatternActive)
            {
                UpdateResourcePattern(player, effects);
            }
            else if (IsShardPatternActive)
            {
                UpdateShardPattern(player, effects, mapWidth);
            }
            else
            {
                basicAttackTimer++;
                if (basicAttackTimer >= 120)
                {
                    PerformBasicAttack(boss, player, effects);
                    basicAttackTimer = 0;
                }
            }
        }

        private void PerformBasicAttack(Monster boss, Player player, List<Effect> effects)
        {
            float dx = player.X - boss.X;
            float dy = (player.Y - 20) - (boss.Y - 30);
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            float speed = 6.0f;

            Projectiles.Add(new BossProjectile(
                boss.X,
                boss.Y - 30,
                (dx / dist) * speed,
                (dy / dist) * speed,
                boss.Attack / 5
            ));

            effects.Add(new Effect("projectile", boss.X, boss.Y - 30, player.X, player.Y - 30, 40, Color.MediumPurple, "ERR"));
        }

        private void StartResourcePattern()
        {
            IsResourcePatternActive = true;
            ResourceTimer = 270;
            DebugButtons.Clear();
            for (int i = 0; i < 3; i++)
                DebugButtons.Add(new Rectangle(rand.Next(400, 900), rand.Next(250, 500), 110, 40));
        }

        private void UpdateResourcePattern(Player player, List<Effect> effects)
        {
            ResourceTimer--;
            if (ResourceTimer <= 0)
            {
                if (DebugButtons.Count > 0)
                {
                    player.Hp /= 2;
                    effects.Add(new Effect("text", player.X, player.Y, player.X, player.Y, 60, Color.Red, "SYSTEM OVERLOAD: HP HALVED"));
                }
                IsResourcePatternActive = false;
            }
        }

        private void StartShardPattern(float mapWidth)
        {
            IsShardPatternActive = true;
            ShardSequence = 0;
            SpawnNextShard(mapWidth);
        }

        private void SpawnNextShard(float mapWidth)
        {
            ShardTimer = 300; // 5초 제한 (60fps 기준)

            // 지정된 6개의 위치 중 현재 순서(Sequence)와 페이즈에 맞는 좌표 할당
            int posIndex = isSecondPhase ? (ShardSequence + 3) : ShardSequence;
            CurrentShardPos = fixedShardPositions[posIndex];
        }

        private void UpdateShardPattern(Player player, List<Effect> effects, float mapWidth)
        {
            ShardTimer--;

            float dx = player.X - CurrentShardPos.X;
            float dy = (player.Y - 20) - CurrentShardPos.Y;

            // 충돌 판정 (범위 80)
            if (Math.Sqrt(dx * dx + dy * dy) < 80)
            {
                effects.Add(new Effect("burst", CurrentShardPos.X, CurrentShardPos.Y, CurrentShardPos.X, CurrentShardPos.Y, 20, Color.Lime, "FIXED"));
                ProgressShardPattern(mapWidth);
                return;
            }

            if (ShardTimer <= 0)
            {
                int dmg = (int)(player.MaxHp * 0.15f);
                player.Hp -= dmg;
                effects.Add(new Effect("text", player.X, player.Y, player.X, player.Y, 40, Color.OrangeRed, "PATCH FAILED: -15% HP"));
                ProgressShardPattern(mapWidth);
            }
        }

        private void ProgressShardPattern(float mapWidth)
        {
            ShardSequence++;
            if (ShardSequence < 3) SpawnNextShard(mapWidth);
            else IsShardPatternActive = false;
        }

        public void HandleClick(Point mousePos)
        {
            if (IsResourcePatternActive)
            {
                for (int i = DebugButtons.Count - 1; i >= 0; i--)
                {
                    if (DebugButtons[i].Contains(mousePos)) DebugButtons.RemoveAt(i);
                }
                if (DebugButtons.Count == 0) IsResourcePatternActive = false;
            }
        }
    }
}