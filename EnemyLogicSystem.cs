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
                    // 💡 [탑다운 대전환]: 기존 2.5D 바닥 제약을 제거하고, Y축 상하 사방을 전면 개방합니다.
                    float minX = 64f;
                    float maxX = Math.Max(minX, mapWidth - 64f);
                    float minY = 64f;
                    float maxY = Math.Max(minY, client.Height - 64f); // 🌟 바닥선 억압 차단

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

                    // * [1. Delay_Inertia_Dash 패턴 구동]
                    if (allocatedPattern == "Delay_Inertia_Dash")
                    {
                        m.StateTimer++;

                        if (m.MonsterState == 0)
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
                        else if (m.MonsterState == 1)
                        {
                            m.VX *= 0.94f; m.VY *= 0.94f;
                            m.X += m.VX; m.Y += m.VY;

                            if (Math.Abs(m.VX) < 0.15f && Math.Abs(m.VY) < 0.15f)
                            {
                                m.MonsterState = 2;
                                m.StateTimer = 0;
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

                            if (Math.Abs(m.VX) < 0.3f && Math.Abs(m.VY) < 0.3f)
                            {
                                m.MonsterState = 0;
                                m.StateTimer = 0;
                            }
                        }
                    }

                    // * [2. Heavy_Projectile_Spread 패턴 구동]
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

                        if (tick % 70 == 0)
                        {
                            float tDx = player.X - m.X;
                            float tDy = player.Y - m.Y;
                            float tDist = (float)Math.Sqrt(tDx * tDx + tDy * tDy);
                            if (tDist < 1) tDist = 1;

                            float fixedLength = 550f;
                            float fixedEndX = m.X + (tDx / tDist) * fixedLength;
                            float fixedEndY = m.Y + (tDy / tDist) * fixedLength;

                            effects.Add(new Effect("projectile", m.X, m.Y, fixedEndX, fixedEndY, 40, Color.OrangeRed, "TRASH"));
                        }
                    }
                    // * [3. Random_Teleport_Barrage 패턴 구동]
                    else if (allocatedPattern == "Random_Teleport_Barrage")
                    {
                        m.StateTimer++;
                        m.X += m.VX * 0.4f; m.Y += m.VY * 0.4f;

                        if (m.StateTimer >= 120)
                        {
                            m.StateTimer = 0;

                            float screenMinX = Math.Max(minX, player.X - 350f);
                            float screenMaxX = Math.Min(maxX, player.X + 350f);

                            m.X = (float)(rand.NextDouble() * (screenMaxX - screenMinX) + screenMinX);
                            m.Y = (float)(rand.NextDouble() * (maxY - minY) + minY);
                            effects.Add(new Effect("spark", m.X, m.Y, m.X, m.Y, 20, Color.Cyan, "LINK_JUMP"));

                            float baseAngle = (float)Math.Atan2(player.Y - m.Y, player.X - m.X);
                            for (int k = 0; k < 6; k++)
                            {
                                float angle = baseAngle + (float)(k * Math.PI * 2 / 6);

                                float bulletLength = 600f;
                                float bEndX = m.X + (float)Math.Cos(angle) * bulletLength;
                                float bEndY = m.Y + (float)Math.Sin(angle) * bulletLength;

                                // 🌟 [식별 코드 주입]: 텔레포트 패턴 투사체임을 명시하여 projectile_teleport 자산을 당겨오도록 세팅합니다.
                                effects.Add(new Effect("projectile", m.X, m.Y, bEndX, bEndY, 100, Color.OrangeRed, "TELEPORT_BULLET"));
                            }
                        }
                    }
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
                    // 💡 [보스 탑다운 연산 고정]: 보스 개체 역시 바닥 격리 한계를 풀고 플레이어를 전방위 추격합니다.
                    float towardX = player.X - m.X;
                    float towardY = player.Y - m.Y;
                    float distance = (float)Math.Sqrt(towardX * towardX + towardY * towardY);
                    if (distance > 10)
                    {
                        float speed = 1.0f + st.Index * 0.06f;
                        m.X += towardX / distance * speed;
                        m.Y += towardY / distance * speed;
                    }

                    // Y축 상하 바운더리 리미트 확장 완료
                    m.Y = Math.Max(64f, Math.Min(client.Height - 64f, m.Y));
                    if (bossPhase) bossRuntime.Update(currentStage, m, player, effects, client, mapWidth);
                }

                if (m.HitFlash > 0) m.HitFlash--;

                if (tick % 24 == 0)
                {
                    bool isHit = false;
                    double hitDamagePercent = 0.0;

                    // =============================================================================
                    // 🌟 [최종 정밀 교정]: 캐릭터 상체 및 가방 몸통 부위(image_1e0083.png) 피격 동기화 격실
                    // =============================================================================
                    // 💡 골반과 발바닥 쪽에 쏠려있던 충돌 그릇을 플레이어 Y축 기준 위로 18픽셀 올리고(-18f),
                    // 형진님이 표시하신 몸통 두께에 완벽히 밀착하도록 가로 46px, 세로 54px 크기로 재조립합니다.
                    float hitBoxW = 46f;
                    float hitBoxH = 54f;
                    float yOffset = 18f; // 🌟 가슴과 가방 부위로 충돌 상자를 끌어올리는 마법의 오프셋

                    RectangleF playerAdjustedHitBox = new RectangleF(
                        player.X - (hitBoxW / 2f),
                        player.Y - (hitBoxH / 2f) - yOffset, // 🎯 Y축 상단 보정을 통해 명치/가슴 판정 일치 완료
                        hitBoxW,
                        hitBoxH
                    );

                    // 1. 몬스터 본체와의 상체 피격 체크
                    if (m.Bounds.IntersectsWith(Rectangle.Round(playerAdjustedHitBox)))
                    {
                        isHit = true;
                        hitDamagePercent = rand.Next(7, 16) / 100.0;
                    }
                    else
                    {
                        // 2. 적 에너지 구체(energy_ball / projectile_teleport) 투사체와 상체 피격 체크
                        for (int k = 0; k < effects.Count; k++)
                        {
                            Effect eff = effects[k];
                            if (eff.Kind == "projectile")
                            {
                                float progress = 1f - eff.Ticks / (float)Math.Max(1, eff.MaxTicks);
                                float effCurrX = eff.X + (eff.X2 - eff.X) * progress;
                                float effCurrY = eff.Y + (eff.Y2 - eff.Y) * progress;

                                float bulletRadius = 8f;
                                RectangleF bulletHitBox = new RectangleF(
                                    effCurrX - bulletRadius,
                                    effCurrY - bulletRadius,
                                    bulletRadius * 2f,
                                    bulletRadius * 2f
                                );

                                // 낡은 Contains 대신, 두 사각형이 공중에서 '교차(IntersectsWith)'했는지 정밀 연산합니다.
                                if (playerAdjustedHitBox.IntersectsWith(Rectangle.Round(bulletHitBox)))
                                {
                                    string effTxt = (eff.Text ?? "").ToUpper();
                                    if (effTxt == "BULLET" || effTxt == "TRASH" || effTxt == "SPARK_LINE" || effTxt == "TELEPORT_BULLET")
                                    {
                                        isHit = true;
                                        eff.Ticks = 0; // 맞은 총알은 화면에서 즉시 소멸
                                        player.InvincibleTicks = 45; // 피격 후 0.75초간 무적 타임 작동
                                        hitDamagePercent = rand.Next(7, 16) / 100.0;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    // =============================================================================

                    if (isHit)
                    {
                        int damage = (int)(player.MaxHp * hitDamagePercent);
                        if (player.DefenseTicks > 0) damage = Math.Max(1, damage / 3);

                        player.Hp -= damage;
                        player.SystemStability = Math.Max(0, player.SystemStability - 1);

                        effects.Add(new Effect("text", player.X, player.Y - 40, player.X, player.Y - 40, 34, Color.OrangeRed, "-" + damage));
                        effects.Add(new Effect("spark", player.X, player.Y, player.X, player.Y, 22, Color.Red, ""));

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