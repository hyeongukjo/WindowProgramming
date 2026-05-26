using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace DebugHeroFileDungeonRPG
{
    public sealed partial class MainForm
    {
        // [홀수층 전용 제어 상태값]
        private int currentWaveIndex = 0;  // 현재 0=a, 1=b, 2=c, 3=d 무리
        private int waveDelayTicks = 0;     // 5초 유예시간 카운터 (30FPS 기준 150틱)
        private bool isWaveWaiting = false; // 유예시간 대기 플래그

        private int GetStageMapWidth(StageInfo st)
        {
            if (stageBossPhase)
                return Math.Max(ClientSize.Width, 1760 + st.Index * 55);

            return Math.Max(ClientSize.Width, 1650 + st.Index * 180);
        }

        private void UpdateStage()
        {
            stageTime++;
            StageInfo st = stages[currentStage - 1];
            int mapWidth = GetStageMapWidth(st);

            PlayerMovementSystem.Update(player, st, stageBossPhase, ClientSize.Width, ClientSize.Height, mapWidth, ref cameraX, tick);
            PlayerMovementSystem.UpdateActionAnimation(player);

            // 몬스터 AI 및 물리 충돌 업데이트
            EnemyUpdateResult enemyResult = EnemyLogicSystem.Update(enemies, player, st, currentStage, stageBossPhase, tick, mapWidth, ClientRectangle, bossRuntime, effects);

            if (enemyResult.PlayerReturnedToStart)
            {
                player.X = 180; player.TargetX = 180; player.TargetY = player.Y;
                player.MoveVelocityX = 0f; player.MoveVelocityY = 0f; player.WalkCycle = 0f;
                effects.Add(new Effect("text", player.X, player.Y - 90, player.X, player.Y - 90, 70, Color.Red, "복구 지점으로 반환"));
            }

            // ---------------------------------------------------------------------------------
            // [Bug 1 해결 핵심]: 죽은 몬스터(Hp <= 0)를 리스트에서 실시간으로 완벽하게 제거하여 Count를 0으로 만듭니다.
            // ---------------------------------------------------------------------------------
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (enemies[i].Hp <= 0)
                {
                    enemies.RemoveAt(i); // 완전히 리스트에서 소거
                }
            }

            // ---------------------------------------------------------------------------------
            // [분기 제어]: 짝수층(보스 스테이지)일 때는 하단의 홀수 웨이브 제어 시스템을 완전히 스킵(Pass-through)
            // ---------------------------------------------------------------------------------
            if (currentStage % 2 == 0) return;

            // ---------------------------------------------------------------------------------
            // [홀수층 전용 무리 시퀀스 및 즉시 보상 제어]
            // ---------------------------------------------------------------------------------
            // 필드에 살아있는 리스트가 완벽히 비었고, 대기 상태가 아니며, 보상 파일이 드롭되지 않았을 때 진입
            if (enemies.Count == 0 && !isWaveWaiting && weaponDrops.Count == 0)
            {
                if (currentWaveIndex < 3) // a(0), b(1), c(2), d(3) 총 4개 무리 제한
                {
                    // 다음 무리가 남아 있다면 즉시 5초 유예시간(150틱) 가동
                    isWaveWaiting = true;
                    waveDelayTicks = 150;

                    char nextWaveChar = (char)('a' + currentWaveIndex + 1);
                    effects.Add(new Effect("text", player.X + 150, player.Y - 120, player.X + 150, player.Y - 120, 120, Color.Yellow, $"무리 정화 완료. 5초 후 {nextWaveChar} 무리 진입..."));
                }
                else
                {
                    // 마지막 d 무리까지 정화 완료 시 그 자리에 무기 강화 파일 즉시 드롭!
                    if (weaponDrops.Count == 0)
                    {
                        WeaponUpgradeFile drop = new WeaponUpgradeFile
                        {
                            X = player.X + 250,
                            Y = player.Y - 15,
                            StageIndex = currentStage,
                            UpgradeLevel = player.WeaponLevel + 1
                        };
                        weaponDrops.Add(drop);
                        effects.Add(new Effect("text", drop.X, drop.Y - 70, drop.X, drop.Y - 70, 150, Color.Cyan, "모든 무리 정화! UPGRADE FILE DROP"));
                    }
                }
            }

            // 5초 타이머 카운트다운 처리 및 다음 웨이브 호출
            if (isWaveWaiting)
            {
                waveDelayTicks--;
                if (waveDelayTicks <= 0)
                {
                    isWaveWaiting = false;
                    currentWaveIndex++;

                    // StageEnemyFactory에서 다음 순번의 무리 소환
                    enemies.AddRange(StageEnemyFactory.CreateWaveEnemies(st, currentWaveIndex, ClientSize.Height));

                    char currentWaveChar = (char)('a' + currentWaveIndex);
                    effects.Add(new Effect("text", player.X + 150, player.Y - 120, player.X + 150, player.Y - 120, 80, Color.Orange, $"{currentWaveChar} 무리 출현! 시스템을 정화하세요."));
                    TryBeep(640, 70);
                }
            }
        }

        private void StartStage(int stageIndex)
        {
            if (stageIndex < 1 || stageIndex > unlockedStage) return;
            currentStage = stageIndex;
            StageInfo st = stages[stageIndex - 1];

            enemies.Clear();
            effects.Clear();
            weaponDrops.Clear();
            draggedWeaponDrop = null;
            bossRuntime.Reset(currentStage);
            stage1BossPhase = false;
            player.Hp = player.MaxHp;
            player.Mp = player.MaxMp;
            player.SystemStability = 100;
            player.X = 180;
            player.Y = ClientSize.Height - 118;
            player.TargetX = player.X;
            player.TargetY = player.Y;
            player.MoveVelocityX = 0f;
            player.MoveVelocityY = 0f;
            stageTime = 0;
            cameraX = 0;
            stageNpcHintClosed = false;
            firstDesktopNotice = false;
            screen = ScreenMode.Stage;

            // 💡 [핵심 개편] 스테이지 속성이 보스/최종방(IsBossStage)이면 일반 몹 없이 바로 보스를 스폰합니다!
            if (st.IsBossStage)
            {
                stageBossPhase = true;
                enemies.Add(StageEnemyFactory.CreateBoss(st, Math.Max(760, ClientSize.Width - 360), ClientSize.Height, stages.Count));
                string bossText = $"STAGE {currentStage:00} 보스 레이드 개시: {st.BossName}";
                effects.Add(new Effect("text", player.X + 290, player.Y - 120, player.X + 290, player.Y - 120, 100, Color.Gold, bossText));
            }
            else
            {
                stageBossPhase = false;
                // [수정]: 5초 대기 없이 진입 즉시 첫 번째 'a' 무리(인덱스 0)를 필드에 강제 소환합니다!
                enemies.AddRange(StageEnemyFactory.CreateWaveEnemies(st, currentWaveIndex, ClientSize.Height));
                effects.Add(new Effect("text", player.X + 220, player.Y - 110, player.X + 220, player.Y - 110, 80, Color.FromArgb(220, 255, 255), "일반 데이터 정화 시작"));
            }

            TryBeep(600, 80);
        }

        private void ApplyWeaponUpgrade(WeaponUpgradeFile drop)
        {
            player.WeaponLevel = Math.Max(player.WeaponLevel + 1, drop.UpgradeLevel);
            effects.Add(new Effect("text", player.X, player.Y - 104, player.X, player.Y - 104, 80, Color.Gold, "WEAPON +" + player.WeaponLevel));
            TryBeep(980, 90);

            // 무기 강화 압축 파일을 캐릭터 바운드에 드롭하는 즉시 스테이지 클리어
            ClearCurrentStage();
        }

        private void ClearCurrentStage()
        {
            clearStage = currentStage;
            player.ClearedStages = Math.Max(player.ClearedStages, clearStage);
            player.Level++;
            player.Exp += 50 + stages[clearStage - 1].Index * 20;

            lastClearWasBoss = (clearStage % 2 == 0);
            if (clearStage < stages.Count) unlockedStage = Math.Max(unlockedStage, clearStage + 1);

            currentStage = 0;
            enemies.Clear();
            screen = ScreenMode.StageClearDialog;
            TryBeep(720, 90);
        }

        private void AwardDefeatReward(GameEntity m)
        {
            if (m == null || m.RewardGiven) return;
            RewardSystem.AwardDefeatReward(m, player, currentStage, effects, random);
        }


        private void StartStageBossPhase()
        {
            StageInfo st = stages[currentStage - 1];
            enemies.Clear();
            effects.Clear();
            weaponDrops.Clear();
            draggedWeaponDrop = null;
            bossRuntime.Reset(currentStage);
            stageBossPhase = true;
            stage1BossPhase = false;
            stageNpcHintClosed = true;

            player.Hp = Math.Min(player.MaxHp, player.Hp + Math.Max(20, player.MaxHp / 5));
            player.Mp = Math.Min(player.MaxMp, player.Mp + Math.Max(12, player.MaxMp / 4));
            player.SystemStability = Math.Min(100, player.SystemStability + 5);
            player.X = 190;
            player.TargetX = player.X;
            player.TargetY = player.Y;
            player.MoveVelocityX = 0f;
            player.MoveVelocityY = 0f;
            player.WalkCycle = 0f;
            cameraX = 0;
            stageTime = 0;

            enemies.Add(StageEnemyFactory.CreateBoss(st, Math.Max(760, ClientSize.Width - 360), ClientSize.Height, stages.Count));
            string bossText = "STAGE " + currentStage.ToString("00") + " 보스방 자동 진입: " + st.BossName;
            effects.Add(new Effect("text", player.X + 290, player.Y - 120, player.X + 290, player.Y - 120, 100, Color.Gold, bossText));
            effects.Add(new Effect("spark", player.X + 280, player.Y - 48, player.X + 280, player.Y - 48, 55, Color.Gold, ""));
            TryBeep(760, 90);
        }

        private void DropWeaponUpgradeFile(GameEntity boss)
        {
            for (int i = 0; i < weaponDrops.Count; i++)
            {
                if (weaponDrops[i].StageIndex == currentStage) return;
            }
            WeaponUpgradeFile drop = new WeaponUpgradeFile
            {
                X = boss.X,
                Y = Math.Max(115, Math.Min(ClientSize.Height - 95, boss.Y - 25)),
                StageIndex = currentStage,
                UpgradeLevel = player.WeaponLevel + 1
            };
            weaponDrops.Add(drop);
            effects.Add(new Effect("text", drop.X, drop.Y - 72, drop.X, drop.Y - 72, 120, Color.LightSkyBlue, "UPGRADE FILE DROP"));
        }

        private void UseHpPotion()
        {
            if (screen != ScreenMode.Stage) return;
            if (player.HpPotions <= 0)
            {
                effects.Add(new Effect("text", player.X, player.Y - 88, player.X, player.Y - 88, 35, Color.OrangeRed, "HP 포션 없음"));
                TryBeep(320, 70);
                return;
            }
            if (player.Hp >= player.MaxHp)
            {
                effects.Add(new Effect("text", player.X, player.Y - 88, player.X, player.Y - 88, 30, Color.LightGray, "HP 가득"));
                return;
            }
            player.HpPotions--;

            // * [수정 보정: 고정 수치 회복을 폐기하고 플레이어 최대 체력의 정확히 25% 비율로 연산]
            int heal = (int)(player.MaxHp * 0.25);
            if (player.Hp + heal > player.MaxHp) heal = player.MaxHp - player.Hp;

            player.Hp += heal;
            effects.Add(new Effect("spark", player.X, player.Y - 42, player.X, player.Y - 42, 34, Color.LimeGreen, ""));
            effects.Add(new Effect("text", player.X, player.Y - 92, player.X, player.Y - 92, 42, Color.LimeGreen, "HP +" + heal));
            TryBeep(760, 55);
        }

        private void UseMpPotion()
        {
            if (screen != ScreenMode.Stage) return;
            if (player.MpPotions <= 0)
            {
                effects.Add(new Effect("text", player.X, player.Y - 88, player.X, player.Y - 88, 35, Color.DeepSkyBlue, "MP 포션 없음"));
                TryBeep(320, 70);
                return;
            }
            if (player.Mp >= player.MaxMp)
            {
                effects.Add(new Effect("text", player.X, player.Y - 88, player.X, player.Y - 88, 30, Color.LightGray, "MP 가득"));
                return;
            }
            player.MpPotions--;

            // * [수정 보정: 고정 수치 회복을 폐기하고 플레이어 최대 마나의 정확히 25% 비율로 연산]
            int restore = (int)(player.MaxMp * 0.25);
            if (player.Mp + restore > player.MaxMp) restore = player.MaxMp - player.Mp;

            player.Mp += restore;
            effects.Add(new Effect("spark", player.X, player.Y - 42, player.X, player.Y - 42, 34, Color.DeepSkyBlue, ""));
            effects.Add(new Effect("text", player.X, player.Y - 92, player.X, player.Y - 92, 42, Color.DeepSkyBlue, "MP +" + restore));
            TryBeep(820, 55);
        }

        private int ResolveAttackDirection(float range)
        {
            int fallback = player.Facing == 0 ? 1 : player.Facing;
            float moveDx = player.TargetX - player.X;
            if (Math.Abs(moveDx) > 6f) fallback = moveDx > 0 ? 1 : -1;

            float bestScore = float.MaxValue;
            int bestDir = fallback;
            for (int i = 0; i < enemies.Count; i++)
            {
                GameEntity m = enemies[i];
                if (m.Hp <= 0) continue;
                float dx = m.X - player.X;
                float dist = Math.Abs(dx);
                if (dist > range + 170f) continue;
                if (Math.Abs(m.Y - player.Y) > 180f) continue;

                int dir = dx >= 0 ? 1 : -1;
                float score = dist;
                // 이동 중에는 이동 방향의 적을 우선하지만, 아주 가까운 반대편 적은 자동 보정합니다.
                if (dir != fallback && dist > 130f) score += 90f;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestDir = dir;
                }
            }
            return bestDir;
        }

        private float GetMovingAttackOriginX(int dir)
        {
            float moveDx = player.TargetX - player.X;
            if (Math.Abs(moveDx) <= 6f) return player.X;

            int moveDir = moveDx > 0 ? 1 : -1;
            // 이동 방향과 공격 방향이 같을 때만 살짝 앞당겨서, 달리면서 쏠 때 투사체와 판정이 뒤처지지 않게 합니다.
            if (moveDir == dir)
            {
                float lead = Math.Min(48f, Math.Abs(moveDx) * 0.22f);
                return player.X + dir * lead;
            }
            return player.X;
        }

        private void CastSkill(int slot)
        {
            if (screen != ScreenMode.Stage) return;
            int mpCost = slot == 0 ? 0 : slot == 1 ? 8 : slot == 2 ? 16 : 10;
            if (player.Mp < mpCost)
            {
                effects.Add(new Effect("text", player.X, player.Y - 92, player.X, player.Y - 92, 36, Color.DeepSkyBlue, "MP 부족"));
                TryBeep(300, 80);
                return;
            }
            player.Mp -= mpCost;

            if (slot == 3)
            {
                player.DefenseTicks = 100;
                player.SystemStability = Math.Min(100, player.SystemStability + 4);
                effects.Add(new Effect("spark", player.X, player.Y - 42, player.X, player.Y - 42, 52, Color.LightSkyBlue, ""));
                effects.Add(new Effect("text", player.X, player.Y - 96, player.X, player.Y - 96, 52, Color.LightSkyBlue, "DEFEND"));
                TryBeep(360, 70);
                return;
            }

            bool moving = Math.Abs(player.TargetX - player.X) > 6f || Math.Abs(player.TargetY - player.Y) > 6f;
            float range = slot == 0 ? 190f : slot == 1 ? 265f : 390f;
            if (moving) range += 65f; // 이동 중에는 판정이 밀리지 않도록 사거리를 약간 보정

            int dir = ResolveAttackDirection(range);
            player.Facing = dir;

            float originX = GetMovingAttackOriginX(dir);
            int damage = slot == 0 ? 30 + player.Level * 8 : slot == 1 ? 22 + player.Level * 6 : 74 + player.Level * 14;
            damage += (player.WeaponLevel - 1) * (slot == 2 ? 18 : slot == 1 ? 9 : 7);
            Color color = slot == 0 ? Color.FromArgb(80, 190, 255) : slot == 1 ? Color.FromArgb(80, 255, 130) : Color.FromArgb(255, 210, 60);
            float startX = originX + dir * 34;
            float endX = originX + dir * range;
            float effectY = player.Y - 36 - slot * 3;

            string agentPose = slot == 2 ? "playerDelete" : slot == 1 ? "playerClean" : "playerSlash";
            string actionText = slot == 2 ? "FLASH" : slot == 1 ? "SPRAY" : "SLASH";
            int fxTicks = slot == 0 ? 26 : slot == 1 ? 44 : 38;
            effects.Add(new Effect(agentPose, originX, player.Y, endX, player.Y, fxTicks, color, actionText));
            // Player skill uses one continuous beam. Extra projectile effect removed to prevent double-shot visual.
            effects.Add(new Effect("spark", startX, player.Y - 44, startX, player.Y - 44, 18 + slot * 4, Color.White, ""));
            if (slot == 2) effects.Add(new Effect("spark", originX, player.Y - 40, originX, player.Y - 40, 44, color, ""));

            // 핵심 수정: 이동 중에도 판정이 캐릭터 현재 위치에만 묶이지 않도록 originX, 추가 폭, 넓은 Y 범위 적용.
            RectangleF hit = new RectangleF(
                dir > 0 ? originX - 22f : originX - range - 48f,
                player.Y - (slot == 1 ? 150f : 124f),
                range + 92f,
                slot == 1 ? 210f : slot == 2 ? 190f : 158f
            );

            bool hitAny = false;
            for (int i = 0; i < enemies.Count; i++)
            {
                GameEntity m = enemies[i];
                if (m.Hp <= 0) continue;
                if (hit.IntersectsWith(m.Bounds))
                {
                    // * [기본 스킬 대미지 배정]
                    int finalDamage = damage;

                    // * [2. 피드백 반영: Empty_Folder가 경화 상태(MonsterState == 1)일 때 받는 피해 50% 반감 처리]
                    if (m.Name == "Empty_Folder" && m.MonsterState == 1)
                    {
                        finalDamage = Math.Max(1, finalDamage / 2); // * [대미지 절반으로 감소]

                        // * [단단한 껍데기에 막혔다는 시각적 피드백 효과 텍스트 추가]
                        effects.Add(new Effect("text", m.X + 20, m.Y - 104, m.X + 20, m.Y - 104, 26, Color.LightGray, "GUARD!"));
                    }

                    // * [최종 연산된 대미지를 몬스터 체력에서 차감]
                    m.Hp -= finalDamage;
                    m.HitFlash = 10;
                    hitAny = true;

                    effects.Add(new Effect("text", m.X, m.Y - 84, m.X, m.Y - 84, 34, Color.Yellow, finalDamage.ToString()));
                    effects.Add(new Effect("spark", m.X, m.Y - 44, m.X, m.Y - 44, 22, Color.White, ""));
                    if (m.Hp <= 0) AwardDefeatReward(m);
                }
            }

            if (!hitAny)
            {
                // 빗맞아도 이동 중 공격 입력이 먹었다는 피드백을 남겨 조작감을 명확하게 합니다.
                effects.Add(new Effect("text", endX, effectY - 34, endX, effectY - 34, 18, Color.LightGray, "MISS"));
            }

            TryBeep(520 + slot * 130, 45);
        }

    }
}
