using System;
using System.Collections.Generic;

namespace DebugHeroFileDungeonRPG
{
    public static class StageEnemyFactory
    {
        public static List<GameEntity> CreateWaveEnemies(StageInfo st, int waveIndex, int clientHeight)
        {
            List<GameEntity> list = new List<GameEntity>();
            if (st == null || waveIndex < 0 || waveIndex > 3) return list;

            int stageNum = st.Index;

            // * [1. 제미나이가 이미지 기반으로 명명한 순수 테스트 자산 및 이름 배정 풀 정의]
            string[] testAssets = { "Dash_1.png", "Dash_2.png", "Spread_1.png", "Spread_2.png", "Teleport_1.png" };
            string[] newNames = { "Security_Firewall", "Alert_Popup_Spam", "Runtime_Clock_Buoy", "Runtime_Clock_Buoy", "Registry_Ghost_Key" };

            // * [2. 랜덤 분배 연산 공식 가동: 스테이지 번호와 웨이브 인덱스를 조합하여 전 구역에 골고루 섞어 사출]
            int monsterTypeIndex = (stageNum + waveIndex) % testAssets.Length;

            string monsterName = newNames[monsterTypeIndex];
            string targetAssetFileName = testAssets[monsterTypeIndex];

            // * [3. 능력치 밸런스 및 4웨이브 정예화 버프 연산]
            int baseHp = 44 + stageNum * 16;
            int baseAtk = 4 + stageNum;
            int hp = baseHp + (waveIndex * 14);

            int spawnCount = (waveIndex == 3) ? 2 : 4;
            if (waveIndex == 3)
            {
                hp = hp * 3;
            }

            for (int i = 0; i < spawnCount; i++)
            {
                list.Add(new GameEntity
                {
                    Name = monsterName,               // * [식별 네임]: AI 엔진이 어떤 패턴을 가동할지 판정하는 근본 키
                    DisplayName = monsterName,        // * [머리 위 이름표]: 새 이름 명세 출력
                    Kind = targetAssetFileName,       // * [에셋 파일명]: Renderer가 크롭 드로우 방식을 선택하는 열쇠
                    X = 460 + (i * 245),
                    Y = Math.Max(140, clientHeight - 270 + (i % 2) * 60),
                    VX = (i % 2 == 0 ? 1.1f : -1.1f) * (1.0f + waveIndex * 0.06f),
                    VY = (i % 2 == 0 ? 0.5f : -0.5f) * (1.0f + waveIndex * 0.03f),
                    Hp = hp + (i * 3),
                    MaxHp = hp + (i * 3),
                    Attack = baseAtk + waveIndex,
                    IsBoss = false,
                    RewardGiven = false
                });
            }
            return list;
        }

        public static GameEntity CreateBoss(StageInfo st, float x, int clientHeight, int totalStages)
        {
            if (st == null) return null;
            int bossHp = 950 + st.Index * 280;
            int bossAttack = 10 + st.Index * 3;
            return new GameEntity
            {
                Name = st.BossName,
                DisplayName = st.BossName,
                Kind = "BOSS",
                X = x,
                Y = Math.Max(170, clientHeight - 210),
                VX = 0,
                VY = 0,
                Hp = bossHp,
                MaxHp = bossHp,
                Attack = bossAttack,
                IsBoss = true,
                Color = st.Accent
            };
        }

        public static List<GameEntity> CreatePreBossEnemies(StageInfo st, int clientHeight, Random random)
        {
            return new List<GameEntity>();
        }
    }
}