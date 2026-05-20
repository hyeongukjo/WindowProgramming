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
        private int GetStageMapWidth(StageInfo st)
        {
            return StageFlowRules.GetStageMapWidth(st, stageBossPhase, ClientSize.Width);
        }

        private void UpdateStage()
        {
            stageTime++;
            StageInfo st = stages[currentStage - 1];
            int mapWidth = GetStageMapWidth(st);
            PlayerMovementSystem.Update(player, st, stageBossPhase, ClientSize.Width, ClientSize.Height, mapWidth, ref cameraX, tick);
            PlayerMovementSystem.UpdateActionAnimation(player);
            EnemyUpdateResult enemyResult = EnemyLogicSystem.Update(enemies, player, st, currentStage, stageBossPhase, tick, mapWidth, ClientRectangle, bossRuntime, effects);
            if (enemyResult.PlayerReturnedToStart)
            {
                player.X = 180;
                player.TargetX = 180;
                player.TargetY = player.Y;
                player.MoveVelocityX = 0f;
                player.MoveVelocityY = 0f;
                player.WalkCycle = 0f;
                effects.Add(new Effect("text", player.X, player.Y - 90, player.X, player.Y - 90, 70, Color.Red, "복구 지점으로 반환"));
            }
            if (enemyResult.AllEnemiesDefeated && enemies.Count > 0)
            {
                if (!stageBossPhase)
                {
                    StartStageBossPhase();
                    return;
                }
                if (weaponDrops.Count > 0) return;
                ClearCurrentStage();
            }
        }

        private void ClearCurrentStage()
        {
            clearStage = currentStage;
            StageInfo st = stages[clearStage - 1];
            lastClearWasBoss = stageBossPhase || st.IsBossStage;
            player.ClearedStages = Math.Max(player.ClearedStages, clearStage);
            player.Level++;
            player.Exp += 50 + st.Index * 20;
            player.Hp = Math.Min(player.MaxHp + 12, player.Hp + 35);
            player.Mp = Math.Min(player.MaxMp + 8, player.Mp + 22);
            player.MaxHp += 4;
            player.MaxMp += 2;
            player.SystemStability = Math.Min(100, player.SystemStability + 6);
            if (lastClearWasBoss) player.QuarantinedBosses++;
            if (clearStage < stages.Count) unlockedStage = Math.Max(unlockedStage, clearStage + 1);
            if (st.Index >= 7) player.ProfileTruthScore++;
            currentStage = 0;
            enemies.Clear();
            screen = ScreenMode.StageClearDialog;
            TryBeep(720, 90);
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
            stageBossPhase = false;
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

            enemies.AddRange(StageEnemyFactory.CreatePreBossEnemies(st, ClientSize.Height, random));
            effects.Add(new Effect("text", player.X + 220, player.Y - 110, player.X + 220, player.Y - 110, 80, Color.FromArgb(220, 255, 255), "몬스터 정리 후 보스방 자동 진입"));
            firstDesktopNotice = false;
            screen = ScreenMode.Stage;
            TryBeep(600, 80);
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

        private void AwardDefeatReward(GameEntity m)
        {
            RewardSystem.AwardDefeatReward(m, player, currentStage, effects, random);
            if (m != null && m.IsBoss) DropWeaponUpgradeFile(m);
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

        private void ApplyWeaponUpgrade(WeaponUpgradeFile drop)
        {
            player.WeaponLevel = Math.Max(player.WeaponLevel + 1, drop.UpgradeLevel);
            effects.Add(new Effect("spark", player.X, player.Y - 46, player.X, player.Y - 46, 70, Color.DeepSkyBlue, ""));
            effects.Add(new Effect("text", player.X, player.Y - 104, player.X, player.Y - 104, 80, Color.Gold, "WEAPON +" + player.WeaponLevel));
            TryBeep(980, 90);
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
            int heal = Math.Min(player.MaxHp - player.Hp, 45 + player.Level * 10);
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
            int restore = Math.Min(player.MaxMp - player.Mp, 36 + player.Level * 8);
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
                    m.Hp -= damage;
                    m.HitFlash = 10;
                    hitAny = true;
                    effects.Add(new Effect("text", m.X, m.Y - 84, m.X, m.Y - 84, 34, Color.Yellow, damage.ToString()));
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
