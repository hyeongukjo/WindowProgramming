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

                    // 3. R 스킬 해금 상태 디스플레이
                    string rText = player.ClearedStages >= 8 ? (rCooldownTicks > 0 ? $"{(rCooldownTicks / 60.0f):0.0}초" : "READY") : "LOCKED";
                    Brush rBrush = player.ClearedStages >= 8 ? (rCooldownTicks > 0 ? Brushes.Tomato : Brushes.DarkViolet) : Brushes.DarkGray;
                    g.DrawString($"[R 시스템콜] {rText}", skillFont, rBrush, rightColumnX, h.Y + 92);
                }
                string coolMeter = $"W 쿨: {(wCooldownTicks > 0 ? (wCooldownTicks / 60.0f).ToString("0.0") + "초" : "READY")} | " +
                                   $"E 쿨: {(eCooldownTicks > 0 ? (eCooldownTicks / 60.0f).ToString("0.0") + "초" : "READY")} | " +
                                   $"R 쿨: {(rCooldownTicks > 0 ? (rCooldownTicks / 60.0f).ToString("0.0") + "초" : "READY")}";

                g.DrawString(
                    coolMeter,
                    f,
                    b,
                    new Rectangle(h.X + 22, h.Y + 206, h.Width - 44, 18),
                    Renderer.LeftMiddle()
                );
            }
        }

        // =============================================================================
        // 🌟 [최종 완결]: 전 스테이지(1~10) 안내 대사 정밀 하드코딩 매핑 및 덮어쓰기 격실
        // =============================================================================
        private void DrawStageNpcHint(Graphics g, StageInfo st)
        {
            if (st == null) return;

            // 1. 공용 가이드 알림창 상자의 XP 순정 영역 좌표를 연산합니다.
            Rectangle r = SystemWindowUI.Shared.GetStandardNoticeRect(ClientSize);

            // 2. 타이틀바 텍스트 구조 정의
            string hintTitle = $"Stage {st.Index:00} - 시스템 가이드 안내";

            // 3. 💡 [전체 스테이지 대사집 연동]: st.Index(1~10)에 맞춰 고유 기획 문구를 매핑합니다.
            string stageHintBody = "";

            switch (st.Index)
            {
                case 1:
                    stageHintBody = "안녕하세요, 에이전트님! 시스템 복구 작전 구역에 진입하셨습니다.\n" +
                                    "주변의 이진 가비지 파일(Security_Firewall)들이 시스템을 오염시키고 있으니\n" +
                                    "[마우스 클릭]으로 이동하고 [Q 키: Quick Scan] 명령으로 소멸시키세요!";
                    break;

                case 2:
                    stageHintBody = $"STAGE {st.Index:00}  {st.Name}\n\n" +
                                    "이곳은 임시 저장소 구역입니다. 버려진 찌꺼기 프로세스들이 메모리를 점유하고 있군요.\n" +
                                    "가비지 데이터들을 모두 청소하고 시스템 캐시를 비워주세요.\n" +
                                    "★ 이번 스테이지를 클리어하면 새로운 기술 [W 키: 오버클럭]이 해금됩니다!";
                    break;

                case 3:
                    stageHintBody = $"STAGE {st.Index:00}  {st.Name}\n\n" +
                                    "주의하세요! 악성 스크립트 파일들이 디렉터리 경로를 무단으로 변경하고 있습니다.\n" +
                                    "오염된 인덱스 파일들을 찾아 격리 조치하고, 시스템 무결성을 확보해 주십시오.";
                    break;

                case 4:
                    stageHintBody = $"STAGE {st.Index:00}  {st.Name}\n\n" +
                                    "커널 동기화 격실에 도달했습니다. 시스템 버스가 비정상적인 신호로 가득 차 있습니다.\n" +
                                    "화면 곳곳에서 튀어 나오는 널 참조 예외(NullReference) 탄환들을 정밀하게 회피하며\n" +
                                    "보안 프로토콜을 가동하세요!";
                    break;

                case 5:
                    // 🎯 [image_1dedbf.jpg 순정 사양 완벽 안착]
                    stageHintBody = $"STAGE {st.Index:00}  {st.Name}\n\n" +
                                    $"{st.Name}에 진입했습니다.\n" +
                                    "현재 여러 개의 외부 연결이 감지되고 있습니다.\n" +
                                    "불필요한 연결은 시스템 안정성을 위해 차단하는 것이 좋습니다.";
                    break;

                case 6:
                    stageHintBody = $"STAGE {st.Index:00}  {st.Name}\n\n" +
                                    "파일 시스템 분할 영역입니다. 디스크 단편화 현상이 극도에 달해 시스템이 느려지고 있습니다.\n" +
                                    "가비지 컬렉터 프로그램이 정상 작동할 수 있도록 변조된 섹터들을 정화하십시오.";
                    break;

                case 7:
                    stageHintBody = $"STAGE {st.Index:00}  {st.Name}\n\n" +
                                    "위험 구역입니다! 악성 드라이버 좀비 프로세스들이 복구 에이전트를 요격하려 합니다.\n" +
                                    "적들의 하이퍼 투사체 세례를 상체 피격 판정 상자로 유연하게 회피하며\n" +
                                    "중앙 제어 장치로 가는 길을 개척하세요.";
                    break;

                case 8:
                    stageHintBody = $"STAGE {st.Index:00}  {st.Name}\n\n" +
                                    "레지스트리 하이브 내부 격실입니다. 잘못된 키 값들이 시스템을 붕괴시키고 있습니다.\n" +
                                    "변조된 고스트 키(Ghost_Key)들을 모조리 소멸시키고 연쇄 오류를 차단해 주십시오.\n" +
                                    "★ 이번 스테이지를 클리어하면 궁극기 [R 키: 시스템콜]이 활성화됩니다!";
                    break;

                case 9:
                    stageHintBody = $"STAGE {st.Index:00}  {st.Name}\n\n" +
                                    "최종 방화벽 관문 앞입니다. 메인 커널을 오염시킨 흑막의 데이터 오라가 뿜어져 나오고 있습니다.\n" +
                                    "시스템 안정성이 0%가 되기 전에 밀려오는 가비지 군단을 저지하고 에이전트의 한계를 시험하세요.";
                    break;

                case 10:
                    stageHintBody = $"STAGE {st.Index:00}  {st.Name} [FINAL ZONE]\n\n" +
                                    "에이전트님, 드디어 최종 코어 격실인 루트 디렉터리에 도달했습니다.\n" +
                                    "모든 버그와 파란 화면(BSOD)의 근원이 전방에서 대기하고 있습니다.\n" +
                                    "지금까지 업그레이드한 백신 파일과 오버클럭 스킬을 총동원하여 코어를 구원해 주십시오!";
                    break;

                default:
                    // 예외 방어용 기본 출력 포맷
                    stageHintBody = $"현재 구역 [{st.Name}] 복구 공정을 개시합니다.\n" +
                                    $"시스템 관리자가 지정한 보안 프로토콜을 활성화해 주세요.\n\n" +
                                    $"▶ 작전 목표: {st.Objective}";
                    break;
            }

            // 4. 복구 완료된 진짜 전용 대사 변수(stageHintBody)를 공용 안내창 모듈로 인젝션합니다.
            SystemWindowUI.Shared.DrawAssistantNotice(
                g,
                r,
                hintTitle,
                stageHintBody,  // 🌟 인덱스 충돌 없이 10개 스테이지 전체 대사 출력 보장!
                NpcMood.Basic,
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