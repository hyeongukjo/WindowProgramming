using System;
using System.Drawing;

namespace DebugHeroFileDungeonRPG
{
    public sealed partial class MainForm
    {
        private const string OldGoogleCloseKey = "oldGoogleClose";
        private const string OldGoogleSearchFocusKey = "oldGoogleSearchFocus";

        private bool showOldGoogleWindow = false;
        private bool oldGoogleSearchFocused = false;
        private string oldGoogleSearchText = "";
        private string oldGoogleLastQuery = "";

        // MainForm.cs 생성자에서 이미 호출 중이라 남겨두는 빈 함수
        private void InitializeOldGoogleWindow()
        {
        }

        private void OpenOldGoogleWindow()
        {
            showLeaderboardWindow = false;

            showOldGoogleWindow = true;
            oldGoogleSearchFocused = true;
            oldGoogleSearchText = "";
            oldGoogleLastQuery = "";

            TryBeep(880, 60);
            Invalidate();
        }

        private void CloseOldGoogleWindow()
        {
            showOldGoogleWindow = false;
            oldGoogleSearchFocused = false;

            TryBeep(750, 50);
            Invalidate();
        }

        private void UpdateOldGoogleWindowVisibility()
        {
            if (screen != ScreenMode.Desktop)
            {
                showOldGoogleWindow = false;
                oldGoogleSearchFocused = false;
            }

            Invalidate();
        }

        private bool IsOldGoogleWindowVisible()
        {
            return showOldGoogleWindow && screen == ScreenMode.Desktop && !firstDesktopNotice;
        }

        private Rectangle GetInternetExplorerIconBounds()
        {
            // DesktopIconUI.cs 기준:
            // Internet Explorer 아이콘: x = 30, y = 34 + 122 * 2
            return new Rectangle(0, 272, 120, 114);
        }

        private Rectangle GetOldGoogleWindowRect()
        {
            int winW = 820;
            int winH = 520;

            return new Rectangle(
                ClientSize.Width / 2 - winW / 2,
                82,
                winW,
                winH
            );
        }

        private Rectangle GetOldGoogleContentRect(Rectangle win)
        {
            return new Rectangle(
                win.X + 34,
                win.Y + 48,
                win.Width - 68,
                win.Height - 84
            );
        }

        private Rectangle GetOldGoogleSearchRect()
        {
            Rectangle win = GetOldGoogleWindowRect();
            Rectangle content = GetOldGoogleContentRect(win);

            // 검색 결과 화면: 왼쪽 Google 로고와 겹치지 않게 오른쪽으로 충분히 민다.
            if (!string.IsNullOrWhiteSpace(oldGoogleLastQuery))
            {
                return new Rectangle(
                    content.X + 190,
                    content.Y + 34,
                    content.Width - 230,
                    30
                );
            }

            // 첫 화면: 중앙 검색창
            return new Rectangle(
                content.X + content.Width / 2 - 230,
                content.Y + 220,
                460,
                32
            );
        }
        private bool IsOldGoogleEasterEggKeyword(string query)
        {
            string q = query.Trim().ToLowerInvariant();

            return
                q == "update" ||
                q == "driver" ||
                q == "system32" ||
                q == "network" ||
                q == "localhost" ||
                q == "bsod" ||
                q == "registry" ||
                q == "error" ||
                q == "cache" ||
                q == "recycle" ||
                q == "assistant";
        }

        private void SubmitOldGoogleSearch()
        {
            string query = oldGoogleSearchText.Trim();

            if (string.IsNullOrWhiteSpace(query))
            {
                TryBeep(320, 80);
                Invalidate();
                return;
            }

            oldGoogleLastQuery = query;
            oldGoogleSearchFocused = true;

            if (IsOldGoogleEasterEggKeyword(query))
                TryBeep(900, 55);
            else
                TryBeep(320, 80);

            Invalidate();
        }

        private void DrawOldGoogleWindow(Graphics g)
        {
            if (!IsOldGoogleWindowVisible())
                return;

            Rectangle win = GetOldGoogleWindowRect();

            using (SolidBrush dim = new SolidBrush(Color.FromArgb(45, 0, 0, 0)))
                g.FillRectangle(dim, ClientRectangle);

            SystemWindowUI.Shared.DrawSystemPanelFrame(
                g,
                win,
                "Internet Explorer - Google",
                SystemWindowStyle.Blue,
                true,
                buttons,
                OldGoogleCloseKey
            );
            Rectangle largeCloseHitBox = new Rectangle(
                win.Right - 92,
                win.Y + 2,
                82,
                34
            );

            buttons.Add(new UiButton(largeCloseHitBox, OldGoogleCloseKey));

            Rectangle content = GetOldGoogleContentRect(win);

            if (string.IsNullOrWhiteSpace(oldGoogleLastQuery))
                DrawOldGoogleHome(g, content);
            else
                DrawOldGoogleResult(g, content);
        }

        private void DrawOldGoogleHome(Graphics g, Rectangle content)
        {
            Rectangle logoArea = new Rectangle(
                content.X,
                content.Y + 82,
                content.Width,
                80
            );

            DrawGoogleLogoCentered(g, logoArea, 56f);

            Rectangle searchRect = GetOldGoogleSearchRect();
            DrawOldGoogleSearchBox(g, searchRect);

            using (Font guideFont = Renderer.F(10f, FontStyle.Regular))
            using (SolidBrush guideBrush = new SolidBrush(Color.FromArgb(95, 95, 95)))
            using (StringFormat center = new StringFormat())
            {
                center.Alignment = StringAlignment.Center;
                center.LineAlignment = StringAlignment.Center;

                Rectangle guideRect = new Rectangle(
                    content.X,
                    searchRect.Bottom + 18,
                    content.Width,
                    28
                );

                g.DrawString(
                    "검색어를 입력한 뒤 Enter를 누르세요.",
                    guideFont,
                    guideBrush,
                    guideRect,
                    center
                );
            }

            using (Font smallFont = Renderer.F(9f, FontStyle.Regular))
            using (SolidBrush smallBrush = new SolidBrush(Color.FromArgb(120, 120, 120)))
            using (StringFormat center = new StringFormat())
            {
                center.Alignment = StringAlignment.Center;
                center.LineAlignment = StringAlignment.Center;

                Rectangle cacheRect = new Rectangle(
                    content.X,
                    content.Bottom - 46,
                    content.Width,
                    24
                );

                g.DrawString(
                    "©2001 Google - cached local page",
                    smallFont,
                    smallBrush,
                    cacheRect,
                    center
                );
            }
        }


        private void DrawOldGoogleResult(Graphics g, Rectangle content)
        {
            DrawGoogleLogo(g, content.X + 24, content.Y + 18, 20f);

            Rectangle searchRect = GetOldGoogleSearchRect();
            DrawOldGoogleSearchBox(g, searchRect);

            using (Pen linePen = new Pen(Color.FromArgb(185, 185, 185)))
            {
                g.DrawLine(
                    linePen,
                    content.X + 12,
                    searchRect.Bottom + 18,
                    content.Right - 12,
                    searchRect.Bottom + 18
                );
            }

            string query = oldGoogleLastQuery.Trim();
            string q = query.ToLowerInvariant();

            using (Font infoFont = Renderer.F(9.5f, FontStyle.Regular))
            using (SolidBrush infoBrush = new SolidBrush(Color.FromArgb(80, 80, 80)))
            {
                g.DrawString(
                    "검색어: " + query,
                    infoFont,
                    infoBrush,
                    content.X + 24,
                    searchRect.Bottom + 30
                );
            }

            int resultY = searchRect.Bottom + 66;

            bool isKnownKeyword =
                q == "update" ||
                q == "driver" ||
                q == "system32" ||
                q == "network" ||
                q == "localhost" ||
                q == "bsod" ||
                q == "registry" ||
                q == "error" ||
                q == "cache" ||
                q == "recycle" ||
                q == "assistant";

            if (!isKnownKeyword)
            {
                DrawFakeSearchResult(
                    g,
                    content.X + 28,
                    resultY,
                    "검색 결과를 찾지 못했습니다.",
                    "Internet Explorer",
                    "입력한 검색어와 일치하는 로컬 캐시 항목이 없습니다."
                );

                using (Font guideFont = Renderer.F(9f, FontStyle.Regular))
                using (SolidBrush guideBrush = new SolidBrush(Color.FromArgb(95, 95, 95)))
                {
                    g.DrawString(
                        "다른 검색어를 입력하려면 검색창을 클릭하세요.  Enter: 검색 / Esc: 닫기",
                        guideFont,
                        guideBrush,
                        content.X + 24,
                        content.Bottom - 34
                    );
                }

                return;
            }

            if (q == "update")
            {
                DrawFakeSearchResult(
                    g,
                    content.X + 28,
                    resultY,
                    "Windows Update - Pending Restart",
                    "http://update.local/pending",
                    "다시 시작 알림이 연기된 기록이 있습니다. RestartReminder = Delayed"
                );

                resultY += 92;

                DrawFakeSearchResult(
                    g,
                    content.X + 28,
                    resultY,
                    "KB0003 설치 로그",
                    "C:\\WINDOWS\\SoftwareDistribution\\KB0003.log",
                    "업데이트 진행률이 여러 번 0%로 돌아간 기록이 저장되어 있습니다."
                );
            }
            else if (q == "driver")
            {
                DrawFakeSearchResult(
                    g,
                    content.X + 28,
                    resultY,
                    "Unknown Device_K",
                    "http://driver-vault.local/device",
                    "장치가 복구 명령을 거부했습니다. Driver-K 상태 기록이 남아 있습니다."
                );

                resultY += 92;

                DrawFakeSearchResult(
                    g,
                    content.X + 28,
                    resultY,
                    "Legacy Device",
                    "C:\\WINDOWS\\inf\\legacy_device.log",
                    "오래된 장치 정보가 드라이버 충돌 기록과 함께 묶여 있습니다."
                );
            }
            else if (q == "system32")
            {
                DrawFakeSearchResult(
                    g,
                    content.X + 28,
                    resultY,
                    "System32 Integrity Check",
                    "C:\\WINDOWS\\system32\\integrity.log",
                    "보호된 시스템 파일 접근 기록이 있습니다. AccessPermission = Allowed"
                );

                resultY += 92;

                DrawFakeSearchResult(
                    g,
                    content.X + 28,
                    resultY,
                    "High-Kernel 보호 절차",
                    "system://protected/high-kernel",
                    "요청 작업은 완료되었지만 일부 보호 절차가 강제로 해제되었습니다."
                );
            }
            else if (q == "network")
            {
                DrawFakeSearchResult(
                    g,
                    content.X + 28,
                    resultY,
                    "Windows Firewall Decision",
                    "http://firewall.local/decision",
                    "알 수 없는 연결 요청이 처리되었습니다. PortDecision = Blocked"
                );

                resultY += 92;

                DrawFakeSearchResult(
                    g,
                    content.X + 28,
                    resultY,
                    "Network Port Cache",
                    "C:\\WINDOWS\\Temp\\port_cache.tmp",
                    "차단된 연결 정보가 임시 저장소에 남아 있습니다."
                );
            }
            else if (q == "localhost")
            {
                DrawFakeSearchResult(
                    g,
                    content.X + 28,
                    resultY,
                    "localhost",
                    "http://localhost/",
                    "페이지를 표시할 수 없습니다. 로컬 주소의 응답 시간이 초과되었습니다."
                );

                resultY += 92;

                DrawFakeSearchResult(
                    g,
                    content.X + 28,
                    resultY,
                    "Local Session",
                    "http://localhost/session",
                    "LastSession = Not Closed Properly"
                );
            }
            else if (q == "bsod")
            {
                DrawFakeSearchResult(
                    g,
                    content.X + 28,
                    resultY,
                    "Blue Screen Dump",
                    "C:\\WINDOWS\\Minidump\\BSOD_dump.tmp",
                    "CrashRecovery = Interrupted. 시스템 안정성 검사 기록이 손상되었습니다."
                );

                resultY += 92;

                DrawFakeSearchResult(
                    g,
                    content.X + 28,
                    resultY,
                    "STOP: 0x0000007E",
                    "system://stop-error",
                    "Damage prevention mode was activated."
                );
            }
            else if (q == "registry")
            {
                DrawFakeSearchResult(
                    g,
                    content.X + 28,
                    resultY,
                    "Registry Editor - RecentActions",
                    "regedit://HKEY_CURRENT_USER/RecentActions",
                    "Stage03_UpdateReminder = Delayed / Stage05_PortDecision = Blocked"
                );

                resultY += 92;

                DrawFakeSearchResult(
                    g,
                    content.X + 28,
                    resultY,
                    "RecoveryProfile",
                    "regedit://HKEY_CURRENT_USER/RecoveryProfile",
                    "ProfileName = 입력한 이름 / LastSession = Not Closed Properly"
                );
            }
            else if (q == "error")
            {
                DrawFakeSearchResult(
                    g,
                    content.X + 28,
                    resultY,
                    "Unhandled Exception",
                    "C:\\WINDOWS\\Temp\\ExceptionQueen.err",
                    "닫힌 오류창이 복원 대기 상태로 남아 있습니다."
                );

                resultY += 92;

                DrawFakeSearchResult(
                    g,
                    content.X + 28,
                    resultY,
                    "Error Report",
                    "system://error-report/unsent",
                    "오류 보고서가 전송되지 않았습니다. ReportStatus = Unsent"
                );
            }
            else if (q == "cache")
            {
                DrawFakeSearchResult(
                    g,
                    content.X + 28,
                    resultY,
                    "UNSENT_REPORT.tmp",
                    "C:\\Documents and Settings\\...\\Local Settings\\Temp",
                    "닫힌 창은 사라지지 않았습니다. 임시 저장소에 보고서가 남아 있습니다."
                );

                resultY += 92;

                DrawFakeSearchResult(
                    g,
                    content.X + 28,
                    resultY,
                    "RecoveryProfile.cache",
                    "C:\\WINDOWS\\Temp\\RecoveryProfile.cache",
                    "ProfileName = 입력한 이름 / RecentCommand = Close, Delay, Allow, Block"
                );
            }
            else if (q == "recycle")
            {
                DrawFakeSearchResult(
                    g,
                    content.X + 28,
                    resultY,
                    "Recycle Bin Dungeon",
                    "recycle://deleted-items",
                    "삭제된 항목이 최종 정리 구역으로 이동했습니다."
                );

                resultY += 92;

                DrawFakeSearchResult(
                    g,
                    content.X + 28,
                    resultY,
                    "Illegal_Binny.dat",
                    "recycle://Illegal_Binny.dat",
                    "비워진 적 없는 항목이 너무 많습니다. 삭제된 것들은 보관될 뿐입니다."
                );
            }
            else if (q == "assistant")
            {
                DrawFakeSearchResult(
                    g,
                    content.X + 28,
                    resultY,
                    "Windows Recovery Assistant",
                    "assistant://recovery",
                    "복구 절차가 아직 종료되지 않았습니다."
                );

                resultY += 92;

                DrawFakeSearchResult(
                    g,
                    content.X + 28,
                    resultY,
                    "assistant_recovery.tmp",
                    "C:\\WINDOWS\\Temp\\assistant_recovery.tmp",
                    "종료하면 복구를 계속할 수 없습니다."
                );
            }

            using (Font guideFont = Renderer.F(9f, FontStyle.Regular))
            using (SolidBrush guideBrush = new SolidBrush(Color.FromArgb(95, 95, 95)))
            {
                g.DrawString(
                    "검색어를 수정하려면 검색창을 클릭하세요.  Enter: 검색 / Esc: 닫기",
                    guideFont,
                    guideBrush,
                    content.X + 24,
                    content.Bottom - 34
                );
            }
        }

        private void DrawOldGoogleSearchBox(Graphics g, Rectangle searchRect)
        {
            using (SolidBrush boxBrush = new SolidBrush(Color.White))
                g.FillRectangle(boxBrush, searchRect);

            Color borderColor = oldGoogleSearchFocused
                ? Color.FromArgb(0, 84, 227)
                : Color.FromArgb(120, 120, 120);

            using (Pen borderPen = new Pen(borderColor, oldGoogleSearchFocused ? 2f : 1f))
                g.DrawRectangle(borderPen, searchRect);

            string displayText = oldGoogleSearchText;

            if (oldGoogleSearchFocused && tick % 60 < 30)
                displayText += "_";

            using (Font inputFont = Renderer.F(12f, FontStyle.Regular))
            using (SolidBrush inputBrush = new SolidBrush(Color.Black))
            using (StringFormat leftMiddle = new StringFormat())
            {
                leftMiddle.Alignment = StringAlignment.Near;
                leftMiddle.LineAlignment = StringAlignment.Center;

                Rectangle textRect = new Rectangle(
                    searchRect.X + 8,
                    searchRect.Y + 1,
                    searchRect.Width - 16,
                    searchRect.Height - 2
                );

                g.DrawString(displayText, inputFont, inputBrush, textRect, leftMiddle);
            }

            buttons.Add(new UiButton(searchRect, OldGoogleSearchFocusKey));
        }

        private void DrawGoogleLogo(Graphics g, int x, int y, float fontSize)
        {
            string[] letters = { "G", "o", "o", "g", "l", "e" };
            Color[] colors =
            {
                Color.FromArgb(66, 133, 244),
                Color.FromArgb(219, 68, 55),
                Color.FromArgb(244, 180, 0),
                Color.FromArgb(66, 133, 244),
                Color.FromArgb(15, 157, 88),
                Color.FromArgb(219, 68, 55)
            };

            using (Font logoFont = Renderer.F(fontSize, FontStyle.Bold))
            {
                float drawX = x;

                for (int i = 0; i < letters.Length; i++)
                {
                    using (SolidBrush brush = new SolidBrush(colors[i]))
                        g.DrawString(letters[i], logoFont, brush, drawX, y);

                    SizeF size = g.MeasureString(letters[i], logoFont);
                    drawX += size.Width - fontSize * 0.14f;
                }
            }
        }
        private void DrawGoogleLogoCentered(Graphics g, Rectangle area, float fontSize)
        {
            string[] letters = { "G", "o", "o", "g", "l", "e" };
            Color[] colors =
            {
        Color.FromArgb(66, 133, 244),
        Color.FromArgb(219, 68, 55),
        Color.FromArgb(244, 180, 0),
        Color.FromArgb(66, 133, 244),
        Color.FromArgb(15, 157, 88),
        Color.FromArgb(219, 68, 55)
    };

            using (Font logoFont = Renderer.F(fontSize, FontStyle.Bold))
            {
                float totalW = 0f;

                for (int i = 0; i < letters.Length; i++)
                {
                    SizeF size = g.MeasureString(letters[i], logoFont);
                    totalW += size.Width - fontSize * 0.14f;
            }

                float drawX = area.X + area.Width / 2f - totalW / 2f;
                float drawY = area.Y;

                for (int i = 0; i < letters.Length; i++)
                {
                    using (SolidBrush brush = new SolidBrush(colors[i]))
                    {
                        g.DrawString(letters[i], logoFont, brush, drawX, drawY);
        }

                    SizeF size = g.MeasureString(letters[i], logoFont);
                    drawX += size.Width - fontSize * 0.14f;
                }
            }
        }
        private void DrawFakeSearchResult(Graphics g, int x, int y, string title, string url, string desc)
        {
            using (Font titleFont = Renderer.F(11f, FontStyle.Underline))
            using (Font urlFont = Renderer.F(9.5f, FontStyle.Regular))
            using (Font descFont = Renderer.F(9.5f, FontStyle.Regular))
            using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb(0, 0, 204)))
            using (SolidBrush urlBrush = new SolidBrush(Color.FromArgb(0, 128, 0)))
            using (SolidBrush descBrush = new SolidBrush(Color.FromArgb(45, 45, 45)))
            {
                g.DrawString(title, titleFont, titleBrush, x, y);
                g.DrawString(url, urlFont, urlBrush, x, y + 24);

                Rectangle descRect = new Rectangle(x, y + 46, 650, 40);
                g.DrawString(desc, descFont, descBrush, descRect);
            }
        }

    }
}