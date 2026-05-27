using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace DebugHeroFileDungeonRPG
{
    // 배경 그리기 전담 클래스
    public static class BackgroundRenderer
    {
        // 에셋 이미지 변수들
        public static Image bootScreenImg = null;
        public static Image desktopBgImg = null;
        public static Image taskbarImg = null;
        public static Image startBtnImg = null;
        public static Image volumeIconImg = null;

        // 1. 배경 관련 리소스 초기화 (Assets 폴더에서 가져오기)
        public static void InitializeBackgrounds()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;

                string bootPath = Path.Combine(baseDir, "Assets", "Backgrounds", "Booting_screen.png");
                if (File.Exists(bootPath)) bootScreenImg = Image.FromFile(bootPath);

                string bgPath = Path.Combine(baseDir, "Assets", "Backgrounds", "stage01_background.png");
                if (File.Exists(bgPath)) desktopBgImg = Image.FromFile(bgPath);

                string barPath = Path.Combine(baseDir, "Assets", "UI", "taskbar.png");
                if (File.Exists(barPath)) taskbarImg = Image.FromFile(barPath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("배경/UI 리소스 로드 실패: " + ex.Message);
            }
        }

        // 2. 부팅 화면 그리기 로직
        public static void DrawBootScreen(Graphics g, int w, int h, int bootTicks)
        {
            g.Clear(Color.Black);

            if (bootScreenImg != null)
            {
                int imgW = bootScreenImg.Width;
                int imgH = bootScreenImg.Height;

                float ratio = Math.Min((float)w / imgW, (float)h / imgH);
                int drawW = (int)(imgW * ratio);
                int drawH = (int)(imgH * ratio);
                int drawX = (w - drawW) / 2;
                int drawY = (h - drawH) / 2;

                g.DrawImage(bootScreenImg, drawX, drawY, drawW, drawH);

                float xWidthRatio = 0.23f;
                float yHeightRatio = 0.032f;
                float yPositionRatio = 0.72f;

                int barWidth = (int)(drawW * xWidthRatio);
                int barHeight = (int)(drawH * yHeightRatio);
                int barX = drawX + (drawW - barWidth) / 2;
                int barY = drawY + (int)(drawH * yPositionRatio);

                using (Pen trackPen = new Pen(Color.White, 2f))
                {
                    g.DrawRectangle(trackPen, barX, barY, barWidth, barHeight);
                }

                int blockWidth = Math.Max(6, barWidth / 13);
                int spacing = 3;
                int speed = 5;

                int startX = (bootTicks * speed) % (barWidth + (blockWidth + spacing) * 3);
                startX -= (blockWidth + spacing) * 3;

                var oldSmoothing = g.SmoothingMode;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (SolidBrush blockBrush = new SolidBrush(Color.FromArgb(43, 142, 243)))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        int currentBlockX = barX + startX + (i * (blockWidth + spacing));
                        if (currentBlockX >= barX && currentBlockX + blockWidth <= barX + barWidth)
                        {
                            g.FillRectangle(blockBrush, currentBlockX, barY + 1, blockWidth, barHeight - 2);
                        }
                    }
                }
                g.SmoothingMode = oldSmoothing;
            }
            else
            {
                using (Font f = Renderer.F(42f, FontStyle.Bold))
                using (SolidBrush b = new SolidBrush(Color.White))
                    g.DrawString("Windows XP", f, b, new Rectangle(0, h / 2 - 100, w, 70), Renderer.Center());

                Rectangle bar = new Rectangle(w / 2 - 160, h / 2 + 30, 320, 22);
                using (Pen p = new Pen(Color.White)) g.DrawRectangle(p, bar);
                int fill = (bootTicks * 7) % (bar.Width - 20);
                using (SolidBrush b = new SolidBrush(Color.FromArgb(60, 150, 255))) g.FillRectangle(b, bar.X + 4 + fill, bar.Y + 4, 60, bar.Height - 8);
            }

            using (Font f = Renderer.F(10f, FontStyle.Regular))
                g.DrawString("프로그램 불러오는 중...", f, Brushes.LightGray, new Rectangle(0, h - 60, w, 22), Renderer.Center());
        }

        // 3. 바탕화면 배경과 작업 표시줄 UI를 동적으로 그리는 함수
        public static void DrawDesktopFramework(Graphics g, int w, int h)
        {
            // 바탕화면 배경 이미지 그리기
            if (desktopBgImg != null)
            {
                g.DrawImage(desktopBgImg, 0, 0, w, h);
            }
            else
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(0, 100, 200)))
                    g.FillRectangle(b, 0, 0, w, h);
            }

        }
    }
}