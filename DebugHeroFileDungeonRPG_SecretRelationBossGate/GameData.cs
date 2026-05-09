using System;
using System.Collections.Generic;
using System.Drawing;

namespace DebugHeroFileDungeonRPG
{
    public enum GameScreen
    {
        CinematicIntro,
        Title,
        AdminIntro,
        Customize,
        Desktop,
        Dungeon,
        EquipPrompt,
        Result,
        RelationChoice,
        SuspectSelect,
        Ending,
        Help
    }

    public enum ItemKind
    {
        Weapon,
        Armor,
        Outfit,
        Consumable,
        Material
    }

    public enum MonsterKind
    {
        PopupSlime,
        DefenderBot,
        ProcessWolf,
        RegistryWraith,
        KernelGolem,
        BSODDragon
    }

    public sealed class Player
    {
        public string Name = "admin";
        public int Level = 1;
        public int Exp = 0;
        public int NextExp = 100;
        public int Gold = 250;
        public int PatchShards = 0;
        public int Hp = 150;
        public int MaxHp = 150;
        public int Mp = 70;
        public int MaxMp = 70;
        public int Attack = 26;
        public int Defense = 10;
        public int Speed = 8;
        public int Potion = 5;
        public int MpPotion = 3;
        public float X = 180;
        public float Y = 330;
        public float TargetX = 180;
        public float TargetY = 330;
        public int Facing = 1;
        public int Outfit = 0;
        public int Weapon = 0;
        public int Armor = 0;
        public int Cape = 0;
        public int WeaponLevel = 1;
        public int ShieldTicks = 0;
        public int InvincibleTicks = 0;
        public int StoredItems = 0;
        public int EquippedItems = 0;

        public RectangleF Bounds
        {
            get { return new RectangleF(X - 24, Y - 42, 48, 72); }
        }

        public Color OutfitColor
        {
            get
            {
                int idx = Math.Max(0, Math.Min(Outfit, OutfitPalette.Length - 1));
                return OutfitPalette[idx];
            }
        }

        public Color WeaponColor
        {
            get
            {
                int idx = Math.Max(0, Math.Min(Weapon, WeaponPalette.Length - 1));
                return WeaponPalette[idx];
            }
        }

        public string OutfitName
        {
            get
            {
                int idx = Math.Max(0, Math.Min(Outfit, GameData.OutfitNames.Length - 1));
                return GameData.OutfitNames[idx];
            }
        }

        public string WeaponName
        {
            get
            {
                int idx = Math.Max(0, Math.Min(Weapon, GameData.WeaponNames.Length - 1));
                return GameData.WeaponNames[idx] + "+" + WeaponLevel;
            }
        }

        public string ArmorName
        {
            get
            {
                int idx = Math.Max(0, Math.Min(Armor, GameData.ArmorNames.Length - 1));
                return GameData.ArmorNames[idx];
            }
        }

        public string CapeName
        {
            get
            {
                int idx = Math.Max(0, Math.Min(Cape, GameData.CapeNames.Length - 1));
                return GameData.CapeNames[idx];
            }
        }

        private static readonly Color[] OutfitPalette = new Color[]
        {
            Color.FromArgb(55, 170, 230), Color.FromArgb(72, 120, 255), Color.FromArgb(110, 220, 140), Color.FromArgb(245, 178, 60),
            Color.FromArgb(75, 120, 255), Color.FromArgb(210, 115, 245), Color.FromArgb(45, 50, 70), Color.FromArgb(255, 95, 105)
        };

        private static readonly Color[] WeaponPalette = new Color[]
        {
            Color.FromArgb(100, 220, 255), Color.FromArgb(255, 95, 125), Color.FromArgb(90, 210, 230), Color.FromArgb(255, 185, 70),
            Color.FromArgb(135, 190, 255), Color.FromArgb(95, 155, 255), Color.FromArgb(140, 255, 160), Color.FromArgb(255, 220, 110)
        };

        public void ApplyCustomization(int outfit, int weapon, int armor, int cape)
        {
            Outfit = Math.Max(0, Math.Min(outfit, GameData.OutfitNames.Length - 1));
            Weapon = Math.Max(0, Math.Min(weapon, GameData.WeaponNames.Length - 1));
            Armor = Math.Max(0, Math.Min(armor, GameData.ArmorNames.Length - 1));
            Cape = Math.Max(0, Math.Min(cape, GameData.CapeNames.Length - 1));

            int[] outfitMp = new int[] { 0, 8, 18, 10, 14, 24, 6, 4 };
            int[] weaponAtk = new int[] { 0, 8, 6, 10, 12, 15, 7, 18 };
            int[] armorHp = new int[] { 0, 22, 28, 14, 36, 30, 46, 40 };
            int[] armorDef = new int[] { 0, 4, 5, 2, 7, 6, 9, 8 };
            int[] capeSpeed = new int[] { 0, 1, 2, 3, 2, 1, 2, 4 };
            int[] capeMp = new int[] { 0, 0, 8, 4, 10, 6, 12, 5 };
            //테스트용으로 피통 데미지 보정. 나중에 다시 조정
            MaxHp = 1000 + armorHp[Armor] + Outfit * 3;
            MaxMp = 1000 + outfitMp[Outfit] + capeMp[Cape];
            Attack = (28 + weaponAtk[Weapon] + WeaponLevel * 2) * 2;
            Defense = 12 + armorDef[Armor] + Armor / 2;
            Speed = 8 + capeSpeed[Cape] + (Outfit == 3 ? 1 : 0);
            Hp = Math.Min(MaxHp, Math.Max(1, Hp <= 0 ? MaxHp : Hp));
            Mp = Math.Min(MaxMp, Math.Max(0, Mp <= 0 ? MaxMp : Mp));
            X = 180;
            Y = 330;
            TargetX = X;
            TargetY = Y;
        }

        public bool AddExp(int amount)
        {
            bool leveled = false;
            Exp += Math.Max(0, amount);
            while (Exp >= NextExp)
            {
                Exp -= NextExp;
                Level++;
                MaxHp += 14;
                MaxMp += 7;
                Attack += 3;
                Defense += 1;
                Speed += Level % 2 == 0 ? 1 : 0;
                Hp = MaxHp;
                Mp = MaxMp;
                NextExp = 100 + Level * 90;
                leveled = true;
            }
            return leveled;
        }
    }

    public sealed class DungeonInfo
    {
        public string Name;
        public string FileName;
        public string Description;
        public int RequiredPatch;
        public int RecommendedLevel;
        public int MapWidth;
        public Color Accent;
        public Color BackColor;
        public bool Boss;

        public DungeonInfo(string name, string fileName, string description, int requiredPatch, int recommendedLevel, int mapWidth, Color accent, Color backColor, bool boss)
        {
            Name = name;
            FileName = fileName;
            Description = description;
            RequiredPatch = requiredPatch;
            RecommendedLevel = recommendedLevel;
            MapWidth = mapWidth;
            Accent = accent;
            BackColor = backColor;
            Boss = boss;
        }
    }

    public sealed class Monster
    {
        public MonsterKind Kind;
        public string Name;
        public int Level;
        public int Hp;
        public int MaxHp;
        public int Attack;
        public int Exp;
        public int Gold;
        public bool Boss;
        public float X;
        public float Y;
        public float VX;
        public float VY;
        public int HitFlash;
        public int AttackCooldown;
        public Color Color;
        // 몬스터 클래스 내에서 패턴 사용 여부를 추적하는 변수들
        public bool Pattern75Used = false;
        public bool Pattern50Used = false;
        public bool Pattern25Used = false;

        public Monster(MonsterKind kind, string name, int level, int hp, int attack, int exp, int gold, bool boss, float x, float y, Color color)
        {
            Kind = kind;
            Name = name;
            Level = level;
            Hp = hp;
            MaxHp = hp;
            Attack = attack;
            Exp = exp;
            Gold = gold;
            Boss = boss;
            X = x;
            Y = y;
            VX = boss ? 1.0f : 1.6f;
            VY = 0;
            Color = color;
        }

        public RectangleF Bounds
        {
            get
            {
                if (Boss) return new RectangleF(X - 78, Y - 90, 156, 128);
                return new RectangleF(X - 28, Y - 38, 56, 62);
            }
        }
    }

    public sealed class DroppedItem
    {
        public string Name;
        public string Description;
        public ItemKind Kind;
        public Color Color;
        public float X;
        public float Y;
        public int AttackBonus;
        public int DefenseBonus;
        public int HpBonus;
        public int MpBonus;
        public int PatchBonus;
        public bool Dragging;
        public Point DragPoint;

        public DroppedItem(string name, string description, ItemKind kind, Color color, float x, float y,
            int attack, int defense, int hp, int mp, int patch)
        {
            Name = name;
            Description = description;
            Kind = kind;
            Color = color;
            X = x;
            Y = y;
            AttackBonus = attack;
            DefenseBonus = defense;
            HpBonus = hp;
            MpBonus = mp;
            PatchBonus = patch;
        }

        public Rectangle ScreenRect(float cameraX, float cameraY)
        {
            if (Dragging) return new Rectangle(DragPoint.X - 26, DragPoint.Y - 30, 52, 60);
            return new Rectangle((int)(X - cameraX - 24), (int)(Y - cameraY - 32), 48, 56);
        }
    }

    public sealed class Effect
    {
        public string Kind;
        public float X;
        public float Y;
        public float X2;
        public float Y2;
        public int Ticks;
        public int MaxTicks;
        public Color Color;
        public string Text;

        public Effect(string kind, float x, float y, float x2, float y2, int ticks, Color color, string text)
        {
            Kind = kind;
            X = x;
            Y = y;
            X2 = x2;
            Y2 = y2;
            Ticks = ticks;
            MaxTicks = Math.Max(1, ticks);
            Color = color;
            Text = text;
        }
    }

    public sealed class UiButton
    {
        public Rectangle Bounds;
        public string Action;
        public UiButton(Rectangle bounds, string action)
        {
            Bounds = bounds;
            Action = action;
        }
    }

    public static class GameData
    {
        public static readonly string[] OutfitNames = new string[]
        {
            "디버그 코트", "관리자 트렌치", "백신 네온 후드", "레어파일 재킷",
            "블루스크린 롱코트", "레지스트리 로브", "커널 블랙수트", "패치워크 아머"
        };

        public static readonly string[] WeaponNames = new string[]
        {
            "Debug Blade", "Crash Breaker", "Packet Saber", "Registry Keyblade",
            "Kernel Halberd", "BSOD Reaver", "Patch Cannon", "Admin Executor"
        };

        public static readonly string[] ArmorNames = new string[]
        {
            "Basic Patchwear", "Firewall Guard", "Driver Plate", "Cache Booster",
            "Kernel Vest", "Registry Mail", "BSOD Barrier", "Admin Authority Armor"
        };

        public static readonly string[] CapeNames = new string[]
        {
            "없음", "Shortcut Cape", "Admin Trail", "Packet Streamer",
            "Registry Cloak", "Kernel Shadow", "BSOD Neon Wing", "Patch Aurora"
        };

        public static List<DungeonInfo> CreateDungeons()
        {
            List<DungeonInfo> list = new List<DungeonInfo>();
            // 일반 맵은 유지, 보스 맵(두 번째 인자)의 가로 길이를 절반으로 수정
            list.Add(new DungeonInfo("01 File Explorer 숲", "Explorer_FolderForest.exe", "...", 0, 1, 3600, Color.FromArgb(54, 184, 105), Color.FromArgb(20, 74, 66), false));
            list.Add(new DungeonInfo("02 Driver Vault 격납고", "Driver_VaultDevice.drv", "Boss 1", 10, 3, 1900, Color.FromArgb(155, 175, 200), Color.FromArgb(48, 58, 76), true));
            list.Add(new DungeonInfo("03 Windows Update 연구소", "Windows_Update_Lab.patch", "...", 24, 5, 3900, Color.FromArgb(88, 178, 255), Color.FromArgb(34, 60, 98), false));
            list.Add(new DungeonInfo("04 System32 금지구역", "System32_KernelGuard.sys", "Boss 2", 42, 7, 2000, Color.FromArgb(230, 82, 66), Color.FromArgb(76, 28, 36), true));
            list.Add(new DungeonInfo("05 Network Port 항구", "Network_PortTunnel.net", "...", 64, 9, 4200, Color.FromArgb(56, 190, 190), Color.FromArgb(18, 70, 86), false));
            list.Add(new DungeonInfo("06 Blue Screen Tower", "BSOD_DragonFault.bsod", "Boss 3", 90, 11, 2000, Color.FromArgb(80, 150, 255), Color.FromArgb(12, 24, 88), true));
            list.Add(new DungeonInfo("07 Registry Hive 보관소", "Registry_HiveArchive.reg", "...", 120, 13, 4300, Color.FromArgb(232, 160, 56), Color.FromArgb(82, 52, 28), false));
            list.Add(new DungeonInfo("08 Popup Error 미궁", "Popup_ErrorZone.dll", "Boss 4", 154, 15, 2100, Color.FromArgb(82, 158, 255), Color.FromArgb(28, 50, 96), true));
            list.Add(new DungeonInfo("09 Temp Cache 동굴", "Temp_CacheCleaner.tmp", "...", 192, 17, 4450, Color.FromArgb(115, 205, 150), Color.FromArgb(42, 74, 52), false));
            list.Add(new DungeonInfo("10 Recycle Bin 던전", "$Recycle.Bin_Quarantine.zip", "Final", 235, 20, 2150, Color.FromArgb(158, 100, 230), Color.FromArgb(50, 36, 84), true));
            return list;
        }

        public static List<Monster> CreateMonsters(DungeonInfo dungeon)
        {
            List<Monster> m = new List<Monster>();
            int lv = dungeon.RecommendedLevel;
            Color a = dungeon.Accent;
            string file = dungeon.FileName;

            int baseHp = 230 + lv * 58;
            int baseAtk = 20 + lv * 5;
            int baseExp = 80 + lv * 22;
            int baseGold = 60 + lv * 15;

            // 보스 스테이지인 경우 보스만 생성하고 즉시 반환
            if (dungeon.Boss)
            {
                if (file.Contains("Driver"))
                {
                    m.Add(new Monster(MonsterKind.DefenderBot, "Driver-K", lv + 2, 3200 + lv * 145, baseAtk + 18, 720, 520, true, 2750, 330, Color.FromArgb(220, 230, 245)));
                }
                else if (file.Contains("System32"))
                {
                    m.Add(new Monster(MonsterKind.KernelGolem, "High-Kernel", lv + 2, 4300 + lv * 170, baseAtk + 24, 920, 680, true, 2900, 330, a));
                }
                else if (file.Contains("BSOD"))
                {
                    m.Add(new Monster(MonsterKind.BSODDragon, "BSOD", lv + 3, 5600 + lv * 210, baseAtk + 30, 1200, 900, true, 2860, 330, Color.FromArgb(60, 120, 255)));
                }
                else if (file.Contains("Popup"))
                {
                    m.Add(new Monster(MonsterKind.KernelGolem, "Exception Queen", lv + 2, 5200 + lv * 190, baseAtk + 28, 1100, 820, true, 2880, 335, a));
                }
                else if (file.Contains("Recycle"))
                {
                    m.Add(new Monster(MonsterKind.KernelGolem, "Illegal_Binny", lv + 3, 7800 + lv * 260, baseAtk + 40, 1800, 1300, true, 3000, 345, Color.FromArgb(190, 120, 255)));
                }
                return m;
            }

            // 일반 스테이지용 몬스터 구성
            if (file.Contains("Explorer"))
            {
                m.Add(new Monster(MonsterKind.PopupSlime, "Popup Slime", lv, baseHp, baseAtk, baseExp, baseGold, false, 560, 330, Color.FromArgb(70, 210, 255)));
                m.Add(new Monster(MonsterKind.DefenderBot, "Defender Process", lv + 1, baseHp + 70, baseAtk + 3, baseExp + 20, baseGold + 12, false, 1060, 260, Color.FromArgb(235, 235, 245)));
            }
            else if (file.Contains("Temp"))
            {
                m.Add(new Monster(MonsterKind.PopupSlime, "Cache Blob", lv + 2, baseHp + 160, baseAtk + 6, baseExp + 42, baseGold + 24, false, 3380, 295, Color.FromArgb(120, 255, 180)));
            }
            else if (file.Contains("Network"))
            {
                m.Add(new Monster(MonsterKind.ProcessWolf, "Internal Packet", lv + 2, baseHp + 150, baseAtk + 6, baseExp + 42, baseGold + 24, false, 3360, 280, Color.FromArgb(65, 210, 210)));
            }
            else if (file.Contains("Registry"))
            {
                m.Add(new Monster(MonsterKind.RegistryWraith, "Path Key", lv + 2, baseHp + 170, baseAtk + 6, baseExp + 44, baseGold + 24, false, 3360, 285, Color.FromArgb(240, 190, 90)));
            }
            else
            {
                // 기본 몬스터 세트
                m.Add(new Monster(MonsterKind.PopupSlime, "System Slime", lv, baseHp, baseAtk, baseExp, baseGold, false, 560, 330, a));
                m.Add(new Monster(MonsterKind.ProcessWolf, "Task Wolf", lv + 1, baseHp + 90, baseAtk + 4, baseExp + 26, baseGold + 16, false, 1540, 390, a));
            }

            return m;
        }

        public static string GetDungeonNpc(DungeonInfo dungeon)
        {
            string f = dungeon.FileName;
            if (f.Contains("Explorer")) return "탐색요정 Searchy";
            if (f.Contains("Driver")) return "드라이버 정비공 Driver-K";
            if (f.Contains("Update")) return "업데이트 아저씨 PatchMan";
            if (f.Contains("System32")) return "커널 수호자 High-Kernel";
            if (f.Contains("Network")) return "패킷 선장 Ping";
            if (f.Contains("BSOD")) return "블루스크린 드래곤 BSOD";
            if (f.Contains("Registry")) return "레지스트리 사서 Regi";
            if (f.Contains("Popup")) return "오류지옥 집행관 Exception Queen";
            if (f.Contains("Temp")) return "임시파일 청소부 Temp";
            if (f.Contains("Recycle")) return "휴지통 관리자 Illegal_Binny";
            return "NPC 404호";
        }

        public static string GetDungeonClearLog(DungeonInfo dungeon, int truthScore)
        {
            string f = dungeon.FileName;
            if (f.Contains("Explorer")) return "[단서 1]\n범인은 파일 구조에 능숙하다. 바로가기와 원본 파일의 위치를 구분할 수 있는 존재입니다.";
            if (f.Contains("Driver")) return "[Boss 1 격리]\nDriver-K는 장치 충돌의 주범으로 몰려 NPC 404호에 의해 Recycle Bin 격리 구역으로 이동됩니다.";
            if (f.Contains("Update")) return "[단서 2]\n범인은 시스템 최적화를 고의로 방해했다. 업데이트 지연 로그가 조작되어 있습니다.";
            if (f.Contains("System32")) return "[Boss 2 격리]\nHigh-Kernel은 무단 침입자로 오인받아 격퇴되었습니다. NPC 404호가 격리 절차를 실행합니다.";
            if (f.Contains("Network")) return "[단서 3]\n공격은 외부 패킷이 아니라 내부 마우스 조작에서 시작되었습니다.";
            if (f.Contains("BSOD")) return "[Boss 3 격리]\nBSOD는 시스템 붕괴의 원흉으로 지목되어 Recycle Bin 격리 구역으로 이동됩니다.";
            if (f.Contains("Registry")) return "[단서 4]\n범인은 관리자 권한을 가진 유일한 존재다. 실행 경로와 권한 로그가 같은 방향을 가리킵니다.";
            if (f.Contains("Popup")) return "[Boss 4 격리]\nException Queen은 예외 발생을 방치한 죄로 Recycle Bin 격리 구역에 구금됩니다.";
            if (f.Contains("Temp")) return "[마지막 단서]\n범인의 로그 기록은 지금 이 순간에도 쌓이고 있습니다. 임시 캐시는 지워져도 흔적을 남깁니다.";
            if (f.Contains("Recycle")) return "[FINAL]\nIllegal_Binny는 구금된 용의자들을 지키는 최후의 보루였습니다. 이제 격리 구역의 문이 열립니다.";
            return "[SYSTEM LOG]\n알 수 없는 로그입니다.";
        }

        public static string GetHintText(DungeonInfo dungeon, int truthScore)
        {
            string f = dungeon.FileName;
            bool deep = truthScore >= 4;
            if (f.Contains("Explorer")) return deep ? "심층 힌트: 범인은 외부 바이러스가 아니라 가장 안쪽에서 파일을 조작할 수 있는 존재입니다." : "힌트: 범인은 파일 구조에 능숙합니다.";
            if (f.Contains("Driver")) return "격리 기록: Driver-K는 피의자지만 단독 범행 증거는 부족합니다.";
            if (f.Contains("Update")) return deep ? "심층 힌트: 최적화 방해는 누군가의 반복된 연기 명령에서 시작되었습니다." : "힌트: 시스템 최적화가 고의로 방해되었습니다.";
            if (f.Contains("System32")) return "격리 기록: High-Kernel은 시스템 방어자일 가능성이 있습니다.";
            if (f.Contains("Network")) return deep ? "심층 힌트: 시작점은 외부 접속이 아니라 내부 마우스 조작입니다." : "힌트: 공격은 외부가 아닌 내부 조작에서 시작됐습니다.";
            if (f.Contains("BSOD")) return "격리 기록: BSOD는 원인이라기보다 누적된 오류의 결과일 수 있습니다.";
            if (f.Contains("Registry")) return deep ? "심층 힌트: 관리자 권한 로그가 결정적입니다." : "힌트: 범인은 관리자 권한을 가진 유일한 존재입니다.";
            if (f.Contains("Popup")) return "격리 기록: Exception Queen은 오류를 만든 자가 아니라 오류를 보여준 자일 수 있습니다.";
            if (f.Contains("Temp")) return deep ? "심층 힌트: 범인의 로그는 지금도 누적 중입니다." : "힌트: 로그 기록은 아직 쌓이고 있습니다.";
            if (f.Contains("Recycle")) return "최종 힌트: 모든 구금 기록을 비교하세요. 진실은 한 보스에게만 있지 않습니다.";
            return "힌트: 아직 읽을 수 없는 로그입니다.";
        }
    }
}
