using System.Collections.Generic;
using System.Drawing;

namespace DebugHeroFileDungeonRPG
{
    public sealed class RecoveryToolsUI
    {
        public static readonly RecoveryToolsUI Shared = new RecoveryToolsUI();

        private RecoveryToolsUI()
        {
        }

        public void DrawDesktopShortcut(
            Graphics g,
            Rectangle client,
            int coins,
            List<UiButton> buttons)
        {
            Rectangle iconRect = new Rectangle(
                120,
                client.Height - 170,
                150,
                116
            );

            Renderer.DrawShopShortcut(g, iconRect, coins);

            buttons.Add(new UiButton(iconRect, "openShop"));
        }
    }
}