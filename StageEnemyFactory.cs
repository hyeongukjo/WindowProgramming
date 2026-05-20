using System;
using System.Collections.Generic;

namespace DebugHeroFileDungeonRPG
{
    public static class StageEnemyFactory
    {
        public static List<GameEntity> CreatePreBossEnemies(StageInfo st, int clientHeight, Random random)
        {
            List<GameEntity> enemies = new List<GameEntity>();
            if (st == null) return enemies;
            string[] names = GetPreBossEnemyNames(st);
            int baseHp = 48 + st.Index * 20;
            for (int i = 0; i < names.Length; i++)
            {
                int hp = baseHp + i * 18 + (st.Index >= 6 ? 18 : 0);
                enemies.Add(new GameEntity
                {
                    Name = names[i],
                    DisplayName = names[i],
                    Kind = StageEnemyExtension(names[i]),
                    X = 380 + i * 245,
                    Y = Math.Max(130, clientHeight - 260 + (i % 4) * 44),
                    VX = (i % 2 == 0 ? 1 : -1) * (1.0f + st.Index * 0.075f),
                    VY = (i % 3 == 0 ? 1 : -1) * (0.55f + st.Index * 0.035f),
                    Hp = hp,
                    MaxHp = hp,
                    Attack = 5 + st.Index + i,
                    IsBoss = false,
                    Color = st.Accent,
                    CoinReward = 12 + st.Index * 3 + i * 4
                });
            }
            return enemies;
        }

        public static GameEntity CreateBoss(StageInfo st, float x, int clientHeight, int totalStages)
        {
            int bossHp = 950 + st.Index * 280;
            int bossAttack = 10 + st.Index * 3;
            int minimumHp = 950 + st.Index * 260;
            int minimumAttack = 10 + st.Index * 2;
            if (st.Index == 1 && st.BossName.IndexOf("Driver", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                minimumHp = 1850;
                minimumAttack = 22;
            }
            if (st.Index >= 8)
            {
                minimumHp += 420;
                bossHp += 520;
                bossAttack += 4;
            }
            if (st.Index == totalStages)
            {
                minimumHp += 900;
                bossHp += 900;
                bossAttack += 6;
            }
            bossHp = Math.Max(bossHp, minimumHp);
            bossAttack = Math.Max(bossAttack, minimumAttack);
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
                Color = st.Accent,
                CoinReward = 90 + st.Index * 18
            };
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
