import os
import random
import datetime

def GetStatsByName(dbinsertImporterObj,playerName,limit):

	if limit > 5:
		limit = 5
	players=dbinsertImporterObj.GetPlayer(playerName,1,limit)
	return FormatStatsDetails(players)

def GetStatsByNameAndDate(dbinsertImporterObj,playerName,startDate,endDate):

	players=dbinsertImporterObj.GetPlayerByDate(playerName,1,startDate,endDate)
	return FormatStatsDetails(players)

def GetStatsById(dbinsertImporterObj,id,limit=3):

	if limit > 5:
		limit = 5
	players=dbinsertImporterObj.GetPlayer(id,0,limit)
	return FormatStatsDetails(players)

def GetStatsByIdAndDate(dbinsertImporterObj,id,startDate,endDate):

	players=dbinsertImporterObj.GetPlayerByDate(id,0,startDate,endDate)
	return FormatStatsDetails(players)

def GetStatsByGuildId(dbinsertImporterObj,guildId):

	strList = []
	players=dbinsertImporterObj.GetStatsByGuildId(guildId)
	return FormatMultipleStatsDetails	(players)

def FormatMultipleStatsDetails(players):

	strlist = []

	time ="{:<15}".format("date:")
	name ="{:<15}".format("name:")
	cp ="{:<15}".format("cp:")
	rank="{:<15}".format("rank:")
	hero_class="{:<15}".format("class:")
	patk ="{:<15}".format("patk:")
	matk ="{:<15}".format("matk:")
	pdef ="{:<15}".format("pdef:")
	mdef ="{:<15}".format("mdef:")
	recover ="{:<15}".format("recover:")
	atk_support ="{:<15}".format("atk support:")
	def_support ="{:<15}".format("def support:")
	atk_debuff ="{:<15}".format("atk debuff:")
	def_debuff ="{:<15}".format("def debuff:")
	harp_demon ="{:<15}".format("harp demon:")
	tome_demon ="{:<15}".format("tome demon:")
	staff_demon ="{:<15}".format("staff debuff:")

	loop=1
	for row in reversed(players):
		dateStr = str(row['last_updated'])
		time += "{:^13}".format(dateStr)
		name += "{:^13}".format(row['name'])
		cp += "{:^13}".format(row['cp'])
		rank += "{:^13}".format(row['player_rank'])
		hero_class += "{:^13}".format(row['hero_class'])
		patk += "{:^13}".format(row['patk'])
		matk += "{:^13}".format(row['matk'])
		pdef += "{:^13}".format(row['pdef'])
		mdef += "{:^13}".format(row['mdef'])
		recover += "{:^13}".format(row['recover'])
		atk_support += "{:^13}".format(row['ally_atk_support'])
		def_support += "{:^13}".format(row['ally_def_support'])
		atk_debuff += "{:^13}".format(row['enemy_atk_debuff'])
		def_debuff += "{:^13}".format(row['enemy_def_debuff'])

		if int(row['harp_demon']) == 1:
			harp_demon += "{:^13}".format("GOTTEN")
		else:
			harp_demon += "{:^13}".format("NOPE")

		if int(row['tome_demon']) == 1:
			tome_demon += "{:^13}".format("GOTTEN")
		else:
			tome_demon += "{:^13}".format("NOPE")

		if int(row['staff_demon']) == 1:
			staff_demon += "{:^13}".format("GOTTEN")
		else:
			staff_demon += "{:^13}".format("NOPE")

		if loop % 3 == 0:
			strlist.append("""```yaml\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}```""".format(
			time,name,cp,rank,hero_class,patk,matk,pdef,mdef,recover,atk_support,def_support,atk_debuff,def_debuff,harp_demon,tome_demon,staff_demon))

			time ="{:<15}".format("date:")
			name ="{:<15}".format("name:")
			cp ="{:<15}".format("cp:")
			rank="{:<15}".format("rank:")
			hero_class="{:<15}".format("class:")
			patk ="{:<15}".format("patk:")
			matk ="{:<15}".format("matk:")
			pdef ="{:<15}".format("pdef:")
			mdef ="{:<15}".format("mdef:")
			recover ="{:<15}".format("recover:")
			atk_support ="{:<15}".format("atk support:")
			def_support ="{:<15}".format("def support:")
			atk_debuff ="{:<15}".format("atk debuff:")
			def_debuff ="{:<15}".format("def debuff:")
			harp_demon ="{:<15}".format("harp demon:")
			tome_demon ="{:<15}".format("tome demon:")
			staff_demon ="{:<15}".format("staff debuff:")

		loop += 1

	return strlist

def FormatStatsDetails(players):
	time ="{:<15}".format("date:")
	name ="{:<15}".format("name:")
	cp ="{:<15}".format("cp:")
	rank="{:<15}".format("rank:")
	hero_class="{:<15}".format("class:")
	patk ="{:<15}".format("patk:")
	matk ="{:<15}".format("matk:")
	pdef ="{:<15}".format("pdef:")
	mdef ="{:<15}".format("mdef:")
	recover ="{:<15}".format("recover:")
	atk_support ="{:<15}".format("atk support:")
	def_support ="{:<15}".format("def support:")
	atk_debuff ="{:<15}".format("atk debuff:")
	def_debuff ="{:<15}".format("def debuff:")
	harp_demon ="{:<15}".format("harp demon:")
	tome_demon ="{:<15}".format("tome demon:")
	staff_demon ="{:<15}".format("staff debuff:")

	for row in reversed(players):
		dateStr = str(row['last_updated'])
		time += "{:^13}".format(dateStr)
		name += "{:^13}".format(row['name'])
		cp += "{:^13}".format(row['cp'])
		rank += "{:^13}".format(row['player_rank'])
		hero_class += "{:^13}".format(row['hero_class'])
		patk += "{:^13}".format(row['patk'])
		matk += "{:^13}".format(row['matk'])
		pdef += "{:^13}".format(row['pdef'])
		mdef += "{:^13}".format(row['mdef'])
		recover += "{:^13}".format(row['recover'])
		atk_support += "{:^13}".format(row['ally_atk_support'])
		def_support += "{:^13}".format(row['ally_def_support'])
		atk_debuff += "{:^13}".format(row['enemy_atk_debuff'])
		def_debuff += "{:^13}".format(row['enemy_def_debuff'])

		if int(row['harp_demon']) == 1:
			harp_demon += "{:^13}".format("GOTTEN")
		else:
			harp_demon += "{:^13}".format("NOPE")

		if int(row['tome_demon']) == 1:
			tome_demon += "{:^13}".format("GOTTEN")
		else:
			tome_demon += "{:^13}".format("NOPE")

		if int(row['staff_demon']) == 1:
			staff_demon += "{:^13}".format("GOTTEN")
		else:
			staff_demon += "{:^13}".format("NOPE")


	return """```{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}\n{}```""".format(
		time,name,cp,rank,hero_class,patk,matk,pdef,mdef,recover,atk_support,def_support,atk_debuff,def_debuff,harp_demon,tome_demon,staff_demon)