using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DebugHeroFileDungeonRPG
{
    public static class Renderer
    {
        public static Font Font(float size, FontStyle style)
        {
            return new Font("Malgun Gothic", size, style, GraphicsUnit.Point);
        }

        public static StringFormat Center()
        {
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            sf.Trimming = StringTrimming.EllipsisCharacter;
            return sf;
        }

        public static StringFormat Left()
        {
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Near;
            sf.LineAlignment = StringAlignment.Near;
            sf.Trimming = StringTrimming.EllipsisCharacter;
            return sf;
        }

        public static StringFormat LeftMiddle()
        {
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Near;
            sf.LineAlignment = StringAlignment.Center;
            sf.Trimming = StringTrimming.EllipsisCharacter;
            return sf;
        }

        public static Color Lighten(Color c, int amount)
        {
            return Color.FromArgb(c.A, Math.Min(255, c.R + amount), Math.Min(255, c.G + amount), Math.Min(255, c.B + amount));
        }

        public static Color Darken(Color c, int amount)
        {
            return Color.FromArgb(c.A, Math.Max(0, c.R - amount), Math.Max(0, c.G - amount), Math.Max(0, c.B - amount));
        }

        private static int ClampAlpha(int value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return value;
        }

        public static void Panel(Graphics g, Rectangle r, Color fill)
        {
            if (r.Width <= 0 || r.Height <= 0) return;
            using (LinearGradientBrush b = new LinearGradientBrush(r, Lighten(fill, 24), Darken(fill, 8), 90f))
                g.FillRectangle(b, r);
            using (Pen light = new Pen(Color.FromArgb(255, 255, 255)))
            using (Pen dark = new Pen(Color.FromArgb(95, 104, 118)))
            using (Pen mid = new Pen(Color.FromArgb(176, 186, 198)))
            {
                g.DrawLine(light, r.Left, r.Top, r.Right - 1, r.Top);
                g.DrawLine(light, r.Left, r.Top, r.Left, r.Bottom - 1);
                g.DrawLine(dark, r.Right - 1, r.Top, r.Right - 1, r.Bottom - 1);
                g.DrawLine(dark, r.Left, r.Bottom - 1, r.Right - 1, r.Bottom - 1);
                if (r.Width > 3 && r.Height > 3) g.DrawRectangle(mid, r.X + 1, r.Y + 1, r.Width - 3, r.Height - 3);
            }
        }

        public static void Inset(Graphics g, Rectangle r, Color fill)
        {
            if (r.Width <= 0 || r.Height <= 0) return;
            using (LinearGradientBrush b = new LinearGradientBrush(r, Darken(fill, 8), Lighten(fill, 16), 90f))
                g.FillRectangle(b, r);
            using (Pen dark = new Pen(Color.FromArgb(95, 104, 118)))
            using (Pen light = new Pen(Color.White))
            {
                g.DrawLine(dark, r.Left, r.Top, r.Right - 1, r.Top);
                g.DrawLine(dark, r.Left, r.Top, r.Left, r.Bottom - 1);
                g.DrawLine(light, r.Right - 1, r.Top, r.Right - 1, r.Bottom - 1);
                g.DrawLine(light, r.Left, r.Bottom - 1, r.Right - 1, r.Bottom - 1);
            }
        }

        public static void Header(Graphics g, Rectangle r, string title)
        {
            using (LinearGradientBrush b = new LinearGradientBrush(r, Color.FromArgb(20, 54, 135), Color.FromArgb(50, 145, 230), 0f))
                g.FillRectangle(b, r);
            using (Pen p = new Pen(Color.FromArgb(175, 220, 255)))
                g.DrawRectangle(p, r.X, r.Y, r.Width - 1, r.Height - 1);
            using (Font f = Font(10.5f, FontStyle.Bold))
            using (SolidBrush br = new SolidBrush(Color.White))
                g.DrawString(title, f, br, r, Center());
        }

        public static void Button(Graphics g, Rectangle r, string text, bool selected)
        {
            Panel(g, r, selected ? Color.FromArgb(222, 238, 255) : Color.FromArgb(218, 224, 232));
            if (selected)
            {
                using (Pen p = new Pen(Color.FromArgb(40, 104, 210), 3f))
                    g.DrawRectangle(p, r.X + 2, r.Y + 2, r.Width - 5, r.Height - 5);
            }
            using (Font f = Font(12f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(24, 32, 48)))
                g.DrawString(text, f, b, r, Center());
        }

        public static void Bar(Graphics g, Rectangle r, int value, int max, Color color)
        {
            Inset(g, r, Color.FromArgb(235, 239, 245));
            int w = 0;
            if (max > 0) w = (int)((r.Width - 4) * Math.Max(0, Math.Min(1f, value / (float)max)));
            if (w > 0)
            {
                Rectangle fill = new Rectangle(r.X + 2, r.Y + 2, w, r.Height - 4);
                using (LinearGradientBrush b = new LinearGradientBrush(fill, Lighten(color, 30), Darken(color, 15), 90f))
                    g.FillRectangle(b, fill);
                using (SolidBrush shine = new SolidBrush(Color.FromArgb(52, 255, 255, 255)))
                    g.FillRectangle(shine, fill.X, fill.Y, fill.Width, Math.Max(1, fill.Height / 2));
            }
        }

        public static void DrawDesktopWallpaper(Graphics g, Rectangle client, float parallax)
        {
            using (LinearGradientBrush b = new LinearGradientBrush(client, Color.FromArgb(22, 82, 174), Color.FromArgb(157, 221, 255), 90f))
                g.FillRectangle(b, client);
            DrawCloud(g, 120 - parallax * 0.1f, 64, 1.1f);
            DrawCloud(g, 560 - parallax * 0.16f, 130, 0.8f);
            DrawCloud(g, 1030 - parallax * 0.13f, 84, 1.0f);
            DrawCloud(g, 1500 - parallax * 0.09f, 155, 0.9f);

            Point[] far = new Point[] { new Point(-160, client.Height), new Point(210, client.Height - 170), new Point(690, client.Height - 110), new Point(1160, client.Height - 205), new Point(client.Width + 200, client.Height - 100), new Point(client.Width + 200, client.Height) };
            using (SolidBrush hb = new SolidBrush(Color.FromArgb(54, 148, 78))) g.FillPolygon(hb, far);
            Point[] near = new Point[] { new Point(-160, client.Height), new Point(280, client.Height - 88), new Point(770, client.Height - 138), new Point(1280, client.Height - 75), new Point(client.Width + 200, client.Height - 140), new Point(client.Width + 200, client.Height) };
            using (SolidBrush hb = new SolidBrush(Color.FromArgb(99, 181, 75))) g.FillPolygon(hb, near);
            using (Pen p = new Pen(Color.FromArgb(74, 130, 64)))
            {
                for (int x = -200; x < client.Width + 200; x += 95)
                    g.DrawLine(p, x, client.Height - 86, x + 230, client.Height);
            }
        }

        private static void DrawCloud(Graphics g, float x, float y, float s)
        {
            using (SolidBrush b = new SolidBrush(Color.FromArgb(230, 255, 255, 255)))
            {
                g.FillEllipse(b, x, y + 18 * s, 80 * s, 34 * s);
                g.FillEllipse(b, x + 28 * s, y, 90 * s, 58 * s);
                g.FillEllipse(b, x + 92 * s, y + 20 * s, 72 * s, 34 * s);
                g.FillRectangle(b, x + 22 * s, y + 28 * s, 126 * s, 28 * s);
            }
        }


        public static void DrawDungeonDesktopIcon(Graphics g, Rectangle r, DungeonInfo dungeon, bool selected, bool locked, int index)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (selected)
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(82, 60, 130, 255)))
                    g.FillRectangle(b, r.X - 4, r.Y - 4, r.Width + 8, r.Height + 8);
                using (Pen p = new Pen(Color.FromArgb(230, 255, 255, 255), 2f))
                    g.DrawRectangle(p, r.X - 4, r.Y - 4, r.Width + 8, r.Height + 8);
            }

            int fileW = Math.Min(64, Math.Max(42, r.Width - 40));
            int fileH = Math.Min(72, Math.Max(54, r.Height - 42));
            Rectangle file = new Rectangle(r.X + r.Width / 2 - fileW / 2, r.Y + 4, fileW, fileH);
            DrawLargeFileSymbol(g, file, dungeon.Accent, locked);

            string ext = ".exe";
            int dot = dungeon.FileName.LastIndexOf('.');
            if (dot >= 0 && dot < dungeon.FileName.Length - 1) ext = dungeon.FileName.Substring(dot);
            Rectangle extBox = new Rectangle(file.X + 6, file.Bottom - 24, file.Width - 12, 18);
            using (SolidBrush b = new SolidBrush(locked ? Color.FromArgb(150, 120, 120, 120) : Color.FromArgb(190, dungeon.Accent)))
                g.FillRectangle(b, extBox);
            using (Pen p = new Pen(Color.FromArgb(120, 0, 0, 0))) g.DrawRectangle(p, extBox);
            using (Font f = Font(7.2f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.White))
                g.DrawString(ext.ToUpperInvariant(), f, b, extBox, Center());

            Rectangle arrow = new Rectangle(file.X - 2, file.Bottom - 17, 18, 18);
            using (SolidBrush b = new SolidBrush(Color.FromArgb(230, 240, 248))) g.FillRectangle(b, arrow);
            using (Pen p = new Pen(Color.FromArgb(30, 70, 180), 2.2f))
            {
                g.DrawLine(p, arrow.X + 4, arrow.Y + 12, arrow.X + 13, arrow.Y + 5);
                g.DrawLine(p, arrow.X + 9, arrow.Y + 5, arrow.X + 13, arrow.Y + 5);
                g.DrawLine(p, arrow.X + 13, arrow.Y + 5, arrow.X + 13, arrow.Y + 10);
            }

            Rectangle labelRect = new Rectangle(r.X + 1, r.Y + fileH + 10, r.Width - 2, r.Height - fileH - 12);
            using (SolidBrush shadow = new SolidBrush(Color.FromArgb(90, 0, 0, 0)))
                g.FillRectangle(shadow, labelRect.X + 2, labelRect.Y + 2, labelRect.Width, Math.Max(16, labelRect.Height - 2));
            using (SolidBrush b = new SolidBrush(selected ? Color.FromArgb(72, 108, 210) : Color.FromArgb(28, 35, 52)))
                g.FillRectangle(b, labelRect);
            using (Font f = Font(7.4f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(locked ? Color.Silver : Color.White))
                g.DrawString(dungeon.FileName, f, b, labelRect, Center());

            if (locked)
            {
                Rectangle lockBox = new Rectangle(file.Right - 22, file.Y + 2, 24, 24);
                using (SolidBrush b = new SolidBrush(Color.FromArgb(225, 130, 40, 40))) g.FillEllipse(b, lockBox);
                using (Font f = Font(10f, FontStyle.Bold))
                using (SolidBrush b = new SolidBrush(Color.White)) g.DrawString("!", f, b, lockBox, Center());
            }
        }

        public static void DrawFileIcon(Graphics g, Rectangle r, DungeonInfo dungeon, bool selected, bool locked)
        {
            Panel(g, r, selected ? Color.FromArgb(245, 249, 255) : Color.FromArgb(232, 236, 242));
            if (selected)
            {
                using (Pen p = new Pen(dungeon.Accent, 4f)) g.DrawRectangle(p, r.X + 2, r.Y + 2, r.Width - 5, r.Height - 5);
            }
            Rectangle icon = new Rectangle(r.X + 20, r.Y + 22, 82, 82);
            DrawLargeFileSymbol(g, icon, dungeon.Accent, locked);
            using (Font f = Font(11f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(locked ? Color.Gray : Color.FromArgb(24, 32, 48)))
                g.DrawString(dungeon.DisplayName, f, b, new Rectangle(r.X + 118, r.Y + 22, r.Width - 135, 28), LeftMiddle());
            using (Font f = Font(9f, FontStyle.Regular))
            using (SolidBrush b = new SolidBrush(locked ? Color.Gray : Color.FromArgb(52, 68, 96)))
            {
                g.DrawString(dungeon.FileName, f, b, new Rectangle(r.X + 118, r.Y + 51, r.Width - 135, 20), LeftMiddle());
                g.DrawString(dungeon.Description, f, b, new Rectangle(r.X + 118, r.Y + 76, r.Width - 135, 40), Left());
            }
            using (Font f = Font(8.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(locked ? Color.Firebrick : dungeon.Accent))
            {
                string req = locked ? "잠김: 패치 조각 " + dungeon.RequiredPatch + "개 필요" : "권장 Lv. " + dungeon.RecommendedLevel + "  /  Enter 또는 E로 실행";
                g.DrawString(req, f, b, new Rectangle(r.X + 118, r.Bottom - 30, r.Width - 135, 20), LeftMiddle());
            }
        }

        public static void DrawLargeFileSymbol(Graphics g, Rectangle r, Color color, bool locked)
        {
            Color c = locked ? Color.Gray : color;
            Point[] paper = new Point[] {
                new Point(r.X + 12, r.Y + 4), new Point(r.Right - 20, r.Y + 4),
                new Point(r.Right - 6, r.Y + 18), new Point(r.Right - 6, r.Bottom - 8),
                new Point(r.X + 12, r.Bottom - 8)
            };
            using (LinearGradientBrush b = new LinearGradientBrush(r, Color.White, Color.FromArgb(214, 226, 240), 90f))
                g.FillPolygon(b, paper);
            using (Pen p = new Pen(Darken(c, 45), 2f)) g.DrawPolygon(p, paper);
            using (SolidBrush b = new SolidBrush(Color.FromArgb(38, c))) g.FillRectangle(b, r.X + 22, r.Y + 28, r.Width - 38, 26);
            using (Pen p = new Pen(c, 3f))
            {
                g.DrawLine(p, r.X + 26, r.Y + 36, r.Right - 24, r.Y + 36);
                g.DrawLine(p, r.X + 26, r.Y + 48, r.Right - 30, r.Y + 48);
            }
            if (locked)
            {
                using (Font f = Font(20f, FontStyle.Bold))
                using (SolidBrush b = new SolidBrush(Color.Firebrick))
                    g.DrawString("LOCK", f, b, r, Center());
            }
        }



        public static void DrawDungeonBackground(Graphics g, Rectangle view, DungeonInfo dungeon, float cameraX, int tick)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Color baseBack = dungeon.BackColor;
            using (LinearGradientBrush b = new LinearGradientBrush(view, Darken(baseBack, 34), Lighten(baseBack, 32), 90f))
                g.FillRectangle(b, view);

            using (SolidBrush glow = new SolidBrush(Color.FromArgb(54, dungeon.Accent)))
            {
                g.FillEllipse(glow, view.X + view.Width / 2 - 460, view.Y + 30, 920, 340);
                g.FillEllipse(glow, view.Right - 410, view.Bottom - 315, 560, 320);
                g.FillEllipse(glow, view.X - 170, view.Bottom - 290, 480, 280);
            }

            DrawOpenedFileShell(g, view, dungeon, cameraX, tick);
            DrawDungeonParallaxWindows(g, view, dungeon, cameraX, tick);
            DrawCircuitGrid(g, view, dungeon.Accent, cameraX, tick);
            DrawFloatingCode(g, view, dungeon, cameraX, tick);
            DrawDungeonThemeProps(g, view, dungeon, cameraX, tick);
            DrawExplorerBreadcrumb(g, view, dungeon, cameraX);
        }


        public static void DrawPlatform(Graphics g, Platform p, float cameraX)
        {
            Rectangle r = Rectangle.Round(new RectangleF(p.Bounds.X - cameraX, p.Bounds.Y, p.Bounds.Width, p.Bounds.Height));
            if (r.Right < -80 || r.Left > 4000) return;

            using (SolidBrush shadow = new SolidBrush(Color.FromArgb(75, 0, 0, 0)))
                g.FillRectangle(shadow, r.X + 8, r.Bottom + 4, r.Width - 8, 8);

            // Window title-bar platform: file objects become actual floor.
            Rectangle title = new Rectangle(r.X, r.Y, r.Width, Math.Min(17, Math.Max(10, r.Height / 2)));
            Rectangle body = new Rectangle(r.X, r.Y + title.Height, r.Width, r.Height - title.Height);
            using (LinearGradientBrush b = new LinearGradientBrush(r, Lighten(p.Color, 36), Darken(p.Color, 28), 90f))
                g.FillRectangle(b, r);
            using (LinearGradientBrush tb = new LinearGradientBrush(title, Color.FromArgb(26, 62, 146), Lighten(p.Color, 28), 0f))
                g.FillRectangle(tb, title);
            using (SolidBrush shine = new SolidBrush(Color.FromArgb(45, 255, 255, 255)))
                g.FillRectangle(shine, r.X + 2, r.Y + 2, r.Width - 4, Math.Max(2, r.Height / 3));
            using (Pen pLight = new Pen(Color.FromArgb(235, 255, 255, 255), 2f))
                g.DrawLine(pLight, r.X, r.Y, r.Right, r.Y);
            using (Pen pDark = new Pen(Color.FromArgb(170, 0, 0, 0), 2f))
                g.DrawLine(pDark, r.X, r.Bottom - 1, r.Right, r.Bottom - 1);
            using (Pen border = new Pen(Color.FromArgb(210, 30, 42, 64), 1f))
                g.DrawRectangle(border, r.X, r.Y, r.Width - 1, r.Height - 1);

            // Small window control buttons at the right.
            int bx = r.Right - 44;
            for (int i = 0; i < 3; i++)
            {
                Rectangle wb = new Rectangle(bx + i * 13, r.Y + 3, 9, 9);
                using (SolidBrush sb = new SolidBrush(Color.FromArgb(228, 234, 242))) g.FillRectangle(sb, wb);
                using (Pen bp = new Pen(Color.FromArgb(55, 65, 80))) g.DrawRectangle(bp, wb);
            }

            DrawPlatformFileGlyph(g, new Rectangle(r.X + 8, r.Y + title.Height + 3, 20, Math.Max(10, r.Height - title.Height - 6)), p.Label);
            using (Font f = Font(8f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(235, 255, 255, 255)))
                g.DrawString(p.Label, f, b, new Rectangle(r.X + 30, r.Y + 1, r.Width - 82, title.Height - 1), LeftMiddle());

            // Subtle slots like taskbar/file-list rows.
            using (Pen row = new Pen(Color.FromArgb(42, 255, 255, 255)))
            {
                for (int x = r.X + 34; x < r.Right - 10; x += 54)
                    g.DrawLine(row, x, body.Y + 5, x + 24, body.Y + 5);
            }
        }



        private static void DrawOpenedFileShell(Graphics g, Rectangle view, DungeonInfo dungeon, float cameraX, int tick)
        {
            // 던전 내부가 실제 파일을 열어본 공간처럼 느껴지도록 파일 탐색기/속성/코드 뷰를 배경에 깔아 둡니다.
            int scroll = (int)(cameraX * 0.11f) % 520;
            for (int i = -1; i < 4; i++)
            {
                int x = view.X + i * 520 - scroll + 36;
                Rectangle doc = new Rectangle(x, view.Y + 95 + (i % 2) * 36, 360, 245);
                using (SolidBrush sh = new SolidBrush(Color.FromArgb(34, 0, 0, 0))) g.FillRectangle(sh, doc.X + 8, doc.Y + 8, doc.Width, doc.Height);
                Panel(g, doc, Color.FromArgb(232, 238, 247));
                Rectangle title = new Rectangle(doc.X + 5, doc.Y + 5, doc.Width - 10, 28);
                using (LinearGradientBrush tb = new LinearGradientBrush(title, Color.FromArgb(30, 70, 160), Lighten(dungeon.Accent, 25), 0f))
                    g.FillRectangle(tb, title);
                using (Font f = Font(8.5f, FontStyle.Bold))
                using (SolidBrush b = new SolidBrush(Color.White))
                    g.DrawString(i % 2 == 0 ? dungeon.FileName : "Properties - " + dungeon.FileName, f, b, new Rectangle(title.X + 10, title.Y, title.Width - 66, title.Height), LeftMiddle());
                for (int k = 0; k < 3; k++)
                {
                    Rectangle btn = new Rectangle(title.Right - 48 + k * 14, title.Y + 7, 9, 9);
                    using (SolidBrush bb = new SolidBrush(Color.FromArgb(230, 238, 245))) g.FillRectangle(bb, btn);
                    using (Pen bp = new Pen(Color.FromArgb(65, 70, 90))) g.DrawRectangle(bp, btn);
                }
                Rectangle content = new Rectangle(doc.X + 18, doc.Y + 48, doc.Width - 36, doc.Height - 68);
                DrawFileRows(g, content, dungeon, tick + i * 11);
            }

            // 왼쪽 폴더 트리
            Rectangle tree = new Rectangle(view.X + 18, view.Y + 118, 178, Math.Min(330, view.Height - 230));
            using (SolidBrush sh = new SolidBrush(Color.FromArgb(30, 0, 0, 0))) g.FillRectangle(sh, tree.X + 7, tree.Y + 7, tree.Width, tree.Height);
            Panel(g, tree, Color.FromArgb(226, 234, 243));
            Header(g, new Rectangle(tree.X + 6, tree.Y + 6, tree.Width - 12, 28), "파일 트리");
            string[] nodes = new string[] { "▸ Dungeons", "  ▸ " + ShortName(dungeon.FileName), "    data.bin", "    monster.db", "    map.layout", "    boss.ai", "    reward.tbl" };
            using (Font f = Font(8.2f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(36, 52, 82)))
            {
                for (int i = 0; i < nodes.Length; i++)
                    g.DrawString(nodes[i], f, b, new Rectangle(tree.X + 14, tree.Y + 42 + i * 27, tree.Width - 28, 20), LeftMiddle());
            }
        }

        private static string ShortName(string file)
        {
            if (file.Length <= 18) return file;
            return file.Substring(0, 15) + "...";
        }

        private static void DrawFileRows(Graphics g, Rectangle r, DungeonInfo dungeon, int tick)
        {
            Inset(g, r, Color.FromArgb(246, 250, 255));
            using (Pen p = new Pen(Color.FromArgb(90, 160, 190, 220)))
            {
                for (int y = r.Y + 26; y < r.Bottom - 6; y += 26) g.DrawLine(p, r.X + 8, y, r.Right - 8, y);
                for (int x = r.X + 74; x < r.Right - 8; x += 88) g.DrawLine(p, x, r.Y + 8, x, r.Bottom - 8);
            }
            string[] names = new string[] { "header", "spawn", "platform", "skill", "reward", "portal" };
            using (Font f = Font(7.6f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(70, dungeon.Accent)))
            using (SolidBrush dark = new SolidBrush(Color.FromArgb(48, 64, 90)))
            {
                for (int i = 0; i < 6; i++)
                {
                    int y = r.Y + 10 + i * 26;
                    g.DrawString(names[i % names.Length], f, dark, new Rectangle(r.X + 12, y, 66, 18), LeftMiddle());
                    g.DrawString("0x" + ((tick * 19 + i * 941) % 65535).ToString("X4"), f, b, new Rectangle(r.X + 84, y, 80, 18), LeftMiddle());
                    g.DrawString(i % 2 == 0 ? "FILE_OK" : "ENCRYPT", f, b, new Rectangle(r.X + 172, y, 90, 18), LeftMiddle());
                }
            }
        }

        private static void DrawDungeonParallaxWindows(Graphics g, Rectangle view, DungeonInfo dungeon, float cameraX, int tick)
        {
            int offset1 = (int)(cameraX * 0.16f) % 360;
            for (int i = -1; i < 6; i++)
            {
                int x = view.X + i * 360 - offset1;
                Rectangle win = new Rectangle(x + 26, view.Y + 82 + (i % 2) * 30, 235, 132);
                DrawGlassWindow(g, win, dungeon.Accent, i % 3 == 0 ? "Explorer.exe" : i % 3 == 1 ? "Properties" : "System Log");
            }

            int offset2 = (int)(cameraX * 0.32f) % 290;
            for (int i = -1; i < 8; i++)
            {
                int x = view.X + i * 290 - offset2;
                Rectangle icon = new Rectangle(x + 70, view.Bottom - 212 + (i % 3) * 18, 54, 54);
                DrawDesktopFileProp(g, icon, dungeon.Accent, i % 4);
            }
        }

        private static void DrawCircuitGrid(Graphics g, Rectangle view, Color accent, float cameraX, int tick)
        {
            using (Pen grid = new Pen(Color.FromArgb(26, 160, 220, 255)))
            {
                int offset = (int)(cameraX * 0.42f) % 42;
                for (int x = view.X - offset; x < view.Right; x += 42) g.DrawLine(grid, x, view.Y, x, view.Bottom);
                for (int y = view.Y; y < view.Bottom; y += 38) g.DrawLine(grid, view.X, y, view.Right, y);
            }
            using (Pen wire = new Pen(Color.FromArgb(70, accent), 2f))
            {
                int off = (int)(cameraX * 0.22f) % 260;
                for (int x = view.X - off; x < view.Right; x += 260)
                {
                    Point[] pts = new Point[] {
                        new Point(x, view.Bottom - 150), new Point(x + 46, view.Bottom - 150),
                        new Point(x + 66, view.Bottom - 118), new Point(x + 128, view.Bottom - 118),
                        new Point(x + 158, view.Bottom - 84), new Point(x + 236, view.Bottom - 84)
                    };
                    g.DrawLines(wire, pts);
                    using (SolidBrush node = new SolidBrush(Color.FromArgb(110, accent)))
                    {
                        for (int i = 0; i < pts.Length; i += 2) g.FillEllipse(node, pts[i].X - 3, pts[i].Y - 3, 6, 6);
                    }
                }
            }
        }

        private static void DrawFloatingCode(Graphics g, Rectangle view, DungeonInfo dungeon, float cameraX, int tick)
        {
            string[] bits = new string[] { "0x", "01", "FILE", "DLL", "SYS", "ERR", "WIN", "EXE", "INI", "REG" };
            using (Font f = Font(8.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(50, 210, 240, 255)))
            {
                for (int i = 0; i < 34; i++)
                {
                    int x = view.X + (int)((i * 91 - cameraX * 0.28f + tick * 0.45f) % (view.Width + 180)) - 70;
                    int y = view.Y + 54 + (i * 43) % Math.Max(1, view.Height - 132);
                    g.DrawString(bits[i % bits.Length], f, b, x, y);
                }
            }
        }

        private static void DrawDungeonThemeProps(Graphics g, Rectangle view, DungeonInfo dungeon, float cameraX, int tick)
        {
            int theme = (int)dungeon.Type;
            int offset = (int)(cameraX * 0.55f) % 520;
            for (int i = -1; i < 5; i++)
            {
                int x = view.X + i * 520 - offset + 70;
                int y = view.Bottom - 306 + (i % 2) * 28;
                if (theme == 0) DrawFolderTree(g, new Rectangle(x, y, 126, 148), dungeon.Accent);
                else if (theme == 1) DrawRecycleContainer(g, new Rectangle(x, y + 8, 126, 142), dungeon.Accent);
                else if (theme == 2) DrawControlWidget(g, new Rectangle(x, y, 136, 145), dungeon.Accent);
                else if (theme == 3) DrawSystemWarningPanel(g, new Rectangle(x, y, 150, 142), dungeon.Accent);
                else DrawBsodPanel(g, new Rectangle(x, y, 160, 145), dungeon.Accent, tick + i * 7);
            }
        }

        private static void DrawExplorerBreadcrumb(Graphics g, Rectangle view, DungeonInfo dungeon, float cameraX)
        {
            Rectangle bar = new Rectangle(view.X + 18, view.Y + 42, Math.Min(760, view.Width - 36), 34);
            using (SolidBrush shadow = new SolidBrush(Color.FromArgb(70, 0, 0, 0))) g.FillRectangle(shadow, bar.X + 4, bar.Y + 4, bar.Width, bar.Height);
            Panel(g, bar, Color.FromArgb(232, 238, 247));
            DrawPlatformFileGlyph(g, new Rectangle(bar.X + 12, bar.Y + 6, 22, 22), dungeon.FileName);
            string path = "C:\\WindowsKingdom\\Dungeons\\" + dungeon.FileName;
            using (Font f = Font(9.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(28, 42, 64)))
                g.DrawString(path, f, b, new Rectangle(bar.X + 42, bar.Y + 4, bar.Width - 54, bar.Height - 8), LeftMiddle());
        }

        private static void DrawGlassWindow(Graphics g, Rectangle r, Color accent, string title)
        {
            using (SolidBrush sh = new SolidBrush(Color.FromArgb(36, 0, 0, 0))) g.FillRectangle(sh, r.X + 8, r.Y + 8, r.Width, r.Height);
            using (LinearGradientBrush b = new LinearGradientBrush(r, Color.FromArgb(58, Lighten(accent, 30)), Color.FromArgb(34, Darken(accent, 20)), 90f))
                g.FillRectangle(b, r);
            Rectangle titleBar = new Rectangle(r.X, r.Y, r.Width, 24);
            using (LinearGradientBrush tb = new LinearGradientBrush(titleBar, Color.FromArgb(34, 86, 180), Color.FromArgb(80, 170, 240), 0f))
                g.FillRectangle(tb, titleBar);
            using (Pen p = new Pen(Color.FromArgb(100, 210, 240, 255))) g.DrawRectangle(p, r.X, r.Y, r.Width - 1, r.Height - 1);
            using (Font f = Font(7.8f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(230, 255, 255, 255)))
                g.DrawString(title, f, b, new Rectangle(r.X + 8, r.Y + 3, r.Width - 58, 20), LeftMiddle());
            for (int i = 0; i < 3; i++)
            {
                Rectangle btn = new Rectangle(r.Right - 52 + i * 15, r.Y + 6, 10, 10);
                using (SolidBrush bb = new SolidBrush(Color.FromArgb(210, 238, 245, 255))) g.FillRectangle(bb, btn);
                using (Pen bp = new Pen(Color.FromArgb(100, 50, 70, 90))) g.DrawRectangle(bp, btn);
            }
            using (Pen line = new Pen(Color.FromArgb(40, 255, 255, 255)))
            {
                for (int y = r.Y + 42; y < r.Bottom - 12; y += 22) g.DrawLine(line, r.X + 14, y, r.Right - 16, y);
            }
        }

        private static void DrawDesktopFileProp(Graphics g, Rectangle r, Color accent, int kind)
        {
            if (kind == 0 || kind == 1)
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(225, 210, 150, 35)))
                {
                    g.FillRectangle(b, r.X + 5, r.Y + 19, r.Width - 10, r.Height - 20);
                    g.FillRectangle(b, r.X + 10, r.Y + 11, r.Width / 2, 15);
                }
                using (Pen p = new Pen(Color.FromArgb(90, 60, 20))) g.DrawRectangle(p, r.X + 5, r.Y + 19, r.Width - 10, r.Height - 20);
            }
            else
            {
                DrawLargeFileSymbol(g, r, accent, false);
            }
        }

        private static void DrawFolderTree(Graphics g, Rectangle r, Color accent)
        {
            using (SolidBrush trunk = new SolidBrush(Color.FromArgb(98, 62, 30))) g.FillRectangle(trunk, r.X + r.Width / 2 - 8, r.Bottom - 42, 16, 42);
            using (SolidBrush shadow = new SolidBrush(Color.FromArgb(40, 0, 0, 0))) g.FillEllipse(shadow, r.X + 12, r.Bottom - 14, r.Width - 24, 12);
            for (int i = 0; i < 4; i++)
            {
                Rectangle folder = new Rectangle(r.X + 18 + (i % 2) * 32, r.Y + 15 + i * 22, 54, 38);
                using (SolidBrush b = new SolidBrush(Color.FromArgb(235, 196, 58)))
                {
                    g.FillRectangle(b, folder.X, folder.Y + 12, folder.Width, folder.Height - 12);
                    g.FillRectangle(b, folder.X + 6, folder.Y + 5, 28, 14);
                }
                using (Pen p = new Pen(Color.FromArgb(100, 75, 25))) g.DrawRectangle(p, folder.X, folder.Y + 12, folder.Width, folder.Height - 12);
            }
            using (Pen p = new Pen(Color.FromArgb(130, accent), 2f)) g.DrawEllipse(p, r.X + 10, r.Y + 6, r.Width - 20, r.Height - 18);
        }

        private static void DrawRecycleContainer(Graphics g, Rectangle r, Color accent)
        {
            Rectangle bin = new Rectangle(r.X + 26, r.Y + 28, r.Width - 52, r.Height - 34);
            using (LinearGradientBrush b = new LinearGradientBrush(bin, Color.FromArgb(210, 230, 235), Color.FromArgb(105, 130, 140), 90f)) g.FillRectangle(b, bin);
            using (Pen p = new Pen(Color.FromArgb(45, 70, 75), 2f)) g.DrawRectangle(p, bin);
            using (Pen p = new Pen(Color.FromArgb(80, 220, 120), 4f)) g.DrawArc(p, bin.X + 18, bin.Y + 20, bin.Width - 36, bin.Height - 42, 35, 260);
            using (SolidBrush lid = new SolidBrush(Color.FromArgb(190, 205, 210))) g.FillRectangle(lid, bin.X - 8, bin.Y - 12, bin.Width + 16, 14);
            using (SolidBrush aura = new SolidBrush(Color.FromArgb(48, accent))) g.FillEllipse(aura, r.X + 4, r.Y + 4, r.Width - 8, r.Height - 10);
        }

        private static void DrawControlWidget(Graphics g, Rectangle r, Color accent)
        {
            DrawGlassWindow(g, r, accent, "Control Panel");
            using (Pen p = new Pen(Color.FromArgb(190, 255, 255, 255), 3f))
            {
                g.DrawEllipse(p, r.X + 22, r.Y + 44, 42, 42);
                g.DrawLine(p, r.X + 76, r.Y + 52, r.Right - 24, r.Y + 52);
                g.DrawLine(p, r.X + 76, r.Y + 74, r.Right - 24, r.Y + 74);
                g.DrawLine(p, r.X + 28, r.Bottom - 34, r.Right - 28, r.Bottom - 34);
            }
            using (SolidBrush knob = new SolidBrush(Color.FromArgb(190, accent)))
            {
                g.FillEllipse(knob, r.X + 36, r.Y + 58, 14, 14);
                g.FillEllipse(knob, r.X + 98, r.Y + 45, 14, 14);
                g.FillEllipse(knob, r.X + 132, r.Y + 67, 14, 14);
            }
        }

        private static void DrawSystemWarningPanel(Graphics g, Rectangle r, Color accent)
        {
            DrawGlassWindow(g, r, accent, "System32.sys");
            Point[] tri = new Point[] { new Point(r.X + r.Width / 2, r.Y + 42), new Point(r.X + r.Width / 2 - 34, r.Y + 105), new Point(r.X + r.Width / 2 + 34, r.Y + 105) };
            using (SolidBrush b = new SolidBrush(Color.FromArgb(230, 245, 170, 30))) g.FillPolygon(b, tri);
            using (Pen p = new Pen(Color.FromArgb(170, 40, 20), 3f)) g.DrawPolygon(p, tri);
            using (Font f = Font(24f, FontStyle.Bold)) g.DrawString("!", f, Brushes.DarkRed, new Rectangle(r.X, r.Y + 52, r.Width, 56), Center());
            using (Font f = Font(8f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.White)) g.DrawString("ACCESS DENIED", f, b, new Rectangle(r.X, r.Bottom - 28, r.Width, 18), Center());
        }

        private static void DrawBsodPanel(Graphics g, Rectangle r, Color accent, int tick)
        {
            using (SolidBrush sh = new SolidBrush(Color.FromArgb(52, 0, 0, 0))) g.FillRectangle(sh, r.X + 6, r.Y + 6, r.Width, r.Height);
            using (LinearGradientBrush b = new LinearGradientBrush(r, Color.FromArgb(10, 30, 130), Color.FromArgb(45, 110, 245), 90f)) g.FillRectangle(b, r);
            using (Pen p = new Pen(Color.FromArgb(170, 200, 230, 255), 2f)) g.DrawRectangle(p, r.X, r.Y, r.Width - 1, r.Height - 1);
            using (Font f = Font(8f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.White))
            {
                g.DrawString("BLUE SCREEN", f, b, new Rectangle(r.X + 8, r.Y + 10, r.Width - 16, 18), Center());
                g.DrawString("STOP: 0x" + ((tick * 113) % 999999).ToString("X6"), f, b, new Rectangle(r.X + 10, r.Y + 42, r.Width - 20, 18), Center());
                g.DrawString("DRAGON_FAULT", f, b, new Rectangle(r.X + 10, r.Y + 68, r.Width - 20, 18), Center());
            }
        }

        private static void DrawPlatformFileGlyph(Graphics g, Rectangle r, string label)
        {
            string lower = label.ToLowerInvariant();
            if (lower.Contains("folder") || lower.Contains("desktop") || lower.Contains("explorer"))
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(244, 198, 52)))
                {
                    g.FillRectangle(b, r.X + 2, r.Y + 8, r.Width - 4, r.Height - 9);
                    g.FillRectangle(b, r.X + 4, r.Y + 3, Math.Max(8, r.Width / 2), 9);
                }
                using (Pen p = new Pen(Color.FromArgb(106, 78, 20))) g.DrawRectangle(p, r.X + 2, r.Y + 8, r.Width - 4, r.Height - 9);
            }
            else if (lower.Contains("bin") || lower.Contains("recycle"))
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(205, 220, 220))) g.FillRectangle(b, r.X + 4, r.Y + 5, r.Width - 8, r.Height - 7);
                using (Pen p = new Pen(Color.FromArgb(70, 160, 90), 2f)) g.DrawArc(p, r.X + 7, r.Y + 8, Math.Max(4, r.Width - 14), Math.Max(4, r.Height - 14), 30, 260);
            }
            else if (lower.Contains("sys") || lower.Contains("kernel"))
            {
                Point[] tri = new Point[] { new Point(r.X + r.Width / 2, r.Y + 2), new Point(r.Right - 3, r.Bottom - 3), new Point(r.X + 3, r.Bottom - 3) };
                using (SolidBrush b = new SolidBrush(Color.FromArgb(244, 176, 34))) g.FillPolygon(b, tri);
                using (Pen p = new Pen(Color.DarkRed)) g.DrawPolygon(p, tri);
            }
            else if (lower.Contains("dll") || lower.Contains("cpl") || lower.Contains("reg"))
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(70, 140, 230))) g.FillEllipse(b, r.X + 3, r.Y + 3, r.Width - 6, r.Height - 6);
                using (Pen p = new Pen(Color.White, 2f))
                {
                    g.DrawLine(p, r.X + r.Width / 2, r.Y + 6, r.X + r.Width / 2, r.Bottom - 6);
                    g.DrawLine(p, r.X + 6, r.Y + r.Height / 2, r.Right - 6, r.Y + r.Height / 2);
                }
            }
            else
            {
                Point[] paper = new Point[] {
                    new Point(r.X + 4, r.Y + 2), new Point(r.Right - 7, r.Y + 2),
                    new Point(r.Right - 2, r.Y + 7), new Point(r.Right - 2, r.Bottom - 2),
                    new Point(r.X + 4, r.Bottom - 2)
                };
                using (SolidBrush b = new SolidBrush(Color.FromArgb(238, 246, 255))) g.FillPolygon(b, paper);
                using (Pen p = new Pen(Color.FromArgb(70, 110, 170), 1f)) g.DrawPolygon(p, paper);
                using (Pen p = new Pen(Color.FromArgb(120, 90, 160, 230), 1.5f))
                {
                    g.DrawLine(p, r.X + 7, r.Y + 10, r.Right - 5, r.Y + 10);
                    g.DrawLine(p, r.X + 7, r.Y + 15, r.Right - 8, r.Y + 15);
                }
            }
        }


        public static void DrawPlayer(Graphics g, Player player, float cameraX, int tick, bool moving)
        {
            float sx = player.X - cameraX;
            float sy = player.Y;
            JobProfile job = player.Profile;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int bob = moving ? (int)(Math.Sin(tick / 4.0) * 3) : 0;
            int ox = (int)sx;
            int oy = (int)sy + bob;
            int auraPulse = 6 + (int)(Math.Sin(tick / 6.0) * 5);

            using (GraphicsPath shadowPath = new GraphicsPath())
            {
                shadowPath.AddEllipse(ox - 36, oy + 1, 72, 16);
                using (PathGradientBrush sh = new PathGradientBrush(shadowPath))
                {
                    sh.CenterColor = Color.FromArgb(110, 0, 0, 0);
                    sh.SurroundColors = new Color[] { Color.FromArgb(0, 0, 0, 0) };
                    g.FillPath(sh, shadowPath);
                }
            }

            using (SolidBrush aura = new SolidBrush(Color.FromArgb(24, job.MainColor)))
                g.FillEllipse(aura, ox - 58 - auraPulse / 2, oy - 92 - auraPulse / 2, 116 + auraPulse, 124 + auraPulse);
            using (Pen aura1 = new Pen(Color.FromArgb(110, job.MainColor), 3f))
                g.DrawArc(aura1, ox - 54, oy - 88, 108, 114, (tick * 4) % 360, 220);
            using (Pen aura2 = new Pen(Color.FromArgb(70, Lighten(job.MainColor, 50)), 2f))
                g.DrawArc(aura2, ox - 46, oy - 78, 92, 96, 180 - (tick * 3) % 360, 170);

            for (int i = 0; i < 4; i++)
            {
                double a = (tick / 9.0) + i * Math.PI / 2.0;
                int px = ox + (int)(Math.Cos(a) * (28 + i * 5));
                int py = oy - 48 + (int)(Math.Sin(a * 1.5) * (14 + i * 2));
                DrawGlowDot(g, px, py, 4 + (i % 2), job.MainColor, 90);
            }

            DrawJobCharacter(g, ox, oy, player.Facing, player.Job, job.MainColor);

            if (player.InvincibleTicks > 0)
            {
                using (Pen p = new Pen(Color.FromArgb(180, Color.White), 3f))
                    g.DrawEllipse(p, ox - 40, oy - 76, 80, 88);
                using (Pen p = new Pen(Color.FromArgb(100, Lighten(job.MainColor, 70)), 1.8f))
                    g.DrawArc(p, ox - 50, oy - 86, 100, 108, (tick * 7) % 360, 200);
            }
            if (player.ShieldTicks > 0)
            {
                using (Pen p = new Pen(Color.FromArgb(160, 120, 200, 255), 4.5f))
                    g.DrawEllipse(p, ox - 49, oy - 84, 98, 100);
                using (Pen p = new Pen(Color.FromArgb(90, Color.White), 2f))
                    g.DrawEllipse(p, ox - 40, oy - 74, 80, 82);
            }
        }


        private static void DrawJobCharacter(Graphics g, int ox, int oy, int facing, JobType job, Color accent)
        {
            Color outline = Color.FromArgb(26, 32, 45);
            Rectangle body = new Rectangle(ox - 16, oy - 48, 32, 42);

            if (job == JobType.DebugWarrior || job == JobType.FirewallKnight)
            {
                using (SolidBrush cape = new SolidBrush(Color.FromArgb(180, Darken(accent, 30))))
                {
                    Point[] capePts = new Point[]
                    {
                        new Point(ox - 11, oy - 44), new Point(ox - 30, oy - 4), new Point(ox + 26, oy + 8), new Point(ox + 13, oy - 42)
                    };
                    g.FillPolygon(cape, capePts);
                }
            }

            using (SolidBrush skin = new SolidBrush(Color.FromArgb(239, 198, 154)))
                g.FillEllipse(skin, ox - 13, oy - 70, 26, 26);
            using (Pen p = new Pen(outline, 1.4f))
                g.DrawEllipse(p, ox - 13, oy - 70, 26, 26);

            using (SolidBrush hair = new SolidBrush(job == JobType.VaccineMage ? Color.FromArgb(52, 124, 66) : job == JobType.FileExplorer ? Color.FromArgb(116, 78, 36) : Color.FromArgb(20, 24, 34)))
            {
                g.FillPie(hair, ox - 16, oy - 75, 32, 22, 180, 180);
                g.FillPolygon(hair, new Point[]
                {
                    new Point(ox - 15, oy - 62), new Point(ox - 24, oy - 54), new Point(ox - 4, oy - 59), new Point(ox + 10, oy - 57), new Point(ox + 15, oy - 65)
                });
            }
            using (SolidBrush eye = new SolidBrush(Color.FromArgb(18, 24, 34)))
            {
                int ex = facing >= 0 ? 3 : -8;
                g.FillRectangle(eye, ox + ex, oy - 59, 4, 3);
            }
            using (Pen smile = new Pen(Color.FromArgb(120, 80, 50), 1.2f))
                g.DrawArc(smile, ox - 5, oy - 55, 10, 6, 10, 160);

            using (LinearGradientBrush suit = new LinearGradientBrush(body, Lighten(accent, 25), Darken(accent, 18), 90f))
                g.FillRectangle(suit, body);
            using (Pen p = new Pen(outline, 2f))
                g.DrawRectangle(p, body);
            using (SolidBrush highlight = new SolidBrush(Color.FromArgb(55, 255, 255, 255)))
                g.FillRectangle(highlight, body.X + 3, body.Y + 3, body.Width - 6, Math.Max(8, body.Height / 3));

            using (SolidBrush belt = new SolidBrush(Color.FromArgb(50, 58, 74))) g.FillRectangle(belt, ox - 16, oy - 19, 32, 6);
            using (SolidBrush glove = new SolidBrush(Color.FromArgb(230, 235, 242)))
            {
                g.FillEllipse(glove, ox - 23, oy - 34, 10, 10);
                g.FillEllipse(glove, ox + 13, oy - 34, 10, 10);
            }
            using (Pen limb = new Pen(Color.FromArgb(28, 34, 44), 5f))
            {
                g.DrawLine(limb, ox - 10, oy - 6, ox - 18, oy + 13);
                g.DrawLine(limb, ox + 10, oy - 6, ox + 18, oy + 13);
            }
            using (SolidBrush boot = new SolidBrush(Color.FromArgb(30, 32, 42)))
            {
                g.FillRectangle(boot, ox - 22, oy + 10, 10, 5);
                g.FillRectangle(boot, ox + 12, oy + 10, 10, 5);
            }

            if (job == JobType.VaccineMage)
            {
                using (SolidBrush robe = new SolidBrush(Color.FromArgb(225, 246, 224)))
                    g.FillPolygon(robe, new Point[] { new Point(ox, body.Y + 2), new Point(ox - 24, body.Bottom + 16), new Point(ox + 24, body.Bottom + 16) });
                using (Pen robeLine = new Pen(Color.FromArgb(145, 210, 160), 2f))
                {
                    g.DrawLine(robeLine, ox, oy - 44, ox, oy + 10);
                    g.DrawArc(robeLine, ox - 14, oy - 24, 28, 18, 0, 180);
                }
                using (Pen staff = new Pen(Color.FromArgb(120, 90, 50), 5f))
                    g.DrawLine(staff, ox + facing * 12, oy - 32, ox + facing * 42, oy - 77);
                DrawGlowDot(g, ox + facing * 42, oy - 78, 10, Color.FromArgb(120, 255, 150), 120);
                using (Pen magicRing = new Pen(Color.FromArgb(160, 120, 255, 170), 2f))
                    g.DrawEllipse(magicRing, ox + facing * 42 - 10, oy - 88, 20, 20);
            }
            else if (job == JobType.FirewallKnight)
            {
                using (SolidBrush armor = new SolidBrush(Color.FromArgb(158, 166, 180)))
                {
                    g.FillRectangle(armor, body.X - 1, body.Y - 2, body.Width + 2, body.Height + 4);
                    g.FillRectangle(armor, ox - 10, oy - 57, 20, 10);
                }
                using (Pen armorLine = new Pen(Color.White, 1.5f))
                {
                    g.DrawLine(armorLine, ox, oy - 47, ox, oy - 14);
                    g.DrawLine(armorLine, ox - 12, oy - 28, ox + 12, oy - 28);
                }
                DrawShield(g, ox - facing * 29, oy - 41, facing, Color.FromArgb(68, 120, 220));
                using (Pen bladeGlow = new Pen(Color.FromArgb(180, 110, 240, 255), 6f))
                    g.DrawLine(bladeGlow, ox + facing * 15, oy - 35, ox + facing * 50, oy - 70);
                using (Pen bladeCore = new Pen(Color.White, 2f))
                    g.DrawLine(bladeCore, ox + facing * 15, oy - 35, ox + facing * 50, oy - 70);
            }
            else if (job == JobType.FileExplorer)
            {
                using (SolidBrush coat = new SolidBrush(Color.FromArgb(160, 110, 48)))
                    g.FillRectangle(coat, body);
                using (SolidBrush shirt = new SolidBrush(Color.FromArgb(248, 239, 214)))
                    g.FillRectangle(shirt, ox - 6, oy - 42, 12, 20);
                using (SolidBrush hat = new SolidBrush(Color.FromArgb(198, 145, 50)))
                {
                    g.FillRectangle(hat, ox - 20, oy - 74, 40, 8);
                    g.FillRectangle(hat, ox - 11, oy - 80, 22, 8);
                }
                using (Pen lens = new Pen(accent, 4f))
                {
                    g.DrawEllipse(lens, ox + facing * 21 - 10, oy - 47, 20, 20);
                    g.DrawLine(lens, ox + facing * 28, oy - 32, ox + facing * 44, oy - 16);
                }
                DrawGlowDot(g, ox + facing * 22, oy - 37, 3, accent, 100);
            }
            else
            {
                using (SolidBrush armor = new SolidBrush(Color.FromArgb(38, 76, 152)))
                    g.FillRectangle(armor, body);
                using (SolidBrush shoulder = new SolidBrush(Color.FromArgb(58, 116, 210)))
                {
                    g.FillEllipse(shoulder, ox - 24, oy - 47, 14, 12);
                    g.FillEllipse(shoulder, ox + 10, oy - 47, 14, 12);
                }
                using (Pen chestGlow = new Pen(Color.FromArgb(180, 125, 225, 255), 2f))
                {
                    g.DrawArc(chestGlow, ox - 12, oy - 40, 24, 20, 200, 140);
                    g.DrawLine(chestGlow, ox, oy - 42, ox, oy - 16);
                }
                DrawShield(g, ox - facing * 30, oy - 38, facing, Color.FromArgb(55, 105, 210));
                using (Pen bladeGlow = new Pen(Color.FromArgb(200, 100, 235, 255), 6f))
                    g.DrawLine(bladeGlow, ox + facing * 14, oy - 35, ox + facing * 52, oy - 73);
                using (Pen bladeCore = new Pen(Color.White, 2f))
                    g.DrawLine(bladeCore, ox + facing * 14, oy - 35, ox + facing * 52, oy - 73);
            }
        }


        private static void DrawShield(Graphics g, int x, int y, int facing, Color color)
        {
            int w = 29;
            int h = 38;
            Point[] s = new Point[]
            {
                new Point(x, y),
                new Point(x + facing * w, y + 8),
                new Point(x + facing * (w - 5), y + h - 6),
                new Point(x, y + h),
                new Point(x - facing * (w - 5), y + h - 6),
                new Point(x - facing * w, y + 8)
            };
            using (SolidBrush b = new SolidBrush(color)) g.FillPolygon(b, s);
            using (GraphicsPath gp = new GraphicsPath())
            {
                gp.AddPolygon(s);
                using (PathGradientBrush pgb = new PathGradientBrush(gp))
                {
                    pgb.CenterColor = Color.FromArgb(150, Color.White);
                    pgb.SurroundColors = new Color[] { Color.FromArgb(0, color) };
                    g.FillPath(pgb, gp);
                }
            }
            using (Pen p = new Pen(Color.White, 2f))
            {
                g.DrawLine(p, x, y + 6, x, y + h - 7);
                g.DrawLine(p, x - facing * 18, y + h / 2, x + facing * 18, y + h / 2);
            }
            using (Pen p = new Pen(Color.FromArgb(26, 36, 56), 2f)) g.DrawPolygon(p, s);
            DrawGlowDot(g, x, y + h / 2, 4, Lighten(color, 50), 110);
        }

        private static void DrawGlowDot(Graphics g, int x, int y, int radius, Color color, int alpha)
        {
            int a = ClampAlpha(alpha);
            using (SolidBrush glow = new SolidBrush(Color.FromArgb(ClampAlpha(a / 2), color)))
                g.FillEllipse(glow, x - radius * 2, y - radius * 2, radius * 4, radius * 4);
            using (SolidBrush core = new SolidBrush(Color.FromArgb(a, color)))
                g.FillEllipse(core, x - radius, y - radius, radius * 2, radius * 2);
            using (Pen p = new Pen(Color.FromArgb(a, Color.White), 1.2f))
                g.DrawEllipse(p, x - radius, y - radius, radius * 2, radius * 2);
        }

        public static void DrawMonster(Graphics g, Monster m, float cameraX, int tick)
        {
            RectangleF br = m.Bounds;
            Rectangle r = Rectangle.Round(new RectangleF(br.X - cameraX, br.Y, br.Width, br.Height));
            using (SolidBrush sh = new SolidBrush(Color.FromArgb(90, 0, 0, 0))) g.FillEllipse(sh, r.X + 4, r.Bottom - 12, r.Width - 8, 12);
            using (SolidBrush aura = new SolidBrush(Color.FromArgb(m.HitFlash > 0 ? 80 : 28, m.MainColor))) g.FillEllipse(aura, r.X - 12, r.Y - 12, r.Width + 24, r.Height + 24);
            if (m.Kind == MonsterKind.Dragon) DrawDragon(g, r, m.MainColor, tick);
            else if (m.Kind == MonsterKind.Wolf || m.Kind == MonsterKind.Hound) DrawWolf(g, r, m.MainColor, tick);
            else if (m.Kind == MonsterKind.Bat) DrawBat(g, r, m.MainColor, tick);
            else if (m.Kind == MonsterKind.Ghost) DrawGhost(g, r, m.MainColor, tick);
            else if (m.Kind == MonsterKind.Spider) DrawSpider(g, r, m.MainColor, tick);
            else if (m.Kind == MonsterKind.Golem) DrawGolem(g, r, m.MainColor, tick);
            else if (m.Kind == MonsterKind.Skeleton) DrawSkeleton(g, r, tick);
            else if (m.Kind == MonsterKind.Serpent) DrawSerpent(g, r, m.MainColor, tick);
            else if (m.Kind == MonsterKind.Goblin) DrawGoblin(g, r, m.MainColor, tick);
            else DrawSlime(g, r, m.MainColor, tick);

            Rectangle hp = new Rectangle(r.X, r.Y - 18, r.Width, 7);
            Bar(g, hp, m.Hp, m.MaxHp, m.IsBoss ? Color.Red : Color.OrangeRed);
            using (Font f = Font(7f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.White))
            using (SolidBrush back = new SolidBrush(Color.FromArgb(140, 0, 0, 0)))
            {
                Rectangle name = new Rectangle(r.X - 20, r.Y - 36, r.Width + 40, 16);
                g.FillRectangle(back, name);
                g.DrawString(m.KoreanName, f, b, name, Center());
            }
        }

        private static void DrawSlime(Graphics g, Rectangle r, Color c, int t)
        {
            int x = r.X + r.Width / 2;
            int y = r.Bottom - 30 + (int)(Math.Sin(t / 8.0) * 3);
            using (SolidBrush b = new SolidBrush(c)) g.FillEllipse(b, x - 25, y - 32, 50, 40);
            using (Pen p = new Pen(Lighten(c, 60), 3f)) g.DrawEllipse(p, x - 25, y - 32, 50, 40);
            using (SolidBrush eye = new SolidBrush(Color.Black)) { g.FillEllipse(eye, x - 11, y - 18, 5, 5); g.FillEllipse(eye, x + 6, y - 18, 5, 5); }
        }

        private static void DrawGoblin(Graphics g, Rectangle r, Color c, int t)
        {
            int x = r.X + r.Width / 2; int y = r.Bottom - 48;
            using (SolidBrush b = new SolidBrush(c)) g.FillEllipse(b, x - 22, y - 34, 44, 32);
            using (SolidBrush b = new SolidBrush(Darken(c, 35))) g.FillRectangle(b, x - 16, y - 4, 32, 34);
            using (Pen p = new Pen(Color.SaddleBrown, 4f)) g.DrawLine(p, x + 18, y + 4, x + 42, y - 22);
            using (Pen p = new Pen(Color.Silver, 3f)) g.DrawLine(p, x + 41, y - 23, x + 52, y - 32);
            using (SolidBrush eye = new SolidBrush(Color.Red)) { g.FillRectangle(eye, x - 8, y - 22, 4, 3); g.FillRectangle(eye, x + 5, y - 22, 4, 3); }
        }

        private static void DrawBat(Graphics g, Rectangle r, Color c, int t)
        {
            int flap = (int)(Math.Sin(t / 5.0) * 12); int x = r.X + r.Width / 2; int y = r.Y + 36;
            using (SolidBrush b = new SolidBrush(c))
            {
                g.FillPolygon(b, new Point[] { new Point(x - 7, y + 12), new Point(x - 56, y - 10 - flap), new Point(x - 28, y + 36), new Point(x - 6, y + 24) });
                g.FillPolygon(b, new Point[] { new Point(x + 7, y + 12), new Point(x + 56, y - 10 - flap), new Point(x + 28, y + 36), new Point(x + 6, y + 24) });
                g.FillEllipse(b, x - 14, y + 4, 28, 32);
            }
            using (SolidBrush eye = new SolidBrush(Color.Yellow)) { g.FillEllipse(eye, x - 7, y + 17, 4, 4); g.FillEllipse(eye, x + 3, y + 17, 4, 4); }
        }

        private static void DrawWolf(Graphics g, Rectangle r, Color c, int t)
        {
            int x = r.X + r.Width / 2; int y = r.Bottom - 48;
            using (SolidBrush b = new SolidBrush(c))
            {
                g.FillEllipse(b, x - 38, y - 10, 68, 34);
                g.FillEllipse(b, x + 8, y - 22, 34, 28);
                g.FillPolygon(b, new Point[] { new Point(x + 13, y - 22), new Point(x + 19, y - 40), new Point(x + 26, y - 20) });
                g.FillPolygon(b, new Point[] { new Point(x + 29, y - 21), new Point(x + 43, y - 36), new Point(x + 40, y - 11) });
                g.FillPolygon(b, new Point[] { new Point(x - 38, y - 2), new Point(x - 62, y - 16), new Point(x - 52, y + 8) });
            }
            using (Pen p = new Pen(Color.FromArgb(180, 140, 80, 240), 3f))
            {
                g.DrawLine(p, x - 18, y - 8, x - 7, y - 28);
                g.DrawLine(p, x + 2, y - 7, x + 10, y - 28);
                g.DrawLine(p, x + 16, y - 4, x + 26, y - 23);
            }
            using (SolidBrush eye = new SolidBrush(Color.Red)) g.FillEllipse(eye, x + 28, y - 12, 6, 6);
            using (Pen p = new Pen(Color.FromArgb(25, 25, 30), 5f)) { g.DrawLine(p, x - 20, y + 20, x - 24, y + 40); g.DrawLine(p, x + 12, y + 20, x + 15, y + 40); }
        }

        private static void DrawGhost(Graphics g, Rectangle r, Color c, int t)
        {
            int x = r.X + r.Width / 2; int y = r.Bottom - 60 + (int)(Math.Sin(t / 7.0) * 5);
            using (SolidBrush b = new SolidBrush(Color.FromArgb(175, c)))
            {
                g.FillEllipse(b, x - 30, y - 48, 60, 52);
                g.FillPolygon(b, new Point[] { new Point(x - 30, y - 22), new Point(x - 18, y + 32), new Point(x, y + 8), new Point(x + 18, y + 32), new Point(x + 30, y - 22) });
            }
            using (SolidBrush eye = new SolidBrush(Color.FromArgb(5, 18, 40))) { g.FillEllipse(eye, x - 12, y - 30, 9, 12); g.FillEllipse(eye, x + 4, y - 30, 9, 12); }
        }

        private static void DrawSpider(Graphics g, Rectangle r, Color c, int t)
        {
            int x = r.X + r.Width / 2; int y = r.Bottom - 42;
            using (Pen leg = new Pen(Color.FromArgb(25, 25, 28), 4f))
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    g.DrawLine(leg, x + side * 14, y - 10, x + side * 48, y - 30);
                    g.DrawLine(leg, x + side * 18, y, x + side * 52, y + 4);
                    g.DrawLine(leg, x + side * 12, y + 12, x + side * 44, y + 28);
                }
            }
            using (SolidBrush b = new SolidBrush(c)) { g.FillEllipse(b, x - 25, y - 24, 50, 40); g.FillEllipse(b, x - 13, y - 42, 26, 24); }
            using (SolidBrush eye = new SolidBrush(Color.Cyan)) { g.FillEllipse(eye, x - 7, y - 34, 5, 5); g.FillEllipse(eye, x + 2, y - 34, 5, 5); }
        }

        private static void DrawGolem(Graphics g, Rectangle r, Color c, int t)
        {
            int x = r.X + r.Width / 2; int y = r.Bottom - 30;
            using (SolidBrush b = new SolidBrush(c))
            {
                g.FillRectangle(b, x - 32, y - 72, 64, 54);
                g.FillRectangle(b, x - 20, y - 95, 40, 28);
                g.FillRectangle(b, x - 48, y - 62, 18, 36);
                g.FillRectangle(b, x + 30, y - 62, 18, 36);
            }
            using (SolidBrush core = new SolidBrush(Color.FromArgb(90, 220, 255))) g.FillRectangle(core, x - 12, y - 55, 24, 22);
            using (Pen p = new Pen(Color.FromArgb(30, 40, 60), 3f)) g.DrawRectangle(p, x - 32, y - 72, 64, 54);
        }

        private static void DrawSkeleton(Graphics g, Rectangle r, int t)
        {
            int x = r.X + r.Width / 2; int y = r.Bottom - 42;
            using (Pen bone = new Pen(Color.FromArgb(220, 220, 210), 5f))
            {
                g.DrawLine(bone, x, y - 34, x, y + 10);
                g.DrawLine(bone, x - 22, y - 14, x + 22, y - 14);
                g.DrawLine(bone, x, y + 10, x - 18, y + 35);
                g.DrawLine(bone, x, y + 10, x + 18, y + 35);
            }
            using (SolidBrush skull = new SolidBrush(Color.FromArgb(230, 230, 220))) g.FillEllipse(skull, x - 17, y - 62, 34, 30);
            using (SolidBrush eye = new SolidBrush(Color.Black)) { g.FillEllipse(eye, x - 8, y - 51, 5, 5); g.FillEllipse(eye, x + 4, y - 51, 5, 5); }
        }

        private static void DrawSerpent(Graphics g, Rectangle r, Color c, int t)
        {
            int x = r.X + r.Width / 2; int y = r.Bottom - 36;
            using (Pen p = new Pen(c, 16f))
            {
                Point[] pts = new Point[] { new Point(x - 48, y), new Point(x - 20, y - 30), new Point(x + 18, y - 18), new Point(x + 48, y - 50) };
                g.DrawCurve(p, pts);
            }
            using (SolidBrush head = new SolidBrush(c)) g.FillEllipse(head, x + 36, y - 64, 38, 30);
            using (SolidBrush eye = new SolidBrush(Color.Yellow)) g.FillEllipse(eye, x + 55, y - 54, 5, 5);
        }

        private static void DrawDragon(Graphics g, Rectangle r, Color c, int t)
        {
            int x = r.X + r.Width / 2; int y = r.Bottom - 44;
            using (SolidBrush wing = new SolidBrush(Color.FromArgb(110, 60, 70, 180)))
            {
                g.FillPolygon(wing, new Point[] { new Point(x - 18, y - 82), new Point(x - 118, y - 144), new Point(x - 86, y - 38) });
                g.FillPolygon(wing, new Point[] { new Point(x + 18, y - 82), new Point(x + 118, y - 144), new Point(x + 86, y - 38) });
            }
            using (SolidBrush b = new SolidBrush(c))
            {
                g.FillEllipse(b, x - 62, y - 104, 124, 94);
                g.FillEllipse(b, x - 50, y - 142, 72, 52);
                g.FillEllipse(b, x + 48, y - 88, 46, 34);
                g.FillRectangle(b, x - 38, y - 62, 76, 48);
            }
            using (Pen p = new Pen(Color.FromArgb(120, 240, 255), 3f))
            {
                g.DrawLine(p, x - 35, y - 60, x + 35, y - 60);
                g.DrawLine(p, x - 30, y - 45, x + 30, y - 45);
                g.DrawString("BSOD", Font(9f, FontStyle.Bold), Brushes.White, new Rectangle(x - 32, y - 82, 64, 22), Center());
            }
            using (Pen p = new Pen(Color.White, 4f))
            {
                g.DrawLine(p, x - 24, y - 127, x - 12, y - 115);
                g.DrawLine(p, x - 12, y - 127, x - 24, y - 115);
                g.DrawLine(p, x + 5, y - 127, x + 17, y - 115);
                g.DrawLine(p, x + 17, y - 127, x + 5, y - 115);
            }
        }


        public static void DrawEffect(Graphics g, Effect e, float cameraX)
        {
            float life = e.Ticks / (float)Math.Max(1, e.MaxTicks);
            float progress = 1f - life;
            int alpha = ClampAlpha((int)(235 * life));
            float x1 = e.X - cameraX;
            float x2 = e.X2 - cameraX;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (e.Kind == EffectKind.Projectile)
            {
                float cx = x1 + (x2 - x1) * progress;
                float cy = e.Y + (e.Y2 - e.Y) * progress;
                int dir = e.Direction == 0 ? 1 : e.Direction;

                for (int i = 0; i < 5; i++)
                {
                    float tp = Math.Max(0f, progress - i * 0.08f);
                    float tx = x1 + (x2 - x1) * tp;
                    float ty = e.Y + (e.Y2 - e.Y) * tp;
                    int size = 14 - i * 2;
                    using (SolidBrush tb = new SolidBrush(Color.FromArgb(ClampAlpha(Math.Max(20, alpha - i * 35)), e.Color)))
                        g.FillEllipse(tb, tx - size, ty - size, size * 2, size * 2);
                }

                using (Pen glow = new Pen(Color.FromArgb(ClampAlpha(Math.Min(170, alpha)), e.Color), 18f))
                    g.DrawLine(glow, x1, e.Y, cx, cy);
                using (Pen mid = new Pen(Color.FromArgb(ClampAlpha(Math.Min(220, alpha)), Lighten(e.Color, 60)), 9f))
                    g.DrawLine(mid, x1, e.Y, cx, cy);
                using (Pen core = new Pen(Color.FromArgb(alpha, Color.White), 3.6f))
                    g.DrawLine(core, x1, e.Y, cx, cy);

                for (int i = 0; i < 6; i++)
                {
                    double a = progress * 10.0 + i * Math.PI / 3.0;
                    int px = (int)(cx + Math.Cos(a) * (18 + i % 2 * 5));
                    int py = (int)(cy + Math.Sin(a) * (18 + i % 2 * 5));
                    DrawGlowDot(g, px, py, 3, Lighten(e.Color, 40), ClampAlpha(alpha - 25));
                }

                using (GraphicsPath gp = new GraphicsPath())
                {
                    gp.AddEllipse(cx - 22, cy - 22, 44, 44);
                    using (PathGradientBrush pgb = new PathGradientBrush(gp))
                    {
                        pgb.CenterColor = Color.FromArgb(alpha, Color.White);
                        pgb.SurroundColors = new Color[] { Color.FromArgb(0, e.Color) };
                        g.FillPath(pgb, gp);
                    }
                }
                using (Pen ring = new Pen(Color.FromArgb(alpha, Color.White), 2.2f))
                    g.DrawEllipse(ring, cx - 18, cy - 18, 36, 36);
                using (Pen spark = new Pen(Color.FromArgb(alpha, Lighten(e.Color, 80)), 2f))
                {
                    g.DrawLine(spark, cx - dir * 18, cy, cx + dir * 18, cy);
                    g.DrawLine(spark, cx, cy - 14, cx, cy + 14);
                }
                if (!string.IsNullOrEmpty(e.Text))
                {
                    using (Font f = Font(9.5f, FontStyle.Bold))
                    using (SolidBrush shadow = new SolidBrush(Color.FromArgb(ClampAlpha(alpha - 50), 0, 0, 0)))
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(alpha, Color.White)))
                    {
                        g.DrawString(e.Text, f, shadow, new RectangleF(cx - 15, cy - 11, 34, 24), Center());
                        g.DrawString(e.Text, f, b, new RectangleF(cx - 16, cy - 12, 32, 24), Center());
                    }
                }
            }
            else if (e.Kind == EffectKind.Slash)
            {
                Rectangle rect = new Rectangle((int)(x1 - 74 - progress * 35), (int)(e.Y - 72 - progress * 24), (int)(148 + progress * 88), (int)(124 + progress * 62));
                int start = e.Direction > 0 ? 205 : -25;
                using (Pen glow = new Pen(Color.FromArgb(ClampAlpha(Math.Min(170, alpha)), e.Color), 16f))
                    g.DrawArc(glow, rect, start, 126);
                using (Pen mid = new Pen(Color.FromArgb(ClampAlpha(Math.Min(220, alpha)), Lighten(e.Color, 50)), 8f))
                    g.DrawArc(mid, rect, start + 6, 110);
                using (Pen core = new Pen(Color.FromArgb(alpha, Color.White), 3f))
                    g.DrawArc(core, rect, start + 10, 100);
                for (int i = 0; i < 5; i++)
                {
                    int sx = (int)x1 + e.Direction * (8 + i * 10);
                    int sy = (int)e.Y - 8 + i * 5;
                    using (Pen p = new Pen(Color.FromArgb(ClampAlpha(alpha - i * 30), Lighten(e.Color, 70)), 2f))
                        g.DrawLine(p, sx, sy, sx + e.Direction * (20 + i * 8), sy - 10 - i * 2);
                }
            }
            else if (e.Kind == EffectKind.SkillBurst)
            {
                Rectangle rect = new Rectangle((int)(x1 - 72 - progress * 68), (int)(e.Y - 72 - progress * 68), (int)(144 + progress * 136), (int)(144 + progress * 136));
                using (Pen outer = new Pen(Color.FromArgb(alpha, e.Color), 8f)) g.DrawEllipse(outer, rect);
                using (Pen mid = new Pen(Color.FromArgb(ClampAlpha(Math.Min(180, alpha)), Lighten(e.Color, 50)), 4f)) g.DrawEllipse(mid, rect.X + 18, rect.Y + 18, rect.Width - 36, rect.Height - 36);
                using (Pen inner = new Pen(Color.FromArgb(ClampAlpha(Math.Min(130, alpha)), Color.White), 2f)) g.DrawEllipse(inner, rect.X + 34, rect.Y + 34, rect.Width - 68, rect.Height - 68);
                int cx = (int)x1;
                int cy = (int)e.Y;
                using (Pen ray = new Pen(Color.FromArgb(ClampAlpha(Math.Min(165, alpha)), Lighten(e.Color, 60)), 2.6f))
                {
                    for (int i = 0; i < 12; i++)
                    {
                        double a = i * Math.PI / 6.0 + progress * 0.6;
                        int innerR = 28 + (int)(progress * 18);
                        int outerR = 58 + (int)(progress * 52);
                        g.DrawLine(ray,
                            cx + (int)(Math.Cos(a) * innerR), cy + (int)(Math.Sin(a) * innerR),
                            cx + (int)(Math.Cos(a) * outerR), cy + (int)(Math.Sin(a) * outerR));
                    }
                }
                DrawGlowDot(g, cx, cy, 10 + (int)(progress * 6), Lighten(e.Color, 70), ClampAlpha(Math.Min(220, alpha)));
                if (!string.IsNullOrEmpty(e.Text))
                {
                    using (Font f = Font(17f, FontStyle.Bold))
                    using (SolidBrush shadow = new SolidBrush(Color.FromArgb(ClampAlpha(alpha - 60), 0, 0, 0)))
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(alpha, Color.White)))
                    {
                        g.DrawString(e.Text, f, shadow, new RectangleF(x1 - 38, e.Y - 35, 84, 42), Center());
                        g.DrawString(e.Text, f, b, new RectangleF(x1 - 40, e.Y - 36, 80, 42), Center());
                    }
                }
            }
            else if (e.Kind == EffectKind.HitSpark)
            {
                int cx = (int)x1;
                int cy = (int)e.Y;
                using (Pen p = new Pen(Color.FromArgb(alpha, e.Color), 4f))
                {
                    for (int i = 0; i < 16; i++)
                    {
                        double a = i * Math.PI * 2 / 16 + progress * 2.4;
                        int inner = (int)(10 + progress * 16);
                        int outer = (int)(30 + progress * 52);
                        g.DrawLine(p,
                            cx + (int)(Math.Cos(a) * inner), cy + (int)(Math.Sin(a) * inner),
                            cx + (int)(Math.Cos(a) * outer), cy + (int)(Math.Sin(a) * outer));
                    }
                }
                using (Pen cross = new Pen(Color.FromArgb(alpha, Color.White), 3f))
                {
                    g.DrawLine(cross, cx - 16, cy, cx + 16, cy);
                    g.DrawLine(cross, cx, cy - 16, cx, cy + 16);
                }
                DrawGlowDot(g, cx, cy, 8, e.Color, alpha);
            }
            else if (e.Kind == EffectKind.Text)
            {
                using (Font f = Font(13.5f, FontStyle.Bold))
                using (SolidBrush shadow = new SolidBrush(Color.FromArgb(ClampAlpha(alpha - 70), 0, 0, 0)))
                using (SolidBrush b = new SolidBrush(Color.FromArgb(alpha, e.Color)))
                {
                    g.DrawString(e.Text, f, shadow, new RectangleF(x1 - 59, e.Y - progress * 32 + 2, 120, 26), Center());
                    g.DrawString(e.Text, f, b, new RectangleF(x1 - 60, e.Y - progress * 32, 120, 26), Center());
                }
            }
            else if (e.Kind == EffectKind.Heal || e.Kind == EffectKind.Guard || e.Kind == EffectKind.ScanLine)
            {
                Rectangle rect = new Rectangle((int)(x1 - 50 - progress * 28), (int)(e.Y - 67 - progress * 28), (int)(100 + progress * 56), (int)(100 + progress * 56));
                using (Pen p = new Pen(Color.FromArgb(alpha, e.Color), e.Kind == EffectKind.ScanLine ? 4f : 7f))
                    g.DrawEllipse(p, rect);
                using (Pen p2 = new Pen(Color.FromArgb(ClampAlpha(Math.Min(150, alpha)), Color.White), 2f))
                    g.DrawEllipse(p2, rect.X + 14, rect.Y + 14, rect.Width - 28, rect.Height - 28);
                if (e.Kind == EffectKind.ScanLine)
                {
                    using (Pen p = new Pen(Color.FromArgb(alpha, e.Color), 2f))
                    {
                        g.DrawLine(p, rect.Left, rect.Top + rect.Height / 2, rect.Right, rect.Top + rect.Height / 2);
                        g.DrawLine(p, rect.Left + rect.Width / 2, rect.Top, rect.Left + rect.Width / 2, rect.Bottom);
                        g.DrawArc(p, rect.X + 10, rect.Y + 10, rect.Width - 20, rect.Height - 20, 180, 120);
                    }
                }
                else if (e.Kind == EffectKind.Guard)
                {
                    using (Pen p = new Pen(Color.FromArgb(alpha, Lighten(e.Color, 60)), 2f))
                    {
                        g.DrawLine(p, x1, e.Y - 34, x1, e.Y + 14);
                        g.DrawLine(p, x1 - 20, e.Y - 8, x1 + 20, e.Y - 8);
                    }
                }
                else
                {
                    for (int i = 0; i < 6; i++)
                    {
                        double a = progress * 1.1 + i * Math.PI / 3.0;
                        DrawGlowDot(g, (int)(x1 + Math.Cos(a) * 26), (int)(e.Y + Math.Sin(a) * 26), 4, e.Color, ClampAlpha(alpha - 25));
                    }
                }
                if (!string.IsNullOrEmpty(e.Text))
                {
                    using (Font f = Font(12.5f, FontStyle.Bold))
                    using (SolidBrush shadow = new SolidBrush(Color.FromArgb(ClampAlpha(alpha - 60), 0, 0, 0)))
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(alpha, Color.White)))
                    {
                        g.DrawString(e.Text, f, shadow, new RectangleF(x1 - 23, e.Y - 29, 48, 28), Center());
                        g.DrawString(e.Text, f, b, new RectangleF(x1 - 24, e.Y - 30, 48, 28), Center());
                    }
                }
            }
        }
    }
}
