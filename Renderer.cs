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
        private static Image npcEmotionSheetCache = null;
        private static Image xpBlissBackgroundCache = null;
        private static Bitmap xpBlissScaledCache = null;
        private static Size xpBlissScaledSize = Size.Empty;
        private static readonly Dictionary<string, Bitmap> stageBackgroundCache = new Dictionary<string, Bitmap>();
        private static readonly Dictionary<string, Image> stageImageCache = new Dictionary<string, Image>();
        private static Image playerAgentSheet = null;
        private static Image playerAgentIdleFrame = null;
        private static readonly Image[] playerAgentWalkFrames = new Image[8];
        private static System.Drawing.Text.PrivateFontCollection fontCollection;
        private static FontFamily customFontFamily;
        public static Image Img_AlarmBg = null;
        public static Image Img_PopupBtn = null;

        // 보스 이미지
        public static Image ImgBoss_DriverK;
        public static Image ImgBoss_BSOD;
        public static Image ImgBoss_HighKernel;
        public static Image ImgBoss_ExceptionQueen;
        public static Image ImgBoss_IllegalBinny;
        private static readonly Dictionary<string, Image> playerSpriteSheets = new Dictionary<string, Image>();
        private static readonly Dictionary<string, Image> playerActionSheetCache = new Dictionary<string, Image>();
        private static readonly Dictionary<string, Image> playerMotionSheetCache = new Dictionary<string, Image>();
        private static readonly Dictionary<string, Rectangle> playerActionTrimCache = new Dictionary<string, Rectangle>();
        private static Image playerStillSwordImage;
        private static Image playerShieldImage;
        private static Image normalMonsterSheet = null;
        private static readonly Dictionary<string, Image> strictMonsterCache = new Dictionary<string, Image>();
        private static readonly object strictMonsterLock = new object();

        // 이펙트 자산
        public static Image Img_DiskSprite;
        public static Image Img_Meteor;
        public static Image Img_Meteor2;
        public static Image Img_Safezone;
        public static Image Img_IceSword;
        public static Image Img_FireSword;
        public static Image Img_LightningSword;
        public static Image Img_SwordDark = null;
        public static Image Img_SwordCold = null;

        public static Image Img_SkillBarrier = null;
        // 🌟 [추가 주입]: 적 투사체 전용 고해상도 그래픽 자산 홀더
        public static Image Img_EnergyBall = null;
        public static Image Img_ProjectileTeleport = null;


        static Renderer()
        {
            try
            {
                fontCollection = new System.Drawing.Text.PrivateFontCollection();
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;

                string alarmPath = Path.Combine(baseDir, "Assets", "UI", "SystemAlarmBlueCancel.png");
                if (File.Exists(alarmPath)) Img_AlarmBg = Image.FromFile(alarmPath);

                string btnPath = Path.Combine(baseDir, "Assets", "UI", "button.png");
                if (File.Exists(btnPath)) Img_PopupBtn = Image.FromFile(btnPath);

                string fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "custom_font.ttf");

                string darkPath = Path.Combine(baseDir, "Assets", "UI", "w_longsword_dark.png");
                if (File.Exists(darkPath)) Img_SwordDark = Image.FromFile(darkPath);

                string coldPath = Path.Combine(baseDir, "Assets", "UI", "w_longsword_cold.png");
                if (File.Exists(coldPath)) Img_SwordCold = Image.FromFile(coldPath);

                string barrierPath = Path.Combine(baseDir, "Assets", "UI", "skill_barrier.png");
                if (File.Exists(barrierPath)) Img_SkillBarrier = Image.FromFile(barrierPath);

                // 💡 [자산 매핑]: Assets\UI\ 폴더 내부에 저장될 투사체 파일들을 정밀 인젝션합니다.
                string energyBallPath = Path.Combine(baseDir, "Assets", "UI", "energy_ball.png");
                if (File.Exists(energyBallPath)) Img_EnergyBall = Image.FromFile(energyBallPath);

                string projTeleportPath = Path.Combine(baseDir, "Assets", "UI", "projectile_teleport.png");
                if (File.Exists(projTeleportPath)) Img_ProjectileTeleport = Image.FromFile(projTeleportPath);

                if (File.Exists(fontPath))
                {
                    fontCollection.AddFontFile(fontPath);
                    customFontFamily = fontCollection.Families[0];
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("폰트 로드 실패: " + ex.Message);
            }
        }

        public static Font F(float size, FontStyle style)
        {
            if (customFontFamily != null) return new Font(customFontFamily, size, style);
            return new Font("Malgun Gothic", size, style);
        }

        public static StringFormat Center()
        {
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            sf.Trimming = StringTrimming.None;
            sf.FormatFlags |= StringFormatFlags.NoClip;
            return sf;
        }

        public static StringFormat Left()
        {
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Near;
            sf.LineAlignment = StringAlignment.Near;
            sf.Trimming = StringTrimming.None;
            sf.FormatFlags |= StringFormatFlags.NoClip;
            return sf;
        }

        public static StringFormat LeftMiddle()
        {
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Near;
            sf.LineAlignment = StringAlignment.Center;
            sf.Trimming = StringTrimming.None;
            sf.FormatFlags |= StringFormatFlags.NoClip;
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
            DesktopBackgroundUI.Shared.Draw(g, r);
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

        // 🌟 [전투 필드 한정 탑다운 드로우 라우터]
        public static void DrawEnemy(Graphics g, GameEntity e, float cameraX)
        {
            if (e == null) return;
            int screenX = (int)(e.X - cameraX);
            int screenY = (int)e.Y;

            using (SolidBrush sh = new SolidBrush(Color.FromArgb(75, 0, 0, 0)))
                g.FillEllipse(sh, screenX - 24, screenY + 16, 48, 16);

            Rectangle r = new Rectangle(screenX - 64, screenY - 64, 128, 128);

            if (e.IsBoss || e.Kind == "BOSS")
            {
                DrawBoss(g, r, e);
                return;
            }
            else
            {
                DrawFileMonster(g, r, e);
            }

            int uiTopY = screenY - 55;
            Rectangle hp = new Rectangle(screenX - 32, uiTopY, 64, 7);
            DrawBar(g, hp, e.Hp, e.MaxHp, Color.OrangeRed);

            using (Font f = F(7f, FontStyle.Bold))
            using (SolidBrush sb = new SolidBrush(Color.White))
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(140, 0, 0, 0)))
            {
                Rectangle nameRect = new Rectangle(screenX - 50, uiTopY - 18, 100, 15);
                g.FillRectangle(bg, nameRect);
                g.DrawString(e.Name ?? "UNKNOWN", f, sb, nameRect, Center());
            }
        }

        public static void DrawEnemy(Graphics g, GameEntity e, float cameraX, int clientHeight)
        {
            DrawEnemy(g, e, cameraX); // 인게임 전용 통합 브릿지 연결
        }

        private static Image LoadStrictMonsterAssetDirect(GameEntity e)
        {
            if (e == null || string.IsNullOrEmpty(e.Kind)) return null;
            string targetPngName = e.Kind.Trim();

            lock (strictMonsterLock)
            {
                if (strictMonsterCache.TryGetValue(targetPngName, out Image cachedImg) && cachedImg != null)
                    return cachedImg;

                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string[] folderNames = { "Assets/Characters/Enemies", "Assets/Characters/Enemise", "Assets/Monsters" };
                string fullPath = "";

                foreach (var folder in folderNames)
                {
                    string checkPath = Path.Combine(baseDir, folder.Replace('/', Path.DirectorySeparatorChar), targetPngName);
                    if (File.Exists(checkPath)) { fullPath = checkPath; break; }
                }
                try
                {
                    if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
                    {
                        Image img = Image.FromFile(fullPath);
                        strictMonsterCache[targetPngName] = img;
                        return img;
                    }
                }
                catch { }
                return null;
            }
        }

        private static void DrawFileMonster(Graphics g, Rectangle r, GameEntity e)
        {
            Image sheet = LoadStrictMonsterAssetDirect(e);
            if (sheet == null)
            {
                DrawFallbackFileMonster(g, r, e);
                return;
            }

            if (e.Kind != null && e.Kind.Contains("Teleport_2"))
            {
                const int fixedCellW = 341;
                const int fixedCellH = 341;
                const int cols = 3;
                int targetFrameIndex = 0;

                if (e.StateTimer >= 115) targetFrameIndex = 5;
                else if (e.StateTimer >= 0 && e.StateTimer < 10) targetFrameIndex = 8;
                else targetFrameIndex = (Environment.TickCount / 150) % 5;

                int col = targetFrameIndex % cols;
                int row = targetFrameIndex / cols;

                Rectangle srcRect = new Rectangle(col * fixedCellW + 3, row * fixedCellH + 3, fixedCellW - 6, fixedCellH - 6);

                int displaySize = 110;
                Rectangle perfectGridDst = new Rectangle(
                    r.X + (r.Width / 2) - (displaySize / 2),
                    r.Y + (r.Height / 2) - (displaySize / 2),
                    displaySize,
                    displaySize
                );

                var oldInterpolation = g.InterpolationMode;
                var oldPixelOffset = g.PixelOffsetMode;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;

                g.DrawImage(sheet, perfectGridDst, srcRect, GraphicsUnit.Pixel);

                g.InterpolationMode = oldInterpolation;
                g.PixelOffsetMode = oldPixelOffset;
                return;
            }

            int drawSize = 110;
            Rectangle dst = new Rectangle(
                r.X + (r.Width / 2) - (drawSize / 2),
                r.Y + (r.Height / 2) - (drawSize / 2),
                drawSize,
                drawSize
            );

            var prevInterpolation = g.InterpolationMode;
            var prevPixelOffset = g.PixelOffsetMode;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            if (e.Kind == "Dash_1.png") Draw_Image_Interpretation_1(g, sheet, dst, e);
            else if (e.Kind == "Dash_2.png") Draw_Image_Interpretation_2(g, sheet, dst, e);
            else if (e.Kind == "Spread_1.png" || e.Kind == "Spread_2.png") Draw_Image_Interpretation_3(g, sheet, dst, e);
            else if (e.Kind == "Teleport_1.png") Draw_Image_Interpretation_4(g, sheet, dst, e);
            else
            {
                Rectangle src = new Rectangle(0, 0, sheet.Width, sheet.Height);
                g.DrawImage(sheet, dst, src, GraphicsUnit.Pixel);
            }

            g.InterpolationMode = prevInterpolation;
            g.PixelOffsetMode = prevPixelOffset;

            if (e.HitFlash > 0)
            {
                using (SolidBrush flash = new SolidBrush(Color.FromArgb(90, Color.White)))
                    g.FillEllipse(flash, dst.X + 8, dst.Y + 8, dst.Width - 16, dst.Height - 16);
            }
        }

        private static void Draw_Image_Interpretation_1(Graphics g, Image sheet, Rectangle dst, GameEntity e)
        {
            int cols = 4; int rows = 4;
            int cellW = sheet.Width / cols; int cellH = sheet.Height / rows;
            int targetFrameIndex = 15;
            if (e.MonsterState == 1) targetFrameIndex = (Environment.TickCount / 90) % 15;
            int col = targetFrameIndex % cols; int row = targetFrameIndex / cols; int pad = 2;
            Rectangle src = new Rectangle(col * cellW + pad, row * cellH + pad, Math.Max(1, cellW - pad * 2), Math.Max(1, cellH - pad * 2));
            g.DrawImage(sheet, dst, src, GraphicsUnit.Pixel);
        }

        private static void Draw_Image_Interpretation_2(Graphics g, Image sheet, Rectangle dst, GameEntity e)
        {
            int cols = 4; int rows = 2;
            int cellW = sheet.Width / cols; int cellH = sheet.Height / rows;
            int targetFrameIndex = 2;
            if (e.MonsterState == 1)
            {
                int[] dashSequence = { 0, 1, 3, 4, 5, 6, 7 };
                int currentFramePointer = e.StateTimer / 5;
                targetFrameIndex = currentFramePointer >= dashSequence.Length ? dashSequence[dashSequence.Length - 1] : dashSequence[currentFramePointer];
            }
            int col = targetFrameIndex % cols; int row = targetFrameIndex / cols;
            Rectangle src = new Rectangle(col * cellW + 2, row * cellH + 2, Math.Max(1, cellW - 4), Math.Max(1, cellH - 5));
            g.DrawImage(sheet, dst, src, GraphicsUnit.Pixel);
        }

        private static void Draw_Image_Interpretation_3(Graphics g, Image sheet, Rectangle dst, GameEntity e)
        {
            int cols = 4; int rows = 4;
            int cellW = sheet.Width / cols; int cellH = sheet.Height / rows;
            int targetFrameIndex = 4;
            int localAiTick = (Environment.TickCount / 33) % 70;
            if (localAiTick < 20) targetFrameIndex = (4 + (localAiTick / 7) % 3);
            else if (localAiTick < 45) targetFrameIndex = (7 + ((localAiTick - 20) / 5) % 5);
            else if (localAiTick < 60) targetFrameIndex = (12 + ((localAiTick - 45) / 4) % 4);
            int col = targetFrameIndex % cols; int row = targetFrameIndex / rows;
            Rectangle src = new Rectangle(col * cellW + 4, row * cellH + 4, Math.Max(1, cellW - 8), Math.Max(1, cellH - 12));
            g.DrawImage(sheet, dst, src, GraphicsUnit.Pixel);
        }

        private static void Draw_Image_Interpretation_4(Graphics g, Image sheet, Rectangle dst, GameEntity e)
        {
            int cols = 4; int rows = 4;
            int cellW = sheet.Width / cols; int cellH = sheet.Height / rows;
            int targetFrameIndex = e.StateTimer >= 115 ? 14 : (e.StateTimer < 10 ? 15 : (Environment.TickCount / 120) % 14);
            int col = targetFrameIndex % cols; int row = targetFrameIndex / cols;
            Rectangle src = new Rectangle(col * cellW + 3, row * cellH + 3, Math.Max(1, cellW - 6), Math.Max(1, cellH - 6));
            g.DrawImage(sheet, dst, src, GraphicsUnit.Pixel);
        }

        // 인게임 탑다운 전용 에이전트 드로우 시스템 고수
        public static void DrawRecoveryProgram(Graphics g, PlayerState p, bool selected, float cameraX, bool moving)
        {
            float drawX = p.X - cameraX;
            float drawY = p.Y;
            int facing = p.Facing == 0 ? 1 : p.Facing;

            using (SolidBrush shadow = new SolidBrush(Color.FromArgb(85, 0, 0, 0)))
                g.FillEllipse(shadow, drawX - 22, drawY + 14, 44, 14);

            if (selected)
            {
                using (SolidBrush aura = new SolidBrush(Color.FromArgb(18, 70, 180, 255)))
                    g.FillEllipse(aura, drawX - 45, drawY - 45, 90, 90);
            }

            if (p.ActionState == PlayerActionState.Die) DrawPlayerMotionFrame(g, p, drawX, drawY + 35, facing, "player_gameover.png");
            else if (p.ActionState == PlayerActionState.Hit) DrawPlayerMotionFrame(g, p, drawX, drawY + 35, facing, "player_attacked.png");
            else if (p.ActionState == PlayerActionState.Skill) DrawPlayerSkillAction(g, p, drawX, drawY + 45, facing);
            else if (moving) DrawPlayerSwordSprite(g, p, drawX, drawY + 45, true);
            else DrawPlayerStillSprite(g, p, drawX, drawY + 45, facing);

            using (Font f = F(7.5f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.White))
            using (SolidBrush back = new SolidBrush(Color.FromArgb(140, 0, 0, 0)))
            {
                Rectangle label = new Rectangle((int)drawX - 75, (int)drawY + 30, 150, 32);
                g.FillRectangle(back, label);
                g.DrawString("Player.exe\n(AntiVirus Agent)", f, b, label, Center());
            }
        }

        public static void DrawRecoveryProgram(Graphics g, PlayerState p, bool selected)
        {
            DrawRecoveryProgram(g, p, selected, 0f, false);
        }

        private static void DrawBoss(Graphics g, Rectangle r, GameEntity e)
        {
            int centerX = r.X + r.Width / 2;
            int centerY = r.Y + r.Height / 2;
            int bSize = 260;

            Rectangle destRect = new Rectangle(centerX - (bSize / 2), centerY - (bSize / 2), bSize, bSize);
            using (SolidBrush aura = new SolidBrush(Color.FromArgb(50, e.Color)))
                g.FillEllipse(aura, destRect.X - 15, destRect.Y - 15, destRect.Width + 30, destRect.Height + 30);

            if (e.Name.Contains("Driver-K") && ImgBoss_DriverK != null)
            {
                int fW = ImgBoss_DriverK.Width / 4; int fH = ImgBoss_DriverK.Height / 2;
                int fX = e.IsCastingPattern ? 2 : (e.Facing == -1 ? 3 : 0);
                int fY = e.IsCastingPattern ? 1 : 0;
                g.DrawImage(ImgBoss_DriverK, destRect, new Rectangle(fX * fW, fY * fH, fW, fH), GraphicsUnit.Pixel);
                return;
            }
            if (e.Name.Contains("High-Kernel") && ImgBoss_HighKernel != null)
            {
                int fW = ImgBoss_HighKernel.Width / 4; int fH = ImgBoss_HighKernel.Height;
                int fX = e.IsCastingPattern ? 3 : (e.Facing == -1 ? 1 : 0);
                g.DrawImage(ImgBoss_HighKernel, destRect, new Rectangle(fX * fW, 0, fW, fH), GraphicsUnit.Pixel);
                return;
            }
            if (e.Name.Contains("BSOD") && ImgBoss_BSOD != null)
            {
                int fW = ImgBoss_BSOD.Width / 4; int fH = ImgBoss_BSOD.Height / 2;
                int fX = e.IsCastingPattern ? 1 : (e.Facing == -1 ? 3 : 0);
                g.DrawImage(ImgBoss_BSOD, destRect, new Rectangle(fX * fW, 0, fW, fH), GraphicsUnit.Pixel);
                return;
            }
            if ((e.Name.Contains("Exception Queen") || e.Name.Contains("Exception_Queen")) && ImgBoss_ExceptionQueen != null)
            {
                int srcX = e.Facing == -1 ? 810 : 0;
                g.DrawImage(ImgBoss_ExceptionQueen, destRect, new Rectangle(srcX, 0, 790, ImgBoss_ExceptionQueen.Height), GraphicsUnit.Pixel);
                return;
            }
            if (e.Name.Contains("Binny") && ImgBoss_IllegalBinny != null)
            {
                int srcX = e.MotionIndex == 0 ? 0 : (e.MotionIndex == 1 ? 471 : 1042);
                int srcW = e.MotionIndex == 0 ? 450 : 511;
                g.DrawImage(ImgBoss_IllegalBinny, destRect, new Rectangle(srcX, 0, srcW, ImgBoss_IllegalBinny.Height), GraphicsUnit.Pixel);

                if (Img_IceSword != null && Img_FireSword != null && Img_LightningSword != null)
                {
                    int swordH = 140; int swordW = 40;
                    int swordY = destRect.Y - swordH - 5;
                    g.DrawImage(Img_IceSword, centerX - 65 - swordW / 2, swordY, swordW, swordH);
                    g.DrawImage(Img_FireSword, centerX - swordW / 2, swordY, swordW, swordH);
                    g.DrawImage(Img_LightningSword, centerX + 65 - swordW / 2, swordY, swordW, swordH);
                }
                return;
            }
            DrawFallbackFileMonster(g, r, e);
        }

        public static void DrawWeaponUpgradeFile(Graphics g, WeaponUpgradeFile drop, float cameraX)
        {
            RectangleF b = drop.Bounds;
            Rectangle r = Rectangle.Round(new RectangleF(b.X - cameraX, b.Y, b.Width, b.Height));
            int iconSize = 48;
            Rectangle iconRect = new Rectangle(r.X + r.Width / 2 - iconSize / 2, r.Y + r.Height / 2 - iconSize / 2, iconSize, iconSize);
            using (SolidBrush glow = new SolidBrush(Color.FromArgb(75, 80, 190, 255))) g.FillEllipse(glow, iconRect.X - 10, iconRect.Y - 10, iconRect.Width + 20, iconRect.Height + 20);
            DesktopIconUI.Shared.DrawIconOnly(g, 2, 3, iconRect);
            Rectangle levelBox = new Rectangle(iconRect.Right - 16, iconRect.Bottom - 14, 18, 14);
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(165, 0, 0, 0))) g.FillRectangle(bg, levelBox);
            using (Font f = F(7f, FontStyle.Bold)) g.DrawString("+" + drop.UpgradeLevel, f, Brushes.White, levelBox, Center());
        }

        private static void DrawFallbackFileMonster(Graphics g, Rectangle r, GameEntity e)
        {
            using (SolidBrush flashBrush = new SolidBrush(Color.FromArgb(120, Color.White)))
            {
                g.FillEllipse(flashBrush, r.X - 8, r.Y - 8, r.Width + 16, r.Height + 16);
            }
            Rectangle icon = new Rectangle(r.X + r.Width / 2 - 25, r.Y + r.Height / 2 - 24, 50, 48);
            using (LinearGradientBrush b = new LinearGradientBrush(icon, Color.White, Color.FromArgb(225, 232, 246), 90f)) g.FillRectangle(b, icon);
            using (Pen p = new Pen(e.Color, 2f)) g.DrawRectangle(p, icon);
            using (Font f = F(7f, FontStyle.Bold))
            using (SolidBrush sb = new SolidBrush(Darken(e.Color, 30)))
                g.DrawString(e.Kind, f, sb, new Rectangle(icon.X + 3, icon.Y + 16, icon.Width - 6, 16), Center());
        }

        // =============================================================================
        // 🌟 [교정]: 조수 이미지 경로 복구 및 GDI+ 정밀 슬라이싱 엔진 원본 완벽 이식
        // =============================================================================
        private static Image LoadNpc(NpcMood mood)
        {
            if (npcCache.ContainsKey(mood))
                return npcCache[mood];

            Image sheet = LoadNpcEmotionSheet();
            if (sheet != null)
            {
                Rectangle cell = GetNpcEmotionCell(sheet, mood);
                Bitmap normalized = CropNpcEmotionNormalized(sheet, cell, mood);
                if (normalized != null)
                {
                    npcCache[mood] = normalized;
                    return normalized;
                }
            }
            return null;
        }

        private static Image LoadNpcEmotionSheet()
        {
            if (npcEmotionSheetCache != null)
                return npcEmotionSheetCache;

            string fileName = "npc_emotions.png";
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // 💡 억까 방지 전방위 경로 스캔 파이프라인 가동
            string[] paths =
            {
                Path.Combine(baseDir, "Assets", "Characters", "NPC", fileName),
                Path.Combine(baseDir, "Assets", "UI", fileName),
                Path.Combine(baseDir, fileName)
            };

            foreach (string path in paths)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        npcEmotionSheetCache = Image.FromFile(path);
                        return npcEmotionSheetCache;
                    }
                }
                catch
                {
                    npcEmotionSheetCache = null;
                }
            }
            return null;
        }

        private static Rectangle GetNpcEmotionCell(Image sheet, NpcMood mood)
        {
            int col = 0; int row = 0;
            if (mood == NpcMood.Basic) { col = 0; row = 0; }
            else if (mood == NpcMood.Welcome) { col = 1; row = 0; }
            else if (mood == NpcMood.Happy) { col = 0; row = 1; }
            else if (mood == NpcMood.Question) { col = 0; row = 2; }
            else if (mood == NpcMood.Error) { col = 1; row = 2; }
            else if (mood == NpcMood.Bsod) { col = 3; row = 2; }
            else if (mood == NpcMood.Progress) { col = 0; row = 3; }
            else if (mood == NpcMood.Loading) { col = 3; row = 0; }
            else if (mood == NpcMood.Damaged) { col = 2; row = 2; }
            else if (mood == NpcMood.Log) { col = 3; row = 3; }
            else if (mood == NpcMood.Warning) { col = 2; row = 3; }
            else if (mood == NpcMood.Thinking) { col = 1; row = 3; }

            int x1 = (int)Math.Round(sheet.Width * (col / 4.0));
            int y1 = (int)Math.Round(sheet.Height * (row / 4.0));
            int x2 = (int)Math.Round(sheet.Width * ((col + 1) / 4.0));
            int y2 = (int)Math.Round(sheet.Height * ((row + 1) / 4.0));

            return new Rectangle(x1, y1, Math.Max(1, x2 - x1), Math.Max(1, y2 - y1));
        }

        private static Bitmap CropNpcEmotionNormalized(Image sheet, Rectangle cell, NpcMood mood)
        {
            using (Bitmap src = new Bitmap(sheet))
            {
                int left = cell.Right; int top = cell.Bottom;
                int right = cell.Left; int bottom = cell.Top;

                for (int y = cell.Top; y < cell.Bottom && y < src.Height; y++)
                {
                    for (int x = cell.Left; x < cell.Right && x < src.Width; x++)
                    {
                        if (src.GetPixel(x, y).A > 1)
                        {
                            if (x < left) left = x; if (x > right) right = x;
                            if (y < top) top = y; if (y > bottom) bottom = y;
                        }
                    }
                }

                if (right <= left || bottom <= top) return null;

                left = Math.Max(cell.Left, left - 6);
                top = Math.Max(cell.Top, top - 6);
                right = Math.Min(cell.Right - 1, right + 6);
                bottom = Math.Min(cell.Bottom - 1, bottom + 28);

                Rectangle crop = new Rectangle(left, top, right - left + 1, bottom - top + 1);
                Bitmap result = new Bitmap(240, 270);

                using (Graphics g = Graphics.FromImage(result))
                {
                    g.Clear(Color.Transparent);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    float scale = Math.Min(240f / crop.Width, 270f / crop.Height);
                    int drawW = (int)(crop.Width * scale);
                    int drawH = (int)(crop.Height * scale);

                    g.DrawImage(sheet, new Rectangle((240 - drawW) / 2, 270 - drawH, drawW, drawH), crop, GraphicsUnit.Pixel);
                }
                return result;
            }
        }

        public static void DrawNpcImage(Graphics g, Rectangle r, NpcMood mood)
        {
            Image img = LoadNpc(mood);
            if (img != null)
            {
                var oldInterpolation = g.InterpolationMode;
                var oldPixelOffset = g.PixelOffsetMode;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                g.DrawImage(img, r);

                g.InterpolationMode = oldInterpolation;
                g.PixelOffsetMode = oldPixelOffset;
            }
            else
            {
                // 백업 프레임 가동 격실
                using (SolidBrush b = new SolidBrush(Color.FromArgb(42, 70, 120)))
                    g.FillRectangle(b, r);
                using (Font f = F(9f, FontStyle.Bold))
                    g.DrawString("Recovery\nAssistant", f, Brushes.White, r, Center());
            }
        }

        public static void DrawNotification(Graphics g, Rectangle r, string title, string text, NpcMood mood, bool error)
        {
            DrawXPWindow(g, r, title, error);
            Rectangle npcRect = new Rectangle(r.X + 22, r.Y + 58, 140, Math.Max(120, r.Height - 104));
            DrawNpcImage(g, npcRect, mood);

            Rectangle tx = new Rectangle(r.X + 172, r.Y + 58, r.Width - 196, r.Height - 110);
            using (Font f = F(10f, FontStyle.Regular))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(26, 34, 54)))
                g.DrawString(text, f, b, tx, Left());

            Rectangle btn = new Rectangle(r.Right - 120, r.Bottom - 46, 96, 30);
            DrawButton(g, btn, "확인", true);
        }
        // =============================================================================

        private static Image LoadStageBackgroundImage(int stageIndex, bool bossRoom)
        {
            string fileName = "StageBg_" + stageIndex.ToString("00") + (bossRoom ? "_Boss" : "") + ".png";
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", fileName);
            if (!File.Exists(path) && bossRoom) path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "StageBg_" + stageIndex.ToString("00") + ".png");
            if (!File.Exists(path)) return null;
            if (stageImageCache.ContainsKey(path)) return stageImageCache[path];
            try { Image img = Image.FromFile(path); stageImageCache[path] = img; return img; } catch { return null; }
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
                    cg.Clear(Color.FromArgb(12, 16, 28));
                    using (Pen gridPen = new Pen(Color.FromArgb(35, st.Accent), 1f))
                    {
                        for (int x = 0; x < virtualWidth; x += 40) cg.DrawLine(gridPen, x, 0, x, client.Height);
                        for (int y = 0; y < client.Height; y += 40) cg.DrawLine(gridPen, 0, y, virtualWidth, y);
                    }
                    Image stageImage = LoadStageBackgroundImage(st.Index, bossRoom);
                    if (stageImage != null) DrawImageCover(cg, stageImage, new Rectangle(0, 0, virtualWidth, client.Height));
                }
                stageBackgroundCache[key] = cached;
            }
            int scrollX = (int)Math.Max(0, Math.Min(cameraX, virtualWidth - client.Width));
            g.DrawImage(cached, client, new Rectangle(scrollX, 0, client.Width, client.Height), GraphicsUnit.Pixel);
        }

        public static void DrawStageBackground(Graphics g, Rectangle client, StageInfo st, float cameraX) => DrawStageBackground(g, client, st, cameraX, false, client.Width);
        public static void DrawStageBackground(Graphics g, Rectangle client, StageInfo st, float cameraX, bool bossRoom) => DrawStageBackground(g, client, st, cameraX, bossRoom, client.Width);
        public static void DrawXPTaskbar(Graphics g, Rectangle client, string title) => TaskbarUI.Shared.Draw(g, client);

        public static void DrawXPWindow(Graphics g, Rectangle r, string title, bool error)
        {
            using (SolidBrush sh = new SolidBrush(Color.FromArgb(55, 0, 0, 0))) g.FillRectangle(sh, r.X + 6, r.Y + 6, r.Width, r.Height);
            using (SolidBrush b = new SolidBrush(Color.FromArgb(236, 241, 248))) g.FillRectangle(b, r);
            using (Pen p = new Pen(Color.FromArgb(0, 68, 170), 2f)) g.DrawRectangle(p, r.X, r.Y, r.Width - 1, r.Height - 1);
            Rectangle tb = new Rectangle(r.X + 2, r.Y + 2, r.Width - 4, 28);
            Color c1 = error ? Color.FromArgb(210, 35, 35) : Color.FromArgb(20, 88, 210);
            Color c2 = error ? Color.FromArgb(116, 0, 0) : Color.FromArgb(60, 145, 255);
            using (LinearGradientBrush b = new LinearGradientBrush(tb, c1, c2, 0f)) g.FillRectangle(b, tb);
            using (Font f = F(9.5f, FontStyle.Bold)) g.DrawString(title, f, Brushes.White, new Rectangle(tb.X + 8, tb.Y, tb.Width - 80, tb.Height), LeftMiddle());
        }

        public static void DrawXPButton(Graphics g, Rectangle r, string text)
        {
            using (LinearGradientBrush b = new LinearGradientBrush(r, Color.White, Color.FromArgb(190, 210, 240), 90f)) g.FillRectangle(b, r);
            using (Pen p = new Pen(Color.FromArgb(40, 80, 150))) g.DrawRectangle(p, r.X, r.Y, r.Width - 1, r.Height - 1);
            using (Font f = F(8f, FontStyle.Bold)) g.DrawString(text, f, Brushes.Black, r, Center());
        }

        public static void DrawButton(Graphics g, Rectangle r, string text, bool selected)
        {
            using (LinearGradientBrush b = new LinearGradientBrush(r, selected ? Color.FromArgb(255, 246, 182) : Color.White, selected ? Color.FromArgb(240, 178, 54) : Color.FromArgb(202, 222, 250), 90f)) g.FillRectangle(b, r);
            using (Pen p = new Pen(selected ? Color.FromArgb(190, 105, 0) : Color.FromArgb(56, 110, 190), selected ? 2f : 1f)) g.DrawRectangle(p, r.X, r.Y, r.Width - 1, r.Height - 1);
            using (Font f = F(9f, FontStyle.Bold)) g.DrawString(text, f, Brushes.Black, r, Center());
        }

        public static void DrawBar(Graphics g, Rectangle r, int value, int max, Color color)
        {
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(230, 238, 248))) g.FillRectangle(bg, r);
            using (Pen p = new Pen(Color.FromArgb(92, 118, 154))) g.DrawRectangle(p, r.X, r.Y, r.Width - 1, r.Height - 1);
            int w = max > 0 ? Math.Max(0, Math.Min(r.Width - 2, (int)((r.Width - 2) * (value / (float)max)))) : 0;
            if (w > 0)
            {
                Rectangle fill = new Rectangle(r.X + 1, r.Y + 1, w, r.Height - 2);
                using (LinearGradientBrush b = new LinearGradientBrush(fill, Lighten(color, 30), Darken(color, 18), 90f)) g.FillRectangle(b, fill);
            }
        }

        private static void DrawPlayerSwordSprite(Graphics g, PlayerState p, float drawX, float baseY, bool walking) => DrawPlayerAgentFrameImage(g, drawX, p.Y, p.Facing == 0 ? 1 : p.Facing, walking ? (Environment.TickCount / 100) % 8 : -1, walking);
        private static void DrawPlayerStillSprite(Graphics g, PlayerState p, float drawX, float baseY, int facing) => DrawPlayerAgentFrameImage(g, drawX, p.Y, facing, -1, false);

        private static void DrawPlayerAgentFrameImage(Graphics g, float worldX, float worldY, int facing, int frameIndex, bool walking)
        {
            Image frame = LoadPlayerAgentFrame(frameIndex);
            if (frame == null) return;
            GraphicsState state = g.Save();
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            if (facing < 0) { g.TranslateTransform(worldX, worldY); g.ScaleTransform(-1f, 1f); worldX = 0; worldY = 0; }
            g.DrawImage(frame, new Rectangle((int)worldX - 43, (int)worldY - 43, 86, 86));
            g.Restore(state);
        }

        private static Image LoadPlayerAgentFrame(int index)
        {
            if (index < 0)
            {
                if (playerAgentIdleFrame == null)
                {
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "PlayerAgentIdle.png");
                    if (File.Exists(path)) playerAgentIdleFrame = Image.FromFile(path);
                }
                return playerAgentIdleFrame;
            }
            int slot = Math.Max(0, Math.Min(7, index));
            if (playerAgentWalkFrames[slot] == null)
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "PlayerAgentWalk" + slot + ".png");
                if (File.Exists(path)) playerAgentWalkFrames[slot] = Image.FromFile(path);
            }
            return playerAgentWalkFrames[slot];
        }

        private static void DrawPlayerSkillAction(Graphics g, PlayerState p, float drawX, float baseY, int facing) => DrawPlayerAgentFrameImage(g, drawX, p.Y, facing, (Environment.TickCount / 60) % 8, true);
        private static void DrawPlayerMotionFrame(Graphics g, PlayerState p, float drawX, float baseY, int facing, string file) => DrawPlayerAgentFrameImage(g, drawX, p.Y, facing, -1, false);

        public static void DrawEffect(Graphics g, Effect e, float cameraX)
        {
            if (e == null) return;
            int alpha = ClampAlpha((int)(235 * e.Ticks / (float)Math.Max(1, e.MaxTicks)));
            float progress = 1f - e.Ticks / (float)Math.Max(1, e.MaxTicks);
            float x1 = e.X - cameraX;
            float x2 = e.X2 - cameraX;

            if (e.Kind == "binnyIce" || e.Kind == "binnyFire" || e.Kind == "binnyLight")
            {
                Image swordImg = e.Kind == "binnyIce" ? Img_IceSword : (e.Kind == "binnyFire" ? Img_FireSword : Img_LightningSword);
                if (swordImg != null)
                {
                    GraphicsState state = g.Save();
                    g.TranslateTransform(x1, e.Y);
                    g.RotateTransform(progress * 360f);
                    g.DrawImage(swordImg, -20, -60, 40, 120);
                    g.Restore(state);
                }
                return;
            }

            if (e.Kind == "driveShard" && Img_DiskSprite != null)
            {
                float drawX = (e.X + (e.X2 - e.X) * progress) - cameraX;
                float drawY = e.Y + (e.Y2 - e.Y) * progress;
                int fIdx = (Environment.TickCount / 80) % 10;
                int fW = Img_DiskSprite.Width / 10;
                g.DrawImage(Img_DiskSprite, new Rectangle((int)drawX - 16, (int)drawY - 16, 32, 32), new Rectangle(fIdx * fW, 0, fW, Img_DiskSprite.Height), GraphicsUnit.Pixel);
                return;
            }

            if (e.Kind == "safeZone" && Img_Safezone != null)
            {
                int szSize = 170;
                g.DrawImage(Img_Safezone, new Rectangle((int)x1 - szSize / 2, (int)e.Y - szSize / 2, szSize, szSize));
                return;
            }

            if (e.Kind == "projectile")
            {
                float cx = x1 + (x2 - x1) * progress;
                float cy = e.Y + (e.Y2 - e.Y) * progress;

                // ------------------------------------------------=============================
                // 🌟 [교정]: 형광펜 범위 밀착 크기 보정 및 패턴별 투사체 이미지 스핀 분기 격실
                // ------------------------------------------------=============================
                // 💡 기존의 거대한 100px 외곽 하얀 원 영역 내부에 쏙 들어가도록, 
                // 형진님이 칠해주신 노란색 형광펜 밀착 규격인 '55px' 크기로 정밀 드로우 범위를 조정합니다!
                int targetBulletSize = 55;
                Image targetBulletImg = null;

                // EnemyLogicSystem에서 쏘아 올린 투사체 정보의 텍스트(Text) 키워드를 분석하여 무기를 분기합니다.
                string bulletType = (e.Text ?? "").ToUpper();

                if (bulletType == "TELEPORT_BULLET")
                {
                    // 🌀 텔레포트 일제 소사 패턴일 때 바인딩
                    targetBulletImg = Img_ProjectileTeleport;
                }
                else
                {
                    // ⚡ 기본 Heavy_Projectile_Spread 스프레드 사격일 때 바인딩 (기본값)
                    targetBulletImg = Img_EnergyBall;
                }

                if (targetBulletImg != null)
                {
                    // 🚀 [커스텀 에너지 구체 렌더링 파이프라인 가동]
                    GraphicsState bulletState = g.Save();

                    // 렌더링 품질을 고화질 픽셀 아트로 업스케일 설정
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    // 투사체의 진행 좌표 정중앙으로 도화지 중심축 이동 (피봇팅)
                    g.TranslateTransform(cx, cy);

                    // ✨ 슈팅 게임 액션 연출: 날아가는 속도와 시간(progress)에 비례하여 사방으로 팽팽 흩뿌려지며 고속 회전!
                    g.RotateTransform(progress * 1080f);

                    // 지정된 형광펜 최적화 규격(-27.5px ~ 27.5px 오프셋)으로 이미지 중심 매칭 인쇄
                    g.DrawImage(targetBulletImg, -targetBulletSize / 2, -targetBulletSize / 2, targetBulletSize, targetBulletSize);

                    g.Restore(bulletState);
                }
                else
                {
                    // 🛡️ [백업 가드]: 혹시라도 이미지 파일이 누락되었을 때 게임이 뻗지 않도록 기존 하얀 원을 축소하여 출력
                    using (SolidBrush core = new SolidBrush(Color.FromArgb(alpha, Color.FromArgb(255, 230, 100))))
                        g.FillEllipse(core, cx - (targetBulletSize / 2), cy - (targetBulletSize / 2), targetBulletSize, targetBulletSize);
                }
                return;
            }
            if (e.Kind == "text")
            {
                using (Font f = F(11f, FontStyle.Bold)) g.DrawString(e.Text, f, Brushes.OrangeRed, x1 - 50, e.Y - progress * 40);
            }
            else if (e.Kind == "spark")
            {
                using (Pen p = new Pen(Color.FromArgb(alpha, e.Color), 2.5f)) g.DrawEllipse(p, x1 - 20 * progress, e.Y - 20 * progress, 40 * progress, 40 * progress);
            }
        }

        // =============================================================================
        // 🌟 [안전화 복원]: 외부 드로우 충돌을 차단하기 위한 순정 규격 브릿지 인터페이스
        // =============================================================================
        public static void DrawShopShortcut(Graphics g, Rectangle r, int coins)
        {
            // 이중 간섭을 막기 위해 DesktopIconUI의 내부 자산 파이프라인으로 안전하게 이관 우회합니다.
            DesktopIconUI.Shared.DrawRecoveryToolsShortcut(g, r, coins, null);
        }

        public static void DrawFileShortcut(Graphics g, Rectangle r, StageInfo st, bool sel, bool bNew)
        {
            // 오타가 나던 수동 그리기를 중단하고 DesktopIconUI의 안정적인 이미지 렌더러로 연결을 양도합니다.
            if (st != null)
            {
                DesktopIconUI.Shared.DrawIconOnly(g, 2, 1, r);
            }
        }
        public static string GetBossKoreanName(string engName) => engName;

        public static void DrawBossGlobalUI(Graphics g, GameEntity boss, Size clientSize)
        {
            if (boss == null || boss.Hp <= 0) return;
            Rectangle hpBarRect = new Rectangle(clientSize.Width / 2 - 350, 45, 700, 22);
            DrawBar(g, hpBarRect, boss.Hp, boss.MaxHp, Color.FromArgb(210, 35, 35));
            using (Font f = F(11f, FontStyle.Bold)) g.DrawString($"{boss.Name} [ HP: {boss.Hp} / {boss.MaxHp} ]", f, Brushes.White, hpBarRect, Center());
        }
        private static int ClampAlpha(int v) => Math.Max(0, Math.Min(255, v));
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
        public static void FillRoundedRectangle(this Graphics g, Brush brush, RectangleF bounds, float radius) { using (GraphicsPath path = RoundedPath(bounds, radius)) g.FillPath(brush, path); }
        public static void DrawRoundedRectangle(this Graphics g, Pen pen, RectangleF bounds, float radius) { using (GraphicsPath path = RoundedPath(bounds, radius)) g.DrawPath(pen, path); }
    }
}