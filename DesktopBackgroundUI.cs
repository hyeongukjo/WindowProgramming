using System.Drawing;

namespace DebugHeroFileDungeonRPG
{
    public sealed class DesktopBackgroundUI
    {
        public static readonly DesktopBackgroundUI Shared = new DesktopBackgroundUI();

        private DesktopBackgroundUI()
        {
        }

        public void Draw(Graphics g, Rectangle client)
        {
            BackgroundRenderer.DrawDesktopFramework(g, client.Width, client.Height);
        }
    }
}