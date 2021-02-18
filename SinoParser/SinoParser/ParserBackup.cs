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
        UNKNOWN, //we will need to log this
    }

    public enum FileType
    {
        SKILL_DICTIONARY,
        SKILL_CORRECTION_DICTIONARY,
        SKILL_SELF_CORRECTING,
        NAME_SELF_CORRECTING,
        RELATION_SELF_CORRECTING,
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

    class Unconfirmed //Used for text that seems to be correct but unable to confirm
    {
        Unconfirmed() { }
    }

    class Parser
    {
        private string[] lines;
        private string[] lastFileLines;

        //First string is the players
        //Second string is who this player has affected
        //Relation is what kind of relation.
        //eg. Arco atk up 100 for Ibis
        // This would mean in Arco Key -> Ibis Key -> relation patk + 100
        private Dictionary<string, Dictionary<string, Relation>> relationDictionary = new Dictionary<string, Dictionary<string, Relation>>();

        //Weapon details needs to be frequently updated. HashSet updating is troublesome
        private Dictionary<string, Dictionary<string,WeaponDetails>> weaponHash = new Dictionary<string, Dictionary<string,WeaponDetails>>();





        private HashSet<string> skillsDictionary = new HashSet<string>();
        private Dictionary<string, string> relationCorrectionDictionary = new Dictionary<string, string>();
        private Dictionary<string,string> skillsCorrectionDictionary = new Dictionary<string, string>();
        private Dictionary<string, string> skillSelfCorrectionDictionary = new Dictionary<string, string>();
        private Dictionary<string, string> nameSelfCorrectionDictionary = new Dictionary<string, string>();


        private StreamWriter errorLog;
        private string folder;

        //Analyse variables
        int blockCount = 0;
        bool firstLineOfBlock = false;
        bool skipBlock = false;
        bool lastLine = false;
        string name = string.Empty;
        string holdString = string.Empty;
        string timeString = string.Empty;
        BlockType bt = BlockType.UNKNOWN;
        BlockState blockState = BlockState.HEADER_NOT_FOUND;

        //Last file lines analyse variables
        int lastFileBlocksFound = 0;

        List<Tuple<string, StatsRelation>> stringToStatsRelation = new List<Tuple<string, StatsRelation>>();


        //Stores the current block for combat detail. We need this because combat details read are in descending order.
        List<StoreColoCombatDetails> storeColoCombatDetailsForBlock = new List<StoreColoCombatDetails>();

        //Stores how much buff/debuff each player has at any point of time. 
        Dictionary<string, StoreColoStatsDetails> storeColoCurrentStatsDetails = new Dictionary<string, StoreColoStatsDetails>();

        Dictionary<string, List<StoreColoAttackingDetails>> storeColoAttacks = new Dictionary<string, List<StoreColoAttackingDetails>>();

        //Relation store variables
        string storeWeaponString = string.Empty;
        List<string> storeSupportWeaponString = new List<string>();
        List<StoreColoStats> storeRelationStats = new List<StoreColoStats>();

        //Store previous relation variables. Used to check for duplicates
        List<StoreColoStats> storePreviousRelationStats = new List<StoreColoStats>();

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

        public Parser() 
        {
            //errorLog..OpenWrite("C:/Users/Onsla/OneDrive/Desktop/Sinoalice SQL/Images/error.txt");
            errorLog = new StreamWriter("C:/Users/Onsla/OneDrive/Desktop/Sinoalice SQL/Images/error.txt");

            Initialize();
        }

        void Initialize()
        {
            stringToStatsRelation.Add(new Tuple<string, StatsRelation>(mAtkUpBy, StatsRelation.MATK_UP));
            stringToStatsRelation.Add(new Tuple<string, StatsRelation>(mAtkDownBy, StatsRelation.MATK_DOWN));
            stringToStatsRelation.Add(new Tuple<string, StatsRelation>(mDefUpBy, StatsRelation.MDEF_UP));
            stringToStatsRelation.Add(new Tuple<string, StatsRelation>(mDefDownBy, StatsRelation.MDEF_DOWN));
            stringToStatsRelation.Add(new Tuple<string, StatsRelation>(atkUpBy, StatsRelation.PATK_UP));
            stringToStatsRelation.Add(new Tuple<string, StatsRelation>(atkDownBy, StatsRelation.PATK_DOWN));
            stringToStatsRelation.Add(new Tuple<string, StatsRelation>(defUpBy, StatsRelation.PDEF_UP));
            stringToStatsRelation.Add(new Tuple<string, StatsRelation>(defDownBy, StatsRelation.PDEF_DOWN));
            stringToStatsRelation.Add(new Tuple<string, StatsRelation>(hpRecoveredBy, StatsRelation.RECOVER));
            stringToStatsRelation.Add(new Tuple<string, StatsRelation>(damageTo, StatsRelation.DAMAGE));

        }


        public void ParseConfigFile(string file, FileType ft)
        {
            if (File.Exists(file))
            {
                lines = File.ReadAllLines(file);
                string correctText = string.Empty;
                for (int i = 0; i < lines.Length; ++i)
                {
                    if(lines[i].StartsWith('*'))
                    {
                        correctText = lines[i].Substring(1, lines[i].Length - 1);

                        if (ft == FileType.SKILL_DICTIONARY)
                            skillsDictionary.Add(correctText);

                        Console.WriteLine("Correct text is " + correctText);
                    }
                    else
                    {
                        switch (ft)
                        {
                            case FileType.SKILL_SELF_CORRECTING:
                                if(!skillSelfCorrectionDictionary.ContainsKey(lines[i]))
                                    skillSelfCorrectionDictionary.Add(lines[i], correctText);
                                break;

                            case FileType.NAME_SELF_CORRECTING:
                                if (!nameSelfCorrectionDictionary.ContainsKey(lines[i]))
                                    nameSelfCorrectionDictionary.Add(lines[i], correctText);
                                break;

                            case FileType.SKILL_CORRECTION_DICTIONARY:
                                if (!skillsCorrectionDictionary.ContainsKey(lines[i]))
                                    skillsCorrectionDictionary.Add(lines[i], correctText);
                                break;

                            case FileType.RELATION_SELF_CORRECTING:
                                if (!relationCorrectionDictionary.ContainsKey(lines[i]))
                                    relationCorrectionDictionary.Add(lines[i], correctText);
                                break;

                            default:
                                break;
                        }     
                    }
                }
            }
        }


        public bool ParseFile(string file, string folder, string lastFile)
        {
            file = folder + file;
            lastFile = folder + lastFile;

            if (!File.Exists(file))
                return false;

            if (!File.Exists(lastFile))
                return false;

            lines = File.ReadAllLines(file);
            lines = lines.Where(x => x.Length > 0 && x != "\f").ToArray();

            lastFileLines = File.ReadAllLines(lastFile);
            lastFileLines = lastFileLines.Where(x => x.Length > 0 && x != "\f").ToArray();

            this.folder = folder;
            //foreach(string s in lines)
            //{
            //    Console.WriteLine(s);
            //}
            Console.WriteLine("File is " + file);

           // PreAnalyseLastLines();
            // AnalyseFile();
            //AnalyseFileV2();
            AnalyseLines(false, true);
            AnalyseLines(true, false);
            return true;
        }

        public void ExportWeaponHash()
        {
            var weaponHashLog = new StreamWriter("C:/Users/Onsla/OneDrive/Desktop/Sinoalice SQL/Images/weaponHash.txt");

            foreach(KeyValuePair<string,Dictionary<string, WeaponDetails>> kvp in weaponHash)
            {
                weaponHashLog.WriteLine("**" +kvp.Key+"**");

                foreach(KeyValuePair<string,WeaponDetails> kvpInner in kvp.Value)
                {
                    weaponHashLog.WriteLine(kvpInner.Value.ToString());
                }
            }

            weaponHashLog.Close();
        }
        public void ExportRelationshipTable()
        {
            var relationshipTableLog = new StreamWriter("C:/Users/Onsla/OneDrive/Desktop/Sinoalice SQL/Images/relationshipTable.csv");

            foreach (KeyValuePair<string, Dictionary<string, Relation>> kvp in relationDictionary)
            {
                relationshipTableLog.WriteLine("**" + kvp.Key + "**");

                foreach (KeyValuePair<string, Relation> kvpInner in kvp.Value)
                {
                    relationshipTableLog.WriteLine(kvpInner.Key.ToString()+","+kvpInner.Value.ToString());
                }
            }

            relationshipTableLog.Close();
        }

        public void ExportRelationshipCorrectionTable()
        {
            var relationshipCorrectionTableLog = new StreamWriter("C:/Users/Onsla/OneDrive/Desktop/Sinoalice SQL/Images/relationshipCorrectionTable.txt");

            List<string> relationshipList = new List<string>();

            foreach (KeyValuePair<string, Dictionary<string, Relation>> kvp in relationDictionary)
            {
                relationshipList.Add(kvp.Key);
            }

            relationshipList = relationshipList.OrderBy(x => x).ToList();

            for(int i = 0; i < relationshipList.Count; ++i)
            {
                relationshipCorrectionTableLog.WriteLine(relationshipList[i]);
            }

            relationshipCorrectionTableLog.Close();
        }

        public void ExportColoCombatDetails()
        {
            var coloCombatLog = new StreamWriter("C:/Users/Onsla/OneDrive/Desktop/Sinoalice SQL/Images/coloCombatDetails.txt");

            // Dictionary<string, List<StoreColoAttackingDetails>> storeColoAttacks = new Dictionary<string, List<StoreColoAttackingDetails>>();
            foreach (KeyValuePair<string, List<StoreColoAttackingDetails>> kvp in storeColoAttacks)
            {
                coloCombatLog.WriteLine("**" + kvp.Key + "**");

                for(int i = 0; i < kvp.Value.Count; ++i)
                {
                    if (kvp.Value[i].combatDetails.GetDamage() > 0u)
                    {
                        coloCombatLog.WriteLine(kvp.Value[i].combatDetails.GetInitiator() + " " + kvp.Value[i].currentStatsOfInitiator.ToString());

                        for(int j = 0; j < kvp.Value[i].currentStatsOfTarget.Count; ++j)
                            coloCombatLog.WriteLine(kvp.Value[i].currentStatsOfTarget[j].GetReceiver() + " " + kvp.Value[i].currentStatsOfTarget[j].ToString());

                        coloCombatLog.WriteLine("Targeted " + kvp.Value[i].combatDetails.GetReceiver() + " with " +
                            kvp.Value[i].combatDetails.GetWeaponUsed().GetWeaponNameWithLevel() + " Damage dealt:" +
                            kvp.Value[i].combatDetails.GetDamage());

                        coloCombatLog.WriteLine();
                    }
                }
            }

            coloCombatLog.Close();
        }

        void WriteToErrorLog(string errorString)
        {
            errorLog.WriteLine(folder);
            errorLog.WriteLine(errorString);
            errorLog.WriteLine("");
        }

        public void CloseFile()
        {
            errorLog.Close();
        }

        private void ResetAnalyseVariables()
        {
            blockCount = 0;
            blockState = BlockState.HEADER_NOT_FOUND;
            firstLineOfBlock = false;
            skipBlock = false;
            lastLine = false;
            name = string.Empty;
            holdString = string.Empty;
            storeWeaponString = string.Empty;
            storeSupportWeaponString.Clear();
            storeRelationStats.Clear();
            bt = BlockType.UNKNOWN;
            storeColoCombatDetailsForBlock.Clear();

            lastFileBlocksFound = 0;

        }

        private void ResetBlockVariables()
        {
            holdString = string.Empty;
            skipBlock = false;
            // headerGotten = true;
            blockState = BlockState.HEADER_FOUND;
            firstLineOfBlock = true;
            storeWeaponString = string.Empty;
            storeSupportWeaponString.Clear();
            storeRelationStats.Clear();
        }

        private void PreAnalyseLastLines()
        {
            bool headerFound = false;
            //We want to see how many full blocks we can capture here. That will tell us when to switch over to use this
            for(int i = 0; i < lastFileLines.Length; ++i)
            {
                if (!headerFound)
                {
                    if (ContainsTime(lastFileLines[i]))
                    {
                        string[] splitLine = lastFileLines[i].Split(' ');
                        if (splitLine.Length > 2) // €) 10/13 23:16 will be invalid
                        {
                            ++lastFileBlocksFound;
                            headerFound = true;
                        }
                    }
                }

                if (headerFound)
                {
                    if ((i + 1 < lastFileLines.Length) && ContainsTime(lastFileLines[i + 1]))
                    {
                        headerFound = false;
                    }
                }
            }
        }
  
        private void AnalyseLines(bool isLastLine, bool doResetAnalyseVariables)
        {
            if (doResetAnalyseVariables)
            {
                ResetAnalyseVariables();
                PreAnalyseLastLines();
            }

            //Every page has only up to 30 records
            string[] linesToAnalyse;

            if (!isLastLine)
                linesToAnalyse = lines;
            else
            {
                //Clear previous dirty data since we are reading from a different file now
                linesToAnalyse = lastFileLines;
                storeWeaponString = string.Empty;
                storeSupportWeaponString.Clear();
                storeRelationStats.Clear();
                storePreviousRelationStats.Clear();

                PopulateColoBattleData();
                storeColoCombatDetailsForBlock.Clear();
            }

            //The only thing certain is if the line contains time, its the start of block.
            //If we failed to parse finish the previous block, then we need to put into the unconfirmed class for debugging
            for (int i = 0; i < linesToAnalyse.Length; ++i)
            {
                //Max number of blocks we will analyse in a file.
                //It is possible to hit 30 before reaching the end because of duplicates in taking the screenshots.
                if (blockCount > 30 && blockState == BlockState.HEADER_NOT_FOUND)// || i + 1 >= lines.Length)
                {
                    if (i + 1 >= linesToAnalyse.Length)
                        WriteToErrorLog("Hit 30 block count but still got more lines");
                    break;
                }

                if (blockCount >= (30 - lastFileBlocksFound) && blockState == BlockState.HEADER_NOT_FOUND)
                {
                    //We need to switch to read from last line file instead of this
                    if (isLastLine == false)
                        return;
                }

                //Always get header first. Will always consists of time
                if (blockState == BlockState.HEADER_NOT_FOUND)
                {
                    //We are in a block
                    //Because of counter shield, its possible that there is no name, but we still need to count it among the 30
                    //Need to do a name dictionary
                    if (ContainsTime(linesToAnalyse[i]))
                    {
                        string[] splitLine = linesToAnalyse[i].Split(' ');
                        if (splitLine.Length > 2) // €) 10/13 23:16 will be invalid
                        {
                            name = splitLine[0];
                            CheckNameSelfCorrect(ref name);
                        }
                        else
                        {
                            //Most likely its counter shield. Need to output to a file to print out uncertain stuff

                        }
                    }
                    else
                        continue;

                    ResetBlockVariables();
                    ++blockCount;
                }
                else
                {
                    //We can do some parsing here to already know what kind of block we are dealing with
                    if (firstLineOfBlock)
                    {
                        bt = CheckFirstLineOfBlock(linesToAnalyse[i]);

                        switch (bt)
                        {
                            case BlockType.COUNTER_SHIELD:
                            case BlockType.REVIVE:
                            case BlockType.SELF_REVIVE:
                            case BlockType.SUMMON:
                            case BlockType.SUMMON_PREP:
                            case BlockType.SWAP:
                                skipBlock = true; //No point getting data from them. Maybe only revive in the future
                                break;

                            case BlockType.GUILDSHIP: //We will just skip to the end for now
                                skipBlock = true;
                                break;

                            case BlockType.UNKNOWN: //Need to log this down somewhere
                                skipBlock = true;
                                break;

                            case BlockType.NORMAL:
                                break;

                            default:  //If we miss out anything, we just ignore for now and log it down. Treat same as unknown   
                                break;
                        }
                    }

                    //If the next line contains time, it means we have hit the end of our block. 
                    //Wrap up whatever we have for this block and reset
                    if ((i + 1 < linesToAnalyse.Length) && ContainsTime(linesToAnalyse[i + 1]) || i == linesToAnalyse.Length-1)
                    {
                        //headerGotten = false;
                        if (!skipBlock)
                            blockState = BlockState.LAST_LINE;
                        else
                            blockState = BlockState.HEADER_NOT_FOUND;
                    }

                    if (skipBlock)
                        continue;

                    //Check if we need to skip block
                    if (firstLineOfBlock)
                    {
                        firstLineOfBlock = false;
                        if (SkipLineofBlock(linesToAnalyse[i]))
                            continue;
                    }

                    //Proceed to get contents. Need to check if we are on a weapon line
                    if (blockState == BlockState.HEADER_FOUND)
                    {
                        CheckSkillSelfCorrect(ref linesToAnalyse[i]);

                        string[] splitLine = linesToAnalyse[i].Split(' ');

                        if (string.Compare(splitLine[splitLine.Length - 1], "activated.") == 0)
                        {
                            blockState = BlockState.WEAPON_FOUND;
                            if (holdString.Length > 0)
                                holdString += " " + linesToAnalyse[i];
                            else
                                holdString = linesToAnalyse[i];
                        }
                        else
                        {
                            holdString = linesToAnalyse[i];
                            continue;
                        }
                    }

                    if (blockState == BlockState.WEAPON_FOUND)
                    {
                        //ParseWeapon(holdString);
                        storeWeaponString = holdString;
                        holdString = string.Empty;
                        blockState = BlockState.CHECKING_SUPPORT_SKILLS;
                    }
                    else if (blockState == BlockState.CHECKING_SUPPORT_SKILLS)
                    {
                        //We need to check if there are even support skills.
                        //if (linesToAnalyse[i].Contains("damage to")) //Change to use a self corrector
                        //{
                        //    blockState = BlockState.ADDING_RELATION_STATS;
                        //    continue;
                        //}
                        //else if (DoesLineContainStatsRelation(linesToAnalyse[i]) != StatsRelation.NONE)
                        //{
                        //    blockState = BlockState.ADDING_RELATION_STATS;
                        //    holdString = string.Empty;
                        //}
                        if (DoesLineContainStatsRelation(linesToAnalyse[i]) != StatsRelation.NONE) //We want to check damage to now as well
                        {
                            blockState = BlockState.ADDING_RELATION_STATS;
                            holdString = string.Empty;
                        }
                        else
                        {
                            //Most likely its a support skill.
                            CheckSkillSelfCorrect(ref linesToAnalyse[i]);

                            string[] splitLine = linesToAnalyse[i].Split(' ');

                            if (string.Compare(splitLine[splitLine.Length - 1], "activated.") == 0) //change to use a self corrector
                            {
                                if (holdString.Length > 0)
                                    holdString += " " + linesToAnalyse[i];
                                else
                                    holdString = linesToAnalyse[i];

                                //ParseWeapon(holdString, false);
                                storeSupportWeaponString.Add(holdString);

                                holdString = string.Empty;
                            }
                            else
                            {
                                holdString = linesToAnalyse[i];
                                continue;
                            }
                        }
                    }

                    if (blockState == BlockState.ADDING_RELATION_STATS)
                    {
                        StatsRelation sr = DoesLineContainStatsRelation(linesToAnalyse[i]);

                        if (sr == StatsRelation.NONE)
                        {
                            //WriteToErrorLog("DoesLineContainStatsRelation check: " + lines[i]);
                            continue;
                        }

                        if(sr == StatsRelation.DAMAGE)
                        {
                            //Ignore for now
                        }
                        //ParseRelation(lines[i], sr);
                        storeRelationStats.Add(new StoreColoStats(linesToAnalyse[i], sr));

                    }

                    if (blockState == BlockState.LAST_LINE)
                    {
                        blockState = BlockState.HEADER_NOT_FOUND;

                        //If mastery is found here, then we add our weapons or else ignore this block.
                        if (linesToAnalyse[i].Contains("mastery"))
                        {
                            if (storePreviousRelationStats.Count > 0)
                            {
                                //We need to compare and check if our current relation is the same. 
                                //If it is means its a duplicate and we ignore the entire block (Not 100% foolproof but will work until we change the taking of screenshots

                                bool duplicate = false;
                                for (int s = 0; s < storeRelationStats.Count; ++s)
                                {
                                    for (int s2 = 0; s2 < storePreviousRelationStats.Count; ++s2)
                                    {
                                        if (storeRelationStats[s] == storePreviousRelationStats[s2])
                                        {
                                            duplicate = true;
                                            break;
                                        }
                                    }

                                    if (duplicate)
                                        break;
                                }

                                if (!duplicate)
                                    storePreviousRelationStats.Clear();
                                else
                                {
                                    WriteToErrorLog("Duplicate found " + linesToAnalyse[i]);
                                    --blockCount;
                                    continue;
                                }
                            }

                            if (storeWeaponString.Length > 0)
                            {
                                storeColoCombatDetailsForBlock.Add(new StoreColoCombatDetails(name));

                                if (!ParseWeapon(storeWeaponString, true))
                                    storeColoCombatDetailsForBlock[storeColoCombatDetailsForBlock.Count-1].UpdateWeaponUsed(new WeaponDetails(storeWeaponString,""));
                            }
                            else
                                continue; //If there are no weapons, we ignore it because this means the block has issues as there is mastery earned

                            for (int s = 0; s < storeSupportWeaponString.Count; ++s)
                                ParseWeapon(storeSupportWeaponString[s], false);

                            for (int s = 0; s < storeRelationStats.Count; ++s)
                                if (ParseRelation(storeRelationStats[s].coloStatsString, storeRelationStats[s].relation))
                                    storePreviousRelationStats.Add(new StoreColoStats(storeRelationStats[s]));

                        }
                        else
                        {
                            WriteToErrorLog("Mastery not found " + linesToAnalyse[i]);
                            --blockCount;
                        }


                    }


                }  //End of else from header gotten check

            } //End of first for

         }

        StatsRelation DoesLineContainStatsRelation(string line)
        {
            //We only need this local because we use StatsRelation to check instead of M.DEf, ATK etc..
            CheckRelationSelfCorrect(ref line);
            
            //foreach(KeyValuePair<string,StatsRelation> kvp in stringToStatsRelation)
            //{
            //    if (line.Contains(kvp.Key))
            //        return kvp.Value;
            //}

            for(int i = 0; i < stringToStatsRelation.Count; ++i)
            {
                if (line.Contains(stringToStatsRelation[i].Item1))
                    return stringToStatsRelation[i].Item2;
            }

            return StatsRelation.NONE;
        }

        bool ParseRelation(string holdStringIn, StatsRelation relation)
        {
            //Contains something like this if passed into this function
            //Ibis's HP recovered by 12,810.
            //Ibis's M.ATK DOWN by 1,673.

            //Getting names cannot just use splitString[0] because of other languages レ ヴ ァ ン ダ ー

            //Damage needs to be handled different because the format is different
            if (relation == StatsRelation.DAMAGE)
                return ParseDamage(holdStringIn);

            string holdString = holdStringIn;
            string targetedName = string.Empty;
            targetedName = CheckNameSelfCorrectForWeapons(holdString);

            if (targetedName.Length == 0)
                return false;

            string[] splitString = holdString.Split(' ');

           // string targetedName = splitString[0];
           // targetedName = targetedName.Substring(0, targetedName.Length - 2);


            string amountString = splitString[splitString.Length - 1];
            amountString = amountString.Replace(",",string.Empty);
            uint amount = 0;
            if (!uint.TryParse(amountString.Replace(".", string.Empty), out amount))
            {
                WriteToErrorLog("From ParseRelation " +holdStringIn);
                return false;
            }

            if (!relationDictionary.ContainsKey(name))
                relationDictionary.Add(name, new Dictionary<string, Relation>());

            if (!relationDictionary[name].ContainsKey(targetedName))
                relationDictionary[name].Add(targetedName, new Relation());

            if (!relationDictionary.ContainsKey(targetedName))
                relationDictionary.Add(targetedName, new Dictionary<string, Relation>());

            if (!relationDictionary[targetedName].ContainsKey(name))
                relationDictionary[targetedName].Add(name, new Relation());

            switch (relation)
            {
                case StatsRelation.PATK_UP:
                    relationDictionary[name][targetedName].PAtk(amount, true, false);
                    relationDictionary[targetedName][name].PAtk(amount, true, true);
                    break;
                case StatsRelation.PATK_DOWN:
                    relationDictionary[name][targetedName].PAtk(amount, false, false);
                    relationDictionary[targetedName][name].PAtk(amount, false, true);
                    break;
                case StatsRelation.MATK_UP:
                    relationDictionary[name][targetedName].MAtk(amount, true, false);
                    relationDictionary[targetedName][name].MAtk(amount, true, true);
                    break;
                case StatsRelation.MATK_DOWN:
                    relationDictionary[name][targetedName].MAtk(amount, false, false);
                    relationDictionary[targetedName][name].MAtk(amount, false, true);
                    break;
                case StatsRelation.PDEF_UP:
                    relationDictionary[name][targetedName].PDef(amount, true, false);
                    relationDictionary[targetedName][name].PDef(amount, true, true);
                    break;
                case StatsRelation.PDEF_DOWN:
                    relationDictionary[name][targetedName].PDef(amount, false, false);
                    relationDictionary[targetedName][name].PDef(amount, false, true);
                    break;
                case StatsRelation.MDEF_UP:
                    relationDictionary[name][targetedName].MDef(amount, true, false);
                    relationDictionary[targetedName][name].MDef(amount, true, true);
                    break;
                case StatsRelation.MDEF_DOWN:
                    relationDictionary[name][targetedName].MDef(amount, false, false);
                    relationDictionary[targetedName][name].MDef(amount, false, true);
                    break;
                case StatsRelation.RECOVER:
                    relationDictionary[name][targetedName].Healing(amount,false);
                    relationDictionary[targetedName][name].Healing(amount,true);
                    break;

                default:
                    break;
            }

            UpdateColoCombatBlockRelation(targetedName, amount, relation);

            return true;
        }

        bool ParseDamage(string holdStringIn)
        {
            //Contains something like this if passed into this function
            //3,212 damage to Ibis.
            //8,015 damage to 2 target(s). We ignore multiple targets

            //Getting names cannot just use splitString[0] because of other languages レ ヴ ァ ン ダ ー
            if(holdStringIn.Contains("target(s)")) //Update this to use self correcting
            {
                //This should be a valid string but we don't need to parse multiple target data
                return true;
            }
 

            string[] splitString = holdStringIn.Split(' ');
            string targetedName = splitString[splitString.Length-1].Substring(0, splitString[splitString.Length - 1].Length - 1); //Take away the last . (fullstop)
            CheckNameSelfCorrect(ref targetedName); 

            string amountString = splitString[0];
            amountString = amountString.Replace(",", string.Empty);
            uint amount = 0;
            if (!uint.TryParse(amountString.Replace(".", string.Empty), out amount))
            {
                WriteToErrorLog("From ParseDamage " + holdStringIn);
                return false;
            }

            UpdateColoCombatBlockDamage(targetedName, amount);


            return true;
        }

        bool ParseWeapon(string holdStringIn, bool isWeapon = true)
        {
            //Hold string should contain something like this at this point
            //Brutality's Tool's Inferno of Destruction (III) Lv.3 activated.

            //Can be improved if we have a config of all available weapons.
            //Right now we only have a config of all available skills

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
                    if (holdStringSplit[splitIndex].Contains("Lv.") || holdStringSplit[splitIndex].Contains("LV.")) //Change this to self correcting
                    {
                        //string d = holdStringSplit[splitIndex].Substring(3, holdStringSplit[splitIndex].Length - 2 - 1); //-1 extra because startIndex from 1
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
                    if (skillsDictionary.Contains(skillString))
                    {
                        skillStringFound = true;
                    }
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
                            //Try to fix.
                            TryFixWeaponString(ref weaponString);

                            if (!OnlyContainsAcceptableWeaponCharacters(weaponString))
                            {
                                WriteToErrorLog("From ParseWeapon. Weapon string weird: " + weaponString);
                                return false;
                            }
                        }

                        //Take away 's and then trim unneccessary characters from the end
                        weaponString = weaponString.Substring(0, weaponString.Length - 2); 
                        while(weaponString.Length > 0)
                        {
                            if (IsAlphabet(weaponString[weaponString.Length - 1]))
                                break;
                            else
                                weaponString = weaponString.Substring(0, weaponString.Length - 1);
                        }

                       // weaponStringFound = true;
                    }
                }
            }
            //Add weapon details to weaponHash
            if (weaponString.Length < 2)
            {
                WriteToErrorLog("From ParseWeapon. Weapon string length <2 " + holdStringIn);
                return false;
            }

            if (isWeapon)
            {
                UpdateWeapon(weaponString, "", coloSkillLevel);
                UpdateColoCombatBlockWeapon(true, weaponString, "", coloSkillLevel);
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
            if(!weaponHash.ContainsKey(name))
                weaponHash.Add(name, new Dictionary<string, WeaponDetails>());

            if (weaponHash[name].ContainsKey(weaponName))
            {
                if (coloSkillLevel > 0)
                    weaponHash[name][weaponName].SetColoSkillLevel(coloSkillLevel);

                if (coloSupportSkillLevel > 0)
                    weaponHash[name][weaponName].SetColoSupportLevel(coloSupportSkillLevel);

                if(skillName.Length > 0)
                    weaponHash[name][weaponName].SetSkillName(skillName);
            }
            else
            {
                weaponHash[name].Add(weaponName, new WeaponDetails(weaponName, skillName, coloSkillLevel, coloSupportSkillLevel));
            }
        }

        void UpdateColoCombatBlockWeapon(bool isWeapon, string weaponName, string skillName = "", uint coloSkillLevel = 0, uint coloSupportSkillLevel = 0)
        {
            int last = storeColoCombatDetailsForBlock.Count - 1;

            if(isWeapon)
                storeColoCombatDetailsForBlock[last].UpdateWeaponUsed(new WeaponDetails(weaponName, skillName, coloSkillLevel, coloSupportSkillLevel));
            else
                storeColoCombatDetailsForBlock[last].PushActivatedColoSkills(new WeaponDetails(weaponName, skillName, coloSkillLevel, coloSupportSkillLevel));
        }

        void UpdateColoCombatBlockDamage(string target, uint amount)
        {
            int last = storeColoCombatDetailsForBlock.Count - 1;
            storeColoCombatDetailsForBlock[last].UpdateDamage(amount);
            storeColoCombatDetailsForBlock[last].UpdateReceiver(target);
        }

        void UpdateColoCombatBlockRelation(string target, uint amount, StatsRelation relation)
        {
            int last = storeColoCombatDetailsForBlock.Count - 1;
            storeColoCombatDetailsForBlock[last].AddColoStatsDetails();
            storeColoCombatDetailsForBlock[last].GetLastColoStatsDetails().UpdateReceiver(target);
            

            switch (relation)
            {
                case StatsRelation.PATK_UP:
                    storeColoCombatDetailsForBlock[last].GetLastColoStatsDetails().ModifyPATKStat((int)amount, true);
                    break;
                case StatsRelation.PATK_DOWN:
                    storeColoCombatDetailsForBlock[last].GetLastColoStatsDetails().ModifyPATKStat((int)amount, false);
                    break;
                case StatsRelation.MATK_UP:
                    storeColoCombatDetailsForBlock[last].GetLastColoStatsDetails().ModifyMATKStat((int)amount, true);
                    break;
                case StatsRelation.MATK_DOWN:
                    storeColoCombatDetailsForBlock[last].GetLastColoStatsDetails().ModifyMATKStat((int)amount, false);
                    break;
                case StatsRelation.PDEF_UP:
                    storeColoCombatDetailsForBlock[last].GetLastColoStatsDetails().ModifyPDEFStat((int)amount, true);
                    break;
                case StatsRelation.PDEF_DOWN:
                    storeColoCombatDetailsForBlock[last].GetLastColoStatsDetails().ModifyPDEFStat((int)amount, false);
                    break;
                case StatsRelation.MDEF_UP:
                    storeColoCombatDetailsForBlock[last].GetLastColoStatsDetails().ModifyMDEFStat((int)amount, true);
                    break;
                case StatsRelation.MDEF_DOWN:
                    storeColoCombatDetailsForBlock[last].GetLastColoStatsDetails().ModifyMDEFStat((int)amount, false);
                    break;

                default:
                    break;
            }
        }

        void PopulateColoBattleData()
        {
            //We need to do this backwards because colo logs are inverted
            for (int i = storeColoCombatDetailsForBlock.Count - 1; i >= 0; --i)
            {
                string damageInitiator = storeColoCombatDetailsForBlock[i].GetInitiator();
                string damageReceiver = storeColoCombatDetailsForBlock[i].GetReceiver();

                if (!storeColoAttacks.ContainsKey(damageInitiator))
                    storeColoAttacks.Add(damageInitiator, new List<StoreColoAttackingDetails>());
                if (!storeColoCurrentStatsDetails.ContainsKey(damageInitiator))
                    storeColoCurrentStatsDetails.Add(damageInitiator, new StoreColoStatsDetails());

                //We need to check if its a buff/debuff/healing attack
                if (damageReceiver != null && damageReceiver != string.Empty)
                {
                    if (!storeColoAttacks.ContainsKey(damageReceiver))
                        storeColoAttacks.Add(damageReceiver, new List<StoreColoAttackingDetails>());
                    if (!storeColoCurrentStatsDetails.ContainsKey(damageReceiver))
                        storeColoCurrentStatsDetails.Add(damageReceiver, new StoreColoStatsDetails());
                }

                //storeColoCombatDetailsForBlock[i].GetLastColoStatsDetails().UpdateAllStats(storeColoCurrentStatsDetails[receiver]);

                StoreColoAttackingDetails attackingDetails = new StoreColoAttackingDetails();
                List<StoreColoStatsDetails> statsDetails = storeColoCombatDetailsForBlock[i].GetColoStatsDetails();
                for (int j = 0; j < statsDetails.Count; ++j)
                {
                    if (!storeColoCurrentStatsDetails.ContainsKey(statsDetails[j].GetReceiver()))
                        storeColoCurrentStatsDetails.Add(statsDetails[j].GetReceiver(), new StoreColoStatsDetails());

                    statsDetails[j].UpdateAllStats(storeColoCurrentStatsDetails[statsDetails[j].GetReceiver()]);


                    attackingDetails.currentStatsOfTarget.Add(new StoreColoStatsDetails());
                    attackingDetails.currentStatsOfTarget[attackingDetails.currentStatsOfTarget.Count - 1].UpdateReceiver(statsDetails[j].GetReceiver());
                }

                for (int k = 0; k < attackingDetails.currentStatsOfTarget.Count; ++k)
                {
                    if (attackingDetails.currentStatsOfTarget[k].GetReceiver() != null)
                        storeColoCurrentStatsDetails[attackingDetails.currentStatsOfTarget[k].GetReceiver()].UpdateAllStats(attackingDetails.currentStatsOfTarget[k]);
                }

                //Update the stats when we attack our target
                storeColoCurrentStatsDetails[damageInitiator].UpdateAllStats(attackingDetails.currentStatsOfInitiator);
                storeColoCombatDetailsForBlock[i].UpdateCombatDetails(attackingDetails.combatDetails);

                storeColoAttacks[damageInitiator].Add(attackingDetails);
            }
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

            return BlockType.NORMAL;
        }

        bool SkipLineofBlock(string line)
        {
            return line.Contains("combo") || line.Contains("guildship"); //Needs to be changed to a self correcting config

        }

        bool ContainsTime(string line)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(line, "[0-9][0-9]:[0-9][0-9]", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        bool OnlyContainsAcceptableWeaponCharacters(string line)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(line, "^[-0-9A-Za-z '.()&＆é`]+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            //return true;
        }

        bool IsAlphabet(string line)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(line, "^[-0-9A-Za-z()]+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            //return true;
        }

        bool IsAlphabet(char letter)
        {
            return ((letter >= 'A' && letter <= 'Z') || (letter >= 'a' && letter <= 'z'));
        }

        void TryFixWeaponString(ref string line)
        {
           line = line.Replace("‘", "'");
           line = line.Replace("’", "'");
           line = line.Replace("「", "'");
        }

        void CheckRelationSelfCorrect(ref string line)
        {
            string[] splitText = line.Split(' ');
            // Console.WriteLine("Initial text is " + line);

            string replacementStr = string.Empty;

            for (int i = 0; i < splitText.Length; ++i)
            {
                if (relationCorrectionDictionary.ContainsKey(splitText[i]))
                {
                   // WriteToErrorLog("CheckRelationSelfCorrect changed " + line);
                    splitText[i] = relationCorrectionDictionary[splitText[i]];
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
           // Console.WriteLine("Initial text is " + line);

            string replacementStr = string.Empty;

            for (int i = 0; i < splitText.Length; ++i )
            {
                if(skillSelfCorrectionDictionary.ContainsKey(splitText[i]))
                {
                    //Console.WriteLine("Changing " + splitText[i] + " to " + skillSelfCorrectionDictionary[splitText[i]]);
                    splitText[i] = skillSelfCorrectionDictionary[splitText[i]];
                }

                replacementStr += splitText[i];

                if (i != splitText.Length - 1)
                    replacementStr += " ";
            }

            if(replacementStr.Length > 0)
                line = replacementStr;

           // Console.WriteLine("Skill self corrected text is " + line);
        }

        void CheckSkillNameSelfCorrect(ref string line)
        {
            string[] splitText = line.Split(' ');
            // Console.WriteLine("Initial text is " + line);

            string replacementStr = string.Empty;

            for (int i = 0; i < splitText.Length; ++i)
            {
                if (skillsCorrectionDictionary.ContainsKey(splitText[i]))
                {
                    //Console.WriteLine("Changing " + splitText[i] + " to " + skillSelfCorrectionDictionary[splitText[i]]);
                    splitText[i] = skillsCorrectionDictionary[splitText[i]];
                }

                replacementStr += splitText[i];

                if (i != splitText.Length - 1)
                    replacementStr += " ";
            }

            if (replacementStr.Length > 0)
                line = replacementStr;

            // Console.WriteLine("Skill self corrected text is " + line);
        }

        void CheckNameSelfCorrect(ref string line)
        {
           // string[] splitText = line.Split(' ');
          //  Console.WriteLine("Initial text is " + line);

           // string replacementStr = string.Empty;

         
            if (nameSelfCorrectionDictionary.ContainsKey(line))
            {
                Console.WriteLine("Changing " + line + " to " + nameSelfCorrectionDictionary[line]);
                line = nameSelfCorrectionDictionary[line];
            }

            //foreach (KeyValuePair<string,string> kvp in nameSelfCorrectionDictionary)
            //{
            //    if (line.Contains(kvp.Key))
            //    {
            //        line = line.Replace(kvp.Key, kvp.Value);
            //        break;
            //    }
            //}

            // line = replacementStr;

            //  Console.WriteLine("Name self corrected text is " + line);
        }

        string CheckNameSelfCorrectForWeapons(string line)
        {
            if (line.IndexOf("'s") <= 0)
            {
                TryFixWeaponString(ref line);

                if (line.IndexOf("'s") <= 0)
                {
                    WriteToErrorLog("CheckNameSelfCorrectForWeapons error: " + line);
                    return string.Empty;
                }
            }

            string nameSubString = line.Substring(0, line.IndexOf("'s"));

            if (nameSelfCorrectionDictionary.ContainsKey(nameSubString))
            {
                nameSubString = nameSelfCorrectionDictionary[nameSubString];
               // WriteToErrorLog("CheckNameSelfCorrectForWeapons changed: " + line);
            }

            return nameSubString;
        }

    }


}
