using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace DebugHeroFileDungeonRPG
{
    public sealed class DesktopIconUI
    {
        public static readonly DesktopIconUI Shared = new DesktopIconUI();

        private Image desktopIconsSheet;

        private const int IconSheetColumns = 4;
        private const int IconSheetRows = 4;
        private const int DesktopIconSize = 60;
        private const int DesktopIconLabelWidth = 120;

        private Image LoadDesktopIconsSheet()
        {
            if (desktopIconsSheet != null)
                return desktopIconsSheet;

            string path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                "UI",
                "icons.png"
            );

            try
            {
                if (File.Exists(path))
                    desktopIconsSheet = Image.FromFile(path);
            }
            catch
            {
                desktopIconsSheet = null;
            }

            return desktopIconsSheet;
        }

        public void DrawFixedDesktopIcons(Graphics g, Rectangle client)
        {
            Image sheet = LoadDesktopIconsSheet();

            int x = 30;
            int y = 34;
            int gapY = 122;

            DrawIconFromSheet(g, sheet, 0, 0, x, y + gapY * 0, "내 컴퓨터");
            DrawIconFromSheet(g, sheet, 1, 0, x, y + gapY * 1, "파일");
            DrawIconFromSheet(g, sheet, 2, 0, x, y + gapY * 2, "Internet Explorer");
            DrawIconFromSheet(g, sheet, 3, 0, x, y + gapY * 3, "휴지통");
        }
        public void DrawRecoveryToolsShortcut(
    Graphics g,
    Rectangle client,
    int coins,
    List<UiButton> buttons)
        {
            Image sheet = LoadDesktopIconsSheet();

            int desktopStartX = 30;
            int desktopStartY = 34;
            int desktopGapY = 122;

            // 휴지통이 gapY * 3 이므로, 그 아래는 gapY * 4
            int iconX = desktopStartX;
            int iconY = desktopStartY + desktopGapY * 4;

            // icons.png 기준: 4행 2열 아이콘
            int recoveryIconCol = 1;
            int recoveryIconRow = 3;

            string label = "Recovery Tools\n" + coins + " coin";

            Rectangle hitBox = new Rectangle(
                iconX - 30,
                iconY - 6,
                DesktopIconLabelWidth,
                DesktopIconSize + 54
            );

            DrawIconFromSheet(g, sheet, recoveryIconCol, recoveryIconRow, iconX, iconY, label);

            if (buttons != null)
                buttons.Add(new UiButton(hitBox, "openShop"));
        }

        private void DrawIconFromSheet(
            Graphics g,
            Image sheet,
            int col,
            int row,
            int x,
            int y,
            string label)
        {
            Rectangle dest = new Rectangle(x, y, DesktopIconSize, DesktopIconSize);

            if (sheet != null)
            {
                Rectangle src = GetIconSourceRect(sheet, col, row);

                InterpolationMode oldInterpolation = g.InterpolationMode;
                PixelOffsetMode oldPixelOffset = g.PixelOffsetMode;

                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;

                g.DrawImage(sheet, dest, src, GraphicsUnit.Pixel);

                g.InterpolationMode = oldInterpolation;
                g.PixelOffsetMode = oldPixelOffset;
            }
            else
            {
                DrawFallbackIcon(g, dest);
            }

            DrawIconLabel(g, x + DesktopIconSize / 2, y + DesktopIconSize + 3, label);
        }
        private Rectangle FitIconToBox(Rectangle src, Rectangle box)
        {
            if (src.Width <= 0 || src.Height <= 0)
                return box;

            float scale = Math.Min(
                box.Width / (float)src.Width,
                box.Height / (float)src.Height
            );

            int drawW = Math.Max(1, (int)(src.Width * scale));
            int drawH = Math.Max(1, (int)(src.Height * scale));

            int drawX = box.X + (box.Width - drawW) / 2;
            int drawY = box.Y + (box.Height - drawH) / 2;

            return new Rectangle(drawX, drawY, drawW, drawH);
        }

        private Rectangle GetIconSourceRect(Image sheet, int col, int row)
        {
            int x1 = sheet.Width * col / IconSheetColumns;
            int y1 = sheet.Height * row / IconSheetRows;
            int x2 = sheet.Width * (col + 1) / IconSheetColumns;
            int y2 = sheet.Height * (row + 1) / IconSheetRows;

            Rectangle cell = new Rectangle(x1, y1, x2 - x1, y2 - y1);

            Bitmap bmp = sheet as Bitmap;
            if (bmp == null)
                return cell;

            int minX = cell.Right;
            int minY = cell.Bottom;
            int maxX = cell.Left;
            int maxY = cell.Top;

            for (int y = cell.Top; y < cell.Bottom; y++)
            {
                for (int x = cell.Left; x < cell.Right; x++)
                {
                    Color px = bmp.GetPixel(x, y);

                    if (px.A > 20)
                    {
                        if (x < minX) minX = x;
                        if (y < minY) minY = y;
                        if (x > maxX) maxX = x;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            if (maxX <= minX || maxY <= minY)
                return cell;

            int padding = 4;

            minX = Math.Max(cell.Left, minX - padding);
            minY = Math.Max(cell.Top, minY - padding);
            maxX = Math.Min(cell.Right - 1, maxX + padding);
            maxY = Math.Min(cell.Bottom - 1, maxY + padding);

            return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        private void DrawIconLabel(Graphics g, int centerX, int topY, string label)
        {
            Rectangle labelRect = new Rectangle(
                centerX - DesktopIconLabelWidth / 2,
                topY,
                DesktopIconLabelWidth,
                44
            );

            using (Font font = Renderer.F(10.5f, FontStyle.Bold))
            using (SolidBrush shadow = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
            using (SolidBrush white = new SolidBrush(Color.White))
            {
                Rectangle shadowRect = new Rectangle(
                    labelRect.X + 1,
                    labelRect.Y + 1,
                    labelRect.Width,
                    labelRect.Height
                );

                g.DrawString(label, font, shadow, shadowRect, Renderer.Center());
                g.DrawString(label, font, white, labelRect, Renderer.Center());
            }
        }

        private void DrawFallbackIcon(Graphics g, Rectangle dest)
        {
            using (SolidBrush b = new SolidBrush(Color.FromArgb(245, 220, 70)))
                g.FillRectangle(b, dest);

            using (Pen p = new Pen(Color.FromArgb(30, 70, 150), 2f))
                g.DrawRectangle(p, dest.X, dest.Y, dest.Width - 1, dest.Height - 1);
        }
        private DesktopIconUI()
        {
        }

        public void DrawStageIcons(
    Graphics g,
    List<StageInfo> stages,
    int unlockedStage,
    int selectedStage,
    int clearedStages,
    List<UiButton> buttons)
        {
            Image sheet = LoadDesktopIconsSheet();

            // 기본 바탕화면 아이콘과 같은 기준값
            int desktopStartX = 30;
            int desktopStartY = 34;
            int desktopGapX = 125;
            int desktopGapY = 122;

            // 기본 아이콘들이 1열을 쓰고 있으므로, 스테이지 아이콘은 그 오른쪽 열부터 시작
            int firstStageColumn = 1;

            // icons.png에서 사용할 아이콘 위치
            
            int stageIconCol = 2;
            int stageIconRow = 1;

            int rowsPerColumn = 4;

            for (int i = 1; i <= unlockedStage && i <= stages.Count; i++)
            {
                int index = i - 1;

                int desktopCol = firstStageColumn + index / rowsPerColumn;
                int desktopRow = index % rowsPerColumn;

                int iconX = desktopStartX + desktopCol * desktopGapX;
                int iconY = desktopStartY + desktopRow * desktopGapY;

                StageInfo st = stages[i - 1];

                bool selected = selectedStage == i;
                bool newly = i == unlockedStage && i > clearedStages;

                Rectangle hitBox = new Rectangle(
                    iconX - 30,
                    iconY - 6,
                    DesktopIconLabelWidth,
                    DesktopIconSize + 54
                );

                if (selected)
                {
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(90, 40, 100, 230)))
                        g.FillRectangle(b, hitBox);

                    using (Pen p = new Pen(Color.FromArgb(210, 180, 215, 255), 2f))
                        g.DrawRectangle(p, hitBox.X, hitBox.Y, hitBox.Width - 1, hitBox.Height - 1);
                }
                else if (newly)
                {
                    using (Pen p = new Pen(Color.FromArgb(210, 255, 245, 170), 2f))
                        g.DrawRectangle(p, hitBox.X, hitBox.Y, hitBox.Width - 1, hitBox.Height - 1);
                }

                string label = string.IsNullOrEmpty(st.FileName)
                    ? "Stage " + i.ToString("00")
                    : st.FileName;

                DrawIconFromSheet(g, sheet, stageIconCol, stageIconRow, iconX, iconY, label);

                buttons.Add(new UiButton(hitBox, "stage" + i.ToString()));
            }
        }
    }
}