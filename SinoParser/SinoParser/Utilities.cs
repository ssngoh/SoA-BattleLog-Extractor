using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SinoParser
{
    static class Utilities
    {
        public enum CharClass
        {
            NONE,
            BREAKER,
            CRUSHER,
            PALADIN,
            CRUSHER_HNM,
            PALADIN_HNM,
        }


        //Used when running from VS. If we build an exe, the 2 root directories code will be different. (TBC)
        //This gives us Sinoalice SQL\SinoParser\SinoParser
        public static string rootProjectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;// "C:/Users/Onsla/OneDrive/Desktop/Sinoalice SQL/Images"; //Environment.CurrentDirectory;

        //This gives us Sinoalice SQL
        public static string rootDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent.FullName;

        public static string configDirectory = rootProjectDirectory + "/Config";
        public static string parseDirectory = rootDirectory + "/Images/ToParse";
        public static string inputDirectory = "\"" + configDirectory + "/Input" + "\"";
        public static string processDirectory = configDirectory + "/Output";
        public static string processDirectoryForArg = "\"" + configDirectory + "/Output" + "\"";

        private static StreamWriter errorLog;

        private static Dictionary<int, BuffDebuffEffectiveness> harpDampening = new Dictionary<int, BuffDebuffEffectiveness>();
        private static Dictionary<int, BuffDebuffEffectiveness> tomeDampening = new Dictionary<int, BuffDebuffEffectiveness>();
        private static Dictionary<int, int> statsNumberDisplay = new Dictionary<int, int>();
        private static Dictionary<uint, float> staffSkillLevel = new Dictionary<uint, float>();
        private static Dictionary<uint, float> harpTomeSkillLevel = new Dictionary<uint, float>();
        private static Dictionary<uint, float> dpsSkillLevel = new Dictionary<uint, float>();
        private static Dictionary<string, StoreColoStatsDetails> initialStats = new Dictionary<string, StoreColoStatsDetails>();
        private static Dictionary<string, Weapon> weaponDictionary = new Dictionary<string, Weapon>();
        private static HashSet<string> comboWeapons = new HashSet<string>();

        //Stores friendlies and enemies. bool to check VG or not. true = VG
        private static Dictionary<string,bool> storeFriendlyNames = new Dictionary<string, bool>();
        private static Dictionary<string, bool> storeEnemyNames = new Dictionary<string, bool>();

        //First int is SB (1/2/3). Second int is skill level. float is estimated effectiveness
        private static Dictionary<uint, Dictionary<uint, float>> supportBoonDictionary = new Dictionary<uint, Dictionary<uint, float>>();
        private static Dictionary<uint, Dictionary<uint, float>> recoverySupportDictionary = new Dictionary<uint, Dictionary<uint, float>>();
        private static Dictionary<uint, Dictionary<uint, float>> dauntlessCourageDictionary = new Dictionary<uint, Dictionary<uint, float>>();

        static Utilities()
        {
            errorLog = new StreamWriter(processDirectory + "/error.txt");
        }

        public static void WriteToErrorLog(string errorString, string folder = "")
        {
            if(folder.Length > 0)
                errorLog.WriteLine(folder);

            errorLog.WriteLine(errorString);
            errorLog.WriteLine();
        }

        public static void CloseFile()
        {
            errorLog.Close();
        }

        public static float GetHarpDisplayAndEffectiveness(int percentageFloorIn)
        {
            if (harpDampening.ContainsKey(percentageFloorIn))
                //return (int)((1 - harpDampening[percentageFloorIn].damping) * 100f);
                return harpDampening[percentageFloorIn].effectiveness;
            else
            {
                WriteToErrorLog("GetHarpDisplayAndDampening key not found: " + percentageFloorIn);
                return float.MaxValue;
            }
        }


        public static float GetTomeDisplayAndEffectiveness(int percentageFloorIn)
        {
            if (tomeDampening.ContainsKey(percentageFloorIn))
                //return (int)((1 - tomeDampening[percentageFloorIn].damping) * 100f);
                return tomeDampening[percentageFloorIn].effectiveness;
            else
            {
                WriteToErrorLog("GetTomeDisplayAndDampening key not found: " + percentageFloorIn);
                return int.MinValue;
            }
        }

        public static int GetStatsNumberDisplay(int percentageFloorIn)
        {
            if (statsNumberDisplay.ContainsKey(percentageFloorIn))
                return statsNumberDisplay[percentageFloorIn];
            else
            {
                WriteToErrorLog("GetStatsNumberDisplay key not found: " + percentageFloorIn);
                return int.MinValue;
            }
        }

        public static void AddInitialStats(string key, StoreColoStatsDetails value)
        {
            initialStats.Add(key, value);
        }

        public static Stats GetInitialStats(string key)
        {
            if (initialStats.ContainsKey(key))
                return initialStats[key].GetCurrentStats();

            WriteToErrorLog("Get initial stats cannot find:" + key);
            return new Stats(0, 0, 0, 0);
        }

        public static void ParseBuffDebuffDampening(string file, FileType ft)
        {
            string [] lines = File.ReadAllLines(file);
            string correctText = string.Empty;
            for (int i = 0; i < lines.Length; ++i)
            {
                string[] split = lines[i].Split(",");

                if (split.Length == 3)
                {
                    if (ft == FileType.HARP_EFFECTIVENESS)
                        harpDampening.Add(int.Parse(split[0]), new BuffDebuffEffectiveness(int.Parse(split[1]), float.Parse(split[2])));
                    else
                        tomeDampening.Add(int.Parse(split[0]), new BuffDebuffEffectiveness(int.Parse(split[1]), float.Parse(split[2])));
                }
                else
                    WriteToErrorLog(file + " row " + i + " in ParseBuffDebuff doesn't have 3 columns");
            }
        }

        public static void ParseWeaponsDictionary(string file)
        {
            string [] lines = File.ReadAllLines(file);
            string correctText = string.Empty;

            //lines[0] is for description
            for (int i = 1; i < lines.Length; ++i)
            {
                string[] split = lines[i].Split(",");

                if (split.Length == 10)
                {
                    float targets, damage, recovery, patk, matk, pdef, mdef;
                    float.TryParse(split[2], out targets);
                    float.TryParse(split[3], out damage);
                    float.TryParse(split[4], out recovery);
                    float.TryParse(split[5], out patk);
                    float.TryParse(split[6], out matk);
                    float.TryParse(split[7], out pdef);
                    float.TryParse(split[8], out mdef);

                    weaponDictionary.TryAdd(split[0], new Weapon(split[1], targets, damage, recovery, patk, matk, pdef, mdef, split[9]));
                }
                else
                    WriteToErrorLog(file + " row " + i + " in ParseWeaponsDictionary doesn't have 10 columns");
            }
        }

        public static void ParseSupportConfig(string file, FileType ft)
        {
            string[] lines = File.ReadAllLines(file);
            string correctText = string.Empty;

            for (int i = 0; i < lines.Length; ++i)
            {
                string[] split = lines[i].Split(",");

                if (split.Length == 3)
                {
                    uint level, skillLevel;
                    float effectiveness;

                    uint.TryParse(split[0], out level);
                    uint.TryParse(split[1], out skillLevel);
                    float.TryParse(split[2], out effectiveness);

                    if (ft == FileType.SUPPORT_BOON_CONFIG)
                    {
                        if (!supportBoonDictionary.ContainsKey(level))
                            supportBoonDictionary.Add(level, new Dictionary<uint, float>());

                        supportBoonDictionary[level].Add(skillLevel, effectiveness);
                    }
                    else if(ft == FileType.RECOVERY_SUPPORT_CONFIG)
                    {
                        if (!recoverySupportDictionary.ContainsKey(level))
                            recoverySupportDictionary.Add(level, new Dictionary<uint, float>());

                        recoverySupportDictionary[level].Add(skillLevel, effectiveness);
                    }
                    else if (ft == FileType.DAUNTLESS_COURAGE_CONFIG)
                    {
                        if (!dauntlessCourageDictionary.ContainsKey(level))
                            dauntlessCourageDictionary.Add(level, new Dictionary<uint, float>());

                        dauntlessCourageDictionary[level].Add(skillLevel, effectiveness);
                    }
                }
                else
                    WriteToErrorLog(file + " row " + i + " in ParseSupportConfig doesn't have 3 columns");
            }
        }

        public static void ParseSimpleKeyValue(string file, FileType ft)
        {
            string[] lines = File.ReadAllLines(file);
            string correctText = string.Empty;
            for (int i = 0; i < lines.Length; ++i)
            {
                string[] split = lines[i].Split(",");

                if (split.Length == 2)
                {
                    if (ft == FileType.STATS_DISPLAY)
                        statsNumberDisplay.Add(int.Parse(split[0]), int.Parse(split[1]));
                    else if (ft == FileType.HARP_TOME_SKILL_LEVEL_CONFIG)
                        harpTomeSkillLevel.Add(uint.Parse(split[0]), float.Parse(split[1]));
                    else if (ft == FileType.STAFF_SKILL_LEVEL_CONFIG)
                        staffSkillLevel.Add(uint.Parse(split[0]), float.Parse(split[1]));
                    else if (ft == FileType.DPS_SKILL_LEVEL_CONFIG)
                       dpsSkillLevel.Add(uint.Parse(split[0]), float.Parse(split[1]));
                }
                else
                    WriteToErrorLog(file + " row " + i + " in ParseSimpleKeyValue doesn't have 2 columns");
            }
        }

        public static void ParseSimpleHashSet(string file, FileType ft)
        {
            string[] lines = File.ReadAllLines(file);
            string correctText = string.Empty;
            for (int i = 0; i < lines.Length; ++i)
            {
                if (ft == FileType.COMBO_WEAPONS_DICTIONARY)
                    comboWeapons.Add(lines[i]);
            }
        }

        public static void ParseNames(string file, FileType ft)
        {
            string[] lines = File.ReadAllLines(file);
            string correctText = string.Empty;
            for (int i = 0; i < lines.Length; ++i)
            {
                string[] split = lines[i].Split(',');

                if (ft == FileType.INITIAL_STATS_FRIENDLY)
                    storeFriendlyNames.Add(split[1],int.Parse(split[6]) > 0 );
                else
                    storeEnemyNames.Add(split[1], int.Parse(split[6]) > 0);

            }
        }

        public static bool CheckIfComboWeapon(string weaponIn)
        {
            return comboWeapons.Contains(weaponIn);
        }

        public static bool CheckIfWeaponExists(string weaponIn)
        {
            return weaponDictionary.ContainsKey(weaponIn);
        }

        public static bool IsFriendly(string name)
        {
            return storeFriendlyNames.ContainsKey(name);
        }

        public static bool IsEnemy(string name)
        {
            return storeEnemyNames.ContainsKey(name);
        }

        public static Dictionary<string,bool> GetAllFriendlyNames()
        {
            return storeFriendlyNames;
        }
        public static Dictionary<string, bool> GetAllEnemyNames()
        {
            return storeEnemyNames;
        }

        public static bool isPlayerVG(string name)
        {
            if (storeFriendlyNames.ContainsKey(name))
                return storeFriendlyNames[name];

            if (storeEnemyNames.ContainsKey(name))
                return storeEnemyNames[name];

            WriteToErrorLog("Is VG name not found: " + name);
            return false;
        }

        public static Tuple<Stats, ElementalType, WeaponType> GetBuffDebuffWeaponEffectiveness(string playerName, WeaponDetails weaponDetails, float boonMultiplier = 1f)
        {
            //We will use a modification of the buff/debuff formula as this used for colo prep
            //Reference Stat * Main skill multiplier * main skill level multiplier * 0.05 * targets * SB mod
            if(initialStats.ContainsKey(playerName))
            {
                if (weaponDictionary.ContainsKey(weaponDetails.GetWeaponName()))
                {
                    Weapon weapon = weaponDictionary[weaponDetails.GetWeaponName()];

                        Stats stats = initialStats[playerName].GetCurrentStats();
                        uint mainSkillLevel = weaponDetails.GetColoSkillLevel();

                        return new Tuple<Stats,ElementalType,WeaponType>(new Stats((int)(stats.patk * weapon.patkValue * harpTomeSkillLevel[mainSkillLevel] * 0.05f * weapon.targets * boonMultiplier),
                                         (int)(stats.pdef * weapon.pdefValue * harpTomeSkillLevel[mainSkillLevel] * 0.05f * weapon.targets * boonMultiplier),
                                         (int)(stats.matk * weapon.matkValue * harpTomeSkillLevel[mainSkillLevel] * 0.05f * weapon.targets * boonMultiplier),
                                         (int)(stats.mdef * weapon.mdefValue * harpTomeSkillLevel[mainSkillLevel] * 0.05f * weapon.targets * boonMultiplier)),
                                         weapon.elementalType, weapon.weaponType);
                }
                else
                {
                    WriteToErrorLog("GetWeaponEffectiveness weaponName not found: " + weaponDetails.GetWeaponName());
                    return new Tuple<Stats, ElementalType, WeaponType>(new Stats(0, 0, 0, 0), ElementalType.NONE, WeaponType.NONE);
                }
            }
            else
            {
                WriteToErrorLog("GetWeaponEffectiveness playerName not found: " + playerName);
                return new Tuple<Stats, ElementalType, WeaponType>(new Stats(0, 0, 0, 0), ElementalType.NONE, WeaponType.NONE);
            }
        }

        public static Tuple<Stats, ElementalType, WeaponType> GetStaffWeaponEffectiveness(string playerName, WeaponDetails weaponDetails, float RSMultiplier = 1f)
        {
            //We will use a modification of the buff/debuff formula as this used for colo prep
            //Total Def * RSMultiplier * main skill multiplier * main skill level multiplier * 0.05 * targets
            if (initialStats.ContainsKey(playerName))
            {
                if (weaponDictionary.ContainsKey(weaponDetails.GetWeaponName()))
                {
                    Weapon weapon = weaponDictionary[weaponDetails.GetWeaponName()];


                        Stats stats = initialStats[playerName].GetCurrentStats();
                        uint mainSkillLevel = weaponDetails.GetColoSkillLevel();

                        return new Tuple<Stats, ElementalType, WeaponType>(new Stats(
                                         0,
                                         (int)(stats.pdef * weapon.recovery * staffSkillLevel[mainSkillLevel] * 0.05f * weapon.targets * RSMultiplier),
                                         0,
                                         (int)(stats.mdef * weapon.recovery * staffSkillLevel[mainSkillLevel] * 0.05f * weapon.targets * RSMultiplier)),
                                         weapon.elementalType, weapon.weaponType);
 
                }
                else
                {
                    WriteToErrorLog("GetWeaponEffectiveness weaponName not found: " + weaponDetails.GetWeaponName());
                    return new Tuple<Stats, ElementalType, WeaponType>(new Stats(0, 0, 0, 0), ElementalType.NONE, WeaponType.NONE);
                }
            }
            else
            {
                WriteToErrorLog("GetWeaponEffectiveness playerName not found: " + playerName);
                return new Tuple<Stats, ElementalType, WeaponType>(new Stats(0, 0, 0, 0), ElementalType.NONE, WeaponType.NONE);
            }
        }

        public static Tuple<Stats, ElementalType, WeaponType> GetOtherWeaponEffectiveness(string playerName, WeaponDetails weaponDetails, float multiplier = 1f)
        {
            if (initialStats.ContainsKey(playerName))
            {
                if (weaponDictionary.ContainsKey(weaponDetails.GetWeaponName()))
                {
                    Weapon weapon = weaponDictionary[weaponDetails.GetWeaponName()];

                    Stats stats = initialStats[playerName].GetCurrentStats();
                    uint mainSkillLevel = weaponDetails.GetColoSkillLevel();

                    return new Tuple<Stats, ElementalType, WeaponType>(new Stats(0, 0, 0, 0), weapon.elementalType, weapon.weaponType);
                }
                else
                {
                    WriteToErrorLog("GetWeaponEffectiveness weaponName not found: " + weaponDetails.GetWeaponName());
                    return new Tuple<Stats, ElementalType, WeaponType>(new Stats(0, 0, 0, 0), ElementalType.NONE, WeaponType.NONE);
                }
            }
            else
            {
                WriteToErrorLog("GetWeaponEffectiveness playerName not found: " + playerName);
                return new Tuple<Stats, ElementalType, WeaponType>(new Stats(0, 0, 0, 0), ElementalType.NONE, WeaponType.NONE);
            }
        }

        public static Tuple<int, ElementalType, WeaponType> GetVGWeaponEffectiveness(string playerName, WeaponDetails weaponDetails, int defence = 0, float DCmultiplier = 1f, CharClass characterClass = CharClass.NONE, 
                                                                                     uint overrideMainSkillLevel = 0, bool setTargetToOne = true)
        {
            //We will use a modification of the buff/debuff formula as this used for colo prep
            // (PATK/MATK - (defence * 0.667)) * DCMultuplier * main skill multiplier * main skill level multiplier * 0.05 * 0.95 * targets
            if (initialStats.ContainsKey(playerName))
            {
                if (weaponDictionary.ContainsKey(weaponDetails.GetWeaponName()))
                {
                    Weapon weapon = weaponDictionary[weaponDetails.GetWeaponName()];

                    Stats stats = initialStats[playerName].GetCurrentStats();
                    uint mainSkillLevel = overrideMainSkillLevel > 0 ? overrideMainSkillLevel : weaponDetails.GetColoSkillLevel();
                    float damageMod = GetDamageModifier(weapon.weaponType,characterClass);

                    float targets = setTargetToOne ? 1f : weapon.targets;


                    if (weapon.weaponType == WeaponType.HAMMER || weapon.weaponType == WeaponType.SWORD)
                        return new Tuple<int, ElementalType, WeaponType>(
                                         (int)( (stats.patk - (defence * 0.66667f)) * weapon.damage * dpsSkillLevel[mainSkillLevel] * 0.05 *  0.95 * targets * DCmultiplier * damageMod),
                                         weapon.elementalType, weapon.weaponType);

                    else if (weapon.weaponType == WeaponType.SPEAR || weapon.weaponType == WeaponType.BOW)
                        return new Tuple<int, ElementalType, WeaponType>(
                                         (int)((stats.matk - (defence * 0.66667f)) * weapon.damage * dpsSkillLevel[mainSkillLevel] * 0.05 * 0.95 * targets * DCmultiplier * damageMod),
                                         weapon.elementalType, weapon.weaponType);

                    return new Tuple<int, ElementalType, WeaponType>(0, ElementalType.NONE, WeaponType.NONE);
                }
                else
                {
                    WriteToErrorLog("GetWeaponEffectiveness weaponName not found: " + weaponDetails.GetWeaponName());
                    return new Tuple<int, ElementalType, WeaponType>(0, ElementalType.NONE, WeaponType.NONE);
                }
            }
            else
            {
                WriteToErrorLog("GetWeaponEffectiveness playerName not found: " + playerName);
                return new Tuple<int, ElementalType, WeaponType>(0, ElementalType.NONE, WeaponType.NONE);
            }
        }

        static float GetDamageModifier(WeaponType wt, CharClass charClass)
        {
            if (charClass == CharClass.BREAKER)
                return wt == WeaponType.SWORD ? 1.1f : 1f;

            if (charClass == CharClass.CRUSHER)
                return wt == WeaponType.HAMMER ? 1.1f : 1f;

            if (charClass == CharClass.PALADIN)
                return wt == WeaponType.SPEAR ? 1.1f : 1f;

            if (charClass == CharClass.CRUSHER_HNM)
            {
                if (wt == WeaponType.HAMMER)
                    return 1.35f;

                if (wt == WeaponType.BOW)
                    return 1;

                return 0.25f;
            }

            if (charClass == CharClass.PALADIN_HNM)
            {
                if (wt == WeaponType.SPEAR)
                    return 1.35f;

                if (wt == WeaponType.SWORD)
                    return 1;

                return 0.25f;
            }

            return 1f;
        }


        public static float ExtractSupportBoonBonus(Dictionary<string, WeaponDetails> fullWeaponInfo)
        {
            float currentBonus = 1f;
            foreach(KeyValuePair<string,WeaponDetails> kvp in fullWeaponInfo)
            {
                string skillName = kvp.Value.GetSkillName();

                if(skillName.Contains("Support Boon"))
                {
                    uint supportLevel = kvp.Value.GetColoSupportLevel();

                    if (skillName.Contains("(I)"))
                        currentBonus += supportBoonDictionary[1][supportLevel];
                    else if (skillName.Contains("(II)"))
                        currentBonus += supportBoonDictionary[2][supportLevel];
                    else if (skillName.Contains("(III)"))
                        currentBonus += supportBoonDictionary[3][supportLevel];
                }
            }

            return currentBonus;
        }

        public static float ExtractRecoverySupportBonus(Dictionary<string, WeaponDetails> fullWeaponInfo)
        {
            float currentBonus = 1f;
            foreach (KeyValuePair<string, WeaponDetails> kvp in fullWeaponInfo)
            {
                string skillName = kvp.Value.GetSkillName();

                if (skillName.Contains("Recovery Support"))
                {
                    uint supportLevel = kvp.Value.GetColoSupportLevel();

                    if (skillName.Contains("(I)"))
                        currentBonus += supportBoonDictionary[1][supportLevel];
                    else if (skillName.Contains("(II)"))
                        currentBonus += supportBoonDictionary[2][supportLevel];
                }
            }

            return currentBonus;
        }

        public static float ExtractDauntlessCourageBonus(Dictionary<string, WeaponDetails> fullWeaponInfo)
        {
            float currentBonus = 1f;
            foreach (KeyValuePair<string, WeaponDetails> kvp in fullWeaponInfo)
            {
                string skillName = kvp.Value.GetSkillName();

                if (skillName.Contains("Dauntless Courage"))
                {
                    uint supportLevel = kvp.Value.GetColoSupportLevel();

                    if (skillName.Contains("(I)"))
                        currentBonus += dauntlessCourageDictionary[1][supportLevel];
                    else if (skillName.Contains("(II)"))
                        currentBonus += dauntlessCourageDictionary[2][supportLevel];
                    else if (skillName.Contains("(III)"))
                        currentBonus += dauntlessCourageDictionary[3][supportLevel];
                }
            }

            return currentBonus;
        }

        public static WeaponType GetWeaponType(string weaponName)
        {
            if (weaponDictionary.ContainsKey(weaponName))
                return weaponDictionary[weaponName].weaponType;
            else
            {
                //Need to log this once the utility logger is out
                WriteToErrorLog("Weapon dictionary does not contain " + weaponName);
                return WeaponType.NONE;
            }
                
        }

    }
}
