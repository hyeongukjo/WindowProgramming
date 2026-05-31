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
                if (profileTutorialOpen)
                {
                    if (ignoreEnterUntilKeyUp && e.KeyCode == Keys.Enter)
                    {
                        return;
                    }

                    if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space || e.KeyCode == Keys.E)
                    {
                        AdvanceProfileTutorial();
                    }

                    return;
                }

                if (e.KeyCode == Keys.Back && profileInput.Length > 0)
                {
                    profileInput = profileInput.Substring(0, profileInput.Length - 1);
                    Invalidate();
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
                if (e.KeyCode == Keys.Back && finalInput.Length > 0)
                {
                    finalInput = finalInput.Substring(0, finalInput.Length - 1);
                    Invalidate();
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    ResolveEnding();
                }

                return;
            }

            if (screen == ScreenMode.Ending)
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape || e.KeyCode == Keys.Space)
                {
                    endingTitle = "";
                    endingBody = "";
                    finalInput = "";

                    currentStage = 0;
                    screen = ScreenMode.StartMenu;

                    Invalidate();
                }

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
                if (profileTutorialOpen)
                {
                    e.Handled = true;
                    return;
                }

                if (!char.IsControl(e.KeyChar) && profileInput.Length < 16)
                {
                    profileInput += e.KeyChar;
                    e.Handled = true;
                    Invalidate();
                }

                return;
            }
            else if (screen == ScreenMode.FinalInput)
            {
                if (!char.IsControl(e.KeyChar) && finalInput.Length < 32)
                {
                    finalInput += e.KeyChar;
                    e.Handled = true;
                    Invalidate();
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
            Point mousePos = e.Location;

            if (screen == ScreenMode.Desktop && showStartMenuPopup)
            {
                Rectangle menuBounds = GetStartMenuPopupBounds();

                // 시작 메뉴 내부 버튼 컬렉션들 체크 순회
                for (int i = 0; i < buttons.Count; i++)
                {
                    if (buttons[i].Bounds.Contains(mousePos))
                    {
                        if (buttons[i].Action == startMenuSaveKey ||
                            buttons[i].Action == startMenuHelpKey ||
                            buttons[i].Action == startMenuExitKey)
                        {
                            HandleAction(buttons[i].Action);
                            showStartMenuPopup = false; // 실행 후 메뉴판은 자동으로 스르륵 클로즈
                            Invalidate(true);
                            return;
                        }
                    }
                }

                // 만약 시작 메뉴가 열린 상태에서 메뉴 영역 외의 딴 곳을 누르면 메뉴판 닫기 처리
                if (!menuBounds.Contains(mousePos))
                {
                    showStartMenuPopup = false;
                    TryBeep(650, 40);
                    Invalidate(true);
                }
            }




            if (screen == ScreenMode.Desktop && showMonsterBookWindow)
            {
                // 수동 사치 계산을 버리고, OnPaint에서 정밀 캡처된 투명 X 버튼 컬렉션을 루프 검증합니다.
                for (int i = 0; i < buttons.Count; i++)
                {
                    if (buttons[i].Action == monsterBookCloseKey && buttons[i].Bounds.Contains(mousePos))
                    {
                        showMonsterBookWindow = false; // 도감 격리 종료 (바탕화면 복귀)
                        TryBeep(720, 60);
                        Invalidate();
                        return;
                    }
                }
                return; // 창 내부의 엉뚱한 빈 공간이나 뒤쪽 배경 아이콘 클릭을 철통 가드 차단
            }

            if (screen == ScreenMode.Desktop && showTrashCanWindow)
            {
                for (int i = 0; i < buttons.Count; i++)
                {
                    if (buttons[i].Bounds.Contains(mousePos))
                    {
                        if (buttons[i].Action == trashCanCloseKey)
                        {
                            showTrashCanWindow = false; // 휴지통 클로즈
                            TryBeep(720, 60);
                            Invalidate();
                            return;
                        }
                        else if (buttons[i].Action.StartsWith("delete_trash_"))
                        {
                            int index = int.Parse(buttons[i].Action.Substring(13));
                            ExecuteTrashDelete(index); // 가감산 로직 수행
                            return;
                        }
                    }
                }
                return; // 모달 활성화 시 뒷배경 클릭 철저히 분쇄
            }

            // [모달 가드 2] 리더보드 창 오픈 시 락온
            if (screen == ScreenMode.Desktop && showLeaderboardWindow)
            {
                for (int i = 0; i < buttons.Count; i++)
                {
                    if (buttons[i].Action == leaderboardCloseKey && buttons[i].Bounds.Contains(mousePos))
                    {
                        showLeaderboardWindow = false;
                        TryBeep(750, 50);
                        Invalidate();
                        return;
                    }
                }
                return;
            }

            // [모달 가드 3] 올드 구글 창 오픈 시 락온
            if (screen == ScreenMode.Desktop && IsOldGoogleWindowVisible())
            {
                for (int i = 0; i < buttons.Count; i++)
                {
                    if (!buttons[i].Bounds.Contains(mousePos)) continue;

                    if (buttons[i].Action == OldGoogleCloseKey || buttons[i].Action == OldGoogleSearchFocusKey)
                    {
                        HandleAction(buttons[i].Action);
                        return;
                    }
                }
                return;
            }



            // ⚡ [일반 공용 버튼 클릭 엔진] 중첩 루프 해제하여 단일 플랫 연산으로 격상
            for (int buttonIndex = 0; buttonIndex < buttons.Count; buttonIndex++)
            {
                if (buttons[buttonIndex].Bounds.Contains(mousePos))
                {
                    HandleAction(buttons[buttonIndex].Action);
                    return;
                }
            }

            // 🖥️ [바탕화면 순정 아이콘 투명 레이더 감지기]
            if (screen == ScreenMode.Desktop)
            {
                if (firstDesktopNotice) return;

                // Internet Explorer
                if (GetInternetExplorerIconBounds().Contains(mousePos))
                {
                    OpenOldGoogleWindow();
                    return;
                }

                // 내 컴퓨터 아이콘
                Rectangle myComputerIconBounds = new Rectangle(15, 15, 95, 95);
                if (myComputerIconBounds.Contains(mousePos))
                {
                    showLeaderboardWindow = true;
                    UpdateAllBossRankings();
                    TryBeep(880, 60);
                    Invalidate();
                    return;
                }

                // 몬스터 도감 파일 아이콘
                Rectangle fileIconBounds = new Rectangle(15, 140, 95, 95);
                if (fileIconBounds.Contains(mousePos))
                {
                    showMonsterBookWindow = true;
                    TryBeep(880, 60);
                    Invalidate();
                    return;
                }

                Rectangle trashIconBounds = new Rectangle(15, 390, 95, 95);
                if (trashIconBounds.Contains(mousePos))
                {
                    showTrashCanWindow = true; // 휴지통 전격 오픈!
                    TryBeep(880, 60);
                    Invalidate();
                    return;
                }

                Rectangle startBtnBounds = new Rectangle(0, ClientSize.Height - 45, 110, 45);
                if (startBtnBounds.Contains(mousePos))
                {
                    showStartMenuPopup = !showStartMenuPopup; // 시작 메뉴 토글 가동
                    TryBeep(850, 50);
                    Invalidate(true);
                    return;
                }
            }


            // ⚔️ [인게임 필드 스테이지 조작 감지부]
            if (screen == ScreenMode.Stage)
            {
                if (e.Button == MouseButtons.Left)
                {
                    WeaponUpgradeFile drop = FindWeaponDropAt(mousePos);
                    if (drop != null)
                    {
                        draggedWeaponDrop = drop;
                        draggedWeaponDrop.Dragging = true;
                        return;
                    }
                }

                if (stageBossPhase && bossRuntime.HandleClick(mousePos))
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
            //  [시작 메뉴 전용 커맨드 실행 파이프라인]
            if (action == startMenuSaveKey)
            {
                SaveCurrentGame(); // C# 오리지널 직렬화 세이브 시스템 호출
                // 초고속 플래시 텍스트 알림 터트리기
                effects.Add(new Effect("text", ClientSize.Width / 2, ClientSize.Height / 2 - 40, ClientSize.Width / 2, ClientSize.Height / 2 - 40, 18, Color.Gold, "💾 SYSTEM: GAME_SAVE_SUCCESS!!"));
                TryBeep(1050, 80);
                return;
            }
            if (action == startMenuHelpKey)
            {
                screen = ScreenMode.Help; // 도움말 스크린 전환
                TryBeep(900, 50);
                return;
            }
            if (action == startMenuExitKey)
            {
                Application.Exit(); // 백신 커널 안전 종료 및 윈도우 완전 클로즈
                return;
            }

            if (action == "newGame" || action == "start" || action == "adminStart") { StartNewGameFromAdminMenu(); return; }
            if (action == "continue" || action == "continueGame") { ContinueFromAdminMenu(); return; }

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


            else if (action == OldGoogleCloseKey)
            {
                CloseOldGoogleWindow();
            }
            else if (action == OldGoogleSearchFocusKey)
            {
                oldGoogleSearchFocused = true;
                Invalidate();
            }

            else if (action == "npcHintClose") AdvanceStageNpcHint();
            else if (action == "profileTutorialNext") AdvanceProfileTutorial();
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
            else if (action == "endingBackToTitle")
            {
                endingTitle = "";
                endingBody = "";
                finalInput = "";

                currentStage = 0;
                screen = ScreenMode.StartMenu;

                Invalidate();
            }
            else if (action == "endingStay") Invalidate();
            else if (action == "helpBack") screen = ScreenMode.Desktop;
        }
        private void AdvanceProfileTutorial()
        {
            profileTutorialIndex++;

            if (profileTutorialIndex >= NpcDialogueData.GetProfileTutorialCount())
            {
                profileTutorialIndex = 0;
                profileTutorialOpen = false;

                screen = ScreenMode.Desktop;
                UpdateAllBossRankings();
            }

            TryBeep(880, 45);
            Invalidate();
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

            profileTutorialIndex = 0;
            profileTutorialOpen = true;
            firstDesktopNotice = false;

            TryBeep(920, 60);
            Invalidate();
        }
        private void BuyShopItem(string item)
        {
            int cost = 0;
            string label = "";

           
            if (item == "hp") { cost = 30; label = "HP 포션 +1"; }
            else if (item == "mp") { cost = 30; label = "MP 포션 +1"; }
            else { cost = 110; label = "포션 묶음 +2/+2"; item = "bundle"; }

            // =============================================================================
            // 소지 한도 제한 잠금 (HP 최대 6개, MP 최대 6개)
            // =============================================================================
            if (item == "hp" && player.HpPotions >= 6)
            {
                effects.Add(new Effect("text", ClientSize.Width / 2, 210, ClientSize.Width / 2, 210, 50, Color.Orange, "HP 포션 소지 한도 초과 (최대 6개)"));
                TryBeep(320, 100);
                return;
            }
            if (item == "mp" && player.MpPotions >= 6)
            {
                effects.Add(new Effect("text", ClientSize.Width / 2, 210, ClientSize.Width / 2, 210, 50, Color.Orange, "MP 포션 소지 한도 초과 (최대 6개)"));
                TryBeep(320, 100);
                return;
            }
            // 묶음 구매시 하나라도 최대 소지 한도를 넘어가면 구매를 완전히 차단합니다.
            if (item == "bundle" && (player.HpPotions + 2 > 6 || player.MpPotions + 2 > 6))
            {
                effects.Add(new Effect("text", ClientSize.Width / 2, 210, ClientSize.Width / 2, 210, 50, Color.Orange, "소지 한도 초과로 묶음 구매 불가"));
                TryBeep(320, 100);
                return;
            }

            // =============================================================================
            // 코인 자산 부족 검증
            // =============================================================================
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
                finalInput = "";
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
            if (player.Mp < 20) { effects.Add(new Effect("text", player.X, player.Y - 90, player.X, player.Y - 90, 35, Color.DeepSkyBlue, "MP 부족 (요구 마나: 20)")); return; }

            player.Mp -= 20;
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
            if (player.Mp < 35) { Red: effects.Add(new Effect("text", player.X, player.Y - 90, player.X, player.Y - 90, 35, Color.DeepSkyBlue, "MP 부족 (요구 마나: 35)")); return; }

            player.Mp -= 35;
            playerShield = (int)(player.MaxHp * 0.20f); // 20 HP 보호막 생성
            eShieldDurationTicks = 300;
            eCooldownTicks = 1200;      // 20초 쿨타임

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
            if (player.Mp < 60) { effects.Add(new Effect("text", player.X, player.Y - 90, player.X, player.Y - 90, 35, Color.DeepSkyBlue, "MP 부족 (요구 마나: 60)")); return; }

            player.Mp -= 60;
            rCooldownTicks = 1500; // 25초 쿨타임

            PlayerMovementSystem.StartSkillAnimation(player, 3);

            float targetX = player.X + (player.Facing == 0 ? 250f : player.Facing * 250f);

            playerSkySwords.Add(new PlayerSkySword
            {
                X = targetX,
                Y = player.Y,
                Timer = 15,       
                MaxTimer = 23,
                SwordType = "cold"
            });

            effects.Add(new Effect("text", player.X, player.Y - 120, player.X, player.Y - 120, 60, Color.DodgerBlue, "SYSTEM_CALL: COLD_LONGSWORD"));
            TryBeep(350, 200);
        }

    }
}
