using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace DebugHeroFileDungeonRPG
{
    public enum SystemWindowStyle
    {
        Blue,
        Red
    }

    public sealed class SystemWindowUI
    {
        public static readonly SystemWindowUI Shared = new SystemWindowUI();

        private Image blueWindowImage;
        private Image redWindowImage;
        private string lastTypewriterBody = "";
        private int typewriterStartTick = 0;

        // 창 이미지 기준 내부 여백값
        private int titleX = 18;
        private int titleY = 8;
        private int titleHeight = 28;

        private int npcX = 24;
        private int npcY = 58;
        private int npcWidth = 170;
        private int npcHeight = 220;

        private int bodyX = 210;
        private int bodyY = 70;
        private int bodyRightPadding = 32;
        private int bodyBottomPadding = 80;

        private int buttonWidth = 88;
        private int buttonHeight = 28;
        private int buttonRightPadding = 24;
        private int buttonBottomPadding = 24;

        private int closeSize = 22;
        private int closeRightPadding = 16;
        private int closeTopPadding = 10;

        private SystemWindowUI()
        {
            LoadImages();
        }

        private void LoadImages()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            string bluePath = Path.Combine(baseDir, "Assets", "UI", "SystemAlarmWindowBlue.png");
            string redPath = Path.Combine(baseDir, "Assets", "UI", "SystemAlarmWindowRed.png");

            if (File.Exists(bluePath))
                blueWindowImage = Image.FromFile(bluePath);

            if (File.Exists(redPath))
                redWindowImage = Image.FromFile(redPath);
        }

        public void DrawAssistantNotice(
            Graphics g,
            Rectangle rect,
            string title,
            string body,
            NpcMood mood,
            int tick,
            List<UiButton> buttons,
            string okButtonId,
            string closeButtonId)
        {
            DrawImageWindow(
                g,
                rect,
                title,
                body,
                SystemWindowStyle.Blue,
                mood,
                tick,
                "확인",
                buttons,
                okButtonId,
                closeButtonId
            );
        }

        public void DrawWarningNotice(
            Graphics g,
            Rectangle rect,
            string title,
            string body,
            NpcMood mood,
            int tick,
            List<UiButton> buttons,
            string okButtonId,
            string closeButtonId)
        {
            DrawImageWindow(
                g,
                rect,
                title,
                body,
                SystemWindowStyle.Red,
                mood,
                tick,
                "확인",
                buttons,
                okButtonId,
                closeButtonId
            );
        }

        private void DrawImageWindow(
            Graphics g,
            Rectangle rect,
            string title,
            string body,
            SystemWindowStyle style,
            NpcMood mood,
            int tick,
            string buttonText,
            List<UiButton> buttons,
            string okButtonId,
            string closeButtonId)
        {
            DrawFrameImage(g, rect, style);
            DrawCloseButton(g, rect, buttons, closeButtonId);
            DrawTitleText(g, rect, title);
            DrawNpc(g, rect, mood);
            DrawTypewriterBody(g, rect, body, tick);
            DrawConfirmButton(g, rect, buttonText, buttons, okButtonId);
        }

        private void DrawFrameImage(Graphics g, Rectangle rect, SystemWindowStyle style)
        {
            Image frame = style == SystemWindowStyle.Red
                ? redWindowImage
                : blueWindowImage;

            if (frame != null)
            {
                Rectangle src = new Rectangle(126, 103, 1196, 873);
                g.DrawImage(frame, rect, src, GraphicsUnit.Pixel);
                return;
            }

            Color borderColor = style == SystemWindowStyle.Red
                ? Color.FromArgb(180, 40, 40)
                : Color.FromArgb(40, 100, 210);

            using (SolidBrush b = new SolidBrush(Color.FromArgb(235, 245, 255)))
                g.FillRectangle(b, rect);

            using (Pen p = new Pen(borderColor, 3))
                g.DrawRectangle(p, rect);
        }

        private void DrawTitleText(Graphics g, Rectangle rect, string title)
        {
            Rectangle titleRect = new Rectangle(
                rect.X + titleX,
                rect.Y + titleY,
                rect.Width - 90,
                titleHeight
            );

            using (Font f = Renderer.F(10.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.White))
            {
                g.DrawString(title, f, b, titleRect, Renderer.LeftMiddle());
            }
        }

        private void DrawNpc(Graphics g, Rectangle rect, NpcMood mood)
        {
            Rectangle npcRect = new Rectangle(
                rect.X + npcX,
                rect.Y + npcY,
                npcWidth,
                npcHeight
            );

            Renderer.DrawNpcImage(g, npcRect, mood);
        }

        private void DrawTypewriterBody(Graphics g, Rectangle rect, string body, int tick)
        {
            Rectangle bodyRect = new Rectangle(
                rect.X + bodyX,
                rect.Y + bodyY,
                rect.Width - bodyX - bodyRightPadding,
                rect.Height - bodyY - bodyBottomPadding
            );

            if (body != lastTypewriterBody)
            {
                lastTypewriterBody = body;
                typewriterStartTick = tick;
            }

            int localTick = Math.Max(0, tick - typewriterStartTick);
            string visibleText = GetVisibleText(body, localTick);

            using (Font f = Renderer.F(10f, FontStyle.Regular))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(25, 30, 45)))
            {
                g.DrawString(visibleText, f, b, bodyRect, Renderer.Left());
            }
        }

        private string GetVisibleText(string body, int tick)
        {
            if (string.IsNullOrEmpty(body))
                return "";

            int charsPerTick = 1;
            int count = Math.Min(body.Length, tick * charsPerTick);

            return body.Substring(0, count);
        }

        private void DrawCloseButton(
            Graphics g,
            Rectangle rect,
            List<UiButton> buttons,
            string closeButtonId)
        {
            Rectangle closeRect = GetCloseButtonRect(rect);

            using (SolidBrush b = new SolidBrush(Color.FromArgb(210, 50, 40)))
                g.FillRectangle(b, closeRect);

            using (Pen p = new Pen(Color.White, 2f))
            {
                g.DrawLine(p, closeRect.X + 6, closeRect.Y + 6, closeRect.Right - 6, closeRect.Bottom - 6);
                g.DrawLine(p, closeRect.Right - 6, closeRect.Y + 6, closeRect.X + 6, closeRect.Bottom - 6);
            }

            using (Pen p = new Pen(Color.FromArgb(110, 20, 15), 1f))
                g.DrawRectangle(p, closeRect);

            if (buttons != null && !string.IsNullOrEmpty(closeButtonId))
                buttons.Add(new UiButton(closeRect, closeButtonId));
        }

        private void DrawConfirmButton(
            Graphics g,
            Rectangle rect,
            string buttonText,
            List<UiButton> buttons,
            string okButtonId)
        {
            Rectangle buttonRect = GetConfirmButtonRect(rect);

            Renderer.DrawButton(g, buttonRect, buttonText, true);

            if (buttons != null && !string.IsNullOrEmpty(okButtonId))
                buttons.Add(new UiButton(buttonRect, okButtonId));
        }

        public Rectangle GetConfirmButtonRect(Rectangle rect)
        {
            return new Rectangle(
                rect.Right - buttonRightPadding - buttonWidth,
                rect.Bottom - buttonBottomPadding - buttonHeight,
                buttonWidth,
                buttonHeight
            );
        }

        public Rectangle GetCloseButtonRect(Rectangle rect)
        {
            return new Rectangle(
                rect.Right - closeRightPadding - closeSize,
                rect.Y + closeTopPadding,
                closeSize,
                closeSize
            );
        }
    }
}