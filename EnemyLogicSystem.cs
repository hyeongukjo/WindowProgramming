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

                    // 💡 [버그 해결]: 원본 프로젝트 명세 파일의 모든 일반 몹 이름을 패키징하여 연동 통로 개통
                    // A. 돌진형 몬스터 그룹 (1성 및 정예 포함)
                    if (m.Name == "Security_Firewall" || m.Name == "Alert_Popup_Spam" || m.Name == "Broken_Document.txt" || m.Name == "Broken Key" || m.Name == "Temp Fragment")
                    {
                        allocatedPattern = "Delay_Inertia_Dash";
                    }
                    // B. 부표형 포탑 몬스터 그룹 (Spread 전용 - 이 구역이 완전히 뚫려야 정상 사출됩니다)
                    else if (m.Name == "Runtime_Clock_Buoy" || m.Name == "Empty_Folder" || m.Name == "Open Port Buoy" || m.Name == "Firewall Barnacle")
                    {
                        allocatedPattern = "Heavy_Projectile_Spread";
                    }
                    // C. 유령형 텔레포트 몬스터 그룹
                    else if (m.Name == "Registry_Ghost_Key" || m.Name == "Broken_Shortcut.lnk" || m.Name == "Request Crab" || m.Name == "Unsent Report" || m.Name == "Recent Ghost")
                    {
                        allocatedPattern = "Random_Teleport_Barrage";
                    }
                    else if (m.Name == "Registry_Ghost_Key" || m.Name == "Packet_Minnow" || m.Name == "Broken_Shortcut.lnk" || m.Name == "Request Crab" || m.Name == "Unsent Report" || m.Name == "Recent Ghost")
                    {
                        allocatedPattern = "Random_Teleport_Barrage";
}

                    // * [1. Delay_Inertia_Dash 패턴 구현 - 돌진 후 임펄스 초기화 분산]
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

                            if (Math.Abs(m.VX) < 0.15f && Math.Abs(m.VY) < 0.15f)
                            {
                                m.MonsterState = 2; // 튕김 전용 상태로 분리 가드
                                m.StateTimer = 0;

                                m.VX = 0; m.VY = 0; // 플레이어 방향 속도 완전 거세

                                double randomAngle = rand.NextDouble() * Math.PI * 2.0;
                                float scatterForce = 15.0f + (float)(rand.NextDouble() * 15.0f); // 과감한 5배급 기하학 분산

                                m.VX = (float)Math.Cos(randomAngle) * scatterForce;
                                m.VY = (float)Math.Sin(randomAngle) * scatterForce;
                            }
                        }
                        else if (m.MonsterState == 2) // 튕김 상태
                        {
                            m.VX *= 0.92f; m.VY *= 0.92f;
                            m.X += m.VX; m.Y += m.VY;

                            if (Math.Abs(m.VX) < 0.3f && Math.Abs(m.VY) < 0.3f)
                            {
                                m.MonsterState = 0;
                                m.StateTimer = 0;
                            }
                        }
                    }

                    // * [2. Heavy_Projectile_Spread 패턴 구현 - 부표형 투사체 사출 모듈]
                    else if (allocatedPattern == "Heavy_Projectile_Spread")
                    {
                        if (tick % (110 + i * 15) == 0)
                        {
                            m.VX += Math.Sign(player.X - m.X) * 0.12f;
                            m.VY += Math.Sign(player.Y - m.Y) * 0.08f;
                        }
                        float maxSpeed = 0.9f;
                        float speed = (float)Math.Sqrt(m.VX * m.VX + m.VY * m.VY);
                        if (speed > maxSpeed) { m.VX = m.VX / speed * maxSpeed; m.VY = m.VY / speed * maxSpeed; }
                        m.X += m.VX; m.Y += m.VY;

                        // 70틱 주기로 정밀 탄막 사출
                        if (tick % 70 == 0)
                        {
                            // 💡 [기획 명세 고수]: 상대적 길이 오차 완전 격리 공식
                            // 단위 벡터 추정 후 고정 사거리 계수(550픽셀) 곱연산 수행
                            float tDx = player.X - m.X;
                            float tDy = player.Y - m.Y;
                            float tDist = (float)Math.Sqrt(tDx * tDx + tDy * tDy);
                            if (tDist < 1) tDist = 1;

                            float fixedLength = 550f; // 변하지 않는 절대적 고정 길이 가이드
                            float fixedEndX = m.X + (tDx / tDist) * fixedLength;
                            float fixedEndY = m.Y + (tDy / tDist) * fixedLength;

                            // 이펙트 매니저 유입 등록
                            effects.Add(new Effect("projectile", m.X, m.Y, fixedEndX, fixedEndY, 40, Color.OrangeRed, "TRASH"));
                        }
                    }

                    // * [3. Random_Teleport_Barrage 패턴 구현 - 유령형 6방향 탄막 사출]
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

                                float bulletLength = 600f;
                                float bEndX = m.X + (float)Math.Cos(angle) * bulletLength;
                                float bEndY = m.Y + (float)Math.Sin(angle) * bulletLength;

                                effects.Add(new Effect("projectile", m.X, m.Y, bEndX, bEndY, 50, Color.Cyan, "TELEPORT_BULLET"));
                            }
                        }
                    }

                    // 패턴 가드를 받지 못하는 나머지 예외 잡몹들을 위한 유연한 백업 추적선
                    if (allocatedPattern == "None")
                    {
                        m.VX += Math.Sign(player.X - m.X) * 0.15f;
                        m.VY += Math.Sign(player.Y - m.Y) * 0.10f;
                        float maxSpeed = 1.3f;
                        float speed = (float)Math.Sqrt(m.VX * m.VX + m.VY * m.VY);
                        if (speed > maxSpeed) { m.VX = m.VX / speed * maxSpeed; m.VY = m.VY / speed * maxSpeed; }
                        m.X += m.VX; m.Y += m.VY;
                    }

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

                if (tick % 24 == 0)
                {
                    bool isHit = false;
                    double hitDamagePercent = 0.0;

                    if (m.Bounds.IntersectsWith(player.Bounds))
                    {
                        isHit = true;
                        hitDamagePercent = rand.Next(7, 16) / 100.0;
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
                                        eff.Ticks = 0;
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