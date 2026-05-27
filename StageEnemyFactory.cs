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

            // * [1. 제미나이 새 이름 명세 풀 정의]
            string[] testAssets = { "Dash_1.png", "Dash_2.png", "Spread_1.png", "Spread_2.png", "Teleport_1.png" };
            string[] newNames = { "Security_Firewall", "Alert_Popup_Spam", "Runtime_Clock_Buoy", "Runtime_Clock_Buoy", "Registry_Ghost_Key" };

            // * [2. 무작위 분배 공식 연동]
            int monsterTypeIndex = (stageNum + waveIndex) % testAssets.Length;
            string monsterName = newNames[monsterTypeIndex];
            string targetAssetFileName = testAssets[monsterTypeIndex];

            // * [3. 일반 몹 기준 베이스 스펙 연산]
            int baseHp = 44 + stageNum * 16;
            int baseAtk = 4 + stageNum;

            int finalHp = baseHp + (waveIndex * 14);
            int finalAtk = baseAtk + waveIndex;

            // * [4. 형진님 지시 마스터 정예화 조건 주입]: 패턴 종류와 관계없이 무조건 4웨이브 타겟팅
            // * [4웨이브(waveIndex == 3)일 때만 정확하게 스폰 카운트를 2마리로 줄이고 능력치를 증폭합니다]
            int spawnCount = (waveIndex == 3) ? 2 : 4;

            if (waveIndex == 3)
            {
                // * [기존의 잘못된 3배 곱 연산을 전면 폐기하고, 약속된 공식만 대입합니다]
                finalHp = (int)(finalHp * 2.5f);   // * [체력 정확히 2.5배 격상]
                finalAtk = (int)(finalAtk * 1.5f); // * [공격력 정확히 1.5배 격상]
            }

            for (int i = 0; i < spawnCount; i++)
            {
                list.Add(new GameEntity
                {
                    Name = monsterName,
                    DisplayName = monsterName,
                    Kind = targetAssetFileName,
                    X = 460 + (i * 245),
                    Y = Math.Max(140, clientHeight - 270 + (i % 2) * 60),
                    VX = (i % 2 == 0 ? 1.1f : -1.1f) * (1.0f + waveIndex * 0.06f),
                    VY = (i % 2 == 0 ? 0.5f : -0.5f) * (1.0f + waveIndex * 0.03f),
                    Hp = finalHp + (i * 3),
                    MaxHp = finalHp + (i * 3),
                    Attack = finalAtk,
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