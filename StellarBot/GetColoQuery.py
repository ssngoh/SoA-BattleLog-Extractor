import os
import random
import datetime

def GetColoDetailsShort(dbinsertImporterObj,limit):

	if limit > 30:
		limit = 30
	winlose=dbinsertImporterObj.GetColo(limit)

	strList = []

	for row in reversed(winlose):
		textToPrint=""
		victoryText =""
		if int(row['is_victory']) == 1:
			victoryText="Yes"
		else:
			victoryText="No"

		textToPrint += "**{}**  VS  **{}**\n".format(str(row['colo_date']),row['guild_name'])
		textToPrint += """```\nguild id: {}\nvictory: {}\n{:<22}enemy score: {}\n{:<22}enemy combo: {}```\n``````""".format(
			row['guild_id'], victoryText, "our score: " +str(row['our_score']), row['enemy_score'], "our combo: "+str(row['our_combo']), row['enemy_combo'])

		strList.append(textToPrint)

	return strList

def GetColoDetailsShortByDate(dbinsertImporterObj,startDate,endDate):

	winlose=dbinsertImporterObj.GetColoByDate(startDate,endDate)

	strList = []

	for row in reversed(winlose):
		textToPrint=""
		victoryText =""
		if int(row['is_victory']) == 1:
			victoryText="Yes"
		else:
			victoryText="No"

		textToPrint += "**{}**  VS  **{}**\n".format(str(row['colo_date']),row['guild_name'])
		textToPrint += """```\nguild id: {}\nvictory: {}\n{:<22}enemy score: {}\n{:<22}enemy combo: {}```\n``````""".format(
			row['guild_id'], victoryText, "our score: " +str(row['our_score']), row['enemy_score'], "our combo: "+str(row['our_combo']), row['enemy_combo'])

		strList.append(textToPrint)

	return strList

def GetColoDetails(dbinsertImporterObj,limit):

	if limit > 5:
		limit = 5
	winlose=dbinsertImporterObj.GetColo(limit)
	return formatColoDetails(winlose)
	# return """```{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}```""".format(
	# 	time,guild_id,guild_name,victory,our_score,enemy_score,our_combo,enemy_combo,our_guildship_down,enemy_guildship_down,
	# 	our_vg,enemy_vg,our_rg,enemy_rg,our_cp,enemy_cp,size_diff,
	# 	our_total_recover,enemy_total_recover,our_total_atk_support,enemy_total_atk_support,our_total_def_support,enemy_total_def_support,
	# 	our_total_atk_debuff,enemy_total_atk_debuff,our_total_def_debuff,enemy_total_def_debuff,
	# 	first_demon,first_demon_our_score,first_demon_enemy_score,second_demon,second_demon_our_score,second_demon_enemy_score)

def GetColoDetailsByDate(dbinsertImporterObj,startDate,endDate):

	winlose=dbinsertImporterObj.GetColoByDate(startDate,endDate)
	return formatColoDetails(winlose)

def GetColoDetailsById(dbinsertImporterObj,id):

	winlose=dbinsertImporterObj.GetColoById(id)
	return formatColoDetails(winlose)

def GetColoDetailsByName(dbinsertImporterObj,guildName):

	winlose=dbinsertImporterObj.GetColoByName(guildName)
	return formatColoDetails(winlose)

def CompareColoDetailsById(dbinsertImporterObj,id,id2):

	winlose=dbinsertImporterObj.CompareColoById(id,id2)
	return formatColoDetails(winlose)

def GetBuffDebuffSummary(dbinsertImporterObj,id):
	bd=dbinsertImporterObj.GetBuffDebuffSummary(id)
	return formatColoBuffDebuff(bd)


def formatColoBuffDebuff(buffdebuff):
	strList = []

	filler ="-------------------------------------------------------------------"
	statsDisplayRow="\t\t{:^8}{:^8}{:^8}{:^8}".format("PATK:","PDEF:","MATK:","MDEF:")
	guild_name=""

	#there is only 1 row
	for row in buffdebuff:
		guild_name = row['guild_name']

		fireWeaponSummary ="Fire Weapons Summary: ({})".format(row['total_fire_harptome'])
		fireBuffDisplayRow="Buff:\t{:<8}{:<8}{:<8}{:<8}=> {}".format(
			row['total_fire_weapons_patk_buff'],row['total_fire_weapons_pdef_buff'],row['total_fire_weapons_matk_buff'],
			row['total_fire_weapons_mdef_buff'],row['total_fire_weapons_buff'])
		fireDebuffDisplayRow="Debuff:  {:<8}{:<8}{:<8}{:<8}=> {}".format(
			row['total_fire_weapons_patk_debuff'],row['total_fire_weapons_pdef_debuff'],row['total_fire_weapons_matk_debuff'],
			row['total_fire_weapons_mdef_debuff'],row['total_fire_weapons_debuff'])

		waterWeaponSummary ="Water Weapons Summary: ({})".format(row['total_water_harptome'])
		waterBuffDisplayRow="Buff:\t{:<8}{:<8}{:<8}{:<8}=> {}".format(
			row['total_water_weapons_patk_buff'],row['total_water_weapons_pdef_buff'],row['total_water_weapons_matk_buff'],
			row['total_water_weapons_mdef_buff'],row['total_water_weapons_buff'])
		waterDebuffDisplayRow="Debuff:  {:<8}{:<8}{:<8}{:<8}=> {}".format(
			row['total_water_weapons_patk_debuff'],row['total_water_weapons_pdef_debuff'],row['total_water_weapons_matk_debuff'],
			row['total_water_weapons_mdef_debuff'],row['total_water_weapons_debuff'])

		windWeaponSummary ="Wind Weapons Summary: ({})".format(row['total_wind_harptome'])
		windBuffDisplayRow="Buff:\t{:<8}{:<8}{:<8}{:<8}=> {}".format(
			row['total_wind_weapons_patk_buff'],row['total_wind_weapons_pdef_buff'],row['total_wind_weapons_matk_buff'],
			row['total_wind_weapons_mdef_buff'],row['total_wind_weapons_buff'])
		windDebuffDisplayRow="Debuff:  {:<8}{:<8}{:<8}{:<8}=> {}".format(
			row['total_wind_weapons_patk_debuff'],row['total_wind_weapons_pdef_debuff'],row['total_wind_weapons_matk_debuff'],
			row['total_wind_weapons_mdef_debuff'],row['total_wind_weapons_debuff'])

		allWeaponSummary ="All Weapons: ({})".format(row['total_fire_harptome'] + row['total_water_harptome'] + row['total_wind_harptome'])
		allBuffDisplayRow="Buff:\t{:<8}{:<8}{:<8}{:<8}=> {}".format(
			row['total_fire_weapons_patk_buff'] + row['total_water_weapons_patk_buff'] + row['total_wind_weapons_patk_buff'],
			row['total_fire_weapons_pdef_buff'] + row['total_water_weapons_pdef_buff'] + row['total_wind_weapons_pdef_buff'],
			row['total_fire_weapons_matk_buff'] + row['total_water_weapons_matk_buff'] + row['total_wind_weapons_matk_buff'],
			row['total_fire_weapons_mdef_buff'] + row['total_water_weapons_mdef_buff'] + row['total_wind_weapons_mdef_buff'],
			row['total_fire_weapons_buff'] + row['total_water_weapons_buff'] + row['total_wind_weapons_buff'])
		allDebuffDisplayRow="Debuff:  {:<8}{:<8}{:<8}{:<8}=> {}".format(
			row['total_fire_weapons_patk_debuff'] + row['total_water_weapons_patk_debuff'] + row['total_wind_weapons_patk_debuff'],
			row['total_fire_weapons_pdef_debuff'] + row['total_water_weapons_pdef_debuff'] + row['total_wind_weapons_pdef_debuff'],
			row['total_fire_weapons_matk_debuff'] + row['total_water_weapons_matk_debuff'] + row['total_wind_weapons_matk_debuff'],
			row['total_fire_weapons_mdef_debuff'] + row['total_water_weapons_mdef_debuff'] + row['total_wind_weapons_mdef_debuff'],
			row['total_fire_weapons_debuff'] + row['total_water_weapons_debuff'] + row['total_wind_weapons_debuff'])


		fireStavesSummary = "Fire Weapons Summary: ({})".format(row['total_fire_staves'])
		fireHealingDisplay = "Healing => {}".format(row['total_fire_heal'])
		waterStavesSummary = "Water Weapons Summary: ({})".format(row['total_water_staves'])
		waterHealingDisplay = "Healing => {}".format(row['total_water_heal'])
		windStavesSummary = "Wind Weapons Summary: ({})".format(row['total_wind_staves'])
		windHealingDisplay = "Healing => {}".format(row['total_wind_heal'])
		allStavesSummary = "All Weapons: ({})".format(row['total_fire_staves'] + row['total_water_staves'] + row['total_wind_staves'])
		allHealingDisplay = "Healing => {}".format(row['total_fire_heal'] + row['total_water_heal'] +row['total_wind_heal'])

		comboWeaponsSummary = "Combo Weapons brought: ({})".format(row['total_bonus_combo_weapons'])



	header ="{}: BUFF/DEBUFF SUMMARY".format(guild_name)
	strList.append("""```yaml\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n```""".format(
	header,filler,fireWeaponSummary,statsDisplayRow,fireBuffDisplayRow,fireDebuffDisplayRow,filler,
	waterWeaponSummary,statsDisplayRow,waterBuffDisplayRow,waterDebuffDisplayRow,filler,
	windWeaponSummary,statsDisplayRow,windBuffDisplayRow,windDebuffDisplayRow,filler,
	allWeaponSummary,statsDisplayRow,allBuffDisplayRow,allDebuffDisplayRow,filler))


	header ="{}: HEAL SUMMARY".format(guild_name)

	strList.append("""```yaml\n\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}```""".format(
	header,filler,fireStavesSummary,fireHealingDisplay,
	filler,waterStavesSummary,waterHealingDisplay,
	filler,windStavesSummary,windHealingDisplay,
	filler,allStavesSummary,allHealingDisplay,filler))

	header ="{}: COMBO WEAPONS BROUGHT SUMMARY".format(guild_name)

	strList.append("""```yaml\n\n{}\n{}\n{}\n{}```""".format(
	header,filler,comboWeaponsSummary,filler))

	return strList

def formatColoDetails(winlose):

	strList = [] 	

	time ="{:<18}".format("date:")
	guild_id ="{:<18}".format("guild id:")
	guild_name ="{:<18}".format("guild name:")
	victory ="{:<18}".format("victory:")
	our_score="{:<18}".format("our score:")
	enemy_score="{:<18}".format("enemy score:")
	our_combo ="{:<18}".format("our combo:")
	enemy_combo ="{:<18}".format("enemy combo:")
	our_guildship_down ="{:<18}".format("our GS down:")
	enemy_guildship_down ="{:<18}".format("enemy GS down:")
	our_vg ="{:<18}".format("our vg:")
	enemy_vg ="{:<18}".format("enemy vg:")
	our_rg ="{:<18}".format("our rg:")
	enemy_rg ="{:<18}".format("enemy rg:")
	our_cp ="{:<18}".format("our cp:")
	enemy_cp ="{:<18}".format("enemy cp:")
	size_diff ="{:<18}".format("size diff:")
	our_total_recover ="{:<18}".format("our heal:")
	enemy_total_recover ="{:<18}".format("enemy heal:")
	our_total_atk_support ="{:<18}".format("our atk sup:")
	enemy_total_atk_support ="{:<18}".format("enemy atk sup:")
	our_total_def_support ="{:<18}".format("our def sup:")
	enemy_total_def_support ="{:<18}".format("enemy def sup:")
	our_total_atk_debuff ="{:<18}".format("our atk debuff:")
	enemy_total_atk_debuff ="{:<18}".format("enemy atk debuff:")
	our_total_def_debuff ="{:<18}".format("our def debuff:")
	enemy_total_def_debuff ="{:<18}".format("enemy def debuff:")
	first_demon ="{:<18}".format("first demon:")
	first_demon_our_score ="{:<18}".format("our score:")
	first_demon_enemy_score ="{:<18}".format("enemy score:")
	second_demon ="{:<18}".format("second demon:")
	second_demon_our_score ="{:<18}".format("our score:")
	second_demon_enemy_score ="{:<18}".format("enemy score:")

	our_nightmare_used ="{:<25}".format("Our nightmare used:")
	enemy_nightmare_used ="{:<25}".format("Enemy nightmare used:")

	nightmare_used_and_comment_list = []

	alignmentVar=20
	nightmareAlignmentVar=20

	for row in reversed(winlose):
		dateStr = str(row['colo_date'])
		time += "{:^{x}}".format(dateStr,x=alignmentVar)
		guild_id += "{:^{x}}".format(row['guild_id'],x=alignmentVar)
		guild_name += "{:^{x}}".format(row['guild_name'],x=alignmentVar)
		victory += "{:^{x}}".format(row['is_victory'],x=alignmentVar)
		our_score += "{:^{x}}".format(row['our_score'],x=alignmentVar)
		enemy_score += "{:^{x}}".format(row['enemy_score'],x=alignmentVar)
		our_combo += "{:^{x}}".format(row['our_combo'],x=alignmentVar)
		enemy_combo += "{:^{x}}".format(row['enemy_combo'],x=alignmentVar)
		our_guildship_down += "{:^{x}}".format(row['our_guildship_down'],x=alignmentVar)
		enemy_guildship_down += "{:^{x}}".format(row['enemy_guildship_down'],x=alignmentVar)
		our_vg += "{:^{x}}".format(row['our_vg'],x=alignmentVar)
		enemy_vg += "{:^{x}}".format(row['enemy_vg'],x=alignmentVar)
		our_rg += "{:^{x}}".format(row['our_rg'],x=alignmentVar)
		enemy_rg += "{:^{x}}".format(row['enemy_rg'],x=alignmentVar)
		our_cp += "{:^{x}}".format(row['our_cp'],x=alignmentVar)
		enemy_cp += "{:^{x}}".format(row['enemy_cp'],x=alignmentVar)
		size_diff += "{:^{x}}".format(row['size_diff'],x=alignmentVar)
		our_total_recover += "{:^{x}}".format(row['our_total_recover'],x=alignmentVar)
		enemy_total_recover += "{:^{x}}".format(row['enemy_total_recover'],x=alignmentVar)
		our_total_atk_support += "{:^{x}}".format(row['our_total_ally_atk_support'],x=alignmentVar)
		enemy_total_atk_support += "{:^{x}}".format(row['enemy_total_ally_atk_support'],x=alignmentVar)
		our_total_def_support += "{:^{x}}".format(row['our_total_ally_def_support'],x=alignmentVar)
		enemy_total_def_support += "{:^{x}}".format(row['enemy_total_ally_def_support'],x=alignmentVar)
		our_total_atk_debuff += "{:^{x}}".format(row['our_total_enemy_atk_debuff'],x=alignmentVar)
		enemy_total_atk_debuff += "{:^{x}}".format(row['enemy_total_enemy_atk_debuff'],x=alignmentVar)
		our_total_def_debuff += "{:^{x}}".format(row['our_total_enemy_def_debuff'],x=alignmentVar)
		enemy_total_def_debuff += "{:^{x}}".format(row['enemy_total_enemy_def_debuff'],x=alignmentVar)
		first_demon += "{:^{x}}".format(row['first_demon'],x=alignmentVar)
		first_demon_our_score += "{:^{x}}".format(row['first_demon_our_score'],x=alignmentVar)
		first_demon_enemy_score += "{:^{x}}".format(row['first_demon_enemy_score'],x=alignmentVar)
		second_demon += "{:^{x}}".format(row['second_demon'],x=alignmentVar)
		second_demon_our_score += "{:^{x}}".format(row['second_demon_our_score'],x=alignmentVar)
		second_demon_enemy_score += "{:^{x}}".format(row['second_demon_enemy_score'],x=alignmentVar)

		additionalText =""
		additionalText += "**{}**\n".format(str(row['colo_date']))

		splitStr1 = str(row['our_nightmare_used']).split(';')
		splitStr2 = str(row['enemy_nightmare_used']).split(';')
		our_nm =""
		enemy_nm = ""

		for s in splitStr1:
			our_nm += "{:<{x}}".format(s,x=nightmareAlignmentVar)

		for s in splitStr2:
			enemy_nm += "{:<{x}}".format(s,x=nightmareAlignmentVar)
		
		additionalText += """```yaml\n{:<18}{}\n\n{:<18}{}\n\n{:<18}{}```""".format(
			"Our NM used: ",our_nm,"Enemy NM used: ",enemy_nm, "Comments: ", str(row['comments']))

		nightmare_used_and_comment_list.append(additionalText)

	strList.append("""```yaml\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}```""".format(
	time,guild_id,guild_name,victory,our_score,enemy_score,our_combo,enemy_combo,our_guildship_down,enemy_guildship_down))

	strList.append("""```yaml\n{}\n{}\n{}\n{}\n{}\n{}\n{}```""".format(
	our_vg,enemy_vg,our_rg,enemy_rg,our_cp,enemy_cp,size_diff))

	strList.append("""```yaml\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}```""".format(
	our_total_recover,enemy_total_recover,our_total_atk_support,enemy_total_atk_support,our_total_def_support,enemy_total_def_support,
	our_total_atk_debuff,enemy_total_atk_debuff,our_total_def_debuff,enemy_total_def_debuff))
	
	strList.append("""```yaml\n{}\n{}\n{}\n{}\n{}\n{}```""".format(
	first_demon,first_demon_our_score,first_demon_enemy_score,second_demon,second_demon_our_score,second_demon_enemy_score))

	return strList, nightmare_used_and_comment_list	

