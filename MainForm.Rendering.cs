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
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            // 전체 화면은 매 프레임 다시 그려지므로 기본 품질을 고속으로 설정합니다.
            // 주요 배경은 Renderer 내부 캐시를 사용하고, 필요한 부분만 별도로 고품질 처리합니다.
            g.SmoothingMode = SmoothingMode.HighSpeed;
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.InterpolationMode = InterpolationMode.Low;
            g.PixelOffsetMode = PixelOffsetMode.HighSpeed;

            buttons.Clear();

            if (screen == ScreenMode.StartMenu) DrawAdminStartMenu(g);
          

            if (screen == ScreenMode.Boot) DrawBoot(g);
            else if (screen == ScreenMode.AssistantIntro) DrawAssistantIntro(g);
            else if (screen == ScreenMode.ProfileSetup) DrawProfileSetup(g);
            else if (screen == ScreenMode.Desktop) DrawDesktop(g);
            else if (screen == ScreenMode.Shop) DrawShop(g);
            else if (screen == ScreenMode.Stage) DrawStage(g);
            else if (screen == ScreenMode.StageClearDialog) DrawStageClear(g);
            else if (screen == ScreenMode.FinalInput) DrawFinalInput(g);
            else if (screen == ScreenMode.Ending) DrawEnding(g);
            else if (screen == ScreenMode.Help) DrawHelp(g);

            // Stage 화면은 DrawStage 안에서 카메라 좌표 기준으로 이펙트를 이미 그립니다.
            // 다시 한 번 0카메라로 그리면 렉과 잔상/중복 표시가 생겨서 비-스테이지 화면에서만 처리합니다.
            if (screen != ScreenMode.Stage)
            {
                for (int i = 0; i < effects.Count; i++) Renderer.DrawEffect(g, effects[i], 0);
            }
        }

        private void DrawBoot(Graphics g)
        {
            BackgroundRenderer.DrawBootScreen(g, ClientSize.Width, ClientSize.Height, bootTicks);
        }

        private void DrawAssistantIntro(Graphics g)
        {
            Renderer.DrawXPWallpaper(g, ClientRectangle);
            DesktopIconUI.Shared.DrawFixedDesktopIcons(g, ClientRectangle);
            Renderer.DrawXPTaskbar(g, ClientRectangle, "Windows XP Desktop");

            string title = NpcDialogueData.GetIntroTitle(introIndex);
            string introBody = NpcDialogueData.GetIntroMessage(introIndex);
            using (SolidBrush dim = new SolidBrush(Color.FromArgb(70, 0, 0, 0))) g.FillRectangle(dim, ClientRectangle);
            Rectangle introNotice = SystemWindowUI.Shared.GetStandardNoticeRect(ClientSize);
            SystemWindowUI.Shared.DrawAssistantNotice(
                g,
                introNotice,
                title,
                introBody,
                NpcMood.Welcome,
                Environment.TickCount / 30,
                buttons,
                "introNext",
                "introNext"
            );
        }

        private void DrawProfileSetup(Graphics g)
        {
            Renderer.DrawXPWallpaper(g, ClientRectangle);
            DesktopIconUI.Shared.DrawFixedDesktopIcons(g, ClientRectangle);
            Renderer.DrawXPTaskbar(g, ClientRectangle, "Recovery Profile Setup");
            Rectangle win = SystemWindowUI.Shared.GetStandardNoticeRect(ClientSize);
            SystemWindowUI.Shared.DrawProfileSetupWindow(
                g,
                win,
                profileInput,
                NpcMood.Happy,
                Environment.TickCount / 30,
                buttons
            );
        }

        private void DrawDesktop(Graphics g)
        {
            int w = ClientSize.Width;
            int h = ClientSize.Height;

            // 1. 기본 XP 바탕화면 배경
            DesktopBackgroundUI.Shared.Draw(g, ClientRectangle);
            DesktopIconUI.Shared.DrawFixedDesktopIcons(g, ClientRectangle);

            // 2. 바탕화면 스테이지 바로가기 아이콘
            DesktopIconUI.Shared.DrawStageIcons(g, stages, unlockedStage, selectedStage, player.ClearedStages, buttons);

            // 우측의 파일 속성 / 복구 상태 정보창 패널 그리기 시스템 연동
            DrawDesktopInfoPanel(g);

            // 좌측 하단 영역의 휴지통 아이템 상점 아이콘 그리기 시스템 연동
            DesktopIconUI.Shared.DrawRecoveryToolsShortcut(g, ClientRectangle, player.Coins, buttons);

            // 3. 하단 작업 표시줄
            TaskbarUI.Shared.Draw(g, ClientRectangle);

            // 4. 최초 진입 안내창
            if (firstDesktopNotice)
            {
                using (SolidBrush dim = new SolidBrush(Color.FromArgb(68, 0, 0, 0)))
                    g.FillRectangle(dim, ClientRectangle);

                Rectangle notice = SystemWindowUI.Shared.GetStandardNoticeRect(ClientSize);

                SystemWindowUI.Shared.DrawAssistantNotice(
                    g,
                    notice,
                    "Windows Recovery Assistant",
                    NpcDialogueData.DesktopNoticeBody,
                    NpcMood.Basic,
                    Environment.TickCount / 30,
                    buttons,
                    "desktopNoticeOk",
                    "desktopNoticeClose"
                );
            }
            TaskbarUI.Shared.Draw(g, ClientRectangle);
        }

        private void DrawShop(Graphics g)
        {
            RecoveryToolsUI.Shared.DrawShop(
                g,
                ClientRectangle,
                player,
                selectedShopItem,
                buttons
            );
        }

        private void DrawDesktopInfoPanel(Graphics g)
        {
            RecoveryToolsUI.Shared.DrawDesktopStatusPanel(
                g,
                ClientRectangle,
                player,
                unlockedStage,
                stages.Count
            );
        }

        private void DrawStage(Graphics g)
        {
            StageInfo st = stages[currentStage - 1];
            int mapWidth = GetStageMapWidth(st);

          
            if (stageBossPhase)
            {
                bool shouldShake = false;

                // 현재 맵에 활성화되어 살아있는 보스 객체를 탐색
                GameEntity currentBoss = enemies.Find(e => e.IsBoss && e.Hp > 0);

                if (currentBoss != null)
                {
                    // 1. 1번 보스 (Driver-K): 50% 리소스 부족 디버그 팝업 패턴이 켜져 있을 때
                    if (currentBoss.Name.Contains("Driver-K") && bossManager.IsResourcePatternActive)
                    {
                        shouldShake = true;
                    }

                    // 2. 2번 보스 (High-Kernel): 75%/25%(AccessDenied), 50%(SystemWipe), 10%(Enrage) 모든 특수기믹일 때
                    if (currentBoss.Name.Contains("High-Kernel") &&
                       (bossManager.IsAccessDeniedActive ||  bossManager.IsEnrageActive))
                    {
                        shouldShake = true;
                    }

                    // 3. 4번 보스 (Exception Queen): 75%/25%(NullRef), 10%(StackOverflow) 패턴일 때 
                    // (※ 지시사항에 따라 50% 패턴인 IsTryCatchActive일 때는 흔들리지 않도록 원천 제외)
                    if ((currentBoss.Name.Contains("Exception Queen") || currentBoss.Name.Contains("Exception_Queen")) &&
                       (bossManager.IsNullRefActive || bossManager.IsStackOverflowActive))
                    {
                        shouldShake = true;
                    }
                }

                // 흔들림 조건 충족 시 Graphics 도화지 자체를 무작위로 뒤흔듦 (-5 ~ +5 픽셀 강도)
                if (shouldShake)
                {
                    int shakeX = random.Next(-5, 6);
                    int shakeY = random.Next(-5, 6);
                    g.TranslateTransform(shakeX, shakeY);
                }
            }
            // ==========================================================

            // 배경 그리기 시작 (이 아래로는 기존에 완성해두신 코드와 100% 동일합니다)
            Renderer.DrawStageBackground(g, ClientRectangle, st, cameraX, stageBossPhase, mapWidth);

            if (!stageBossPhase)
            {
                using (Font f = Renderer.F(10f, FontStyle.Bold))
                using (SolidBrush b = new SolidBrush(Color.White))
                using (SolidBrush bg = new SolidBrush(Color.FromArgb(130, 0, 0, 0)))
                {
                    Rectangle top = new Rectangle(250, 14, ClientSize.Width - 500, 48);
                    g.FillRectangle(bg, top);
                    string stageTitle = "STAGE " + st.Index.ToString("00") + "  " + st.Name + "  |  " + st.Objective;
                    g.DrawString(stageTitle, f, b, top, Renderer.Center());
                }
            }

            DrawHud(g, st);
            if (IsStageNpcHintOpen())
            {
                DrawStageNpcHint(g, st);
                return;
            }
            foreach (GameEntity m in enemies) if (m.Hp > 0) Renderer.DrawEnemy(g, m, cameraX, ClientSize.Height); 
            for (int i = 0; i < weaponDrops.Count; i++) Renderer.DrawWeaponUpgradeFile(g, weaponDrops[i], cameraX);
            bossRuntime.DrawOverlay(g, currentStage, stageBossPhase, cameraX, ClientSize);

            if (stageBossPhase)
            {
                DrawCustomBossGimmicks(g);
                GameEntity currentBoss = enemies.Find(e => e.IsBoss);
                if (currentBoss != null)
                {
                    Renderer.DrawBossGlobalUI(g, currentBoss, ClientSize);
                }
            }

            bossRuntime.DrawOverlay(g, currentStage, stageBossPhase, cameraX, ClientSize);
            if (stageBossPhase)
            {
                DrawCustomBossGimmicks(g);
                GameEntity currentBoss = enemies.Find(e => e.IsBoss);
                if (currentBoss != null)
                {
                    // 1. 진짜 보스 본체의 레이드 스타일 상단 대형 바 출력
                    Renderer.DrawBossGlobalUI(g, currentBoss, ClientSize);

                    // ==========================================================
                    // [3번 수정] 1% 최종 페이즈 진입 시 보스바 하단에 분신 전용 보라색 체력바 및 타이머 연동
                    // ==========================================================
                    if (currentBoss.Name.Contains("Binny") && bossManager.IsIllusionActive && bossManager.BinnyClone != null)
                    {
                        // 본체 체력바(Y: 45, H: 24) 바로 아랫단 지점(Y: 74)에 정확히 밀착 정렬
                        Rectangle cloneBarRect = new Rectangle(ClientSize.Width / 2 - 350, 74, 700, 16);

                        // [요구사항 원칙 준수] 진짜 보스(빨간색)와 완벽히 분리되도록 보라색(Color.Purple) HP 바 출력
                        Renderer.DrawBar(g, cloneBarRect, bossManager.BinnyClone.Hp, bossManager.BinnyClone.MaxHp, Color.Purple);

                        // 외곽 화이트 테두리 선 마감
                        using (Pen borderPen = new Pen(Color.White, 1.5f))
                            g.DrawRectangle(borderPen, cloneBarRect);

                        using (Font f = Renderer.F(10f, FontStyle.Bold))
                        {
                            string cloneHpText = $"Illegal_Binny 분신 개체 (0번 인덱스)  [ HP : {bossManager.BinnyClone.Hp} / {bossManager.BinnyClone.MaxHp} ]";
                            g.DrawString(cloneHpText, f, Brushes.White, cloneBarRect, Renderer.Center());

                            // 12초 타임어택 제한시간 역산 엔진 디스플레이 (30 FPS 기준 초 단위 환산)
                            float secLeft = bossManager.IllusionTimer / 30f;
                            if (secLeft < 0f) secLeft = 0f;
                            string timerDisplay = $"⏳ 가비지 컬렉션 동기화 파쇄 제한시간: {secLeft:0.0}초";

                            // 멀티 사살 룰: 보스나 분신 중 한쪽 격파 시 발동되는 3초 카운트다운 실시간 텍스트 결합
                            if (bossManager.DualDeathTimer > 0)
                            {
                                float syncLeft = bossManager.DualDeathTimer / 30f;
                                if (syncLeft < 0f) syncLeft = 0f;
                                timerDisplay += $"  |  🚨 양방향 소거 메모리 링크 마감: {syncLeft:0.0}초 경고!!";
                            }

                            g.DrawString(timerDisplay, f, Brushes.Magenta, ClientSize.Width / 2, cloneBarRect.Bottom + 12, Renderer.Center());
                        }
                    }
                }
                for (int i = 0; i < effects.Count; i++) Renderer.DrawEffect(g, effects[i], cameraX);
                //if (!stageNpcHintClosed) DrawStageNpcHint(g, st);

                if (showStageClearPopup)
                {
                    int winW = 500; int winH = 280;
                    int winX = (ClientSize.Width - winW) / 2;
                    int winY = (ClientSize.Height - winH) / 2;

                    // 1. 배경 이미지 드로우 (기존 코드 유지)
                    if (Renderer.Img_AlarmBg != null) g.DrawImage(Renderer.Img_AlarmBg, winX, winY, winW, winH);

                    // ----------------------------------------------------------
                    // [보정 모듈 주입] 글자가 번지고 흐려지는 현상 원천 차단!
                    // GDI+의 텍스트 렌더링 힌트를 'ClearTypeGridFit'으로 격상시킵니다.
                    // ----------------------------------------------------------
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                    // ----------------------------------------------------------

                    // 2. 텍스트 정보 매핑 (이제 번짐 없이 완벽하게 칼출력됩니다)
                    using (Font mainFont = Renderer.F(16f, FontStyle.Bold))
                    using (Font subFont = Renderer.F(11.5f, FontStyle.Regular))
                    {
                        // 타이틀바 텍스트
                        g.DrawString("STAGE CLEAN 리포트 전송 완료", subFont, Brushes.White, winX + 12, winY + 8);

                        // 본문 메시지
                        g.DrawString($"STAGE {currentStage} SYSTEM COMPLETED!", mainFont, Brushes.MidnightBlue, winX + winW / 2, winY + 50, Renderer.Center());

                        string reportText = $"▶ 이진 가비지 데이터 소멸 완료\r\n" +
                                            $"▶ 업그레이드 인덱스 파일 : [CORE_UPGRADE.bin]\r\n" +
                                            $"▶ 복구 공헌도 추가 보상 : +{popupBonusCoins} COINS";
                        g.DrawString(reportText, subFont, Brushes.Black, winX + 55, winY + 115);
                    }

                    // 3. 확인 버튼 드로우 및 문자열 배치 (기존 코드 유지)
                    int btnW = 120; int btnH = 36;
                    popupConfirmBtnBounds = new Rectangle((winX + winW / 2) - btnW / 2, winY + winH - 60, btnW, btnH);
                    if (Renderer.Img_PopupBtn != null) g.DrawImage(Renderer.Img_PopupBtn, popupConfirmBtnBounds);

                    using (Font btnFont = Renderer.F(11f, FontStyle.Bold))
                    {
                        g.DrawString("확인 (OK)", btnFont, Brushes.Black, popupConfirmBtnBounds, Renderer.Center());
                    }
                }
            }


            bool playerMovingNow = Math.Abs(player.TargetX - player.X) > 3.5f || Math.Abs(player.TargetY - player.Y) > 3.5f ||
                                   Math.Abs(player.MoveVelocityX) > 0.25f || Math.Abs(player.MoveVelocityY) > 0.25f;
            System.Drawing.Drawing2D.GraphicsState playerScaleState = g.Save();

            // 화면 기준 플레이어의 발바닥 좌표를 축(Pivot)으로 잡습니다.
            float pivotX = player.X - cameraX;
            float pivotY = player.Y;

            // 1. 발바닥 위치로 중심 이동 -> 2. 항상 크기 0.8배(20% 축소) -> 3. 다시 중심 복귀
            g.TranslateTransform(pivotX, pivotY);
            g.ScaleTransform(0.8f, 0.8f);
            g.TranslateTransform(-pivotX, -pivotY);

            // 축소 배율이 적용된 도화지에 대기/이동 스프라이트를 렌더링합니다.
            Renderer.DrawRecoveryProgram(g, player, true, cameraX, playerMovingNow);

            // 플레이어 렌더링이 완료되었으므로 축소 배율을 해제하고 원래 배율(1.0배)로 복원합니다.
            if (playerScaleState != null)
            {
                g.Restore(playerScaleState);
            }

            for (int i = 0; i < effects.Count; i++) Renderer.DrawEffect(g, effects[i], cameraX);
            if (IsStageNpcHintOpen())
            {
                DrawStageNpcHint(g, st);
            }
        }

        private void DrawStage1BossPatternOverlay(Graphics g)
        {
            bossRuntime.DrawOverlay(g, currentStage, stageBossPhase, cameraX, ClientSize);
        }

        private void DrawHud(Graphics g, StageInfo st)
        {
            int hudWidth = 350;
            int hudHeight = 250;
            int hudMargin = 18;

            Rectangle h = new Rectangle(
                ClientSize.Width - hudWidth - hudMargin,
                hudMargin,
                hudWidth,
                hudHeight
            );

            // HUD 전용 이미지 배경 사용
            SystemWindowUI.Shared.DrawBlueHudFrame(
                g,
                h,
                "Recovery Program"
            );

            using (Font f = Renderer.F(8.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(34, 42, 60)))
            {
                g.DrawString("Profile: " + player.ProfileName, f, b, new Rectangle(h.X + 22, h.Y + 52, h.Width - 44, 18), Renderer.LeftMiddle());
                g.DrawString("Program: Recovery Program", f, b, new Rectangle(h.X + 22, h.Y + 72, h.Width - 44, 18), Renderer.LeftMiddle());
                g.DrawString("Level: " + player.Level + "   Weapon: +" + player.WeaponLevel + "   Coin: " + player.Coins, f, b, new Rectangle(h.X + 22, h.Y + 92, h.Width - 44, 18), Renderer.LeftMiddle());
            }

            int labelX = h.X + 22;
            int barX = h.X + 104;
            int barW = h.Width - 128;

            Renderer.DrawBar(g, new Rectangle(barX, h.Y + 122, barW, 12), player.Hp, player.MaxHp, Color.LimeGreen);
            Renderer.DrawBar(g, new Rectangle(barX, h.Y + 144, barW, 12), player.Mp, player.MaxMp, Color.DeepSkyBlue);
            Renderer.DrawBar(g, new Rectangle(barX, h.Y + 166, barW, 12), player.SystemStability, 100, Color.FromArgb(75, 150, 255));

            using (Font f = Renderer.F(7.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.Black))
            {
                g.DrawString("HP", f, b, labelX, h.Y + 117);
                g.DrawString("MP", f, b, labelX, h.Y + 139);
                g.DrawString("Stability", f, b, labelX, h.Y + 161);

                g.DrawString(
                    "D: HP포션(" + player.HpPotions + ")   F: MP포션(" + player.MpPotions + ")",
                    f,
                    b,
                    new Rectangle(h.X + 22, h.Y + 188, h.Width - 44, 18),
                    Renderer.LeftMiddle()
                );

                g.DrawString(
                    "마우스 클릭 이동 / 부드러운 추적 / Q W E R",
                    f,
                    b,
                    new Rectangle(h.X + 22, h.Y + 206, h.Width - 44, 18),
                    Renderer.LeftMiddle()
                );
            }
        }

        private void DrawStageNpcHint(Graphics g, StageInfo st)
        {
            using (SolidBrush dim = new SolidBrush(Color.FromArgb(72, 0, 0, 0))) g.FillRectangle(dim, ClientRectangle);
            Rectangle r = SystemWindowUI.Shared.GetStandardNoticeRect(ClientSize);
            string body = NpcDialogueData.GetStageHintText(st.Index, st.Name, stageNpcHintIndex);
            NpcMood mood = NpcDialogueData.GetStageHintMood(st.Index, stageNpcHintIndex, st.NpcMood);

            SystemWindowUI.Shared.DrawAssistantNotice(
                g,
                r,
                "Windows Recovery Assistant",
                body,
                mood,
                Environment.TickCount / 30,
                buttons,
                "npcHintClose",
                "npcHintClose"
            );
        }
        private void DrawStageClear(Graphics g)
        {
            Renderer.DrawXPWallpaper(g, ClientRectangle);
            TaskbarUI.Shared.Draw(g, ClientRectangle);

            using (SolidBrush dim = new SolidBrush(Color.FromArgb(55, 0, 0, 0)))
                g.FillRectangle(dim, ClientRectangle);

            StageInfo st = stages[clearStage - 1];

            string body = NpcDialogueData.BuildStageClearText(st, clearStage, stages);
            NpcMood mood = NpcDialogueData.GetStageClearMood(st);

            Rectangle clearNotice = SystemWindowUI.Shared.GetStandardNoticeRect(ClientSize);

            SystemWindowUI.Shared.DrawAssistantNotice(
                g,
                clearNotice,
                "Windows Recovery Assistant",
                body,
                mood,
                Environment.TickCount / 30,
                buttons,
                "clearNext",
                "clearNext"
            );
        }
        private string BuildStageClearNoticeBody(StageInfo st)
        {
            string body = "";

            body += "STAGE " + st.Index.ToString("00") + " 복구 리포트\n\n";
            body += "▶ 처리 항목 : " + GetStageClearProcessText(st) + "\n";
            body += "▶ 확인 파일 : " + GetStageClearFileText(st) + "\n";
            body += "▶ 복구 상태 : 완료\n\n";

            if (clearStage < stages.Count)
            {
                body += "다음 복구 항목이 바탕화면에 생성되었습니다.\n";
                body += stages[clearStage].FileName;
            }
            else
            {
                body += "최종 입력 절차로 이동합니다.";
            }

            return body;
        }

        private string GetStageClearProcessText(StageInfo st)
        {
            if (st.Index == 1) return "바탕화면 정리 완료";
            if (st.Index == 2) return "Driver-K 충돌 상태 해제";
            if (st.Index == 3) return "업데이트 구성 요소 정리 완료";
            if (st.Index == 4) return "System32 무결성 확인 완료";
            if (st.Index == 5) return "네트워크 연결 상태 안정화";
            if (st.Index == 6) return "Blue Screen 충돌 회피 완료";
            if (st.Index == 7) return "레지스트리 기록 검사 완료";
            if (st.Index == 8) return "Exception Queen 오류 격리 완료";
            if (st.Index == 9) return "임시 캐시 정리 완료";
            if (st.Index == 10) return "Recycle Bin 최종 정리 완료";

            return st.Name + " 복구 완료";
        }

        private string GetStageClearFileText(StageInfo st)
        {
            if (st.Index == 1) return "DESKTOP_CLEANUP.log";
            if (st.Index == 2) return "CORE_UPGRADE.bin";
            if (st.Index == 3) return "UPDATE_PATCH_INDEX.bin";
            if (st.Index == 4) return "SYSTEM32_CHECK.log";
            if (st.Index == 5) return "PORT_BLOCK_RECORD.dat";
            if (st.Index == 6) return "BSOD_DUMP_TRACE.tmp";
            if (st.Index == 7) return "RECENT_ACTIONS.reg";
            if (st.Index == 8) return "UNSENT_REPORT.tmp";
            if (st.Index == 9) return "TEMP_CACHE_TRACE.tmp";
            if (st.Index == 10) return "FINAL_PROCESS_INPUT.sys";

            return "RECOVERY_REPORT.log";
        }

        private NpcMood GetStageClearNpcMood(StageInfo st)
        {
            if (st.Index <= 2) return NpcMood.Happy;
            if (st.Index <= 5) return NpcMood.Basic;
            if (st.Index <= 7) return NpcMood.Log;
            if (st.Index <= 9) return NpcMood.Warning;

            return NpcMood.Damaged;
        }

        private void DrawFinalInput(Graphics g)
        {
            Renderer.DrawStageBackground(g, ClientRectangle, stages[9], 0);
            Rectangle win = new Rectangle(ClientSize.Width / 2 - 460, ClientSize.Height / 2 - 235, 920, 450);
            Renderer.DrawXPWindow(g, win, "최종 입력창 - 삭제할 프로세스 이름", true);
            Renderer.DrawNpcImage(g, new Rectangle(win.X + 24, win.Y + 64, 170, 250), NpcMood.Warning);
            using (Font f = Renderer.F(10f, FontStyle.Regular))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(32, 38, 50)))
            {
                string text = NpcDialogueData.FinalInputInstruction;
            }
            Rectangle input = new Rectangle(win.X + 215, win.Y + 250, 520, 36);
            using (SolidBrush b = new SolidBrush(Color.White)) g.FillRectangle(b, input);
            using (Pen p = new Pen(Color.DarkRed, 2f)) g.DrawRectangle(p, input);
            using (Font f = Renderer.F(13f, FontStyle.Bold)) g.DrawString(finalInput + "_", f, Brushes.Black, new Rectangle(input.X + 8, input.Y, input.Width - 16, input.Height), Renderer.LeftMiddle());
            Rectangle btn = new Rectangle(win.Right - 170, win.Bottom - 58, 130, 34);
            Renderer.DrawButton(g, btn, "입력", true);
            buttons.Add(new UiButton(btn, "finalOk"));
        }

        private void DrawEnding(Graphics g)
        {
            Renderer.DrawXPWallpaper(g, ClientRectangle);
            Renderer.DrawXPTaskbar(g, ClientRectangle, endingTitle);
            Rectangle win = new Rectangle(ClientSize.Width / 2 - 430, ClientSize.Height / 2 - 190, 860, 360);
            Renderer.DrawXPWindow(g, win, endingTitle, endingTitle.Contains("잘못") || endingTitle.Contains("루프"));
            Renderer.DrawNpcImage(g, new Rectangle(win.X + 24, win.Y + 62, 160, 230), endingTitle.Contains("진엔딩") ? NpcMood.Happy : NpcMood.Warning);
            using (Font f = Renderer.F(12f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(32, 42, 64)))
                g.DrawString(endingBody, f, b, new Rectangle(win.X + 210, win.Y + 80, win.Width - 250, 170), Renderer.Left());
            using (Font f = Renderer.F(9f, FontStyle.Regular))
                g.DrawString("Enter를 누르면 종료 화면을 유지합니다.", f, Brushes.DarkBlue, new Rectangle(win.X + 210, win.Bottom - 70, win.Width - 250, 24), Renderer.LeftMiddle());
        }

        private void DrawHelp(Graphics g)
        {
            Renderer.DrawXPWallpaper(g, ClientRectangle);
            Renderer.DrawXPTaskbar(g, ClientRectangle, "Help");
            Rectangle win = new Rectangle(ClientSize.Width / 2 - 460, ClientSize.Height / 2 - 245, 920, 460);
            Renderer.DrawXPWindow(g, win, "문서 기반 고정 스테이지 안내", false);
            using (Font f = Renderer.F(10f, FontStyle.Regular))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(32, 42, 64)))
            {
                string text = "이 버전은 업로드한 STAGE01~STAGE10 문서의 화면 구성, 플레이어 캐릭터, NPC 표현, 스테이지 흐름을 게임 안에 고정 반영한 빌드입니다.\n\n" +
                              "핵심 반영:\n" +
                              "1) 플레이어는 인간 용사가 아니라 Recovery Program입니다.\n" +
                              "2) NPC는 문서 이미지의 Windows Recovery Assistant 원본 크롭을 사용합니다.\n" +
                              "3) Stage 1은 별도 전투창이 아니라 XP 바탕화면 전체가 전투 필드입니다.\n" +
                              "4) 보스 스테이지는 일반 몹전 없이 보스 중심으로 구성됩니다.\n" +
                              "5) Stage 10은 Illegal_Binny 이후 직접 입력 기반 엔딩 분기로 진행됩니다.\n\nESC 또는 Enter: 돌아가기";
                g.DrawString(text, f, b, new Rectangle(win.X + 30, win.Y + 58, win.Width - 60, win.Height - 90), Renderer.Left());
            }
            Rectangle btn = new Rectangle(win.Right - 160, win.Bottom - 48, 120, 32);
            Renderer.DrawButton(g, btn, "돌아가기", true);
            buttons.Add(new UiButton(btn, "helpBack"));
        }


        private Rectangle NotificationOkRect(Rectangle r)
        {
            return new Rectangle(r.Right - 112, r.Bottom - 42, 88, 28);
        }

        private void DrawCustomBossGimmicks(Graphics g)
        {
            if (bossRuntime == null) return;

            // 1. High-Kernel: 권한 거부(미사일) 경고문구
            if (bossManager.IsAccessDeniedActive)
            {
                using (Font f = Renderer.F(14f, FontStyle.Bold))
                    g.DrawString("권한 거부 상태! 스킬 사용 시 미사일 투하!", f, Brushes.Red, new Point(ClientSize.Width / 2 - 170, 110));
            }

            
            foreach (var sm in bossManager.SkyMissiles)
            {
                float sPosX = sm.X - cameraX;
                using (Pen p = new Pen(Color.FromArgb(200, Color.Red), 2f)) g.DrawEllipse(p, sPosX - 50, sm.Y - 25, 100, 50);
                float missileY = sm.Y - 500 + (1f - (sm.Timer / 60f)) * 500;

                // 💡 [디자인 완전 교체] 투박한 OrangeRed 직사각형 막대 대신 Meteor 이미지 전체 투사
                if (Renderer.Img_Meteor != null)
                {
                    g.DrawImage(Renderer.Img_Meteor, sPosX - 32, missileY - 32, 64, 64);
                }
                else
                {
                    g.FillRectangle(Brushes.OrangeRed, sPosX - 5, missileY, 10, 25);
                }
            }
            foreach (var p in bossManager.Projectiles)
            {
                float sPosX = p.X - cameraX;
                if (p.IsEnrageMissile)
                {
                    // ❌ 기존의 투박한 주황색 사각형 채우기 및 흰색 테두리선 삭제 구역

                    // 💡 [디자인 요청 반영] 10% 기믹용 이미지를 Meteor2에서 일반 Meteor(Img_Meteor) 이미지 전체 투사로 변경합니다.
                    if (Renderer.Img_Meteor != null)
                    {
                        g.DrawImage(Renderer.Img_Meteor, sPosX - 32, p.Y - 32, 64, 64);
                    }
                    else
                    {
                        using (SolidBrush ob = new SolidBrush(Color.OrangeRed))
                            g.FillEllipse(ob, sPosX - 20, p.Y - 20, 40, 40);
                    }
                }
                else
                {
                    g.FillEllipse(Brushes.BlueViolet, sPosX - 12, p.Y - 12, 24, 24);
                    g.FillEllipse(Brushes.White, sPosX - 5, p.Y - 5, 10, 10);
                }
            }

            // 3. 리소스 부족 패턴 (DEBUG 버튼 미니게임)
            if (bossManager.IsResourcePatternActive)
            {
                Rectangle overlay = new Rectangle(ClientSize.Width / 2 - 400, ClientSize.Height / 2 - 150, 800, 300);
                using (SolidBrush bg = new SolidBrush(Color.FromArgb(180, 20, 20, 20))) g.FillRectangle(bg, overlay);
                using (Font f = Renderer.F(16f, FontStyle.Bold))
                {
                    string resMsg = $"!!! 리소스 부족: 시스템 과부하 !!!\n4.5초 안에 DEBUG 버튼을 전부 눌러 DEBUG를 실행하세요!\n(남은 시간: {(bossManager.ResourceTimer / 60.0):0.0}s)";
                    g.DrawString(resMsg, f, Brushes.Yellow, overlay, Renderer.Center());
                }
                foreach (var btn in bossManager.DebugButtons)
                {
                    using (SolidBrush bb = new SolidBrush(Color.DarkRed)) g.FillRectangle(bb, btn);
                    using (Pen p = new Pen(Color.White, 2f)) g.DrawRectangle(p, btn);
                    using (Font f = Renderer.F(10f, FontStyle.Bold)) g.DrawString("DEBUG.EXE", f, Brushes.White, btn, Renderer.Center());
                }
            }

            // 4. Exception Queen: 타이핑 페이즈 바 및 텍스트
            if (bossManager.IsTryCatchActive && bossManager.IsTypingPhaseActive)
            {
                float sPosX = player.X - cameraX;
                Rectangle typeBox = new Rectangle((int)sPosX - 85, (int)player.Y - 160, 170, 45);
                using (SolidBrush bg = new SolidBrush(Color.FromArgb(220, 0, 0, 0))) g.FillRectangle(bg, typeBox);
                using (Font f = Renderer.F(18f, FontStyle.Bold)) g.DrawString(bossManager.CurrentTypingTarget, f, Brushes.Lime, typeBox, Renderer.Center());

                Rectangle timerBar = new Rectangle(typeBox.X, typeBox.Bottom + 2, typeBox.Width, 6);
                int barWidth = (int)((bossManager.TypingTimer / 300f) * timerBar.Width);
                using (SolidBrush barBg = new SolidBrush(Color.Yellow)) g.FillRectangle(barBg, timerBar.X, timerBar.Y, Math.Max(0, barWidth), timerBar.Height);
            }

            if (bossManager.IsSystemWipeActive)
            {
                float szX = bossManager.SafeZoneCenter.X - cameraX;
                float szY = bossManager.SafeZoneCenter.Y;
                float radius = bossManager.SafeZoneRadius; // 80f

                if (Renderer.Img_Safezone != null)
                {
                    // 자르지 않고 세이프존 이미지 전체 크기를 원형 범위(지름 160px)에 맞춰 드로우
                    g.DrawImage(Renderer.Img_Safezone, szX - radius, szY - radius, radius * 2, radius * 2);
                }
                else
                {
                    // 이미지 로드 실패 시의 가시성 확보용 네온 그린 가이드라인 백업 백업선
                    using (Pen p = new Pen(Color.Lime, 3f))
                    {
                        p.DashStyle = DashStyle.Dash;
                        g.DrawEllipse(p, szX - radius, szY - radius, radius * 2, radius * 2);
                    }
                }
            }

        }
       
        private void DrawAdminStartMenu(Graphics g)
        {
            
            StartMenuScreen.Draw(g, ClientRectangle, buttons, GameSaveSystem.HasSave());
        }

    }
}
