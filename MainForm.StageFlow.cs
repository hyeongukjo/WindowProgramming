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

            // ==========================================================
            // 본체가 죽어 enemies 리스트에서 지워지더라도,
            // 분신 패턴이 실행 중이라면 이곳에서 강제로 제어권을 넘겨받아 3초 링크/타이머를 계속 연산시킵니다.
            // ==========================================================
            if (stageBossPhase && currentStage == 10 && bossRuntime.patternManager.IsIllusionActive)
            {
                GameEntity mainBoss = enemies.Find(e => e.IsBoss); // 없으면 null이 전달되며 안전 가드가 작동함
                bossRuntime.patternManager.Update(mainBoss, player, effects, mapWidth);
            }

            // ==========================================================
            // 본체(IsMainDead)와 분신(IsCloneDead)이 모두 죽고 패턴이 종료되었을 때만
            // 정확히 단 한 번 정식 최종 보상을 드랍하고 스테이지를 클리어시킵니다.
            // ==========================================================
            if (stageBossPhase && currentStage == 10 && bossRuntime.patternManager.IsMainDead && bossRuntime.patternManager.IsCloneDead && !bossRuntime.patternManager.IsIllusionActive)
            {
                if (weaponDrops.Count == 0) // 보상 파일 중복 생성 방지
                {
                    // 정식 보상 레이아웃 및 큐 관리 가동
                    GameEntity rewardDummy = new GameEntity { X = player.X + 150, Y = player.Y - 50, IsBoss = true };
                    RewardSystem.AwardDefeatReward(rewardDummy, player, currentStage, effects, random);

                    WeaponUpgradeFile drop = new WeaponUpgradeFile
                    {
                        X = rewardDummy.X,
                        Y = Math.Max(115, Math.Min(ClientSize.Height - 95, rewardDummy.Y - 25)),
                        StageIndex = currentStage,
                        UpgradeLevel = player.WeaponLevel + 1
                    };
                    weaponDrops.Add(drop);
                    effects.Add(new Effect("text", drop.X, drop.Y - 72, drop.X, drop.Y - 72, 120, Color.LightSkyBlue, "FINAL UPGRADE FILE DROP"));
                    TryBeep(980, 150);
                }

                if (weaponDrops.Count > 0) return; // 드랍된 파일을 드래그해서 먹을 때까지 맵 종료 유보 대기
                ClearCurrentStage();
                return;
            }

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

                // 맵에 소환된 적이 존재하고, 그중 살아있는 개체가 단 한 마리도 없을 때
                if (enemies.Count > 0 && !anyEnemyAlive)
                {
                    // 정산 팝업창 레이아웃 활성화
                    if (!showStageClearPopup)
                    {
                        showStageClearPopup = true;

                        popupBonusCoins = currentStage * 450;
                        player.Coins += popupBonusCoins;

                        TryBeep(1050, 200); // 클리어 알림음
                        return;
                    }

                    if (showStageClearPopup) return; // 확인 단추 클릭 전까지 프레임 잠금
                }
            }

            // 💡 10스테이지 최종보스전이 아닐 때 작동하는 기존 일반 몹 클리어 조건 분기
            if (enemyResult.AllEnemiesDefeated && enemies.Count > 0)
            {
                if (st.Index == 10 && bossRuntime.patternManager.IsIllusionActive) return; // 최종전 도중 날림 클리어 방지

                if (!stageBossPhase)
                {
                    if (st.Kind == StageKind.Normal)
                    {
                        if (weaponDrops.Count > 0) return;
                        ClearCurrentStage();
                        return;
                    }
                    StartStageBossPhase();
                    return;
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
        }

        // * [최종 디버깅]: 스테이지 전환 시 웨이브 인덱스 유실로 인한 1, 2웨이브 동시 시작 버그 해결
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

            currentWaveIndex = 0;  // 무조건 첫 번째 a 무리인 0번 인덱스로 강제 고정 리셋
            isWaveWaiting = false; // 5초 타이머 대기 플래그도 깨끗하게 클리어
            waveDelayTicks = 0;    // 대기 틱수 연산 완전 제로화

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

            if (st.IsBossStage)
            {
                stageBossPhase = true;
                enemies.Add(StageEnemyFactory.CreateBoss(st, Math.Max(760, ClientSize.Width - 360), ClientSize.Height, stages.Count));

                // 💡 [유지 가드]: 형진님의 요청에 따라 보스전 진입 시 금빛 인트로 선언 텍스트는 그대로 보존합니다.
                string bossText = $"STAGE {currentStage:00} 보스 레이드 개시: {st.BossName}";
                effects.Add(new Effect("text", player.X + 290, player.Y - 120, player.X + 290, player.Y - 120, 100, Color.Gold, bossText));
            }
            else
            {
                stageBossPhase = false;
                enemies.AddRange(StageEnemyFactory.CreateWaveEnemies(st, currentWaveIndex, ClientSize.Height));

                // 💡 [삭제 완료]: 일반 방에 들어설 때 나오던 "일반 데이터 정화 시작" 문구 사출 차단
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

            // 💡 [유지 가드]: 보스방 자동 진입 연출 문구 역시 보스 스테이지 규칙에 해당하므로 그대로 남겨둡니다.
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
            if (moving) range += 65f;

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
                if (m.Hp <= 0) continue; // 험악한 보스가 죽으면 본체 타격은 스킵

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

                    // 💡 [조기 드랍 봉쇄] 5번 최종 보스가 아닐 때만 즉시 보상을 드랍하도록 격리 필터 적용
                    if (m.Hp <= 0 && !m.Name.Contains("Binny") && !m.Name.Contains("Illegal_Binny"))
                    {
                        AwardDefeatReward(m);
                    }
                }
            }

            // ==========================================================
            // 💡 [무적 버그 해결] enemies 루프를 완전히 빠져나온 바깥 구역에 분신 타격 엔진 독립 매핑
            // ==========================================================
            if (bossRuntime.patternManager.IsIllusionActive && bossRuntime.patternManager.BinnyClone != null && !bossRuntime.patternManager.IsCloneDead)
            {
                // 분신의 실제 유영 위치를 바탕으로 타격용 가상 Bounds 레이아웃 실시간 계산
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
    }
}