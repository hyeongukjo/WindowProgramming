using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DebugHeroFileDungeonRPG
{
    public sealed class GameSaveData
    {
        public string ProfileName = "";
        public int UnlockedStage = 1;
        public int SelectedStage = 1;
        public int ClearedStages = 0;
        public int CurrentStage = 0;
        public int Level = 1;
        public int WeaponLevel = 1;
        public int Exp = 0;
        public int Coins = 0;
        public int HpPotions = 2;
        public int MpPotions = 2;
        public int MaxHp = 10000;
        public int MaxMp = 60000;
        public int RelationLog = 0;
        public int QuarantinedBosses = 0;
        public int ProfileTruthScore = 0;
    }

    /// <summary>
    /// 저장/불러오기 전담 클래스입니다.
    /// Continue는 이 파일이 존재할 때만 작동하고, 플레이어가 사망하면 저장 파일을 삭제합니다.
    /// </summary>
    public static class GameSaveSystem
    {
        private const int SaveVersion = 1;

        public static string SaveDirectory
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "DebugHeroFileDungeonRPG");
            }
        }

        public static string SavePath
        {
            get { return Path.Combine(SaveDirectory, "save.dat"); }
        }

        public static bool HasSave()
        {
            return File.Exists(SavePath);
        }

        public static void DeleteSave()
        {
            try
            {
                if (File.Exists(SavePath)) File.Delete(SavePath);
            }
            catch { }
        }

        public static void Save(GameSaveData data)
        {
            try
            {
                Directory.CreateDirectory(SaveDirectory);
                List<string> lines = new List<string>();
                lines.Add("version=" + SaveVersion);
                lines.Add("profile=" + ToBase64(data.ProfileName ?? ""));
                lines.Add("unlockedStage=" + data.UnlockedStage);
                lines.Add("selectedStage=" + data.SelectedStage);
                lines.Add("clearedStages=" + data.ClearedStages);
                lines.Add("currentStage=" + data.CurrentStage);
                lines.Add("level=" + data.Level);
                lines.Add("weaponLevel=" + data.WeaponLevel);
                lines.Add("exp=" + data.Exp);
                lines.Add("coins=" + data.Coins);
                lines.Add("hpPotions=" + data.HpPotions);
                lines.Add("mpPotions=" + data.MpPotions);
                lines.Add("maxHp=" + data.MaxHp);
                lines.Add("maxMp=" + data.MaxMp);
                lines.Add("relationLog=" + data.RelationLog);
                lines.Add("quarantinedBosses=" + data.QuarantinedBosses);
                lines.Add("truthScore=" + data.ProfileTruthScore);
                File.WriteAllLines(SavePath, lines.ToArray(), Encoding.UTF8);
            }
            catch { }
        }

        public static bool TryLoad(out GameSaveData data)
        {
            data = new GameSaveData();
            try
            {
                if (!File.Exists(SavePath)) return false;
                string[] lines = File.ReadAllLines(SavePath, Encoding.UTF8);
                Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;
                    map[line.Substring(0, eq).Trim()] = line.Substring(eq + 1).Trim();
                }

                data.ProfileName = FromBase64(Get(map, "profile", ""));
                data.UnlockedStage = Clamp(GetInt(map, "unlockedStage", 1), 1, 10);
                data.SelectedStage = Clamp(GetInt(map, "selectedStage", 1), 1, 10);
                data.ClearedStages = Clamp(GetInt(map, "clearedStages", 0), 0, 10);
                data.CurrentStage = Clamp(GetInt(map, "currentStage", 0), 0, 10);
                data.Level = Math.Max(1, GetInt(map, "level", 1));
                data.WeaponLevel = Math.Max(1, GetInt(map, "weaponLevel", 1));
                data.Exp = Math.Max(0, GetInt(map, "exp", 0));
                data.Coins = Math.Max(0, GetInt(map, "coins", 0));
                data.HpPotions = Math.Max(0, GetInt(map, "hpPotions", 2));
                data.MpPotions = Math.Max(0, GetInt(map, "mpPotions", 2));
                data.MaxHp = Math.Max(1, GetInt(map, "maxHp", 10000));
                data.MaxMp = Math.Max(1, GetInt(map, "maxMp", 60000));
                data.RelationLog = Math.Max(0, GetInt(map, "relationLog", 0));
                data.QuarantinedBosses = Math.Max(0, GetInt(map, "quarantinedBosses", 0));
                data.ProfileTruthScore = Math.Max(0, GetInt(map, "truthScore", 0));
                return true;
            }
            catch
            {
                data = new GameSaveData();
                return false;
            }
        }

        private static string Get(Dictionary<string, string> map, string key, string fallback)
        {
            string value;
            return map.TryGetValue(key, out value) ? value : fallback;
        }

        private static int GetInt(Dictionary<string, string> map, string key, int fallback)
        {
            string value;
            if (!map.TryGetValue(key, out value)) return fallback;
            int n;
            return int.TryParse(value, out n) ? n : fallback;
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private static string ToBase64(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value ?? ""));
        }

        private static string FromBase64(string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value)) return "";
                return Encoding.UTF8.GetString(Convert.FromBase64String(value));
            }
            catch { return ""; }
        }
    }
}
