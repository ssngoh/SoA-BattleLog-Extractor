using System;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace SinoParser
{
    class Program
    {
        static void Main(string[] args)
        {

            Parser parser = new Parser();
            string prefix = string.Empty;

            //Used when running from VS. If we build an exe, the 2 root directories code will be different. (TBC)
            //This gives us Sinoalice SQL\SinoParser\SinoParser
            //string rootProjectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;// "C:/Users/Onsla/OneDrive/Desktop/Sinoalice SQL/Images"; //Environment.CurrentDirectory;

            //This gives us Sinoalice SQL
            //string rootDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent.FullName;

            //string parseDirectory = "C:/Users/Onsla/OneDrive/Desktop/Sinoalice SQL/Images/ToParse";
            //string inputDirectory = "\"" + "C:/Users/Onsla/OneDrive/Desktop/Sinoalice SQL/Images/Input" + "\"";
            //string processDirectory = "\"" +"C:/Users/Onsla/OneDrive/Desktop/Sinoalice SQL/Images/Output" + "\"" ;


            //string parseDirectory = rootDirectory +  "/Images/ToParse";
            //string inputDirectory = "\"" + rootDirectory + "/Images/Input" + "\"";
            //string processDirectory = "\"" + rootDirectory + "/Images/Output" + "\"";
            //string configDirectory = rootProjectDirectory + "/Config";
            //string parseDirectory = rootDirectory + "/Images/ToParse";
            //string inputDirectory = "\"" + configDirectory + "/Input" + "\"";
            //string processDirectory = "\"" + configDirectory + "/Output" + "\"";

            string[] directories = Directory.GetDirectories(Utilities.parseDirectory, "", SearchOption.TopDirectoryOnly);

            var directoryList = directories.OrderBy(x => x).ToList();
            
            if(directoryList.Count > 0)
                prefix = directoryList[0].Substring(0, directoryList[0].LastIndexOf('\\')+1);

            for(int i = 0; i < directoryList.Count; ++i)
                directoryList[i] = directoryList[i].Substring(directoryList[i].LastIndexOf('\\')+1, directoryList[i].Length - directoryList[i].LastIndexOf('\\')-1);

            directoryList = directoryList.OrderBy(x => int.Parse(x)).ToList(); //Sort them to numerical instead of lexicographic

            for (int i = 0; i < directoryList.Count; ++i)
            {
                directoryList[i] = string.Concat(prefix, directoryList[i]);
            }

            parser.ParseConfigFile(Utilities.configDirectory + "/Input/friendNameSelfCorrectingConfig.txt", FileType.NAME_SELF_CORRECTING);
            parser.ParseConfigFile(Utilities.configDirectory + "/Input/enemyNameSelfCorrectingConfig.txt", FileType.NAME_SELF_CORRECTING);
            parser.ParseConfigFile(Utilities.configDirectory + "/Input/nameFilterConfig.txt", FileType.NAME_FILTER);
            parser.ParseConfigFile(Utilities.configDirectory + "/Input/skillsDictionary.txt", FileType.SKILL_DICTIONARY);
            parser.ParseConfigFile(Utilities.configDirectory + "/Input/skillsCorrectionDictionary.txt", FileType.SKILL_CORRECTION_DICTIONARY);
            parser.ParseConfigFile(Utilities.configDirectory + "/Input/weaponsDictionary.txt", FileType.WEAPONS_DICTIONARY);
            parser.ParseConfigFile(Utilities.configDirectory + "/Input/weaponsCorrectionDictionary.txt", FileType.WEAPONS_CORRECTION_DICTIONARY);

            parser.ParseConfigFile(Utilities.configDirectory + "/Input/bonusComboWeaponsDictionary.txt", FileType.COMBO_WEAPONS_DICTIONARY);

            parser.ParseConfigFile(Utilities.configDirectory + "/Constants/skillNumbersCorrectingConfig.txt", FileType.SKILL_NUMBERS_SELF_CORRECTING);
            parser.ParseConfigFile(Utilities.configDirectory + "/Constants/baseStatsSelfCorrectingConfig.txt", FileType.RELATION_SELF_CORRECTING);
            parser.ParseConfigFile(Utilities.configDirectory + "/Constants/tomeDampingConfig.txt", FileType.TOME_EFFECTIVENESS);
            parser.ParseConfigFile(Utilities.configDirectory + "/Constants/harpDampingConfig.txt", FileType.HARP_EFFECTIVENESS);
            parser.ParseConfigFile(Utilities.configDirectory + "/Constants/statsDisplayConfig.txt", FileType.STATS_DISPLAY);
            parser.ParseConfigFile(Utilities.configDirectory + "/Constants/staffSkillLevelConfig.txt", FileType.STAFF_SKILL_LEVEL_CONFIG);
            parser.ParseConfigFile(Utilities.configDirectory + "/Constants/harpTomeSkillLevelConfig.txt", FileType.HARP_TOME_SKILL_LEVEL_CONFIG);
            parser.ParseConfigFile(Utilities.configDirectory + "/Constants/dpsSkillLevelConfig.txt", FileType.DPS_SKILL_LEVEL_CONFIG);
            parser.ParseConfigFile(Utilities.configDirectory + "/Constants/supportBoonConfig.txt", FileType.SUPPORT_BOON_CONFIG);
            parser.ParseConfigFile(Utilities.configDirectory + "/Constants/recoverySupportConfig.txt", FileType.RECOVERY_SUPPORT_CONFIG);
            parser.ParseConfigFile(Utilities.configDirectory + "/Constants/dauntlessCourageConfig.txt", FileType.DAUNTLESS_COURAGE_CONFIG);

            #region ADVANCED_FUNC
            //Google sheet method. Just run the process once to download the google sheets to populate the 3 text files below
            //If you don't have this setup yet, you can just manually key in the details in these files
            //Process.Start(Utilities.configDirectory + "/Input/downloader.bat", Utilities.inputDirectory).WaitForExit();

            //For ExportColoCombatDetails and ExportWeaponHash to includeWeaponSummary, we need to first grab the initial stats.
            //If you have these text files provided, you can uncomment the 2 lines
            //parser.ParseConfigFile(Utilities.configDirectory + "/Input/enemyInitialStatsConfig.txt", FileType.INITIAL_STATS_ENEMY);
            //parser.ParseConfigFile(Utilities.configDirectory + "/Input/friendInitialStatsConfig.txt", FileType.INITIAL_STATS_FRIENDLY);

            #endregion

            //Get inner directory
            //for (int i = 0; i < directoryList.Count; ++i)
            for (int i = directoryList.Count-1; i >= 0; --i)  
            {
                if (!parser.ParseFile("/tesseractOutput.txt", directoryList[i]))
                {
                    Console.WriteLine("Error parsing " + directoryList[i] + "/tesseractOutput.txt");
                }
            }

            //Use either the Basic functionality or the advanced functionality.
            #region BASIC_FUNC
            parser.ExportRelationshipTable();
            parser.ExportRelationshipCorrectionTable();
            parser.ExportWeaponHash(false);
            #endregion


            #region ADVANCED_FUNC
            //parser.ExportRelationshipTable();
            //parser.ExportRelationshipCorrectionTable();
            //parser.ExportWeaponHash(true);
            //parser.ExportWeaponElementalSummary();
            //parser.ExportColoCombatDetails();
            //parser.ExportIndividualCombatDetails();
            #endregion

            Utilities.CloseFile();

            //Process.Start(rootDirectory + "/Output/csvToExcelRunner.bat", processDirectory).WaitForExit();
            Process.Start(Utilities.configDirectory + "/Output/csvToExcelRunner.bat", Utilities.processDirectoryForArg).WaitForExit();


        }

    }
}
