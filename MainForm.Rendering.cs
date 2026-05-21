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
        private void DrawDesktopInfoPanel(Graphics g)
        {
            Rectangle p = new Rectangle(ClientSize.Width - 400, 70, 370, 520);
            Renderer.DrawXPWindow(g, p, "파일 속성 / 복구 상태", false);
            StageInfo st = stages[Math.Max(0, selectedStage - 1)];
            using (Font f = Renderer.F(10f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.Navy))
                g.DrawString(st.FileName, f, b, new Rectangle(p.X + 20, p.Y + 48, p.Width - 40, 24), Renderer.LeftMiddle());
            using (Font f = Renderer.F(8.4f, FontStyle.Regular))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(42, 50, 68)))
            {
                string info = "스테이지: " + st.Name + "\n" +
                              "유형: " + (st.Kind == StageKind.Normal ? "일반" : st.Kind == StageKind.Boss ? "보스" : "최종") + "\n" +
                              "주요 배경: " + st.Background + "\n" +
                              "전투 공간: " + st.CombatSpace + "\n" +
                              "플레이어: " + st.PlayerCharacter + "\n" +
                              "NPC: " + st.Npc + "\n" +
                              "분위기: " + st.Mood + "\n\n" +
                              "문서 핵심 지시:\n" + st.MustKeep + "\n\n" +
                              "진행:\n" + st.Flow;
                g.DrawString(info, f, b, new Rectangle(p.X + 20, p.Y + 80, p.Width - 40, 312), Renderer.Left());
            }
            using (Font f = Renderer.F(8f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.DarkGreen))
                g.DrawString("해금된 파일: " + unlockedStage + " / " + stages.Count + "\n클리어: " + player.ClearedStages + " / " + stages.Count + "\n격리된 보스: " + player.QuarantinedBosses, f, b, new Rectangle(p.X + 20, p.Bottom - 106, p.Width - 40, 70), Renderer.Left());
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
            Rectangle h = new Rectangle(18, 18, 255, 218);
            Renderer.DrawXPWindow(g, h, "Recovery Program", false);
            using (Font f = Renderer.F(8.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(34, 42, 60)))
            {
                g.DrawString("Profile: " + player.ProfileName, f, b, new Rectangle(h.X + 16, h.Y + 46, h.Width - 32, 18), Renderer.LeftMiddle());
                g.DrawString("Program: Recovery Program", f, b, new Rectangle(h.X + 16, h.Y + 66, h.Width - 32, 18), Renderer.LeftMiddle());
                g.DrawString("Level: " + player.Level + "   Weapon: +" + player.WeaponLevel + "   Coin: " + player.Coins, f, b, new Rectangle(h.X + 16, h.Y + 86, h.Width - 32, 18), Renderer.LeftMiddle());
            }
            Renderer.DrawBar(g, new Rectangle(h.X + 82, h.Y + 114, 156, 12), player.Hp, player.MaxHp, Color.LimeGreen);
            Renderer.DrawBar(g, new Rectangle(h.X + 82, h.Y + 136, 156, 12), player.Mp, player.MaxMp, Color.DeepSkyBlue);
            Renderer.DrawBar(g, new Rectangle(h.X + 82, h.Y + 158, 156, 12), player.SystemStability, 100, Color.FromArgb(75, 150, 255));
            using (Font f = Renderer.F(7.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.Black))
            {
                g.DrawString("HP", f, b, h.X + 16, h.Y + 109);
                g.DrawString("MP", f, b, h.X + 16, h.Y + 131);
                g.DrawString("Stability", f, b, h.X + 16, h.Y + 153);
                g.DrawString("D: HP포션(" + player.HpPotions + ")   F: MP포션(" + player.MpPotions + ")", f, b, new Rectangle(h.X + 16, h.Y + 178, h.Width - 32, 18), Renderer.LeftMiddle());
                g.DrawString("마우스 클릭 이동 / 부드러운 추적 / Q W E R", f, b, new Rectangle(h.X + 16, h.Y + 196, h.Width - 32, 18), Renderer.LeftMiddle());
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
            Renderer.DrawXPTaskbar(g, ClientRectangle, "Stage Clear");
            StageInfo st = stages[clearStage - 1];
            string body = "복구 절차가 완료되었습니다.\n\n" + st.Name + " 클리어.\n";
            if (st.IsBossStage) body += "보스 개체 [" + st.BossName + "]는 완전히 삭제되지 않고 격리 기록으로 보관됩니다.\n";
            if (clearStage < stages.Count) body += "새 바로가기 생성: " + stages[clearStage].FileName + "\n";
            else body += "최종 입력 절차로 이동합니다.\n";
            body += "\n문서 고정 흐름:\n" + st.Flow;
            Rectangle clearNotice = SystemWindowUI.Shared.GetLargeNoticeRect(ClientSize);
            using (SolidBrush dim = new SolidBrush(Color.FromArgb(60, 0, 0, 0))) g.FillRectangle(dim, ClientRectangle);
            SystemWindowUI.Shared.DrawAssistantNotice(
                g,
                clearNotice,
                "Windows Recovery Assistant",
                body,
                st.NpcMood,
                Environment.TickCount / 30,
                buttons,
                "clearNext",
                null
            );
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
