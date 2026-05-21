using System;
using System.Drawing;

namespace DebugHeroFileDungeonRPG
{
    public sealed class TaskbarUI
    {
        public static readonly TaskbarUI Shared = new TaskbarUI();

        public int Height { get; set; } = 56;

        public int StartX { get; set; } = -10;
        public int StartYAdjust { get; set; } = -3;
        public int StartWidth { get; set; } = 160;
        public int StartHeightAdjust { get; set; } = 6;

        public int VolumeSize { get; set; } = 34;
        public int VolumeRightMargin { get; set; } = 120;
        public int VolumeYAdjust { get; set; } = 11;

        public int ClockRightMargin { get; set; } = 82;
        public int ClockYAdjust { get; set; } = 18;
        private void DrawStartText(Graphics g, Rectangle taskbarDest, int barY, int barHeight)
        {
            // taskbar.png에서 시작 버튼 영역 비율 기준
            int startButtonWidth = (int)(taskbarDest.Width * 240f / 1628f);

            Rectangle textRect = new Rectangle(
                taskbarDest.X + 62,
                barY + 1,
                startButtonWidth - 72,
                barHeight - 2
            );

            var oldHint = g.TextRenderingHint;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

            using (Font font = Renderer.F(15.5f, FontStyle.Bold))
            using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(90, 0, 80, 0)))
            using (SolidBrush textBrush = new SolidBrush(Color.White))
            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                Rectangle shadowRect = new Rectangle(
                    textRect.X + 1,
                    textRect.Y + 1,
                    textRect.Width,
                    textRect.Height
                );

                g.DrawString("시작", font, shadowBrush, shadowRect, format);
                g.DrawString("시작", font, textBrush, textRect, format);
            }

            g.TextRenderingHint = oldHint;
        }

        public void Draw(Graphics g, Rectangle client)
        {
            int barHeight = 42;
            int barY = client.Bottom - barHeight;

            if (BackgroundRenderer.taskbarImg != null)
            {
                // 현재 taskbar.png 이미지에서 실제 작업표시줄 부분만 잘라서 사용
                Rectangle src = new Rectangle(
                    22,
                    428,
                    1628,
                    80
                );

                Rectangle dest = new Rectangle(
                    client.X - 7,
                    barY,
                    client.Width + 14,
                    barHeight
                );

                var oldMode = g.InterpolationMode;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                g.DrawImage(
                    BackgroundRenderer.taskbarImg,
                    dest,
                    src,
                    GraphicsUnit.Pixel
                );
                DrawStartText(g, dest, barY, barHeight);
                g.InterpolationMode = oldMode;

                string timeText = DateTime.Now.ToString("tt h:mm");

                Rectangle clockRect = new Rectangle(
                    client.Right - 122,
                    barY,
                    106,
                    barHeight
                );

                using (Font clockFont = Renderer.F(14f, FontStyle.Bold))
                using (SolidBrush clockBrush = new SolidBrush(Color.White))
                {
                    g.DrawString(
                        timeText,
                        clockFont,
                        clockBrush,
                        clockRect,
                        Renderer.Center()
                    );
                }

                return;
            }
        }
    }
}