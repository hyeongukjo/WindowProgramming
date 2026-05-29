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
            if (playerDeathSequenceActive) return;
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
                if (e.KeyCode == Keys.Back && profileInput.Length > 0)
                {
                    profileInput = profileInput.Substring(0, profileInput.Length - 1);
                    return;
                }

                if (e.KeyCode == Keys.Enter)
                {
                    ConfirmProfile();
                    ignoreEnterUntilKeyUp = true;
                    return;
                }

                return;
            }
            if (ignoreEnterUntilKeyUp && e.KeyCode == Keys.Enter)
            {
                return;
            }
            if (screen == ScreenMode.Desktop)
            {
                if (firstDesktopNotice)
                {
                    if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space || e.KeyCode == Keys.E)
                    {
                        firstDesktopNotice = false;
                    }

                    return;
                }
                if (showOldGoogleWindow)
                {
                    if (e.KeyCode == Keys.Escape)
                    {
                        CloseOldGoogleWindow();
                        return;
                    }

                    if (e.KeyCode == Keys.Back && oldGoogleSearchFocused && oldGoogleSearchText.Length > 0)
                    {
                        oldGoogleSearchText = oldGoogleSearchText.Substring(0, oldGoogleSearchText.Length - 1);
                        Invalidate();
                        return;
                    }

                    if (e.KeyCode == Keys.Enter)
                    {
                        SubmitOldGoogleSearch();
                        return;
                    }

                    return;
                }
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
                if (IsStageNpcHintOpen() && (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space))
                {
                    AdvanceStageNpcHint();
                    return;
                }
                if (e.KeyCode == Keys.Left) MovePlayerBy(-160, 0);
                else if (e.KeyCode == Keys.Right) MovePlayerBy(160, 0);
                else if (e.KeyCode == Keys.Up) MovePlayerBy(0, -120);
                else if (e.KeyCode == Keys.Down) MovePlayerBy(0, 120);


                // [단축키 매핑] QWER 스킬 라우터 배치
                if (e.KeyCode == Keys.Q) CastSkill(0); // Q: 기본 공격 (기존 평타 슬롯 0번 가동 및 현상유지)
                if (e.KeyCode == Keys.W) CastPlayerSkillW();
                if (e.KeyCode == Keys.E) CastPlayerSkillE();
                if (e.KeyCode == Keys.R) CastPlayerSkillR();


                else if (e.KeyCode == Keys.D) UseHpPotion();
                else if (e.KeyCode == Keys.F) UseMpPotion();
                else if (e.KeyCode == Keys.Space) { effects.Add(new Effect("spark", player.X, player.Y - 44, player.X, player.Y - 44, 28, Color.FromArgb(120, 200, 255), "")); TryBeep(420, 40); }
                else if (e.KeyCode == Keys.Escape)
                {
                    currentStage = 0;
                    enemies.Clear();       // 활성화된 몬스터 객체 풀 즉시 해제
                    effects.Clear();       // 화면에 남아돌던 공격/글자 이펙트 풀 청소
                    weaponDrops.Clear();   // 드롭된 잔여 아이템 데이터 소멸
                    stageBossPhase = false;
                    stage1BossPhase = false;

                    // 10스테이지 최종보스 분신 기믹 상태값 완전 초기화
                    if (bossRuntime?.patternManager != null)
                    {
                        bossRuntime.patternManager.IsIllusionActive = false;
                        bossRuntime.patternManager.BinnyClone = null;
                    }

                    screen = ScreenMode.Desktop; // 깨끗해진 상태로 바탕화면 복귀
                }
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
            if (screen == ScreenMode.Desktop && showOldGoogleWindow && oldGoogleSearchFocused)
            {
                if (!char.IsControl(e.KeyChar) && oldGoogleSearchText.Length < 48)
                {
                    oldGoogleSearchText += e.KeyChar;
                    e.Handled = true;
                    Invalidate();
                }

                return;
            }
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
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e.KeyCode == Keys.Enter)
            {
                ignoreEnterUntilKeyUp = false;
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (playerDeathSequenceActive) return;
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
            if (playerDeathSequenceActive) return;

            // ==========================================================
            // 리더보드 창이 열려 있을 때
            // - 리더보드 X 버튼만 허용
            // - 그 외 클릭은 전부 막음
            // ==========================================================
            if (screen == ScreenMode.Desktop && showLeaderboardWindow)
            {
                Point mousePos = e.Location;

                for (int i = 0; i < buttons.Count; i++)
                {
                    if (buttons[i].Action == leaderboardCloseKey &&
                        buttons[i].Bounds.Contains(mousePos))
                    {
                        showLeaderboardWindow = false;
                        TryBeep(750, 50);
                        Invalidate();
                        return;
                    }
                }

                return;
            }

            // ==========================================================
            // Old Google 창이 열려 있을 때
            // - Google 창의 X 버튼만 허용
            // - Google 검색창 클릭만 허용
            // - 뒤에 있는 Stage 버튼 / 상점 버튼 / 바탕화면 아이콘 클릭은 전부 막음
            // ==========================================================
            if (screen == ScreenMode.Desktop && IsOldGoogleWindowVisible())
            {
                for (int i = 0; i < buttons.Count; i++)
                {
                    if (!buttons[i].Bounds.Contains(e.Location))
                        continue;

                    if (buttons[i].Action == OldGoogleCloseKey ||
                        buttons[i].Action == OldGoogleSearchFocusKey)
                    {
                        HandleAction(buttons[i].Action);
                        return;
                    }
                }

                return;
            }

            // ==========================================================
            // 일반 버튼 클릭 처리
            // - Old Google 창이 닫혀 있을 때만 실행됨
            // ==========================================================
            for (int i = 0; i < buttons.Count; i++)
            {
                if (buttons[i].Bounds.Contains(e.Location))
                {
                    HandleAction(buttons[i].Action);
                    return;
                }
            }

            // ==========================================================
            // 바탕화면 아이콘 클릭 처리
            // ==========================================================
            if (screen == ScreenMode.Desktop)
            {
                Point mousePos = e.Location;

                // 최초 진입 안내창이 떠 있으면 바탕화면 클릭 무시
                if (firstDesktopNotice)
                {
                    return;
                }

                // Internet Explorer 아이콘 클릭
                if (GetInternetExplorerIconBounds().Contains(mousePos))
                {
                    OpenOldGoogleWindow();
                    return;
                }

                // 내 컴퓨터 아이콘 클릭
                Rectangle myComputerIconBounds = new Rectangle(15, 15, 95, 95);
                if (myComputerIconBounds.Contains(mousePos))
                {
                    showLeaderboardWindow = true;
                    UpdateAllBossRankings();
                    TryBeep(880, 60);
                    Invalidate();
                    return;
                }
            }

            // ==========================================================
            // 스테이지 화면 클릭 처리
            // ==========================================================
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

            else if (action == OldGoogleCloseKey)
            {
                CloseOldGoogleWindow();
            }
            else if (action == OldGoogleSearchFocusKey)
            {
                oldGoogleSearchFocused = true;
                if (!string.IsNullOrWhiteSpace(oldGoogleLastQuery))
                {
                    oldGoogleSearchText = "";
                }
                Invalidate();
            }

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
            UpdateAllBossRankings();
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

        // ==========================================================
        // [W 스킬] 오버클럭 버프 가동 (5초간 가속 및 데미지 증가)
        // ==========================================================
        private void CastPlayerSkillW()
        {
            // 스킬 해금 조건 검증 검사
            if (player.ClearedStages < 2) { effects.Add(new Effect("text", player.X, player.Y - 90, player.X, player.Y - 90, 35, Color.Red, "W 스킬 미해금 (Stage 02 클리어 필요)")); TryBeep(320, 70); return; }

            if (wCooldownTicks > 0) { effects.Add(new Effect("text", player.X, player.Y - 90, player.X, player.Y - 90, 30, Color.Red, $"W 쿨타임 대기 중 ({(wCooldownTicks / 60.0f):0.0}초)")); TryBeep(320, 50); return; }
            if (player.Mp < 15) { effects.Add(new Effect("text", player.X, player.Y - 90, player.X, player.Y - 90, 35, Color.DeepSkyBlue, "MP 부족")); return; }

            player.Mp -= 15;
            wBuffTicks = 300;      // 5초 지속
            wCooldownTicks = 900;  // 15초 쿨타임 인젝션 (15 * 60 = 900틱)

            effects.Add(new Effect("spark", player.X, player.Y - 40, player.X, player.Y - 40, 40, Color.Orange, ""));
            effects.Add(new Effect("text", player.X, player.Y - 100, player.X, player.Y - 100, 50, Color.Orange, "OVERCLOCK (SPEED & DAMAGE UP)"));
            TryBeep(650, 100);
        }

        // ==========================================================
        // [E 스킬] 방화벽 보증막 가동 (최대 체력의 50% 실드 부여)
        // ==========================================================
        private void CastPlayerSkillE()
        {
            // 스킬 해금 조건 검증 검사
            if (player.ClearedStages < 5) { effects.Add(new Effect("text", player.X, player.Y - 90, player.X, player.Y - 90, 35, Color.Red, "E 스킬 미해금 (Stage 05 클리어 필요)")); TryBeep(320, 70); return; }

            if (eCooldownTicks > 0) { effects.Add(new Effect("text", player.X, player.Y - 90, player.X, player.Y - 90, 30, Color.Red, $"E 쿨타임 대기 중 ({(eCooldownTicks / 60.0f):0.0}초)")); TryBeep(320, 50); return; }
            if (player.Mp < 25) { effects.Add(new Effect("text", player.X, player.Y - 90, player.X, player.Y - 90, 35, Color.DeepSkyBlue, "MP 부족")); return; }

            player.Mp -= 25;

            // 밸런스 조정: 최대 체력의 정확히 20% 가산
            playerShield = (int)(player.MaxHp * 0.20f);
            eShieldDurationTicks = 300; //  5초 지속시간 타이머 주입 (60fps * 5 = 300틱)
            eCooldownTicks = 1200;      // 총 쿨타임 20초 주입 (20 * 60 = 1200틱)

            effects.Add(new Effect("spark", player.X, player.Y - 40, player.X, player.Y - 40, 45, Color.Cyan, ""));
            effects.Add(new Effect("text", player.X, player.Y - 100, player.X, player.Y - 100, 50, Color.Cyan, $"FIREWALL SHIELD (+{playerShield})"));
            TryBeep(800, 120);
        }

        // ==========================================================
        // [R 스킬] 시스템 콜: 1단계 빙결 장검 소환 (전방 낙하 폭격)
        // ==========================================================
        private void CastPlayerSkillR()
        {
            // 스킬 해금 조건 검증 검사
            if (player.ClearedStages < 8) { effects.Add(new Effect("text", player.X, player.Y - 90, player.X, player.Y - 90, 35, Color.Red, "R 스킬 미해금 (Stage 08 클리어 필요)")); TryBeep(320, 70); return; }

            if (rCooldownTicks > 0) { effects.Add(new Effect("text", player.X, player.Y - 90, player.X, player.Y - 90, 30, Color.Red, $"R 궁극기 쿨타임 대기 중 ({(rCooldownTicks / 60.0f):0.0}초)")); TryBeep(320, 50); return; }
            if (player.Mp < 45) { effects.Add(new Effect("text", player.X, player.Y - 90, player.X, player.Y - 90, 35, Color.DeepSkyBlue, "MP 부족")); return; }

            player.Mp -= 45;
            rCooldownTicks = 1500; // 25초 쿨타임 인젝션 (25 * 60 = 1500틱)

            PlayerMovementSystem.StartSkillAnimation(player, 3);

            float targetX = player.X + (player.Facing == 0 ? 250f : player.Facing * 250f);

            playerSkySwords.Add(new PlayerSkySword
            {
                X = targetX,
                Y = player.Y,
                Timer = 23,        // ⚡ [속도 가속] 기존 35틱에서 23틱으로 단축 (1.5배 고속 하강 연산)
                MaxTimer = 23,
                SwordType = "cold"
            });

            effects.Add(new Effect("text", player.X, player.Y - 120, player.X, player.Y - 120, 60, Color.DodgerBlue, "SYSTEM_CALL: COLD_LONGSWORD"));
            TryBeep(350, 200);
        }

    }
}
