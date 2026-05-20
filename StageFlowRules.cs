using System;

namespace DebugHeroFileDungeonRPG
{
    public static class StageFlowRules
    {
        public static int GetStageMapWidth(StageInfo st, bool bossPhase, int clientWidth)
        {
            if (st == null) return Math.Max(clientWidth, 1600);
            if (bossPhase) return Math.Max(clientWidth, 1760 + st.Index * 55);
            return Math.Max(clientWidth, 1650 + st.Index * 180);
        }
    }
}
