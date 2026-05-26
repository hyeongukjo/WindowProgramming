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
            string monsterName = "Unknown_Enemy";
            string targetAssetFileName = "moster.png";

            // * [1. 순수 원본 기획서 데이터 명세 테이블 매핑 완벽 복구]
            if (stageNum == 1)
            {
                string[] names = { "Broken_Document.txt", "Empty_Folder", "Broken_Shortcut.lnk", "Unemptied_Trash.bak" };
                string[] assets = { "file_monster.png", "folder_monster.png", "shortcut_monster.png", "trash_monster.png" };
                monsterName = names[waveIndex];
                targetAssetFileName = assets[waveIndex];
            }
            else if (stageNum == 3)
            {
                string[] names = { "Unknown Device", "Broken Driver Icon", "IRQ Conflict", "Driver Cache Fragment" };
                string[] assets = { "patch_monster.png", "slime_monster.png", "reminder_monster.png", "failed_monster.png" };
                monsterName = names[waveIndex];
                targetAssetFileName = assets[waveIndex];
            }
            else if (stageNum == 5)
            {
                string[] names = { "Packet Minnow", "Open Port Buoy", "Request Crab", "Firewall Barnacle" };
                string[] assets = { "packet_monster.png", "port_monster.png", "crab_monster.png", "firewall_monster.png" };
                monsterName = names[waveIndex];
                targetAssetFileName = assets[waveIndex];
            }
            else if (stageNum == 7)
            {
                string[] names = { "Broken Key", "Duplicate Value", "Orphan Entry", "Recent Trace" };
                string[] assets = { "key_monster.png", "value_monster.png", "orphan_monster.png", "trace_monster.png" };
                monsterName = names[waveIndex];
                targetAssetFileName = assets[waveIndex];
            }
            else if (stageNum == 9)
            {
                string[] names = { "Temp Fragment", "Cache Leech", "Unsent Report", "Recent Ghost" };
                string[] assets = { "temp_monster.png", "leech_monster.png", "report_monster.png", "ghost_monster.png" };
                monsterName = names[waveIndex];
                targetAssetFileName = assets[waveIndex];
            }

            // -----------------------------------------------------------------
            // * [2. 임시 주입 공간]: 향후 테스트나 비중 조절을 하실 때만 이 아래 공간을 사용하시면 됩니다.
            // * [현재는 순수한 원본 복구를 위해 깨끗하게 비워둡니다]
            // -----------------------------------------------------------------

            // * [기본 능력치 밸런스 연산]
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
                    Name = monsterName,
                    DisplayName = monsterName,
                    Kind = targetAssetFileName,
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