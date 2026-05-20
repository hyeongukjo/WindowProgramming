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

    public enum SystemWindowButtonKind
    {
        Ok,
        Cancel,
        Later
    }

    public sealed class SystemDialogButton
    {
        public string Text { get; private set; }
        public string ActionId { get; private set; }
        public SystemWindowButtonKind Kind { get; private set; }

        public SystemDialogButton(string text, string actionId, SystemWindowButtonKind kind)
        {
            Text = text;
            ActionId = actionId;
            Kind = kind;
        }
    }

    public sealed class SystemWindowUI
    {
        public static readonly SystemWindowUI Shared = new SystemWindowUI();

        private Image blueWindowImage;
        private Image blueWindowCloseImage;
        private Image redWindowImage;
        private Image redWindowCloseImage;
        private Image okButtonImage;
        private Image cancelButtonImage;
        private Image laterButtonImage;

        private Rectangle okButtonSource;
        private Rectangle cancelButtonSource;
        private Rectangle laterButtonSource;

        private Rectangle blueWindowSource;
        private Rectangle blueWindowCloseSource;
        private Rectangle redWindowSource;
        private Rectangle redWindowCloseSource;

        private string lastTypewriterBody = "";
        private int typewriterStartTick = 0;

        // 고정 안내창 틀 기준 좌표.
        // normal: 600 x 315 / large: 720 x 360 기준으로 사용한다.
        private int titleX = 28;
        private int titleY = 8;
        private int titleHeight = 30;

        private int npcX = 24;
        private int npcY = 72;
        private int npcWidth = 158;
        private int npcHeight = 188;

        private int bodyX = 190;
        private int bodyY = 76;
        private int bodyRightPadding = 30;
        private int bodyBottomPadding = 82;

        private int buttonWidth = 110;
        private int buttonHeight = 28;
        private int buttonGap = 14;
        private int buttonRightPadding = 36;
        private int buttonBottomPadding = 24;

        private int closeSize = 24;
        private int closeRightPadding = 16;
        private int closeTopPadding = 10;

        private SystemWindowUI()
        {
            LoadImages();
        }

        private void LoadImages()
        {
            string uiDir = FindAssetDirectory("Assets", "UI");

            blueWindowImage = LoadImage(Path.Combine(uiDir, "SystemAlarmBlue.png"));
            blueWindowCloseImage = LoadImage(Path.Combine(uiDir, "SystemAlarmBlueCancel.png"));
            redWindowImage = LoadImage(Path.Combine(uiDir, "SystemAlarmRed.png"));
            redWindowCloseImage = LoadImage(Path.Combine(uiDir, "SystemAlarmRedCancel.png"));

            if (blueWindowImage == null)
                blueWindowImage = LoadImage(Path.Combine(uiDir, "SystemAlarmWindowBlue.png"));

            if (redWindowImage == null)
                redWindowImage = LoadImage(Path.Combine(uiDir, "SystemAlarmWindowRed.png"));

            blueWindowSource = GetVisibleBounds(blueWindowImage);
            blueWindowCloseSource = GetVisibleBounds(blueWindowCloseImage);
            redWindowSource = GetVisibleBounds(redWindowImage);
            redWindowCloseSource = GetVisibleBounds(redWindowCloseImage);

            Image commonButtonImage = LoadImage(Path.Combine(uiDir, "button.png"));

            okButtonImage = commonButtonImage;
            cancelButtonImage = commonButtonImage;
            laterButtonImage = commonButtonImage;

            okButtonSource = GetVisibleBounds(commonButtonImage);
            cancelButtonSource = okButtonSource;
            laterButtonSource = okButtonSource;
        }

        public Rectangle GetStandardNoticeRect(Size clientSize)
        {
            return CenterRect(clientSize, 600, 315);
        }

        public Rectangle GetLargeNoticeRect(Size clientSize)
        {
            return CenterRect(clientSize, 720, 360);
        }

        private Rectangle CenterRect(Size clientSize, int width, int height)
        {
            return new Rectangle(
                clientSize.Width / 2 - width / 2,
                clientSize.Height / 2 - height / 2,
                width,
                height
            );
        }

        private Image LoadImage(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return null;

                using (Image temp = Image.FromFile(path))
                {
                    return new Bitmap(temp);
                }
            }
            catch
            {
                return null;
            }
        }
        private string FindAssetDirectory(params string[] parts)
        {
            string current = AppDomain.CurrentDomain.BaseDirectory;

            for (int i = 0; i < 8; i++)
            {
                string candidate = current;

                for (int j = 0; j < parts.Length; j++)
                    candidate = Path.Combine(candidate, parts[j]);

                if (Directory.Exists(candidate))
                    return candidate;

                DirectoryInfo parent = Directory.GetParent(current);

                if (parent == null)
                    break;

                current = parent.FullName;
            }

            string fallback = AppDomain.CurrentDomain.BaseDirectory;

            for (int j = 0; j < parts.Length; j++)
                fallback = Path.Combine(fallback, parts[j]);

            return fallback;
        }

        private Rectangle GetVisibleBounds(Image image)
        {
            if (image == null)
                return Rectangle.Empty;

            Bitmap bitmap = image as Bitmap;
            if (bitmap == null)
                return new Rectangle(0, 0, image.Width, image.Height);

            int minX = bitmap.Width;
            int minY = bitmap.Height;
            int maxX = -1;
            int maxY = -1;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color color = bitmap.GetPixel(x, y);
                    if (color.A <= 10)
                        continue;

                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }
            }

            if (maxX < minX || maxY < minY)
                return new Rectangle(0, 0, image.Width, image.Height);

            return Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
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
                new List<SystemDialogButton>
                {
                    new SystemDialogButton("확인", okButtonId, SystemWindowButtonKind.Ok)
                },
                buttons,
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
                new List<SystemDialogButton>
                {
                    new SystemDialogButton("확인", okButtonId, SystemWindowButtonKind.Ok)
                },
                buttons,
                closeButtonId
            );
        }

        public void DrawAssistantChoice(
            Graphics g,
            Rectangle rect,
            string title,
            string body,
            NpcMood mood,
            int tick,
            List<SystemDialogButton> dialogButtons,
            List<UiButton> buttons,
            string closeButtonId)
        {
            DrawImageWindow(g, rect, title, body, SystemWindowStyle.Blue, mood, tick, dialogButtons, buttons, closeButtonId);
        }

        public void DrawWarningChoice(
            Graphics g,
            Rectangle rect,
            string title,
            string body,
            NpcMood mood,
            int tick,
            List<SystemDialogButton> dialogButtons,
            List<UiButton> buttons,
            string closeButtonId)
        {
            DrawImageWindow(g, rect, title, body, SystemWindowStyle.Red, mood, tick, dialogButtons, buttons, closeButtonId);
        }

        public void DrawProfileSetupWindow(
            Graphics g,
            Rectangle rect,
            string profileInput,
            NpcMood mood,
            int tick,
            List<UiButton> buttons)
        {
            DrawFrameImage(g, rect, SystemWindowStyle.Blue, false);
            DrawTitleText(g, rect, "Recovery Profile Setup");
            DrawNpc(g, rect, mood);

            Rectangle labelRect = new Rectangle(
                rect.X + bodyX,
                rect.Y + bodyY,
                rect.Width - bodyX - bodyRightPadding,
                34
            );

            Rectangle descRect = new Rectangle(
                rect.X + bodyX,
                rect.Y + bodyY + 42,
                rect.Width - bodyX - bodyRightPadding,
                78
            );

            Rectangle inputRect = new Rectangle(
                rect.X + bodyX,
                rect.Y + bodyY + 106,
                Math.Min(360, rect.Width - bodyX - bodyRightPadding - 10),
                36
            );

            using (Font labelFont = Renderer.F(12.5f, FontStyle.Bold))
            using (SolidBrush labelBrush = new SolidBrush(Color.FromArgb(25, 30, 45)))
                g.DrawString("복구 프로필 이름을 입력하세요.", labelFont, labelBrush, labelRect, Renderer.LeftMiddle());

            using (Font descFont = Renderer.F(10.5f, FontStyle.Regular))
            using (SolidBrush descBrush = new SolidBrush(Color.FromArgb(45, 50, 65)))
                g.DrawString("이 이름은 캐릭터 이름이 아니라 복구 기록과 진행 상황을 저장하는 프로필 이름입니다.", descFont, descBrush, descRect, Renderer.Left());

            using (SolidBrush inputBrush = new SolidBrush(Color.White))
                g.FillRectangle(inputBrush, inputRect);

            using (Pen inputPen = new Pen(Color.FromArgb(40, 80, 160), 2f))
                g.DrawRectangle(inputPen, inputRect);

            using (Font inputFont = Renderer.F(14f, FontStyle.Bold))
            using (SolidBrush inputTextBrush = new SolidBrush(Color.Black))
                g.DrawString(profileInput + "_", inputFont, inputTextBrush, new Rectangle(inputRect.X + 8, inputRect.Y, inputRect.Width - 16, inputRect.Height), Renderer.LeftMiddle());

            SystemDialogButton okButton = new SystemDialogButton("확인", "profileOk", SystemWindowButtonKind.Ok);
            Rectangle okRect = GetConfirmButtonRect(rect);
            DrawImageButton(g, okRect, okButton);

            if (buttons != null)
                buttons.Add(new UiButton(okRect, "profileOk"));
        }
        public void DrawBluePanelFrame(
            Graphics g,
            Rectangle rect,
            string title,
             List<UiButton> buttons,
            string closeButtonId)
        {
            DrawBlueCancelFrameNineSlice(g, rect);
            DrawTitleText(g, rect, title);
            RegisterCloseButton(rect, buttons, closeButtonId);
        }

        private void DrawImageWindow(
            Graphics g,
            Rectangle rect,
            string title,
            string body,
            SystemWindowStyle style,
            NpcMood mood,
            int tick,
            List<SystemDialogButton> dialogButtons,
            List<UiButton> buttons,
            string closeButtonId)
        {
            bool hasClose = !string.IsNullOrEmpty(closeButtonId);

            DrawFrameImage(g, rect, style, hasClose);
            RegisterCloseButton(rect, buttons, closeButtonId);
            DrawTitleText(g, rect, title);
            DrawNpc(g, rect, mood);
            DrawTypewriterBody(g, rect, body, tick);
            DrawDialogButtons(g, rect, dialogButtons, buttons);
        }

        public void DrawBlueCancelFrameNineSlice(Graphics g, Rectangle destRect)
        {
            Image img = blueWindowCloseImage;
            Rectangle sourceRect = blueWindowCloseSource;

            if (img == null)
            {
                DrawFrameImage(g, destRect, SystemWindowStyle.Blue, true);
                return;
            }

            if (sourceRect.IsEmpty)
                sourceRect = new Rectangle(0, 0, img.Width, img.Height);

            DrawNineSliceImage(
                g,
                img,
                sourceRect,
                destRect,
                34,  // left
                25,  // top
                80,  // right: X 버튼 영역 포함
                34   // bottom
            );
        }

        private void DrawNineSliceImage(
    Graphics g,
    Image img,
    Rectangle source,
    Rectangle dest,
    int left,
    int top,
    int right,
    int bottom)
        {
            int centerW = source.Width - left - right;
            int centerH = source.Height - top - bottom;

            int destCenterW = dest.Width - left - right;
            int destCenterH = dest.Height - top - bottom;

            if (centerW <= 0 || centerH <= 0 || destCenterW <= 0 || destCenterH <= 0)
            {
                g.DrawImage(img, dest, source, GraphicsUnit.Pixel);
                return;
            }

            Rectangle[] src =
            {
        new Rectangle(source.X, source.Y, left, top),
        new Rectangle(source.X + left, source.Y, centerW, top),
        new Rectangle(source.Right - right, source.Y, right, top),

        new Rectangle(source.X, source.Y + top, left, centerH),
        new Rectangle(source.X + left, source.Y + top, centerW, centerH),
        new Rectangle(source.Right - right, source.Y + top, right, centerH),

        new Rectangle(source.X, source.Bottom - bottom, left, bottom),
        new Rectangle(source.X + left, source.Bottom - bottom, centerW, bottom),
        new Rectangle(source.Right - right, source.Bottom - bottom, right, bottom)
    };

            Rectangle[] dst =
            {
        new Rectangle(dest.X, dest.Y, left, top),
        new Rectangle(dest.X + left, dest.Y, destCenterW, top),
        new Rectangle(dest.Right - right, dest.Y, right, top),

        new Rectangle(dest.X, dest.Y + top, left, destCenterH),
        new Rectangle(dest.X + left, dest.Y + top, destCenterW, destCenterH),
        new Rectangle(dest.Right - right, dest.Y + top, right, destCenterH),

        new Rectangle(dest.X, dest.Bottom - bottom, left, bottom),
        new Rectangle(dest.X + left, dest.Bottom - bottom, destCenterW, bottom),
        new Rectangle(dest.Right - right, dest.Bottom - bottom, right, bottom)
    };

            System.Drawing.Drawing2D.InterpolationMode oldInterpolation = g.InterpolationMode;
            System.Drawing.Drawing2D.PixelOffsetMode oldPixelOffset = g.PixelOffsetMode;

            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

            for (int i = 0; i < 9; i++)
            {
                g.DrawImage(img, dst[i], src[i], GraphicsUnit.Pixel);
            }

            g.InterpolationMode = oldInterpolation;
            g.PixelOffsetMode = oldPixelOffset;
        }

        private void DrawFrameImage(Graphics g, Rectangle rect, SystemWindowStyle style, bool hasClose)
        {
            Image frame;
            Rectangle sourceRect;

            if (style == SystemWindowStyle.Red)
            {
                frame = hasClose ? redWindowCloseImage : redWindowImage;
                sourceRect = hasClose ? redWindowCloseSource : redWindowSource;
            }
            else
            {
                frame = hasClose ? blueWindowCloseImage : blueWindowImage;
                sourceRect = hasClose ? blueWindowCloseSource : blueWindowSource;
            }

            if (frame != null)
            {
                if (sourceRect.IsEmpty)
                    sourceRect = new Rectangle(0, 0, frame.Width, frame.Height);

                g.DrawImage(frame, rect, sourceRect, GraphicsUnit.Pixel);
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
                rect.Width - 100,
                titleHeight
            );

            using (Font f = Renderer.F(12.5f, FontStyle.Bold))
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

            using (Font f = Renderer.F(10f, FontStyle.Regular))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(25, 30, 45)))
            {
                string wrappedBody = WrapText(g, body, f, bodyRect.Width);

                if (wrappedBody != lastTypewriterBody)
                {
                    lastTypewriterBody = wrappedBody;
                    typewriterStartTick = tick;
                }

                int localTick = Math.Max(0, tick - typewriterStartTick);
                string visibleText = GetVisibleText(wrappedBody, localTick);

                g.DrawString(visibleText, f, b, bodyRect, Renderer.Left());
            }
        }

        private string WrapText(Graphics g, string text, Font font, int maxWidth)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            string normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
            string[] sourceLines = normalized.Split('\n');
            List<string> result = new List<string>();

            for (int i = 0; i < sourceLines.Length; i++)
            {
                string line = sourceLines[i];

                if (line.Length == 0)
                {
                    result.Add("");
                    continue;
                }

                string current = "";

                for (int c = 0; c < line.Length; c++)
                {
                    string next = current + line[c];
                    SizeF size = g.MeasureString(next, font);

                    if (size.Width > maxWidth && current.Length > 0)
                    {
                        result.Add(current.TrimEnd());
                        current = line[c].ToString();
                    }
                    else
                    {
                        current = next;
                    }
                }

                if (current.Length > 0)
                    result.Add(current.TrimEnd());
            }

            return string.Join("\n", result.ToArray());
        }

        private string GetVisibleText(string body, int tick)
        {
            if (string.IsNullOrEmpty(body))
                return "";

            int charsPerTick = 1;
            int count = Math.Min(body.Length, tick * charsPerTick);

            return body.Substring(0, count);
        }

        private void RegisterCloseButton(
            Rectangle rect,
            List<UiButton> buttons,
            string closeButtonId)
        {
            if (buttons == null || string.IsNullOrEmpty(closeButtonId))
                return;

            buttons.Add(new UiButton(GetCloseButtonRect(rect), closeButtonId));
        }

        private void DrawDialogButtons(
            Graphics g,
            Rectangle rect,
            List<SystemDialogButton> dialogButtons,
            List<UiButton> buttons)
        {
            if (dialogButtons == null || dialogButtons.Count == 0)
                return;

            int count = dialogButtons.Count;
            int totalWidth = count * buttonWidth + (count - 1) * buttonGap;
            int startX = rect.Right - buttonRightPadding - totalWidth;
            int y = rect.Bottom - buttonBottomPadding - buttonHeight;

            for (int i = 0; i < count; i++)
            {
                SystemDialogButton dialogButton = dialogButtons[i];
                Rectangle buttonRect = new Rectangle(
                    startX + i * (buttonWidth + buttonGap),
                    y,
                    buttonWidth,
                    buttonHeight
                );

                DrawImageButton(g, buttonRect, dialogButton);

                if (buttons != null && !string.IsNullOrEmpty(dialogButton.ActionId))
                    buttons.Add(new UiButton(buttonRect, dialogButton.ActionId));
            }
        }
        public void DrawDialogImageButton(
            Graphics g,
            Rectangle buttonRect,
            SystemWindowButtonKind kind,
            string actionId,
            List<UiButton> buttons)
        {
            string text = "확인";

            if (kind == SystemWindowButtonKind.Cancel)
                text = "취소";
            else if (kind == SystemWindowButtonKind.Later)
                text = "나중에";

            SystemDialogButton dialogButton = new SystemDialogButton(text, actionId, kind);

            DrawImageButton(g, buttonRect, dialogButton);

            if (buttons != null && !string.IsNullOrEmpty(actionId))
                buttons.Add(new UiButton(buttonRect, actionId));
        }

        private void DrawImageButton(Graphics g, Rectangle buttonRect, SystemDialogButton dialogButton)
        {
            Image image = GetButtonImage(dialogButton.Kind);

            if (image != null)
            {
                Rectangle sourceRect = GetButtonSourceRect(dialogButton.Kind);

                if (sourceRect.IsEmpty)
                    sourceRect = new Rectangle(0, 0, image.Width, image.Height);

                g.DrawImage(image, buttonRect, sourceRect, GraphicsUnit.Pixel);
                DrawButtonText(g, buttonRect, dialogButton.Text);
                return;
            }

            Renderer.DrawButton(g, buttonRect, dialogButton.Text, true);
        }

        private void DrawButtonText(Graphics g, Rectangle buttonRect, string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            Rectangle textRect = new Rectangle(
                buttonRect.X,
                buttonRect.Y - 1,
                buttonRect.Width,
                buttonRect.Height
            );

            using (Font font = Renderer.F(9.5f, FontStyle.Bold))
            using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(120, 255, 255, 255)))
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(40, 40, 40)))
            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                Rectangle shadowRect = new Rectangle(
                    textRect.X,
                    textRect.Y + 1,
                    textRect.Width,
                    textRect.Height
                );

                g.DrawString(text, font, shadowBrush, shadowRect, format);
                g.DrawString(text, font, textBrush, textRect, format);
            }
        }

        private Image GetButtonImage(SystemWindowButtonKind kind)
        {
            if (kind == SystemWindowButtonKind.Cancel)
                return cancelButtonImage;

            if (kind == SystemWindowButtonKind.Later)
                return laterButtonImage;

            return okButtonImage;
        }

        private Rectangle GetButtonSourceRect(SystemWindowButtonKind kind)
        {
            if (kind == SystemWindowButtonKind.Cancel)
                return cancelButtonSource;

            if (kind == SystemWindowButtonKind.Later)
                return laterButtonSource;

            return okButtonSource;
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
