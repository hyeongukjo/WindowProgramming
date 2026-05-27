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
        private void AdvanceIntro()
        {
            introIndex++;
            if (introIndex >= NpcDialogueData.IntroMessages.Length)
            {
                screen = ScreenMode.ProfileSetup;
            }
            TryBeep(880, 45);
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (screen == ScreenMode.StartMenu)
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space) StartNewGameFromAdminMenu();
                else if (e.KeyCode == Keys.C) ContinueFromAdminMenu();
                else if (e.KeyCode == Keys.Escape) Close();
                return;
            }

            if (screen == ScreenMode.Boot)
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space) screen = ScreenMode.AssistantIntro;
                return;
            }
            if (screen == ScreenMode.AssistantIntro)
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space || e.KeyCode == Keys.E) AdvanceIntro();
                return;
            }
            if (screen == ScreenMode.ProfileSetup)
            {
                if (e.KeyCode == Keys.Back && profileInput.Length > 0) profileInput = profileInput.Substring(0, profileInput.Length - 1);
                if (e.KeyCode == Keys.Enter) ConfirmProfile();
                return;
            }
            if (screen == ScreenMode.Desktop)
            {
                if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Down) selectedStage = Math.Min(unlockedStage, selectedStage + 1);
                if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Up) selectedStage = Math.Max(1, selectedStage - 1);
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.E) StartStage(selectedStage);
                if (e.KeyCode == Keys.B || e.KeyCode == Keys.Delete) screen = ScreenMode.Shop;
                if (e.KeyCode == Keys.F1) screen = ScreenMode.Help;
                return;
            }
            if (screen == ScreenMode.Shop)
            {
                if (e.KeyCode == Keys.D1 || e.KeyCode == Keys.NumPad1) selectedShopItem = "hp";
                if (e.KeyCode == Keys.D2 || e.KeyCode == Keys.NumPad2) selectedShopItem = "mp";
                if (e.KeyCode == Keys.D3 || e.KeyCode == Keys.NumPad3) selectedShopItem = "bundle";

                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
                    BuyShopItem(selectedShopItem);

                if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.B)
                    screen = ScreenMode.Desktop;

                return;
            }
            if (screen == ScreenMode.Stage)
            {
                if (e.KeyCode == Keys.Left) MovePlayerBy(-160, 0);
                else if (e.KeyCode == Keys.Right) MovePlayerBy(160, 0);
                else if (e.KeyCode == Keys.Up) MovePlayerBy(0, -120);
                else if (e.KeyCode == Keys.Down) MovePlayerBy(0, 120);
                else if (e.KeyCode == Keys.Q)
                {
                    PlayerMovementSystem.StartSkillAnimation(player, 0);
                    CastSkill(0);
                }
                else if (e.KeyCode == Keys.W)
                {
                    PlayerMovementSystem.StartSkillAnimation(player, 1);
                    CastSkill(1);
                }
                else if (e.KeyCode == Keys.E)
                {
                    PlayerMovementSystem.StartSkillAnimation(player, 2);
                    CastSkill(2);
                }
                else if (e.KeyCode == Keys.R)
                {
                    CastSkill(3);
                }
                else if (e.KeyCode == Keys.D) UseHpPotion();
                else if (e.KeyCode == Keys.F) UseMpPotion();
                else if (e.KeyCode == Keys.Space) { effects.Add(new Effect("spark", player.X, player.Y - 44, player.X, player.Y - 44, 28, Color.FromArgb(120, 200, 255), "")); TryBeep(420, 40); }
                else if (e.KeyCode == Keys.Escape) screen = ScreenMode.Desktop;
                return;
            }
            if (screen == ScreenMode.StageClearDialog)
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space || e.KeyCode == Keys.E) ContinueAfterClear();
                return;
            }
            if (screen == ScreenMode.FinalInput)
            {
                if (e.KeyCode == Keys.Back && finalInput.Length > 0) finalInput = finalInput.Substring(0, finalInput.Length - 1);
                if (e.KeyCode == Keys.Enter) ResolveEnding();
                return;
            }
            if (screen == ScreenMode.Help)
            {
                if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Enter) screen = ScreenMode.Desktop;
            }
        }

        private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (screen == ScreenMode.ProfileSetup)
            {
                if (!char.IsControl(e.KeyChar) && profileInput.Length < 16)
                {
                    profileInput += e.KeyChar;
                    e.Handled = true;
                }
            }
            else if (screen == ScreenMode.FinalInput)
            {
                if (!char.IsControl(e.KeyChar) && finalInput.Length < 32)
                {
                    finalInput += e.KeyChar;
                    e.Handled = true;
                }
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            Point mousePos = e.Location;

            // ==========================================================
            //  첫 화면(Admin 시작 메뉴) 투명 버튼 좌표 클릭 판정 
            // ==========================================================
            if (screen == ScreenMode.StartMenu)
            {
                for (int i = 0; i < buttons.Count; i++)
                {
                    var btn = buttons[i];
                    if (btn.Bounds.Contains(mousePos))
                    {
                        if (btn.Action == "adminStart") StartNewGameFromAdminMenu();
                        else if (btn.Action == "adminContinue") ContinueFromAdminMenu();
                        else if (btn.Action == "adminExit") Close();
                        return;
                    }
                }
            }

            // ==========================================================
            // 스테이지 클리어 정산 팝업창 '확인' 버튼 클릭 제어 필터
            // ==========================================================
            if (screen == ScreenMode.Stage && showStageClearPopup)
            {
                // 화면에 뜬 팝업창 확인 버튼 영역을 마우스로 정확히 눌렀다면
                if (popupConfirmBtnBounds.Contains(mousePos))
                {
                    showStageClearPopup = false; // 팝업창을 닫고 장벽 해제
                    ClearCurrentStage();         // 안전하게 정식 NPC 대화 시퀀스로 이관
                    TryBeep(880, 70);            // 딸깍 클릭음 피드백
                    return;
                }

                // 팝업창이 활성화되어 있는 동안에는 다른 빈 땅을 눌러도 무반응 처리 
                return;
            }
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                if (buttons[i].Bounds.Contains(e.Location))
                {
                    HandleAction(buttons[i].Action);
                    return;
                }
            }
            if (screen == ScreenMode.Stage)
            {
                if (e.Button == MouseButtons.Left)
                {
                    WeaponUpgradeFile drop = FindWeaponDropAt(e.Location);
                    if (drop != null)
                    {
                        draggedWeaponDrop = drop;
                        draggedWeaponDrop.Dragging = true;
                        return;
                    }
                }
                if (stageBossPhase && bossRuntime.HandleClick(e.Location))
                {
                    return;
                }
                if (e.Button == MouseButtons.Right)
                {
                    int mapWidth = GetStageMapWidth(stages[currentStage - 1]);
                    player.TargetX = Math.Max(80, Math.Min(mapWidth - 80, cameraX + e.X));
                    player.TargetY = Math.Max(118, Math.Min(ClientSize.Height - 78, e.Y));
                    player.Facing = player.TargetX >= player.X ? 1 : -1;
                }
            }
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (draggedWeaponDrop == null) return;
            draggedWeaponDrop.X = cameraX + e.X;
            draggedWeaponDrop.Y = Math.Max(70, Math.Min(ClientSize.Height - 70, e.Y));
            Invalidate();
        }

        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (draggedWeaponDrop == null) return;
            WeaponUpgradeFile drop = draggedWeaponDrop;
            draggedWeaponDrop = null;
            drop.Dragging = false;

            if (drop.Bounds.IntersectsWith(player.Bounds))
            {
                ApplyWeaponUpgrade(drop);
                weaponDrops.Remove(drop);
            }
        }

        private WeaponUpgradeFile FindWeaponDropAt(Point location)
        {
            for (int i = weaponDrops.Count - 1; i >= 0; i--)
            {
                WeaponUpgradeFile drop = weaponDrops[i];
                RectangleF screenBounds = new RectangleF(drop.Bounds.X - cameraX, drop.Bounds.Y, drop.Bounds.Width, drop.Bounds.Height);
                if (screenBounds.Contains(location)) return drop;
            }
            return null;
        }

        private void MovePlayerBy(float dx, float dy)
        {
            if (currentStage <= 0) return;
            int mapWidth = GetStageMapWidth(stages[currentStage - 1]);
            player.TargetX = Math.Max(80, Math.Min(mapWidth - 80, player.TargetX + dx));
            player.TargetY = Math.Max(118, Math.Min(ClientSize.Height - 78, player.TargetY + dy));
            if (Math.Abs(dx) > 0.1f) player.Facing = dx < 0 ? -1 : 1;
        }

        private void HandleAction(string action)
        {
            if (action == "introNext") AdvanceIntro();
            else if (action == "desktopNoticeOk" || action == "desktopNoticeClose") { firstDesktopNotice = false; }
            else if (action == "npcHintClose") AdvanceStageNpcHint();
            else if (action == "profileOk") ConfirmProfile();
            else if (action == "openShop") screen = ScreenMode.Shop;
            else if (action == "shopBack") screen = ScreenMode.Desktop;
            else if (action == "selecthp") selectedShopItem = "hp";
            else if (action == "selectmp") selectedShopItem = "mp";
            else if (action == "selectbundle") selectedShopItem = "bundle";
            else if (action == "confirmShopPurchase") BuyShopItem(selectedShopItem);
            else if (action.StartsWith("buy")) BuyShopItem(action.Substring(3).ToLowerInvariant());
            else if (action.StartsWith("stage"))
            {
                int n;
                if (int.TryParse(action.Substring(5), out n))
                {
                    selectedStage = n;
                    StartStage(n);
                }
            }
            else if (action == "clearNext") ContinueAfterClear();
            else if (action == "finalOk") ResolveEnding();
            else if (action == "helpBack") screen = ScreenMode.Desktop;
        }
        private void AdvanceStageNpcHint()
        {
            if (currentStage <= 0)
            {
                stageNpcHintClosed = true;
                return;
            }

            int count = NpcDialogueData.GetStageDialogCount(currentStage);

            stageNpcHintIndex++;

            if (stageNpcHintIndex >= count)
            {
                stageNpcHintClosed = true;
            }
            else
            {
                stageNpcHintClosed = false;
            }
        }

        private void ConfirmProfile()
        {
            if (string.IsNullOrWhiteSpace(profileInput))
            {
                effects.Add(new Effect("text", ClientSize.Width / 2, 250, ClientSize.Width / 2, 250, 50, Color.Red, NpcDialogueData.ProfileNameRequired));
                TryBeep(320, 80);
                return;
            }
            player.ProfileName = profileInput.Trim();
            screen = ScreenMode.Desktop;
            TryBeep(920, 60);
        }


        private void BuyShopItem(string item)
        {
            int cost = 0;
            string label = "";
            if (item == "hp") { cost = 30; label = "HP 포션 +1"; }
            else if (item == "mp") { cost = 25; label = "MP 포션 +1"; }
            else { cost = 90; label = "포션 묶음 +2/+2"; item = "bundle"; }

            if (player.Coins < cost)
            {
                effects.Add(new Effect("text", ClientSize.Width / 2, 210, ClientSize.Width / 2, 210, 50, Color.OrangeRed, "코인 부족"));
                TryBeep(280, 90);
                return;
            }
            player.Coins -= cost;
            if (item == "hp") player.HpPotions++;
            else if (item == "mp") player.MpPotions++;
            else { player.HpPotions += 2; player.MpPotions += 2; }
            effects.Add(new Effect("text", ClientSize.Width / 2, 210, ClientSize.Width / 2, 210, 50, Color.Gold, label));
            TryBeep(820, 70);
        }

        private void ContinueAfterClear()
        {
            if (clearStage >= stages.Count)
            {
                screen = ScreenMode.FinalInput;
                return;
            }
            selectedStage = Math.Min(unlockedStage, clearStage + 1);
            screen = ScreenMode.Desktop;
        }

        private void ResolveEnding()
        {
            NpcEndingText ending = NpcDialogueData.ResolveEnding(finalInput, player.ProfileName);
            endingTitle = ending.Title;
            endingBody = ending.Body;
            screen = ScreenMode.Ending;
        }

    }
}
