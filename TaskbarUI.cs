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

        public void Draw(Graphics g, Rectangle client)
        {
            // 화면에 보일 작업표시줄 두께
            // 두꺼우면 48, 더 얇게 하고 싶으면 44 추천
            int barHeight = 42;
            int barY = client.Bottom - barHeight;

            if (BackgroundRenderer.taskbarImg != null)
            {
                // taskbar.png 전체 이미지 안에서 실제 작업표시줄 부분만 잘라서 사용
                // 현재 이미지 기준 실제 taskbar 위치:
                // X: 22 ~ 1649
                // Y: 428 ~ 506
                Rectangle src = new Rectangle(
                    22,
                    428,
                    1628,
                    79
                );

                // 화면 아래쪽 전체 너비로 늘려서 그림
                Rectangle dest = new Rectangle(
                    client.X - 7,
                    barY,
                    client.Width + 14,
                    barHeight
                );

                var oldMode = g.InterpolationMode;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                g.DrawImage(
                    BackgroundRenderer.taskbarImg, dest, src, GraphicsUnit.Pixel);

                g.InterpolationMode = oldMode;

                // 작업표시줄 오른쪽 시계 표시
                string timeText = DateTime.Now.ToString("tt h:mm");

                Rectangle clockRect = new Rectangle(
                    client.Right - 122,
                    barY,
                    106,
                    barHeight
                );

                using (Font clockFont = Renderer.F(16f, FontStyle.Bold))
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