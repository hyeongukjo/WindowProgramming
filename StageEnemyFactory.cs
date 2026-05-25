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

            // * [각 스테이지별 기획서 데이터 명세 수동 테이블 매핑]
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

            int baseHp = 44 + stageNum * 16;
            int baseAtk = 4 + stageNum;
            int hp = baseHp + (waveIndex * 14);

            // * [2. 정예 유닛 자동화 판정: 패턴과 무관하게 4번째 웨이브(index 3)라면 버프 적용]
            int spawnCount = (waveIndex == 3) ? 2 : 4;
            if (waveIndex == 3)
            {
                // * [피드백 반영: 4번째 웨이브 몬스터는 체력을 일반의 3배로 대폭 증폭]
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

        private static string[] GetPreBossEnemyNames(StageInfo st)
        {
            List<string> result = new List<string>();
            if (st.Enemies != null)
            {
                for (int i = 0; i < st.Enemies.Length; i++)
                {
                    string name = st.Enemies[i];
                    if (string.IsNullOrWhiteSpace(name)) continue;
                    if (!string.IsNullOrWhiteSpace(st.BossName) && name.Trim().Equals(st.BossName.Trim(), StringComparison.OrdinalIgnoreCase)) continue;
                    result.Add(name);
                }
            }
            if (result.Count >= 3) return result.ToArray();
            string[] fallback;
            switch (st.Index)
            {
                case 1: fallback = new string[] { "새 폴더 무리", "진짜최종 파일", "깨진 바로가기", "휴지통 과부하" }; break;
                case 2: fallback = new string[] { "Unknown Device", "Broken Driver Icon", "IRQ Conflict", "Driver Cache Fragment" }; break;
                case 3: fallback = new string[] { "Update Patch 조각", "Loading Bar Slime", "Restart Reminder", "Failed Update Fragment" }; break;
                case 4: fallback = new string[] { "Access Denied Hound", "Kernel Fragment", "Protected File", "System Guard" }; break;
                case 5: fallback = new string[] { "Packet Jelly", "Port Scanner", "Latency Ghost", "Broken Cable" }; break;
                case 6: fallback = new string[] { "Crash Pixel", "STOP Code", "Blue Fragment", "Frozen Cursor" }; break;
                case 7: fallback = new string[] { "Key Value Wraith", "Broken Hive", "Registry Lock", "Permission Node" }; break;
                case 8: fallback = new string[] { "Popup Slime", "Warning Box", "Unhandled Exception", "Close Button Mimic" }; break;
                case 9: fallback = new string[] { "Temp File", "Cache Dust", "Overload Crumb", "Memory Leak Spark" }; break;
                default: fallback = new string[] { "Quarantine Guard", "Deleted Fragment", "Recycle Warden", "Trash Cache" }; break;
            }
            while (result.Count < 4) result.Add(fallback[result.Count % fallback.Length]);
            return result.ToArray();
        }

        private static string StageEnemyExtension(string name)
        {
            if (name.Contains("폴더")) return ".folder";
            if (name.Contains("바로가기")) return ".lnk";
            if (name.Contains("Update") || name.Contains("Patch")) return ".patch";
            if (name.Contains("Key") || name.Contains("Value")) return ".reg";
            if (name.Contains("Report")) return ".tmp";
            if (name.Contains("Port") || name.Contains("Packet")) return ".net";
            return ".file";
        }
    }
}

