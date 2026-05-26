using System;
using System.Collections.Generic;
using System.Drawing;

namespace DebugHeroFileDungeonRPG
{
    public sealed class EnemyUpdateResult
    {
        public bool PlayerReturnedToStart;
        public bool AllEnemiesDefeated;
    }

    public static class EnemyLogicSystem
    {
        public static EnemyUpdateResult Update(List<GameEntity> enemies, PlayerState player, StageInfo st, int currentStage, bool bossPhase, int tick, int mapWidth, Rectangle client, BossRuntime bossRuntime, List<Effect> effects)
        {
            EnemyUpdateResult result = new EnemyUpdateResult();
            for (int i = 0; i < enemies.Count; i++)
            {
                GameEntity m = enemies[i];
                if (m.Hp <= 0) continue;
                if (!m.IsBoss)
                {
                    float minX = 170f;
                    float maxX = Math.Max(minX, mapWidth - 130f);
                    float minY = 124f;
                    float maxY = Math.Max(minY, client.Height - 82f);

                    if (tick % (96 + i * 13) == 0)
                    {
                        m.VX += Math.Sign(player.X - m.X) * (0.18f + st.Index * 0.01f);
                        m.VY += Math.Sign(player.Y - m.Y) * (0.12f + st.Index * 0.008f);
                    }

                    float maxSpeed = 1.35f + st.Index * 0.06f;
                    float speed = (float)Math.Sqrt(m.VX * m.VX + m.VY * m.VY);
                    if (speed > maxSpeed)
                    {
                        m.VX = m.VX / speed * maxSpeed;
                        m.VY = m.VY / speed * maxSpeed;
                    }

                    m.X += m.VX;
                    m.Y += m.VY;
                    if (m.X < minX) { m.X = minX; m.VX = Math.Abs(m.VX); }
                    if (m.X > maxX) { m.X = maxX; m.VX = -Math.Abs(m.VX); }
                    if (m.Y < minY) { m.Y = minY; m.VY = Math.Abs(m.VY); }
                    if (m.Y > maxY) { m.Y = maxY; m.VY = -Math.Abs(m.VY); }
                }
                else
                {
                    float towardX = player.X - m.X;
                    float towardY = player.Y - m.Y;
                    float distance = (float)Math.Sqrt(towardX * towardX + towardY * towardY);
                    if (distance > 110)
                    {
                        float speed = 1.0f + st.Index * 0.06f;
                        m.X += towardX / distance * speed;
                        m.Y += towardY / distance * speed;
                    }
                    m.Y = Math.Max(150f, Math.Min(client.Height - 92f, m.Y));
                    if (bossPhase) bossRuntime.Update(currentStage, m, player, effects, client, mapWidth);
                }
                if (m.HitFlash > 0) m.HitFlash--;
                // 일반 몸빵 충돌 주기 조정 및 계수 대폭 상향
                if (m.Bounds.IntersectsWith(player.Bounds) && tick % 20 == 0)
                {
                    //  몬스터 공격력의 무려 2.5배를 다이렉트로 관통 대미지로 주입!
                    int damage = Math.Max(25, (int)(m.Attack * 2.5f));
                    if (player.DefenseTicks > 0) damage = Math.Max(5, damage / 2); // 방어 시 경감율 축소

                    player.Hp -= damage;
                    player.SystemStability = Math.Max(0, player.SystemStability - 3); // 안정성 파괴 지수 상향

                    effects.Add(new Effect("text", player.X, player.Y - 70, player.X, player.Y - 70, 34, Color.OrangeRed, "CRITICAL -" + damage));
                    effects.Add(new Effect("spark", player.X, player.Y - 32, player.X, player.Y - 32, 22, Color.Red, ""));

                    if (player.Hp <= 0)
                    {
                        // player.Hp = player.MaxHp 복구 코드를 완전히 제거하여 즉시 0 이하 상태로 타이머 틱에 전달, 
                        // MainForm 최하단 예외 필터를 통해 바탕화면으로 즉각 영구 사출(퇴장)되게 연동합니다.
                        player.Hp = 0;
                    }
                }
            }
            result.AllEnemiesDefeated = enemies.Count > 0;
            for (int i = 0; i < enemies.Count; i++) if (enemies[i].Hp > 0) result.AllEnemiesDefeated = false;
            return result;
        }
    }
}
