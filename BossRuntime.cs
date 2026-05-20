using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DebugHeroFileDungeonRPG
{
    public sealed class BossRuntime
    {
        public readonly BossPatternManager patternManager = new BossPatternManager();
        private int currentStage;

        public void Reset(int stageIndex)
        {
            currentStage = stageIndex;
            patternManager.Reset();
        }

        public void Update(int stageIndex, GameEntity boss, PlayerState player, List<Effect> effects, Rectangle client, float mapWidth)
        {
            currentStage = stageIndex;
            patternManager.Update(boss, player, effects, mapWidth);
        }

        public bool HandleClick(Point mousePos)
        {
            return patternManager.HandleClick(mousePos);
        }

        public void DrawOverlay(Graphics g, int stageIndex, bool stageBossPhase, float cameraX, Size clientSize)
        {
            if (!stageBossPhase) return;
            DrawProjectiles(g, cameraX);
            DrawShardPattern(g, cameraX);
            DrawResourcePattern(g, clientSize);
            DrawNotice(g, clientSize);
        }

        private void DrawProjectiles(Graphics g, float cameraX)
        {
            foreach (BossProjectile p in patternManager.Projectiles)
            {
                int sx = (int)(p.X - cameraX);
                int sy = (int)p.Y;
                using (SolidBrush glow = new SolidBrush(Color.FromArgb(90, 150, 80, 230))) g.FillEllipse(glow, sx - 18, sy - 18, 36, 36);
                using (SolidBrush core = new SolidBrush(Color.FromArgb(230, 255, 255, 255))) g.FillEllipse(core, sx - 7, sy - 7, 14, 14);
                using (Pen pen = new Pen(Color.MediumPurple, 2f)) g.DrawEllipse(pen, sx - 18, sy - 18, 36, 36);
            }
        }

        private void DrawShardPattern(Graphics g, float cameraX)
        {
            if (!patternManager.IsShardPatternActive) return;
            int sx = (int)(patternManager.CurrentShardPos.X - cameraX);
            int sy = (int)patternManager.CurrentShardPos.Y;
            using (SolidBrush glow = new SolidBrush(Color.FromArgb(90, Color.Lime))) g.FillEllipse(glow, sx - 36, sy - 36, 72, 72);
            using (SolidBrush body = new SolidBrush(Color.FromArgb(70, 255, 120)))
                g.FillPolygon(body, new Point[] { new Point(sx, sy - 28), new Point(sx + 24, sy), new Point(sx, sy + 28), new Point(sx - 24, sy) });
            using (Pen pen = new Pen(Color.White, 2f))
                g.DrawPolygon(pen, new Point[] { new Point(sx, sy - 28), new Point(sx + 24, sy), new Point(sx, sy + 28), new Point(sx - 24, sy) });
            using (Font f = Renderer.F(10f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.White))
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
            {
                Rectangle label = new Rectangle(sx - 135, sy - 76, 270, 30);
                g.FillRectangle(bg, label);
                g.DrawString("DRIVE SHARD " + (patternManager.ShardSequence + 1) + "/3  " + (patternManager.ShardTimer / 60.0).ToString("0.0") + "s", f, b, label, Renderer.Center());
            }
        }

        private void DrawResourcePattern(Graphics g, Size clientSize)
        {
            if (!patternManager.IsResourcePatternActive) return;
            using (Font f = Renderer.F(11f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.White))
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(170, 0, 0, 0)))
            {
                Rectangle label = new Rectangle(clientSize.Width / 2 - 260, 82, 520, 34);
                g.FillRectangle(bg, label);
                g.DrawString("RESOURCE 부족 경고: DEBUG 아이콘을 클릭해 삭제하세요  " + (patternManager.ResourceTimer / 60.0).ToString("0.0") + "s", f, b, label, Renderer.Center());
            }
            for (int i = 0; i < patternManager.DebugButtons.Count; i++)
            {
                Rectangle r = patternManager.DebugButtons[i];
                using (LinearGradientBrush br = new LinearGradientBrush(r, Color.FromArgb(255, 245, 245), Color.FromArgb(255, 110, 110), 90f)) g.FillRectangle(br, r);
                using (Pen pen = new Pen(Color.DarkRed, 2f)) g.DrawRectangle(pen, r.X, r.Y, r.Width - 1, r.Height - 1);
                using (Font f = Renderer.F(10f, FontStyle.Bold))
                using (SolidBrush b = new SolidBrush(Color.DarkRed)) g.DrawString("DEBUG", f, b, r, Renderer.Center());
            }
        }

        private void DrawNotice(Graphics g, Size clientSize)
        {
            if (patternManager.NoticeTicks <= 0 || string.IsNullOrEmpty(patternManager.NoticeText)) return;
            using (Font f = Renderer.F(10f, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.FromArgb(255, 240, 120)))
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(150, 0, 0, 0)))
            {
                Rectangle label = new Rectangle(clientSize.Width / 2 - 230, 122, 460, 30);
                g.FillRectangle(bg, label);
                g.DrawString(patternManager.NoticeText, f, b, label, Renderer.Center());
            }
        }
    }
}
