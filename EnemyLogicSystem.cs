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

            // * [플레이어 디버프 이동속도 상태값 배율 초기화]
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

                    // * [패턴 특징 명명 변수 선언 및 데이터 테이블 기반 몬스터별 패턴 할당 유도]
                    string allocatedPattern = "None";

                    // * [Delay_Inertia_Dash 패턴 할당 대상 지정]
                    if (m.Name == "Broken_Document.txt" || m.Name == "Broken Key" || m.Name == "Temp Fragment")
                    {
                        allocatedPattern = "Delay_Inertia_Dash";
                    }
                    // * [Periodic_Hardening_Guard 패턴 할당 대상 지정]
                    else if (m.Name == "Empty_Folder" || m.Name == "Open Port Buoy")
                    {
                        allocatedPattern = "Periodic_Hardening_Guard";
                    }
                    // * [Random_Teleport_Barrage 패턴 할당 대상 지정]
                    else if (m.Name == "Broken_Shortcut.lnk" || m.Name == "Request Crab" || m.Name == "Unsent Report")
                    {
                        allocatedPattern = "Random_Teleport_Barrage";
                    }
                    // * [Heavy_Projectile_Spread 패턴 할당 대상 지정]
                    else if (m.Name == "Unemptied_Trash.bak" || m.Name == "Firewall Barnacle" || m.Name == "Recent Trace" || m.Name == "Recent Ghost" || m.Name == "Driver Cache Fragment")
                    {
                        allocatedPattern = "Heavy_Projectile_Spread";
                    }
                    // * [Slime_Bounce_Approach 패턴 할당 대상 지정]
                    else if (m.Name == "Unknown Device" || m.Name == "Packet Minnow" || m.Name == "Orphan Entry")
                    {
                        allocatedPattern = "Slime_Bounce_Approach";
                    }
                    // * [Contamination_Zone_Leak 패턴 할당 대상 지정]
                    else if (m.Name == "Broken Driver Icon" || m.Name == "Duplicate Value" || m.Name == "Cache Leech")
                    {
                        allocatedPattern = "Contamination_Zone_Leak";
                    }
                    // * [IRQ Conflict 전용 번개 기믹 임시 수용]
                    else if (m.Name == "IRQ Conflict")
                    {
                        allocatedPattern = "IRQ_Lightning_Strike";
                    }

                    // * [1. Delay_Inertia_Dash 패턴 구현 - 2초 유예 대기 후 관성 돌진]
                    if (allocatedPattern == "Delay_Inertia_Dash")
                    {
                        m.StateTimer++;
                        if (m.MonsterState == 0) // * [0: 대기 폼]
                        {
                            m.VX *= 0.8f; m.VY *= 0.8f;

                            if (m.StateTimer >= 60) // * [2초 대기 만료]
                            {
                                m.MonsterState = 1; m.StateTimer = 0;
                                float dx = player.X - m.X; float dy = player.Y - m.Y;
                                float dist = (float)Math.Sqrt(dx * dx + dy * dy); if (dist < 1) dist = 1;

                                m.TargetPosX = player.X + (dx / dist) * 180f;
                                m.TargetPosY = player.Y + (dy / dist) * 180f;

                                m.VX = (m.TargetPosX - m.X) * 0.08f;
                                m.VY = (m.TargetPosY - m.Y) * 0.08f;
                                effects.Add(new Effect("spark", m.X, m.Y, m.X, m.Y, 20, Color.White, ""));
                            }
                        }
                        else // * [1: 관성 미끄러짐 폼]
                        {
                            m.VX *= 0.94f; m.VY *= 0.94f;
                            m.X += m.VX; m.Y += m.VY;

                            if (Math.Abs(m.VX) < 0.15f && Math.Abs(m.VY) < 0.15f)
                            {
                                m.MonsterState = 0; m.StateTimer = 0;
                            }
                        }
                    }

                    // * [2. Periodic_Hardening_Guard 패턴 구현 - 주기적 정지 경화 방어]
                    else if (allocatedPattern == "Periodic_Hardening_Guard")
                    {
                        m.StateTimer++;
                        if (m.MonsterState == 0) // * [0: 기본 추적 속성]
                        {
                            m.VX += Math.Sign(player.X - m.X) * 0.15f;
                            m.VY += Math.Sign(player.Y - m.Y) * 0.10f;
                            float maxSpeed = 1.2f; float speed = (float)Math.Sqrt(m.VX * m.VX + m.VY * m.VY);
                            if (speed > maxSpeed) { m.VX = m.VX / speed * maxSpeed; m.VY = m.VY / speed * maxSpeed; }
                            m.X += m.VX; m.Y += m.VY;

                            if (m.StateTimer >= 90) // * [3초 활성화 후 방어]
                            {
                                m.MonsterState = 1; m.StateTimer = 0; m.HitFlash = 30;
                            }
                        }
                        else // * [1: GUARD 정지 상태]
                        {
                            m.VX = 0; m.VY = 0;
                            if (m.HitFlash < 5) m.HitFlash = 5;
                            if (m.StateTimer >= 30) // * [1초 후 해제]
                            {
                                m.MonsterState = 0; m.StateTimer = 0;
                            }
                        }
                    }

                    // * [3. Random_Teleport_Barrage 패턴 구현 - 무작위 텔레포트 탄막 사출]
                    else if (allocatedPattern == "Random_Teleport_Barrage")
                    {
                        m.StateTimer++;
                        m.X += m.VX * 0.4f; m.Y += m.VY * 0.4f;

                        if (m.StateTimer >= 120) // * [4초 주기 발동]
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
                                effects.Add(new Effect("projectile", m.X, m.Y, m.X + speedX * 100, m.Y + speedY * 100, 50, Color.Cyan, "BULLET"));
                            }
                        }
                    }

                    // * [4. Heavy_Projectile_Spread 패턴 구현 - 정예 묵직 이동 및 파편 살포]
                    else if (allocatedPattern == "Heavy_Projectile_Spread")
                    {
                        // * [물리: 묵직한 가속 및 느린 최대 속도 유지]
                        if (tick % (110 + i * 15) == 0)
                        {
                            m.VX += Math.Sign(player.X - m.X) * 0.12f;
                            m.VY += Math.Sign(player.Y - m.Y) * 0.08f;
                        }
                        float maxSpeed = 0.9f;
                        float speed = (float)Math.Sqrt(m.VX * m.VX + m.VY * m.VY);
                        if (speed > maxSpeed) { m.VX = m.VX / speed * maxSpeed; m.VY = m.VY / speed * maxSpeed; }
                        m.X += m.VX; m.Y += m.VY;

                        // * [공격: 주기적으로 TRASH 투사체 조준 발사]
                        if (tick % 70 == 0)
                        {
                            effects.Add(new Effect("projectile", m.X, m.Y, player.X, player.Y, 40, Color.OrangeRed, "TRASH"));
                        }
                    }

                    // * [5. Slime_Bounce_Approach 패턴 구현 - 슬라임 점프 도약식 느린 추적]
                    else if (allocatedPattern == "Slime_Bounce_Approach")
                    {
                        m.StateTimer++;
                        if (m.StateTimer % 40 < 18) // * [도약 상태]
                        {
                            m.VX += Math.Sign(player.X - m.X) * 0.18f;
                            m.VY += Math.Sign(player.Y - m.Y) * 0.12f;
                        }
                        else // * [착지 브레이크 상태]
                        {
                            m.VX *= 0.75f; m.VY *= 0.75f;
                        }
                        float maxSpeed = 0.9f; float speed = (float)Math.Sqrt(m.VX * m.VX + m.VY * m.VY);
                        if (speed > maxSpeed) { m.VX = m.VX / speed * maxSpeed; m.VY = m.VY / speed * maxSpeed; }
                        m.X += m.VX; m.Y += m.VY;
                    }

                    // * [6. Contamination_Zone_Leak 패턴 구현 - 디버프 오염장판 흔적 전개]
                    else if (allocatedPattern == "Contamination_Zone_Leak")
                    {
                        m.VX += Math.Sign(player.X - m.X) * 0.14f;
                        m.VY += Math.Sign(player.Y - m.Y) * 0.09f;
                        float maxSpeed = 1.1f; float speed = (float)Math.Sqrt(m.VX * m.VX + m.VY * m.VY);
                        if (speed > maxSpeed) { m.VX = m.VX / speed * maxSpeed; m.VY = m.VY / speed * maxSpeed; }
                        m.X += m.VX; m.Y += m.VY;

                        if (tick % 6 == 0)
                        {
                            effects.Add(new Effect("spark", m.X, m.Y, m.X, m.Y, 8, Color.FromArgb(45, Color.LimeGreen), ""));
                        }

                        float dx = player.X - m.X; float dy = player.Y - m.Y;
                        float distance = (float)Math.Sqrt(dx * dx + dy * dy);
                        if (distance <= 130f)
                        {
                            playerCurrentSpeedModifier = 0.6f; // * [이동속도 40% 저하]
                            if (tick % 30 == 0) // * [매초 2%씩 독 연산 처리]
                            {
                                int poisonDamage = (int)(player.MaxHp * 0.02);
                                player.Hp -= poisonDamage;
                                effects.Add(new Effect("text", player.X + 25, player.Y - 80, player.X + 25, player.Y - 80, 20, Color.LimeGreen, "-" + poisonDamage));
                            }
                        }
                    }

                    // * [7. IRQ Conflict 전용 보조 전격 스파크 코드 패턴]
                    else if (allocatedPattern == "IRQ_Lightning_Strike")
                    {
                        m.StateTimer++;
                        if (tick % 12 == 0) { m.VX = (float)(rand.NextDouble() * 3.0 - 1.5); m.VY = (float)(rand.NextDouble() * 2.0 - 1.0); }
                        m.X += m.VX; m.Y += m.VY;

                        if (m.StateTimer >= 90)
                        {
                            m.StateTimer = 0; float rangeRadius = 140f;
                            for (int k = 0; k < 9; k++)
                            {
                                double angle = k * Math.PI * 2 / 9;
                                float tX = m.X + (float)Math.Cos(angle) * rangeRadius;
                                float tY = m.Y + (float)Math.Sin(angle) * rangeRadius;
                                effects.Add(new Effect("projectile", m.X, m.Y, tX, tY, 22, Color.Gold, "SPARK_LINE"));
                            }
                        }
                    }

                    // * [공통 외곽 벽면 충돌 제동]
                    if (m.X < minX) { m.X = minX; m.VX = Math.Abs(m.VX); }
                    if (m.X > maxX) { m.X = maxX; m.VX = -Math.Abs(m.VX); }
                    if (m.Y < minY) { m.Y = minY; m.VY = Math.Abs(m.VY); }
                    if (m.Y > maxY) { m.Y = maxY; m.VY = -Math.Abs(m.VY); }
                }
                else
                {
                    // * [보스 오리지널 패턴 링크 작동 보존 구역]
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