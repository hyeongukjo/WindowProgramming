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

            //  플레이어의 피격 판정 확장 범위 정의 (가로 40px, 세로 115px)
            RectangleF extendedPlayerBounds = new RectangleF(
                player.X - 20f,
                player.Y - 110f, // 머리 꼭대기 위치
                40f,
                115f            // 발바닥까지의 높이
            );

            // 확장된 피격 범위 상자의 '완벽한 정중앙 좌표'를 산출
            float playerCenterX = extendedPlayerBounds.X + (extendedPlayerBounds.Width / 2f);
            float playerCenterY = extendedPlayerBounds.Y + (extendedPlayerBounds.Height / 2f);

            for (int i = 0; i < enemies.Count; i++)
            {
                GameEntity m = enemies[i];
                if (m.IsBoss && m.Hp <= 0 && bossPhase)
                {
                    bossRuntime.Update(currentStage, m, player, effects, client, mapWidth);
                }

                if (m.Hp <= 0) continue;

                // 몬스터 피격 판정 범위
                RectangleF extendedMonsterBounds = new RectangleF(m.X - 25f, m.Y - 50f, 50f, 60f);

                if (!m.IsBoss)
                {
                    float minX = 170f;
                    float maxX = Math.Max(minX, mapWidth - 130f);
                    float minY = 124f;
                    float maxY = Math.Max(minY, client.Height - 82f);

                    string allocatedPattern = "None";

                    if (m.Name == "Security_Firewall" || m.Name == "Alert_Popup_Spam" || m.Name == "Broken_Document.txt" || m.Name == "Broken Key" || m.Name == "Temp Fragment")
                    {
                        allocatedPattern = "Delay_Inertia_Dash";
                    }
                    else if (m.Name == "Runtime_Clock_Buoy" || m.Name == "Runtime_Clock_Buoy_Elite" || m.Name == "Empty_Folder" || m.Name == "Open Port Buoy" || m.Name == "Firewall Barnacle")
                    {
                        allocatedPattern = "Heavy_Projectile_Spread";
                    }
                    else if (m.Name == "Registry_Ghost_Key" || m.Name == "Packet_Minnow" || m.Name == "Broken_Shortcut.lnk" || m.Name == "Request Crab" || m.Name == "Unsent Report" || m.Name == "Recent Ghost")
                    {
                        allocatedPattern = "Random_Teleport_Barrage";
                    }

                    // =================================================================
                    // * [1. Delay_Inertia_Dash 패턴 구동 - 대시형 몹 조준 보정]
                    // =================================================================
                    if (allocatedPattern == "Delay_Inertia_Dash")
                    {
                        m.StateTimer++;
                        if (m.MonsterState == 0)
                        {
                            m.VX *= 0.8f; m.VY *= 0.8f;
                            if (m.StateTimer >= 60)
                            {
                                m.MonsterState = 1; m.StateTimer = 0;

                                // 돌진하는 목표 좌표를 기존 player.X/Y에서 '피격범위 정중앙'으로 교체
                                float dx = playerCenterX - m.X;
                                float dy = playerCenterY - m.Y;

                                float dist = (float)Math.Sqrt(dx * dx + dy * dy); if (dist < 1) dist = 1;
                                m.TargetPosX = playerCenterX + (dx / dist) * 180f;
                                m.TargetPosY = playerCenterY + (dy / dist) * 180f;
                                m.VX = (m.TargetPosX - m.X) * 0.08f;
                                m.VY = (m.TargetPosY - m.Y) * 0.08f;
                            }
                        }
                        else if (m.MonsterState == 1)
                        {
                            m.VX *= 0.94f; m.VY *= 0.94f;
                            m.X += m.VX; m.Y += m.VY;

                            if (Math.Abs(m.VX) < 0.15f && Math.Abs(m.VY) < 0.15f)
                            {
                                m.MonsterState = 2; m.StateTimer = 0;
                                m.VX = 0; m.VY = 0;
                                double randomAngle = rand.NextDouble() * Math.PI * 2.0;
                                float scatterForce = 15.0f + (float)(rand.NextDouble() * 15.0f);
                                m.VX = (float)Math.Cos(randomAngle) * scatterForce;
                                m.VY = (float)Math.Sin(randomAngle) * scatterForce;
                            }
                        }
                        else if (m.MonsterState == 2)
                        {
                            m.VX *= 0.92f; m.VY *= 0.92f;
                            m.X += m.VX; m.Y += m.VY;
                            if (Math.Abs(m.VX) < 0.3f && Math.Abs(m.VY) < 0.3f) { m.MonsterState = 0; m.StateTimer = 0; }
                        }
                    }

                    // =================================================================
                    // * [2. Heavy_Projectile_Spread 패턴 구동 - 스프레드 투사체 조준 보정]
                    // =================================================================
                    else if (allocatedPattern == "Heavy_Projectile_Spread")
                    {
                        if (tick % (110 + i * 15) == 0)
                        {
                            // 기본 자석 추적도 중앙점을 향해 부드럽게 이동하도록 보정
                            m.VX += Math.Sign(playerCenterX - m.X) * 0.12f;
                            m.VY += Math.Sign(playerCenterY - m.Y) * 0.08f;
                        }
                        float maxSpeed = 0.9f;
                        float speed = (float)Math.Sqrt(m.VX * m.VX + m.VY * m.VY);
                        if (speed > maxSpeed) { m.VX = m.VX / speed * maxSpeed; m.VY = m.VY / speed * maxSpeed; }
                        m.X += m.VX; m.Y += m.VY;

                        if (tick % 140 == 0) // 주기는 지시사항에 맞춰 140틱 유지
                        {
                            //  발사되는 투사체의 궤적 벡터 타겟을 '피격범위 정중앙'으로 교체
                            float tDx = playerCenterX - m.X;
                            float tDy = playerCenterY - m.Y;
                            float tDist = (float)Math.Sqrt(tDx * tDx + tDy * tDy); if (tDist < 1) tDist = 1;

                            float fixedLength = 550f;
                            float fixedEndX = m.X + (tDx / tDist) * fixedLength;
                            float fixedEndY = m.Y + (tDy / tDist) * fixedLength;

                            effects.Add(new Effect("projectile", m.X, m.Y, fixedEndX, fixedEndY, 40, Color.OrangeRed, "TRASH"));
                        }
                    }

                    // =================================================================
                    // * [3. Random_Teleport_Barrage 패턴 구동 - 텔레포트 몹 미사일 조준 보정]
                    // =================================================================
                    else if (allocatedPattern == "Random_Teleport_Barrage")
                    {
                        m.StateTimer++;
                        m.X += m.VX * 0.4f; m.Y += m.VY * 0.4f;

                        if (m.StateTimer >= 180) // 동기화된 180틱 주기 유지
                        {
                            m.StateTimer = 0;
                            float screenMinX = Math.Max(minX, playerCenterX - 350f);
                            float screenMaxX = Math.Min(maxX, playerCenterX + 350f);

                            m.X = (float)(rand.NextDouble() * (screenMaxX - screenMinX) + screenMinX);
                            m.Y = (float)(rand.NextDouble() * (maxY - minY) + minY);
                            effects.Add(new Effect("spark", m.X, m.Y, m.X, m.Y, 20, Color.Cyan, "LINK_JUMP"));

                            // 발사 각도 산출의 베이스 타겟을 '피격범위 정중앙'으로 교체
                            float baseAngle = (float)Math.Atan2(playerCenterY - m.Y, playerCenterX - m.X);

                            for (int k = 0; k < 3; k++) // 투사체 수 3개 유지
                            {
                                float angle = baseAngle + (float)(k * Math.PI * 2 / 3);
                                float bulletLength = 600f;
                                float bEndX = m.X + (float)Math.Cos(angle) * bulletLength;
                                float bEndY = m.Y + (float)Math.Sin(angle) * bulletLength;

                                effects.Add(new Effect("projectile", m.X, m.Y, bEndX, bEndY, 100, Color.OrangeRed, "TRASH"));
                            }
                        }
                    }

                    // 기본 패턴 없는 유도 몬스터 보정
                    if (allocatedPattern == "None")
                    {
                        m.VX += Math.Sign(playerCenterX - m.X) * 0.15f;
                        m.VY += Math.Sign(playerCenterY - m.Y) * 0.10f;
                        float maxSpeed = 1.3f; float speed = (float)Math.Sqrt(m.VX * m.VX + m.VY * m.VY);
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
                    // 보스 몬스터 추적 가이드 보정
                    float towardX = playerCenterX - m.X;
                    float towardY = playerCenterY - m.Y;
                    float distance = (float)Math.Sqrt(towardX * towardX + towardY * towardY);
                    if (distance > 110)
                    {
                        float speed = 1.0f + st.Index * 0.06f;
                        m.X += towardX / distance * speed; m.Y += towardY / distance * speed;
                    }
                    m.Y = Math.Max(150f, Math.Min(client.Height - 92f, m.Y));
                    if (bossPhase) bossRuntime.Update(currentStage, m, player, effects, client, mapWidth);
                }

                if (m.HitFlash > 0) m.HitFlash--;

                // --- 충돌 및 피격 엔진 연산 세션 (24틱 주기) ---
                if (tick % 24 == 0)
                {
                    bool isHit = false;
                    double hitDamagePercent = rand.Next(7, 16) / 100.0;

                    if (extendedMonsterBounds.IntersectsWith(extendedPlayerBounds))
                    {
                        isHit = true;
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

                                string effTxt = (eff.Text ?? "").ToUpper();
                                if (effTxt == "BULLET" || effTxt == "TRASH" || effTxt == "SPARK_LINE" || effTxt == "TELEPORT_BULLET")
                                {
                                    // 지시된 십자형 정밀 슬롯 콜렉션 마스크 연산 구역
                                    RectangleF coreBox = new RectangleF(effCurrX - 15f, effCurrY - 15f, 30f, 30f);
                                    RectangleF wingH1 = new RectangleF(effCurrX - 13f, effCurrY - 0.5f, 26f, 1f);
                                    RectangleF wingV1 = new RectangleF(effCurrX - 0.5f, effCurrY - 13f, 1f, 26f);
                                    RectangleF wingH2 = new RectangleF(effCurrX - 11f, effCurrY - 0.5f, 22f, 1f);
                                    RectangleF wingV2 = new RectangleF(effCurrX - 0.5f, effCurrY - 11f, 1f, 22f);

                                    if (coreBox.IntersectsWith(extendedPlayerBounds) ||
                                        wingH1.IntersectsWith(extendedPlayerBounds) ||
                                        wingV1.IntersectsWith(extendedPlayerBounds) ||
                                        wingH2.IntersectsWith(extendedPlayerBounds) ||
                                        wingV2.IntersectsWith(extendedPlayerBounds))
                                    {
                                        isHit = true;
                                        eff.Ticks = 0;
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

                        if (player.Hp <= 0) player.Hp = 0;
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