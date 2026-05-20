using System;
using System.Collections.Generic;
using System.Drawing;

namespace DebugHeroFileDungeonRPG
{
    public static class RewardSystem
    {
        public static void AwardDefeatReward(GameEntity m, PlayerState player, int currentStage, List<Effect> effects, Random random)
        {
            if (m == null || m.RewardGiven) return;
            m.RewardGiven = true;
            int coin = m.CoinReward > 0 ? m.CoinReward : (m.IsBoss ? 60 + currentStage * 12 : 12 + currentStage * 3 + random.Next(3, 9));
            player.Coins += coin;
            effects.Add(new Effect("text", m.X, m.Y - 105, m.X, m.Y - 105, 48, Color.Gold, "COIN +" + coin));
            if (m.IsBoss) effects.Add(new Effect("text", m.X, m.Y - 145, m.X, m.Y - 145, 56, Color.Orange, "BOSS REWARD"));
        }
    }
}
