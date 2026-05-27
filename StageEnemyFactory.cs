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

            // * [1. 6종의 고유 몬스터 자산 명세 완벽 유지]
            string[] testAssets = {
                "Dash_1.png", "Dash_2.png",
                "Spread_1.png", "Spread_2.png",
                "Teleport_1.png", "Teleport_2.png"
            };

            string[] newNames = {
                "Security_Firewall", "Alert_Popup_Spam",
                "Runtime_Clock_Buoy", "Runtime_Clock_Buoy",
                "Registry_Ghost_Key", "Packet_Minnow"
            };

            // 💡 [2. 규칙적 순서 전면 파괴 - 하이퍼 비선형 랜덤 믹싱 알고리즘]
            // * [돌진->포탑->텔포의 뻔한 고정 순서 시퀀스를 완전히 깨부숩니다]
            // * [스테이지와 웨이브 값에 소수 곱 연산과 XOR 비트 노이즈를 믹스하여 불규칙성을 극대화합니다]
            int hashSeed = (stageNum * 269) ^ (waveIndex * 397);
            hashSeed = (hashSeed ^ (hashSeed >> 5)) * 7919; // 소수 기반 비선형 난수 유도

            int monsterTypeIndex = Math.Abs(hashSeed) % testAssets.Length;

            string monsterName = newNames[monsterTypeIndex];
            string targetAssetFileName = testAssets[monsterTypeIndex];

            // * [3. 형진님 마스터 명세]: 기본 수치 150 및 스테이지별 가중치 100 밸런스 완전 사수
            int baseHp = 150 + stageNum * 100;
            int baseAtk = 4 + stageNum;

            int finalHp = baseHp + (waveIndex * 14);
            int finalAtk = baseAtk + waveIndex;

            // * [4. 4웨이브 정예 중간 보스 규칙 완벽 사수]
            int spawnCount = (waveIndex == 3) ? 2 : 4;

            if (waveIndex == 3)
            {
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