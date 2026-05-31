using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
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
        private bool IsStageNpcHintOpen()
        {
            return screen == ScreenMode.Stage &&
                   currentStage > 0 &&
                   !stageNpcHintClosed &&
                   !showStageClearPopup;
        }

        private void UpdateStage()
        {
            stageTime++;
            totalGameTime++;
            StageInfo st = stages[currentStage - 1];
            int mapWidth = GetStageMapWidth(st);

            // 매 프레임마다 스킬 Cooldown 실시간 연산 차감 스케줄러
            if (wCooldownTicks > 0) wCooldownTicks--;
            if (eCooldownTicks > 0) eCooldownTicks--;
            if (rCooldownTicks > 0) rCooldownTicks--;

            if (eShieldDurationTicks > 0)
            {
                eShieldDurationTicks--;
                if (eShieldDurationTicks <= 0)
                {
                    // 5초가 지나면 쉴드량이 남아있어도 강제로 0으로 만들어 증발시킵니다.
                    if (playerShield > 0)
                    {
                        playerShield = 0;
                        effects.Add(new Effect("text", player.X, player.Y - 75, player.X, player.Y - 75, 30, Color.LightGray, "⏳ 실드 유지시간 만료"));
                    }
                }
            }

            if (wBuffTicks > 0)
            {
                wBuffTicks--;
                player.X += (player.MoveVelocityX * 0.5f);
            }

            // 실시간 체력 피해 가로채기를 위한 이전 HP 백업선 세팅
            int preUpdateHp = player.Hp;

            PlayerMovementSystem.Update(player, st, stageBossPhase, ClientSize.Width, ClientSize.Height, mapWidth, ref cameraX, tick);
            PlayerMovementSystem.UpdateActionAnimation(player);

            // 몬스터 AI 및 물리 충돌 업데이트
            EnemyUpdateResult enemyResult = EnemyLogicSystem.Update(enemies, player, st, currentStage, stageBossPhase, tick, mapWidth, ClientRectangle, bossRuntime, effects);


            // ==========================================================
            //  [E 보호막 데미지 상쇄 엔진] 내 체력이 깎였을 때 실드가신 가로채기 연산
            // ==========================================================
            if (player.Hp < preUpdateHp && playerShield > 0)
            {
                int damageTaken = preUpdateHp - player.Hp; // 적이 입힌 원본 피해량
                if (playerShield >= damageTaken)
                {
                    playerShield -= damageTaken;
                    player.Hp = preUpdateHp; // 플레이어 본체 체력 원상복구 방어
                    effects.Add(new Effect("text", player.X, player.Y - 75, player.X, player.Y - 75, 30, Color.DeepSkyBlue, $"🛡️ 실드 데이터 상쇄 (-{damageTaken})"));
                }
                else
                {
                    int remainder = damageTaken - playerShield;
                    playerShield = 0;
                    player.Hp = preUpdateHp - remainder; // 남은 관통 대미지만 본체에 적중
                    effects.Add(new Effect("text", player.X, player.Y - 75, player.X, player.Y - 75, 35, Color.Cyan, "💥 방화벽 실드 파괴!"));
                }
            }

            // ==========================================================
            // [R 궁극기 낙하 물리 폭발 스케줄러] 땅에 닿는 순간 광역 타격
            // ==========================================================
            for (int i = playerSkySwords.Count - 1; i >= 0; i--)
            {
                var sword = playerSkySwords[i];
                sword.Timer--;

                if (sword.Timer <= 0)
                {
                    float explosionRadius = 240f; // 폭발 감지 사거리
                    int finalUltDamage = 450 + player.Level * 40; // 궁극기 기본 누킹 데미지
                    if (wBuffTicks > 0) finalUltDamage = (int)(finalUltDamage * 1.5f); // W버프 시 궁극기 분쇄딜 증폭

                    // 맵 전체 적 탐색 후 범위 내 일괄 대미지 및 힛플래시 주입
                    for (int k = 0; k < enemies.Count; k++)
                    {
                        var em = enemies[k];
                        if (em.Hp > 0 && Math.Abs(em.X - sword.X) <= explosionRadius)
                        {
                            em.Hp -= finalUltDamage;
                            em.HitFlash = 12;
                            effects.Add(new Effect("text", em.X, em.Y - 85, em.X, em.Y - 85, 45, Color.DodgerBlue, $"❄️ COLD BURST {finalUltDamage}"));
                            effects.Add(new Effect("spark", em.X, em.Y - 40, em.X, em.Y - 40, 25, Color.LightBlue, ""));
                        }
                    }

                    effects.Add(new Effect("spark", sword.X, sword.Y - 30, sword.X, sword.Y - 30, 75, Color.LightCyan, ""));
                    TryBeep(220, 250); // 중저음의 묵직한 폭발음 피드백
                    playerSkySwords.RemoveAt(i);
                }
            }


            if (enemyResult.PlayerReturnedToStart)
            {
                player.X = 180; player.TargetX = 180; player.TargetY = player.Y;
                player.MoveVelocityX = 0f; player.MoveVelocityY = 0f; player.WalkCycle = 0f;
                effects.Add(new Effect("text", player.X, player.Y - 90, player.X, player.Y - 90, 70, Color.Red, "복구 지점으로 반환"));
            }

           
            if (!stageBossPhase)
            {
                for (int i = enemies.Count - 1; i >= 0; i--)
                {
                    if (enemies[i].Hp <= 0)
                    {
                        enemies.RemoveAt(i); // 완전히 리스트에서 메모리 소거 완료
                    }
                }
            }

            // ==========================================================
            // 본체가 죽어 enemies 리스트에서 지워지더라도,
            // 분신 패턴이 실행 중이라면 이곳에서 강제로 제어권을 넘겨받아 3초 링크/타이머를 계속 연산시킵니다.
            // ==========================================================
            if (stageBossPhase && currentStage == 10 && bossRuntime.patternManager.IsIllusionActive)
            {
                GameEntity mainBoss = enemies.Find(e => e.IsBoss);
                bossRuntime.patternManager.Update(mainBoss, player, effects, mapWidth);
            }

            
            if (stageBossPhase && currentStage == 10)
            {
                // 현재 에너미 리스트에서 최종 보스 본체 객체를 탐색합니다.
                GameEntity mainBoss = enemies.Find(e => e.IsBoss);

                // 1. 정상 기믹을 거쳤든, 데미지가 높아서 원턴킬로 증발했든 상관없이 
                //    '보스 본체 데이터가 존재하고 체력이 0 이하'라면 무조건 클리어 판정 진입!
                if (mainBoss != null && mainBoss.Hp <= 0)
                {
                    // 2. 만약 정상 피격으로 분신 발악 기믹(IsIllusionActive)이 발동된 상태라면,
                    //    잔여 분신(BinnyClone)까지 완벽하게 처리되었는지 한 번 더 체크해 줍니다.
                    bool isCloneActiveNow = bossRuntime.patternManager.IsIllusionActive;
                    bool isCloneDeadOrNull = (bossRuntime.patternManager.BinnyClone == null) || bossRuntime.patternManager.IsCloneDead;

                    // 분신 기믹 중이 아닐 때 보스가 한방에 죽었거나, 분신 기믹 중인데 분신까지 다 잡았다면 최종 승리!
                    if (!isCloneActiveNow || isCloneDeadOrNull)
                    {
                        // 잔여 상태값 깔끔하게 초기화
                        bossRuntime.patternManager.IsIllusionActive = false;
                        bossRuntime.patternManager.BinnyClone = null;

                        ClearCurrentStage();
                        return;
                    }
                }
            }
            // STAGE SYSTEM COMPLETED 팝업 막고, ScreenMode.StageClearDialog으로 넘어가서 npc클리어 대사 출력 수정
            if (currentStage != 10)
            {
                bool anyEnemyAlive = false;

                for (int i = 0; i < enemies.Count; i++)
                {
                    if (enemies[i].Hp > 0)
                    {
                        anyEnemyAlive = true;
                        break;
                    }
                }

                if (enemies.Count > 0 && !anyEnemyAlive)
                {
                    ClearCurrentStage();
                    return;
                }
            }
           
            if ((enemies.Count == 0 || enemyResult.AllEnemiesDefeated) && currentStage != 10)
            {
                if (st.Index == 10 && bossRuntime.patternManager.IsIllusionActive) return;

                if (!stageBossPhase)
                {
                    if (st.Kind == StageKind.Normal)
                    {
                        // ---------------------------------------------------------------------------------
                        // [분기 제어]: 짝수층(보스 스테이지)일 때는 하단의 홀수 웨이브 제어 시스템을 완전히 스킵(Pass-through)
                        // ---------------------------------------------------------------------------------
                        if (currentStage % 2 == 0) return;

                      
                        if (enemies.Count == 0 && !isWaveWaiting && weaponDrops.Count == 0)
                        {
                            //  현재 2웨이브(인덱스 1) 무리를 모두 정화했다면, 더 이상 기다리지 않고 즉시 보상 스폰 단계로 스위칭!
                            if (currentWaveIndex == 1)
                            {
                                currentWaveIndex = 3;  // 기존의 최종 보상 획득 조건(>=3)을 강제로 트리거
                                waveDelayTicks = 0;    // 5초 지연(150틱)을 즉시 0으로 소거하여 딜레이 파쇄
                                isWaveWaiting = false; // 대기 없이 아래 보상 스폰 로직을 즉시 실행하도록 설정

                                //  3웨이브에는 아무것도 추가하지 않고 즉시 보상 파일 스폰
                                WeaponUpgradeFile drop = new WeaponUpgradeFile
                                {
                                    X = player.X + 250,
                                    Y = player.Y - 15,
                                    StageIndex = currentStage,
                                    UpgradeLevel = player.WeaponLevel + 1
                                };
                                weaponDrops.Add(drop);
                                TryBeep(880, 150); // 보상 출현 기분 좋은 Beep 음 연출
                            }
                            // 1웨이브(인덱스 0) 클리어 시에는 정상적으로 5초 뒤 2웨이브가 소환되도록 유지
                            else if (currentWaveIndex < 1)
                            {
                                isWaveWaiting = true;
                                waveDelayTicks = 150; // 5초 대기 시간 할당
                            }
                            else
                            {
                                // 방어 코드: 예외적인 인덱스 상태일 때 기존 보상 트리거 유지
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
                                }
                            }
                        }

                        // 5초 타이머 카운트다운 처리 및 다음 웨이브 호출 (1웨이브 클리어 직후에만 정상 작동함)
                        if (isWaveWaiting)
                        {
                            waveDelayTicks--;
                            if (waveDelayTicks <= 0)
                            {
                                isWaveWaiting = false;
                                currentWaveIndex++;

                                // StageEnemyFactory에서 설계한 2웨이브(인덱스 1)의 적들을 정상 스폰
                                enemies.AddRange(StageEnemyFactory.CreateWaveEnemies(st, currentWaveIndex, ClientSize.Height));
                                TryBeep(640, 70);
                            }
                        }
                        return;
                    }
                    StartStageBossPhase();
                    return;
                }
            }
        }

        // [최종 디버깅]: 스테이지 전환 시 웨이브 인덱스 유실로 인한 1, 2웨이브 동시 시작 버그 해결
        private void StartStage(int stageIndex)
        {
            if (stageIndex < 1 || stageIndex > unlockedStage) return;
            if (stageIndex == 10)
            {
                StartStage10Cutscene();
                return;
            }
            currentStage = stageIndex;
            StageInfo st = stages[stageIndex - 1];

            enemies.Clear();
            effects.Clear();
            weaponDrops.Clear();
            draggedWeaponDrop = null;
            bossRuntime.Reset(currentStage);
            stage1BossPhase = false;

            currentWaveIndex = 0;
            isWaveWaiting = false;
            waveDelayTicks = 0;

            player.Hp = player.MaxHp;
            player.Mp = player.MaxMp;
            player.SystemStability = 100;
            wCooldownTicks = 0;         // W 쿨타임 제로화
            eCooldownTicks = 0;         // E 쿨타임 제로화
            rCooldownTicks = 0;         // R 쿨타임 제로화

            wBuffTicks = 0;             // W 지속 버프 강제 종료
            playerShield = 0;           // E 보호막 내구도 리셋
            eShieldDurationTicks = 0;   // E 보호막 유지시간 타이머 리셋
            playerSkySwords.Clear();    // 하늘에 남아있던 R 궁극기 검 객체 리스트 완전 소멸
            lastPlayerHpForMotion = player.Hp;
            playerHitMotionCooldown = 0;
            playerDeathSequenceActive = false;
            playerDeathSequenceTicks = 0;
            player.ActionState = PlayerActionState.Idle;
            player.ActionFrame = 0;
            player.ActionTick = 0;
            player.SkillIndex = -1;
            player.X = 180;
            player.Y = ClientSize.Height - 118;
            player.TargetX = player.X;
            player.TargetY = player.Y;
            player.MoveVelocityX = 0f;
            player.MoveVelocityY = 0f;
            stageTime = 0;
            stageNpcHintIndex = 0;
            cameraX = 0;
            stageNpcHintClosed = false;

            if (stageIndex == 1)
            {
                totalGameTime = 0;
            }

            enemies.AddRange(StageEnemyFactory.CreatePreBossEnemies(st, ClientSize.Height, random));
            effects.Add(new Effect("text", player.X + 220, player.Y - 110, player.X + 220, player.Y - 110, 80, Color.FromArgb(220, 255, 255), "몬스터 정리 후 보스방 자동 진입"));
            firstDesktopNotice = false;
            screen = ScreenMode.Stage;

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
                enemies.AddRange(StageEnemyFactory.CreateWaveEnemies(st, currentWaveIndex, ClientSize.Height));
              
            }
            SaveCurrentGame();

            TryBeep(600, 80);
        }

        private void ApplyWeaponUpgrade(WeaponUpgradeFile drop)
        {
            player.WeaponLevel = Math.Max(player.WeaponLevel + 1, drop.UpgradeLevel);
            effects.Add(new Effect("text", player.X, player.Y - 104, player.X, player.Y - 104, 80, Color.Gold, "WEAPON +" + player.WeaponLevel));
            TryBeep(980, 90);

            ClearCurrentStage();
        }

        private void ClearCurrentStage()
        {
            clearStage = currentStage;
            if (clearStage == 2 || clearStage == 4 || clearStage == 6 || clearStage == 8 || clearStage == 10)
            {
                _ = SupabaseManager.SendClearLogAsync(
                    player.ProfileName,
                    clearStage,
                    totalGameTime, // 1스테이지부터 해당 보스를 잡기까지 누적된 총 시간 (틱 수)
                    player.TotalDeaths // 누적 사망(재도전) 수
                );
            }
            player.ClearedStages = Math.Max(player.ClearedStages, clearStage);
            player.Level++;
            player.Exp += 50 + stages[clearStage - 1].Index * 20;

            int stageClearBonus = clearStage * 100;
            player.Coins += stageClearBonus;

            lastClearWasBoss = (clearStage % 2 == 0);
            if (clearStage < stages.Count) unlockedStage = Math.Max(unlockedStage, clearStage + 1);

            currentStage = 0;
            enemies.Clear();
            screen = ScreenMode.StageClearDialog;

            SaveCurrentGame();

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
            lastPlayerHpForMotion = player.Hp;
            playerHitMotionCooldown = 0;
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

            //[유지 가드]: 보스방 자동 진입 연출 문구 역시 보스 스테이지 규칙에 해당하므로 그대로 남겨둡니다.
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
            effects.Add(new Effect("text", drop.X, drop.Y - 72, drop.X, drop.Y - 72, 120, Color.LightSkyBlue, "Weapon Patch Drop"));
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

            if (slot == 0 && player.ActionState == PlayerActionState.Skill && player.SkillIndex == 0)
            {
                return;
            }
           
            int mpCost = slot == 0 ? 0 : slot == 1 ? 8 : slot == 2 ? 16 : 10;
            if (player.Mp < mpCost)
            {
                effects.Add(new Effect("text", player.X, player.Y - 92, player.X, player.Y - 92, 36, Color.DeepSkyBlue, "MP 부족"));
                TryBeep(300, 80);
                return;
            }
            player.Mp -= mpCost;

            PlayerMovementSystem.StartSkillAnimation(player, slot);

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
            if (moving) range += 65f;

            int dir = ResolveAttackDirection(range);
            player.Facing = dir;

            float originX = GetMovingAttackOriginX(dir);
            // =============================================================================
            //무기 강화 효율이 칼같이 작동하는 소울류 정석 하드코어 밸런스 폼
            // =============================================================================
            int damage = 0;
            if (slot == 0) damage = 10 + (player.WeaponLevel * 3); // Q 평타 대미지 공식
            else if (slot == 1) damage = 15 + (player.WeaponLevel * 4); // 정예용 스킬 1 대미지
            else if (slot == 2) damage = 25 + (player.WeaponLevel * 6); // 정예용 스킬 2 대미지
            else damage = 10;

            // [W 버프 공격력 연동] 오버클럭 상태일 때 최종 피해량 1.5배 증폭
            if (wBuffTicks > 0)
            {
                damage = (int)(damage * 1.5f);
            }

            
            Color color = slot == 0 ? Color.FromArgb(80, 190, 255) : slot == 1 ? Color.FromArgb(80, 255, 130) : Color.FromArgb(255, 210, 60);
            float startX = originX + dir * 34;
            float endX = originX + dir * range;
            float effectY = player.Y - 36 - slot * 3;

            string agentPose = slot == 2 ? "playerDelete" : slot == 1 ? "playerClean" : "playerSlash";
            string actionText = slot == 2 ? "FLASH" : slot == 1 ? "SPRAY" : "SLASH";
            int fxTicks = slot == 0 ? 26 : slot == 1 ? 44 : 38;
            effects.Add(new Effect(agentPose, originX, player.Y, endX, player.Y, fxTicks, color, actionText));
            effects.Add(new Effect("spark", startX, player.Y - 44, startX, player.Y - 44, 18 + slot * 4, Color.White, ""));
            if (slot == 2) effects.Add(new Effect("spark", originX, player.Y - 40, originX, player.Y - 40, 44, color, ""));

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
                    if ((m.Name.Contains("Binny") || m.Name.Contains("Illegal_Binny")) && bossRuntime.patternManager.IsDPSCheckActive && bossRuntime.patternManager.BinnyShield > 0)
                    {
                        bossRuntime.patternManager.BinnyShield -= damage;
                        if (bossRuntime.patternManager.BinnyShield < 0) bossRuntime.patternManager.BinnyShield = 0;
                        effects.Add(new Effect("text", m.X, m.Y - 84, m.X, m.Y - 84, 34, Color.DeepSkyBlue, $"[SHIELD -{damage}]"));
                    }
                    else
                    {
                        m.Hp -= damage;
                        effects.Add(new Effect("text", m.X, m.Y - 84, m.X, m.Y - 84, 34, Color.Yellow, damage.ToString()));
                    }

                    m.HitFlash = 10;
                    hitAny = true;
                    effects.Add(new Effect("spark", m.X, m.Y - 44, m.X, m.Y - 44, 22, Color.White, ""));

                    if (m.Hp <= 0 && !m.Name.Contains("Binny") && !m.Name.Contains("Illegal_Binny"))
                    {
                        AwardDefeatReward(m);
                    }
                }
            }

            if (bossRuntime.patternManager.IsIllusionActive && bossRuntime.patternManager.BinnyClone != null && !bossRuntime.patternManager.IsCloneDead)
            {
                RectangleF cloneBounds = new RectangleF(
                    bossRuntime.patternManager.BinnyClone.X - 121f,
                    bossRuntime.patternManager.BinnyClone.Y - 252f,
                    243f,
                    252f
                );

                if (hit.IntersectsWith(cloneBounds))
                {
                    bossRuntime.patternManager.BinnyClone.Hp -= damage;
                    if (bossRuntime.patternManager.BinnyClone.Hp < 0) bossRuntime.patternManager.BinnyClone.Hp = 0;

                    hitAny = true;
                    effects.Add(new Effect("text", bossRuntime.patternManager.BinnyClone.X, bossRuntime.patternManager.BinnyClone.Y - 84, bossRuntime.patternManager.BinnyClone.X, bossRuntime.patternManager.BinnyClone.Y - 84, 34, Color.Purple, damage.ToString()));
                    effects.Add(new Effect("spark", bossRuntime.patternManager.BinnyClone.X, bossRuntime.patternManager.BinnyClone.Y - 44, bossRuntime.patternManager.BinnyClone.X, bossRuntime.patternManager.BinnyClone.Y - 44, 22, Color.Purple, ""));
                }
            }

            if (!hitAny)
            {
                effects.Add(new Effect("text", endX, effectY - 34, endX, effectY - 34, 18, Color.LightGray, "MISS"));
            }

            TryBeep(520 + slot * 130, 45);
        }

        private void StartStage10Cutscene()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // 1. 추천 포맷인 .wmv 경로를 1순위로 탐색합니다.
            string videoPath = Path.Combine(baseDir, "Assets", "UI", "Illegal_cutscene.wmv");

            // 2. 만약 아직 .mp4 파일만 있다면 mp4라도 서치하도록 예비 경로 보정
            if (!File.Exists(videoPath))
            {
                videoPath = Path.Combine(baseDir, "Assets", "UI", "Illegal_cutscene.mp4");
            }

            // 파일이 폴더에 실제로 존재할 때 진입
            if (File.Exists(videoPath))
            {
                //  mciSendString 명령은 장치를 여는 데 성공하면 정확히 '0'을 반환합니다.
                int openResult = mciSendString($"open \"{videoPath}\" type mpegvideo alias Stage10Video style child parent {this.Handle}", null, 0, IntPtr.Zero);

                // 코덱이 정상적으로 존재하여 영상을 여는 데 완벽히 성공했을 때만 컷씬 모드를 가동합니다.
                if (openResult == 0)
                {
                    mciSendString($"put Stage10Video window at 0 0 {ClientSize.Width} {ClientSize.Height}", null, 0, IntPtr.Zero);
                    mciSendString("play Stage10Video", null, 0, IntPtr.Zero);

                    cutsceneTicks = 480; // 8초 상영 타이머 가동
                    screen = ScreenMode.Cutscene;
                    return; // 정상 흐름 종료
                }
            }

            // ==========================================================
            // 파일이 없거나, 코덱이 미설치되어 비디오 오픈에 실패(openResult != 0)했다면
            // 화면을 굳기 만들지 않고 즉시 최종 보스 결전장으로 바로 강제 워프시킵니다
            // ==========================================================
            InitializeStage10BossFight();
        }

        // ==========================================================
        // 컷씬 종료 시 장치 디얼로케이션 마감 처리기
        // ==========================================================
        private void EndStage10Cutscene()
        {
            mciSendString("stop Stage10Video", null, 0, IntPtr.Zero);
            mciSendString("close Stage10Video", null, 0, IntPtr.Zero);

            // 비디오 하드웨어가 꺼졌으므로 실제 10스테이지 전투 필드를 엽니다.
            InitializeStage10BossFight();
        }

        // ==========================================================
        // 컷씬 이후 최종 보스전 정식 빌더
        // ==========================================================
        private void InitializeStage10BossFight()
        {
            currentStage = 10;
            StageInfo st = stages[9];

            enemies.Clear();
            effects.Clear();
            weaponDrops.Clear();
            draggedWeaponDrop = null;
            bossRuntime.Reset(currentStage);
            stage1BossPhase = false;

            player.Hp = player.MaxHp;
            player.Mp = player.MaxMp;
            player.SystemStability = 100;

            wCooldownTicks = 0; eCooldownTicks = 0; rCooldownTicks = 0;
            wBuffTicks = 0; playerShield = 0; eShieldDurationTicks = 0;
            playerSkySwords.Clear();

            player.X = 180;
            player.Y = ClientSize.Height - 118;
            player.TargetX = player.X;
            player.TargetY = player.Y;
            player.MoveVelocityX = 0f;
            player.MoveVelocityY = 0f;
            stageTime = 0;
            cameraX = 0;
            stageNpcHintClosed = true; // 최종전 몰입을 위해 도우미 대화 상자는 가려줍니다.
            firstDesktopNotice = false;

            stageBossPhase = true;
            screen = ScreenMode.Stage;

            enemies.Add(StageEnemyFactory.CreateBoss(st, Math.Max(760, ClientSize.Width - 360), ClientSize.Height, stages.Count));
            string bossText = $"STAGE 10 최종 결전 개시: {st.BossName}";
            effects.Add(new Effect("text", player.X + 290, player.Y - 120, player.X + 290, player.Y - 120, 100, Color.Red, bossText));

            TryBeep(760, 120);
        }

    }
}