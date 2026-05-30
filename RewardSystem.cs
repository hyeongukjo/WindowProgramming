using System;
using System.Collections.Generic;
using System.Drawing;

namespace DebugHeroFileDungeonRPG
{
    public static class RewardSystem
    {
        // =============================================================================
        //  [소울류 재화 밸런싱] 감질나는 잡몹 드랍 코인 및 보스 대형 현상금 공식 매립
        // =============================================================================
        public static void AwardDefeatReward(GameEntity m, PlayerState player, int currentStage, List<Effect> effects, Random random)
        {
            if (m == null || m.RewardGiven) return;
            m.RewardGiven = true;

            int coin = 0;

            if (m.CoinReward > 0)
            {
                // 데이터팩 자체에 고정 보상 코인이 심겨 있는 특수 개체인 경우
                coin = m.CoinReward;
            }
            else if (m.IsBoss)
            {
                // 1. 보스 처치 시 대형 현상금 수급 (다음 구역 진입 전 상점 풀파밍 자원 제공)
                //    Stage 2: 400 코인 | Stage 6: 1200 코인 | Stage 10: 2000 코인
                coin = currentStage * 200;
            }
            else
            {
                // 2. 일반 잡몹은 감질나게 지급 (방 하나를 완벽히 털어야 포션 1개 살 돈이 겨우 모임)
                //    Stage 1 기준: 대략 11 ~ 18 코인 수급 루프
                coin = random.Next(20, 30) + (currentStage * 3);
            }

            player.Coins += coin;

            // 획득한 코인 수치를 노란색 이펙트 텍스트로 화면에 실시간 팝업
            effects.Add(new Effect("text", m.X, m.Y - 105, m.X, m.Y - 105, 48, Color.Gold, $"COIN +{coin}"));

            if (m.IsBoss)
            {
                // 소울류 맛을 살린 보스 처단 쾌감 문구 출력
                effects.Add(new Effect("text", m.X, m.Y - 145, m.X, m.Y - 145, 56, Color.Orange, "BOSS QUARANTINED"));
            }
        }
    }
}