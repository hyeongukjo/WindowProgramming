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
            if (Math.Abs(dx) > 3.0f || Math.Abs(dy) > 3.0f)
            {
                if (Math.Abs(dx) > Math.Abs(dy))
                {
                    player.Direction = dx >= 0 ? 1 : 3;
                    player.Facing = dx >= 0 ? 1 : -1;
                }
                else
                {
                    player.Direction = dy >= 0 ? 0 : 2;
                }
            }
            float maxSpeed = 4.6f + Math.Min(2.6f, player.Level * 0.22f);
            float desiredVelocityX = 0f;
            float desiredVelocityY = 0f;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);
            if (distance > 3.0f)
            {
                float slowRatio = Math.Min(1f, distance / 120f);
                float desiredSpeed = maxSpeed * (0.34f + 0.66f * slowRatio);
                desiredVelocityX = dx / distance * desiredSpeed;
                desiredVelocityY = dy / distance * desiredSpeed;
            }
            float accel = distance > 140f ? 0.14f : distance > 45f ? 0.105f : 0.075f;
            player.MoveVelocityX += (desiredVelocityX - player.MoveVelocityX) * accel;
            player.MoveVelocityY += (desiredVelocityY - player.MoveVelocityY) * accel;
            float velocityLength = (float)Math.Sqrt(player.MoveVelocityX * player.MoveVelocityX + player.MoveVelocityY * player.MoveVelocityY);
            if (distance <= Math.Max(2.8f, velocityLength + 0.9f))
            {
                player.X = player.TargetX;
                player.Y = player.TargetY;
                player.MoveVelocityX = 0f;
                player.MoveVelocityY = 0f;
            }
            else
            {
                player.X += player.MoveVelocityX;
                player.Y += player.MoveVelocityY;
            }
            if (velocityLength > 0.08f)
            {
                if (distance > 3.0f) player.Facing = dx >= 0 ? 1 : -1;
                float stepAdvance = Math.Max(0.10f, Math.Min(0.22f, velocityLength * 0.052f));
                player.WalkCycle += stepAdvance;
                while (player.WalkCycle >= 8f) player.WalkCycle -= 8f;
                player.LastMoveTicks = tick;
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

        public static void UpdateActionAnimation(PlayerState player)
        {
            if (player == null) return;

            if (player.ActionState == PlayerActionState.Idle)
                return;

            player.ActionTick++;

            int frameDelay = 5;

            if (player.ActionTick % frameDelay == 0)
                player.ActionFrame++;

            int maxFrame = 4;

            if (player.SkillIndex == 0)
                maxFrame = 4;      // Q: 1��
            else if (player.SkillIndex == 1)
                maxFrame = 5;      // W: 2��
            else if (player.SkillIndex == 2)
                maxFrame = 5;      // E: 3��
            else if (player.SkillIndex == 3)
                maxFrame = 5;      // R: �ӽ÷� 3�� ����

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
