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
        private static readonly Random rand = new Random();

        public static EnemyUpdateResult Update(List<GameEntity> enemies, PlayerState player, StageInfo st, int currentStage, bool bossPhase, int tick, int mapWidth, Rectangle client, BossRuntime bossRuntime, List<Effect> effects)
        {
            EnemyUpdateResult result = new EnemyUpdateResult();
            float playerCurrentSpeedModifier = 1.0f;

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

                    string allocatedPattern = "None";

                    if (m.Name == "Security_Firewall" || m.Name == "Alert_Popup_Spam")
                    {
                        allocatedPattern = "Delay_Inertia_Dash";
                    }
                    else if (m.Name == "Runtime_Clock_Buoy")
                    {
                        allocatedPattern = "Heavy_Projectile_Spread";
                    }
                    else if (m.Name == "Registry_Ghost_Key")
                    {
                        allocatedPattern = "Random_Teleport_Barrage";
                    }

                    // * [1. Delay_Inertia_Dash 패턴 최종 정밀 교정 - 플레이어 방향성 완전 차단]
                    if (allocatedPattern == "Delay_Inertia_Dash")
                    {
                        m.StateTimer++;

                        if (m.MonsterState == 0) // 대기 상태
                        {
                            m.VX *= 0.8f; m.VY *= 0.8f;
                            if (m.StateTimer >= 60)
                            {
                                m.MonsterState = 1; m.StateTimer = 0;
                                float dx = player.X - m.X; float dy = player.Y - m.Y;
                                float dist = (float)Math.Sqrt(dx * dx + dy * dy); if (dist < 1) dist = 1;
                                m.TargetPosX = player.X + (dx / dist) * 180f;
                                m.TargetPosY = player.Y + (dy / dist) * 180f;
                                m.VX = (m.TargetPosX - m.X) * 0.08f;
                                m.VY = (m.TargetPosY - m.Y) * 0.08f;
                            }
                        }
                        else if (m.MonsterState == 1) // 돌진 상태
                        {
                            m.VX *= 0.94f; m.VY *= 0.94f;
                            m.X += m.VX; m.Y += m.VY;

                            // * [돌진 종료 판정]
                            if (Math.Abs(m.VX) < 0.15f && Math.Abs(m.VY) < 0.15f)
                            {
                                m.MonsterState = 2; // 튕김 전용 상태로 전환
                                m.StateTimer = 0;

                                // 💡 [핵심 수정]: 기존 속도를 완전히 지우고 플레이어와 무관한 각도 생성
                                m.VX = 0; m.VY = 0;

                                // * 개체별 고유 시드를 이용한 완전 무작위 방향(0~360도) 결정
                                double randomAngle = rand.NextDouble() * Math.PI * 2.0;

                                // * [과감한 분산]: 최소 15.0에서 최대 30.0 사이의 강력한 임펄스 부여
                                float scatterForce = 15.0f + (float)(rand.NextDouble() * 15.0f);

                                m.VX = (float)Math.Cos(randomAngle) * scatterForce;
                                m.VY = (float)Math.Sin(randomAngle) * scatterForce;
                            }
                        }
                        else if (m.MonsterState == 2) // 튕김 전용 상태 (관성 가드 해제 구역)
                        {
                            // * 플레이어를 향하는 연산 없이 순수하게 튕겨 나간 벡터로만 이동
                            m.VX *= 0.92f; m.VY *= 0.92f;
                            m.X += m.VX; m.Y += m.VY;

                            if (Math.Abs(m.VX) < 0.3f && Math.Abs(m.VY) < 0.3f)
                            {
                                m.MonsterState = 0; // 충분히 흩어진 후 대기 상태로 복귀
                                m.StateTimer = 0;
                            }
                        }
                    }

                    // * [3. Random_Teleport_Barrage 패턴 구현 - 무작위 텔레포트 탄막 사출]
                    else if (allocatedPattern == "Random_Teleport_Barrage")
                    {
                        m.StateTimer++;
                        m.X += m.VX * 0.4f; m.Y += m.VY * 0.4f;

                        if (m.StateTimer >= 120)
                        {
                            m.StateTimer = 0;
                            m.X = (float)(rand.NextDouble() * (maxX - minX) + minX);
                            m.Y = (float)(rand.NextDouble() * (maxY - minY) + minY);
                            effects.Add(new Effect("spark", m.X, m.Y, m.X, m.Y, 20, Color.Cyan, "LINK_JUMP"));

                            float baseAngle = (float)Math.Atan2(player.Y - m.Y, player.X - m.X);
                            for (int k = 0; k < 6; k++)
                            {
                                float angle = baseAngle + (float)(k * Math.PI * 2 / 6);
                                float speedX = (float)Math.Cos(angle) * 3.5f; float speedY = (float)Math.Sin(angle) * 3.5f;

                                // * [동일 적용]: 유령의 6방향 전방위 탄막 역시 플레이어 위치와 상관없이 절대 길이 600픽셀로 영구 고정 직선 관통 유도
                                float bulletLength = 600f;
                                float bEndX = m.X + (float)Math.Cos(angle) * bulletLength;
                                float bEndY = m.Y + (float)Math.Sin(angle) * bulletLength;

                                effects.Add(new Effect("projectile", m.X, m.Y, bEndX, bEndY, 50, Color.Cyan, "TELEPORT_BULLET"));
                            }
                        }
                    }

                    // ==========================================================
                    // 💡 [유저 브랜치 복원] 패턴이 None일 때 작동하는 일반 잔몹 기본 물리 추적
                    // ==========================================================
                    if (allocatedPattern == "None")
                    {
                        m.VX += Math.Sign(player.X - m.X) * 0.15f;
                        m.VY += Math.Sign(player.Y - m.Y) * 0.10f;
                        float maxSpeed = 1.3f;
                        float speed = (float)Math.Sqrt(m.VX * m.VX + m.VY * m.VY);
                        if (speed > maxSpeed) { m.VX = m.VX / speed * maxSpeed; m.VY = m.VY / speed * maxSpeed; }
                        m.X += m.VX; m.Y += m.VY;
                    }

                    // * [공통 외곽 벽면 충돌 제동]
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

                // ==========================================================
                // 💡 [통합 물리 충돌 엔진] 몸체 충돌 및 원거리 탄막 피격 모듈 복합 가동 (24틱 주기)
                // ==========================================================
                if (tick % 24 == 0)
                {
                    bool isHit = false;
                    double hitDamagePercent = 0.0;

                    if (m.Bounds.IntersectsWith(player.Bounds))
                    {
                        isHit = true;
                        hitDamagePercent = rand.Next(7, 16) / 100.0; // 기본 최대 체력의 7% ~ 15% 랜덤 대미지 배정
                    }
                    else
                    {
                        for (int k = 0; k < effects.Count; k++)
                        {
                            Effect eff = effects[k];
                            if (eff.Kind == "projectile")
                            {
                                float progress = 1f - eff.Ticks / (float)Math.Max(1, eff.MaxTicks);
                                float effCurrX = eff.X + (eff.X2 - eff.X) * progress;
                                float effCurrY = eff.Y + (eff.Y2 - eff.Y) * progress;

                                if (player.Bounds.Contains(effCurrX, effCurrY))
                                {
                                    if (eff.Text == "BULLET" || eff.Text == "TRASH" || eff.Text == "SPARK_LINE" || eff.Text == "TELEPORT_BULLET")
                                    {
                                        isHit = true;
                                        eff.Ticks = 0; // 피격된 탄막 즉시 소멸
                                        hitDamagePercent = rand.Next(7, 16) / 100.0;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (isHit)
                    {
                        int damage = (int)(player.MaxHp * hitDamagePercent);
                        if (player.DefenseTicks > 0) damage = Math.Max(1, damage / 3);

                        player.Hp -= damage;
                        player.SystemStability = Math.Max(0, player.SystemStability - 1);

                        effects.Add(new Effect("text", player.X, player.Y - 70, player.X, player.Y - 70, 34, Color.OrangeRed, "-" + damage));
                        effects.Add(new Effect("spark", player.X, player.Y - 32, player.X, player.Y - 32, 22, Color.Red, ""));

                        // ⚠️ [유저 브랜치 핵심 스펙 철저 고수] 
                        // 로컬 부활을 차단하고 HP를 즉시 0으로 강제 세팅하여 MainForm 하단의 퇴장 엔진을 직동시킵니다.
                        if (player.Hp <= 0)
                        {
                            player.Hp = 0;
                        }
                    }
                }
            }

            player.MoveVelocityX *= playerCurrentSpeedModifier;
            player.MoveVelocityY *= playerCurrentSpeedModifier;

            result.AllEnemiesDefeated = enemies.Count > 0;
            for (int i = 0; i < enemies.Count; i++) if (enemies[i].Hp > 0) result.AllEnemiesDefeated = false;
            return result;
        }
    }
}