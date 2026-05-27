using System;

namespace DebugHeroFileDungeonRPG
{
    public static class PlayerMovementSystem
    {
        public static void Update(PlayerState player, StageInfo st, bool bossPhase, int clientWidth, int clientHeight, int mapWidth, ref float cameraX, int tick)
        {
            float minX = 80f;
            float maxX = Math.Max(minX, mapWidth - 80f);
            float minY = 118f;
            float maxY = Math.Max(minY, clientHeight - 78f);
            player.TargetX = Math.Max(minX, Math.Min(maxX, player.TargetX));
            player.TargetY = Math.Max(minY, Math.Min(maxY, player.TargetY));
            player.X = Math.Max(minX, Math.Min(maxX, player.X));
            player.Y = Math.Max(minY, Math.Min(maxY, player.Y));

            float dx = player.TargetX - player.X;
            float dy = player.TargetY - player.Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            // 💡 [레트로 RPG 고정 속도 주입] 가속/감속 관성을 완전히 지우고, 일정한 속도로 빠릿하게 기동합니다.
            // 아래에서 프레임을 60 FPS로 끌어올리기 때문에, 초당 9픽셀 이동이 가장 크리스프(Crisp)한 손맛을 냅니다.
            float retroMoveSpeed = 9.5f;

            if (distance > 0.1f)
            {
                // 방향 및 시선 전환 즉시 판정 반영 (레트로 사이드/탑뷰 맛 가미)
                if (Math.Abs(dx) > Math.Abs(dy))
                {
                    player.Direction = dx >= 0 ? 1 : 3;
                    player.Facing = dx >= 0 ? 1 : -1;
                }
                else
                {
                    player.Direction = dy >= 0 ? 0 : 2;
                }

                // 💡 [얼음판 미끄러짐 원천 차단] 남은 거리가 이동 속도보다 작으면 미끄러지지 않고 그 자리에 즉각 스냅 안착!
                if (distance <= retroMoveSpeed)
                {
                    player.X = player.TargetX;
                    player.Y = player.TargetY;
                    player.MoveVelocityX = 0f;
                    player.MoveVelocityY = 0f;
                    player.WalkCycle = 0f;
                }
                else
                {
                    // 관성 누적 없이 즉시 최고 속도로 락온 주행
                    player.MoveVelocityX = (dx / distance) * retroMoveSpeed;
                    player.MoveVelocityY = (dy / distance) * retroMoveSpeed;
                    player.X += player.MoveVelocityX;
                    player.Y += player.MoveVelocityY;

                    // 픽셀 이동에 걸맞은 빠릿한 발걸음 애니메이션 수치 동기화
                    player.WalkCycle += 0.28f;
                    while (player.WalkCycle >= 8f) player.WalkCycle -= 8f;
                    player.LastMoveTicks = tick;
                }
            }
            else
            {
                player.MoveVelocityX = 0f;
                player.MoveVelocityY = 0f;
                player.WalkCycle = 0f;
            }

            if (player.DefenseTicks > 0) player.DefenseTicks--;
            if (player.InvincibleTicks > 0) player.InvincibleTicks--;
            if (player.StunTicks > 0) player.StunTicks--;

            cameraX = player.X - clientWidth * 0.38f;
            if (cameraX < 0) cameraX = 0;
            if (cameraX > mapWidth - clientWidth) cameraX = Math.Max(0, mapWidth - clientWidth);
        }

        public static void StartSkillAnimation(PlayerState player, int skillIndex)
        {
            if (player == null) return;
            player.ActionState = PlayerActionState.Skill;
            player.ActionFrame = 0;
            player.ActionTick = 0;
            player.SkillIndex = skillIndex;
        }

        public static void StartHitAnimation(PlayerState player)
        {
            if (player == null) return;
            if (player.ActionState == PlayerActionState.Die) return;
            player.ActionState = PlayerActionState.Hit;
            player.ActionFrame = 0;
            player.ActionTick = 0;
            player.SkillIndex = -1;
        }

        public static void StartDeathAnimation(PlayerState player)
        {
            if (player == null) return;
            player.ActionState = PlayerActionState.Die;
            player.ActionFrame = 0;
            player.ActionTick = 0;
            player.SkillIndex = -1;
        }

        public static void UpdateActionAnimation(PlayerState player)
        {
            if (player == null) return;
            if (player.ActionState == PlayerActionState.Idle) return;

            player.ActionTick++;

            if (player.ActionState == PlayerActionState.Hit)
            {
                int hitFrameDelay = 4;
                if (player.ActionTick % hitFrameDelay == 0) player.ActionFrame++;
                if (player.ActionFrame >= 6)
                {
                    player.ActionState = PlayerActionState.Idle;
                    player.ActionFrame = 0;
                    player.ActionTick = 0;
                    player.SkillIndex = -1;
                }
                return;
            }

            if (player.ActionState == PlayerActionState.Die)
            {
                int deathFrameDelay = 7;
                if (player.ActionTick % deathFrameDelay == 0 && player.ActionFrame < 5) player.ActionFrame++;
                return;
            }

            int frameDelay = 5;
            if (player.ActionTick % frameDelay == 0) player.ActionFrame++;

            int maxFrame = 5;

            if (player.SkillIndex == 0)
                maxFrame = 5;
            else if (player.SkillIndex == 1)
                maxFrame = 5;
            else if (player.SkillIndex == 2)
                maxFrame = 5;
            else if (player.SkillIndex == 3)
                maxFrame = 5;

            if (player.ActionFrame >= maxFrame)
            {
                player.ActionState = PlayerActionState.Idle;
                player.ActionFrame = 0;
                player.ActionTick = 0;
                player.SkillIndex = -1;
            }
        }
    }
}