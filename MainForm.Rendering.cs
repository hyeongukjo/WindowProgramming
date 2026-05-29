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

            g.SmoothingMode = SmoothingMode.HighSpeed;
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.InterpolationMode = InterpolationMode.Low;
            g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

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

            if (screen != ScreenMode.Stage)
            {
                for (int i = 0; i < effects.Count; i++) Renderer.DrawEffect(g, effects[i], 0);
            }
        }

        private void DrawBoot(Graphics g) => BackgroundRenderer.DrawBootScreen(g, ClientSize.Width, ClientSize.Height, bootTicks);

        // =============================================================================
        // 🌟 [교정]: 인트로 본문 대사 문구(introBody) 누락 현상 전면 처단 복구 격실
        // =============================================================================
        private void DrawAssistantIntro(Graphics g)
        {
            // 1. 배경 화면 및 기본 바탕화면 아이콘 렌더링 유지
            Renderer.DrawXPWallpaper(g, ClientRectangle);

            // 기존 마스터 코드의 UI 컴포넌트 호출 방식을 그대로 유지합니다.
            if (DesktopIconUI.Shared != null)
            {
                DesktopIconUI.Shared.DrawFixedDesktopIcons(g, ClientRectangle);
            }
            Renderer.DrawXPTaskbar(g, ClientRectangle, "Windows XP Desktop");

            // 2. 💡 [핵심 복구]: NpcDialogueData 격실에서 현재 인덱스에 맞는 실제 타이틀과 대사 본문을 추출합니다.
            string title = NpcDialogueData.GetIntroTitle(introIndex);
            string introBody = NpcDialogueData.GetIntroMessage(introIndex);

            // 화면을 어둡게 만드는 반투명 딤 처리
            using (SolidBrush dim = new SolidBrush(Color.FromArgb(70, 0, 0, 0)))
            {
                g.FillRectangle(dim, ClientRectangle);
            }

            // 공용 공지 알림창 영역 좌표 연산 데이터 로드
            Rectangle introNotice = SystemWindowUI.Shared.GetStandardNoticeRect(ClientSize);

            // 3. 🌟 고정 문자열 "System Intro"를 전면 폐기하고, 실제 대사인 'introBody' 변수를 정확하게 매핑하여 전달합니다.
            SystemWindowUI.Shared.DrawAssistantNotice(
                g,
                introNotice,
                title,       // 데이터에서 가져온 진짜 타이틀 (예: Windows Recovery Assistant)
                introBody,   // 💡 데이터에서 가져온 진짜 문장 문구 (안녕하세요. 저는... 복구 과정을 안내하는...)
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
            Rectangle win = SystemWindowUI.Shared.GetStandardNoticeRect(ClientSize);
            SystemWindowUI.Shared.DrawProfileSetupWindow(g, win, profileInput, NpcMood.Happy, Environment.TickCount / 30, buttons);
        }

        // =============================================================================
        // 🌟 [교정]: 컨텍스트에 존재하지 않던 clearedStages 변수를 player.ClearedStages로 정밀 동기화
        // =============================================================================
        private void DrawDesktop(Graphics g)
        {
            // 1. Bliss 바탕화면 순정 벽지를 도화지에 그려줍니다.
            DesktopBackgroundUI.Shared.Draw(g, ClientRectangle);

            // 2. 내 컴퓨터, 파일, 인터넷 익스플로러, 휴지통 고정 숏컷 인쇄
            if (DesktopIconUI.Shared != null)
            {
                DesktopIconUI.Shared.DrawFixedDesktopIcons(g, ClientRectangle);
            }

            // 3. 💡 [핵심 복구]: 존재하지 않던 변수를 올바른 인스턴스 멤버인 'player.ClearedStages'로 정확히 바인딩합니다!
            if (DesktopIconUI.Shared != null && stages != null && player != null)
            {
                DesktopIconUI.Shared.DrawStageIcons(
                    g,
                    stages,
                    unlockedStage,
                    selectedStage,
                    player.ClearedStages, // 🌟 컨텍스트 탈출 오류 완벽 컷!
                    buttons
                );
            }

            // 4. 우측 정보창 패널 내부에 실시간 코인 수치 연동 상점 숏컷 배치
            if (DesktopIconUI.Shared != null && player != null)
            {
                DesktopIconUI.Shared.DrawRecoveryToolsShortcut(
                    g,
                    ClientRectangle,
                    player.Coins,
                    buttons
                );
            }

            // 5. 정보 상태창 전체 프레임 마킹 및 하단 윈도우 작업표시줄 최종 렌더링
            DrawDesktopInfoPanel(g);
            TaskbarUI.Shared.Draw(g, ClientRectangle);
        }

        private void DrawShop(Graphics g)
        {
            var oldHint = g.TextRenderingHint;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            RecoveryToolsUI.Shared.DrawShop(g, ClientRectangle, player, selectedShopItem, buttons);
            g.TextRenderingHint = oldHint;
        }

        private void DrawDesktopInfoPanel(Graphics g) => RecoveryToolsUI.Shared.DrawDesktopStatusPanel(g, ClientRectangle, player, unlockedStage, stages.Count);

        private void DrawStage(Graphics g)
        {
            StageInfo st = stages[currentStage - 1];
            int mapWidth = GetStageMapWidth(st);

            if (stageBossPhase)
            {
                bool shouldShake = false;
                GameEntity currentBoss = enemies.Find(e => e.IsBoss && e.Hp > 0);
                if (currentBoss != null)
                {
                    if (currentBoss.Name.Contains("Driver-K") && bossManager.IsResourcePatternActive) shouldShake = true;
                    if (currentBoss.Name.Contains("High-Kernel") && (bossManager.IsAccessDeniedActive || bossManager.IsEnrageActive)) shouldShake = true;
                    if ((currentBoss.Name.Contains("Exception Queen") || currentBoss.Name.Contains("Exception_Queen")) && (bossManager.IsNullRefActive || bossManager.IsStackOverflowActive)) shouldShake = true;
                }
                if (shouldShake) g.TranslateTransform(random.Next(-4, 5), random.Next(-4, 5));
            }

            Renderer.DrawStageBackground(g, ClientRectangle, st, cameraX, stageBossPhase, mapWidth);

            if (!stageBossPhase)
            {
                using (Font f = Renderer.F(10f, FontStyle.Bold))
                using (SolidBrush bg = new SolidBrush(Color.FromArgb(130, 0, 0, 0)))
                {
                    Rectangle top = new Rectangle(250, 14, ClientSize.Width - 500, 44);
                    g.FillRectangle(bg, top);
                    g.DrawString($"STAGE {st.Index:00} | {st.Name} | {st.Objective}", f, Brushes.White, top, Renderer.Center());
                }
            }

            DrawHud(g, st);
            if (IsStageNpcHintOpen()) { DrawStageNpcHint(g, st); return; }

            foreach (GameEntity m in enemies) if (m.Hp > 0) Renderer.DrawEnemy(g, m, cameraX);
            for (int i = 0; i < weaponDrops.Count; i++) Renderer.DrawWeaponUpgradeFile(g, weaponDrops[i], cameraX);

            if (stageBossPhase)
            {
                DrawCustomBossGimmicks(g);
                GameEntity currentBoss = enemies.Find(e => e.IsBoss);
                if (currentBoss != null) Renderer.DrawBossGlobalUI(g, currentBoss, ClientSize);
            }

            bool playerMovingNow = Math.Abs(player.MoveVelocityX) > 0.25f || Math.Abs(player.MoveVelocityY) > 0.25f;
            float pivotX = player.X - cameraX;

            Renderer.DrawRecoveryProgram(g, player, true, cameraX, playerMovingNow);

            if (playerShield > 0)
            {
                int shieldSize = 100;
                float shieldX = pivotX - (shieldSize / 2);
                float shieldY = player.Y - (shieldSize / 2);

                if (Renderer.Img_SkillBarrier != null)
                {
                    using (var imageAttributes = new System.Drawing.Imaging.ImageAttributes())
                    {
                        float[][] colorMatrixElements = {
                            new float[] {1, 0, 0, 0, 0},
                            new float[] {0, 1, 0, 0, 0},
                            new float[] {0, 0, 1, 0, 0},
                            new float[] {0, 0, 0, 0.35f, 0},
                            new float[] {0, 0, 0, 0, 1}
                        };
                        imageAttributes.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(colorMatrixElements));
                        g.DrawImage(Renderer.Img_SkillBarrier, new Rectangle((int)shieldX, (int)shieldY, shieldSize, shieldSize), 0, 0, Renderer.Img_SkillBarrier.Width, Renderer.Img_SkillBarrier.Height, GraphicsUnit.Pixel, imageAttributes);
                    }
                }
                g.DrawString($"SHIELD: {playerShield}", Renderer.F(8.5f, FontStyle.Bold), Brushes.Cyan, pivotX - 35, player.Y - 55);
            }

            foreach (var sword in playerSkySwords)
            {
                float progress = 1.0f - ((float)sword.Timer / sword.MaxTimer);
                float currentSwordY = (sword.Y - 400f) + (400f * progress);

                if (Renderer.Img_SwordCold != null)
                {
                    GraphicsState swordState = g.Save();
                    g.TranslateTransform(sword.X - cameraX, currentSwordY);
                    g.ScaleTransform(1.5f, 1.5f);
                    g.RotateTransform(progress * 720f);
                    g.DrawImage(Renderer.Img_SwordCold, -Renderer.Img_SwordCold.Width / 2, -Renderer.Img_SwordCold.Height / 2);
                    g.Restore(swordState);
                }
            }

            for (int i = 0; i < effects.Count; i++) Renderer.DrawEffect(g, effects[i], cameraX);

            if (showStageClearPopup)
            {
                int winW = 500; int winH = 260;
                int winX = (ClientSize.Width - winW) / 2;
                int winY = (ClientSize.Height - winH) / 2;
                if (Renderer.Img_AlarmBg != null) g.DrawImage(Renderer.Img_AlarmBg, winX, winY, winW, winH);

                using (Font mainFont = Renderer.F(15f, FontStyle.Bold))
                using (Font subFont = Renderer.F(11f, FontStyle.Regular))
                {
                    g.DrawString("STAGE CLEAN 리포트 전송 완료", subFont, Brushes.White, winX + 12, winY + 8);
                    g.DrawString($"STAGE {currentStage} SYSTEM COMPLETED!", mainFont, Brushes.MidnightBlue, winX + winW / 2, winY + 45, Renderer.Center());

                    string reportText = $"▶ 이진 가비지 데이터 소멸 완료\r\n▶ 업그레이드 인덱스 파일 : [CORE_UPGRADE.bin]\r\n▶ 복구 공헌도 추가 보상 : +{popupBonusCoins} COINS";
                    if (currentStage == 2) reportText += "\r\n\r\n★ NEW SKILL UNLOCKED: [W 키] 오버클럭 해금!";
                    else if (currentStage == 5) reportText += "\r\n\r\n★ NEW SKILL UNLOCKED: [E 키] 데이터실드 해금!";
                    else if (currentStage == 8) reportText += "\r\n\r\n★ NEW SKILL UNLOCKED: [R 키] 시스템콜 해금!";

                    Rectangle reportBoxRect = new Rectangle(winX + 55, winY + 105, winW - 90, 110);
                    using (StringFormat sf = new StringFormat())
                    {
                        sf.Alignment = StringAlignment.Near; sf.LineAlignment = StringAlignment.Near;
                        sf.FormatFlags = StringFormatFlags.NoClip; sf.Trimming = StringTrimming.None;
                        g.DrawString(reportText, subFont, Brushes.Black, reportBoxRect, sf);
                    }
                }

                popupConfirmBtnBounds = new Rectangle((winX + winW / 2) - 60, winY + winH - 55, 120, 34);
                if (Renderer.Img_PopupBtn != null) g.DrawImage(Renderer.Img_PopupBtn, popupConfirmBtnBounds);
                g.DrawString("확인 (OK)", Renderer.F(10f, FontStyle.Bold), Brushes.Black, popupConfirmBtnBounds, Renderer.Center());
            }
        }

        private void DrawStage1BossPatternOverlay(Graphics g) => bossRuntime.DrawOverlay(g, currentStage, stageBossPhase, cameraX, ClientSize);

        private void DrawHud(Graphics g, StageInfo st)
        {
            Rectangle h = new Rectangle(ClientSize.Width - 368, 18, 350, 240);
            SystemWindowUI.Shared.DrawBlueHudFrame(g, h, "Recovery Program");

            using (Font f = Renderer.F(8.5f, FontStyle.Bold))
            {
                g.DrawString("Profile: " + player.ProfileName, f, Brushes.Black, new Rectangle(h.X + 22, h.Y + 48, h.Width - 44, 16), Renderer.LeftMiddle());
                g.DrawString($"Level: {player.Level}   Weapon: +{player.WeaponLevel}   Coin: {player.Coins}", f, Brushes.Navy, new Rectangle(h.X + 22, h.Y + 68, h.Width - 44, 16), Renderer.LeftMiddle());
            }

            int barX = h.X + 90; int barW = h.Width - 112;
            Renderer.DrawBar(g, new Rectangle(barX, h.Y + 105, barW, 11), player.Hp, player.MaxHp, Color.LimeGreen);
            Renderer.DrawBar(g, new Rectangle(barX, h.Y + 127, barW, 11), player.Mp, player.MaxMp, Color.DeepSkyBlue);
            Renderer.DrawBar(g, new Rectangle(barX, h.Y + 149, barW, 11), player.SystemStability, 100, Color.CornflowerBlue);

            int colX = h.X + 185;
            using (Font sF = Renderer.F(7.8f, FontStyle.Bold))
            {
                string wText = player.ClearedStages >= 2 ? (wCooldownTicks > 0 ? $"{(wCooldownTicks / 60f):0.0}s" : "READY") : "LOCKED";
                g.DrawString($"[W 오버클럭] {wText}", sF, Brushes.DarkGreen, colX, h.Y + 48);
                string eText = player.ClearedStages >= 5 ? (eCooldownTicks > 0 ? $"{(eCooldownTicks / 60f):0.0}s" : "READY") : "LOCKED";
                g.DrawString($"[E 데이터실] {eText}", sF, Brushes.DeepSkyBlue, colX, h.Y + 68);
            }
        }

        // =============================================================================
        // 🌟 [교정]: 초반 스테이지 진입 시 NPC 힌트 대사 문구 먹통 현상 전면 처단 복구 격실
        // =============================================================================
        private void DrawStageNpcHint(Graphics g, StageInfo st)
        {
            if (st == null) return;

            // 1. 공용 가이드 다이얼로그 상자의 XP 순정 규격 영역 좌표를 연산합니다.
            Rectangle r = SystemWindowUI.Shared.GetStandardNoticeRect(ClientSize);

            // 2. 💡 [핵심 데이터 동기화]: 대사 엔진(NpcDialogueData) 격실로부터 
            // 현재 활성화된 스테이지 번호(st.Index)에 귀속된 진짜 가이드 대사 본문을 추출합니다.
            string hintTitle = $"Stage {st.Index:00} - 시스템 가이드 안내";
            
            // 만약 NpcDialogueData 내에 별도의 함수명(예: GetStageHint)이 배정되어 있다면 매핑 흐름에 맞춰 연동됩니다.
            // 본 소스코드 환경에 안전하게 안착하도록 st.Objective 또는 대사집 데이터를 유기적으로 바인딩합니다.
            string stageHintBody = NpcDialogueData.GetIntroMessage(st.Index + 10) ?? $"현재 구역 [{st.Name}] 복구를 가동합니다.\n목표: {st.Objective}";
            
            if (st.Index == 1)
            {
                stageHintBody = "안녕하세요, 에이전트님! 시스템 복구 작전 구역에 진입하셨습니다.\n" +
                                "주변의 이진 가비지 파일(Security_Firewall)들이 시스템을 오염시키고 있으니\n" +
                                "[마우스 클릭]으로 이동하고 [Q 키: Quick Scan] 명령으로 소멸시키세요!";
            }

            // 3. 🌟 고정 문자열 "System Analysing..."을 영구 처단하고, 추출된 진짜 대사 변수를 주입합니다!
            SystemWindowUI.Shared.DrawAssistantNotice(
                g,
                r,
                hintTitle,      // 상단바에 출력될 진짜 타이틀 명 명세
                stageHintBody,  // 💡 조수 옆 텍스트 박스에 타자기 이펙트로 흩뿌려질 진짜 NPC 대사 본문 문구
                NpcMood.Basic,  // 조수의 기본 컴퓨터 모니터 감정 상태 유지
                Environment.TickCount / 30,
                buttons,
                "npcHintClose", // 확인 버튼 누를 시 알림창을 닫는 액션 ID 연동
                "npcHintClose"
            );
        }

        private void DrawStageClear(Graphics g)
        {
            Renderer.DrawXPWallpaper(g, ClientRectangle);
            Rectangle clearNotice = SystemWindowUI.Shared.GetStandardNoticeRect(ClientSize);
            SystemWindowUI.Shared.DrawAssistantNotice(g, clearNotice, "System Info", "Stage Clear Completed.", NpcMood.Happy, Environment.TickCount / 30, buttons, "clearNext", "clearNext");
        }

        private void DrawFinalInput(Graphics g)
        {
            Rectangle win = new Rectangle(ClientSize.Width / 2 - 400, ClientSize.Height / 2 - 200, 800, 400);
            Renderer.DrawXPWindow(g, win, "Final Input Process", true);
            // 💡 [CS0103 오류 교정 완료]: MainForm 고유 멤버인 Renderer.DrawButton 대신 전역 DrawButton 연계 매핑
            DrawButton(g, new Rectangle(win.Right - 150, win.Bottom - 50, 120, 32), "입력", true);
        }

        private void DrawEnding(Graphics g)
        {
            Renderer.DrawXPWallpaper(g, ClientRectangle);
            Rectangle win = new Rectangle(ClientSize.Width / 2 - 400, ClientSize.Height / 2 - 160, 800, 320);
            Renderer.DrawXPWindow(g, win, "System Terminated", false);
        }

        private void DrawHelp(Graphics g)
        {
            Renderer.DrawXPWallpaper(g, ClientRectangle);
            Rectangle win = new Rectangle(ClientSize.Width / 2 - 400, ClientSize.Height / 2 - 200, 800, 400);
            Renderer.DrawXPWindow(g, win, "Help Documentation", false);
        }

        private void DrawCustomBossGimmicks(Graphics g)
        {
            if (bossManager.IsAccessDeniedActive) g.DrawString("!!! WARNING: ACCESS DENIED !!!", Renderer.F(12f, FontStyle.Bold), Brushes.Red, ClientSize.Width / 2 - 140, 100);

            foreach (var sm in bossManager.SkyMissiles)
            {
                float sPosX = sm.X - cameraX;
                float missileY = sm.Y - 500 + (1f - (sm.Timer / 60f)) * 500;
                if (Renderer.Img_Meteor != null) g.DrawImage(Renderer.Img_Meteor, sPosX - 24, missileY - 24, 48, 48);
            }
            foreach (var p in bossManager.Projectiles)
            {
                float sPosX = p.X - cameraX;
                if (p.IsEnrageMissile && Renderer.Img_Meteor != null) g.DrawImage(Renderer.Img_Meteor, sPosX - 24, p.Y - 24, 48, 48);
            }
        }

        // 💡 [CS0103 완벽 처단]: MainForm 하단 및 단독 윈도우 팝업용 그리기 함수 철자 원본 복구 연동
        private void DrawButton(Graphics g, Rectangle r, string text, bool selected)
        {
            Renderer.DrawButton(g, r, text, selected);
        }

        private void DrawAdminStartMenu(Graphics g)
        {
            StartMenuScreen.Draw(g, ClientRectangle, buttons, GameSaveSystem.HasSave());
        }
    }
}