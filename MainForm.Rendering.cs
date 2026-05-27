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

            string[] text = new string[]
            {
                "안녕하세요!\nWindows Recovery Assistant입니다.\n현재 바탕화면에 정리되지 않은 파일 개체가 많아 보여요.\n걱정하지 마세요. 제가 옆에서 도와드릴게요!",
                "먼저 간단한 복구 테스트를 시작해볼까요?\n위험한 건 아니에요.\n가벼운 정리 작업이라고 생각하시면 됩니다!",
                "복구 작업을 시작하기 전에\n프로필 이름을 설정해주세요.\n이 이름은 복구 기록과 진행 상황을 저장하는 데 사용됩니다."
            };
            string title = introIndex < 2 ? "Windows Recovery Assistant" : "Recovery Profile Setup";
            using (SolidBrush dim = new SolidBrush(Color.FromArgb(70, 0, 0, 0))) g.FillRectangle(dim, ClientRectangle);
            Rectangle introNotice = SystemWindowUI.Shared.GetStandardNoticeRect(ClientSize);
            SystemWindowUI.Shared.DrawAssistantNotice(
                g,
                introNotice,
                title,
                text[Math.Min(introIndex, text.Length - 1)],
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
                    "Recovery Program이 생성되었습니다.\n바탕화면에 보이는 파일 바로가기를 실행해 복구 절차를 진행하세요.\n아직 보이지 않는 파일은 이전 스테이지를 완료해야 생성됩니다.",
                    NpcMood.Basic,
                    Environment.TickCount / 30,
                    buttons,
                    "desktopNoticeOk",
                    "desktopNoticeClose"
                );
            }
            TaskbarUI.Shared.Draw(g, ClientRectangle);
        }
        /*
        Renderer.DrawXPWallpaper(g, ClientRectangle);
        Renderer.DrawXPTaskbar(g, ClientRectangle, "Windows XP Desktop - File Dungeon Shortcuts");
        using (Font f = Renderer.F(11f, FontStyle.Bold))
        using (SolidBrush b = new SolidBrush(Color.White))
        using (SolidBrush bg = new SolidBrush(Color.FromArgb(130, 0, 0, 0)))
        {
            Rectangle header = new Rectangle(120, 20, 610, 42);
            g.FillRectangle(bg, header);
            g.DrawString("문서 고정 진행: 열리지 않은 파일 던전은 보이지 않고, 클리어 후 새 바로가기가 생성됩니다.", f, b, header, Renderer.Center());
        }
        int cols = 5;
        int startX = 120;
        int startY = 90;
        for (int i = 1; i <= unlockedStage && i <= stages.Count; i++)
        {
            int col = (i - 1) % cols;
            int row = (i - 1) / cols;
            Rectangle r = new Rectangle(startX + col * 170, startY + row * 145, 128, 112);
            StageInfo st = stages[i - 1];
            bool sel = selectedStage == i;
            bool newly = i == unlockedStage && i > player.ClearedStages;
            Renderer.DrawFileShortcut(g, r, st, sel, newly);
            buttons.Add(new UiButton(r, "stage" + i.ToString()));
        }
        DrawDesktopInfoPanel(g);
        DrawRecycleBinShopShortcut(g);
        if (firstDesktopNotice)
        {
            using (SolidBrush dim = new SolidBrush(Color.FromArgb(68, 0, 0, 0))) g.FillRectangle(dim, ClientRectangle);
            Rectangle notice = new Rectangle(ClientSize.Width / 2 - 330, ClientSize.Height / 2 - 145, 660, 290);
            Renderer.DrawNotification(g, notice, "NPC_404_DESKTOP_NOTICE.exe - 확인 필요", "Recovery Program이 생성되었습니다.\n바탕화면에 보이는 파일 바로가기를 실행해 복구 절차를 진행하세요.\n아직 보이지 않는 파일은 이전 스테이지를 완료해야 생성됩니다.", NpcMood.Basic, true);
            buttons.Add(new UiButton(NotificationOkRect(notice), "desktopNoticeOk"));
        }*/


        /*
        private void DrawRecycleBinShopShortcut(Graphics g)
        {
            Rectangle r = new Rectangle(120, ClientSize.Height - 170, 150, 116);
            Renderer.DrawShopShortcut(g, r, player.Coins);
            buttons.Add(new UiButton(r, "openShop"));
        }*/

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

        //게임 진행상 불필요 할수도... 삭제??
        // 바탕화면 우측 상태 패널: 개발용 스테이지 설명 대신 실제 플레이 정보 표시
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
            Renderer.DrawStageBackground(g, ClientRectangle, st, cameraX, stageBossPhase, mapWidth);
            using (Font f = Renderer.F(10f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.White))
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(130, 0, 0, 0)))
            {
                Rectangle top = new Rectangle(250, 14, ClientSize.Width - 500, 48);
                g.FillRectangle(bg, top);
                string stageTitle = "STAGE " + st.Index.ToString("00") + "  " + st.Name + "  |  " + st.Objective;
                if (stageBossPhase) stageTitle = "STAGE " + st.Index.ToString("00") + "  보스방  |  " + st.BossName + " 격리 작전";
                g.DrawString(stageTitle, f, b, top, Renderer.Center());
            }
            DrawHud(g, st);
            foreach (GameEntity m in enemies) if (m.Hp > 0) Renderer.DrawEnemy(g, m, cameraX);
            for (int i = 0; i < weaponDrops.Count; i++) Renderer.DrawWeaponUpgradeFile(g, weaponDrops[i], cameraX);
            bossRuntime.DrawOverlay(g, currentStage, stageBossPhase, cameraX, ClientSize);
            bool playerMovingNow = Math.Abs(player.TargetX - player.X) > 3.5f || Math.Abs(player.TargetY - player.Y) > 3.5f ||
                                   Math.Abs(player.MoveVelocityX) > 0.25f || Math.Abs(player.MoveVelocityY) > 0.25f;
            Renderer.DrawRecoveryProgram(g, player, true, cameraX, playerMovingNow);
            for (int i = 0; i < effects.Count; i++) Renderer.DrawEffect(g, effects[i], cameraX);
            if (!stageNpcHintClosed) DrawStageNpcHint(g, st);
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
            string text = st.Dialogs[Math.Min(stageTime / 520, st.Dialogs.Length - 1)];
            string body = "STAGE " + st.Index.ToString("00") + "  " + st.Name + "\n\n" + text;
            SystemWindowUI.Shared.DrawAssistantNotice(
                g,
                r,
                "Windows Recovery Assistant",
                body,
                st.NpcMood,
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

            string body = BuildStageClearNoticeBody(st);

            Rectangle clearNotice = SystemWindowUI.Shared.GetStandardNoticeRect(ClientSize);

            SystemWindowUI.Shared.DrawAssistantNotice(
                g,
                clearNotice,
                "Windows Recovery Assistant",
                body,
                GetStageClearNpcMood(st),
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
                string text = "Stage 10 문서 지시사항:\nIllegal_Binny 처치 이후 진짜 갈등은 Windows Recovery Assistant와 최종 입력창으로 넘어갑니다.\n\n삭제할 프로세스 이름을 직접 입력하세요.\n- 복구 프로필 이름 입력: 진엔딩\n- 보스 이름 입력: 일반 엔딩\n- Windows Recovery Assistant 입력: Assistant 루프 엔딩\n- 빈칸/없는 이름: 잘못된 입력 엔딩";
                g.DrawString(text, f, b, new Rectangle(win.X + 215, win.Y + 62, win.Width - 250, 170), Renderer.Left());
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

    }
}
