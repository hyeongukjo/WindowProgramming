using System;
using System.Collections.Generic;

namespace DebugHeroFileDungeonRPG
{
    public static class StageEnemyFactory
    {
        public static List<GameEntity> CreateWaveEnemies(StageInfo st, int waveIndex, int clientHeight)
        {
            List<GameEntity> list = new List<GameEntity>();
            // 💡 [지시사항 1 반영]: 웨이브를 딱 2개로 축소합니다. (0웨이브, 1웨이브만 허용)
            if (st == null || waveIndex < 0 || waveIndex > 1) return list;

            int stageNum = st.Index;
            string monsterName = "UNKNOWN";
            string targetAssetFileName = "Dash_1.png";

            // 💡 [지시사항 2 반영]: 스테이지별 / 웨이브별 등장 개체 및 규칙 완벽 하드코딩 고정
            if (waveIndex == 0)
            {
                // 1웨이브 일반 몬스터 배치 명세
                if (stageNum == 1 || stageNum == 3 || stageNum == 5 || stageNum == 7)
                {
                    monsterName = "Registry_Ghost_Key";
                    targetAssetFileName = "Teleport_1.png";
                }
                else if (stageNum == 9)
                {
                    monsterName = "Runtime_Clock_Buoy";
                    targetAssetFileName = "Spread_1.png";
                }
            }
            else if (waveIndex == 1)
            {
                // 2웨이브: 지정된 몬스터들이 보스급(기존 4웨이브 정예 중간보스 스펙)으로 등장
                if (stageNum == 1)
                {
                    monsterName = "Security_Firewall";
                    targetAssetFileName = "Dash_1.png";
                }
                else if (stageNum == 3)
                {
                    monsterName = "Alert_Popup_Spam";
                    targetAssetFileName = "Dash_2.png";
                }
                else if (stageNum == 5)
                {
                    monsterName = "Runtime_Clock_Buoy";
                    targetAssetFileName = "Spread_1.png";
                }
                else if (stageNum == 7)
                {
                    monsterName = "Runtime_Clock_Buoy_Elite";
                    targetAssetFileName = "Spread_2.png";
                }
                else if (stageNum == 9)
                {
                    monsterName = "Packet_Minnow";
                    targetAssetFileName = "Teleport_2.png"; // 3x3 고해상도 PNG 지정
                }
            }

            // * 형진님 스펙 명세: 기본 수치 150 및 스테이지별 가중치 100 밸런스 완전 유지
            int baseHp = 150 + stageNum * 100;
            int baseAtk = 4 + stageNum;

            int finalHp = baseHp + (waveIndex * 14);
            int finalAtk = baseAtk + waveIndex;

            // 💡 [지시사항 1 후반부 반영]: 1웨이브는 4마리 스폰, 2웨이브는 기존 4웨이브 정예 기믹 수치 주입
            int spawnCount = (waveIndex == 1) ? 2 : 4;

            if (waveIndex == 1)
            {
                finalHp = (int)(finalHp * 2.5f);   // 2웨이브 정예 중간보스급 HP 2.5배 격상
                finalAtk = (int)(finalAtk * 1.5f); // 2웨이브 정예 중간보스급 ATK 1.5배 격상
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
                    IsBoss = false, // 대형 게이지를 띄우지 않는 일반 배치형 정예 보스 룰 고수
                    RewardGiven = false
                });
            }
            return list;
        }

        public static GameEntity CreateBoss(StageInfo st, float x, int clientHeight, int totalStages)
        {
            // 메인 거대 보스방용 팩토리 링크 유지 (일반 웨이브 규칙에 영향 없음)
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