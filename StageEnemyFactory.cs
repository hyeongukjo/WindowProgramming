using System;
using System.Collections.Generic;

namespace DebugHeroFileDungeonRPG
{
    public static class StageEnemyFactory
    {
        public static List<GameEntity> CreateWaveEnemies(StageInfo st, int waveIndex, int clientHeight)
        {
            List<GameEntity> list = new List<GameEntity>();
            //  웨이브를 딱 2개로 축소합니다. (0웨이브, 1웨이브만 허용)
            if (st == null || waveIndex < 0 || waveIndex > 1) return list;

            int stageNum = st.Index;
            string monsterName = "UNKNOWN";
            string targetAssetFileName = "Dash_1.png";

            //  스테이지별 / 웨이브별 등장 개체 및 규칙 완벽 하드코딩 고정
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

          
           
            int baseHp = 60 + stageNum * 40;
            int baseAtk = 4 + stageNum;

            // waveIndex가 올라갈 때마다 기본 체력이 20씩 증가합니다.
            int finalHp = baseHp + (waveIndex * 20);
            int finalAtk = baseAtk + waveIndex;

            // [지시사항 1 후반부 반영]: 1웨이브는 4마리 스폰, 2웨이브는 기존 4웨이브 정예 기믹 수치 주입
            int spawnCount = (waveIndex == 1) ? 2 : 4;

            if (waveIndex == 1)
            {
                finalHp = (int)(finalHp * 2.5f);   // 2웨이브 정예 중간보스급 HP 2.5배 격상
                finalAtk = (int)(finalAtk * 1.5f); // 2웨이브 정예 중간보스급 ATK 1.5배 격상
            }

            for (int i = 0; i < spawnCount; i++)
            {
                // 최종 산출: 1스테이지 0웨이브 i=0 일 때 -> finalHp(100) + (0 * 5) = 정확히 100 시작!
                int calculatedHp = finalHp + (i * 5);

                list.Add(new GameEntity
                {
                    Name = monsterName,
                    DisplayName = monsterName,
                    Kind = targetAssetFileName,
                    X = 460 + (i * 245),
                    Y = Math.Max(140, clientHeight - 270 + (i % 2) * 60),
                    VX = (i % 2 == 0 ? 1.1f : -1.1f) * (1.0f + waveIndex * 0.06f),
                    VY = (i % 2 == 0 ? 0.5f : -0.5f) * (1.0f + waveIndex * 0.03f),

                    //재조정된 체력 변수 매핑 연동
                    Hp = calculatedHp,
                    MaxHp = calculatedHp,

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