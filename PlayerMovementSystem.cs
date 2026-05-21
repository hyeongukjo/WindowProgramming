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
            if (Math.Abs(dx) > 3.0f) player.Facing = dx >= 0 ? 1 : -1;
            float maxSpeed =/* 4.6f + Math.Min(2.6f, player.Level * 0.22f);*/ 15.0f;
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
            cameraX = player.X - clientWidth * 0.38f;
            if (cameraX < 0) cameraX = 0;
            if (cameraX > mapWidth - clientWidth) cameraX = Math.Max(0, mapWidth - clientWidth);
        }
    }
}
