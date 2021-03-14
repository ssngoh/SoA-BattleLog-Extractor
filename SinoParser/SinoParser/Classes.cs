using System;
using System.Collections.Generic;
using System.Text;

namespace SinoParser
{
    enum StatsEnum
    {
        PATK,
        PDEF,
        MATK,
        MDEF
    }

    enum WeaponType
    {
        NONE,
        SWORD,
        HAMMER,
        BOW,
        SPEAR,
        HARP,
        TOME,
        STAFF,
        ORB
    }

    enum ElementalType
    {
        NONE,
        FIRE,
        WATER,
        WIND
    }


    //This stores the details of each attack.
    class StoreColoCombatDetails
    {
        public StoreColoCombatDetails(bool specialCombatCaseIn = false)
        {
            specialCombatCase = specialCombatCaseIn;
        }

        public StoreColoCombatDetails(StoreColoCombatDetails copy)
        {
            weaponUsedToAttack = copy.weaponUsedToAttack;
            supportWeaponProcs = copy.supportWeaponProcs;
            initiator = copy.initiator;
            initiatorStats = copy.initiatorStats;
            blockTime = copy.blockTime;

            receiverStatsModification = copy.receiverStatsModification;
        }

        public string GetAttackerAndStats()
        {
            if (initiatorStats == null)
                return initiator + " - " + "Unable to extract stats";

            return initiator + " - " + initiatorStats.ToString();
        }

        public WeaponDetails weaponUsedToAttack;
        public List<WeaponDetails> supportWeaponProcs = new List<WeaponDetails>();
        public string initiator;
        public string blockTime;
        public Stats initiatorStats;
        public bool specialCombatCase { get; private set; } 

        //key holds the name of the receiver.
        public Dictionary<string, List<CombatRelation>> receiverStatsModification = new Dictionary<string, List<CombatRelation>>();
    
        //Still need to add stuff like is NM active, shinma etc

    }


    class Stats
    { 
        public Stats()
        {

        }

        public Stats(int patk, int pdef, int matk, int mdef)
        {
            this.patk = patk;
            this.pdef = pdef;
            this.matk = matk;
            this.mdef = mdef;
        }

        public override string ToString()
        {
            return "PATK " + patk + ", PDEF " + pdef + ", MATK " + matk + ", MDEF " + mdef;
        }

        public string ToStringWithPad()
        {
            return string.Format("PATK {0}, PDEF {1}, MATK {2}, MDEF {3}", patk.ToString("D4"), pdef.ToString("D4"), matk.ToString("D4"), mdef.ToString("D4"));
        }

        public string GetBuffStatString()
        {
            return "PATK +" + patk + ", PDEF +" + pdef + ", MATK +" + matk + ", MDEF +" + mdef;
        }

        public string GetDebuffStatString()
        {
            return "PATK -" + patk + ", PDEF -" + pdef + ", MATK -" + matk + ", MDEF -" + mdef;
        }

        public int GetTotalStats()
        {
            return patk + matk + pdef + mdef;
        }

        public static Stats operator +(Stats a, Stats b)
        {
            return new Stats(a.patk + b.patk, a.pdef + b.pdef, a.matk + b.matk, a.mdef + b.mdef);
        }

        public static Stats operator *(Stats a, float b)
        {
            return new Stats((int)(a.patk * b), (int)(a.pdef * b), (int)(a.matk * b), (int)(a.mdef * b));
        }


        public int patk;
        public int matk;
        public int pdef;
        public int mdef;
    }

    class CombatRelation 
    {
        public CombatRelation(uint relationAmountIn, StatsRelation statsRelationIn,  bool damageIsHypotheticalIn = false) 
        {
            relationAmount = relationAmountIn;
            statsRelation = statsRelationIn;
            damageIsHypothetical = damageIsHypotheticalIn;
        }

        public CombatRelation(bool guildShipResetIn)
        {
            guildShipReset = guildShipResetIn;
        }

        public void PopulateStoreColoStatDetails(StoreColoStatsDetails copy)
        {
            statsDetails = new StoreColoStatsDetails(copy);
        }

        public override string ToString()
        {
            if (statsRelation == StatsRelation.DAMAGE)
                return " damage taken: " + relationAmount;
            else if (statsRelation == StatsRelation.RECOVER)
                return " healed for: " + relationAmount;
            else if (statsRelation == StatsRelation.PATK_UP)
            {
                float effectiveness = statsDetails.GetBuffEffectiveness(StatsEnum.PATK);
                return " PATK: +" + relationAmount + " Actual Value " + relationAmount / effectiveness + ". Effectiveness " + effectiveness * 100 + "%";
            }
            else if (statsRelation == StatsRelation.PATK_DOWN)
            {
                float effectiveness = statsDetails.GetDebuffEffectiveness(StatsEnum.PATK);
                return " PATK: -" + relationAmount + " Actual Value " + relationAmount / effectiveness + ". Effectiveness " + effectiveness * 100 + "%";
            }
            else if (statsRelation == StatsRelation.PDEF_UP)
            {
                float effectiveness = statsDetails.GetBuffEffectiveness(StatsEnum.PDEF);
                return " PDEF: +" + relationAmount + " Actual Value " + relationAmount / effectiveness + ". Effectiveness " + effectiveness * 100 + "%";
            }
            else if (statsRelation == StatsRelation.PDEF_DOWN)
            {
                float effectiveness = statsDetails.GetDebuffEffectiveness(StatsEnum.PDEF);
                return " PDEF: -" + relationAmount + " Actual Value " + relationAmount / effectiveness + ". Effectiveness " + effectiveness * 100 + "%";
            }
            else if (statsRelation == StatsRelation.MATK_UP)
            {
                float effectiveness = statsDetails.GetBuffEffectiveness(StatsEnum.MATK);
                return " MATK: +" + relationAmount + " Actual Value " + relationAmount / effectiveness + ". Effectiveness " + effectiveness * 100 + "%";
            }
            else if (statsRelation == StatsRelation.MATK_DOWN)
            {
                float effectiveness = statsDetails.GetDebuffEffectiveness(StatsEnum.MATK);
                return " MATK: -" + relationAmount + " Actual Value " + relationAmount / effectiveness + ". Effectiveness " + effectiveness * 100 + "%";
            }
            else if (statsRelation == StatsRelation.MDEF_UP)
            {
                float effectiveness = statsDetails.GetBuffEffectiveness(StatsEnum.MDEF);
                return " MDEF: +" + relationAmount + " Actual Value " + relationAmount / effectiveness + ". Effectiveness " + effectiveness * 100 + "%";
            }
            else if (statsRelation == StatsRelation.MDEF_DOWN)
            {
                float effectiveness = statsDetails.GetDebuffEffectiveness(StatsEnum.MDEF);
                return " MDEF: -" + relationAmount + " Actual Value " + relationAmount / effectiveness + ". Effectiveness " + effectiveness * 100 + "%";
            }

            Console.WriteLine("Something went wrong in getting CombatRelation.ToString(). StatsRelation is " + statsRelation.ToString());
            return string.Empty;
        }

        public Tuple<StatsRelation, uint, uint, float> GetRelationAmount()
        {
            float effectiveness = 0.0f;

            if (statsRelation == StatsRelation.PATK_UP)
                 effectiveness = statsDetails.GetBuffEffectiveness(StatsEnum.PATK);
            else if (statsRelation == StatsRelation.PATK_DOWN)
                effectiveness = statsDetails.GetDebuffEffectiveness(StatsEnum.PATK);
            else if (statsRelation == StatsRelation.PDEF_UP)
                effectiveness = statsDetails.GetBuffEffectiveness(StatsEnum.PDEF);
            else if (statsRelation == StatsRelation.PDEF_DOWN)
                effectiveness = statsDetails.GetDebuffEffectiveness(StatsEnum.PDEF);
            else if (statsRelation == StatsRelation.MATK_UP)
                effectiveness = statsDetails.GetBuffEffectiveness(StatsEnum.MATK);
            else if (statsRelation == StatsRelation.MATK_DOWN)
                effectiveness = statsDetails.GetDebuffEffectiveness(StatsEnum.MATK);
            else if (statsRelation == StatsRelation.MDEF_UP)
                effectiveness = statsDetails.GetBuffEffectiveness(StatsEnum.MDEF);
            else if (statsRelation == StatsRelation.MDEF_DOWN)
                effectiveness = statsDetails.GetDebuffEffectiveness(StatsEnum.MDEF);

            return new Tuple<StatsRelation, uint, uint,float>(statsRelation, relationAmount, (uint)(relationAmount / effectiveness), effectiveness);
        }

        public uint relationAmount { get; private set; }
        public StatsRelation statsRelation { get; private set; }

        public bool damageIsHypothetical { get; private set; } //To be used later when the log says deals damage to 2 or more targets
        public bool guildShipReset { get; private set; } //To be used later when the log says deals damage to 2 or more targets

        public StoreColoStatsDetails statsDetails { get; private set; }

    }


    //This stores the details of each participant's current stats
    class StoreColoStatsDetails
    {
        public StoreColoStatsDetails()
        {

        }

        public StoreColoStatsDetails(StoreColoStatsDetails copy)
        {
            if (copy == null)
                return;

            initialPATK = copy.initialPATK;
            initialMATK = copy.initialMATK;
            initialPDEF = copy.initialPDEF;
            initialMDEF = copy.initialMDEF;

            currentPATKbuff = copy.currentPATKbuff;
            currentPATKdebuff = copy.currentPATKdebuff;
            currentMATKbuff = copy.currentMATKbuff;
            currentMATKdebuff = copy.currentMATKdebuff;

            currentPDEFbuff = copy.currentPDEFbuff;
            currentPDEFdebuff = copy.currentPDEFdebuff;
            currentMDEFbuff = copy.currentMDEFbuff;
            currentMDEFdebuff = copy.currentMDEFdebuff;
        }

        public StoreColoStatsDetails(int initialPATK, int initialMATK, int initialPDEF, int initialMDEF, int isVG)
        {
            this.initialPATK = initialPATK;
            this.initialMATK = initialMATK;
            this.initialPDEF = initialPDEF;
            this.initialMDEF = initialMDEF;
            this.isVG = isVG > 0 ? true : false;
        }

        public void ModifyStat(int value, StatsRelation sr)
        {
            switch(sr)
            {
                case StatsRelation.PATK_UP:

                    if(currentPATKdebuff > 0 )
                    {
                        if(currentPATKdebuff >= value)
                            currentPATKdebuff -= value;
                        else
                        {
                            value -= currentPATKdebuff;
                            currentPATKdebuff = 0;
                            currentPATKbuff = Math.Min(currentPATKbuff + value, initialPATK);
                        }
                    }
                    else
                        currentPATKbuff = Math.Min(currentPATKbuff + value, initialPATK);

                    break;

                case StatsRelation.PATK_DOWN:

                    if (currentPATKbuff > 0)
                    {
                        if (currentPATKbuff >= value)
                            currentPATKbuff -= value;
                        else
                        {
                            value -= currentPATKbuff;
                            currentPATKbuff = 0;
                            currentPATKdebuff = (int)Math.Min((float)(currentPATKdebuff + value), (float)initialPATK * 0.7);
                        }
                    }
                    else
                        currentPATKdebuff = (int)Math.Min((float)(currentPATKdebuff + value), (float)initialPATK * 0.7);

                    break;

                case StatsRelation.MATK_UP:

                    if (currentMATKdebuff > 0)
                    {
                        if (currentMATKdebuff >= value)
                            currentMATKdebuff -= value;
                        else
                        {
                            value -= currentMATKdebuff;
                            currentMATKdebuff = 0;
                            currentMATKbuff = Math.Min(currentMATKbuff + value, initialMATK);
                        }
                    }
                    else
                        currentMATKbuff = Math.Min(currentMATKbuff + value, initialMATK);

                    break;

                case StatsRelation.MATK_DOWN:

                    if (currentMATKbuff > 0)
                    {
                        if (currentMATKbuff >= value)
                            currentMATKbuff -= value;
                        else
                        {
                            value -= currentMATKbuff;
                            currentMATKbuff = 0;
                            currentMATKdebuff = (int)Math.Min((float)(currentMATKdebuff + value), (float)initialMATK * 0.7);
                        }
                    }
                    else
                        currentMATKdebuff = (int)Math.Min((float)(currentMATKdebuff + value), (float)initialMATK * 0.7);

                    break;

                case StatsRelation.PDEF_UP:

                    if (currentPDEFdebuff > 0)
                    {
                        if (currentPDEFdebuff >= value)
                            currentPDEFdebuff -= value;
                        else
                        {
                            value -= currentPDEFdebuff;
                            currentPDEFdebuff = 0;
                            currentPDEFbuff = Math.Min(currentPDEFbuff + value, initialPDEF);
                        }
                    }
                    else
                        currentPDEFbuff = Math.Min(currentPDEFbuff + value, initialPDEF);

                    break;

                case StatsRelation.PDEF_DOWN:

                    if (currentPDEFbuff > 0)
                    {
                        if (currentPDEFbuff >= value)
                            currentPDEFbuff -= value;
                        else
                        {
                            value -= currentPDEFbuff;
                            currentPDEFbuff = 0;
                            currentPDEFdebuff = (int)Math.Min((float)(currentPDEFdebuff + value), (float)initialPDEF * 0.7);
                        }
                    }
                    else
                        currentPDEFdebuff = (int)Math.Min((float)(currentPDEFdebuff + value), (float)initialPDEF * 0.7);

                    break;

                case StatsRelation.MDEF_UP:

                    if (currentMDEFdebuff > 0)
                    {
                        if (currentMDEFdebuff >= value)
                            currentMDEFdebuff -= value;
                        else
                        {
                            value -= currentMDEFdebuff;
                            currentMDEFdebuff = 0;
                            currentMDEFbuff = Math.Min(currentMDEFbuff + value, initialMDEF);
                        }
                    }
                    else
                        currentMDEFbuff = Math.Min(currentMDEFbuff + value, initialMDEF);

                    break;

                case StatsRelation.MDEF_DOWN:

                    if (currentMDEFbuff > 0)
                    {
                        if (currentMDEFbuff >= value)
                            currentMDEFbuff -= value;
                        else
                        {
                            value -= currentMDEFbuff;
                            currentMDEFbuff = 0;
                            currentMDEFdebuff = (int)Math.Min((float)(currentMDEFdebuff + value), (float)initialMDEF * 0.7);
                        }
                    }
                    else
                        currentMDEFdebuff = (int)Math.Min((float)(currentMDEFdebuff + value), (float)initialMDEF * 0.7);

                    break;

                default:
                    break;
            }
        }

        public void SetDebuffsToZero()
        {
            currentPATKdebuff = 0;
            currentPDEFdebuff = 0;
            currentMATKdebuff = 0;
            currentMDEFdebuff = 0;
        }
       
        public int GetCurrentPATK()
        {
            return initialPATK + currentPATKbuff - currentPATKdebuff;
        }

        public int GetCurrentMATK()
        {
            return initialMATK + currentMATKbuff - currentMATKdebuff;
        }

        public int GetCurrentPDEF()
        {
            return initialPDEF + currentPDEFbuff - currentPDEFdebuff;
        }

        public int GetCurrentMDEF()
        {
            return initialMDEF + currentMDEFbuff - currentMDEFdebuff;
        }

        public string GetCurrentStatsAndBuffDebuff()
        {
            int patkStatsDisplayAmount = GetStatsDiffDisplay(StatsEnum.PATK);
            int pdefStatsDisplayAmount = GetStatsDiffDisplay(StatsEnum.PDEF);
            int matkStatsDisplayAmount = GetStatsDiffDisplay(StatsEnum.MATK);
            int mdefStatsDisplayAmount = GetStatsDiffDisplay(StatsEnum.MDEF);

            string patkDisplay = patkStatsDisplayAmount >= 0 ? "(+" + patkStatsDisplayAmount + ")" : "(" + patkStatsDisplayAmount + ")";
            string pdefDisplay = pdefStatsDisplayAmount >= 0 ? "(+" + pdefStatsDisplayAmount + ")" : "(" + pdefStatsDisplayAmount + ")";
            string matkDisplay = matkStatsDisplayAmount >= 0 ? "(+" + matkStatsDisplayAmount + ")" : "(" + matkStatsDisplayAmount + ")";
            string mdefDisplay = mdefStatsDisplayAmount >= 0 ? "(+" + mdefStatsDisplayAmount + ")" : "(" + mdefStatsDisplayAmount + ")";


            return "PATK " + GetCurrentPATK() + patkDisplay + ", " + "PDEF " + GetCurrentPDEF() + pdefDisplay + ", " +
                   "MATK " + GetCurrentMATK() + matkDisplay + ", " + "MDEF " + GetCurrentMDEF() + mdefDisplay;

        }

        public Stats GetCurrentStats()
        {
            Stats returnStats = new Stats(GetCurrentPATK(), GetCurrentPDEF(), GetCurrentMATK(), GetCurrentMDEF());
            return returnStats;
        }

        public int GetStatsDiffDisplay(StatsEnum statsToGet)
        {
            int currentStats = 0;
            int initialStats = 0;

            switch (statsToGet)
            {
                case StatsEnum.PATK:
                    currentStats = GetCurrentPATK();
                    initialStats = initialPATK;
                    break;

                case StatsEnum.PDEF:
                    currentStats = GetCurrentPDEF();
                    initialStats = initialPDEF;
                    break;

                case StatsEnum.MATK:
                    currentStats = GetCurrentMATK();
                    initialStats = initialMATK;
                    break;

                case StatsEnum.MDEF:
                    currentStats = GetCurrentMDEF();
                    initialStats = initialMDEF;
                    break;
            }

            float ratio = (float)currentStats / (float)initialStats;

            ratio -= 1;
            int flooredAmount = (int)(ratio * 100);

            return Utilities.GetStatsNumberDisplay(flooredAmount);
        }

        public float GetBuffEffectiveness(StatsEnum statsToGet)
        {
            int currentStats = 0;
            int initialStats = 0;

            switch (statsToGet)
            {
                case StatsEnum.PATK:
                    currentStats = GetCurrentPATK();
                    initialStats = initialPATK;
                    break;

                case StatsEnum.PDEF:
                    currentStats = GetCurrentPDEF();
                    initialStats = initialPDEF;
                    break;

                case StatsEnum.MATK:
                    currentStats = GetCurrentMATK();
                    initialStats = initialMATK;
                    break;

                case StatsEnum.MDEF:
                    currentStats = GetCurrentMDEF();
                    initialStats = initialMDEF;
                    break;
            }

            float ratio = (float)currentStats / (float)initialStats;
            ratio -= currentStats >= initialStats ? 1 : 0;

            if(currentStats >= initialStats)
                return Utilities.GetHarpDisplayAndEffectiveness((int)(ratio * 100));
            else
                return Utilities.GetHarpDisplayAndEffectiveness(0);
        }

        public float GetDebuffEffectiveness(StatsEnum statsToGet)
        {
            int currentStats = 0;
            int initialStats = 0;

            switch (statsToGet)
            {
                case StatsEnum.PATK:
                    currentStats = GetCurrentPATK();
                    initialStats = initialPATK;
                    break;

                case StatsEnum.PDEF:
                    currentStats = GetCurrentPDEF();
                    initialStats = initialPDEF;
                    break;

                case StatsEnum.MATK:
                    currentStats = GetCurrentMATK();
                    initialStats = initialMATK;
                    break;

                case StatsEnum.MDEF:
                    currentStats = GetCurrentMDEF();
                    initialStats = initialMDEF;
                    break;
            }

            float ratio = (float)currentStats / (float)initialStats;
            ratio = 1f - ratio;

            if (initialStats >= currentStats)
                return Utilities.GetTomeDisplayAndEffectiveness((int)(ratio * 100));
            else
                return Utilities.GetTomeDisplayAndEffectiveness(0);
        }


        int currentPATKbuff;
        int currentPATKdebuff;
        int currentMATKbuff;
        int currentMATKdebuff;

        int currentPDEFbuff;
        int currentPDEFdebuff;
        int currentMDEFbuff;
        int currentMDEFdebuff;

        int initialPATK;
        int initialMATK;
        int initialPDEF;
        int initialMDEF;

        bool isVG;
    }



    class StoreColoStats
    {
        public StoreColoStats(StoreColoStats copy)
        {
            this.coloStatsString = copy.coloStatsString;
            this.relation = copy.relation;
        }
        public StoreColoStats(string s, StatsRelation sr)
        {
            coloStatsString = s;
            relation = sr;
        }


        public override bool Equals(object obj)
        {
            StoreColoStats stats = (StoreColoStats)obj;
            if (stats == null)
                return false;
            else
                return base.Equals((StoreColoStats)obj) && this == (StoreColoStats)obj;
        }

        public static bool operator ==(StoreColoStats lhs, StoreColoStats rhs)
        {
            return String.Compare(lhs.coloStatsString, rhs.coloStatsString) == 0 && lhs.relation == rhs.relation;
        }

        public static bool operator !=(StoreColoStats lhs, StoreColoStats rhs)
        {
            return String.Compare(lhs.coloStatsString, rhs.coloStatsString) != 0 && lhs.relation != rhs.relation;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public string coloStatsString;
        public StatsRelation relation;
    }


    class Relation
    {
        public Relation()
        {
            patkIncreaseGiven = 0;
            patkDecreaseGiven = 0;
            matkIncreaseGiven = 0;
            matkDecreaseGiven = 0;

            pdefIncreaseGiven = 0;
            pdefDecreaseGiven = 0;
            mdefIncreaseGiven = 0;
            mdefDecreaseGiven = 0;

            healingGiven = 0;

            patkIncreaseReceived = 0;
            patkDecreaseReceived = 0;
            matkIncreaseReceived = 0;
            matkDecreaseReceived = 0;

            pdefIncreaseReceived = 0;
            pdefDecreaseReceived = 0;
            mdefIncreaseReceived = 0;
            mdefDecreaseReceived = 0;

            healingReceived = 0;
        }

        public void PAtk(uint amount, bool increase = true, bool receiving = true)
        {
            if (receiving)
            {
                if (increase)
                    patkIncreaseReceived += amount;
                else
                    patkDecreaseReceived += amount;
            }
            else
            {
                if (increase)
                    patkIncreaseGiven += amount;
                else
                    patkDecreaseGiven += amount;
            }
        }

        public void MAtk(uint amount, bool increase = true, bool receiving = true)
        {
            if (receiving)
            {
                if (increase)
                    matkIncreaseReceived += amount;
                else
                    matkDecreaseReceived += amount;
            }
            else
            {
                if (increase)
                    matkIncreaseGiven += amount;
                else
                    matkDecreaseGiven += amount;
            }
        }

        public void PDef(uint amount, bool increase = true, bool receiving = true)
        {
            if (receiving)
            {
                if (increase)
                    pdefIncreaseReceived += amount;
                else
                    pdefDecreaseReceived += amount;
            }
            else
            {
                if (increase)
                    pdefIncreaseGiven += amount;
                else
                    pdefDecreaseGiven += amount;
            }
        }

        public void MDef(uint amount, bool increase = true, bool receiving = true)
        {
            if (receiving)
            {
                if (increase)
                    mdefIncreaseReceived += amount;
                else
                    mdefDecreaseReceived += amount;
            }
            else
            {
                if (increase)
                    mdefIncreaseGiven += amount;
                else
                    mdefDecreaseGiven += amount;
            }
        }

        public void Healing(uint amount, bool receiving = true)
        {
            if (receiving)
                healingReceived += amount;
            else
                healingGiven += amount;
        }

        public override string ToString()
        {
            return patkIncreaseGiven + "," + pdefIncreaseGiven + "," + matkIncreaseGiven + "," + mdefIncreaseGiven + "," +
                   patkDecreaseGiven + "," + pdefDecreaseGiven + "," + matkDecreaseGiven + "," + mdefDecreaseGiven + "," + healingGiven + "," +
                   patkIncreaseReceived + "," + pdefIncreaseReceived + "," + matkIncreaseReceived + "," + mdefIncreaseReceived + "," +
                   patkDecreaseReceived + "," + pdefDecreaseReceived + "," + matkDecreaseReceived + "," + mdefDecreaseReceived + "," + healingReceived;
        }

        uint patkIncreaseGiven;
        uint pdefIncreaseGiven;
        uint matkIncreaseGiven;
        uint mdefIncreaseGiven;

        uint patkDecreaseGiven;
        uint pdefDecreaseGiven;
        uint matkDecreaseGiven;
        uint mdefDecreaseGiven;

        uint healingGiven;


        uint patkIncreaseReceived;
        uint pdefIncreaseReceived;
        uint matkIncreaseReceived;
        uint mdefIncreaseReceived;

        uint patkDecreaseReceived;
        uint pdefDecreaseReceived;
        uint matkDecreaseReceived;
        uint mdefDecreaseReceived;

        uint healingReceived;
    }

    class WeaponDetails
    {
        public WeaponDetails(string weaponName, string skillName)
        {
            this.weaponName = weaponName;
            this.skillName = skillName;
            weaponType = Utilities.GetWeaponType(weaponName);
           
        }

        public WeaponDetails(string weaponName, string skillName, uint coloSkillLevel, uint coloSupportLevel)
        {
            this.weaponName = weaponName;
            this.skillName = skillName;
            this.coloSkillLevel = coloSkillLevel;
            this.coloSupportLevel = coloSupportLevel;
            weaponType = Utilities.GetWeaponType(weaponName);

        }

        public WeaponDetails(WeaponDetails wd)
        {
            this.weaponName = wd.weaponName;
            this.skillName = wd.skillName;
            this.coloSkillLevel = wd.coloSkillLevel;
            this.coloSupportLevel = wd.coloSupportLevel;
            this.weaponType = wd.weaponType;
        }

        public void SetColoSkillLevel(uint level)
        {
            coloSkillLevel = level;
        }

        public void SetColoSupportLevel(uint level)
        {
            coloSupportLevel = level;
        }
        public void SetSkillName(string skillName)
        {
            this.skillName = skillName;
        }

        public void OverrideDetails(WeaponDetails wd)
        {
            this.weaponName = wd.weaponName;
            this.skillName = wd.skillName;
            this.coloSkillLevel = wd.coloSkillLevel;
            this.coloSupportLevel = wd.coloSupportLevel;
            this.weaponType = wd.weaponType;
        }

        public void OverrideDetailsIfHigher(WeaponDetails wd)
        {
            if (coloSkillLevel == 0)
                coloSkillLevel = wd.coloSkillLevel;

            if (coloSupportLevel == 0)
                coloSupportLevel = wd.coloSupportLevel;

            if (skillName.Length == 0)
                skillName = wd.skillName;
        }

        public override int GetHashCode()
        {
            return weaponName.GetHashCode();
        }

        public override string ToString()
        {
            return weaponName + " level:" + coloSkillLevel + " " + skillName + " level:" + coloSupportLevel;
        }

        public string GetWeaponNameWithLevel()
        {
            return weaponName + " - " + skillName + " level:" +coloSkillLevel;
        }

        public string GetSupportWeaponWithLevel()
        {
            return weaponName + " - " + skillName + " level:" + coloSupportLevel;
        }

        public string GetWeaponNameWithColoSkillLevel()
        {
            return weaponName + " level:" + coloSkillLevel;
        }

        public string GetWeaponName(int space = 0)
        {
            string spacing = string.Empty;
            for (int i = 0; i < space; ++i)
                spacing += " ";

            return weaponName + spacing;
        }

        public uint GetColoSkillLevel()
        {
            return coloSkillLevel;
        }

        public uint GetColoSupportLevel()
        {
            return coloSupportLevel;
        }

        public string GetSkillName()
        {
            return skillName;
        }

        public WeaponType GetWeaponType()
        {
            return weaponType;
        }

        string weaponName;
        string skillName;
        uint coloSkillLevel;
        uint coloSupportLevel;
        WeaponType weaponType = WeaponType.NONE;
    }

    class BuffDebuffEffectiveness
    {
        public BuffDebuffEffectiveness(int levelIn, float effectivenessIn)
        {
            level = levelIn;  //Not used atm
            effectiveness = effectivenessIn;
        }

        public int level { get; private set; }
        public float effectiveness { get; private set; }
    }

    //Holds what we read from the weaponsDictionary
    class Weapon
    {
        public Weapon(string weaponTypeIn, float targetsIn, float damageIn, float recoveryIn, float patkValueIn, float matkValueIn, float pdefValueIn, float mdefValueIn, string elementalTypeIn)
        {
            WeaponType wt = WeaponType.NONE;
            ElementalType et = ElementalType.NONE;

            Enum.TryParse(weaponTypeIn, out wt);
            Enum.TryParse(elementalTypeIn, out et);


            weaponType = wt;
            elementalType = et;
            targets = targetsIn;
            damage = damageIn;
            recovery = recoveryIn;
            patkValue = patkValueIn;
            matkValue = matkValueIn;
            pdefValue = pdefValueIn;
            mdefValue = mdefValueIn;
        }

        public override string ToString()
        {
            return weaponType.ToString() + "," + targets + "," + damage + "," + recovery + "," + patkValue + "," + matkValue + "," + pdefValue + "," + mdefValue;
        }

        public WeaponType weaponType { get; private set; }
        public ElementalType elementalType { get; private set; }
        public float targets { get; private set; }
        public float damage { get; private set; }
        public float recovery { get; private set; }
        public float patkValue { get; private set; }
        public float matkValue { get; private set; }
        public float pdefValue { get; private set; }
        public float mdefValue { get; private set; }

    }

}
