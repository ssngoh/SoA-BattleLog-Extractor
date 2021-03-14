using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Collections;

namespace SinoParser
{
    enum BlockType
    {
        GUILDSHIP,
        COUNTER_SHIELD,
        NORMAL,
        SUMMON_PREP,
        SUMMON,
        REVIVE,
        SWAP,
        SELF_REVIVE,
        CHANGED_GEAR_SET,
        UNKNOWN, //we will need to log this
    }

    public enum FileType
    {
        WEAPONS_DICTIONARY,
        WEAPONS_CORRECTION_DICTIONARY,
        SKILL_DICTIONARY,
        SKILL_CORRECTION_DICTIONARY,
        SKILL_NUMBERS_SELF_CORRECTING,
        NAME_SELF_CORRECTING,
        RELATION_SELF_CORRECTING,
        NAME_FILTER,
        INITIAL_STATS_ENEMY,
        INITIAL_STATS_FRIENDLY,
        TOME_EFFECTIVENESS,
        HARP_EFFECTIVENESS,
        STATS_DISPLAY,
        STAFF_SKILL_LEVEL_CONFIG,
        HARP_TOME_SKILL_LEVEL_CONFIG,
        DPS_SKILL_LEVEL_CONFIG,
        SUPPORT_BOON_CONFIG,
        RECOVERY_SUPPORT_CONFIG,
        COMBO_WEAPONS_DICTIONARY,
        DAUNTLESS_COURAGE_CONFIG,
    }

    enum BlockState
    {
        HEADER_NOT_FOUND = 0,
        HEADER_FOUND,
        WEAPON_FOUND,
        CHECKING_SUPPORT_SKILLS,
        ADDING_RELATION_STATS,
        LAST_LINE,
    }

    enum StatsRelation
    {
        NONE,
        RECOVER,
        PATK_UP,
        PATK_DOWN,
        PDEF_UP,
        PDEF_DOWN,
        MATK_UP,
        MATK_DOWN,
        MDEF_UP,
        MDEF_DOWN,
        DAMAGE
    }

 

    class Parser
    {
        private string[] gv_lines;
        private string[] gv_lastFileLines;

        //First string is the players
        //Second string is who this player has affected
        //Relation is what kind of relation.
        //eg. Arco atk up 100 for Ibis
        // This would mean in Arco Key -> Ibis Key -> relation patk + 100
        private Dictionary<string, Dictionary<string, Relation>> gv_relationDictionary = new Dictionary<string, Dictionary<string, Relation>>();

        //Outer key is name of player as well as is grid swapped. Inner key is weapon name
        private Dictionary<(string,bool), Dictionary<string,WeaponDetails>> gv_weaponHash = new Dictionary<(string,bool), Dictionary<string,WeaponDetails>>();

        //Stores all action blocks for the entire colo battle
        private List<StoreColoCombatDetails> gv_coloEntireCombatDetails = new List<StoreColoCombatDetails>();
        private Dictionary<string, List<StoreColoCombatDetails>> gv_individualColoCombatDetails = new Dictionary<string, List<StoreColoCombatDetails>>();
        
        //Store which grids were changed this block. We need to store this because the blocks are read from latest to earliest
        private Dictionary<string, bool> gv_gridSwappedThisBlock = new Dictionary<string, bool>();
        private Dictionary<(string, bool), Dictionary<string, WeaponDetails>> gv_weaponsAddedThisBlock = new Dictionary<(string, bool), Dictionary<string, WeaponDetails>>();

        private HashSet<string> gv_skillsDictionary = new HashSet<string>();
        private Dictionary<string, string> gv_relationCorrectionDictionary = new Dictionary<string, string>();
        private Dictionary<string,string> gv_skillsCorrectionDictionary = new Dictionary<string, string>();
        private Dictionary<string, string> gv_weaponsCorrectionDictionary = new Dictionary<string, string>();
        private Dictionary<string, string> gv_skillNumbersSelfCorrectionDictionary = new Dictionary<string, string>();
        private Dictionary<string, string> gv_nameSelfCorrectionDictionary = new Dictionary<string, string>();
        private Dictionary<string, bool> gv_isGridSwapped = new Dictionary<string, bool>();
        
        private HashSet<string> gv_nameFilterDictionary = new HashSet<string>();

        private string gv_folder;

        //Analyse variables
        int gv_blockCount = 0;
        bool gv_firstLineOfBlock = false;
        bool gv_skipBlock = false;
        string gv_name = string.Empty;
        string gv_time = string.Empty;
        string gv_holdString = string.Empty;
        BlockType gv_bt = BlockType.UNKNOWN;
        BlockState gv_blockState = BlockState.HEADER_NOT_FOUND;

        //Last file lines analyse variables
        int gv_lastFileBlocksFound = 0;

        List<Tuple<string, StatsRelation>> gv_stringToStatsRelation = new List<Tuple<string, StatsRelation>>();

        //Stores the current block for combat detail. We need this because combat details read are in descending order.
        //So we store this then read from the end to beginning
        List<StoreColoCombatDetails> gv_storeColoCombatDetailsForBlock = new List<StoreColoCombatDetails>();

        //Stores how much buff/debuff each player has at any point of time. 
        Dictionary<string, StoreColoStatsDetails> gv_storeColoCurrentStatsDetails = new Dictionary<string, StoreColoStatsDetails>();

        //Guildship debuff reset variables
        bool gv_skipBlockBecauseOfGuildship = false;
        string gv_guildshipBlockName = string.Empty;

        //Relation store variables
        string gv_storeWeaponString = string.Empty;
        List<string> gv_storeSupportWeaponString = new List<string>();
        List<StoreColoStats> gv_storeRelationStats = new List<StoreColoStats>();

        //Store previous relation variables. Used to check for duplicates
        //List<StoreColoStats> gv_storePreviousRelationStats = new List<StoreColoStats>();
        Queue<StoreColoStats> gv_storePreviousRelationStats = new Queue<StoreColoStats>();

        //For exporting
        Exporter gv_Exporter;

        public Parser() 
        {
            gv_Exporter = new Exporter();

            //These needs to be changed to a config file
            string atkUpBy = "ATK UP by";
            string atkDownBy = "ATK DOWN by";
            string defUpBy = "DEF UP by";
            string defDownBy = "DEF DOWN by";

            string mAtkUpBy = "M.ATK UP by";
            string mAtkDownBy = "M.ATK DOWN by";
            string mDefUpBy = "M.DEF UP by";
            string mDefDownBy = "M.DEF DOWN by";

            string hpRecoveredBy = "HP recovered by";
            string damageTo = "damage to";

            gv_stringToStatsRelation.Add(new Tuple<string, StatsRelation>(mAtkUpBy, StatsRelation.MATK_UP));
            gv_stringToStatsRelation.Add(new Tuple<string, StatsRelation>(mAtkDownBy, StatsRelation.MATK_DOWN));
            gv_stringToStatsRelation.Add(new Tuple<string, StatsRelation>(mDefUpBy, StatsRelation.MDEF_UP));
            gv_stringToStatsRelation.Add(new Tuple<string, StatsRelation>(mDefDownBy, StatsRelation.MDEF_DOWN));
            gv_stringToStatsRelation.Add(new Tuple<string, StatsRelation>(atkUpBy, StatsRelation.PATK_UP));
            gv_stringToStatsRelation.Add(new Tuple<string, StatsRelation>(atkDownBy, StatsRelation.PATK_DOWN));
            gv_stringToStatsRelation.Add(new Tuple<string, StatsRelation>(defUpBy, StatsRelation.PDEF_UP));
            gv_stringToStatsRelation.Add(new Tuple<string, StatsRelation>(defDownBy, StatsRelation.PDEF_DOWN));
            gv_stringToStatsRelation.Add(new Tuple<string, StatsRelation>(hpRecoveredBy, StatsRelation.RECOVER));
            gv_stringToStatsRelation.Add(new Tuple<string, StatsRelation>(damageTo, StatsRelation.DAMAGE));
        }

        public void ParseInitialStats(string file, FileType ft)
        {
            gv_lines = File.ReadAllLines(file);
            string correctText = string.Empty;
            for (int i = 0; i < gv_lines.Length; ++i)
            {
                string[] split = gv_lines[i].Split(','); 

                if (split.Length == 7)
                {
                    gv_storeColoCurrentStatsDetails.Add(split[1], new StoreColoStatsDetails(int.Parse(split[2]), int.Parse(split[4]), int.Parse(split[3]), int.Parse(split[5]), int.Parse(split[6])));
                    Utilities.AddInitialStats(split[1], new StoreColoStatsDetails(gv_storeColoCurrentStatsDetails[split[1]]));
                }
                else
                    WriteToErrorLog("row " + i + " in initialStatsConfig doesn't have 7 columns");
            }
        }

        public void ParseConfigFile(string file, FileType ft)
        {
            if (File.Exists(file))
            {
                if (ft == FileType.INITIAL_STATS_ENEMY || ft == FileType.INITIAL_STATS_FRIENDLY)
                {
                    ParseInitialStats(file,ft);
                    Utilities.ParseNames(file, ft);
                    return;
                }
                else if (ft == FileType.HARP_EFFECTIVENESS || ft == FileType.TOME_EFFECTIVENESS)
                {
                    Utilities.ParseBuffDebuffDampening(file, ft);
                    return;
                }
                else if(ft == FileType.STATS_DISPLAY || ft == FileType.HARP_TOME_SKILL_LEVEL_CONFIG || ft == FileType.STAFF_SKILL_LEVEL_CONFIG
                     || ft == FileType.DPS_SKILL_LEVEL_CONFIG)
                {
                    Utilities.ParseSimpleKeyValue(file, ft);
                    return;
                }
                else if(ft == FileType.WEAPONS_DICTIONARY)
                {
                    Utilities.ParseWeaponsDictionary(file);
                }
                else if (ft == FileType.SUPPORT_BOON_CONFIG || ft == FileType.RECOVERY_SUPPORT_CONFIG || ft == FileType.DAUNTLESS_COURAGE_CONFIG)
                {
                    Utilities.ParseSupportConfig(file, ft);
                }
                else if(ft == FileType.COMBO_WEAPONS_DICTIONARY)
                {
                    Utilities.ParseSimpleHashSet(file, ft);
                }


                gv_lines = File.ReadAllLines(file);
                string correctText = string.Empty;
                for (int i = 0; i < gv_lines.Length; ++i)
                {
                    if(gv_lines[i].StartsWith('*'))
                    {
                        correctText = gv_lines[i].Substring(1, gv_lines[i].Length - 1);

                        switch (ft)
                        { 
                            case FileType.SKILL_DICTIONARY:
                                gv_skillsDictionary.Add(correctText);
                                break;

                            case FileType.NAME_FILTER:
                                gv_nameFilterDictionary.Add(correctText);
                                break;

                            default:
                                break;
                        }

                       // Console.WriteLine("Correct text is " + correctText);
                    }
                    else
                    {
                        switch (ft)
                        {
                            case FileType.SKILL_NUMBERS_SELF_CORRECTING:
                                if(!gv_skillNumbersSelfCorrectionDictionary.ContainsKey(gv_lines[i]))
                                    gv_skillNumbersSelfCorrectionDictionary.Add(gv_lines[i], correctText);
                                break;

                            case FileType.NAME_SELF_CORRECTING:
                                if (!gv_nameSelfCorrectionDictionary.ContainsKey(gv_lines[i]))
                                    gv_nameSelfCorrectionDictionary.Add(gv_lines[i], correctText);
                                break;

                            case FileType.SKILL_CORRECTION_DICTIONARY:
                                if (!gv_skillsCorrectionDictionary.ContainsKey(gv_lines[i]))
                                    gv_skillsCorrectionDictionary.Add(gv_lines[i], correctText);
                                break;

                            case FileType.WEAPONS_CORRECTION_DICTIONARY:
                                if (!gv_weaponsCorrectionDictionary.ContainsKey(gv_lines[i]))
                                    gv_weaponsCorrectionDictionary.Add(gv_lines[i], correctText);
                                break;

                            case FileType.RELATION_SELF_CORRECTING:
                                if (!gv_relationCorrectionDictionary.ContainsKey(gv_lines[i]))
                                    gv_relationCorrectionDictionary.Add(gv_lines[i], correctText);
                                break;

                            default:
                                break;
                        }     
                    }
                }
            }
        }

        public bool ParseFile(string file, string folder)
        {
            file = folder + file;

            if (!File.Exists(file))
                return false;

            gv_lines = File.ReadAllLines(file);
            gv_lines = gv_lines.Where(x => x.Length > 0 && x != "\f").ToArray();

            gv_folder = folder;

            Console.WriteLine("File is " + file);

            ResetAnalyseVariables();

            AnalyseLines();


            PopulateColoBattleData();
            gv_storeColoCombatDetailsForBlock.Clear();

            return true;
        }

        //Used for pre 8.0.0 version
        public bool ParseFile(string file, string folder, string lastFile)
        {
            file = folder + file;
            lastFile = folder + lastFile;

            if (!File.Exists(file))
                return false;

            if (!File.Exists(lastFile))
                return false;

            gv_lines = File.ReadAllLines(file);
            gv_lines = gv_lines.Where(x => x.Length > 0 && x != "\f").ToArray();

            gv_lastFileLines = File.ReadAllLines(lastFile);
            gv_lastFileLines = gv_lastFileLines.Where(x => x.Length > 0 && x != "\f").ToArray();

            gv_folder = folder;

            Console.WriteLine("File is " + file);

            //PreAnalyseLastLines();
            AnalyseLines();


            PopulateColoBattleData();
            gv_storeColoCombatDetailsForBlock.Clear();

            return true;
        }


        public void ExportWeaponHash(bool includeWeaponSummary = false)
        {
            gv_Exporter.ExportWeaponHash(gv_weaponHash, includeWeaponSummary);
        }

        public void ExportWeaponElementalSummary()
        {
            gv_Exporter.ExportWeaponElementalSummary(gv_weaponHash, true);
            gv_Exporter.ExportWeaponElementalSummary(gv_weaponHash, false);
        }
        public void ExportRelationshipTable()
        {
            gv_Exporter.ExportRelationshipTable(gv_relationDictionary);
        }

        public void ExportRelationshipCorrectionTable()
        {
            gv_Exporter.ExportRelationshipCorrectionTable(gv_relationDictionary);
        }

        public void ExportColoCombatDetails()
        {
            gv_Exporter.ExportColoCombatDetails(gv_coloEntireCombatDetails);
        }

        public void ExportIndividualCombatDetails()
        {
            gv_Exporter.ExportIndividualCombatDetails(gv_individualColoCombatDetails);
        }

        void WriteToErrorLog(string errorString)
        {
            Utilities.WriteToErrorLog(errorString, gv_folder);
        }

        private void ResetAnalyseVariables()
        {
            gv_blockCount = 0;
            gv_blockState = BlockState.HEADER_NOT_FOUND;
            gv_firstLineOfBlock = false;
            gv_skipBlock = false;
            gv_name = string.Empty;
            gv_time = string.Empty;
            gv_holdString = string.Empty;
            gv_storeWeaponString = string.Empty;
            gv_storeSupportWeaponString.Clear();
            gv_storeRelationStats.Clear();
            gv_bt = BlockType.UNKNOWN;
            gv_storeColoCombatDetailsForBlock.Clear();
            gv_storePreviousRelationStats.Clear();

            gv_gridSwappedThisBlock.Clear();
            gv_weaponsAddedThisBlock.Clear();

            gv_lastFileBlocksFound = 0;

        }

        private void ResetBlockVariables()
        {
            gv_holdString = string.Empty;
            gv_skipBlock = false;
            gv_blockState = BlockState.HEADER_FOUND;
            gv_firstLineOfBlock = true;
            gv_storeWeaponString = string.Empty;
            gv_storeSupportWeaponString.Clear();
            gv_storeRelationStats.Clear();

        }

        private void PreAnalyseLastLines()
        {
            bool headerFound = false;
            //We want to see how many full blocks we can capture here. That will tell us when to switch over to use this
            for(int i = 0; i < gv_lastFileLines.Length; ++i)
            {
                if (!headerFound)
                {
                    if (ContainsTime(gv_lastFileLines[i]))
                    {
                        string[] splitLine = gv_lastFileLines[i].Split(' ');
                        if (splitLine.Length > 2)
                        {
                            ++gv_lastFileBlocksFound;
                            headerFound = true;
                        }
                    }
                }

                if (headerFound)
                {
                    if ((i + 1 < gv_lastFileLines.Length) && ContainsTime(gv_lastFileLines[i + 1]))
                    {
                        headerFound = false;
                    }
                }
            }
        }
  
        private void AnalyseLines()
        {
            string[] linesToAnalyse;
            linesToAnalyse = gv_lines;
           

            //The only thing certain is if the line contains time, its the start of block.
            //If we failed to parse finish the previous block, then we need to put into the unconfirmed class for debugging
            for (int i = 0; i < linesToAnalyse.Length; ++i)
            {
                //Always get header first. Will always consists of time
                if (gv_blockState == BlockState.HEADER_NOT_FOUND)
                {
                    //We are in a block
                    //Because of counter shield, its possible that there is no name, but we still need to count it among the 30
                    if (ContainsTime(linesToAnalyse[i]))
                    {
                        gv_time = GetTime(linesToAnalyse[i]);
                        string[] splitLine = linesToAnalyse[i].Split(' ');
                        gv_name = string.Empty;
                        if (splitLine.Length > 2) 
                        {
                            if (splitLine.Length != 3)
                            {
                                for (int checkStr = 0; checkStr < splitLine.Length; ++checkStr)
                                {
                                    if (ContainsDate(splitLine[checkStr]))
                                        break;

                                    if (!CheckForNameBreak(splitLine[checkStr]) || !ContainsDate(splitLine[checkStr+1]))
                                        gv_name += splitLine[checkStr];
                                    else
                                        break;
                                }
                            }
                            else
                                gv_name = splitLine[0];
                            
                            CheckNameSelfCorrect(ref gv_name);
                        }
                        else
                        {
                            //Most likely its counter shield. Need to output to a file to print out uncertain stuff
                            WriteToErrorLog("Possible counter shield: " + linesToAnalyse[i]);
                        }

                        ResetBlockVariables();
                        ++gv_blockCount;
                    }
                    else
                        continue;
                }
                else
                {
                    //We can do some parsing here to already know what kind of block we are dealing with
                    if (gv_firstLineOfBlock)
                    {
                        gv_bt = CheckFirstLineOfBlock(linesToAnalyse[i]);

                        switch (gv_bt)
                        {
                            case BlockType.COUNTER_SHIELD:
                            case BlockType.REVIVE:
                            case BlockType.SELF_REVIVE:
                            case BlockType.SUMMON:
                            case BlockType.SUMMON_PREP:
                            case BlockType.SWAP:
                                gv_skipBlock = true; //No point getting data from them. Maybe only revive in the future
                                break;

                            case BlockType.GUILDSHIP: //We will just skip to the end for now
                                gv_skipBlock = true;
                                gv_skipBlockBecauseOfGuildship = true;
                                gv_guildshipBlockName = gv_name;
                                break;

                            case BlockType.UNKNOWN: //Need to log this down somewhere
                                gv_skipBlock = true;
                                break;

                            case BlockType.CHANGED_GEAR_SET:
                                //In case changing gear sets appears at the last line of the page, then we may have duplicates
                                 if(!gv_isGridSwapped.ContainsKey(gv_name))
                                    gv_isGridSwapped.Add(gv_name, true);

                                if (!gv_gridSwappedThisBlock.ContainsKey(gv_name))
                                    gv_gridSwappedThisBlock.Add(gv_name, true);

                                gv_skipBlock = true;
                                break;

                            case BlockType.NORMAL:
                                break;

                            default:  //If we miss out anything, we just ignore for now and log it down. Treat same as unknown   
                                break;
                        }
                    }

                    //If the next line contains time or current line contains mastery earned, it means we have hit the end of our block. 
                    //Wrap up whatever we have for this block and reset
                    // if ((i + 1 < linesToAnalyse.Length) && ContainsTime(linesToAnalyse[i + 1]) || i == linesToAnalyse.Length-1)
                    if ((i + 1 < linesToAnalyse.Length) && ContainsTime(linesToAnalyse[i + 1]) || i == linesToAnalyse.Length - 1 || ContainsMasteryEarned(linesToAnalyse[i]))
                    {
                        if (!gv_skipBlock)
                            gv_blockState = BlockState.LAST_LINE;
                        else
                            gv_blockState = BlockState.HEADER_NOT_FOUND;
                    }

                    if (gv_skipBlock)
                        continue;

                    if(gv_skipBlockBecauseOfGuildship)
                        ResetGuildshipDebuff();

                    //Check if we need to skip block
                    if (gv_firstLineOfBlock)
                    {
                        gv_firstLineOfBlock = false;
                        if (SkipLineofBlock(linesToAnalyse[i]))
                            continue;
                    }

                    //Proceed to get contents. Need to check if we are on a weapon line
                    if (gv_blockState == BlockState.HEADER_FOUND)
                    {
                        CheckSkillSelfCorrect(ref linesToAnalyse[i]);

                        string[] splitLine = linesToAnalyse[i].Split(' ');

                        if (string.Compare(splitLine[splitLine.Length - 1], "activated.") == 0) //May need self correct this as well
                        {
                            gv_blockState = BlockState.WEAPON_FOUND;
                            if (gv_holdString.Length > 0)
                                gv_holdString += " " + linesToAnalyse[i];
                            else
                                gv_holdString = linesToAnalyse[i];
                        }
                        else
                        {
                            gv_holdString = linesToAnalyse[i];
                            continue;
                        }
                    }

                    if (gv_blockState == BlockState.WEAPON_FOUND)
                    {
                        gv_storeWeaponString = gv_holdString;
                        gv_holdString = string.Empty;
                        gv_blockState = BlockState.CHECKING_SUPPORT_SKILLS;
                    }
                    else if (gv_blockState == BlockState.CHECKING_SUPPORT_SKILLS)
                    {
                        if (DoesLineContainStatsRelation(ref linesToAnalyse[i]) != StatsRelation.NONE) //We want to check damage to now as well
                        {
                            gv_blockState = BlockState.ADDING_RELATION_STATS;
                            gv_holdString = string.Empty;
                        }
                        else
                        {
                            //Most likely its a support skill.
                            CheckSkillSelfCorrect(ref linesToAnalyse[i]);

                            string[] splitLine = linesToAnalyse[i].Split(' ');

                            if (string.Compare(splitLine[splitLine.Length - 1], "activated.") == 0) //change to use a self corrector
                            {
                                if (gv_holdString.Length > 0)
                                    gv_holdString += " " + linesToAnalyse[i];
                                else
                                    gv_holdString = linesToAnalyse[i];

                                //ParseWeapon(holdString, false);
                                gv_storeSupportWeaponString.Add(gv_holdString);

                                gv_holdString = string.Empty;
                            }
                            else
                            {
                                gv_holdString = linesToAnalyse[i];
                                continue;
                            }
                        }
                    }

                    if (gv_blockState == BlockState.ADDING_RELATION_STATS)
                    {
                        StatsRelation sr = DoesLineContainStatsRelation(ref linesToAnalyse[i]);

                        if (sr == StatsRelation.NONE)
                        {
                            //WriteToErrorLog("DoesLineContainStatsRelation check: " + lines[i]);
                            continue;
                        }

                        if(sr == StatsRelation.DAMAGE)
                        {
                            //Ignore for now
                        }

                        gv_storeRelationStats.Add(new StoreColoStats(linesToAnalyse[i], sr));
                    }

                    if (gv_blockState == BlockState.LAST_LINE)
                    {
                        gv_blockState = BlockState.HEADER_NOT_FOUND;

                        //If mastery is found here, then we add our weapons or else ignore this block.
                        if (linesToAnalyse[i].Contains("mastery"))
                        {
                            if (gv_storePreviousRelationStats.Count > 0)
                            {
                                //We need to compare and check if our current relation is the same. 
                                //If it is means its a duplicate and we ignore the entire block (Not 100% foolproof but will work until we change the taking of screenshots
                                bool duplicate = false;
                                List<StoreColoStats> tempStoreColoStatsFromQueue = gv_storePreviousRelationStats.ToList();
                                for (int s = 0; s < gv_storeRelationStats.Count; ++s)
                                {
                                    for (int s2 = 0; s2 < tempStoreColoStatsFromQueue.Count; ++s2)
                                    {
                                        if (gv_storeRelationStats[s] == tempStoreColoStatsFromQueue[s2])
                                        {
                                            duplicate = true;
                                            break;
                                        }
                                    }

                                    if (duplicate)
                                        break;
                                }

                                //if (!duplicate)
                                //    storePreviousRelationStats.Clear();
                                //else
                                //{
                                //    WriteToErrorLog("Duplicate found " + linesToAnalyse[i]);
                                //    --blockCount;
                                //    continue;
                                //}

                                if(duplicate)
                                {
                                    //WriteToErrorLog("Duplicate found " + linesToAnalyse[i]);
                                    --gv_blockCount;
                                    continue;
                                }
                            }

                            if (gv_storeWeaponString.Length > 0)
                            {
                                InitializeCurrentBlockCombatDetails();
                                ParseWeapon(gv_storeWeaponString, true);
                            }
                            else
                                continue; //If there are no weapons, we ignore it because this means the block has issues as there is mastery earned

                            for (int s = 0; s < gv_storeSupportWeaponString.Count; ++s)
                                ParseWeapon(gv_storeSupportWeaponString[s], false);

                            for (int s = 0; s < gv_storeRelationStats.Count; ++s)
                            {
                                if (ParseRelation(gv_storeRelationStats[s].coloStatsString, gv_storeRelationStats[s].relation))
                                {
                                    if (gv_storePreviousRelationStats.Count >= 30)
                                        gv_storePreviousRelationStats.Dequeue();

                                    gv_storePreviousRelationStats.Enqueue(new StoreColoStats(gv_storeRelationStats[s]));
                                }
                            }
                                    
                        }
                        else
                        {
                            //WriteToErrorLog("Mastery not found " + linesToAnalyse[i]);
                            --gv_blockCount;
                        }

                        
                    }
                }  //End of else from header gotten check

            } //End of first for

            FixWeaponIfGridChanged();

        }

        StatsRelation DoesLineContainStatsRelation(ref string line)
        {
            //We only need this local because we use StatsRelation to check instead of M.DEf, ATK etc..
            CheckRelationSelfCorrect(ref line);
            
            for(int i = 0; i < gv_stringToStatsRelation.Count; ++i)
            {
                if (line.Contains(gv_stringToStatsRelation[i].Item1))
                    return gv_stringToStatsRelation[i].Item2;
            }

            return StatsRelation.NONE;
        }

        bool ParseRelation(string holdStringIn, StatsRelation relation)
        {
            //Contains something like this if passed into this function
            //Ibis's HP recovered by 12,810.
            //Ibis's M.ATK DOWN by 1,673.

            //Damage needs to be handled different because the format is different
            if (relation == StatsRelation.DAMAGE)
                return ParseDamage(holdStringIn);

            string holdString = holdStringIn;
            string targetedName = string.Empty;
            targetedName = CheckNameSelfCorrectForWeapons(holdString);

            if (targetedName.Length == 0)
                return false;

            string[] splitString = holdString.Split(' ');
            string amountString = splitString[splitString.Length - 1];
            amountString = amountString.Replace(",",string.Empty);

            uint amount;
            if (!uint.TryParse(amountString.Replace(".", string.Empty), out amount))
            {
                WriteToErrorLog("From ParseRelation " +holdStringIn);
                return false;
            }

            if (!gv_relationDictionary.ContainsKey(gv_name))
            {
                gv_relationDictionary.Add(gv_name, new Dictionary<string, Relation>());
                //WriteToErrorLog("From ParseRelation gv_name: " + gv_name + " added. " + "String is " + holdStringIn);
            }

            if (!gv_relationDictionary[gv_name].ContainsKey(targetedName))
                gv_relationDictionary[gv_name].Add(targetedName, new Relation());

            if (!gv_relationDictionary.ContainsKey(targetedName))
            {
                gv_relationDictionary.Add(targetedName, new Dictionary<string, Relation>());
                //WriteToErrorLog("From ParseRelation targeted_name: " + gv_name + " added. " + "String is " + holdStringIn);
            }

            if (!gv_relationDictionary[targetedName].ContainsKey(gv_name))
                gv_relationDictionary[targetedName].Add(gv_name, new Relation());

            switch (relation)
            {
                case StatsRelation.PATK_UP:
                    gv_relationDictionary[gv_name][targetedName].PAtk(amount, true, false);
                    gv_relationDictionary[targetedName][gv_name].PAtk(amount, true, true);
                    break;
                case StatsRelation.PATK_DOWN:
                    gv_relationDictionary[gv_name][targetedName].PAtk(amount, false, false);
                    gv_relationDictionary[targetedName][gv_name].PAtk(amount, false, true);
                    break;
                case StatsRelation.MATK_UP:
                    gv_relationDictionary[gv_name][targetedName].MAtk(amount, true, false);
                    gv_relationDictionary[targetedName][gv_name].MAtk(amount, true, true);
                    break;
                case StatsRelation.MATK_DOWN:
                    gv_relationDictionary[gv_name][targetedName].MAtk(amount, false, false);
                    gv_relationDictionary[targetedName][gv_name].MAtk(amount, false, true);
                    break;
                case StatsRelation.PDEF_UP:
                    gv_relationDictionary[gv_name][targetedName].PDef(amount, true, false);
                    gv_relationDictionary[targetedName][gv_name].PDef(amount, true, true);
                    break;
                case StatsRelation.PDEF_DOWN:
                    gv_relationDictionary[gv_name][targetedName].PDef(amount, false, false);
                    gv_relationDictionary[targetedName][gv_name].PDef(amount, false, true);
                    break;
                case StatsRelation.MDEF_UP:
                    gv_relationDictionary[gv_name][targetedName].MDef(amount, true, false);
                    gv_relationDictionary[targetedName][gv_name].MDef(amount, true, true);
                    break;
                case StatsRelation.MDEF_DOWN:
                    gv_relationDictionary[gv_name][targetedName].MDef(amount, false, false);
                    gv_relationDictionary[targetedName][gv_name].MDef(amount, false, true);
                    break;
                case StatsRelation.RECOVER:
                    gv_relationDictionary[gv_name][targetedName].Healing(amount,false);
                    gv_relationDictionary[targetedName][gv_name].Healing(amount,true);
                    break;

                default:
                    break;
            }

            UpdateColoCombatBlockRelationData(targetedName, amount, relation);

            return true;
        }

        bool ParseDamage(string holdStringIn)
        {
            //Contains something like this if passed into this function
            //3,212 damage to Ibis.
            //8,015 damage to 2 target(s). We ignore multiple targets
            //We will need to add in a damage formula. So that if its 2 targets, we output all hypothetical damage to all VGs 

            string[] splitString = holdStringIn.Split(' ');
            string amountString = splitString[0];
            amountString = amountString.Replace(",", string.Empty);
            uint amount = 0;
            if (!uint.TryParse(amountString.Replace(".", string.Empty), out amount))
            {
                WriteToErrorLog("From ParseDamage " + holdStringIn);
                return false;
            }

            if (holdStringIn.Contains("target(s)")) //Update this to use self correcting
            {
                //This should be a valid string but we don't need to parse multiple target data
                //Instead, next time we will generate all possible to damage to the 5 VGs when this happens
                //UpdateColoCombatBlockRelationData("", amount, StatsRelation.DAMAGE, true);
                return true;
            }
            
            int positionOfTo = holdStringIn.IndexOf("to");
            string targetedName = holdStringIn.Substring(positionOfTo + 3, holdStringIn.Length - (positionOfTo + 4));
            //string targetedName = splitString[splitString.Length-1].Substring(0, splitString[splitString.Length - 1].Length - 1); //Take away the last . (fullstop)
            CheckNameSelfCorrect(ref targetedName);
            UpdateColoCombatBlockRelationData(targetedName,  amount, StatsRelation.DAMAGE);

            return true;
        }

        bool ParseWeapon(string holdStringIn, bool isWeapon = true)
        {
            //Hold string should contain something like this at this point
            //Brutality's Tool's Inferno of Destruction (III) Lv.3 activated.
            string[] holdStringSplit = holdStringIn.Split(' ');

            bool coloSkillLevelFound = false;
            bool skillStringFound = false;
           // bool weaponStringFound = false;

            uint coloSkillLevel = 0;
            string skillString = string.Empty;
            string weaponString = string.Empty;

            for (int splitIndex = holdStringSplit.Length - 1; splitIndex >= 0; --splitIndex)
            {
                if (!coloSkillLevelFound)
                {
                    if (holdStringSplit[splitIndex].Contains("Lv.") || holdStringSplit[splitIndex].Contains("LV.")
                    ||  holdStringSplit[splitIndex].Contains("L.v.")|| holdStringSplit[splitIndex].Contains("T.v.")
                    || holdStringSplit[splitIndex].Contains("I.v.")) //Change this to self correcting
                    {
                        if (uint.TryParse(holdStringSplit[splitIndex].Substring(3, holdStringSplit[splitIndex].Length - 2 - 1), out coloSkillLevel))
                        {
                            coloSkillLevelFound = true;
                        }
                        else
                        {
                            WriteToErrorLog("From parseWeapon. Try parse colo skill failed " +holdStringSplit[splitIndex]);
                            return false; //Parsing error, just ignore the while block
                        }
                    }
                }
                else if (!skillStringFound)
                {
                    CheckSkillNameSelfCorrect(ref holdStringSplit[splitIndex]);
                    skillString = holdStringSplit[splitIndex] + skillString;
                    if (gv_skillsDictionary.Contains(skillString))
                        skillStringFound = true;
                    else
                        skillString = " " + skillString;
                }
                else
                {
                    //Get the name
                    weaponString = holdStringSplit[splitIndex] + weaponString;

                    if (splitIndex != 0)
                        weaponString = " " + weaponString;
                    else
                    {
                        if (!OnlyContainsAcceptableWeaponCharacters(weaponString))
                        {
                            TryFixWeaponString(ref weaponString);

                            if (!OnlyContainsAcceptableWeaponCharacters(weaponString))
                            {
                                WriteToErrorLog("From ParseWeapon. Weapon string weird: " + weaponString);
                                return false;
                            }
                        }

                        //Take away 's and then trim unneccessary characters from the end.
                        //Sometimes the ' gets missed out while reading the file
                        if(weaponString[weaponString.Length - 2] == '\'')
                            weaponString = weaponString.Substring(0, weaponString.Length - 2);
                        else if(weaponString[weaponString.Length-1] == 's')
                            weaponString = weaponString.Substring(0, weaponString.Length - 1);

                        while (weaponString.Length > 0)
                        {
                            if (IsAlphabet(weaponString[weaponString.Length - 1]))
                                break;
                            else
                                weaponString = weaponString.Substring(0, weaponString.Length - 1);
                        }

                        //Check weapon individual string
                        CheckWeaponNameSelfCorrect(ref weaponString);

                    }
                }
            }
            //Add weapon details to weaponHash
            if (weaponString.Length < 2)
            {
                WriteToErrorLog("From ParseWeapon. Weapon string length <2 or skill not found. " + holdStringIn);
                return false;
            }

            if (!Utilities.CheckIfWeaponExists(weaponString))
            {
                WriteToErrorLog("From ParseWeapon. Weapon dictionary doesn't contain " + weaponString);
                return false;
            }

            if (isWeapon)
            {
                UpdateWeapon(weaponString, "", coloSkillLevel);
                UpdateColoCombatBlockWeapon(true, weaponString, skillString, coloSkillLevel);
            }
            else
            {
                UpdateWeapon(weaponString, skillString, 0, coloSkillLevel);
                UpdateColoCombatBlockWeapon(false,weaponString, skillString, 0, coloSkillLevel);
            }

            return true;
        }

        void UpdateWeapon(string weaponName, string skillName = "", uint coloSkillLevel = 0, uint coloSupportSkillLevel = 0)
        {
            bool isGridSwapped = gv_isGridSwapped.ContainsKey(gv_name) ? gv_isGridSwapped[gv_name] : false;

            if (!gv_weaponHash.ContainsKey((gv_name, isGridSwapped)))
                gv_weaponHash.Add((gv_name, isGridSwapped), new Dictionary<string, WeaponDetails>());

            if (gv_weaponHash[(gv_name, isGridSwapped)].ContainsKey(weaponName))
            {
                if (coloSkillLevel > 0)
                    gv_weaponHash[(gv_name, isGridSwapped)][weaponName].SetColoSkillLevel(coloSkillLevel);

                if (coloSupportSkillLevel > 0)
                    gv_weaponHash[(gv_name, isGridSwapped)][weaponName].SetColoSupportLevel(coloSupportSkillLevel);

                if (skillName.Length > 0)
                    gv_weaponHash[(gv_name, isGridSwapped)][weaponName].SetSkillName(skillName);
            }
            else
            {
                gv_weaponHash[(gv_name, isGridSwapped)].Add(weaponName, new WeaponDetails(weaponName, skillName, coloSkillLevel, coloSupportSkillLevel));

                //We ignore support skill procs. We will sort it out at the end during the exporting part
               // if (skillName.Length > 0)
               //     return;

                if (!gv_weaponsAddedThisBlock.ContainsKey((gv_name, isGridSwapped)))
                    gv_weaponsAddedThisBlock.Add((gv_name, isGridSwapped), new Dictionary<string, WeaponDetails>());

                gv_weaponsAddedThisBlock[(gv_name, isGridSwapped)].Add(weaponName, new WeaponDetails(weaponName, skillName, coloSkillLevel, coloSupportSkillLevel));
            }

            //We ignore support skill procs. We will sort it out at the end during the exporting part
            if (skillName.Length > 0)
                return;

            // if (!gv_weaponsAddedThisBlock.ContainsKey((gv_name, isGridSwapped)))
            //     gv_weaponsAddedThisBlock.Add((gv_name, isGridSwapped), new Dictionary<string, WeaponDetails>());

            if (gv_weaponsAddedThisBlock.ContainsKey((gv_name, isGridSwapped)))
            {
                if (gv_weaponsAddedThisBlock[(gv_name, isGridSwapped)].ContainsKey(weaponName))
                    gv_weaponsAddedThisBlock[(gv_name, isGridSwapped)][weaponName].OverrideDetailsIfHigher(gv_weaponHash[(gv_name, isGridSwapped)][weaponName]);
            //    else
              //      gv_weaponsAddedThisBlock[(gv_name, isGridSwapped)].Add(weaponName, new WeaponDetails(gv_weaponHash[(gv_name, isGridSwapped)][weaponName]));
            }
        }

        void InitializeCurrentBlockCombatDetails()
        {
            gv_storeColoCombatDetailsForBlock.Add(new StoreColoCombatDetails());
            int last = gv_storeColoCombatDetailsForBlock.Count - 1;

            gv_storeColoCombatDetailsForBlock[last].initiator = gv_name;
            gv_storeColoCombatDetailsForBlock[last].blockTime = gv_time;
        }

        void UpdateColoCombatBlockWeapon(bool isWeapon, string weaponName, string skillName = "", uint coloSkillLevel = 0, uint coloSupportSkillLevel = 0)
        {
            int last = gv_storeColoCombatDetailsForBlock.Count - 1;

            if (isWeapon)
                gv_storeColoCombatDetailsForBlock[last].weaponUsedToAttack = new WeaponDetails(weaponName, skillName, coloSkillLevel, coloSupportSkillLevel);
            else
                gv_storeColoCombatDetailsForBlock[last].supportWeaponProcs.Add( new WeaponDetails(weaponName, skillName, coloSkillLevel, coloSupportSkillLevel));  
        }

        void UpdateColoCombatBlockRelationData(string targetedName, uint amount, StatsRelation relation, bool doHypotheticalCalculation = false)
        {
            int last = gv_storeColoCombatDetailsForBlock.Count - 1;

            if (!gv_storeColoCombatDetailsForBlock[last].receiverStatsModification.ContainsKey(targetedName))
                gv_storeColoCombatDetailsForBlock[last].receiverStatsModification.Add(targetedName, new List<CombatRelation>());

            if(doHypotheticalCalculation)
            {
                //To be done later
                gv_storeColoCombatDetailsForBlock[last].receiverStatsModification[targetedName].Add(new CombatRelation(amount, relation));
            }
            else
                gv_storeColoCombatDetailsForBlock[last].receiverStatsModification[targetedName].Add(new CombatRelation(amount, relation));
        }

        void FixWeaponIfGridChanged()
        {
            foreach(KeyValuePair<string,bool> kvp in gv_gridSwappedThisBlock)
            {
                if(gv_weaponsAddedThisBlock.ContainsKey((kvp.Key,true)) || gv_weaponsAddedThisBlock.ContainsKey((kvp.Key, false)))
                {
                    //Over here means we have found weapons that were added as true before grid swapping for a particular player
                    if (gv_weaponsAddedThisBlock.ContainsKey((kvp.Key, true)))
                        foreach (KeyValuePair<string, WeaponDetails> kvpInner in gv_weaponsAddedThisBlock[(kvp.Key, true)])
                            gv_weaponHash[(kvp.Key, true)].Remove(kvpInner.Key);

                    if (gv_weaponsAddedThisBlock.ContainsKey((kvp.Key, false)))
                        foreach (KeyValuePair<string, WeaponDetails> kvpInner in gv_weaponsAddedThisBlock[(kvp.Key, false)])
                            gv_weaponHash[(kvp.Key, false)].Remove(kvpInner.Key);  
                }
            }

            foreach (KeyValuePair<string, bool> kvp in gv_gridSwappedThisBlock)
            {
                if (gv_weaponsAddedThisBlock.ContainsKey((kvp.Key, true)))
                {
                    //Over here means we have found weapons that were added as true before grid swapping for a particular player
                    foreach (KeyValuePair<string, WeaponDetails> kvpInner in gv_weaponsAddedThisBlock[(kvp.Key, true)])
                    {
                        //gv_weaponHash[(kvp.Key, true)].Remove(kvpInner.Key);

                        if (!gv_weaponHash.ContainsKey((kvp.Key, false)))
                            gv_weaponHash.Add((kvp.Key, false), new Dictionary<string, WeaponDetails>());

                        if (gv_weaponHash[(kvp.Key, false)].ContainsKey(kvpInner.Key))
                            gv_weaponHash[(kvp.Key, false)][kvpInner.Key].OverrideDetailsIfHigher(kvpInner.Value);
                        else
                            gv_weaponHash[(kvp.Key, false)][kvpInner.Key] = new WeaponDetails(kvpInner.Value);
                       
                    }
                }
                else if (gv_weaponsAddedThisBlock.ContainsKey((kvp.Key, false)))
                {
                    foreach (KeyValuePair<string, WeaponDetails> kvpInner in gv_weaponsAddedThisBlock[(kvp.Key, false)])
                    {
                        //gv_weaponHash[(kvp.Key, false)].Remove(kvpInner.Key);

                        if (!gv_weaponHash.ContainsKey((kvp.Key, true)))
                            gv_weaponHash.Add((kvp.Key, true), new Dictionary<string, WeaponDetails>());

                        if (gv_weaponHash[(kvp.Key, true)].ContainsKey(kvpInner.Key))
                            gv_weaponHash[(kvp.Key, true)][kvpInner.Key].OverrideDetailsIfHigher(kvpInner.Value);
                        else
                            gv_weaponHash[(kvp.Key, true)][kvpInner.Key] = new WeaponDetails(kvpInner.Value);
                    
                    }
                }
            }
        }

        void PopulateColoBattleData()
        {
            for(int i = gv_storeColoCombatDetailsForBlock.Count-1; i >= 0; --i)
            {
                var receivers = gv_storeColoCombatDetailsForBlock[i].receiverStatsModification;

                //Update receiver's stats. The receivers will not have the modified stats because the calculations are done based on their current stats
                foreach (KeyValuePair<string, List<CombatRelation>> kvp in receivers)
                {
                    for (int j = 0; j < kvp.Value.Count; ++j)
                    {
                        if (gv_storeColoCurrentStatsDetails.ContainsKey(kvp.Key))
                            kvp.Value[j].PopulateStoreColoStatDetails(gv_storeColoCurrentStatsDetails[kvp.Key]);
                        else
                        {
                            //Add a filler relation so that we can add it to the relationship correction table
                            if (!gv_relationDictionary.ContainsKey(kvp.Key))
                                gv_relationDictionary.Add(kvp.Key, new Dictionary<string, Relation>());
                        }
                    }
                }

                if (!gv_storeColoCombatDetailsForBlock[i].specialCombatCase)
                {
                    //Update current stats for initator
                    string initiator = gv_storeColoCombatDetailsForBlock[i].initiator;

                    if (gv_storeColoCurrentStatsDetails.ContainsKey(initiator))
                        gv_storeColoCombatDetailsForBlock[i].initiatorStats = gv_storeColoCurrentStatsDetails[initiator].GetCurrentStats();
                    else
                    {
                        //Add a filler relation so that we can add it to the relationship correction table
                        if (!gv_relationDictionary.ContainsKey(initiator))
                            gv_relationDictionary.Add(initiator, new Dictionary<string, Relation>());
                    }

                    //Update the current stats for players that were buff/debuffed
                    foreach (KeyValuePair<string, List<CombatRelation>> kvp in receivers)
                    {
                        for (int j = 0; j < kvp.Value.Count; ++j)
                        {
                            if (gv_storeColoCurrentStatsDetails.ContainsKey(kvp.Key))
                                gv_storeColoCurrentStatsDetails[kvp.Key].ModifyStat((int)kvp.Value[j].relationAmount, kvp.Value[j].statsRelation);
                            else
                            {
                                //Add a filler relation so that we can add it to the relationship correction table
                                if (!gv_relationDictionary.ContainsKey(kvp.Key))
                                    gv_relationDictionary.Add(kvp.Key, new Dictionary<string, Relation>());
                            }
                        }
                    }
                }
                else  //There is a special colo case
                {
                    //Update the current stats for players that were buff/debuffed
                    foreach (KeyValuePair<string, List<CombatRelation>> kvp in receivers)
                    {
                        for (int j = 0; j < kvp.Value.Count; ++j)
                        {
                            if (kvp.Value[j].guildShipReset)
                            {
                                if (gv_storeColoCurrentStatsDetails.ContainsKey(kvp.Key))
                                    gv_storeColoCurrentStatsDetails[kvp.Key].SetDebuffsToZero();
                                else
                                {
                                    //Add a filler relation so that we can add it to the relationship correction table
                                    if (!gv_relationDictionary.ContainsKey(kvp.Key))
                                        gv_relationDictionary.Add(kvp.Key, new Dictionary<string, Relation>());
                                }
                            }
                        }
                    }
                }

                if (!gv_individualColoCombatDetails.ContainsKey(gv_storeColoCombatDetailsForBlock[i].initiator))
                    gv_individualColoCombatDetails.Add(gv_storeColoCombatDetailsForBlock[i].initiator, new List<StoreColoCombatDetails>());

                gv_individualColoCombatDetails[gv_storeColoCombatDetailsForBlock[i].initiator].Add(new StoreColoCombatDetails(gv_storeColoCombatDetailsForBlock[i]));
                gv_coloEntireCombatDetails.Add(new StoreColoCombatDetails(gv_storeColoCombatDetailsForBlock[i]));
            }
        }

        void ResetGuildshipDebuff()
        {
            gv_storeColoCombatDetailsForBlock.Add(new StoreColoCombatDetails(specialCombatCaseIn: true));
            int last = gv_storeColoCombatDetailsForBlock.Count - 1;

            gv_storeColoCombatDetailsForBlock[last].initiator = "GuildShip Reset. Clearing all debuffs";

            if (Utilities.IsFriendly(gv_guildshipBlockName))
            {
                var enemyNames = Utilities.GetAllEnemyNames();
                //If a friendly attacked guildship, we reset our enemy debuffs
                foreach (KeyValuePair<string,bool> kvp in enemyNames)
                {
                    gv_storeColoCombatDetailsForBlock[last].receiverStatsModification.Add(kvp.Key, new List<CombatRelation>());
                    gv_storeColoCombatDetailsForBlock[last].receiverStatsModification[kvp.Key].Add(new CombatRelation(guildShipResetIn: true));
                }
            }
            else
            {
                var friendlyNames = Utilities.GetAllFriendlyNames();
                foreach (KeyValuePair<string, bool> kvp in friendlyNames)
                {
                    gv_storeColoCombatDetailsForBlock[last].receiverStatsModification.Add(kvp.Key, new List<CombatRelation>());
                    gv_storeColoCombatDetailsForBlock[last].receiverStatsModification[kvp.Key].Add(new CombatRelation(guildShipResetIn: true));
                }
            }

            gv_skipBlockBecauseOfGuildship = false;
            gv_guildshipBlockName = string.Empty;
        }

        BlockType CheckFirstLineOfBlock(string line)
        {
            if (line.Contains("guildShip",StringComparison.OrdinalIgnoreCase))
                return BlockType.GUILDSHIP;
            else if (line.Contains("preparing to activate summon skill", StringComparison.OrdinalIgnoreCase))
                return BlockType.SUMMON_PREP;
            else if (line.Contains("activated summon skill", StringComparison.OrdinalIgnoreCase))
                return BlockType.SUMMON;
            else if (line.Contains("Counter shield", StringComparison.OrdinalIgnoreCase))
                return BlockType.COUNTER_SHIELD;
            else if (line.Contains("has revived", StringComparison.OrdinalIgnoreCase))
                return BlockType.REVIVE;
            else if (line.Contains("switched with", StringComparison.OrdinalIgnoreCase)) //eg. Rearguard ally Ibis switched with Vanguard ally Arco.
                return BlockType.SWAP;
            else if (line.Contains("HP recovered", StringComparison.OrdinalIgnoreCase))
                return BlockType.SELF_REVIVE;
            else if (line.Contains("changed gear set", StringComparison.OrdinalIgnoreCase))
                return BlockType.CHANGED_GEAR_SET;

            return BlockType.NORMAL;
        }

        bool SkipLineofBlock(string line)
        {
            return line.Contains("combo") || line.Contains("guildship"); //Needs to be changed to a self correcting config
        }

        bool ContainsMasteryEarned(string line)
        {
            return line.Contains("mastery earned");
        }
        bool ContainsDate(string line)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(line, "[0-9][0-9]/[0-9][0-9]", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        bool ContainsTime(string line)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(line, "[0-9][0-9]:[0-9][0-9]", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        string GetTime(string line)
        {
            return System.Text.RegularExpressions.Regex.Match(line, "[0-9][0-9]:[0-9][0-9]", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Value;
        }

        bool IsAlphabet(char letter)
        {
            return ((letter >= 'A' && letter <= 'Z') || (letter >= 'a' && letter <= 'z'));
        }


        bool OnlyContainsAcceptableWeaponCharacters(string line)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(line, "^[-0-9A-Za-z '.()&＆é`]+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        bool CheckForNameBreak(string line)
        {
            return gv_nameFilterDictionary.Contains(line);
        }


        void TryFixWeaponString(ref string line)
        {
            line = line.Replace("‘", "'");
            line = line.Replace("’", "'");
            line = line.Replace("「", "'");
            line = line.Replace(" s", "'s");
            line = line.Replace("'S", "'s");

            //When using OCR with +jpn
            line = line.Replace("`", "'");
            line = line.Replace(" ' ", "'s");
            line = line.Replace(" '“s", "'s");
            line = line.Replace(" S ", "'s");
            line = line.Replace(" '“s ", "'s");
            line = line.Replace(" ″s ", "'s");
        }

        void CheckRelationSelfCorrect(ref string line)
        {
            string[] splitText = line.Split(' ');
            string replacementStr = string.Empty;

            for (int i = 0; i < splitText.Length; ++i)
            {
                if (gv_relationCorrectionDictionary.ContainsKey(splitText[i]))
                {
                   // WriteToErrorLog("CheckRelationSelfCorrect changed " + line);
                    splitText[i] = gv_relationCorrectionDictionary[splitText[i]];
                }

                replacementStr += splitText[i];

                if (i != splitText.Length - 1)
                    replacementStr += " ";
            }

            if (replacementStr.Length > 0)
                line = replacementStr;
        }

        void CheckSkillSelfCorrect(ref string line)
        {
            string[] splitText = line.Split(' ');
            string replacementStr = string.Empty;

            for (int i = 0; i < splitText.Length; ++i )
            {
                if(gv_skillNumbersSelfCorrectionDictionary.ContainsKey(splitText[i]))
                    splitText[i] = gv_skillNumbersSelfCorrectionDictionary[splitText[i]];

                replacementStr += splitText[i];

                if (i != splitText.Length - 1)
                    replacementStr += " ";
            }

            if(replacementStr.Length > 0)
                line = replacementStr;
        }

        void CheckSkillNameSelfCorrect(ref string line)
        {
            string[] splitText = line.Split(' ');
            string replacementStr = string.Empty;

            for (int i = 0; i < splitText.Length; ++i)
            {
                if (gv_skillsCorrectionDictionary.ContainsKey(splitText[i]))
                    splitText[i] = gv_skillsCorrectionDictionary[splitText[i]];

                replacementStr += splitText[i];

                if (i != splitText.Length - 1)
                    replacementStr += " ";
            }

            if (replacementStr.Length > 0)
                line = replacementStr;
        }

        void CheckWeaponNameSelfCorrect(ref string line)
        {
            string[] splitText = line.Split(' ');
            string replacementStr = string.Empty;

            for (int i = 0; i < splitText.Length; ++i)
            {
                if (gv_weaponsCorrectionDictionary.ContainsKey(splitText[i]))
                    splitText[i] = gv_weaponsCorrectionDictionary[splitText[i]];

                replacementStr += splitText[i];

                if (i != splitText.Length - 1)
                    replacementStr += " ";
            }

            if (replacementStr.Length > 0)
                line = replacementStr;
         }

        void CheckNameSelfCorrect(ref string line)
        {
            if (gv_nameSelfCorrectionDictionary.ContainsKey(line))
            {
                //Console.WriteLine("Changing " + line + " to " + gv_nameSelfCorrectionDictionary[line]);
                line = gv_nameSelfCorrectionDictionary[line];
            }
        }

        string CheckNameSelfCorrectForWeapons(string line)
        {
            if (line.IndexOf("'s") <= 0 || line.IndexOf("`s") <= 0)
            {
                TryFixWeaponString(ref line);

                if (line.IndexOf("'s") <= 0)
                {
                    WriteToErrorLog("CheckNameSelfCorrectForWeapons error: " + line);
                    return string.Empty;
                }
            }

            string nameSubString = line.Substring(0, line.LastIndexOf("'s"));

            if (gv_nameSelfCorrectionDictionary.ContainsKey(nameSubString))
            {
                nameSubString = gv_nameSelfCorrectionDictionary[nameSubString];
               // WriteToErrorLog("CheckNameSelfCorrectForWeapons changed: " + line);
            }

            return nameSubString;
        }

    }


}
