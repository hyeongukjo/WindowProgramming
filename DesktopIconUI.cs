using System.Collections.Generic;
using System.Drawing;

namespace DebugHeroFileDungeonRPG
{
    public sealed class DesktopIconUI
    {
        public static readonly DesktopIconUI Shared = new DesktopIconUI();

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
            int cols = 5;
            int startX = 120;
            int startY = 90;

            for (int i = 1; i <= unlockedStage && i <= stages.Count; i++)
            {
                int col = (i - 1) % cols;
                int row = (i - 1) / cols;

                Rectangle r = new Rectangle(
                    startX + col * 170,
                    startY + row * 145,
                    128,
                    112
                );

                StageInfo st = stages[i - 1];

                bool selected = selectedStage == i;
                bool newly = i == unlockedStage && i > clearedStages;

                Renderer.DrawFileShortcut(g, r, st, selected, newly);
                buttons.Add(new UiButton(r, "stage" + i.ToString()));
            }
        }
    }
}