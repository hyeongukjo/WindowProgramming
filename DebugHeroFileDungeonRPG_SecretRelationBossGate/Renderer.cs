using System;
using System.Collections.Generic;
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

        public static StringFormat LeftMiddle()
        {
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Near;
            sf.LineAlignment = StringAlignment.Center;
            sf.Trimming = StringTrimming.EllipsisCharacter;
            return sf;
        }

        public static StringFormat LeftTop()
        {
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Near;
            sf.LineAlignment = StringAlignment.Near;
            sf.Trimming = StringTrimming.EllipsisCharacter;
            return sf;
        }

        public static Color Lighten(Color c, int n)
        {
            return Color.FromArgb(c.A, Math.Min(255, c.R + n), Math.Min(255, c.G + n), Math.Min(255, c.B + n));
        }

        public static Color Darken(Color c, int n)
        {
            return Color.FromArgb(c.A, Math.Max(0, c.R - n), Math.Max(0, c.G - n), Math.Max(0, c.B - n));
        }

        public static int Alpha(int value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return value;
        }

        public static void Panel(Graphics g, Rectangle r, Color fill)
        {
            if (r.Width <= 0 || r.Height <= 0) return;
            using (LinearGradientBrush b = new LinearGradientBrush(r, Lighten(fill, 22), Darken(fill, 14), 90f))
                g.FillRectangle(b, r);
            using (Pen p1 = new Pen(Color.FromArgb(245, 255, 255, 255)))
            using (Pen p2 = new Pen(Color.FromArgb(125, 70, 80, 100)))
            {
                g.DrawLine(p1, r.Left, r.Top, r.Right - 1, r.Top);
                g.DrawLine(p1, r.Left, r.Top, r.Left, r.Bottom - 1);
                g.DrawLine(p2, r.Right - 1, r.Top, r.Right - 1, r.Bottom - 1);
                g.DrawLine(p2, r.Left, r.Bottom - 1, r.Right - 1, r.Bottom - 1);
            }
        }

        public static void Inset(Graphics g, Rectangle r, Color fill)
        {
            using (LinearGradientBrush b = new LinearGradientBrush(r, Darken(fill, 8), Lighten(fill, 8), 90f))
                g.FillRectangle(b, r);
            using (Pen p1 = new Pen(Color.FromArgb(125, 70, 80, 100)))
            using (Pen p2 = new Pen(Color.White))
            {
                g.DrawLine(p1, r.Left, r.Top, r.Right - 1, r.Top);
                g.DrawLine(p1, r.Left, r.Top, r.Left, r.Bottom - 1);
                g.DrawLine(p2, r.Right - 1, r.Top, r.Right - 1, r.Bottom - 1);
                g.DrawLine(p2, r.Left, r.Bottom - 1, r.Right - 1, r.Bottom - 1);
            }
        }

        public static void Header(Graphics g, Rectangle r, string text)
        {
            using (LinearGradientBrush b = new LinearGradientBrush(r, Color.FromArgb(18, 50, 132), Color.FromArgb(62, 145, 235), 0f))
                g.FillRectangle(b, r);
            using (Pen p = new Pen(Color.FromArgb(165, 220, 255))) g.DrawRectangle(p, r.X, r.Y, r.Width - 1, r.Height - 1);
            using (Font f = Font(10.5f, FontStyle.Bold))
            using (SolidBrush br = new SolidBrush(Color.White))
                g.DrawString(text, f, br, r, Center());
        }

        public static void Button(Graphics g, Rectangle r, string text, bool selected)
        {
            Panel(g, r, selected ? Color.FromArgb(220, 238, 255) : Color.FromArgb(214, 222, 232));
            using (Pen p = new Pen(selected ? Color.FromArgb(35, 105, 220) : Color.FromArgb(145, 155, 170), selected ? 3f : 1.3f))
                g.DrawRectangle(p, r.X + 2, r.Y + 2, r.Width - 5, r.Height - 5);
            using (Font f = Font(10.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(24, 34, 52)))
                g.DrawString(text, f, b, r, Center());
        }

        public static void Bar(Graphics g, Rectangle r, int value, int max, Color color)
        {
            Inset(g, r, Color.FromArgb(230, 236, 245));
            int w = 0;
            if (max > 0) w = (int)((r.Width - 4) * Math.Max(0, Math.Min(1f, value / (float)max)));
            if (w > 0)
            {
                Rectangle rr = new Rectangle(r.X + 2, r.Y + 2, w, r.Height - 4);
                using (LinearGradientBrush b = new LinearGradientBrush(rr, Lighten(color, 35), Darken(color, 15), 90f)) g.FillRectangle(b, rr);
                using (SolidBrush s = new SolidBrush(Color.FromArgb(50, 255, 255, 255))) g.FillRectangle(s, rr.X, rr.Y, rr.Width, Math.Max(1, rr.Height / 2));
            }
        }

        public static void DrawDesktop(Graphics g, Rectangle client, List<DungeonInfo> dungeons, int selectedFile, Player p, List<UiButton> buttons)
        {
            using (LinearGradientBrush sky = new LinearGradientBrush(client, Color.FromArgb(30, 95, 190), Color.FromArgb(150, 220, 255), 90f))
                g.FillRectangle(sky, client);
            DrawCloud(g, 100, 80, 1.1f);
            DrawCloud(g, 640, 130, .9f);
            DrawCloud(g, 1090, 68, 1.0f);
            using (SolidBrush hill = new SolidBrush(Color.FromArgb(78, 170, 80)))
                g.FillPolygon(hill, new Point[] { new Point(0, client.Bottom), new Point(260, client.Bottom - 130), new Point(720, client.Bottom - 75), new Point(1120, client.Bottom - 150), new Point(client.Right, client.Bottom - 88), new Point(client.Right, client.Bottom) });

            Rectangle title = new Rectangle(130, 24, Math.Min(780, client.Width - 520), 46);
            Panel(g, title, Color.FromArgb(238, 244, 252));
            using (Font f = Font(11.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(24, 45, 82)))
                g.DrawString("C:\\WindowsKingdom\\DungeonDesktop  -  파일을 실행해서 던전에 진입", f, b, new Rectangle(title.X + 14, title.Y, title.Width - 28, title.Height), LeftMiddle());

            int startX = 140;
            int startY = 98;
            int cellW = 128;
            int cellH = 116;
            int cols = Math.Max(4, Math.Min(6, (client.Width - 520) / cellW));
            for (int i = 0; i < dungeons.Count; i++)
            {
                int col = i % cols;
                int row = i / cols;
                Rectangle icon = new Rectangle(startX + col * cellW, startY + row * cellH, 105, 102);
                DrawSmallFileIcon(g, icon, dungeons[i], i == selectedFile, p.PatchShards < dungeons[i].RequiredPatch);
                buttons.Add(new UiButton(icon, "file" + i));
            }

            DrawShortcut(g, new Rectangle(26, 26, 74, 82), "내 PC", Color.FromArgb(220, 235, 255));
            DrawShortcut(g, new Rectangle(26, 122, 74, 82), "휴지통", Color.FromArgb(225, 225, 225));
            DrawShortcut(g, new Rectangle(26, 218, 74, 82), "설정", Color.FromArgb(255, 220, 90));
            DrawShortcut(g, new Rectangle(26, 314, 74, 82), "로그", Color.FromArgb(240, 240, 210));

            DungeonInfo d = dungeons[selectedFile];
            Rectangle prop = new Rectangle(client.Right - 390, 22, 360, client.Height - 72);
            Panel(g, prop, Color.FromArgb(232, 239, 248));
            Header(g, new Rectangle(prop.X + 6, prop.Y + 6, prop.Width - 12, 32), "파일 속성 / 던전 정보");
            DrawLargeFileSymbol(g, new Rectangle(prop.X + 24, prop.Y + 58, 108, 116), d.Accent, p.PatchShards < d.RequiredPatch);
            using (Font f = Font(10.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(25, 42, 70)))
                g.DrawString(d.FileName, f, b, new Rectangle(prop.X + 146, prop.Y + 58, prop.Width - 166, 46), LeftTop());
            using (Font f = Font(8.8f, FontStyle.Regular))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(54, 68, 92)))
            {
                string meta = "종류: 실행 가능한 던전 파일\n권장 Lv. " + d.RecommendedLevel + "\n필요 패치: " + d.RequiredPatch + "\n시야: 단일 라인 아레나";
                g.DrawString(meta, f, b, new Rectangle(prop.X + 146, prop.Y + 110, prop.Width - 166, 76), LeftTop());
            }
            Rectangle desc = new Rectangle(prop.X + 22, prop.Y + 202, prop.Width - 44, 122);
            Inset(g, desc, Color.White);
            using (Font f = Font(9f, FontStyle.Regular))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(42, 54, 76)))
                g.DrawString(d.Description, f, b, new Rectangle(desc.X + 12, desc.Y + 12, desc.Width - 24, desc.Height - 24), LeftTop());
            Rectangle hero = new Rectangle(prop.X + 22, desc.Bottom + 16, prop.Width - 44, 176);
            Panel(g, hero, Color.FromArgb(242, 247, 252));
            Header(g, new Rectangle(hero.X + 5, hero.Y + 5, hero.Width - 10, 28), "용사 / 커스텀 장비");
            Player preview = ClonePreview(p, hero.X + 76, hero.Y + 130);
            DrawHero(g, preview, 0, 0, 0, false, true);
            using (Font f = Font(8.7f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(32, 46, 70)))
            {
                string s = p.Name + "  Lv." + p.Level + "\n" + p.OutfitName + "\n" + p.WeaponName + "\n" + p.ArmorName + "\nHP " + p.Hp + "/" + p.MaxHp + "  MP " + p.Mp + "/" + p.MaxMp;
                g.DrawString(s, f, b, new Rectangle(hero.X + 140, hero.Y + 44, hero.Width - 154, 110), LeftTop());
            }
            Rectangle open = new Rectangle(prop.X + 24, prop.Bottom - 54, prop.Width - 48, 36);
            Button(g, open, p.PatchShards < d.RequiredPatch ? "잠김" : "파일 실행", p.PatchShards >= d.RequiredPatch);
            if (p.PatchShards >= d.RequiredPatch) buttons.Add(new UiButton(open, "file" + selectedFile));
        }

        private static Player ClonePreview(Player p, int x, int y)
        {
            Player copy = new Player();
            copy.Name = p.Name;
            copy.Level = p.Level;
            copy.Outfit = p.Outfit;
            copy.Weapon = p.Weapon;
            copy.Armor = p.Armor;
            copy.Cape = p.Cape;
            copy.WeaponLevel = p.WeaponLevel;
            copy.X = x;
            copy.Y = y;
            return copy;
        }

        public static void DrawCustomization(Graphics g, Rectangle client, Player p, int selectedPart, List<UiButton> buttons)
        {
            using (LinearGradientBrush bg = new LinearGradientBrush(client, Color.FromArgb(12, 23, 56), Color.FromArgb(38, 105, 180), 90f))
                g.FillRectangle(bg, client);
            using (Pen grid = new Pen(Color.FromArgb(34, 150, 220, 255)))
            {
                for (int x = 0; x < client.Width; x += 44) g.DrawLine(grid, x, 0, x, client.Height);
                for (int y = 0; y < client.Height; y += 34) g.DrawLine(grid, 0, y, client.Width, y);
            }
            Rectangle frame = new Rectangle(54, 34, client.Width - 108, client.Height - 68);
            Panel(g, frame, Color.FromArgb(232, 239, 248));
            Header(g, new Rectangle(frame.X + 6, frame.Y + 6, frame.Width - 12, 38), "용사 커스터마이징 - 고급 의상/장비 선택");

            Rectangle preview = new Rectangle(frame.X + 30, frame.Y + 66, 345, frame.Height - 112);
            Inset(g, preview, Color.FromArgb(10, 22, 46));
            using (SolidBrush glow = new SolidBrush(Color.FromArgb(52, p.OutfitColor))) g.FillEllipse(glow, preview.X + 45, preview.Y + 28, 245, 285);
            Player dummy = ClonePreview(p, preview.X + preview.Width / 2, preview.Y + preview.Height - 82);
            DrawHero(g, dummy, 0, 0, 0, false, true);
            using (Font f = Font(12f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.White))
                g.DrawString(p.OutfitName + "\n" + p.WeaponName + " / " + p.ArmorName + "\n" + p.CapeName, f, b, new Rectangle(preview.X + 10, preview.Bottom - 84, preview.Width - 20, 74), Center());

            int x0 = preview.Right + 30;
            int y0 = frame.Y + 66;
            int rowW = frame.Right - x0 - 30;
            DrawCustomizerRow(g, "의상", GameData.OutfitNames, p.Outfit, selectedPart == 0, new Rectangle(x0, y0, rowW, 94), buttons, "outfit");
            DrawCustomizerRow(g, "무기", GameData.WeaponNames, p.Weapon, selectedPart == 1, new Rectangle(x0, y0 + 106, rowW, 94), buttons, "weapon");
            DrawCustomizerRow(g, "갑옷", GameData.ArmorNames, p.Armor, selectedPart == 2, new Rectangle(x0, y0 + 212, rowW, 94), buttons, "armor");
            DrawCustomizerRow(g, "망토", GameData.CapeNames, p.Cape, selectedPart == 3, new Rectangle(x0, y0 + 318, rowW, 94), buttons, "cape");

            Rectangle stat = new Rectangle(x0, frame.Bottom - 92, rowW - 260, 58);
            Inset(g, stat, Color.FromArgb(248, 252, 255));
            using (Font f = Font(9f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(38, 54, 80)))
                g.DrawString("HP " + p.MaxHp + "   MP " + p.MaxMp + "   공격 " + p.Attack + "   방어 " + p.Defense + "   속도 " + p.Speed, f, b, new Rectangle(stat.X + 14, stat.Y, stat.Width - 28, stat.Height), LeftMiddle());
            Rectangle start = new Rectangle(frame.Right - 250, frame.Bottom - 92, 210, 54);
            Button(g, start, "10단계 시작", true);
            buttons.Add(new UiButton(start, "startGame"));
            using (Font f = Font(8.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(40, 55, 80)))
                g.DrawString("상하: 파트 선택 · 좌우: 장비 변경 · Enter 시작", f, b, new Rectangle(x0, frame.Bottom - 34, 560, 24), LeftMiddle());
        }

        private static void DrawCustomizerRow(Graphics g, string title, string[] names, int selected, bool active, Rectangle r, List<UiButton> buttons, string prefix)
        {
            Panel(g, r, active ? Color.FromArgb(240, 247, 255) : Color.FromArgb(224, 231, 240));
            using (Font f = Font(10f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(24, 38, 64)))
                g.DrawString(title, f, b, new Rectangle(r.X + 12, r.Y + 8, 74, r.Height - 16), LeftMiddle());
            int cols = Math.Min(4, Math.Max(1, names.Length));
            int rows = (int)Math.Ceiling(names.Length / (float)cols);
            int startX = r.X + 86;
            int startY = r.Y + 9;
            int btnW = Math.Max(118, (r.Width - 100) / cols - 7);
            int btnH = Math.Max(30, (r.Height - 18) / rows - 5);
            for (int i = 0; i < names.Length; i++)
            {
                int col = i % cols;
                int row = i / cols;
                Rectangle b = new Rectangle(startX + col * (btnW + 7), startY + row * (btnH + 5), btnW, btnH);
                Button(g, b, names[i], i == selected);
                buttons.Add(new UiButton(b, prefix + i));
            }
        }

        private static void DrawCloud(Graphics g, float x, float y, float s)
        {
            using (SolidBrush b = new SolidBrush(Color.FromArgb(230, 255, 255, 255)))
            {
                g.FillEllipse(b, x, y + 20 * s, 72 * s, 32 * s);
                g.FillEllipse(b, x + 26 * s, y, 88 * s, 58 * s);
                g.FillEllipse(b, x + 90 * s, y + 20 * s, 72 * s, 34 * s);
                g.FillRectangle(b, x + 22 * s, y + 28 * s, 126 * s, 26 * s);
            }
        }

        private static void DrawShortcut(Graphics g, Rectangle r, string text, Color c)
        {
            DrawLargeFileSymbol(g, new Rectangle(r.X + 12, r.Y + 3, 48, 50), c, false);
            using (Font f = Font(8f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.White))
            using (SolidBrush s = new SolidBrush(Color.FromArgb(120, 0, 0, 0)))
            {
                g.DrawString(text, f, s, new Rectangle(r.X + 1, r.Y + 56, r.Width, 24), Center());
                g.DrawString(text, f, b, new Rectangle(r.X, r.Y + 55, r.Width, 24), Center());
            }
        }

        public static void DrawSmallFileIcon(Graphics g, Rectangle r, DungeonInfo d, bool selected, bool locked)
        {
            if (selected) using (SolidBrush s = new SolidBrush(Color.FromArgb(80, 35, 110, 220))) g.FillRectangle(s, r);
            DrawLargeFileSymbol(g, new Rectangle(r.X + 24, r.Y + 4, 56, 60), d.Accent, locked);
            using (Font f = Font(7.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(locked ? Color.LightGray : Color.White))
            using (SolidBrush sh = new SolidBrush(Color.FromArgb(120, 0, 0, 0)))
            {
                Rectangle text = new Rectangle(r.X - 6, r.Y + 66, r.Width + 12, 34);
                g.DrawString(d.FileName, f, sh, new Rectangle(text.X + 1, text.Y + 1, text.Width, text.Height), Center());
                g.DrawString(d.FileName, f, b, text, Center());
            }
        }

        public static void DrawLargeFileSymbol(Graphics g, Rectangle r, Color color, bool locked)
        {
            Color c = locked ? Color.Gray : color;
            Point[] page = new Point[] { new Point(r.X + 8, r.Y + 4), new Point(r.Right - 16, r.Y + 4), new Point(r.Right - 4, r.Y + 17), new Point(r.Right - 4, r.Bottom - 6), new Point(r.X + 8, r.Bottom - 6) };
            using (LinearGradientBrush b = new LinearGradientBrush(r, Color.White, Color.FromArgb(214, 226, 242), 90f)) g.FillPolygon(b, page);
            using (Pen p = new Pen(Darken(c, 40), 2f)) g.DrawPolygon(p, page);
            using (SolidBrush b = new SolidBrush(Color.FromArgb(45, c))) g.FillRectangle(b, r.X + 15, r.Y + 24, r.Width - 26, 18);
            using (Pen p = new Pen(c, 2.5f))
            {
                g.DrawLine(p, r.X + 17, r.Y + 31, r.Right - 14, r.Y + 31);
                g.DrawLine(p, r.X + 17, r.Y + 41, r.Right - 20, r.Y + 41);
            }
            using (SolidBrush fold = new SolidBrush(Color.FromArgb(210, 232, 245))) g.FillPolygon(fold, new Point[] { new Point(r.Right - 16, r.Y + 4), new Point(r.Right - 4, r.Y + 17), new Point(r.Right - 16, r.Y + 17) });
            if (locked)
            {
                using (Font f = Font(9f, FontStyle.Bold))
                using (SolidBrush b = new SolidBrush(Color.Firebrick)) g.DrawString("LOCK", f, b, r, Center());
            }
        }

        public static void DrawArena(Graphics g, Rectangle client, DungeonInfo dungeon, float cameraX, float cameraY, int tick)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (LinearGradientBrush bg = new LinearGradientBrush(client, Darken(dungeon.BackColor, 20), Lighten(dungeon.BackColor, 20), 90f))
                g.FillRectangle(bg, client);
            using (SolidBrush glow = new SolidBrush(Color.FromArgb(48, dungeon.Accent)))
            {
                g.FillEllipse(glow, client.Width / 2 - 520, 60, 1040, 320);
                g.FillEllipse(glow, -180, client.Height - 330, 520, 260);
                g.FillEllipse(glow, client.Right - 340, client.Height - 360, 520, 280);
            }

            // pseudo-isometric single lane: not based on any copyrighted map, but it gives a premium MOBA-style arena view.
            Point[] lane = new Point[]
            {
                new Point(-240, client.Height - 160),
                new Point(client.Width / 2 - 170, client.Height / 2 + 30),
                new Point(client.Width + 240, 145),
                new Point(client.Width + 260, 285),
                new Point(client.Width / 2 + 120, client.Height / 2 + 178),
                new Point(-240, client.Height - 18)
            };
            using (LinearGradientBrush path = new LinearGradientBrush(client, Color.FromArgb(68, 82, 106), Color.FromArgb(36, 45, 70), 0f)) g.FillPolygon(path, lane);
            using (Pen edge = new Pen(Color.FromArgb(170, Lighten(dungeon.Accent, 50)), 3f)) g.DrawPolygon(edge, lane);
            using (Pen inner = new Pen(Color.FromArgb(70, 255, 255, 255), 1.6f))
            {
                for (int i = 0; i < 8; i++)
                {
                    int offset = i * 90 - (int)(cameraX * .08f) % 90;
                    g.DrawLine(inner, -120 + offset, client.Height - 55, client.Width + offset, 170);
                }
            }

            using (Font f = Font(8.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(55, 255, 255, 255)))
            {
                string[] tokens = new string[] { "EAX", "STACK", "CACHE", "0xFF", "RAM", "PATCH", "DLL", "SYS" };
                for (int i = 0; i < 32; i++)
                {
                    int x = (int)((i * 173 - cameraX * .26f + tick * .4f) % (client.Width + 240)) - 100;
                    int y = 80 + (i * 43) % Math.Max(1, client.Height - 180);
                    g.DrawString(tokens[i % tokens.Length], f, b, x, y);
                }
            }

            DrawArenaStructure(g, new Point(180 - (int)(cameraX * .18f), client.Height - 156), dungeon.Accent, "SPAWN");
            DrawArenaStructure(g, new Point(client.Width - 180 - (int)(cameraX * .05f), 150), Color.FromArgb(240, 80, 80), "CORE");
        }

        private static void DrawArenaStructure(Graphics g, Point p, Color c, string label)
        {
            using (SolidBrush baseB = new SolidBrush(Color.FromArgb(80, 0, 0, 0))) g.FillEllipse(baseB, p.X - 65, p.Y + 35, 130, 28);
            using (LinearGradientBrush b = new LinearGradientBrush(new Rectangle(p.X - 34, p.Y - 56, 68, 112), Lighten(c, 35), Darken(c, 35), 90f)) g.FillRectangle(b, p.X - 34, p.Y - 56, 68, 112);
            using (Pen p1 = new Pen(Color.FromArgb(200, 255, 255, 255), 2f)) g.DrawRectangle(p1, p.X - 34, p.Y - 56, 68, 112);
            DrawGlowDot(g, p.X, p.Y - 70, 16, c, 140);
            using (Font f = Font(8f, FontStyle.Bold))
            using (SolidBrush br = new SolidBrush(Color.White)) g.DrawString(label, f, br, new Rectangle(p.X - 44, p.Y + 20, 88, 20), Center());
        }

        public static PointF WorldToScreen(float x, float y, float cameraX, float cameraY)
        {
            // light isometric projection: X scrolls horizontally, Y gives diagonal depth.
            float sx = x - cameraX + (y - 330) * .42f;
            float sy = y - cameraY + (y - 330) * .18f;
            return new PointF(sx, sy);
        }

        public static PointF ScreenToWorld(Point p, float cameraX, float cameraY)
        {
            float y = p.Y + cameraY;
            float x = p.X + cameraX - (y - 330) * .42f;
            return new PointF(x, y);
        }

        public static void DrawHero(Graphics g, Player p, float cameraX, float cameraY, int tick, bool moving, bool preview)
        {
            PointF s = preview ? new PointF(p.X, p.Y) : WorldToScreen(p.X, p.Y, cameraX, cameraY);
            int ox = (int)s.X;
            int oy = (int)s.Y;
            int bob = moving ? (int)(Math.Sin(tick / 4.0) * 3.0) : 0;
            oy += bob;
            Color outfit = p.OutfitColor;
            Color weapon = p.WeaponColor;

            using (SolidBrush sh = new SolidBrush(Color.FromArgb(100, 0, 0, 0))) g.FillEllipse(sh, ox - 34, oy + 17, 68, 18);
            using (SolidBrush aura = new SolidBrush(Color.FromArgb(42, outfit))) g.FillEllipse(aura, ox - 58, oy - 92, 116, 132);
            using (Pen a = new Pen(Color.FromArgb(130, weapon), 3f))
            {
                g.DrawArc(a, ox - 58, oy - 94, 116, 126, (tick * 4) % 360, 220);
                g.DrawArc(a, ox - 44, oy - 79, 88, 100, 180 - (tick * 3) % 360, 160);
            }

            if (p.Cape > 0)
            {
                Color[] capeColors = new Color[] { Color.Transparent, Color.FromArgb(60, 120, 220), Color.FromArgb(120, 255, 190), Color.FromArgb(70, 210, 255), Color.FromArgb(220, 150, 60), Color.FromArgb(70, 70, 95), Color.FromArgb(95, 170, 255), Color.FromArgb(120, 255, 190) };
                Color cc = capeColors[Math.Max(0, Math.Min(p.Cape, capeColors.Length - 1))];
                using (SolidBrush cape = new SolidBrush(Color.FromArgb(195, cc)))
                    g.FillPolygon(cape, new Point[] { new Point(ox - 15, oy - 52), new Point(ox - 52, oy + 22), new Point(ox + 44, oy + 22), new Point(ox + 15, oy - 52) });
            }

            Rectangle body = new Rectangle(ox - 17, oy - 53, 34, 48);
            using (LinearGradientBrush suit = new LinearGradientBrush(body, Lighten(outfit, 35), Darken(outfit, 22), 90f)) g.FillRectangle(suit, body);
            using (Pen outline = new Pen(Color.FromArgb(22, 28, 42), 2f)) g.DrawRectangle(outline, body);
            using (SolidBrush shine = new SolidBrush(Color.FromArgb(55, 255, 255, 255))) g.FillRectangle(shine, body.X + 4, body.Y + 3, body.Width - 8, 15);

            using (SolidBrush skin = new SolidBrush(Color.FromArgb(238, 197, 154))) g.FillEllipse(skin, ox - 14, oy - 78, 28, 28);
            using (SolidBrush hair = new SolidBrush(Color.FromArgb(24, 28, 40)))
            {
                g.FillPie(hair, ox - 17, oy - 84, 34, 24, 180, 180);
                g.FillPolygon(hair, new Point[] { new Point(ox - 15, oy - 65), new Point(ox - 24, oy - 55), new Point(ox - 2, oy - 63), new Point(ox + 14, oy - 61) });
            }
            using (SolidBrush eye = new SolidBrush(Color.Black)) g.FillRectangle(eye, ox + (p.Facing >= 0 ? 4 : -8), oy - 65, 4, 3);

            using (Pen limb = new Pen(Color.FromArgb(28, 34, 48), 5f))
            {
                g.DrawLine(limb, ox - 10, oy - 8, ox - 21, oy + 16);
                g.DrawLine(limb, ox + 10, oy - 8, ox + 21, oy + 16);
            }
            using (SolidBrush boot = new SolidBrush(Color.FromArgb(24, 28, 38)))
            {
                g.FillRectangle(boot, ox - 25, oy + 13, 14, 6);
                g.FillRectangle(boot, ox + 11, oy + 13, 14, 6);
            }

            if (p.Armor > 0)
            {
                using (Pen armor = new Pen((new Color[] { Color.Gray, Color.FromArgb(110, 190, 255), Color.FromArgb(175, 195, 220), Color.FromArgb(120, 255, 160), Color.FromArgb(255, 140, 100), Color.FromArgb(235, 160, 70), Color.FromArgb(90, 150, 255), Color.FromArgb(255, 220, 110) })[Math.Max(0, Math.Min(p.Armor, 7))], 2.4f))
                {
                    g.DrawRectangle(armor, body.X + 5, body.Y + 6, body.Width - 10, body.Height - 12);
                    g.DrawLine(armor, ox, body.Y + 6, ox, body.Bottom - 6);
                }
            }

            int dir = p.Facing >= 0 ? 1 : -1;
            if (p.Weapon == 2 || p.Weapon == 6)
            {
                using (Pen lens = new Pen(weapon, 5f))
                {
                    g.DrawEllipse(lens, ox + dir * 23 - 11, oy - 46, 22, 22);
                    g.DrawLine(lens, ox + dir * 32, oy - 30, ox + dir * 56, oy - 12);
                    if (p.Weapon == 6) g.DrawRectangle(lens, ox + dir * 40 - 12, oy - 40, 24, 18);
                }
                using (Pen beam = new Pen(Color.FromArgb(120, Lighten(weapon, 60)), 2f))
                    g.DrawLine(beam, ox + dir * 46, oy - 34, ox + dir * 68, oy - 28);
            }
            else
            {
                int length = p.Weapon == 4 ? 72 : p.Weapon == 7 ? 80 : 58;
                using (Pen glow = new Pen(Color.FromArgb(190, weapon), p.Weapon >= 5 ? 9f : 7f)) g.DrawLine(glow, ox + dir * 14, oy - 39, ox + dir * length, oy - 80);
                using (Pen core = new Pen(Color.White, 2f)) g.DrawLine(core, ox + dir * 14, oy - 39, ox + dir * length, oy - 80);
                if (p.Weapon == 3 || p.Weapon == 7)
                {
                    using (Pen p2 = new Pen(Color.FromArgb(170, Lighten(weapon, 60)), 3f)) g.DrawEllipse(p2, ox + dir * length - 11, oy - 91, 22, 22);
                }
            }

            if (p.ShieldTicks > 0)
            {
                using (Pen shield = new Pen(Color.FromArgb(155, 90, 180, 255), 5f)) g.DrawEllipse(shield, ox - 52, oy - 92, 104, 122);
                using (Pen shield = new Pen(Color.FromArgb(105, Color.White), 1.7f)) g.DrawEllipse(shield, ox - 42, oy - 80, 84, 100);
            }
        }

        public static void DrawMonster(Graphics g, Monster m, float cameraX, float cameraY, int tick)
        {
            if (m.Hp <= 0) return;
            PointF s = WorldToScreen(m.X, m.Y, cameraX, cameraY);
            int x = (int)s.X;
            int y = (int)s.Y;
            using (SolidBrush sh = new SolidBrush(Color.FromArgb(95, 0, 0, 0))) g.FillEllipse(sh, x - (m.Boss ? 78 : 34), y + 16, m.Boss ? 156 : 68, m.Boss ? 26 : 16);
            using (SolidBrush aura = new SolidBrush(Color.FromArgb(m.HitFlash > 0 ? 85 : 32, m.Color))) g.FillEllipse(aura, x - (m.Boss ? 110 : 52), y - (m.Boss ? 120 : 70), m.Boss ? 220 : 104, m.Boss ? 190 : 118);
            if (m.Kind == MonsterKind.BSODDragon) DrawDragon(g, x, y, m.Color, tick);
            else if (m.Kind == MonsterKind.KernelGolem) DrawGolem(g, x, y, m.Color);
            else if (m.Kind == MonsterKind.ProcessWolf) DrawWolf(g, x, y, m.Color, tick);
            else if (m.Kind == MonsterKind.RegistryWraith) DrawWraith(g, x, y, m.Color, tick);
            else if (m.Kind == MonsterKind.DefenderBot) DrawBot(g, x, y, m.Color);
            else DrawSlime(g, x, y, m.Color, tick);
            Rectangle hp = new Rectangle(x - (m.Boss ? 90 : 38), y - (m.Boss ? 125 : 76), m.Boss ? 180 : 76, 8);
            Bar(g, hp, m.Hp, m.MaxHp, m.Boss ? Color.Red : Color.OrangeRed);
            using (Font f = Font(7f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.White))
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(130, 0, 0, 0)))
            {
                Rectangle name = new Rectangle(hp.X - 18, hp.Y - 18, hp.Width + 36, 16);
                g.FillRectangle(bg, name);
                g.DrawString(m.Name, f, b, name, Center());
            }
        }

        private static void DrawSlime(Graphics g, int x, int y, Color c, int tick)
        {
            int bob = (int)(Math.Sin(tick / 7.0) * 3);
            using (SolidBrush b = new SolidBrush(c)) g.FillEllipse(b, x - 27, y - 44 + bob, 54, 43);
            using (Pen p = new Pen(Lighten(c, 70), 3f)) g.DrawEllipse(p, x - 27, y - 44 + bob, 54, 43);
            using (SolidBrush e = new SolidBrush(Color.Black)) { g.FillEllipse(e, x - 12, y - 29 + bob, 5, 5); g.FillEllipse(e, x + 7, y - 29 + bob, 5, 5); }
        }

        private static void DrawBot(Graphics g, int x, int y, Color c)
        {
            Rectangle body = new Rectangle(x - 28, y - 56, 56, 48);
            using (LinearGradientBrush b = new LinearGradientBrush(body, Color.White, c, 90f)) g.FillRectangle(b, body);
            using (Pen p = new Pen(Color.FromArgb(32, 42, 60), 2f)) g.DrawRectangle(p, body);
            using (SolidBrush eye = new SolidBrush(Color.Cyan)) { g.FillRectangle(eye, x - 15, y - 39, 9, 7); g.FillRectangle(eye, x + 6, y - 39, 9, 7); }
            using (Pen arm = new Pen(Color.FromArgb(210, 230, 255), 5f)) { g.DrawLine(arm, x - 28, y - 32, x - 48, y - 18); g.DrawLine(arm, x + 28, y - 32, x + 48, y - 18); }
        }

        private static void DrawWolf(Graphics g, int x, int y, Color c, int tick)
        {
            using (SolidBrush b = new SolidBrush(c))
            {
                g.FillEllipse(b, x - 38, y - 46, 68, 36);
                g.FillEllipse(b, x + 12, y - 56, 34, 28);
                g.FillPolygon(b, new Point[] { new Point(x + 17, y - 55), new Point(x + 25, y - 72), new Point(x + 31, y - 51) });
                g.FillPolygon(b, new Point[] { new Point(x - 38, y - 38), new Point(x - 58, y - 50), new Point(x - 49, y - 26) });
            }
            using (Pen virus = new Pen(Color.FromArgb(160, 120, 90, 240), 3f))
            {
                g.DrawLine(virus, x - 14, y - 48, x - 4, y - 67);
                g.DrawLine(virus, x + 3, y - 48, x + 12, y - 66);
                g.DrawLine(virus, x + 18, y - 44, x + 30, y - 61);
            }
            using (SolidBrush e = new SolidBrush(Color.Red)) g.FillEllipse(e, x + 30, y - 48, 6, 6);
        }

        private static void DrawWraith(Graphics g, int x, int y, Color c, int tick)
        {
            int bob = (int)(Math.Sin(tick / 8.0) * 5);
            using (SolidBrush b = new SolidBrush(Color.FromArgb(185, c)))
            {
                g.FillEllipse(b, x - 30, y - 72 + bob, 60, 58);
                g.FillPolygon(b, new Point[] { new Point(x - 30, y - 38 + bob), new Point(x - 18, y + 10 + bob), new Point(x, y - 10 + bob), new Point(x + 18, y + 10 + bob), new Point(x + 30, y - 38 + bob) });
            }
            using (SolidBrush e = new SolidBrush(Color.FromArgb(8, 20, 40))) { g.FillEllipse(e, x - 12, y - 50 + bob, 8, 12); g.FillEllipse(e, x + 5, y - 50 + bob, 8, 12); }
        }

        private static void DrawGolem(Graphics g, int x, int y, Color c)
        {
            using (SolidBrush b = new SolidBrush(c))
            {
                g.FillRectangle(b, x - 34, y - 76, 68, 58);
                g.FillRectangle(b, x - 22, y - 102, 44, 30);
                g.FillRectangle(b, x - 54, y - 62, 22, 40);
                g.FillRectangle(b, x + 32, y - 62, 22, 40);
            }
            using (SolidBrush core = new SolidBrush(Color.FromArgb(120, 230, 255))) g.FillRectangle(core, x - 12, y - 55, 24, 22);
            using (Pen p = new Pen(Color.FromArgb(30, 40, 60), 3f)) g.DrawRectangle(p, x - 34, y - 76, 68, 58);
        }

        private static void DrawDragon(Graphics g, int x, int y, Color c, int tick)
        {
            using (SolidBrush wing = new SolidBrush(Color.FromArgb(120, 70, 70, 200)))
            {
                g.FillPolygon(wing, new Point[] { new Point(x - 18, y - 96), new Point(x - 132, y - 160), new Point(x - 100, y - 42) });
                g.FillPolygon(wing, new Point[] { new Point(x + 18, y - 96), new Point(x + 132, y - 160), new Point(x + 100, y - 42) });
            }
            using (SolidBrush b = new SolidBrush(c))
            {
                g.FillEllipse(b, x - 72, y - 124, 144, 110);
                g.FillEllipse(b, x - 54, y - 172, 82, 58);
                g.FillEllipse(b, x + 60, y - 100, 54, 40);
                g.FillRectangle(b, x - 44, y - 74, 88, 62);
            }
            using (Pen p = new Pen(Color.FromArgb(130, 230, 255), 3f))
            {
                g.DrawLine(p, x - 40, y - 76, x + 40, y - 76);
                g.DrawLine(p, x - 34, y - 56, x + 34, y - 56);
            }
            using (Font f = Font(10f, FontStyle.Bold)) g.DrawString("BSOD", f, Brushes.White, new Rectangle(x - 42, y - 100, 84, 24), Center());
            using (Pen p = new Pen(Color.White, 4f))
            {
                g.DrawLine(p, x - 27, y - 154, x - 13, y - 140);
                g.DrawLine(p, x - 13, y - 154, x - 27, y - 140);
                g.DrawLine(p, x + 7, y - 154, x + 21, y - 140);
                g.DrawLine(p, x + 21, y - 154, x + 7, y - 140);
            }
        }

        public static void DrawEffect(Graphics g, Effect e, float cameraX, float cameraY)
        {
            float life = e.Ticks / (float)Math.Max(1, e.MaxTicks);
            float progress = 1f - life;
            int alpha = Alpha((int)(235 * life));
            PointF s1 = WorldToScreen(e.X, e.Y, cameraX, cameraY);
            PointF s2 = WorldToScreen(e.X2, e.Y2, cameraX, cameraY);
            float cx = s1.X + (s2.X - s1.X) * progress;
            float cy = s1.Y + (s2.Y - s1.Y) * progress;

            if (e.Kind == "projectile")
            {
                using (Pen glow = new Pen(Color.FromArgb(Math.Min(170, alpha), e.Color), 20f)) g.DrawLine(glow, s1, new PointF(cx, cy));
                using (Pen mid = new Pen(Color.FromArgb(Math.Min(220, alpha), Lighten(e.Color, 60)), 9f)) g.DrawLine(mid, s1, new PointF(cx, cy));
                using (Pen core = new Pen(Color.FromArgb(alpha, Color.White), 3.5f)) g.DrawLine(core, s1, new PointF(cx, cy));
                DrawGlowDot(g, (int)cx, (int)cy, 11, e.Color, alpha);
                if (!string.IsNullOrEmpty(e.Text))
                {
                    using (Font f = Font(9.5f, FontStyle.Bold))
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(alpha, Color.White))) g.DrawString(e.Text, f, b, new RectangleF(cx - 16, cy - 12, 32, 24), Center());
                }
            }
            else if (e.Kind == "slash")
            {
                Rectangle rect = new Rectangle((int)(cx - 84 - progress * 36), (int)(cy - 64 - progress * 20), (int)(168 + progress * 90), (int)(128 + progress * 60));
                using (Pen glow = new Pen(Color.FromArgb(Math.Min(180, alpha), e.Color), 17f)) g.DrawArc(glow, rect, 205, 130);
                using (Pen core = new Pen(Color.FromArgb(alpha, Color.White), 4f)) g.DrawArc(core, rect, 212, 112);
            }
            else if (e.Kind == "burst")
            {
                Rectangle rect = new Rectangle((int)(s1.X - 72 - progress * 70), (int)(s1.Y - 72 - progress * 70), (int)(144 + progress * 140), (int)(144 + progress * 140));
                using (Pen outer = new Pen(Color.FromArgb(alpha, e.Color), 8f)) g.DrawEllipse(outer, rect);
                using (Pen inner = new Pen(Color.FromArgb(Math.Min(160, alpha), Color.White), 2f)) g.DrawEllipse(inner, rect.X + 28, rect.Y + 28, rect.Width - 56, rect.Height - 56);
                for (int i = 0; i < 14; i++)
                {
                    double a = i * Math.PI * 2 / 14 + progress;
                    using (Pen p = new Pen(Color.FromArgb(Math.Min(160, alpha), Lighten(e.Color, 60)), 2.3f))
                        g.DrawLine(p, (int)(s1.X + Math.Cos(a) * 35), (int)(s1.Y + Math.Sin(a) * 35), (int)(s1.X + Math.Cos(a) * (70 + progress * 60)), (int)(s1.Y + Math.Sin(a) * (70 + progress * 60)));
                }
            }
            else if (e.Kind == "text")
            {
                using (Font f = Font(13.5f, FontStyle.Bold))
                using (SolidBrush sh = new SolidBrush(Color.FromArgb(Alpha(alpha - 60), 0, 0, 0)))
                using (SolidBrush b = new SolidBrush(Color.FromArgb(alpha, e.Color)))
                {
                    g.DrawString(e.Text, f, sh, new RectangleF(s1.X - 70, s1.Y - progress * 40 + 2, 140, 30), Center());
                    g.DrawString(e.Text, f, b, new RectangleF(s1.X - 70, s1.Y - progress * 40, 140, 30), Center());
                }
            }
            else if (e.Kind == "target")
            {
                Rectangle rect = new Rectangle((int)(s1.X - 22), (int)(s1.Y - 12), 44, 24);
                using (Pen p = new Pen(Color.FromArgb(alpha, e.Color), 2.4f)) g.DrawEllipse(p, rect);
                using (Pen p = new Pen(Color.FromArgb(alpha, Color.White), 1.5f)) { g.DrawLine(p, rect.Left, rect.Top + rect.Height / 2, rect.Right, rect.Top + rect.Height / 2); g.DrawLine(p, rect.Left + rect.Width / 2, rect.Top, rect.Left + rect.Width / 2, rect.Bottom); }
            }
        }

        public static void DrawGlowDot(Graphics g, int x, int y, int r, Color c, int alpha)
        {
            alpha = Alpha(alpha);
            using (SolidBrush glow = new SolidBrush(Color.FromArgb(Alpha(alpha / 2), c))) g.FillEllipse(glow, x - r * 2, y - r * 2, r * 4, r * 4);
            using (SolidBrush core = new SolidBrush(Color.FromArgb(alpha, c))) g.FillEllipse(core, x - r, y - r, r * 2, r * 2);
            using (Pen p = new Pen(Color.FromArgb(alpha, Color.White), 1.2f)) g.DrawEllipse(p, x - r, y - r, r * 2, r * 2);
        }

        public static void DrawDroppedItem(Graphics g, DroppedItem item, float cameraX, float cameraY)
        {
            Rectangle r = item.ScreenRect(cameraX, cameraY);
            DrawLargeFileSymbol(g, new Rectangle(r.X + 5, r.Y + 0, 42, 45), item.Color, false);
            using (Font f = Font(7f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.White))
            using (SolidBrush sh = new SolidBrush(Color.FromArgb(120, 0, 0, 0)))
            {
                g.DrawString(item.Name, f, sh, new Rectangle(r.X - 18, r.Y + 45, r.Width + 36, 20), Center());
                g.DrawString(item.Name, f, b, new Rectangle(r.X - 19, r.Y + 44, r.Width + 36, 20), Center());
            }
        }
    }
}
