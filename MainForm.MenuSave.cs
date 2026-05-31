using System;
using System.Drawing;
using System.Windows.Forms;

namespace DebugHeroFileDungeonRPG
{
    public sealed partial class MainForm
    {
        private void StartNewGameFromAdminMenu()
        {
            GameSaveSystem.DeleteSave();
            ResetForNewGame();
            bootTicks = 0;
            introIndex = 0;
            screen = ScreenMode.Boot;
            TryBeep(720, 70);
        }

        private void ContinueFromAdminMenu()
        {
            GameSaveData data;
            if (!GameSaveSystem.TryLoad(out data))
            {
                TryBeep(300, 90);
                return;
            }

            ResetForNewGame();
            player.ProfileName = string.IsNullOrWhiteSpace(data.ProfileName) ? "admin" : data.ProfileName;
            profileInput = player.ProfileName;
            unlockedStage = Math.Max(1, Math.Min(stages.Count, data.UnlockedStage));
            selectedStage = Math.Max(1, Math.Min(unlockedStage, data.SelectedStage));
            player.ClearedStages = Math.Max(0, Math.Min(stages.Count, data.ClearedStages));
            player.Level = Math.Max(1, data.Level);
            player.WeaponLevel = Math.Max(1, data.WeaponLevel);
            player.Exp = Math.Max(0, data.Exp);
            player.Coins = Math.Max(0, data.Coins);
            player.HpPotions = Math.Max(0, data.HpPotions);
            player.MpPotions = Math.Max(0, data.MpPotions);
            player.MaxHp = Math.Max(1, data.MaxHp);
            player.MaxMp = Math.Max(1, data.MaxMp);
            player.Hp = player.MaxHp;
            player.Mp = player.MaxMp;
            player.RelationLog = Math.Max(0, data.RelationLog);
            player.QuarantinedBosses = Math.Max(0, data.QuarantinedBosses);
            player.ProfileTruthScore = Math.Max(0, data.ProfileTruthScore);
            firstDesktopNotice = false;
            screen = ScreenMode.Desktop;
            TryBeep(860, 70);
        }

        private void SaveCurrentGame()
        {
            if (screen == ScreenMode.StartMenu || screen == ScreenMode.Boot) return;
            GameSaveData data = new GameSaveData();
            data.ProfileName = player.ProfileName;
            data.UnlockedStage = Math.Max(1, Math.Min(stages.Count, unlockedStage));
            data.SelectedStage = Math.Max(1, Math.Min(stages.Count, selectedStage));
            data.ClearedStages = player.ClearedStages;
            data.CurrentStage = currentStage;
            data.Level = player.Level;
            data.WeaponLevel = player.WeaponLevel;
            data.Exp = player.Exp;
            data.Coins = player.Coins;
            data.HpPotions = player.HpPotions;
            data.MpPotions = player.MpPotions;
            data.MaxHp = player.MaxHp;
            data.MaxMp = player.MaxMp;
            data.RelationLog = player.RelationLog;
            data.QuarantinedBosses = player.QuarantinedBosses;
            data.ProfileTruthScore = player.ProfileTruthScore;
            GameSaveSystem.Save(data);
        }

        private void HandlePermanentDeath()
        {
            GameSaveSystem.DeleteSave();
            player.Hp = player.MaxHp;
            player.Mp = player.MaxMp;

            effects.Clear();
            enemies.Clear();
            weaponDrops.Clear();
            draggedWeaponDrop = null;
            currentStage = 0;
            stageBossPhase = false;
            stage1BossPhase = false;
            clearStage = 0;

            // 데이터 유실 없이 현재 자산을 그대로 하드 세이브
            SaveCurrentGame();
            screen = ScreenMode.Desktop;

            TryBeep(600, 150);
            MessageBox.Show("복구 프로세스가 종료되었습니다. 진행 상황(코인, 포션, 스테이지)이 안전하게 보존되어 바탕화면으로 사출됩니다.", "SYSTEM RESTORE", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ResetForNewGame()
        {
            profileInput = "";
            finalInput = "";
           
            unlockedStage = 1; // 기존의 1(또는 0)에서 10으로 수정!
            selectedStage = 1;  // 커서는 1스테이지에 위치시킵니다.
          
            selectedShopItem = "hp";
            currentStage = 0;
            clearStage = 0;
            cameraX = 0;
            stageTime = 0;
            endingTitle = "";
            endingBody = "";
            firstDesktopNotice = true;
            profileTutorialOpen = false;
            profileTutorialIndex = 0;
            ignoreEnterUntilKeyUp = false;
            stageNpcHintClosed = false;
            stageBossPhase = false;
            stage1BossPhase = false;
            lastClearWasBoss = false;
            introIndex = 0;
            bootTicks = 0;
            effects.Clear();
            enemies.Clear();
            weaponDrops.Clear();
            draggedWeaponDrop = null;
            bossRuntime.Reset(0);

            player.ProfileName = "";
            player.ProgramName = "Recovery Program";
            player.X = 180;
            player.Y = Math.Max(360, ClientSize.Height - 118);
            player.TargetX = player.X;
            player.TargetY = player.Y;
            player.MoveVelocityX = 0;
            player.MoveVelocityY = 0;
            player.WalkCycle = 0;
            player.LastMoveTicks = 0;
            player.DefenseTicks = 0;
            player.MaxHp = 100;
            player.Hp = 100;
            player.MaxMp = 100;
            player.Mp = 100;
            player.SystemStability = 100;
            player.CpuLoad = 15;
            player.Coins = 0;
            player.HpPotions = 3;
            player.MpPotions = 2;
            player.Level = 1;
            player.WeaponLevel = 1;
            player.Exp = 0;
            player.Facing = 1;
            player.Direction = 0;
            player.ActionState = PlayerActionState.Idle;
            player.ActionFrame = 0;
            player.ActionTick = 0;
            player.SkillIndex = -1;
            player.RelationLog = 0;
            player.ClearedStages = 0;
            player.QuarantinedBosses = 0;
            player.ProfileTruthScore = 0;
            player.InvincibleTicks = 0;
            player.StunTicks = 0;
        }
    }
}
