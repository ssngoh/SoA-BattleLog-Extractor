using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace SinoParser
{
    class Exporter
    {
        public Exporter()
        {

        }

        public void ExportWeaponHash(Dictionary<string, Dictionary<string, WeaponDetails>> weaponHash, bool includeWeaponSummary = false)
        {
            var friendlyWeaponHashLog = new StreamWriter(Utilities.processDirectory + "/weaponHashFriendly.txt");
            var enemyWeaponHashLog = new StreamWriter(Utilities.processDirectory + "/weaponHashEnemy.txt");
            var weaponHashLogAll = new StreamWriter(Utilities.processDirectory+"/weaponHash.txt");
            StreamWriter weaponHashLog = null;

            foreach (KeyValuePair<string, Dictionary<string, WeaponDetails>> kvp in weaponHash)
            {
                if (includeWeaponSummary)
                {
                    if (Utilities.IsEnemy(kvp.Key))
                        weaponHashLog = enemyWeaponHashLog;
                    else if (Utilities.IsFriendly(kvp.Key))
                        weaponHashLog = friendlyWeaponHashLog;
                    else
                        continue;
                }
                else
                    weaponHashLog = weaponHashLogAll;

                weaponHashLog.WriteLine("****" + kvp.Key + "****");

                if(includeWeaponSummary && Utilities.isPlayerVG(kvp.Key))
                {
                    ExportWeaponHashVG(ref weaponHashLog, kvp.Key, kvp.Value);
                    //weaponHashLog.WriteLine("-------------------------------------------------------------------");
                    weaponHashLog.WriteLine();
                    continue;
                }

                string fire = "Fire({0})";
                string water = "Water({0})";
                string wind = "Wind({0})";

                string fireHarpString = "Harp({0})\n";
                string waterHarpString = "Harp({0})\n";
                string windHarpString = "Harp({0})\n";

                string fireTomeString = "Tome({0})\n";
                string waterTomeString = "Tome({0})\n";
                string windTomeString = "Tome({0})\n";

                string fireStaffString = "Staff({0})\n";
                string waterStaffString = "Staff({0})\n";
                string windStaffString = "Staff({0})\n";

                int fireWeapons = 0;
                int waterWeapons = 0;
                int windWeapons = 0;

                int fireHarpWeapons = 0;
                int fireTomeWeapons = 0;
                int fireStaffWeapons = 0;
                int waterHarpWeapons = 0;
                int waterTomeWeapons = 0;
                int waterStaffWeapons = 0;
                int windHarpWeapons = 0;
                int windTomeWeapons = 0;
                int windStaffWeapons = 0;

                Stats fireStatsbuff = new Stats();
                Stats waterStatsbuff = new Stats();
                Stats windStatsbuff = new Stats();

                Stats fireStatsDebuff = new Stats();
                Stats waterStatsDebuff = new Stats();
                Stats windStatsDebuff = new Stats();

                int fireHealing = 0;
                int waterHealing = 0;
                int windHealing = 0;

                float sbMultiplier = Utilities.ExtractSupportBoonBonus(kvp.Value);
                float recoveryMultiplier = Utilities.ExtractRecoverySupportBonus(kvp.Value);
                

                foreach (KeyValuePair<string, WeaponDetails> kvpInner in kvp.Value)
                {
                    weaponHashLog.WriteLine(kvpInner.Value.ToString());

                    if (includeWeaponSummary)
                    {
                        WeaponType weaponType = kvpInner.Value.GetWeaponType();
                        Tuple<Stats, ElementalType, WeaponType> weaponStats;

                        if (weaponType == WeaponType.HARP || weaponType == WeaponType.TOME)
                            weaponStats = Utilities.GetBuffDebuffWeaponEffectiveness(kvp.Key, kvpInner.Value, sbMultiplier);
                        else if (weaponType == WeaponType.STAFF)
                            weaponStats = Utilities.GetStaffWeaponEffectiveness(kvp.Key, kvpInner.Value, recoveryMultiplier);
                        else
                            weaponStats = Utilities.GetOtherWeaponEffectiveness(kvp.Key, kvpInner.Value); //Not in used atm 

                        if (weaponStats.Item2 == ElementalType.FIRE)
                        {
                            if (weaponStats.Item3 == WeaponType.HARP)
                            {
                                fireStatsbuff += weaponStats.Item1;
                                fireHarpString += kvpInner.Value.GetWeaponName(1).PadRight(28, '-') + " " + weaponStats.Item1.ToStringWithPad();
                                fireHarpString += '\n';
                                ++fireHarpWeapons;
                            }
                            else if (weaponStats.Item3 == WeaponType.TOME)
                            {
                                fireStatsDebuff += weaponStats.Item1;
                                fireTomeString += kvpInner.Value.GetWeaponName(1).PadRight(28, '-') + " " + weaponStats.Item1.ToStringWithPad();
                                fireTomeString += '\n';
                                ++fireTomeWeapons;
                            }
                            else if (weaponStats.Item3 == WeaponType.STAFF)
                            {
                                fireStaffString += kvpInner.Value.GetWeaponName(1).PadRight(28, '-') + " " + "Healing: " + (weaponStats.Item1.pdef + weaponStats.Item1.mdef);
                                fireStaffString += '\n';
                                fireHealing += weaponStats.Item1.pdef + weaponStats.Item1.mdef;
                                ++fireStaffWeapons;
                            }

                            ++fireWeapons;
                        }
                        else if (weaponStats.Item2 == ElementalType.WATER)
                        {
                            if (weaponStats.Item3 == WeaponType.HARP)
                            {
                                waterStatsbuff += weaponStats.Item1;
                                waterHarpString += kvpInner.Value.GetWeaponName(1).PadRight(28, '-') + " " + weaponStats.Item1.ToStringWithPad();
                                waterHarpString += '\n';
                                ++waterHarpWeapons;
                            }
                            else if (weaponStats.Item3 == WeaponType.TOME)
                            {
                                waterStatsDebuff += weaponStats.Item1;
                                waterTomeString += kvpInner.Value.GetWeaponName(1).PadRight(28, '-') + " " + weaponStats.Item1.ToStringWithPad();
                                waterTomeString += '\n';
                                ++waterTomeWeapons;
                            }
                            else if (weaponStats.Item3 == WeaponType.STAFF)
                            {
                                waterStaffString += kvpInner.Value.GetWeaponName(1).PadRight(28, '-') + " " + "Healing: " + (weaponStats.Item1.pdef + weaponStats.Item1.mdef);
                                waterStaffString += '\n';
                                waterHealing += weaponStats.Item1.pdef + weaponStats.Item1.mdef;
                                ++waterStaffWeapons;
                            }

                            ++waterWeapons;
                        }
                        else if (weaponStats.Item2 == ElementalType.WIND)
                        {
                            if (weaponStats.Item3 == WeaponType.HARP)
                            {
                                windStatsbuff += weaponStats.Item1;
                                windHarpString += kvpInner.Value.GetWeaponName(1).PadRight(28, '-') + " " + weaponStats.Item1.ToStringWithPad();
                                windHarpString += '\n';
                                ++windHarpWeapons;
                            }
                            else if (weaponStats.Item3 == WeaponType.TOME)
                            {
                                windStatsDebuff += weaponStats.Item1;
                                windTomeString += kvpInner.Value.GetWeaponName(1).PadRight(28, '-') + " " + weaponStats.Item1.ToStringWithPad();
                                windTomeString += '\n';
                                ++windTomeWeapons;
                            }
                            else if (weaponStats.Item3 == WeaponType.STAFF)
                            {
                                windStaffString += kvpInner.Value.GetWeaponName(1).PadRight(28, '-') + " " + "Healing: " + (weaponStats.Item1.pdef + weaponStats.Item1.mdef);
                                windStaffString += '\n';
                                windHealing += weaponStats.Item1.pdef + weaponStats.Item1.mdef;
                                ++windStaffWeapons;
                            }

                            ++windWeapons;
                        }
                        else
                            Utilities.WriteToErrorLog("Weapon elemental Type is NONE:" + kvpInner.Value);
                    }
                }

                if (includeWeaponSummary)
                {
                    weaponHashLog.WriteLine();
                    weaponHashLog.WriteLine();

                    if(sbMultiplier > recoveryMultiplier)
                        weaponHashLog.WriteLine(string.Format("Weapon effectiveness: SB multiplier {0}. PATK {1}. PDEF {2}. MATK {3}. MDEF {4}.", sbMultiplier, 
                                                Utilities.GetInitialStats(kvp.Key).patk, Utilities.GetInitialStats(kvp.Key).pdef,
                                                Utilities.GetInitialStats(kvp.Key).matk, Utilities.GetInitialStats(kvp.Key).mdef));
                    else
                        weaponHashLog.WriteLine(string.Format("Weapon effectiveness: RS multiplier {0}. PATK {1}. PDEF {2}. MATK {3}. MDEF {4}.", recoveryMultiplier,
                                                Utilities.GetInitialStats(kvp.Key).patk, Utilities.GetInitialStats(kvp.Key).pdef,
                                                Utilities.GetInitialStats(kvp.Key).matk, Utilities.GetInitialStats(kvp.Key).mdef));

                    weaponHashLog.WriteLine("-------------------------------------------------------------------");
                    weaponHashLog.WriteLine(string.Format(fire, fireWeapons));
                    weaponHashLog.WriteLine();

                    if (fireHarpWeapons > 0)
                        weaponHashLog.WriteLine(string.Format(fireHarpString, fireHarpWeapons));
                    if (fireTomeWeapons > 0)
                        weaponHashLog.WriteLine(string.Format(fireTomeString, fireTomeWeapons));
                    if (fireStaffWeapons > 0)
                        weaponHashLog.WriteLine(string.Format(fireStaffString, fireStaffWeapons));

                    weaponHashLog.WriteLine("Fire Weapons Summary");
                    if (fireStatsbuff.GetTotalStats() > 0 || fireStatsDebuff.GetTotalStats() > 0)
                        weaponHashLog.WriteLine("\t\tPATK\tPDEF\tMATK\tMDEF");
                    if (fireStatsbuff.GetTotalStats() > 0)
                        weaponHashLog.WriteLine(string.Format("Buff\t{0}\t{1}\t{2}\t{3}\t=> {4}", fireStatsbuff.patk.ToString("D4"), fireStatsbuff.pdef.ToString("D4"), fireStatsbuff.matk.ToString("D4"), fireStatsbuff.mdef.ToString("D4"), fireStatsbuff.GetTotalStats().ToString("D4")));
                    if (fireStatsDebuff.GetTotalStats() > 0)
                        weaponHashLog.WriteLine(string.Format("Debuff\t{0}\t{1}\t{2}\t{3}\t=> {4}", fireStatsDebuff.patk.ToString("D4"), fireStatsDebuff.pdef.ToString("D4"), fireStatsDebuff.matk.ToString("D4"), fireStatsDebuff.mdef.ToString("D4"), fireStatsDebuff.GetTotalStats().ToString("D4")));
                    if (fireHealing > 0)
                        weaponHashLog.WriteLine(string.Format("Healing\t\t\t\t\t\t\t\t\t=> {0}", fireHealing));
                    weaponHashLog.WriteLine("-------------------------------------------------------------------");

                    weaponHashLog.WriteLine(string.Format(water, waterWeapons));
                    weaponHashLog.WriteLine();

                    if (waterHarpWeapons > 0)
                        weaponHashLog.WriteLine(string.Format(waterHarpString, waterHarpWeapons));
                    if (waterTomeWeapons > 0)
                        weaponHashLog.WriteLine(string.Format(waterTomeString, waterTomeWeapons));
                    if (waterStaffWeapons > 0)
                        weaponHashLog.WriteLine(string.Format(waterStaffString, waterStaffWeapons));

                    weaponHashLog.WriteLine("Water Weapons Summary");
                    if (waterStatsbuff.GetTotalStats() > 0 || waterStatsDebuff.GetTotalStats() > 0)
                        weaponHashLog.WriteLine("\t\tPATK\tPDEF\tMATK\tMDEF");
                    if (waterStatsbuff.GetTotalStats() > 0)
                        weaponHashLog.WriteLine(string.Format("Buff\t{0}\t{1}\t{2}\t{3}\t=> {4}", waterStatsbuff.patk.ToString("D4"), waterStatsbuff.pdef.ToString("D4"), waterStatsbuff.matk.ToString("D4"), waterStatsbuff.mdef.ToString("D4"), waterStatsbuff.GetTotalStats().ToString("D4")));
                    if (waterStatsDebuff.GetTotalStats() > 0)
                        weaponHashLog.WriteLine(string.Format("Debuff\t{0}\t{1}\t{2}\t{3}\t=> {4}", waterStatsDebuff.patk.ToString("D4"), waterStatsDebuff.pdef.ToString("D4"), waterStatsDebuff.matk.ToString("D4"), waterStatsDebuff.mdef.ToString("D4"), waterStatsDebuff.GetTotalStats().ToString("D4")));
                    if (waterHealing > 0)
                        weaponHashLog.WriteLine(string.Format("Healing\t\t\t\t\t\t\t\t\t=> {0}", waterHealing));
                    weaponHashLog.WriteLine("-------------------------------------------------------------------");

                    weaponHashLog.WriteLine(string.Format(wind, windWeapons));
                    weaponHashLog.WriteLine();

                    if (windHarpWeapons > 0)
                        weaponHashLog.WriteLine(string.Format(windHarpString, windHarpWeapons));
                    if (windTomeWeapons > 0)
                        weaponHashLog.WriteLine(string.Format(windTomeString, windTomeWeapons));
                    if (windStaffWeapons > 0)
                        weaponHashLog.WriteLine(string.Format(windStaffString, windStaffWeapons));

                    weaponHashLog.WriteLine("Wind Weapons Summary");

                    if (windStatsbuff.GetTotalStats() > 0 || windStatsDebuff.GetTotalStats() > 0)
                        weaponHashLog.WriteLine("\t\tPATK\tPDEF\tMATK\tMDEF");
                    if (windStatsbuff.GetTotalStats() > 0)
                        weaponHashLog.WriteLine(string.Format("Buff\t{0}\t{1}\t{2}\t{3}\t=> {4}", windStatsbuff.patk.ToString("D4"), windStatsbuff.pdef.ToString("D4"), windStatsbuff.matk.ToString("D4"), windStatsbuff.mdef.ToString("D4"), windStatsbuff.GetTotalStats().ToString("D4")));
                    if (windStatsDebuff.GetTotalStats() > 0)
                        weaponHashLog.WriteLine(string.Format("Debuff\t{0}\t{1}\t{2}\t{3}\t=> {4}", windStatsDebuff.patk.ToString("D4"), windStatsDebuff.pdef.ToString("D4"), windStatsDebuff.matk.ToString("D4"), windStatsDebuff.mdef.ToString("D4"), windStatsDebuff.GetTotalStats().ToString("D4")));
                    if (windHealing > 0)
                        weaponHashLog.WriteLine(string.Format("Healing\t\t\t\t\t\t\t\t\t=> {0}", windHealing));

                    weaponHashLog.WriteLine("-------------------------------------------------------------------");

                    weaponHashLog.WriteLine(string.Format("All Weapons({0})", fireWeapons + waterWeapons + windWeapons));
                    weaponHashLog.WriteLine();
                    weaponHashLog.WriteLine("\t\tPATK\tPDEF\tMATK\tMDEF");
                    weaponHashLog.WriteLine(string.Format("Buff\t{0}\t{1}\t{2}\t{3}\t=> {4}", (fireStatsbuff.patk + waterStatsbuff.patk + windStatsbuff.patk).ToString("D4"), (fireStatsbuff.pdef + waterStatsbuff.pdef + windStatsbuff.pdef).ToString("D4"),
                                                                                      (fireStatsbuff.matk + waterStatsbuff.matk + windStatsbuff.matk).ToString("D4"), (fireStatsbuff.mdef + waterStatsbuff.mdef + windStatsbuff.mdef).ToString("D4"),
                                                                                      (fireStatsbuff.GetTotalStats() + waterStatsbuff.GetTotalStats() + windStatsbuff.GetTotalStats()).ToString("D4")));

                    weaponHashLog.WriteLine(string.Format("Debuff\t{0}\t{1}\t{2}\t{3}\t=> {4}", (fireStatsDebuff.patk + waterStatsDebuff.patk + windStatsDebuff.patk).ToString("D4"), (fireStatsDebuff.pdef + waterStatsDebuff.pdef + windStatsDebuff.pdef).ToString("D4"),
                                                                                        (fireStatsDebuff.matk + waterStatsDebuff.matk + windStatsDebuff.matk).ToString("D4"), (fireStatsDebuff.mdef + waterStatsDebuff.mdef + windStatsDebuff.mdef).ToString("D4"),
                                                                                        (fireStatsDebuff.GetTotalStats() + waterStatsDebuff.GetTotalStats() + windStatsDebuff.GetTotalStats()).ToString("D4")));
                    weaponHashLog.WriteLine(string.Format("Healing\t\t\t\t\t\t\t\t\t=> {0}", fireHealing + waterHealing + windHealing));
                    weaponHashLog.WriteLine("-------------------------------------------------------------------");
                    weaponHashLog.WriteLine();
                }
            }

            friendlyWeaponHashLog.Close();
            enemyWeaponHashLog.Close();
            weaponHashLogAll.Close();
        }

        void ExportWeaponHashVG(ref StreamWriter weaponHashLog, string name, Dictionary<string, WeaponDetails> weaponHash)
        {
            //Could have probably just made the whole thing into a separate function instead of repeating fire/water/wind
            int fireWeapons = 0;
            int fireSwordWeapons = 0;
            int fireHammerWeapons = 0;
            int fireBowWeapons = 0;
            int fireSpearWeapons = 0;

            string fireWeaponsString = "Fire({0})";
            string fireSwordAmountString = "Sword({0})";
            string fireHammerAmountString = "Hammer({0})";
            string fireBowAmountString = "Bow({0})";
            string fireSpearAmountString = "Spear({0})";

            string fireSwordString = string.Empty;
            string fireHammerString = string.Empty;
            string fireBowString = string.Empty;
            string fireSpearString = string.Empty;

            int waterWeapons = 0;
            int waterSwordWeapons = 0;
            int waterHammerWeapons = 0;
            int waterBowWeapons = 0;
            int waterSpearWeapons = 0;

            string waterWeaponsString = "Water({0})";
            string waterSwordAmountString = "Sword({0})";
            string waterHammerAmountString = "Hammer({0})";
            string waterBowAmountString = "Bow({0})";
            string waterSpearAmountString = "Spear({0})";

            string waterSwordString = string.Empty;
            string waterHammerString = string.Empty;
            string waterBowString = string.Empty;
            string waterSpearString = string.Empty;

            int windWeapons = 0;
            int windSwordWeapons = 0;
            int windHammerWeapons = 0;
            int windBowWeapons = 0;
            int windSpearWeapons = 0;

            string windWeaponsString = "Wind({0})";
            string windSwordAmountString = "Sword({0})";
            string windHammerAmountString = "Hammer({0})";
            string windBowAmountString = "Bow({0})";
            string windSpearAmountString = "Spear({0})";

            string windSwordString = string.Empty;
            string windHammerString = string.Empty;
            string windBowString = string.Empty;
            string windSpearString = string.Empty;



            float dcMultiplier = Utilities.ExtractDauntlessCourageBonus(weaponHash);

            foreach (KeyValuePair<string, WeaponDetails> kvpInner in weaponHash)
            {
                weaponHashLog.WriteLine(kvpInner.Value.ToString());

                WeaponType weaponType = kvpInner.Value.GetWeaponType();
                Tuple<int, ElementalType, WeaponType> weaponStats;

                weaponStats = Utilities.GetVGWeaponEffectiveness(name, kvpInner.Value);

                if (weaponStats.Item2 == ElementalType.FIRE)
                {
                    switch(weaponStats.Item3)
                    {
                        case WeaponType.SWORD:
                            WriteWeaponStatsString(ref fireSwordString, name, kvpInner.Value, dcMultiplier);
                            ++fireSwordWeapons;
                            break;

                        case WeaponType.HAMMER:
                            WriteWeaponStatsString(ref fireHammerString, name, kvpInner.Value, dcMultiplier);
                            ++fireHammerWeapons;
                            break;

                        case WeaponType.BOW:
                            WriteWeaponStatsString(ref fireBowString, name, kvpInner.Value, dcMultiplier);
                            ++fireBowWeapons;
                            break;

                        case WeaponType.SPEAR:
                            WriteWeaponStatsString(ref fireSpearString, name, kvpInner.Value, dcMultiplier);
                            ++fireSpearWeapons;
                            break;

                        default:
                            break;
                    }

                    ++fireWeapons;
                }
                else if (weaponStats.Item2 == ElementalType.WATER)
                {
                    switch (weaponStats.Item3)
                    {
                        case WeaponType.SWORD:
                            WriteWeaponStatsString(ref waterSwordString, name, kvpInner.Value, dcMultiplier);
                            ++waterSwordWeapons;
                            break;

                        case WeaponType.HAMMER:
                            WriteWeaponStatsString(ref waterHammerString, name, kvpInner.Value, dcMultiplier);
                            ++waterHammerWeapons;
                            break;

                        case WeaponType.BOW:
                            WriteWeaponStatsString(ref waterBowString, name, kvpInner.Value, dcMultiplier);
                            ++waterBowWeapons;
                            break;

                        case WeaponType.SPEAR:
                            WriteWeaponStatsString(ref waterSpearString, name, kvpInner.Value, dcMultiplier);
                            ++waterSpearWeapons;
                            break;

                        default:
                            break;
                    }

                    ++waterWeapons;
                }
                else if (weaponStats.Item2 == ElementalType.WIND)
                {
                    switch (weaponStats.Item3)
                    {
                        case WeaponType.SWORD:
                            WriteWeaponStatsString(ref windSwordString, name, kvpInner.Value, dcMultiplier);
                            ++windSwordWeapons;
                            break;

                        case WeaponType.HAMMER:
                            WriteWeaponStatsString(ref windHammerString, name, kvpInner.Value, dcMultiplier);
                            ++windHammerWeapons;
                            break;

                        case WeaponType.BOW:
                            WriteWeaponStatsString(ref windBowString, name, kvpInner.Value, dcMultiplier);
                            ++windBowWeapons;
                            break;

                        case WeaponType.SPEAR:
                            WriteWeaponStatsString(ref windSpearString, name, kvpInner.Value, dcMultiplier);
                            ++windSpearWeapons;
                            break;

                        default:
                            break;
                    }

                    ++windWeapons;
                }
                else
                    Utilities.WriteToErrorLog("Weapon elemental Type is NONE:" + kvpInner.Value);
            }

            weaponHashLog.WriteLine();
            weaponHashLog.WriteLine();
            weaponHashLog.WriteLine(string.Format("Weapon effectiveness: DC multiplier {0}. PATK {1}. MATK {2}.", dcMultiplier, Utilities.GetInitialStats(name).patk, Utilities.GetInitialStats(name).matk));
            weaponHashLog.WriteLine();
            weaponHashLog.WriteLine("- WithoutDC => Damage assuming current skill level without DC multiplier");
            weaponHashLog.WriteLine("- WithDC => Damage assuming current skill level with DC multiplier");
            weaponHashLog.WriteLine("- SLvl1+DC => Damage assuming skill level 1 with DC multiplier");
            weaponHashLog.WriteLine("- SLvl10+DC => Damage assuming skill level 10 with DC multiplier");
            weaponHashLog.WriteLine("- SLvl15+DC => Damage assuming skill level 15 with DC multiplier");
            weaponHashLog.WriteLine("- SLvl20+DC => Damage assuming skill level 20 with DC multiplier");
            weaponHashLog.WriteLine("- VS50kDef => Average damage when hitting a target with 50k def with current DC and SLvl");
            weaponHashLog.WriteLine();
            weaponHashLog.WriteLine("*For hammers and spears, the values are lesser because assume 1 target instead of 2.");
            weaponHashLog.WriteLine("This is because I want the actual damage dealt to a single target.");

            weaponHashLog.WriteLine("-------------------------------------------------------------------");

            if (fireWeapons > 0)
            {
                weaponHashLog.WriteLine(string.Format(fireWeaponsString, fireWeapons));
                weaponHashLog.WriteLine();

                if (fireSwordWeapons > 0)
                {
                    weaponHashLog.WriteLine(string.Format(fireSwordAmountString, fireSwordWeapons));
                    weaponHashLog.WriteLine("--------------------------------------------------------------------------");
                    weaponHashLog.WriteLine(fireSwordString);
                }

                if (fireHammerWeapons > 0)
                {
                    weaponHashLog.WriteLine(string.Format(fireHammerAmountString, fireHammerWeapons));
                    weaponHashLog.WriteLine("--------------------------------------------------------------------------");
                    weaponHashLog.WriteLine(fireHammerString);
                }

                if (fireBowWeapons > 0)
                {
                    weaponHashLog.WriteLine(string.Format(fireBowAmountString, fireBowWeapons));
                    weaponHashLog.WriteLine("--------------------------------------------------------------------------");
                    weaponHashLog.WriteLine(fireBowString);
                }

                if (fireSpearWeapons > 0)
                {
                    weaponHashLog.WriteLine(string.Format(fireSpearAmountString, fireSpearWeapons));
                    weaponHashLog.WriteLine("--------------------------------------------------------------------------");
                    weaponHashLog.WriteLine(fireSpearString);
                }
            }

            if (waterWeapons > 0)
            {
                weaponHashLog.WriteLine(string.Format(waterWeaponsString, waterWeapons));
                weaponHashLog.WriteLine();

                if (waterSwordWeapons > 0)
                {
                    weaponHashLog.WriteLine(string.Format(waterSwordAmountString, waterSwordWeapons));
                    weaponHashLog.WriteLine("--------------------------------------------------------------------------");
                    weaponHashLog.WriteLine(waterSwordString);
                }

                if (waterHammerWeapons > 0)
                {
                    weaponHashLog.WriteLine(string.Format(waterHammerAmountString, waterHammerWeapons));
                    weaponHashLog.WriteLine("--------------------------------------------------------------------------");
                    weaponHashLog.WriteLine(waterHammerString);
                }

                if (waterBowWeapons > 0)
                {
                    weaponHashLog.WriteLine(string.Format(waterBowAmountString, waterBowWeapons));
                    weaponHashLog.WriteLine("--------------------------------------------------------------------------");
                    weaponHashLog.WriteLine(waterBowString);
                }

                if (waterSpearWeapons > 0)
                {
                    weaponHashLog.WriteLine(string.Format(waterSpearAmountString, waterSpearWeapons));
                    weaponHashLog.WriteLine("--------------------------------------------------------------------------");
                    weaponHashLog.WriteLine(waterSpearString);
                }
            }

            if (windWeapons > 0)
            {
                weaponHashLog.WriteLine(string.Format(windWeaponsString, windWeapons));
                weaponHashLog.WriteLine();

                if (windSwordWeapons > 0)
                {
                    weaponHashLog.WriteLine(string.Format(windSwordAmountString, windSwordWeapons));
                    weaponHashLog.WriteLine("--------------------------------------------------------------------------");
                    weaponHashLog.WriteLine(windSwordString);
                }

                if (windHammerWeapons > 0)
                {
                    weaponHashLog.WriteLine(string.Format(windHammerAmountString, windHammerWeapons));
                    weaponHashLog.WriteLine("--------------------------------------------------------------------------");
                    weaponHashLog.WriteLine(windHammerString);
                }

                if (windBowWeapons > 0)
                {
                    weaponHashLog.WriteLine(string.Format(windBowAmountString, windBowWeapons));
                    weaponHashLog.WriteLine("--------------------------------------------------------------------------");
                    weaponHashLog.WriteLine(windBowString);
                }

                if (windSpearWeapons > 0)
                {
                    weaponHashLog.WriteLine(string.Format(windSpearAmountString, windSpearWeapons));
                    weaponHashLog.WriteLine("--------------------------------------------------------------------------");
                    weaponHashLog.WriteLine(windSpearString);
                }
            }
        }
    
        void WriteWeaponStatsString(ref string weaponString, string playerName, WeaponDetails weaponDetails, float DCBonus)
        {
            uint firstCheck = 1;
            uint secondCheck = 10;
            uint thirdCheck = 15;
            uint fourthCheck = 20;
            int fifthCheck = 55;
            
            weaponString += weaponDetails.GetWeaponNameWithColoSkillLevel();
            weaponString += '\n';
            weaponString += string.Format("\t\t\t\t\tWithoutDC\tWithDC\t\tSlvl{0}+DC\tSlvl{1}+DC\tSlvl{2}+DC\tSlvl{3}+DC\tVS{4}kDef", firstCheck, secondCheck, thirdCheck, fourthCheck, fifthCheck);
            weaponString += '\n';

            WriteWeaponStatsStringForClass(ref weaponString, playerName, weaponDetails, DCBonus, "Breaker ", 
                                           Utilities.CharClass.BREAKER,firstCheck, secondCheck, thirdCheck, fourthCheck, fifthCheck);
            weaponString += '\n';

            WriteWeaponStatsStringForClass(ref weaponString, playerName, weaponDetails, DCBonus, "Crusher ",
                                 Utilities.CharClass.CRUSHER, firstCheck, secondCheck, thirdCheck, fourthCheck, fifthCheck);
            weaponString += '\n';

            WriteWeaponStatsStringForClass(ref weaponString, playerName, weaponDetails, DCBonus, "CrusherHNM ",
                                Utilities.CharClass.CRUSHER_HNM, firstCheck, secondCheck, thirdCheck, fourthCheck, fifthCheck);
            weaponString += '\n';

            WriteWeaponStatsStringForClass(ref weaponString, playerName, weaponDetails, DCBonus, "Paladin ",
                                 Utilities.CharClass.PALADIN, firstCheck, secondCheck, thirdCheck, fourthCheck, fifthCheck);
            weaponString += '\n';

            WriteWeaponStatsStringForClass(ref weaponString, playerName, weaponDetails, DCBonus, "PaladinHNM ",
                                Utilities.CharClass.PALADIN_HNM, firstCheck, secondCheck, thirdCheck, fourthCheck, fifthCheck);
            weaponString += '\n';

            weaponString += "------------------------------------------------------------------------------------\n";

        }

        void WriteWeaponStatsStringForClass(ref string weaponString, string playerName, WeaponDetails weaponDetails, float DCBonus, string className,
                                            Utilities.CharClass charClass, uint firstCheck, uint secondCheck, uint thirdCheck, uint fourthCheck, int fifthCheck)
        {
            int noDCDmg = Utilities.GetVGWeaponEffectiveness(playerName, weaponDetails, 0, 1, charClass).Item1;
            int baseDmg = Utilities.GetVGWeaponEffectiveness(playerName, weaponDetails, 0, DCBonus, charClass).Item1;
            int firstDmg = Utilities.GetVGWeaponEffectiveness(playerName, weaponDetails, 0, DCBonus, charClass, firstCheck).Item1;
            int secondDmg = Utilities.GetVGWeaponEffectiveness(playerName, weaponDetails, 0, DCBonus, charClass, secondCheck).Item1;
            int thirdDmg = Utilities.GetVGWeaponEffectiveness(playerName, weaponDetails, 0, DCBonus, charClass, thirdCheck).Item1;
            int fourthDmg = Utilities.GetVGWeaponEffectiveness(playerName, weaponDetails, 0, DCBonus, charClass, fourthCheck).Item1;
            int fifthDmg = Utilities.GetVGWeaponEffectiveness(playerName, weaponDetails, fifthCheck * 1000, DCBonus, charClass).Item1;

            weaponString += string.Format("{0}\t{1}\t\t{2}\t\t{3}\t\t{4}\t\t{5}\t\t{6}\t\t{7}", className.PadRight(18, '-'), noDCDmg.ToString("D4"), baseDmg.ToString("D4"), 
                                                                                                firstDmg.ToString("D4"), secondDmg.ToString("D4"), thirdDmg.ToString("D4"), 
                                                                                                fourthDmg.ToString("D4"), fifthDmg.ToString("D4"));

        }

        public void ExportWeaponElementalSummary(Dictionary<string, Dictionary<string, WeaponDetails>> weaponHash)
        {
            var friendlyWeaponHashCSV = new StreamWriter(Utilities.processDirectory + "/friendlyWeaponElementalSummary.csv");
            var enemyWeaponHashCSV = new StreamWriter(Utilities.processDirectory + "/enemyWeaponElementalSummary.csv");
            var weaponHashLog = new StreamWriter(Utilities.processDirectory + "/weaponBuffDebuffElementalSummary.txt");

            int friendlyFireWeapons = 0;
            int friendlyWaterWeapons = 0;
            int friendlyWindWeapons = 0;

            Stats friendlyFireStatsbuff = new Stats();
            Stats friendlyWaterStatsbuff = new Stats();
            Stats friendlyWindStatsbuff = new Stats();

            Stats friendlyFireStatsDebuff = new Stats();
            Stats friendlyWaterStatsDebuff = new Stats();
            Stats friendlyWindStatsDebuff = new Stats();

            int friendlyFireStaves = 0;
            int friendlyWaterStaves = 0;
            int friendlyWindStaves = 0;

            int friendlyFireHealing = 0;
            int friendlyWaterHealing = 0;
            int friendlyWindHealing = 0;

            int friendlyFireVGWeapons = 0;
            int friendlyWaterVGWeapons = 0;
            int friendlyWindVGWeapons = 0;

            int friendlyBonusComboWeapons = 0;

            //Enemy
            int enemyFireWeapons = 0;
            int enemyWaterWeapons = 0;
            int enemyWindWeapons = 0;

            Stats enemyFireStatsbuff = new Stats();
            Stats enemyWaterStatsbuff = new Stats();
            Stats enemyWindStatsbuff = new Stats();

            Stats enemyFireStatsDebuff = new Stats();
            Stats enemyWaterStatsDebuff = new Stats();
            Stats enemyWindStatsDebuff = new Stats();

            //For staves
            int enemyFireStaves = 0;
            int enemyWaterStaves = 0;
            int enemyWindStaves = 0;

            int enemyFireHealing = 0;
            int enemyWaterHealing = 0;
            int enemyWindHealing = 0;

            int enemyFireVGWeapons = 0;
            int enemyWaterVGWeapons = 0;
            int enemyWindVGWeapons = 0;

            int enemyBonusComboWeapons = 0;

            foreach (KeyValuePair<string, Dictionary<string, WeaponDetails>> kvp in weaponHash)
            {
                // weaponHashLog.WriteLine("****************************************" + kvp.Key + "****************************************");

                int fireWeapons = 0, fireStaves = 0, fireVGWeapons = 0;
                int waterWeapons = 0, waterStaves = 0, waterVGWeapons = 0;
                int windWeapons = 0, windStaves = 0, windVGWeapons = 0;
                int comboWeapons = 0;

                int fireHealing = 0;
                int waterHealing = 0;
                int windHealing = 0;

                Stats fireStatsbuff = new Stats();
                Stats waterStatsbuff = new Stats();
                Stats windStatsbuff = new Stats();

                Stats fireStatsDebuff = new Stats();
                Stats waterStatsDebuff = new Stats();
                Stats windStatsDebuff = new Stats();

                float boonMultiplier = Utilities.ExtractSupportBoonBonus(kvp.Value);
                float recoveryMultiplier = Utilities.ExtractRecoverySupportBonus(kvp.Value);

                foreach (KeyValuePair<string, WeaponDetails> kvpInner in kvp.Value)
                {
                    WeaponType weaponType = kvpInner.Value.GetWeaponType();
                    Tuple<Stats, ElementalType, WeaponType> weaponStats;

                    if (weaponType == WeaponType.HARP || weaponType == WeaponType.TOME)
                        weaponStats = Utilities.GetBuffDebuffWeaponEffectiveness(kvp.Key, kvpInner.Value, boonMultiplier);
                    else if (weaponType == WeaponType.STAFF)
                        weaponStats = Utilities.GetStaffWeaponEffectiveness(kvp.Key, kvpInner.Value, recoveryMultiplier);
                    else
                        weaponStats = Utilities.GetOtherWeaponEffectiveness(kvp.Key, kvpInner.Value);

                    if (weaponStats.Item2 == ElementalType.FIRE)
                    {
                        if (weaponStats.Item3 == WeaponType.HARP)
                        {
                            fireStatsbuff += weaponStats.Item1;
                            ++fireWeapons;
                        }
                        else if (weaponStats.Item3 == WeaponType.TOME)
                        {
                            fireStatsDebuff += weaponStats.Item1;
                            ++fireWeapons;
                        }
                        else if (weaponStats.Item3 == WeaponType.STAFF)
                        {
                            fireHealing += weaponStats.Item1.pdef + weaponStats.Item1.mdef;
                            ++fireStaves;
                        }
                        else
                            ++fireVGWeapons;
                    }
                    else if (weaponStats.Item2 == ElementalType.WATER)
                    {
                        if (weaponStats.Item3 == WeaponType.HARP)
                        {
                            waterStatsbuff += weaponStats.Item1;
                            ++waterWeapons;
                        }
                        else if (weaponStats.Item3 == WeaponType.TOME)
                        {
                            waterStatsDebuff += weaponStats.Item1;
                            ++waterWeapons;
                        }
                        else if (weaponStats.Item3 == WeaponType.STAFF)
                        {
                            waterHealing += weaponStats.Item1.pdef + weaponStats.Item1.mdef;
                            ++waterStaves;
                        }
                        else
                            ++waterVGWeapons;
                    }
                    else if (weaponStats.Item2 == ElementalType.WIND)
                    {
                        if (weaponStats.Item3 == WeaponType.HARP)
                        {
                            windStatsbuff += weaponStats.Item1;
                            ++windWeapons;
                        }
                        else if (weaponStats.Item3 == WeaponType.TOME)
                        {
                            windStatsDebuff += weaponStats.Item1;
                            ++windWeapons;
                        }
                        else if (weaponStats.Item3 == WeaponType.STAFF)
                        {
                            windHealing += weaponStats.Item1.pdef + weaponStats.Item1.mdef;
                            ++windStaves;
                        }
                        else
                            ++windVGWeapons;
                    }
                    else
                        Utilities.WriteToErrorLog("Weapon elemental Type is NONE:" + kvpInner.Value);

                    comboWeapons = Utilities.CheckIfComboWeapon(kvpInner.Value.GetWeaponName()) ? comboWeapons + 1 : comboWeapons;
                }

                if (Utilities.IsFriendly(kvp.Key))
                {
                    friendlyFireStatsbuff += fireStatsbuff;
                    friendlyWaterStatsbuff += waterStatsbuff;
                    friendlyWindStatsbuff += windStatsbuff;

                    friendlyFireStatsDebuff += fireStatsDebuff;
                    friendlyWaterStatsDebuff += waterStatsDebuff;
                    friendlyWindStatsDebuff += windStatsDebuff;

                    friendlyFireWeapons += fireWeapons;
                    friendlyWaterWeapons += waterWeapons;
                    friendlyWindWeapons += windWeapons;

                    friendlyFireStaves += fireStaves;
                    friendlyWaterStaves += waterStaves;
                    friendlyWindStaves += windStaves;

                    friendlyFireHealing += fireHealing;
                    friendlyWaterHealing += waterHealing;
                    friendlyWindHealing += windHealing;

                    friendlyFireVGWeapons += fireVGWeapons;
                    friendlyWaterVGWeapons += waterVGWeapons;
                    friendlyWindVGWeapons += windVGWeapons;

                    friendlyBonusComboWeapons += comboWeapons;

                }
                else if (Utilities.IsEnemy(kvp.Key))
                {
                    enemyFireStatsbuff += fireStatsbuff;
                    enemyWaterStatsbuff += waterStatsbuff;
                    enemyWindStatsbuff += windStatsbuff;

                    enemyFireStatsDebuff += fireStatsDebuff;
                    enemyWaterStatsDebuff += waterStatsDebuff;
                    enemyWindStatsDebuff += windStatsDebuff;

                    enemyFireWeapons += fireWeapons;
                    enemyWaterWeapons += waterWeapons;
                    enemyWindWeapons += windWeapons;

                    enemyFireStaves += fireStaves;
                    enemyWaterStaves += waterStaves;
                    enemyWindStaves += windStaves;

                    enemyFireHealing += fireHealing;
                    enemyWaterHealing += waterHealing;
                    enemyWindHealing += windHealing;

                    enemyFireVGWeapons += fireVGWeapons;
                    enemyWaterVGWeapons += waterVGWeapons;
                    enemyWindVGWeapons += windVGWeapons;

                    enemyBonusComboWeapons += comboWeapons;
                }
                else
                    Utilities.WriteToErrorLog("Name doesn't exist in both friendly and enemy hash: " + kvp.Key);
            }

            weaponHashLog.WriteLine("****************************************" + "FRIENDLY BUFF/DEBUFF SUMMARY " + "****************************************");
            weaponHashLog.WriteLine("-------------------------------------------------------------------");
            weaponHashLog.WriteLine(string.Format("Fire Weapons Summary({0})", friendlyFireWeapons));
            weaponHashLog.WriteLine("\t\tPATK\tPDEF\tMATK\tMDEF");
            weaponHashLog.WriteLine(string.Format("Buff\t{0}\t{1}\t{2}\t{3}\t=> {4}", friendlyFireStatsbuff.patk.ToString("D4"), friendlyFireStatsbuff.pdef.ToString("D4"), friendlyFireStatsbuff.matk.ToString("D4"), friendlyFireStatsbuff.mdef.ToString("D4"), friendlyFireStatsbuff.GetTotalStats().ToString("D4")));
            weaponHashLog.WriteLine(string.Format("Debuff\t{0}\t{1}\t{2}\t{3}\t=> {4}", friendlyFireStatsDebuff.patk.ToString("D4"), friendlyFireStatsDebuff.pdef.ToString("D4"), friendlyFireStatsDebuff.matk.ToString("D4"), friendlyFireStatsDebuff.mdef.ToString("D4"), friendlyFireStatsDebuff.GetTotalStats().ToString("D4")));
            weaponHashLog.WriteLine("-------------------------------------------------------------------");

            weaponHashLog.WriteLine(string.Format("Water Weapons Summary({0})", friendlyWaterWeapons));
            weaponHashLog.WriteLine("\t\tPATK\tPDEF\tMATK\tMDEF");
            weaponHashLog.WriteLine(string.Format("Buff\t{0}\t{1}\t{2}\t{3}\t=> {4}", friendlyWaterStatsbuff.patk.ToString("D4"), friendlyWaterStatsbuff.pdef.ToString("D4"), friendlyWaterStatsbuff.matk.ToString("D4"), friendlyWaterStatsbuff.mdef.ToString("D4"), friendlyWaterStatsbuff.GetTotalStats().ToString("D4")));
            weaponHashLog.WriteLine(string.Format("Debuff\t{0}\t{1}\t{2}\t{3}\t=> {4}", friendlyWaterStatsDebuff.patk.ToString("D4"), friendlyWaterStatsDebuff.pdef.ToString("D4"), friendlyWaterStatsDebuff.matk.ToString("D4"), friendlyWaterStatsDebuff.mdef.ToString("D4"), friendlyWaterStatsDebuff.GetTotalStats().ToString("D4")));
            weaponHashLog.WriteLine("-------------------------------------------------------------------");

            weaponHashLog.WriteLine(string.Format("Wind Weapons Summary({0})", friendlyWindWeapons));
            weaponHashLog.WriteLine("\t\tPATK\tPDEF\tMATK\tMDEF");
            weaponHashLog.WriteLine(string.Format("Buff\t{0}\t{1}\t{2}\t{3}\t=> {4}", friendlyWindStatsbuff.patk.ToString("D4"), friendlyWindStatsbuff.pdef.ToString("D4"), friendlyWindStatsbuff.matk.ToString("D4"), friendlyWindStatsbuff.mdef.ToString("D4"), friendlyWindStatsbuff.GetTotalStats().ToString("D4")));
            weaponHashLog.WriteLine(string.Format("Debuff\t{0}\t{1}\t{2}\t{3}\t=> {4}", friendlyWindStatsDebuff.patk.ToString("D4"), friendlyWindStatsDebuff.pdef.ToString("D4"), friendlyWindStatsDebuff.matk.ToString("D4"), friendlyWindStatsDebuff.mdef.ToString("D4"), friendlyWindStatsDebuff.GetTotalStats().ToString("D4")));
            weaponHashLog.WriteLine("-------------------------------------------------------------------");

            weaponHashLog.WriteLine(string.Format("All Weapons({0})", friendlyFireWeapons + friendlyWaterWeapons + friendlyWindWeapons));
            weaponHashLog.WriteLine();
            weaponHashLog.WriteLine("\t\tPATK\tPDEF\tMATK\tMDEF");
            weaponHashLog.WriteLine(string.Format("Buff\t{0}\t{1}\t{2}\t{3}\t=> {4}", (friendlyFireStatsbuff.patk + friendlyWaterStatsbuff.patk + friendlyWindStatsbuff.patk).ToString("D4"), (friendlyFireStatsbuff.pdef + friendlyWaterStatsbuff.pdef + friendlyWindStatsbuff.pdef).ToString("D4"),
                                                                                (friendlyFireStatsbuff.matk + friendlyWaterStatsbuff.matk + friendlyWindStatsbuff.matk).ToString("D4"), (friendlyFireStatsbuff.mdef + friendlyWaterStatsbuff.mdef + friendlyWindStatsbuff.mdef).ToString("D4"),
                                                                                (friendlyFireStatsbuff.GetTotalStats() + friendlyWaterStatsbuff.GetTotalStats() + friendlyWindStatsbuff.GetTotalStats()).ToString("D4")));

            weaponHashLog.WriteLine(string.Format("Debuff\t{0}\t{1}\t{2}\t{3}\t=> {4}", (friendlyFireStatsDebuff.patk + friendlyWaterStatsDebuff.patk + friendlyWindStatsDebuff.patk).ToString("D4"), (friendlyFireStatsDebuff.pdef + friendlyWaterStatsDebuff.pdef + friendlyWindStatsDebuff.pdef).ToString("D4"),
                                                                                (friendlyFireStatsDebuff.matk + friendlyWaterStatsDebuff.matk + friendlyWindStatsDebuff.matk).ToString("D4"), (friendlyFireStatsDebuff.mdef + friendlyWaterStatsDebuff.mdef + friendlyWindStatsDebuff.mdef).ToString("D4"),
                                                                                (friendlyFireStatsDebuff.GetTotalStats() + friendlyWaterStatsDebuff.GetTotalStats() + friendlyWindStatsDebuff.GetTotalStats()).ToString("D4")));
            weaponHashLog.WriteLine("-------------------------------------------------------------------");
            weaponHashLog.WriteLine();

            weaponHashLog.WriteLine("****************************************" + "OPPONENT BUFF/DEBUFF SUMMARY " + "****************************************");
            weaponHashLog.WriteLine("-------------------------------------------------------------------");
            weaponHashLog.WriteLine(string.Format("Fire Weapons Summary({0})", enemyFireWeapons));
            weaponHashLog.WriteLine("\t\tPATK\tPDEF\tMATK\tMDEF");
            weaponHashLog.WriteLine(string.Format("Buff\t{0}\t{1}\t{2}\t{3}\t=> {4}", enemyFireStatsbuff.patk.ToString("D4"), enemyFireStatsbuff.pdef.ToString("D4"), enemyFireStatsbuff.matk.ToString("D4"), enemyFireStatsbuff.mdef.ToString("D4"), enemyFireStatsbuff.GetTotalStats().ToString("D4")));
            weaponHashLog.WriteLine(string.Format("Debuff\t{0}\t{1}\t{2}\t{3}\t=> {4}", enemyFireStatsDebuff.patk.ToString("D4"), enemyFireStatsDebuff.pdef.ToString("D4"), enemyFireStatsDebuff.matk.ToString("D4"), enemyFireStatsDebuff.mdef.ToString("D4"), enemyFireStatsDebuff.GetTotalStats().ToString("D4")));
            weaponHashLog.WriteLine("-------------------------------------------------------------------");

            weaponHashLog.WriteLine(string.Format("Water Weapons Summary({0})", enemyWaterWeapons));
            weaponHashLog.WriteLine("\t\tPATK\tPDEF\tMATK\tMDEF");
            weaponHashLog.WriteLine(string.Format("Buff\t{0}\t{1}\t{2}\t{3}\t=> {4}", enemyWaterStatsbuff.patk.ToString("D4"), enemyWaterStatsbuff.pdef.ToString("D4"), enemyWaterStatsbuff.matk.ToString("D4"), enemyWaterStatsbuff.mdef.ToString("D4"), enemyWaterStatsbuff.GetTotalStats().ToString("D4")));
            weaponHashLog.WriteLine(string.Format("Debuff\t{0}\t{1}\t{2}\t{3}\t=> {4}", enemyWaterStatsDebuff.patk.ToString("D4"), enemyWaterStatsDebuff.pdef.ToString("D4"), enemyWaterStatsDebuff.matk.ToString("D4"), enemyWaterStatsDebuff.mdef.ToString("D4"), enemyWaterStatsDebuff.GetTotalStats().ToString("D4")));
            weaponHashLog.WriteLine("-------------------------------------------------------------------");

            weaponHashLog.WriteLine(string.Format("Wind Weapons Summary({0})", enemyWindWeapons));
            weaponHashLog.WriteLine("\t\tPATK\tPDEF\tMATK\tMDEF");
            weaponHashLog.WriteLine(string.Format("Buff\t{0}\t{1}\t{2}\t{3}\t=> {4}", enemyWindStatsbuff.patk.ToString("D4"), enemyWindStatsbuff.pdef.ToString("D4"), enemyWindStatsbuff.matk.ToString("D4"), enemyWindStatsbuff.mdef.ToString("D4"), enemyWindStatsbuff.GetTotalStats().ToString("D4")));
            weaponHashLog.WriteLine(string.Format("Debuff\t{0}\t{1}\t{2}\t{3}\t=> {4}", enemyWindStatsDebuff.patk.ToString("D4"), enemyWindStatsDebuff.pdef.ToString("D4"), enemyWindStatsDebuff.matk.ToString("D4"), enemyWindStatsDebuff.mdef.ToString("D4"), enemyWindStatsDebuff.GetTotalStats().ToString("D4")));
            weaponHashLog.WriteLine("-------------------------------------------------------------------");

            weaponHashLog.WriteLine(string.Format("All Weapons({0})", enemyFireWeapons + enemyWaterWeapons + enemyWindWeapons));
            weaponHashLog.WriteLine("\t\tPATK\tPDEF\tMATK\tMDEF");
            weaponHashLog.WriteLine(string.Format("Buff\t{0}\t{1}\t{2}\t{3}\t=> {4}", (enemyFireStatsbuff.patk + enemyWaterStatsbuff.patk + enemyWindStatsbuff.patk).ToString("D4"), (enemyFireStatsbuff.pdef + enemyWaterStatsbuff.pdef + enemyWindStatsbuff.pdef).ToString("D4"),
                                                                                (enemyFireStatsbuff.matk + enemyWaterStatsbuff.matk + enemyWindStatsbuff.matk).ToString("D4"), (enemyFireStatsbuff.mdef + enemyWaterStatsbuff.mdef + enemyWindStatsbuff.mdef).ToString("D4"),
                                                                                (enemyFireStatsbuff.GetTotalStats() + enemyWaterStatsbuff.GetTotalStats() + enemyWindStatsbuff.GetTotalStats()).ToString("D4")));

            weaponHashLog.WriteLine(string.Format("Debuff\t{0}\t{1}\t{2}\t{3}\t=> {4}", (enemyFireStatsDebuff.patk + enemyWaterStatsDebuff.patk + enemyWindStatsDebuff.patk).ToString("D4"), (enemyFireStatsDebuff.pdef + enemyWaterStatsDebuff.pdef + enemyWindStatsDebuff.pdef).ToString("D4"),
                                                                                (enemyFireStatsDebuff.matk + enemyWaterStatsDebuff.matk + enemyWindStatsDebuff.matk).ToString("D4"), (enemyFireStatsDebuff.mdef + enemyWaterStatsDebuff.mdef + enemyWindStatsDebuff.mdef).ToString("D4"),
                                                                                (enemyFireStatsDebuff.GetTotalStats() + enemyWaterStatsDebuff.GetTotalStats() + enemyWindStatsDebuff.GetTotalStats()).ToString("D4")));
            weaponHashLog.WriteLine("-------------------------------------------------------------------");

            weaponHashLog.WriteLine();
            weaponHashLog.WriteLine();
            weaponHashLog.WriteLine("****************************************" + "FRIENDLY HEAL SUMMARY " + "****************************************");
            weaponHashLog.WriteLine("-------------------------------------------------------------------");
            weaponHashLog.WriteLine(string.Format("Fire Weapons Summary({0})", friendlyFireStaves));
            weaponHashLog.WriteLine("Healing => " + friendlyFireHealing);
            weaponHashLog.WriteLine("-------------------------------------------------------------------");

            weaponHashLog.WriteLine(string.Format("Water Weapons Summary({0})", friendlyWaterStaves));
            weaponHashLog.WriteLine("Healing => " + friendlyWaterHealing);
            weaponHashLog.WriteLine("-------------------------------------------------------------------");

            weaponHashLog.WriteLine(string.Format("Wind Weapons Summary({0})", friendlyWindStaves));
            weaponHashLog.WriteLine("Healing => " + friendlyWindHealing);
            weaponHashLog.WriteLine("-------------------------------------------------------------------");

            weaponHashLog.WriteLine(string.Format("All Weapons({0})", friendlyFireStaves + friendlyWaterStaves + friendlyWindStaves));
            weaponHashLog.WriteLine();
            weaponHashLog.WriteLine("Healing => " + (friendlyFireHealing + friendlyWaterHealing + friendlyWindHealing));
            weaponHashLog.WriteLine("-------------------------------------------------------------------");
            weaponHashLog.WriteLine();

            weaponHashLog.WriteLine("****************************************" + "OPPONENT HEAL SUMMARY " + "****************************************");
            weaponHashLog.WriteLine("-------------------------------------------------------------------");
            weaponHashLog.WriteLine(string.Format("Fire Weapons Summary({0})", enemyFireStaves));
            weaponHashLog.WriteLine("Healing => " + enemyFireHealing);
            weaponHashLog.WriteLine("-------------------------------------------------------------------");

            weaponHashLog.WriteLine(string.Format("Water Weapons Summary({0})", enemyWaterStaves));
            weaponHashLog.WriteLine("Healing => " + enemyWaterHealing);
            weaponHashLog.WriteLine("-------------------------------------------------------------------");

            weaponHashLog.WriteLine(string.Format("Wind Weapons Summary({0})", enemyWindStaves));
            weaponHashLog.WriteLine("Healing => " + enemyWindHealing);
            weaponHashLog.WriteLine("-------------------------------------------------------------------");

            weaponHashLog.WriteLine(string.Format("All Weapons({0})", enemyFireStaves + enemyWaterStaves + enemyWindStaves));
            weaponHashLog.WriteLine("Healing => " + (enemyFireHealing + enemyWaterHealing + enemyWindHealing));
            weaponHashLog.WriteLine("-------------------------------------------------------------------");
            weaponHashLog.WriteLine();
            weaponHashLog.WriteLine();

            weaponHashLog.WriteLine("****************************************" + "FRIENDLY VG WEAPONS SUMMARY " + "****************************************");
            weaponHashLog.WriteLine("-------------------------------------------------------------------");
            weaponHashLog.WriteLine(string.Format("Fire Weapons Summary({0})", friendlyFireVGWeapons));
            weaponHashLog.WriteLine("-------------------------------------------------------------------");

            weaponHashLog.WriteLine(string.Format("Water Weapons Summary({0})", friendlyWaterVGWeapons));
            weaponHashLog.WriteLine("-------------------------------------------------------------------");

            weaponHashLog.WriteLine(string.Format("Wind Weapons Summary({0})", friendlyWindVGWeapons));
            weaponHashLog.WriteLine("-------------------------------------------------------------------");
            weaponHashLog.WriteLine();

            weaponHashLog.WriteLine("****************************************" + "OPPONENT VG WEAPONS SUMMARY " + "****************************************");
            weaponHashLog.WriteLine("-------------------------------------------------------------------");
            weaponHashLog.WriteLine(string.Format("Fire Weapons Summary({0})", enemyFireVGWeapons));
            weaponHashLog.WriteLine("-------------------------------------------------------------------");

            weaponHashLog.WriteLine(string.Format("Water Weapons Summary({0})", enemyWaterVGWeapons));
            weaponHashLog.WriteLine("-------------------------------------------------------------------");

            weaponHashLog.WriteLine(string.Format("Wind Weapons Summary({0})", enemyWindVGWeapons));
            weaponHashLog.WriteLine("-------------------------------------------------------------------");
            weaponHashLog.WriteLine();
            weaponHashLog.WriteLine();

            weaponHashLog.WriteLine("****************************************" + "FRIENDLY COMBO WEAPONS SUMMARY " + "****************************************");
            weaponHashLog.WriteLine("-------------------------------------------------------------------");
            weaponHashLog.WriteLine(string.Format("Combo Weapons Summary({0})", friendlyBonusComboWeapons));
            weaponHashLog.WriteLine("-------------------------------------------------------------------");
            weaponHashLog.WriteLine();
            weaponHashLog.WriteLine("****************************************" + "ENEMY COMBO WEAPONS SUMMARY " + "****************************************");
            weaponHashLog.WriteLine("-------------------------------------------------------------------");
            weaponHashLog.WriteLine(string.Format("Combo Weapons Summary({0})", enemyBonusComboWeapons));
            weaponHashLog.WriteLine("-------------------------------------------------------------------");
            weaponHashLog.WriteLine();

            friendlyWeaponHashCSV.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}",
                friendlyFireWeapons, friendlyFireStatsbuff.patk, friendlyFireStatsbuff.pdef, friendlyFireStatsbuff.matk, friendlyFireStatsbuff.mdef, friendlyFireStatsbuff.GetTotalStats(),
                friendlyFireStatsDebuff.patk, friendlyFireStatsDebuff.pdef, friendlyFireStatsDebuff.matk, friendlyFireStatsDebuff.mdef, friendlyFireStatsDebuff.GetTotalStats(),
                friendlyFireStaves, friendlyFireHealing, friendlyFireVGWeapons));

            friendlyWeaponHashCSV.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}",
                friendlyWaterWeapons, friendlyWaterStatsbuff.patk, friendlyWaterStatsbuff.pdef, friendlyWaterStatsbuff.matk, friendlyWaterStatsbuff.mdef, friendlyWaterStatsbuff.GetTotalStats(),
                friendlyWaterStatsDebuff.patk, friendlyWaterStatsDebuff.pdef, friendlyWaterStatsDebuff.matk, friendlyWaterStatsDebuff.mdef, friendlyWaterStatsDebuff.GetTotalStats(),
                friendlyWaterStaves, friendlyWaterHealing, friendlyWaterVGWeapons));

            friendlyWeaponHashCSV.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}",
                friendlyWindWeapons, friendlyWindStatsbuff.patk, friendlyWindStatsbuff.pdef, friendlyWindStatsbuff.matk, friendlyWindStatsbuff.mdef, friendlyWindStatsbuff.GetTotalStats(),
                friendlyWindStatsDebuff.patk, friendlyWindStatsDebuff.pdef, friendlyWindStatsDebuff.matk, friendlyWindStatsDebuff.mdef, friendlyWindStatsDebuff.GetTotalStats(),
                friendlyWindStaves, friendlyWindHealing, friendlyWindVGWeapons));

            friendlyWeaponHashCSV.WriteLine(string.Format("{0}", friendlyBonusComboWeapons));


            enemyWeaponHashCSV.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}",
                enemyFireWeapons, enemyFireStatsbuff.patk, enemyFireStatsbuff.pdef, enemyFireStatsbuff.matk, enemyFireStatsbuff.mdef, enemyFireStatsbuff.GetTotalStats(),
                enemyFireStatsDebuff.patk, enemyFireStatsDebuff.pdef, enemyFireStatsDebuff.matk, enemyFireStatsDebuff.mdef, enemyFireStatsDebuff.GetTotalStats(),
                enemyFireStaves, enemyFireHealing, enemyFireVGWeapons));

            enemyWeaponHashCSV.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}",
                enemyWaterWeapons, enemyWaterStatsbuff.patk, enemyWaterStatsbuff.pdef, enemyWaterStatsbuff.matk, enemyWaterStatsbuff.mdef, enemyWaterStatsbuff.GetTotalStats(),
                enemyWaterStatsDebuff.patk, enemyWaterStatsDebuff.pdef, enemyWaterStatsDebuff.matk, enemyWaterStatsDebuff.mdef, enemyWaterStatsDebuff.GetTotalStats(),
                enemyWaterStaves, enemyWaterHealing, enemyWaterVGWeapons));

            enemyWeaponHashCSV.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}",
                enemyWindWeapons, enemyWindStatsbuff.patk, enemyWindStatsbuff.pdef, enemyWindStatsbuff.matk, enemyWindStatsbuff.mdef, enemyWindStatsbuff.GetTotalStats(),
                enemyWindStatsDebuff.patk, enemyWindStatsDebuff.pdef, enemyWindStatsDebuff.matk, enemyWindStatsDebuff.mdef, enemyWindStatsDebuff.GetTotalStats(),
                enemyWindStaves, enemyWindHealing, enemyWindVGWeapons));

            enemyWeaponHashCSV.WriteLine(string.Format("{0}", enemyBonusComboWeapons));

            weaponHashLog.Close();
            friendlyWeaponHashCSV.Close();
            enemyWeaponHashCSV.Close();

        }

        public void ExportRelationshipTable(Dictionary<string, Dictionary<string, Relation>> relationDictionary)
        {
            var relationshipTableLog = new StreamWriter(Utilities.processDirectory + "/relationshipTable.csv");

            foreach (KeyValuePair<string, Dictionary<string, Relation>> kvp in relationDictionary)
            {
                relationshipTableLog.WriteLine("**" + kvp.Key + "**");

                foreach (KeyValuePair<string, Relation> kvpInner in kvp.Value)
                {
                    relationshipTableLog.WriteLine(kvpInner.Key.ToString() + "," + kvpInner.Value.ToString());
                }
            }

            relationshipTableLog.Close();
        }

        public void ExportRelationshipCorrectionTable(Dictionary<string, Dictionary<string, Relation>> relationDictionary)
        {
            var relationshipCorrectionTableLog = new StreamWriter(Utilities.processDirectory + "/relationshipCorrectionTable.txt");

            List<string> relationshipList = new List<string>();

            foreach (KeyValuePair<string, Dictionary<string, Relation>> kvp in relationDictionary)
            {
                relationshipList.Add(kvp.Key);
            }

            relationshipList = relationshipList.OrderBy(x => x).ToList();

            for (int i = 0; i < relationshipList.Count; ++i)
            {
                relationshipCorrectionTableLog.WriteLine(relationshipList[i]);
            }

            relationshipCorrectionTableLog.Close();
        }

        public void ExportColoCombatDetails(List<StoreColoCombatDetails> coloEntireCombatDetails)
        {
            var coloCombatLog = new StreamWriter(Utilities.processDirectory + "/coloCombatDetails.txt");
            HashSet<string> receivers = new HashSet<string>();
            for (int i = 0; i < coloEntireCombatDetails.Count; ++i)
            {
                receivers.Clear();
                var details = coloEntireCombatDetails[i];

                coloCombatLog.WriteLine("*****************************Combat block " + i + "*****************************");
                coloCombatLog.WriteLine("Time:" + details.blockTime);
                coloCombatLog.WriteLine("Initiator: " + details.GetAttackerAndStats());
                coloCombatLog.WriteLine();

                coloCombatLog.WriteLine("Weapons and procs:");

                if (details.weaponUsedToAttack != null)
                    coloCombatLog.WriteLine("Main: " + details.weaponUsedToAttack.GetWeaponNameWithLevel());
                else
                {
                    coloCombatLog.WriteLine("Unable to extract weapon used to attack");
                }

                for (int s = 0; s < details.supportWeaponProcs.Count; ++s)
                    coloCombatLog.WriteLine("Support: " + details.supportWeaponProcs[s].GetSupportWeaponWithLevel());

                coloCombatLog.WriteLine();
                coloCombatLog.WriteLine("Receivers:");
                foreach (KeyValuePair<string, List<CombatRelation>> kvp in details.receiverStatsModification)
                {
                    if (kvp.Value == null || kvp.Value.Count == 0)
                        continue;

                    if (kvp.Value[0].statsDetails != null)
                        coloCombatLog.WriteLine(kvp.Key + " - " + kvp.Value[0].statsDetails.GetCurrentStatsAndBuffDebuff());
                }

                coloCombatLog.WriteLine();
                coloCombatLog.WriteLine("Results:");
                foreach (KeyValuePair<string, List<CombatRelation>> kvp in details.receiverStatsModification)
                {
                    for (int j = 0; j < kvp.Value.Count; ++j)
                    {
                        if (!kvp.Value[j].guildShipReset)
                            coloCombatLog.WriteLine(kvp.Key + " " + kvp.Value[j].ToString());
                    }
                }
                coloCombatLog.WriteLine();

            }

            coloCombatLog.Close();
        }

        public void ExportIndividualCombatDetails(Dictionary<string, List<StoreColoCombatDetails>> individualColoCombatDetails)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(Utilities.processDirectory + "/Individual/");
            foreach (FileInfo file in dirInfo.GetFiles())
            {
                file.Delete();
            }

            foreach (KeyValuePair<string, List<StoreColoCombatDetails>> kvp in individualColoCombatDetails)
            {
                if (!Utilities.IsEnemy(kvp.Key) && !Utilities.IsFriendly(kvp.Key))
                    continue;

                var coloCombatLog = new StreamWriter(Utilities.processDirectory + "/Individual/" + kvp.Key + ".txt");

                uint patkBuff = 0, patkActualBuff = 0, patkBuffCount = 0;
                uint pdefBuff = 0, pdefActualBuff = 0, pdefBuffCount = 0;
                uint matkBuff = 0, matkActualBuff = 0, matkBuffCount = 0;
                uint mdefBuff = 0, mdefActualBuff = 0, mdefBuffCount = 0;
                uint recover = 0, recoverCount = 0;

                uint patkDebuff = 0, patkActualDebuff = 0, patkDebuffCount = 0;
                uint pdefDebuff = 0, pdefActualDebuff = 0, pdefDebuffCount = 0;
                uint matkDebuff = 0, matkActualDebuff = 0, matkDebuffCount = 0;
                uint mdefDebuff = 0, mdefActualDebuff = 0, mdefDebuffCount = 0;

                float patkEffectiveness = 0;
                float pdefEffectiveness = 0;
                float matkEffectiveness = 0;
                float mdefEffectiveness = 0;

                float patkDebuffEffectiveness = 0;
                float pdefDebuffEffectiveness = 0;
                float matkDebuffEffectiveness = 0;
                float mdefDebuffEffectiveness = 0;

                for (int i = 0; i < kvp.Value.Count; ++i)
                {
                    var details = kvp.Value[i];

                    coloCombatLog.WriteLine("*****************************Combat block " + i + "*****************************");
                    coloCombatLog.WriteLine("Time:" + details.blockTime);
                    coloCombatLog.WriteLine("Initiator: " + details.GetAttackerAndStats());
                    coloCombatLog.WriteLine();

                    coloCombatLog.WriteLine("Weapons and procs:");

                    if (details.weaponUsedToAttack != null)
                        coloCombatLog.WriteLine("Main: " + details.weaponUsedToAttack.GetWeaponNameWithLevel());
                    else
                    {
                        coloCombatLog.WriteLine("Unable to extract weapon used to attack");
                    }

                    for (int s = 0; s < details.supportWeaponProcs.Count; ++s)
                        coloCombatLog.WriteLine("Support: " + details.supportWeaponProcs[s].GetSupportWeaponWithLevel());

                    coloCombatLog.WriteLine();
                    coloCombatLog.WriteLine("Receivers:");
                    foreach (KeyValuePair<string, List<CombatRelation>> kvpInner in details.receiverStatsModification)
                    {
                        if (kvpInner.Value == null || kvpInner.Value.Count == 0)
                            continue;

                        if (kvpInner.Value[0].statsDetails != null)
                            coloCombatLog.WriteLine(kvpInner.Key + " - " + kvpInner.Value[0].statsDetails.GetCurrentStatsAndBuffDebuff());
                    }

                    coloCombatLog.WriteLine();
                    coloCombatLog.WriteLine("Results:");
                    foreach (KeyValuePair<string, List<CombatRelation>> kvpInner in details.receiverStatsModification)
                    {
                        for (int j = 0; j < kvpInner.Value.Count; ++j)
                        {
                            if (!kvpInner.Value[j].guildShipReset)
                            {
                                coloCombatLog.WriteLine(kvpInner.Key + " " + kvpInner.Value[j].ToString());

                                var relationDetails = kvpInner.Value[j].GetRelationAmount();

                                switch (relationDetails.Item1)
                                {
                                    case StatsRelation.PATK_UP:
                                        patkBuff += relationDetails.Item2;
                                        patkActualBuff += relationDetails.Item3;
                                        patkEffectiveness += relationDetails.Item4;
                                        ++patkBuffCount;
                                        break;

                                    case StatsRelation.PATK_DOWN:
                                        patkDebuff += relationDetails.Item2;
                                        patkActualDebuff += relationDetails.Item3;
                                        patkDebuffEffectiveness += relationDetails.Item4;
                                        ++patkDebuffCount;
                                        break;

                                    case StatsRelation.PDEF_UP:
                                        pdefBuff += relationDetails.Item2;
                                        pdefActualBuff += relationDetails.Item3;
                                        pdefEffectiveness += relationDetails.Item4;
                                        ++pdefBuffCount;
                                        break;

                                    case StatsRelation.PDEF_DOWN:
                                        pdefDebuff += relationDetails.Item2;
                                        pdefActualDebuff += relationDetails.Item3;
                                        pdefDebuffEffectiveness += relationDetails.Item4;
                                        ++pdefDebuffCount;
                                        break;

                                    case StatsRelation.MATK_UP:
                                        matkBuff += relationDetails.Item2;
                                        matkActualBuff += relationDetails.Item3;
                                        matkEffectiveness += relationDetails.Item4;
                                        ++matkBuffCount;
                                        break;

                                    case StatsRelation.MATK_DOWN:
                                        matkDebuff += relationDetails.Item2;
                                        matkActualDebuff += relationDetails.Item3;
                                        matkDebuffEffectiveness += relationDetails.Item4;
                                        ++matkDebuffCount;
                                        break;

                                    case StatsRelation.MDEF_UP:
                                        mdefBuff += relationDetails.Item2;
                                        mdefActualBuff += relationDetails.Item3;
                                        mdefEffectiveness += relationDetails.Item4;
                                        ++mdefBuffCount;
                                        break;

                                    case StatsRelation.MDEF_DOWN:
                                        mdefDebuff += relationDetails.Item2;
                                        mdefActualDebuff += relationDetails.Item3;
                                        mdefDebuffEffectiveness += relationDetails.Item4;
                                        ++mdefDebuffCount;
                                        break;

                                    case StatsRelation.RECOVER:
                                        recover += relationDetails.Item2;
                                        //mdefActualDebuff += relationDetails.Item3;
                                        // mdefDebuffEffectiveness += relationDetails.Item4;
                                        ++recoverCount;
                                        break;

                                    default:
                                        break;
                                }
                            }
                        }
                    }

                    coloCombatLog.WriteLine();
                    coloCombatLog.WriteLine("Average Buff/Debuff Stats:");
                    coloCombatLog.WriteLine("PATK buff: " + patkBuff / (patkBuffCount == 0 ? 1 : patkBuffCount) + ". Actual Value " + patkActualBuff / (patkBuffCount == 0 ? 1 : patkBuffCount) + ". Effectiveness " + (patkEffectiveness / (patkBuffCount == 0 ? 1 : patkBuffCount)) * 100 + "%");
                    coloCombatLog.WriteLine("PDEF buff: " + pdefBuff / (pdefBuffCount == 0 ? 1 : pdefBuffCount) + ". Actual Value " + pdefActualBuff / (pdefBuffCount == 0 ? 1 : pdefBuffCount) + ". Effectiveness " + (pdefEffectiveness / (pdefBuffCount == 0 ? 1 : pdefBuffCount)) * 100 + "%");
                    coloCombatLog.WriteLine("MATK buff: " + matkBuff / (matkBuffCount == 0 ? 1 : matkBuffCount) + ". Actual Value " + matkActualBuff / (matkBuffCount == 0 ? 1 : matkBuffCount) + ". Effectiveness " + (matkEffectiveness / (matkBuffCount == 0 ? 1 : matkBuffCount)) * 100 + "%");
                    coloCombatLog.WriteLine("MDEF buff: " + mdefBuff / (mdefBuffCount == 0 ? 1 : mdefBuffCount) + ". Actual Value " + mdefActualBuff / (mdefBuffCount == 0 ? 1 : mdefBuffCount) + ". Effectiveness " + (mdefEffectiveness / (mdefBuffCount == 0 ? 1 : mdefBuffCount)) * 100 + "%");

                    coloCombatLog.WriteLine("PATK debuff: " + patkDebuff / (patkDebuffCount == 0 ? 1 : patkDebuffCount) + ". Actual Value " + patkActualDebuff / (patkDebuffCount == 0 ? 1 : patkDebuffCount) + ". Effectiveness " + (patkDebuffEffectiveness / (patkDebuffCount == 0 ? 1 : patkDebuffCount)) * 100 + "%");
                    coloCombatLog.WriteLine("PDEF debuff: " + pdefDebuff / (pdefDebuffCount == 0 ? 1 : pdefDebuffCount) + ". Actual Value " + pdefActualDebuff / (pdefDebuffCount == 0 ? 1 : pdefDebuffCount) + ". Effectiveness " + (pdefDebuffEffectiveness / (pdefDebuffCount == 0 ? 1 : pdefDebuffCount)) * 100 + "%");
                    coloCombatLog.WriteLine("MATK debuff: " + matkDebuff / (matkDebuffCount == 0 ? 1 : matkDebuffCount) + ". Actual Value " + matkActualDebuff / (matkDebuffCount == 0 ? 1 : matkDebuffCount) + ". Effectiveness " + (matkDebuffEffectiveness / (matkDebuffCount == 0 ? 1 : matkDebuffCount)) * 100 + "%");
                    coloCombatLog.WriteLine("MDEF debuff: " + mdefDebuff / (mdefDebuffCount == 0 ? 1 : mdefDebuffCount) + ". Actual Value " + mdefActualDebuff / (mdefDebuffCount == 0 ? 1 : mdefDebuffCount) + ". Effectiveness " + (mdefDebuffEffectiveness / (mdefDebuffCount == 0 ? 1 : mdefDebuffCount)) * 100 + "%");
                    coloCombatLog.WriteLine("Recover: " + recover / (recoverCount == 0 ? 1 : recoverCount));
                    coloCombatLog.WriteLine();
                    coloCombatLog.WriteLine();

                } //End of combat block

                patkBuffCount = Math.Max(patkBuffCount,1);
                pdefBuffCount = Math.Max(pdefBuffCount, 1);
                matkBuffCount = Math.Max(matkBuffCount, 1);
                mdefBuffCount = Math.Max(mdefBuffCount, 1);

                patkDebuffCount = Math.Max(patkDebuffCount, 1);
                pdefDebuffCount = Math.Max(pdefDebuffCount, 1);
                matkDebuffCount = Math.Max(matkDebuffCount, 1);
                mdefDebuffCount = Math.Max(mdefDebuffCount, 1);


                coloCombatLog.WriteLine("*****************************Total Buff/Debuff Stats*****************************");
                coloCombatLog.WriteLine("PATK buff: " + patkBuff + ". Actual Value " + patkActualBuff + ". Effectiveness " + (patkEffectiveness / patkBuffCount) * 100 + "%");
                coloCombatLog.WriteLine("PDEF buff: " + pdefBuff + ". Actual Value " + pdefActualBuff + ". Effectiveness " + (pdefEffectiveness / pdefBuffCount) * 100 + "%");
                coloCombatLog.WriteLine("MATK buff: " + matkBuff + ". Actual Value " + matkActualBuff + ". Effectiveness " + (matkEffectiveness / matkBuffCount) * 100 + "%");
                coloCombatLog.WriteLine("MDEF buff: " + mdefBuff + ". Actual Value " + mdefActualBuff + ". Effectiveness " + (mdefEffectiveness / mdefBuffCount) * 100 + "%");

                coloCombatLog.WriteLine("PATK debuff: " + patkDebuff + ". Actual Value " + patkActualDebuff + ". Effectiveness " + (patkDebuffEffectiveness / patkDebuffCount) * 100 + "%");
                coloCombatLog.WriteLine("PDEF debuff: " + pdefDebuff + ". Actual Value " + pdefActualDebuff + ". Effectiveness " + (pdefDebuffEffectiveness / pdefDebuffCount) * 100 + "%");
                coloCombatLog.WriteLine("MATK debuff: " + matkDebuff + ". Actual Value " + matkActualDebuff + ". Effectiveness " + (matkDebuffEffectiveness / matkDebuffCount) * 100 + "%");
                coloCombatLog.WriteLine("MDEF debuff: " + mdefDebuff + ". Actual Value " + mdefActualDebuff + ". Effectiveness " + (mdefDebuffEffectiveness / mdefDebuffCount) * 100 + "%");
                coloCombatLog.WriteLine("Recover: " + recover);

                coloCombatLog.Close();
            }
        }

        //public void ExportWeaponsDictionary()
        //{
        //    var weaponList = weaponDictionary.OrderBy(x => x.Key).ToList();

        //    var weaponhashlog = new StreamWriter("c:/users/onsla/onedrive/desktop/sinoalice sql/images/output/weaponexport.txt");

        //    foreach (KeyValuePair<string,Weapon> kvp in weaponList)
        //    {
        //        weaponhashlog.WriteLine("*"+kvp.Key+","+kvp.Value.ToString());
        //    }

        //    weaponhashlog.Close();
        //}
    }
}
