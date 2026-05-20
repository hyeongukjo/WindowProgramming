using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace DebugHeroFileDungeonRPG
{
    public static class Renderer
    {
        private static readonly Dictionary<NpcMood, Image> npcCache = new Dictionary<NpcMood, Image>();
        private static Image xpBlissBackgroundCache = null;
        private static Bitmap xpBlissScaledCache = null;
        private static Size xpBlissScaledSize = Size.Empty;
        private static readonly Dictionary<string, Bitmap> stageBackgroundCache = new Dictionary<string, Bitmap>();
        private static readonly Dictionary<string, Image> stageImageCache = new Dictionary<string, Image>();
        private static Image playerAgentSheet = null;
        private static Image playerAgentIdleFrame = null;
        private static readonly Image[] playerAgentWalkFrames = new Image[8];
       
        public static Image ImgBoss_DriverK;
        public static Image ImgBoss_BSOD;
        public static Image ImgBoss_HighKernel;

        public static Font F(float size, FontStyle style)
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

        public static Color Lighten(Color c, int v)
        {
            return Color.FromArgb(c.A, Math.Min(255, c.R + v), Math.Min(255, c.G + v), Math.Min(255, c.B + v));
        }

        public static Color Darken(Color c, int v)
        {
            return Color.FromArgb(c.A, Math.Max(0, c.R - v), Math.Max(0, c.G - v), Math.Max(0, c.B - v));
        }

        public static void DrawXPWallpaper(Graphics g, Rectangle r)
        {
            if (r.Width <= 0 || r.Height <= 0) return;
            Size requested = new Size(r.Width, r.Height);
            if (xpBlissScaledCache == null || xpBlissScaledSize != requested)
            {
                if (xpBlissScaledCache != null) xpBlissScaledCache.Dispose();
                xpBlissScaledCache = new Bitmap(r.Width, r.Height);
                xpBlissScaledSize = requested;
                using (Graphics cg = Graphics.FromImage(xpBlissScaledCache))
                {
                    cg.SmoothingMode = SmoothingMode.HighSpeed;
                    cg.CompositingQuality = CompositingQuality.HighSpeed;
                    cg.InterpolationMode = InterpolationMode.Low;
                    cg.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                    Image bg = LoadXPBlissBackground();
                    Rectangle local = new Rectangle(0, 0, r.Width, r.Height);
                    if (bg != null)
                    {
                        DrawImageCover(cg, bg, local);
                        using (LinearGradientBrush overlay = new LinearGradientBrush(local, Color.FromArgb(8, 255, 255, 255), Color.FromArgb(20, 0, 0, 0), 90f))
                            cg.FillRectangle(overlay, local);
                    }
                    else
                    {
                        DrawFallbackXPWallpaper(cg, local);
                    }
                }
            }
            g.DrawImageUnscaled(xpBlissScaledCache, r.X, r.Y);
        }

        private static void DrawFallbackXPWallpaper(Graphics g, Rectangle r)
        {
            using (LinearGradientBrush sky = new LinearGradientBrush(new Rectangle(0, 0, r.Width, r.Height), Color.FromArgb(38, 111, 219), Color.FromArgb(160, 220, 255), 90f))
                g.FillRectangle(sky, r);
            DrawCloud(g, 120, 76, 1.05f);
            DrawCloud(g, 620, 130, 0.85f);
            DrawCloud(g, 1030, 92, 1.1f);
            Point[] far = { new Point(-120, r.Bottom), new Point(220, r.Bottom - 170), new Point(560, r.Bottom - 100), new Point(980, r.Bottom - 215), new Point(r.Right + 130, r.Bottom - 90), new Point(r.Right + 130, r.Bottom) };
            using (SolidBrush b = new SolidBrush(Color.FromArgb(55, 150, 72))) g.FillPolygon(b, far);
            Point[] near = { new Point(-120, r.Bottom), new Point(300, r.Bottom - 95), new Point(780, r.Bottom - 140), new Point(1180, r.Bottom - 78), new Point(r.Right + 160, r.Bottom - 135), new Point(r.Right + 160, r.Bottom) };
            using (SolidBrush b = new SolidBrush(Color.FromArgb(100, 181, 76))) g.FillPolygon(b, near);
        }

        private static Image LoadXPBlissBackground()
        {
            if (xpBlissBackgroundCache != null) return xpBlissBackgroundCache;
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "XPBlissMapBackground.png");
            try
            {
                if (File.Exists(path)) xpBlissBackgroundCache = Image.FromFile(path);
            }
            catch
            {
                xpBlissBackgroundCache = null;
            }
            return xpBlissBackgroundCache;
        }

        private static void DrawImageCover(Graphics g, Image img, Rectangle dest)
        {
            if (img == null || dest.Width <= 0 || dest.Height <= 0) return;
            float scale = Math.Max(dest.Width / (float)img.Width, dest.Height / (float)img.Height);
            int sw = Math.Max(1, (int)(dest.Width / scale));
            int sh = Math.Max(1, (int)(dest.Height / scale));
            int sx = Math.Max(0, (img.Width - sw) / 2);
            int sy = Math.Max(0, (img.Height - sh) / 2);
            Rectangle src = new Rectangle(sx, sy, Math.Min(sw, img.Width - sx), Math.Min(sh, img.Height - sy));
            g.DrawImage(img, dest, src, GraphicsUnit.Pixel);
        }


        private static Image LoadStageBackgroundImage(int stageIndex, bool bossRoom)
        {
            int assetStage = stageIndex;
            string fileName = "StageBg_" + assetStage.ToString("00") + (bossRoom ? "_Boss" : "") + ".png";
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", fileName);
            if (!File.Exists(path) && bossRoom)
            {
                fileName = "StageBg_" + assetStage.ToString("00") + ".png";
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", fileName);
            }
            if (!File.Exists(path)) return null;
            if (stageImageCache.ContainsKey(path)) return stageImageCache[path];
            try
            {
                Image img = Image.FromFile(path);
                stageImageCache[path] = img;
                return img;
            }
            catch
            {
                return null;
            }
        }


        private static void DrawCloud(Graphics g, float x, float y, float s)
        {
            using (SolidBrush b = new SolidBrush(Color.FromArgb(230, 255, 255, 255)))
            {
                g.FillEllipse(b, x, y + 16 * s, 76 * s, 34 * s);
                g.FillEllipse(b, x + 28 * s, y, 92 * s, 58 * s);
                g.FillEllipse(b, x + 90 * s, y + 18 * s, 74 * s, 36 * s);
                g.FillRectangle(b, x + 24 * s, y + 30 * s, 128 * s, 25 * s);
            }
        }

        public static void DrawXPTaskbar(Graphics g, Rectangle client, string title)
        {
            Rectangle bar = new Rectangle(0, client.Bottom - 40, client.Width, 40);
            using (LinearGradientBrush b = new LinearGradientBrush(bar, Color.FromArgb(22, 106, 218), Color.FromArgb(0, 48, 160), 90f))
                g.FillRectangle(b, bar);
            Rectangle start = new Rectangle(8, bar.Y + 5, 102, 30);
            using (LinearGradientBrush b = new LinearGradientBrush(start, Color.FromArgb(118, 222, 82), Color.FromArgb(32, 142, 46), 90f))
                g.FillRectangle(b, start);
            using (Pen p = new Pen(Color.White, 1.5f)) g.DrawRectangle(p, start.X, start.Y, start.Width - 1, start.Height - 1);
            using (Font f = F(11f, FontStyle.Bold))
            using (SolidBrush sb = new SolidBrush(Color.White))
                g.DrawString("시작", f, sb, start, Center());
            Rectangle app = new Rectangle(126, bar.Y + 6, Math.Min(470, client.Width - 520), 28);
            using (LinearGradientBrush b = new LinearGradientBrush(app, Color.FromArgb(88, 164, 255), Color.FromArgb(32, 80, 190), 90f))
                g.FillRectangle(b, app);
            using (Pen p = new Pen(Color.FromArgb(175, 220, 255))) g.DrawRectangle(p, app.X, app.Y, app.Width - 1, app.Height - 1);
            using (Font f = F(9f, FontStyle.Bold))
            using (SolidBrush sb = new SolidBrush(Color.White))
                g.DrawString(title, f, sb, new Rectangle(app.X + 14, app.Y, app.Width - 20, app.Height), LeftMiddle());
            Rectangle tray = new Rectangle(client.Right - 190, bar.Y + 5, 180, 30);
            using (LinearGradientBrush b = new LinearGradientBrush(tray, Color.FromArgb(88, 166, 238), Color.FromArgb(28, 96, 200), 90f))
                g.FillRectangle(b, tray);
            using (Font f = F(9f, FontStyle.Bold))
            using (SolidBrush sb = new SolidBrush(Color.White))
                g.DrawString(DateTime.Now.ToString("tt hh:mm"), f, sb, tray, Center());
        }

        public static void DrawXPWindow(Graphics g, Rectangle r, string title, bool error)
        {
            using (SolidBrush sh = new SolidBrush(Color.FromArgb(55, 0, 0, 0))) g.FillRectangle(sh, r.X + 8, r.Y + 8, r.Width, r.Height);
            using (SolidBrush b = new SolidBrush(Color.FromArgb(236, 241, 248))) g.FillRectangle(b, r);
            using (Pen p = new Pen(Color.FromArgb(0, 68, 170), 2f)) g.DrawRectangle(p, r.X, r.Y, r.Width - 1, r.Height - 1);
            Rectangle tb = new Rectangle(r.X + 2, r.Y + 2, r.Width - 4, 30);
            Color c1 = error ? Color.FromArgb(210, 35, 35) : Color.FromArgb(20, 88, 210);
            Color c2 = error ? Color.FromArgb(116, 0, 0) : Color.FromArgb(60, 145, 255);
            using (LinearGradientBrush b = new LinearGradientBrush(tb, c1, c2, 0f)) g.FillRectangle(b, tb);
            using (Font f = F(10f, FontStyle.Bold))
            using (SolidBrush sb = new SolidBrush(Color.White))
                g.DrawString(title, f, sb, new Rectangle(tb.X + 10, tb.Y, tb.Width - 90, tb.Height), LeftMiddle());
            DrawXPButton(g, new Rectangle(tb.Right - 76, tb.Y + 5, 22, 20), "_");
            DrawXPButton(g, new Rectangle(tb.Right - 52, tb.Y + 5, 22, 20), "□");
            DrawXPButton(g, new Rectangle(tb.Right - 28, tb.Y + 5, 22, 20), "X");
        }

        public static void DrawXPButton(Graphics g, Rectangle r, string text)
        {
            using (LinearGradientBrush b = new LinearGradientBrush(r, Color.White, Color.FromArgb(190, 210, 240), 90f)) g.FillRectangle(b, r);
            using (Pen p = new Pen(Color.FromArgb(40, 80, 150))) g.DrawRectangle(p, r.X, r.Y, r.Width - 1, r.Height - 1);
            using (Font f = F(8f, FontStyle.Bold))
            using (SolidBrush sb = new SolidBrush(Color.FromArgb(25, 35, 60)))
                g.DrawString(text, f, sb, r, Center());
        }

        public static void DrawButton(Graphics g, Rectangle r, string text, bool selected)
        {
            using (LinearGradientBrush b = new LinearGradientBrush(r, selected ? Color.FromArgb(255, 246, 182) : Color.White, selected ? Color.FromArgb(240, 178, 54) : Color.FromArgb(202, 222, 250), 90f)) g.FillRectangle(b, r);
            using (Pen p = new Pen(selected ? Color.FromArgb(190, 105, 0) : Color.FromArgb(56, 110, 190), selected ? 2f : 1f)) g.DrawRectangle(p, r.X, r.Y, r.Width - 1, r.Height - 1);
            using (Font f = F(9f, FontStyle.Bold))
            using (SolidBrush sb = new SolidBrush(Color.FromArgb(30, 38, 54)))
                g.DrawString(text, f, sb, r, Center());
        }

        public static void DrawBar(Graphics g, Rectangle r, int value, int max, Color color)
        {
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(230, 238, 248))) g.FillRectangle(bg, r);
            using (Pen p = new Pen(Color.FromArgb(92, 118, 154))) g.DrawRectangle(p, r.X, r.Y, r.Width - 1, r.Height - 1);
            int w = 0;
            if (max > 0) w = Math.Max(0, Math.Min(r.Width - 2, (int)((r.Width - 2) * (value / (float)max))));
            if (w > 0)
            {
                Rectangle fill = new Rectangle(r.X + 1, r.Y + 1, w, r.Height - 2);
                using (LinearGradientBrush b = new LinearGradientBrush(fill, Lighten(color, 30), Darken(color, 18), 90f)) g.FillRectangle(b, fill);
            }
        }

        private static Image LoadNpc(NpcMood mood)
        {
            if (npcCache.ContainsKey(mood)) return npcCache[mood];
            string name = "basic";
            if (mood == NpcMood.Welcome) name = "welcome";
            else if (mood == NpcMood.Thinking) name = "thinking";
            else if (mood == NpcMood.Happy) name = "happy";
            else if (mood == NpcMood.Question) name = "question";
            else if (mood == NpcMood.Error) name = "error";
            else if (mood == NpcMood.Bsod) name = "bsod";
            else if (mood == NpcMood.Progress) name = "progress";
            else if (mood == NpcMood.Loading) name = "loading";
            else if (mood == NpcMood.Damaged) name = "damaged";
            else if (mood == NpcMood.Log) name = "log";
            else if (mood == NpcMood.Warning) name = "warning";
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "NPC404_" + name + ".png");
            Image img = null;
            try { if (File.Exists(path)) img = Image.FromFile(path); } catch { img = null; }
            npcCache[mood] = img;
            return img;
        }

        public static void DrawNpcImage(Graphics g, Rectangle r, NpcMood mood)
        {
            Image img = LoadNpc(mood);
            if (img != null)
            {
                InterpolationMode old = g.InterpolationMode;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                float scale = Math.Min(r.Width / (float)img.Width, r.Height / (float)img.Height);
                int w = (int)(img.Width * scale);
                int h = (int)(img.Height * scale);
                Rectangle dst = new Rectangle(r.X + (r.Width - w) / 2, r.Y + (r.Height - h) / 2, w, h);
                g.DrawImage(img, dst);
                g.InterpolationMode = old;
            }
            else
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(42, 70, 120))) g.FillRectangle(b, r);
                using (Font f = F(9f, FontStyle.Bold)) g.DrawString("Recovery\nAssistant", f, Brushes.White, r, Center());
            }
        }

        public static void DrawNotification(Graphics g, Rectangle r, string title, string text, NpcMood mood, bool error)
        {
            DrawXPWindow(g, r, title, error);
            Rectangle npc = new Rectangle(r.X + 22, r.Y + 58, 140, Math.Max(120, r.Height - 104));
            DrawNpcImage(g, npc, mood);

            if (error)
            {
                Rectangle warn = new Rectangle(r.X + 172, r.Y + 58, 42, 42);
                Point[] tri = new Point[] { new Point(warn.X + warn.Width / 2, warn.Y + 2), new Point(warn.Right - 2, warn.Bottom - 2), new Point(warn.X + 2, warn.Bottom - 2) };
                using (SolidBrush wb = new SolidBrush(Color.FromArgb(255, 220, 70))) g.FillPolygon(wb, tri);
                using (Pen wp = new Pen(Color.DarkRed, 2f)) g.DrawPolygon(wp, tri);
                using (Font wf = F(18f, FontStyle.Bold))
                using (SolidBrush rb = new SolidBrush(Color.DarkRed))
                    g.DrawString("!", wf, rb, warn, Center());
            }

            Rectangle tx = new Rectangle(r.X + 222, r.Y + 56, r.Width - 246, r.Height - 110);
            using (Font f = F(10f, FontStyle.Regular))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(26, 34, 54)))
                g.DrawString(text, f, b, tx, Left());
            Rectangle btn = new Rectangle(r.Right - 120, r.Bottom - 46, 96, 30);
            DrawButton(g, btn, "확인", true);
        }



        private static Image LoadPlayerAgentFrame(int index)
        {
            // index < 0 : idle frame. index 0~7 : video-style Player.exe walk frames with fixed baseline, bent knees, and alternating arms/legs.
            if (index < 0)
            {
                if (playerAgentIdleFrame == null)
                {
                    string idlePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "PlayerAgentIdle.png");
                    if (File.Exists(idlePath))
                    {
                        try { playerAgentIdleFrame = Image.FromFile(idlePath); } catch { playerAgentIdleFrame = null; }
                    }
                }
                return playerAgentIdleFrame;
            }

            int slot = Math.Max(0, Math.Min(7, index));
            if (playerAgentWalkFrames[slot] == null)
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "PlayerAgentWalk" + slot.ToString() + ".png");
                if (File.Exists(path))
                {
                    try { playerAgentWalkFrames[slot] = Image.FromFile(path); } catch { playerAgentWalkFrames[slot] = null; }
                }
            }
            return playerAgentWalkFrames[slot];
        }

        private static void DrawPlayerAgentFrameImage(Graphics g, float worldX, float worldY, int facing, int frameIndex, bool walking)
        {
            Image frame = LoadPlayerAgentFrame(frameIndex);
            if (frame == null)
            {
                DrawAntiVirusAgentCharacter(g, worldX, worldY, facing, walking, Environment.TickCount / 180.0, false, false);
                return;
            }

            GraphicsState state = g.Save();
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            if (facing < 0)
            {
                g.TranslateTransform(worldX, worldY);
                g.ScaleTransform(-1f, 1f);
                worldX = 0;
                worldY = 0;
            }

            // PlayerAgentIdle/Walk frames are right-facing by default and share one transparent canvas with the character centered at the bottom.
            // This avoids the previous left-right "clone/jitter" feeling caused by uneven sprite-sheet crops.
            int destW = 138;
            int destH = 174;
            Rectangle dest = new Rectangle((int)(worldX - destW / 2), (int)(worldY - destH + 4), destW, destH);
            g.DrawImage(frame, dest);
            g.Restore(state);
        }

        private static Image GetPlayerAgentSheet()
        {
            if (playerAgentSheet != null) return playerAgentSheet;
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "PlayerAgentSpriteSheet.png");
            if (File.Exists(path))
            {
                try { playerAgentSheet = Image.FromFile(path); } catch { playerAgentSheet = null; }
            }
            return playerAgentSheet;
        }

        private static Rectangle GetPlayerAgentSource(string pose, int frame)
        {
            // Source image is a transparent sprite sheet generated from the provided Player.exe reference.
            // Coordinates are hand-picked to avoid labels and keep the character/action clean.
            if (pose == "clean" || pose == "scan") return new Rectangle(80, 370, 460, 240);
            if (pose == "delete") return new Rectangle(760, 370, 650, 240);
            if (pose == "clean2") return new Rectangle(420, 670, 320, 240);
            if (pose == "delete2") return new Rectangle(740, 670, 650, 240);
            int[] xs = new int[] { 85, 265, 445, 630 };
            int idx = Math.Abs(frame) % xs.Length;
            return new Rectangle(xs[idx], 70, 180, 250);
        }

        private static void DrawPlayerAgentSprite(Graphics g, string pose, float worldX, float worldY, int facing, int frame)
        {
            Image sheet = GetPlayerAgentSheet();
            if (sheet == null) return;
            Rectangle src = GetPlayerAgentSource(pose, frame);
            GraphicsState state = g.Save();
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (facing < 0)
            {
                g.TranslateTransform(worldX, worldY);
                g.ScaleTransform(-1f, 1f);
                worldX = 0;
                worldY = 0;
            }
            Rectangle dest;
            if (pose == "clean" || pose == "scan")
            {
                dest = new Rectangle((int)(worldX - 100), (int)(worldY - 166), 375, 196);
            }
            else if (pose == "delete")
            {
                dest = new Rectangle((int)(worldX - 100), (int)(worldY - 166), 470, 174);
            }
            else
            {
                dest = new Rectangle((int)(worldX - 53), (int)(worldY - 145), 106, 148);
            }
            g.DrawImage(sheet, dest, src, GraphicsUnit.Pixel);
            g.Restore(state);
        }


        private static void DrawPlayerAgentCharacterFrame(Graphics g, string pose, float worldX, float worldY, int facing, int frame)
        {
            bool walking = pose == "walk";
            if (pose == "idle" || pose == "walk")
            {
                // Idle stays in a fixed attention pose; walking uses the right-facing 8-frame real walk cycle. Left movement is rendered by horizontal flip to prevent moonwalk.
                int frameIndex = walking ? Math.Abs(frame) % 8 : -1;
                DrawPlayerAgentFrameImage(g, worldX, worldY, facing, frameIndex, walking);
                return;
            }

            // 공격 자세는 실제 공격 이펙트에서 캐릭터를 다시 그리지 않도록 했지만,
            // 혹시 다른 화면에서 호출되면 원본 시트의 해당 자세를 사용합니다.
            Image sheet = GetPlayerAgentSheet();
            if (sheet == null)
            {
                DrawAntiVirusAgentCharacter(g, worldX, worldY, facing, false, Environment.TickCount / 150.0, pose == "clean" || pose == "delete", pose == "delete");
                return;
            }
            Rectangle src = pose == "delete" ? new Rectangle(760, 370, 650, 240) : new Rectangle(80, 370, 460, 240);
            GraphicsState state = g.Save();
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            if (facing < 0)
            {
                g.TranslateTransform(worldX, worldY);
                g.ScaleTransform(-1f, 1f);
                worldX = 0;
                worldY = 0;
            }
            Rectangle dest = pose == "delete"
                ? new Rectangle((int)(worldX - 100), (int)(worldY - 166), 470, 174)
                : new Rectangle((int)(worldX - 100), (int)(worldY - 166), 375, 196);
            g.DrawImage(sheet, dest, src, GraphicsUnit.Pixel);
            g.Restore(state);
        }

        public static void DrawRecoveryProgram(Graphics g, PlayerState p, bool selected)
        {
            DrawRecoveryProgram(g, p, selected, 0f, false);
        }


        public static void DrawRecoveryProgram(Graphics g, PlayerState p, bool selected, float cameraX, bool moving)
        {
            float drawX = p.X - cameraX;
            float baseY = p.Y;
            float speed = (float)Math.Sqrt(p.MoveVelocityX * p.MoveVelocityX + p.MoveVelocityY * p.MoveVelocityY);
            bool walking = moving && speed > 0.18f;
            int facing = p.Facing == 0 ? 1 : p.Facing;

            // 원본 Player.exe 캐릭터 디자인을 유지하면서, 양쪽 팔과 다리가 번갈아 움직이는 전용 보행 프레임을 사용합니다.
            // 모든 프레임은 같은 발 위치와 중심점에 정렬되어 문워크/좌우 흔들림을 줄입니다.
            int frame = walking ? ((int)Math.Floor(p.WalkCycle)) % 8 : -1;
            // 아주 작은 보행 bob만 적용해 사람처럼 무게중심이 자연스럽게 이어지도록 합니다.
            float walkPhase = (float)((p.WalkCycle / 8f) * Math.PI * 2f);
            float bob = walking ? -Math.Abs((float)Math.Sin(walkPhase)) * 1.15f : 0f;

            if (selected)
            {
                using (SolidBrush aura = new SolidBrush(Color.FromArgb(walking ? 24 : 18, 70, 180, 255)))
                    g.FillEllipse(aura, (int)drawX - 54, (int)baseY - 112, 108, 120);
            }
            if (p.DefenseTicks > 0)
            {
                using (Pen shield = new Pen(Color.FromArgb(150, 120, 210, 255), 3f))
                    g.DrawEllipse(shield, drawX - 58, baseY - 120, 116, 130);
            }

            DrawPlayerAgentCharacterFrame(g, walking ? "walk" : "idle", drawX, baseY + bob, facing, frame);

            using (Font f = F(7.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.White))
            using (SolidBrush back = new SolidBrush(Color.FromArgb(150, 0, 0, 0)))
            {
                Rectangle label = new Rectangle((int)drawX - 75, (int)baseY + 18, 150, 34);
                g.FillRectangle(back, label);
                g.DrawString("Player.exe\n(AntiVirus Agent)", f, b, label, Center());
            }
        }


        public static void DrawShopShortcut(Graphics g, Rectangle r, int coins)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (SolidBrush shadow = new SolidBrush(Color.FromArgb(75, 0, 0, 0)))
                g.FillEllipse(shadow, r.X + 22, r.Y + 74, r.Width - 44, 18);
            Rectangle bin = new Rectangle(r.X + 43, r.Y + 10, 62, 68);
            using (LinearGradientBrush b = new LinearGradientBrush(bin, Color.White, Color.FromArgb(190, 215, 235), 90f))
                g.FillRectangle(b, bin);
            using (Pen p = new Pen(Color.FromArgb(70, 100, 120), 2f))
                g.DrawRectangle(p, bin);
            using (Pen p = new Pen(Color.FromArgb(70, 165, 90), 3f))
                g.DrawArc(p, bin.X + 10, bin.Y + 12, bin.Width - 20, bin.Height - 20, 30, 260);
            Rectangle label = new Rectangle(r.X + 4, r.Y + 82, r.Width - 8, 36);
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(150, 20, 70, 150))) g.FillRectangle(bg, label);
            using (Font f = F(8.5f, FontStyle.Bold))
            using (SolidBrush wb = new SolidBrush(Color.White))
                g.DrawString("휴지통 상점\n" + coins + " coin", f, wb, label, Center());
        }

        public static void DrawFileShortcut(Graphics g, Rectangle r, StageInfo st, bool selected, bool newlyCreated)
        {
            using (SolidBrush glow = new SolidBrush(Color.FromArgb(selected ? 76 : 30, 50, 120, 255))) g.FillEllipse(glow, r.X - 6, r.Y - 2, r.Width + 12, r.Height + 18);
            Rectangle icon = new Rectangle(r.X + 18, r.Y + 6, 52, 62);
            Point[] paper = { new Point(icon.X + 6, icon.Y), new Point(icon.Right - 12, icon.Y), new Point(icon.Right, icon.Y + 12), new Point(icon.Right, icon.Bottom), new Point(icon.X + 6, icon.Bottom) };
            using (LinearGradientBrush b = new LinearGradientBrush(icon, Color.White, Color.FromArgb(214, 226, 244), 90f)) g.FillPolygon(b, paper);
            using (Pen p = new Pen(st.Accent, 2f)) g.DrawPolygon(p, paper);
            using (SolidBrush b = new SolidBrush(Color.FromArgb(44, st.Accent))) g.FillRectangle(b, icon.X + 14, icon.Y + 24, icon.Width - 24, 17);
            using (Pen p = new Pen(st.Accent, 2.2f))
            {
                g.DrawLine(p, icon.X + 16, icon.Y + 28, icon.Right - 14, icon.Y + 28);
                g.DrawLine(p, icon.X + 16, icon.Y + 36, icon.Right - 18, icon.Y + 36);
            }
            // shortcut arrow
            Rectangle ar = new Rectangle(r.X + 10, r.Y + 52, 22, 18);
            using (SolidBrush b = new SolidBrush(Color.White)) g.FillRectangle(b, ar);
            using (Pen p = new Pen(Color.FromArgb(28, 74, 180), 3f))
            {
                g.DrawLine(p, ar.X + 5, ar.Y + 12, ar.Right - 6, ar.Y + 12);
                g.DrawLine(p, ar.X + 6, ar.Y + 12, ar.X + 12, ar.Y + 5);
            }
            if (newlyCreated)
            {
                Rectangle tag = new Rectangle(r.Right - 40, r.Y + 4, 36, 18);
                using (SolidBrush b = new SolidBrush(Color.Red)) g.FillRectangle(b, tag);
                using (Font f = F(7f, FontStyle.Bold)) g.DrawString("NEW", f, Brushes.White, tag, Center());
            }
            if (selected)
            {
                using (Pen p = new Pen(Color.FromArgb(255, 210, 60), 3f)) g.DrawRectangle(p, r.X + 2, r.Y + 2, r.Width - 5, r.Height - 5);
            }
            using (Font f = F(8.2f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.White))
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(120, 0, 0, 0)))
            {
                Rectangle lab = new Rectangle(r.X - 12, r.Bottom - 34, r.Width + 24, 34);
                g.FillRectangle(bg, lab);
                g.DrawString(st.FileName, f, b, lab, Center());
            }
        }


        private static void DrawNaturalWalkOverlay(Graphics g, float x, float y, int facing, double phase, float speed)
        {
            float swing = (float)Math.Sin(phase * Math.PI * 2.0);
            float counter = -swing;
            int dir = facing >= 0 ? 1 : -1;
            int alpha = 185;

            // 팔 스윙: 손이 앞뒤로 번갈아 움직여 걷는 느낌을 보강합니다.
            using (Pen armBack = new Pen(Color.FromArgb(alpha, 20, 45, 90), 4.2f))
            using (Pen armFront = new Pen(Color.FromArgb(alpha, 235, 242, 250), 4.6f))
            {
                g.DrawLine(armBack, x - dir * 16, y - 75, x - dir * (22 + counter * 9), y - 55 + counter * 3);
                g.DrawLine(armFront, x + dir * 15, y - 75, x + dir * (23 + swing * 9), y - 55 + swing * 3);
            }

            // 다리 스윙: 신발 위치를 교차시켜 사람처럼 걷는 실루엣을 만듭니다.
            using (Pen legBack = new Pen(Color.FromArgb(alpha, 20, 25, 34), 5.0f))
            using (Pen legFront = new Pen(Color.FromArgb(alpha, 22, 28, 40), 5.4f))
            using (SolidBrush shoe = new SolidBrush(Color.FromArgb(alpha, 240, 245, 250)))
            using (Pen shoeLine = new Pen(Color.FromArgb(alpha, 35, 55, 85), 1.2f))
            {
                float hipY = y - 44;
                float footY = y - 5;
                float backFootX = x - dir * (10 + counter * 9);
                float frontFootX = x + dir * (10 + swing * 9);
                g.DrawLine(legBack, x - 8, hipY, backFootX, footY);
                g.DrawLine(legFront, x + 8, hipY, frontFootX, footY);
                RectangleF shoe1 = new RectangleF(backFootX - 9, footY - 1, 18, 7);
                RectangleF shoe2 = new RectangleF(frontFootX - 9, footY - 1, 18, 7);
                g.FillEllipse(shoe, shoe1);
                g.FillEllipse(shoe, shoe2);
                g.DrawEllipse(shoeLine, shoe1);
                g.DrawEllipse(shoeLine, shoe2);
            }
        }

        private static void DrawAttentionIdleOverlay(Graphics g, float x, float y, int facing)
        {
            // 대기 상태: 손과 발이 거의 고정된 정자세 느낌만 살짝 강조합니다.
            int alpha = 130;
            using (Pen hand = new Pen(Color.FromArgb(alpha, 240, 245, 250), 4.0f))
            using (Pen foot = new Pen(Color.FromArgb(alpha, 240, 245, 250), 3.6f))
            {
                g.DrawLine(hand, x - 20, y - 67, x - 20, y - 54);
                g.DrawLine(hand, x + 20, y - 67, x + 20, y - 54);
                g.DrawLine(foot, x - 16, y - 5, x - 5, y - 5);
                g.DrawLine(foot, x + 5, y - 5, x + 16, y - 5);
            }
        }

        private static int ClampAlpha(int value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return value;
        }


        private static void DrawShimmerBeam(Graphics g, float x1, float y1, float x2, float y2, Color color, int alpha, float progress, bool deleteBeam)
        {
            if (deleteBeam)
            {
                DrawDeleteBeam(g, x1, y1, x2, y2, color, alpha, progress);
            }
            else
            {
                DrawCleanSprayBeam(g, x1, y1, x2, y2, color, alpha, progress);
            }
        }


        private static void RoundCaps(Pen pen)
        {
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;
            pen.LineJoin = LineJoin.Round;
        }

        private static void DrawLimb(Graphics g, Pen pen, float x1, float y1, float x2, float y2, float x3, float y3)
        {
            RoundCaps(pen);
            g.DrawLine(pen, x1, y1, x2, y2);
            g.DrawLine(pen, x2, y2, x3, y3);
        }

        private static void DrawAntiVirusAgentCharacter(Graphics g, float x, float groundY, int facing, bool walking, double phase, bool aiming, bool deleteMode)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int dir = facing >= 0 ? 1 : -1;
            float walk = walking ? 1f : 0f;
            float s = walking ? (float)Math.Sin(phase) : 0f;
            float c = walking ? (float)Math.Cos(phase) : 1f;
            float bob = walking ? -Math.Abs(s) * 3.2f : 0f;
            float y = groundY + bob;
            Color jacket = Color.FromArgb(22, 76, 155);
            Color jacketLight = Color.FromArgb(48, 125, 220);
            Color outline = Color.FromArgb(18, 26, 38);

            using (SolidBrush shadow = new SolidBrush(Color.FromArgb(105, 0, 0, 0)))
                g.FillEllipse(shadow, x - 38, groundY + 3, 76, 16);

            // Backpack behind body.
            RectangleF pack = new RectangleF(x - dir * 43 - 10, y - 92, 22, 58);
            using (LinearGradientBrush pb = new LinearGradientBrush(pack, Color.FromArgb(70, 110, 145), Color.FromArgb(24, 42, 64), 90f))
                g.FillRoundedRectangle(pb, pack, 5);
            using (Pen p = new Pen(outline, 2f)) g.DrawRoundedRectangle(p, pack, 5);
            using (SolidBrush led = new SolidBrush(Color.FromArgb(105, 255, 90)))
                g.FillRectangle(led, pack.X + pack.Width / 2 - 3, pack.Y + 7, 6, 12);

            // Legs first: two-segment walk with alternating feet.
            float hipY = y - 43;
            float kneeY = y - 22;
            float footY = groundY + 2;
            float leftFoot = -12f + dir * s * 12f;
            float rightFoot = 12f - dir * s * 12f;
            float leftKnee = -8f + dir * s * 5f;
            float rightKnee = 8f - dir * s * 5f;
            if (!walking)
            {
                leftFoot = -14f; rightFoot = 14f; leftKnee = -9f; rightKnee = 9f;
                kneeY = y - 22; footY = groundY + 1;
            }
            using (Pen pants = new Pen(Color.FromArgb(24, 28, 38), 7f))
            using (Pen pantsHi = new Pen(Color.FromArgb(52, 66, 88), 2f))
            {
                DrawLimb(g, pants, x - 8, hipY, x + leftKnee, kneeY, x + leftFoot, footY);
                DrawLimb(g, pants, x + 8, hipY, x + rightKnee, kneeY, x + rightFoot, footY);
                g.DrawLine(pantsHi, x + leftKnee - 1, kneeY, x + leftFoot - 1, footY);
                g.DrawLine(pantsHi, x + rightKnee - 1, kneeY, x + rightFoot - 1, footY);
            }
            using (SolidBrush shoe = new SolidBrush(Color.FromArgb(235, 242, 250)))
            using (Pen shoeLine = new Pen(Color.FromArgb(15, 36, 68), 2f))
            {
                RectangleF sh1 = new RectangleF(x + leftFoot - 11, footY - 2, 22, 8);
                RectangleF sh2 = new RectangleF(x + rightFoot - 11, footY - 2, 22, 8);
                g.FillRoundedRectangle(shoe, sh1, 4);
                g.FillRoundedRectangle(shoe, sh2, 4);
                g.DrawRoundedRectangle(shoeLine, sh1, 4);
                g.DrawRoundedRectangle(shoeLine, sh2, 4);
            }

            // Body.
            RectangleF body = new RectangleF(x - 19, y - 86, 38, 48);
            using (LinearGradientBrush jb = new LinearGradientBrush(body, jacketLight, jacket, 90f))
                g.FillRoundedRectangle(jb, body, 7);
            using (Pen p = new Pen(outline, 2.2f)) g.DrawRoundedRectangle(p, body, 7);
            using (SolidBrush stripe = new SolidBrush(Color.FromArgb(190, 125, 185, 255)))
            {
                g.FillRectangle(stripe, body.X + 5, body.Y + 8, 4, body.Height - 12);
                g.FillRectangle(stripe, body.Right - 9, body.Y + 8, 4, body.Height - 12);
            }
            using (SolidBrush badge = new SolidBrush(Color.White)) g.FillRoundedRectangle(badge, new RectangleF(x - 7, y - 64, 14, 18), 2);
            using (SolidBrush badgeCore = new SolidBrush(Color.FromArgb(75, 190, 100))) g.FillRectangle(badgeCore, x - 3, y - 58, 6, 5);

            // Arms. During walk arms swing opposite to legs. During attack, front arm is extended.
            float armSwing = walking ? -s * 12f : 0f;
            float rearSwing = walking ? s * 10f : 0f;
            using (Pen armBlue = new Pen(Color.FromArgb(22, 68, 145), 7f))
            using (Pen glove = new Pen(Color.FromArgb(235, 242, 250), 7f))
            using (Pen scanner = new Pen(deleteMode ? Color.FromArgb(190, 28, 28) : Color.FromArgb(45, 170, 70), 6f))
            {
                RoundCaps(armBlue); RoundCaps(glove); RoundCaps(scanner);
                if (aiming)
                {
                    // Rear arm lowered, front arm extended like firing the scanner.
                    g.DrawLine(armBlue, x - dir * 15, y - 76, x - dir * 22, y - 55);
                    g.DrawLine(glove, x - dir * 22, y - 55, x - dir * 25, y - 47);
                    g.DrawLine(armBlue, x + dir * 15, y - 76, x + dir * 41, y - 69);
                    g.DrawLine(glove, x + dir * 38, y - 69, x + dir * 50, y - 67);
                    g.DrawLine(scanner, x + dir * 48, y - 67, x + dir * 63, y - 67);
                    DrawTinyCross(g, x + dir * 62, y - 67, 4, deleteMode ? Color.FromArgb(255, 75, 70) : Color.FromArgb(120, 255, 100), 210);
                }
                else
                {
                    g.DrawLine(armBlue, x + dir * 15, y - 77, x + dir * (20 + armSwing), y - 55 + Math.Abs(s) * 2);
                    g.DrawLine(glove, x + dir * (20 + armSwing), y - 55 + Math.Abs(s) * 2, x + dir * (24 + armSwing), y - 47);
                    g.DrawLine(armBlue, x - dir * 15, y - 77, x - dir * (20 + rearSwing), y - 55 + Math.Abs(c) * 2);
                    g.DrawLine(glove, x - dir * (20 + rearSwing), y - 55 + Math.Abs(c) * 2, x - dir * (24 + rearSwing), y - 47);
                }
            }

            // Head and hair on top.
            using (SolidBrush skin = new SolidBrush(Color.FromArgb(246, 198, 142)))
                g.FillEllipse(skin, x - 14, y - 117, 28, 30);
            using (Pen p = new Pen(outline, 2f)) g.DrawEllipse(p, x - 14, y - 117, 28, 30);
            using (SolidBrush hair = new SolidBrush(Color.FromArgb(18, 22, 28)))
            {
                g.FillPie(hair, x - 18, y - 124, 36, 25, 180, 180);
                PointF[] bangs = new PointF[]
                {
                    new PointF(x - 17, y - 106), new PointF(x - 10, y - 118), new PointF(x - 5, y - 106),
                    new PointF(x + 1, y - 119), new PointF(x + 7, y - 106), new PointF(x + 15, y - 116), new PointF(x + 18, y - 104)
                };
                g.FillPolygon(hair, bangs);
            }
            using (SolidBrush eye = new SolidBrush(Color.FromArgb(24, 120, 80)))
            {
                float eyeOffset = dir > 0 ? 4f : -8f;
                g.FillRectangle(eye, x + eyeOffset, y - 103, 4, 5);
                if (!aiming) g.FillRectangle(eye, x - eyeOffset - 4, y - 103, 3, 5);
            }

            // Shoulder security emblem.
            using (SolidBrush shield = new SolidBrush(Color.FromArgb(225, 245, 255)))
            using (Pen sp = new Pen(Color.FromArgb(55, 130, 220), 1.5f))
            {
                RectangleF sr = new RectangleF(x - dir * 27 - 8, y - 82, 16, 18);
                g.FillEllipse(shield, sr);
                g.DrawEllipse(sp, sr);
                using (SolidBrush core = new SolidBrush(Color.FromArgb(75, 185, 95))) g.FillRectangle(core, sr.X + 6, sr.Y + 6, 4, 8);
            }
        }

        private static void DrawTinyCross(Graphics g, float x, float y, int size, Color color, int alpha)
        {
            using (Pen glow = new Pen(Color.FromArgb(ClampAlpha(alpha / 2), color), Math.Max(2f, size * 0.7f)))
            using (Pen core = new Pen(Color.FromArgb(ClampAlpha(alpha), Color.White), Math.Max(1.2f, size * 0.28f)))
            {
                RoundCaps(glow); RoundCaps(core);
                g.DrawLine(glow, x - size, y, x + size, y);
                g.DrawLine(glow, x, y - size, x, y + size);
                g.DrawLine(core, x - size, y, x + size, y);
                g.DrawLine(core, x, y - size, x, y + size);
            }
        }

        private static float EaseOut(float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            return 1f - (1f - t) * (1f - t);
        }

        private static void DrawCleanSprayBeam(Graphics g, float x1, float y1, float x2, float y2, Color color, int alpha, float progress)
        {
            int dir = x2 >= x1 ? 1 : -1;
            float dx = x2 - x1;
            float dy = y2 - y1;
            float visible = EaseOut(progress * 1.08f);
            float endX = x1 + dx * visible;
            float endY = y1 + dy * visible;
            Color green = Color.FromArgb(95, 255, 80);
            Color mint = Color.FromArgb(150, 255, 150);

            // 부드럽게 분사되는 부채꼴 안개. 시작부터 전체가 터지지 않고 길이가 점점 늘어납니다.
            PointF[] cone = new PointF[]
            {
                new PointF(x1, y1 - 5),
                new PointF(endX, endY - 34),
                new PointF(endX, endY + 34),
                new PointF(x1, y1 + 5)
            };
            using (SolidBrush haze = new SolidBrush(Color.FromArgb(ClampAlpha(alpha / 5), green))) g.FillPolygon(haze, cone);

            using (Pen stream = new Pen(Color.FromArgb(ClampAlpha(alpha / 2), green), 4.2f))
            using (Pen stream2 = new Pen(Color.FromArgb(ClampAlpha(alpha / 3), mint), 2.4f))
            using (Pen core = new Pen(Color.FromArgb(ClampAlpha(alpha / 2), Color.White), 1.2f))
            {
                RoundCaps(stream); RoundCaps(stream2); RoundCaps(core);
                for (int i = 0; i < 2; i++)
                {
                    float off = (i - 0.5f) * 10f;
                    float wave = (float)Math.Sin(progress * 7.5f + i * 1.3f) * 9f;
                    float sx = x1;
                    float sy = y1 + off * 0.18f;
                    float ex = endX;
                    float ey = endY + off * 0.42f;
                    g.DrawBezier(stream, sx, sy, x1 + dx * 0.24f * visible, y1 + off + wave, x1 + dx * 0.68f * visible, y2 - off - wave, ex, ey);
                    g.DrawBezier(stream2, sx, sy, x1 + dx * 0.30f * visible, y1 + off - wave, x1 + dx * 0.72f * visible, y2 + off + wave, ex, ey);
                    if (i == 1) g.DrawLine(core, x1, y1, endX, endY);
                }
            }

            // 녹색 십자가가 “뾰롱뾰롱” 순서대로 생기도록 birth time을 나누어 줍니다.
            int count = 28;
            for (int i = 0; i < count; i++)
            {
                float birth = i / (float)count * 0.94f;
                float life = (progress - birth) / 0.28f;
                if (life < 0f || life > 1f) continue;
                float t = birth + life * 0.18f;
                if (t > visible) t = visible;
                float px = x1 + dx * t;
                float spread = (float)Math.Sin(i * 1.71 + progress * 8.0f) * (10f + 22f * t);
                float py = y1 + dy * t + spread;
                int a = ClampAlpha((int)(alpha * (1f - life * 0.75f)));
                int size = 4 + (i % 4);
                DrawTinyCross(g, px, py, size, i % 2 == 0 ? green : mint, a);

                // 작은 반짝이 점을 함께 흘려서 분사감 보강.
                if (i % 2 == 0)
                {
                    using (SolidBrush dot = new SolidBrush(Color.FromArgb(ClampAlpha(a / 2), Color.White)))
                        g.FillEllipse(dot, px + dir * (7 + i % 4), py - 3, 3, 3);
                }
            }
        }

        private static void DrawDeleteBeam(Graphics g, float x1, float y1, float x2, float y2, Color color, int alpha, float progress)
        {
            int dir = x2 >= x1 ? 1 : -1;
            float dx = x2 - x1;
            float dy = y2 - y1;
            float visible = EaseOut(progress * 1.2f);
            float endX = x1 + dx * visible;
            float endY = y1 + dy * visible;
            PointF[] fan = new PointF[]
            {
                new PointF(x1, y1 - 8), new PointF(endX, endY - 32), new PointF(endX, endY + 32), new PointF(x1, y1 + 8)
            };
            using (SolidBrush glow = new SolidBrush(Color.FromArgb(ClampAlpha(alpha / 4), color))) g.FillPolygon(glow, fan);
            using (Pen beam = new Pen(Color.FromArgb(ClampAlpha(alpha / 2), color), 11f))
            using (Pen core = new Pen(Color.FromArgb(ClampAlpha(alpha), Color.White), 2.2f))
            {
                RoundCaps(beam); RoundCaps(core);
                g.DrawLine(beam, x1, y1, endX, endY);
                g.DrawLine(core, x1, y1, endX, endY);
            }
            for (int i = 0; i < 26; i++)
            {
                float t = (i / 26f + progress * 0.22f) % 1f;
                if (t > visible) continue;
                float px = x1 + dx * t;
                float py = y1 + dy * t + (float)Math.Sin(i * 1.9 + progress * 8) * 18f;
                int sz = 3 + i % 4;
                using (SolidBrush rb = new SolidBrush(Color.FromArgb(ClampAlpha(alpha - i * 3), color)))
                    g.FillRectangle(rb, px - sz / 2, py - sz / 2, sz, sz);
            }
        }

        public static void DrawEnemy(Graphics g, GameEntity e, float cameraX)
        {
            RectangleF b = e.Bounds;
            Rectangle r = Rectangle.Round(new RectangleF(b.X - cameraX, b.Y, b.Width, b.Height));
            using (SolidBrush sh = new SolidBrush(Color.FromArgb(80, 0, 0, 0))) g.FillEllipse(sh, r.X + 4, r.Bottom - 10, r.Width - 8, 12);
            if (e.IsBoss)
            {
                DrawBoss(g, r, e);
            }
            else
            {
                DrawFileMonster(g, r, e);
            }
            Rectangle hp = new Rectangle(r.X, r.Y - 18, r.Width, 8);
            DrawBar(g, hp, e.Hp, e.MaxHp, e.IsBoss ? Color.Red : Color.OrangeRed);
            using (Font f = F(7f, FontStyle.Bold))
            using (SolidBrush sb = new SolidBrush(Color.White))
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(140, 0, 0, 0)))
            {
                Rectangle name = new Rectangle(r.X - 20, r.Y - 38, r.Width + 40, 16);
                g.FillRectangle(bg, name);
                g.DrawString(e.DisplayName, f, sb, name, Center());
            }
        }

        public static void DrawWeaponUpgradeFile(Graphics g, WeaponUpgradeFile drop, float cameraX)
        {
            RectangleF b = drop.Bounds;
            Rectangle r = Rectangle.Round(new RectangleF(b.X - cameraX, b.Y, b.Width, b.Height));
            int glowAlpha = drop.Dragging ? 110 : 70;
            using (SolidBrush glow = new SolidBrush(Color.FromArgb(glowAlpha, 80, 190, 255)))
                g.FillEllipse(glow, r.X - 16, r.Y - 14, r.Width + 32, r.Height + 28);
            using (SolidBrush shadow = new SolidBrush(Color.FromArgb(85, 0, 0, 0)))
                g.FillEllipse(shadow, r.X + 6, r.Bottom - 8, r.Width - 12, 12);

            Point[] paper =
            {
                new Point(r.X + 8, r.Y + 4),
                new Point(r.Right - 14, r.Y + 4),
                new Point(r.Right - 4, r.Y + 16),
                new Point(r.Right - 4, r.Bottom - 10),
                new Point(r.X + 8, r.Bottom - 10)
            };
            using (LinearGradientBrush fill = new LinearGradientBrush(r, Color.White, Color.FromArgb(185, 225, 255), 90f))
                g.FillPolygon(fill, paper);
            using (Pen outline = new Pen(Color.FromArgb(40, 105, 210), 2f))
                g.DrawPolygon(outline, paper);
            using (SolidBrush fold = new SolidBrush(Color.FromArgb(120, 170, 225)))
            {
                Point[] corner =
                {
                    new Point(r.Right - 14, r.Y + 4),
                    new Point(r.Right - 4, r.Y + 16),
                    new Point(r.Right - 14, r.Y + 16)
                };
                g.FillPolygon(fold, corner);
            }
            using (Font f = F(7f, FontStyle.Bold))
            using (SolidBrush text = new SolidBrush(Color.FromArgb(25, 70, 150)))
            {
                g.DrawString("WEAPON", f, text, new Rectangle(r.X + 6, r.Y + 20, r.Width - 12, 14), Center());
                g.DrawString("+" + drop.UpgradeLevel, f, text, new Rectangle(r.X + 6, r.Y + 38, r.Width - 12, 16), Center());
            }
        }

        private static void DrawFileMonster(Graphics g, Rectangle r, GameEntity e)
        {
            if (e.HitFlash > 0) using (SolidBrush flash = new SolidBrush(Color.FromArgb(120, Color.White))) g.FillEllipse(flash, r.X - 8, r.Y - 8, r.Width + 16, r.Height + 16);
            Rectangle icon = new Rectangle(r.X + r.Width / 2 - 25, r.Y + 8, 50, 55);
            using (LinearGradientBrush b = new LinearGradientBrush(icon, Color.White, Color.FromArgb(225, 232, 246), 90f)) g.FillRectangle(b, icon);
            using (Pen p = new Pen(e.Color, 2f)) g.DrawRectangle(p, icon.X, icon.Y, icon.Width - 1, icon.Height - 1);
            using (SolidBrush b = new SolidBrush(Color.FromArgb(54, e.Color))) g.FillRectangle(b, icon.X + 8, icon.Y + 14, icon.Width - 16, 15);
            using (Font f = F(7f, FontStyle.Bold))
            using (SolidBrush sb = new SolidBrush(Darken(e.Color, 30)))
                g.DrawString(e.Kind, f, sb, new Rectangle(icon.X + 3, icon.Y + 33, icon.Width - 6, 16), Center());
            // small eyes
            using (SolidBrush eye = new SolidBrush(Color.Red))
            {
                g.FillEllipse(eye, icon.X + 12, icon.Y + 9, 5, 5);
                g.FillEllipse(eye, icon.Right - 17, icon.Y + 9, 5, 5);
            }
        }

        private static void DrawBoss(Graphics g, Rectangle r, GameEntity e)
        {
            using (SolidBrush aura = new SolidBrush(Color.FromArgb(55, e.Color))) g.FillEllipse(aura, r.X - 20, r.Y - 20, r.Width + 40, r.Height + 40);
            // [추가] Driver-K 이미지 스프라이트 렌더링 로직
            if (e.Name.Contains("Driver-K"))
            {
                if (Renderer.ImgBoss_DriverK != null)
                {
                    // 1. 스프라이트 잘라내기 설정 (가로 4칸 기준)
                    int cols = 4;
                    int fW = Renderer.ImgBoss_DriverK.Width / cols;
                    int fH = Renderer.ImgBoss_DriverK.Height;

                    // 2. 상태에 따른 프레임 선택
                    // IsCastingPattern이 true면 4번째 사진(인덱스 3), 아니면 방향에 따라 0~1번
                    int fX = e.IsCastingPattern ? 3 : (e.Facing == -1 ? 1 : 0);

                    Rectangle srcRect = new Rectangle(fX * fW, 0, fW, fH);
                    Rectangle destRect = new Rectangle(r.X + r.Width / 2 - 170, r.Bottom - 300, 340, 340);

                    g.DrawImage(Renderer.ImgBoss_DriverK, destRect, srcRect, GraphicsUnit.Pixel);
                }
                else
                {
                    // 이미지 없을 시 대체 도형 (기존 로직 유지)
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(70, 76, 88))) g.FillRectangle(b, r.X + 40, r.Y + 50, r.Width - 80, r.Height - 55);
                    // ... 기존 드라이버 K 도형 로직 ...
                }
                return;
            }
            else if (e.Name.Contains("Kernel"))
            {
                // 👇 [수정] 우리가 만든 스프라이트 이미지가 있으면 이미지를 그리고, 없으면 예전 도형을 그립니다.
                if (ImgBoss_HighKernel != null)
                {
                    int cols = 4;
                    int rows = 1;
                    int fW = ImgBoss_HighKernel.Width / cols;
                    int fH = ImgBoss_HighKernel.Height / rows;

                    int fX = 0;

                    // 기믹 시전 중이면 4번 사진(index 3)으로 고정
                    if (e.IsCastingPattern)
                    {
                        fX = 3;
                    }
                    else
                    {
                        // 대기 상태: 플레이어 방향에 따라 1번(0)과 2번(1) 스위칭
                        if (e.Facing == -1) fX = 1;
                        else fX = 0;
                    }

                    Rectangle srcRect = new Rectangle(fX * fW, 0, fW, fH);

                    // 보스가 바닥(r.Bottom)을 딛고 서 있도록 거대하게(340x340) 좌표 보정
                    int centerX = r.X + r.Width / 2;
                    Rectangle destRect = new Rectangle(centerX - 170, r.Bottom - 300, 340, 340);

                    g.DrawImage(ImgBoss_HighKernel, destRect, srcRect, GraphicsUnit.Pixel);
                }
                else
                {
                    // (이미지를 못 찾았을 때 나오는 기존 임시 도형 로직)
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(180, 184, 190))) g.FillRectangle(b, r.X + 55, r.Y + 35, r.Width - 110, r.Height - 45);
                    using (Pen p = new Pen(Color.White, 3f))
                    {
                        g.DrawLine(p, r.X + r.Width / 2, r.Y + 42, r.X + r.Width / 2, r.Bottom - 25);
                        g.DrawLine(p, r.X + 70, r.Y + 78, r.Right - 70, r.Y + 78);
                    }
                    using (Font f = F(12f, FontStyle.Bold)) g.DrawString("SYSTEM32", f, Brushes.DarkRed, new Rectangle(r.X, r.Y + 18, r.Width, 24), Center());
                }
            }
            else if (e.Name.Contains("BSOD"))
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(16, 62, 190))) g.FillRectangle(b, r.X + 20, r.Y + 20, r.Width - 40, r.Height - 30);
                using (Font f = F(14f, FontStyle.Bold)) g.DrawString("BSOD", f, Brushes.White, new Rectangle(r.X, r.Y + 34, r.Width, 26), Center());
                using (Font f = F(8f, FontStyle.Bold))
                {
                    g.DrawString("STOP: 0x0000007E", f, Brushes.White, new Rectangle(r.X + 30, r.Y + 72, r.Width - 60, 18), Center());
                    g.DrawString("CRASH DUMP", f, Brushes.White, new Rectangle(r.X + 30, r.Y + 96, r.Width - 60, 18), Center());
                }
            }
            else if (e.Name.Contains("Exception"))
            {
                for (int i = 0; i < 5; i++)
                {
                    Rectangle win = new Rectangle(r.X + 25 + i * 18, r.Y + 18 + i * 14, r.Width - 70, 54);
                    DrawXPWindow(g, win, "Exception", true);
                }
                using (Font f = F(10f, FontStyle.Bold)) g.DrawString("Exception Queen", f, Brushes.DarkRed, new Rectangle(r.X, r.Bottom - 45, r.Width, 24), Center());
            }
            else if (e.Name.Contains("Binny"))
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(210, 225, 218))) g.FillRectangle(b, r.X + 45, r.Y + 35, r.Width - 90, r.Height - 35);
                using (Pen p = new Pen(Color.FromArgb(80, 150, 88), 6f)) g.DrawArc(p, r.X + 65, r.Y + 55, r.Width - 130, r.Height - 80, 30, 260);
                using (Font f = F(12f, FontStyle.Bold)) g.DrawString("Illegal_Binny.dat", f, Brushes.DarkGreen, new Rectangle(r.X, r.Y + 18, r.Width, 26), Center());
            }
            else
            {
                using (SolidBrush b = new SolidBrush(e.Color)) g.FillEllipse(b, r);
            }
            using (Pen p = new Pen(Color.FromArgb(245, 255, 255), 3f)) g.DrawRectangle(p, r.X + 12, r.Y + 12, r.Width - 24, r.Height - 24);
        }

        public static void DrawStageBackground(Graphics g, Rectangle client, StageInfo st, float cameraX)
        {
            DrawStageBackground(g, client, st, cameraX, false);
        }

        public static void DrawStageBackground(Graphics g, Rectangle client, StageInfo st, float cameraX, bool bossRoom)
        {
            DrawStageBackground(g, client, st, cameraX, bossRoom, client.Width);
        }

        public static void DrawStageBackground(Graphics g, Rectangle client, StageInfo st, float cameraX, bool bossRoom, int mapWidth)
        {
            if (st == null || client.Width <= 0 || client.Height <= 0) return;
            int virtualWidth = Math.Max(client.Width, mapWidth);
            string key = st.Index.ToString() + "_" + (bossRoom ? "boss_" : "normal_") + virtualWidth.ToString() + "x" + client.Height.ToString();
            Bitmap cached;
            if (!stageBackgroundCache.TryGetValue(key, out cached))
            {
                cached = new Bitmap(virtualWidth, client.Height);
                using (Graphics cg = Graphics.FromImage(cached))
                {
                    cg.SmoothingMode = SmoothingMode.HighSpeed;
                    cg.CompositingQuality = CompositingQuality.HighSpeed;
                    cg.InterpolationMode = InterpolationMode.Low;
                    cg.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                    DrawStageBackgroundCore(cg, new Rectangle(0, 0, virtualWidth, client.Height), st, 0, bossRoom);
                }
                stageBackgroundCache[key] = cached;
            }
            int scrollX = (int)Math.Max(0, Math.Min(cameraX, virtualWidth - client.Width));
            Rectangle source = new Rectangle(scrollX, 0, client.Width, client.Height);
            g.DrawImage(cached, client, source, GraphicsUnit.Pixel);
        }

        private static void DrawStageBackgroundCore(Graphics g, Rectangle client, StageInfo st, float cameraX, bool bossRoom)
        {
            Image stageImage = LoadStageBackgroundImage(st.Index, bossRoom);
            if (stageImage != null)
            {
                DrawImageCover(g, stageImage, client);
                using (LinearGradientBrush shade = new LinearGradientBrush(client, Color.FromArgb(18, 0, 0, 0), Color.FromArgb(42, 0, 0, 0), 90f))
                    g.FillRectangle(shade, client);
                using (SolidBrush topGlow = new SolidBrush(Color.FromArgb(30, st.Accent)))
                    g.FillEllipse(topGlow, client.Width / 2 - 360, 20, 720, 180);
                DrawXPTaskbar(g, client, bossRoom ? st.Name + " - 보스방" : st.Name);
                return;
            }
            if (st.Index == 1)
            {
                DrawXPWallpaper(g, client);
                DrawXPTaskbar(g, client, st.Name);
                DrawDesktopIcon(g, "내 문서", 24, 36, Color.FromArgb(248, 215, 64));
                DrawDesktopIcon(g, "내 컴퓨터", 24, 126, Color.FromArgb(110, 190, 245));
                DrawDesktopIcon(g, "휴지통", 24, 216, Color.FromArgb(210, 230, 220));
                DrawDesktopIcon(g, "새 폴더", client.Width - 140, 70, Color.FromArgb(248, 215, 64));
                return;
            }
            using (LinearGradientBrush b = new LinearGradientBrush(client, Darken(st.BackColor, 22), Lighten(st.BackColor, 34), 90f)) g.FillRectangle(b, client);
            using (SolidBrush glow = new SolidBrush(Color.FromArgb(48, st.Accent)))
            {
                g.FillEllipse(glow, client.Width / 2 - 400, 60, 800, 240);
                g.FillEllipse(glow, client.Width - 360, client.Height - 300, 520, 280);
            }
            Rectangle window = new Rectangle(80, 70, client.Width - 160, client.Height - 150);
            string title = st.Name;
            if (st.Index == 2) title = "Driver Vault - Device Manager";
            else if (st.Index == 3) title = "Windows Update Lab";
            else if (st.Index == 4) title = "C:\\WINDOWS\\system32";
            else if (st.Index == 5) title = "Network Connections - Port Harbor";
            else if (st.Index == 6) title = "System Stability Check";
            else if (st.Index == 7) title = "Registry Editor - Hive Archive";
            else if (st.Index == 8) title = "Exception Report - Popup Error Maze";
            else if (st.Index == 9) title = "Temp Cache - Local Settings\\Temp";
            else if (st.Index == 10) title = "Recycle Bin Dungeon";
            DrawXPWindow(g, window, title, st.Index == 8 || st.Index == 10);
            Rectangle content = new Rectangle(window.X + 16, window.Y + 48, window.Width - 32, window.Height - 58);
            if (st.Index == 2) DrawDeviceManagerField(g, content);
            else if (st.Index == 3) DrawUpdateField(g, content);
            else if (st.Index == 4) DrawSystem32Field(g, content);
            else if (st.Index == 5) DrawNetworkField(g, content);
            else if (st.Index == 6) DrawBsodField(g, content);
            else if (st.Index == 7) DrawRegistryField(g, content);
            else if (st.Index == 8) DrawPopupField(g, content);
            else if (st.Index == 9) DrawTempField(g, content);
            else if (st.Index == 10) DrawRecycleField(g, content);
            DrawXPTaskbar(g, client, st.Name);
        }

        private static void DrawDesktopIcon(Graphics g, string label, int x, int y, Color c)
        {
            Rectangle icon = new Rectangle(x + 18, y, 48, 48);
            using (LinearGradientBrush b = new LinearGradientBrush(icon, Color.White, c, 90f)) g.FillRectangle(b, icon);
            using (Pen p = new Pen(Color.FromArgb(40, 80, 160), 2f)) g.DrawRectangle(p, icon.X, icon.Y, icon.Width - 1, icon.Height - 1);
            using (Font f = F(8f, FontStyle.Bold))
            using (SolidBrush sb = new SolidBrush(Color.White))
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(130, 0, 0, 0)))
            {
                Rectangle lab = new Rectangle(x, y + 52, 86, 32);
                g.FillRectangle(bg, lab);
                g.DrawString(label, f, sb, lab, Center());
            }
        }

        private static void DrawDeviceManagerField(Graphics g, Rectangle r)
        {
            using (SolidBrush b = new SolidBrush(Color.FromArgb(245, 247, 250))) g.FillRectangle(b, r);
            using (Font f = F(9f, FontStyle.Regular))
            using (SolidBrush sb = new SolidBrush(Color.FromArgb(30, 40, 60)))
            {
                string[] lines = { "+ Display Adapter", "+ Sound Driver", "+ USB Controller     ⚠", "+ Network Adapter", "+ Unknown Device     ⚠", "+ Legacy Device      ⚠" };
                for (int i = 0; i < lines.Length; i++) g.DrawString(lines[i], f, sb, r.X + 24, r.Y + 18 + i * 28);
            }
        }

        private static void DrawUpdateField(Graphics g, Rectangle r)
        {
            using (SolidBrush b = new SolidBrush(Color.FromArgb(235, 242, 255))) g.FillRectangle(b, r);
            Rectangle pr = new Rectangle(r.X + 90, r.Y + 70, r.Width - 180, 28);
            DrawBar(g, pr, 67, 100, Color.FromArgb(50, 135, 245));
            using (Font f = F(20f, FontStyle.Bold)) g.DrawString("Windows Update", f, Brushes.Navy, new Rectangle(r.X, r.Y + 20, r.Width, 40), Center());
            using (Font f = F(10f, FontStyle.Bold)) g.DrawString("Installing updates... 67%", f, Brushes.DarkBlue, new Rectangle(r.X, r.Y + 112, r.Width, 22), Center());
        }

        private static void DrawSystem32Field(Graphics g, Rectangle r)
        {
            using (SolidBrush b = new SolidBrush(Color.FromArgb(245, 245, 248))) g.FillRectangle(b, r);
            using (Font f = F(10f, FontStyle.Regular))
            using (SolidBrush sb = new SolidBrush(Color.FromArgb(60, 50, 55)))
            {
                string[] files = { "kernel32.dll", "ntoskrnl.exe", "drivers", "config", "protected.dat", "system.map" };
                for (int i = 0; i < files.Length; i++) g.DrawString(files[i], f, sb, r.X + 30 + (i % 2) * 220, r.Y + 30 + (i / 2) * 48);
            }
            using (Font f = F(16f, FontStyle.Bold)) g.DrawString("PROTECTED AREA", f, Brushes.DarkRed, new Rectangle(r.X, r.Bottom - 70, r.Width, 40), Center());
        }

        private static void DrawNetworkField(Graphics g, Rectangle r)
        {
            using (SolidBrush b = new SolidBrush(Color.FromArgb(220, 244, 250))) g.FillRectangle(b, r);
            using (Pen p = new Pen(Color.FromArgb(40, 140, 200), 4f))
            {
                for (int i = 0; i < 7; i++) g.DrawBezier(p, r.X + 40, r.Y + 40 + i * 36, r.X + 250, r.Y + 15 + i * 20, r.Right - 260, r.Y + 80 + i * 22, r.Right - 40, r.Y + 40 + i * 36);
            }
            using (Font f = F(11f, FontStyle.Bold)) g.DrawString("PORT 80  PORT 443  PORT 404", f, Brushes.DarkBlue, new Rectangle(r.X, r.Y + 20, r.Width, 24), Center());
        }

        private static void DrawBsodField(Graphics g, Rectangle r)
        {
            using (SolidBrush b = new SolidBrush(Color.FromArgb(0, 40, 180))) g.FillRectangle(b, r);
            using (Font f = F(16f, FontStyle.Bold)) g.DrawString("A problem has been detected and Windows has been shut down to prevent damage", f, Brushes.White, new Rectangle(r.X + 30, r.Y + 28, r.Width - 60, 44), Center());
            using (Font f = F(12f, FontStyle.Bold))
            {
                for (int i = 0; i < 8; i++) g.DrawString("STOP: 0x000000" + (7 + i).ToString("X"), f, Brushes.White, r.X + 70, r.Y + 100 + i * 32);
            }
        }

        private static void DrawRegistryField(Graphics g, Rectangle r)
        {
            using (SolidBrush b = new SolidBrush(Color.FromArgb(248, 247, 250))) g.FillRectangle(b, r);
            using (Font f = F(9f, FontStyle.Regular))
            using (SolidBrush sb = new SolidBrush(Color.FromArgb(45, 35, 70)))
            {
                string[] keys = { "HKEY_CLASSES_ROOT", "HKEY_CURRENT_USER", "HKEY_LOCAL_MACHINE", "RecentActions", "ProfileName", "LastSession" };
                for (int i = 0; i < keys.Length; i++) g.DrawString((i < 3 ? "+ " : "   └ ") + keys[i], f, sb, r.X + 28, r.Y + 20 + i * 32);
            }
        }

        private static void DrawPopupField(Graphics g, Rectangle r)
        {
            using (SolidBrush b = new SolidBrush(Color.FromArgb(238, 238, 246))) g.FillRectangle(b, r);
            for (int i = 0; i < 8; i++)
            {
                Rectangle pop = new Rectangle(r.X + 40 + i * 38, r.Y + 38 + (i % 3) * 48, 220, 90);
                DrawXPWindow(g, pop, "Error", true);
                using (Font f = F(8f, FontStyle.Bold)) g.DrawString("Unhandled exception", f, Brushes.DarkRed, new Rectangle(pop.X + 16, pop.Y + 40, pop.Width - 32, 20), Center());
            }
        }

        private static void DrawTempField(Graphics g, Rectangle r)
        {
            using (SolidBrush b = new SolidBrush(Color.FromArgb(238, 232, 210))) g.FillRectangle(b, r);
            using (Font f = F(10f, FontStyle.Bold))
            {
                string[] names = { "TEMP_01.tmp", "UNSENT_REPORT.tmp", "recent.cache", "profile.cache", "cache_heap.bin", "thumbnail.db" };
                for (int i = 0; i < names.Length; i++) g.DrawString(names[i], f, Brushes.SaddleBrown, r.X + 32 + (i % 3) * 220, r.Y + 35 + (i / 3) * 90);
            }
        }

        private static void DrawRecycleField(Graphics g, Rectangle r)
        {
            using (SolidBrush b = new SolidBrush(Color.FromArgb(220, 232, 220))) g.FillRectangle(b, r);
            using (Font f = F(10f, FontStyle.Bold))
            {
                string[] names = { "Driver-K.log", "High-Kernel.protect", "BSOD_dump.tmp", "ExceptionQueen.err", "UNSENT_REPORT.tmp", "Illegal_Binny.dat" };
                for (int i = 0; i < names.Length; i++) g.DrawString(names[i], f, Brushes.DarkGreen, r.X + 32 + (i % 2) * 320, r.Y + 35 + (i / 2) * 60);
            }
        }

        private static void DrawSwordSlash(Graphics g, float x1, float y1, float x2, float y2, Color color, int alpha, float progress, string text)
        {
            int dir = x2 >= x1 ? 1 : -1;
            float handX = x1 + dir * 40f;
            float handY = y1 - 62f;
            float reach = Math.Min(138f, Math.Max(92f, Math.Abs(x2 - x1) * 0.72f));
            float centerX = handX + dir * 48f;
            float centerY = handY + 16f;
            RectangleF arcBounds = new RectangleF(centerX - reach * 0.55f, centerY - reach * 0.48f, reach * 1.1f, reach * 0.96f);

            float sweep = dir > 0 ? 148f : -148f;
            float start = dir > 0 ? -112f + progress * 28f : 112f - progress * 28f;
            Color slashOuter = Color.FromArgb(alpha, 245, 250, 255);
            Color slashInner = Color.FromArgb(ClampAlpha(alpha + 20), Color.FromArgb(255, 224, 92));
            using (Pen glow = new Pen(Color.FromArgb(ClampAlpha(alpha / 3), color), 18f))
            using (Pen trail = new Pen(Color.FromArgb(ClampAlpha(alpha - 35), slashOuter), 8f))
            using (Pen core = new Pen(slashInner, 3f))
            {
                RoundCaps(glow);
                RoundCaps(trail);
                RoundCaps(core);
                g.DrawArc(glow, arcBounds, start, sweep);
                g.DrawArc(trail, arcBounds, start + dir * 8f, sweep * 0.82f);
                g.DrawArc(core, arcBounds, start + dir * 18f, sweep * 0.54f);
            }

            double bladeAngle = (-68.0 + progress * 136.0) * Math.PI / 180.0;
            float tipX = handX + dir * (float)Math.Cos(bladeAngle) * 82f;
            float tipY = handY + (float)Math.Sin(bladeAngle) * 82f;
            float guardX = handX - dir * 8f;
            float guardY = handY + 8f;
            using (Pen bladeShadow = new Pen(Color.FromArgb(ClampAlpha(alpha - 75), 25, 35, 48), 7f))
            using (Pen blade = new Pen(Color.FromArgb(alpha, 235, 242, 252), 4f))
            using (Pen edge = new Pen(Color.FromArgb(ClampAlpha(alpha + 10), Color.White), 1.5f))
            using (Pen hilt = new Pen(Color.FromArgb(alpha, 88, 56, 28), 5f))
            {
                RoundCaps(bladeShadow);
                RoundCaps(blade);
                RoundCaps(edge);
                RoundCaps(hilt);
                g.DrawLine(bladeShadow, guardX, guardY, tipX, tipY);
                g.DrawLine(blade, guardX, guardY, tipX, tipY);
                g.DrawLine(edge, guardX + dir * 2f, guardY - 2f, tipX, tipY);
                g.DrawLine(hilt, handX - dir * 15f, handY + 18f, handX + dir * 10f, handY + 3f);
            }

            if (!string.IsNullOrEmpty(text))
            {
                using (Font f = F(10f, FontStyle.Bold))
                using (SolidBrush shadow = new SolidBrush(Color.FromArgb(ClampAlpha(alpha - 55), 0, 0, 0)))
                using (SolidBrush b = new SolidBrush(Color.FromArgb(alpha, slashInner)))
                {
                    RectangleF rr = new RectangleF(centerX + dir * 18f - 45f, centerY - 82f, 90f, 26f);
                    g.DrawString(text, f, shadow, new RectangleF(rr.X + 1, rr.Y + 1, rr.Width, rr.Height), Center());
                    g.DrawString(text, f, b, rr, Center());
                }
            }
        }

        public static void DrawEffect(Graphics g, Effect e, float cameraX)
        {
            int alpha = ClampAlpha((int)(235 * e.Ticks / (float)Math.Max(1, e.MaxTicks)));
            float progress = 1f - e.Ticks / (float)Math.Max(1, e.MaxTicks);
            float x1 = e.X - cameraX;
            float x2 = e.X2 - cameraX;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (e.Kind == "playerSlash")
            {
                DrawSwordSlash(g, x1, e.Y, x2, e.Y2, e.Color, alpha, progress, e.Text);
                return;
            }

            if (e.Kind == "playerClean" || e.Kind == "playerScan" || e.Kind == "playerDelete")
            {
                int dir = e.X2 >= e.X ? 1 : -1;
                bool deleteBeam = e.Kind == "playerDelete";

                // 캐릭터는 DrawRecoveryProgram에서 한 번만 그립니다.
                // 여기서 캐릭터 공격 자세를 또 그리면 화면에 분신처럼 보이므로, 손 위치에서 이펙트만 분사합니다.
                Color beamColor = deleteBeam ? Color.FromArgb(255, 70, 60) : e.Kind == "playerClean" ? Color.FromArgb(110, 255, 95) : Color.FromArgb(85, 230, 255);
                float handX = x1 + dir * 42;
                float handY = e.Y - 64;
                DrawShimmerBeam(g, handX, handY, x2, e.Y2 - 42, beamColor, alpha, progress, deleteBeam);

                if (!string.IsNullOrEmpty(e.Text))
                {
                    using (Font f = F(10f, FontStyle.Bold))
                    using (SolidBrush shadow = new SolidBrush(Color.FromArgb(ClampAlpha(alpha - 40), 0, 0, 0)))
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(alpha, beamColor)))
                    {
                        RectangleF rr = new RectangleF(handX + dir * 30 - 42, handY - 44, 84, 26);
                        g.DrawString(e.Text, f, shadow, new RectangleF(rr.X + 1, rr.Y + 1, rr.Width, rr.Height), Center());
                        g.DrawString(e.Text, f, b, rr, Center());
                    }
                }
                return;
            }

            if (e.Kind == "projectile")
            {
                float cx = x1 + (x2 - x1) * progress;
                float cy = e.Y + (e.Y2 - e.Y) * progress;
                bool deleteBeam = e.Color.R > 180 && e.Color.G < 160;
                DrawShimmerBeam(g, x1, e.Y, cx, cy, e.Color, alpha, progress, deleteBeam);
                using (SolidBrush core = new SolidBrush(Color.FromArgb(alpha, Color.White)))
                    g.FillEllipse(core, cx - 5, cy - 5, 10, 10);
                return;
            }
            else if (e.Kind == "text")
            {
                using (Font f = F(13f, FontStyle.Bold))
                using (SolidBrush shadow = new SolidBrush(Color.FromArgb(ClampAlpha(alpha - 80), 0, 0, 0)))
                using (SolidBrush b = new SolidBrush(Color.FromArgb(alpha, e.Color)))
                {
                    RectangleF rr = new RectangleF(x1 - 70, e.Y - progress * 34, 140, 28);
                    g.DrawString(e.Text, f, shadow, new RectangleF(rr.X + 1, rr.Y + 1, rr.Width, rr.Height), Center());
                    g.DrawString(e.Text, f, b, rr, Center());
                }
            }
            else if (e.Kind == "spark")
            {
                int cx = (int)x1;
                int cy = (int)e.Y;
                using (Pen p = new Pen(Color.FromArgb(alpha, e.Color), 3f))
                using (Pen core = new Pen(Color.FromArgb(ClampAlpha(alpha - 30), Color.White), 1.3f))
                {
                    for (int i = 0; i < 12; i++)
                    {
                        double a = i * Math.PI * 2 / 12 + progress * 2.4;
                        int inner = (int)(6 + progress * 12);
                        int outer = (int)(22 + progress * 38);
                        int xA = cx + (int)(Math.Cos(a) * inner);
                        int yA = cy + (int)(Math.Sin(a) * inner);
                        int xB = cx + (int)(Math.Cos(a) * outer);
                        int yB = cy + (int)(Math.Sin(a) * outer);
                        g.DrawLine(p, xA, yA, xB, yB);
                        if (i % 3 == 0) g.DrawLine(core, cx, cy, xB, yB);
                    }
                }
            }
        }
    }

    internal static class GraphicsRoundedExtensions
    {
        private static GraphicsPath RoundedPath(RectangleF bounds, float radius)
        {
            float r = Math.Max(1f, Math.Min(radius, Math.Min(bounds.Width, bounds.Height) / 2f));
            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, r * 2f, r * 2f, 180, 90);
            path.AddArc(bounds.Right - r * 2f, bounds.Y, r * 2f, r * 2f, 270, 90);
            path.AddArc(bounds.Right - r * 2f, bounds.Bottom - r * 2f, r * 2f, r * 2f, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - r * 2f, r * 2f, r * 2f, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void FillRoundedRectangle(this Graphics g, Brush brush, RectangleF bounds, float radius)
        {
            using (GraphicsPath path = RoundedPath(bounds, radius)) g.FillPath(brush, path);
        }

        public static void DrawRoundedRectangle(this Graphics g, Pen pen, RectangleF bounds, float radius)
        {
            using (GraphicsPath path = RoundedPath(bounds, radius)) g.DrawPath(pen, path);
        }
    }

}
