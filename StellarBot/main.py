#! python
import xlrd
import csv
from dbinsert import dbinsertImporter
# from dbinsert import ConnectToDB
import datetime
from  quickstart import downloadLoadoutList

def ReadAndInsertGuildDB(isStellar):

	START_ROW = 1

	FIELD_GUILD_NAME = 0
	FIELD_GUILD_RANK = 1
	FIELD_GUILD_TOTAL_CP = 2
	FIELD_GUILD_VG_CP = 3
	FIELD_GUILD_RG_CP = 4
	FIELD_TOTAL_RECOVER = 5
	FIELD_TOTAL_ALLY_ATK_SUPPORT = 6
	FIELD_TOTAL_ALLY_DEF_SUPPORT = 7
	FIELD_TOTAL_ENEMY_ATK_DEBUFF = 8
	FIELD_TOTAL_ENEMY_DEF_DEBUFF = 9
	FIELD_HARP_DEMON = 10
	FIELD_TOME_DEMON = 11
	FIELD_STAFF_DEMON = 12
	FIELD_COMMENTS = 13

	sheet = wb.sheet_by_index(8)

	if isStellar:
		sheet = wb.sheet_by_index(8)
	else:
		sheet = wb.sheet_by_index(9)

	for rows in range(START_ROW,sheet.nrows):
		guild_name = sheet.cell_value(rows,FIELD_GUILD_NAME)

		if sheet.cell_value(rows,FIELD_GUILD_RANK):
			guild_rank = int(sheet.cell_value(rows,FIELD_GUILD_RANK))
		else:
			guild_rank = None

		guild_total_cp = int(sheet.cell_value(rows,FIELD_GUILD_TOTAL_CP))
		guild_vg_cp = int(sheet.cell_value(rows,FIELD_GUILD_VG_CP))
		guild_rg_cp = int(sheet.cell_value(rows,FIELD_GUILD_RG_CP))
		total_recover = int(sheet.cell_value(rows,FIELD_TOTAL_RECOVER))
		total_ally_atk_support = int(sheet.cell_value(rows,FIELD_TOTAL_ALLY_ATK_SUPPORT))
		total_ally_def_support = int(sheet.cell_value(rows,FIELD_TOTAL_ALLY_DEF_SUPPORT))
		total_enemy_atk_debuff = sheet.cell_value(rows,FIELD_TOTAL_ENEMY_ATK_DEBUFF)
		total_enemy_def_debuff = int(sheet.cell_value(rows,FIELD_TOTAL_ENEMY_DEF_DEBUFF))
		harp_demon = int(sheet.cell_value(rows,FIELD_HARP_DEMON))
		tome_demon = int(sheet.cell_value(rows,FIELD_TOME_DEMON))
		staff_demon = int(sheet.cell_value(rows,FIELD_STAFF_DEMON))
		comments = sheet.cell_value(rows,FIELD_COMMENTS)


		dbinsertImporterObj.InsertToGuildsDBWithoutWeapons(guild_name, guild_rank, guild_total_cp, guild_vg_cp, guild_rg_cp,
						total_recover, total_ally_atk_support, total_ally_def_support, total_enemy_atk_debuff, total_enemy_def_debuff,
						harp_demon, tome_demon, staff_demon, str_now, comments)


def ReadAndInsertGuildDBWithWeaponDetails(isStellar):

	START_ROW = 1

	FIELD_GUILD_NAME = 0
	FIELD_GUILD_RANK = 1
	FIELD_GUILD_TOTAL_CP = 2
	FIELD_GUILD_VG_CP = 3
	FIELD_GUILD_RG_CP = 4
	FIELD_TOTAL_RECOVER = 5
	FIELD_TOTAL_ALLY_ATK_SUPPORT = 6
	FIELD_TOTAL_ALLY_DEF_SUPPORT = 7
	FIELD_TOTAL_ENEMY_ATK_DEBUFF = 8
	FIELD_TOTAL_ENEMY_DEF_DEBUFF = 9
	FIELD_HARP_DEMON = 10
	FIELD_TOME_DEMON = 11
	FIELD_STAFF_DEMON = 12
	FIELD_COMMENTS = 13

	sheet = wb.sheet_by_index(8)
	weaponDetailsFilename= ""
	fireList = []
	waterList = []
	windList = []
	miscList = []
	if isStellar:
		sheet = wb.sheet_by_index(8)
		weaponDetailsFilename = "friendlyWeaponElementalSummary.csv"
	else:
		sheet = wb.sheet_by_index(9)
		weaponDetailsFilename = "enemyWeaponElementalSummary.csv"

	#Weapon details stuff
	with open(weaponDetailsFilename,encoding='utf-8') as csv_file:
		csv_reader = csv.reader(csv_file, delimiter=',')
		row_num = 0
		for row in csv_reader:
			if row_num == 0:
				fireList = [int(row[0]),row[1],int(row[2]),row[3],
								 int(row[4]),int(row[5]),int(row[6]),int(row[7]),int(row[8]),
								 int(row[9]),int(row[10]),int(row[11]),int(row[12]),int(row[13])]
			elif row_num == 1:
				waterList = [int(row[0]),row[1],int(row[2]),row[3],
								 int(row[4]),int(row[5]),int(row[6]),int(row[7]),int(row[8]),
								 int(row[9]),int(row[10]),int(row[11]),int(row[12]),int(row[13])]

			elif row_num == 2:
				windList = [int(row[0]),row[1],int(row[2]),row[3],
								 int(row[4]),int(row[5]),int(row[6]),int(row[7]),int(row[8]),
								 int(row[9]),int(row[10]),int(row[11]),int(row[12]),int(row[13])]
			elif row_num == 3:
				miscList = [int(row[0])]

			row_num += 1


	#There is only one row. There should only be one row
	for rows in range(START_ROW,sheet.nrows):
		guild_name = sheet.cell_value(rows,FIELD_GUILD_NAME)

		if sheet.cell_value(rows,FIELD_GUILD_RANK):
			guild_rank = int(sheet.cell_value(rows,FIELD_GUILD_RANK))
		else:
			guild_rank = None

		guild_total_cp = int(sheet.cell_value(rows,FIELD_GUILD_TOTAL_CP))
		guild_vg_cp = int(sheet.cell_value(rows,FIELD_GUILD_VG_CP))
		guild_rg_cp = int(sheet.cell_value(rows,FIELD_GUILD_RG_CP))
		total_recover = int(sheet.cell_value(rows,FIELD_TOTAL_RECOVER))
		total_ally_atk_support = int(sheet.cell_value(rows,FIELD_TOTAL_ALLY_ATK_SUPPORT))
		total_ally_def_support = int(sheet.cell_value(rows,FIELD_TOTAL_ALLY_DEF_SUPPORT))
		total_enemy_atk_debuff = sheet.cell_value(rows,FIELD_TOTAL_ENEMY_ATK_DEBUFF)
		total_enemy_def_debuff = int(sheet.cell_value(rows,FIELD_TOTAL_ENEMY_DEF_DEBUFF))
		harp_demon = int(sheet.cell_value(rows,FIELD_HARP_DEMON))
		tome_demon = int(sheet.cell_value(rows,FIELD_TOME_DEMON))
		staff_demon = int(sheet.cell_value(rows,FIELD_STAFF_DEMON))
		comments = sheet.cell_value(rows,FIELD_COMMENTS)


		dbinsertImporterObj.InsertToGuildsDBWithWeaponsDetails(guild_name, guild_rank, guild_total_cp, guild_vg_cp, guild_rg_cp,
						total_recover, total_ally_atk_support, total_ally_def_support, total_enemy_atk_debuff, total_enemy_def_debuff,
						harp_demon, tome_demon, staff_demon,
						fireList[0],
						fireList[1],fireList[2],fireList[3],fireList[4],fireList[5],
						fireList[6],fireList[7],fireList[8],fireList[9],fireList[10],
						waterList[0],
						waterList[1],waterList[2],waterList[3],waterList[4],waterList[5],
						waterList[6],waterList[7],waterList[8],waterList[9],waterList[10],
						windList[0],
						windList[1],windList[2],windList[3],windList[4],windList[5],
						windList[6],windList[7],windList[8],windList[9],windList[10],
						fireList[11],fireList[12],waterList[11],waterList[12],windList[11],windList[12],
						fireList[13],waterList[13],windList[13],
						miscList[0],
						str_now, comments)


def UpdateGuildDBWithWeaponDetails(isStellar, guild_id):

	START_ROW = 1

	FIELD_GUILD_NAME = 0
	FIELD_GUILD_RANK = 1
	FIELD_GUILD_TOTAL_CP = 2
	FIELD_GUILD_VG_CP = 3
	FIELD_GUILD_RG_CP = 4
	FIELD_TOTAL_RECOVER = 5
	FIELD_TOTAL_ALLY_ATK_SUPPORT = 6
	FIELD_TOTAL_ALLY_DEF_SUPPORT = 7
	FIELD_TOTAL_ENEMY_ATK_DEBUFF = 8
	FIELD_TOTAL_ENEMY_DEF_DEBUFF = 9
	FIELD_HARP_DEMON = 10
	FIELD_TOME_DEMON = 11
	FIELD_STAFF_DEMON = 12
	FIELD_COMMENTS = 13

	sheet = wb.sheet_by_index(8)
	weaponDetailsFilename= ""
	fireList = []
	waterList = []
	windList = []
	miscList = []
	if isStellar:
		sheet = wb.sheet_by_index(8)
		weaponDetailsFilename = "friendlyWeaponElementalSummary.csv"
	else:
		sheet = wb.sheet_by_index(9)
		weaponDetailsFilename = "enemyWeaponElementalSummary.csv"

	#Weapon details stuff
	with open(weaponDetailsFilename,encoding='utf-8') as csv_file:
		csv_reader = csv.reader(csv_file, delimiter=',')
		row_num = 0
		for row in csv_reader:
			if row_num == 0:
				fireList = [int(row[0]),row[1],int(row[2]),row[3],
								 int(row[4]),int(row[5]),int(row[6]),int(row[7]),int(row[8]),
								 int(row[9]),int(row[10]),int(row[11]),int(row[12]),int(row[13])]
			elif row_num == 1:
				waterList = [int(row[0]),row[1],int(row[2]),row[3],
								 int(row[4]),int(row[5]),int(row[6]),int(row[7]),int(row[8]),
								 int(row[9]),int(row[10]),int(row[11]),int(row[12]),int(row[13])]

			elif row_num == 2:
				windList = [int(row[0]),row[1],int(row[2]),row[3],
								 int(row[4]),int(row[5]),int(row[6]),int(row[7]),int(row[8]),
								 int(row[9]),int(row[10]),int(row[11]),int(row[12]),int(row[13])]
			elif row_num == 3:
				miscList = [int(row[0])]

			row_num += 1


	#There is only one row. There should only be one row
	for rows in range(START_ROW,sheet.nrows):
		guild_name = sheet.cell_value(rows,FIELD_GUILD_NAME)

		if sheet.cell_value(rows,FIELD_GUILD_RANK):
			guild_rank = int(sheet.cell_value(rows,FIELD_GUILD_RANK))
		else:
			guild_rank = None


		dbinsertImporterObj.UpdateGuildsDBWithWeaponsDetails(guild_id,
						fireList[0],
						fireList[1],fireList[2],fireList[3],fireList[4],fireList[5],
						fireList[6],fireList[7],fireList[8],fireList[9],fireList[10],
						waterList[0],
						waterList[1],waterList[2],waterList[3],waterList[4],waterList[5],
						waterList[6],waterList[7],waterList[8],waterList[9],waterList[10],
						windList[0],
						windList[1],windList[2],windList[3],windList[4],windList[5],
						windList[6],windList[7],windList[8],windList[9],windList[10],
						fireList[11],fireList[12],waterList[11],waterList[12],windList[11],windList[12],
						fireList[13],waterList[13],windList[13],
						miscList[0])


def ReadAndInsertToPlayerDB(isStellar):

	START_ROW = 1

	FIELD_ID = 0
	FIELD_NAME = 1
	FIELD_PLAYER_RANK = 2
	FIELD_GUILD_NAME = 3
	FIELD_PATK = 4
	FIELD_PDEF = 5
	FIELD_MATK = 6
	FIELD_MDEF = 7
	FIELD_HERO_CLASS = 8
	FIELD_RECOVER = 9
	FIELD_ALLY_ATK_SUPPORT = 10
	FIELD_ALLY_DEF_SUPPORT = 11
	FIELD_ENEMY_ATK_DEBUFF = 12
	FIELD_ENEMY_DEF_DEBUFF = 13
	FIELD_HARP_DEMON = 14
	FIELD_TOME_DEMON = 15
	FIELD_STAFF_DEMON = 16

	sheet = wb.sheet_by_index(0)

	if isStellar:
		sheet = wb.sheet_by_index(5)
	else:
		sheet = wb.sheet_by_index(6)

	for rows in range(START_ROW,sheet.nrows): 

		if sheet.cell_value(rows,FIELD_ID):
			id = int(sheet.cell_value(rows,FIELD_ID))
		else:
			continue

		name = sheet.cell_value(rows,FIELD_NAME)
		player_rank = int(sheet.cell_value(rows,FIELD_PLAYER_RANK))
		guild_name = sheet.cell_value(rows,FIELD_GUILD_NAME)
		patk = int(sheet.cell_value(rows,FIELD_PATK))
		pdef = int(sheet.cell_value(rows,FIELD_PDEF))
		matk = int(sheet.cell_value(rows,FIELD_MATK))
		mdef = int(sheet.cell_value(rows,FIELD_MDEF))
		hero_class = sheet.cell_value(rows,FIELD_HERO_CLASS)
		recover = int(sheet.cell_value(rows,FIELD_RECOVER))
		ally_atk_support = int(sheet.cell_value(rows,FIELD_ALLY_ATK_SUPPORT))
		ally_def_support = int(sheet.cell_value(rows,FIELD_ALLY_DEF_SUPPORT))
		enemy_atk_debuff = int(sheet.cell_value(rows,FIELD_ENEMY_ATK_DEBUFF))
		enemy_def_debuff = int(sheet.cell_value(rows,FIELD_ENEMY_DEF_DEBUFF))
		harp_demon = int(sheet.cell_value(rows,FIELD_HARP_DEMON))
		tome_demon = int(sheet.cell_value(rows,FIELD_TOME_DEMON))
		staff_demon = int(sheet.cell_value(rows,FIELD_STAFF_DEMON))

		dbinsertImporterObj.InsertToPlayerDB(id,name,player_rank,guild_name,
							patk,pdef,matk,mdef,hero_class,
							recover,ally_atk_support,ally_def_support,enemy_atk_debuff,enemy_def_debuff,
							harp_demon,tome_demon,staff_demon,str_now)


def ReadAndInsertToWinLoseDB():

	START_ROW = 1

	FIELD_GUILD_NAME = 0
	FIELD_IS_VICTORY = 1
	FIELD_OUR_SCORE = 2
	FIELD_ENEMY_SCORE = 3
	FIELD_OUR_COMBO = 4
	FIELD_ENEMY_COMBO = 5
	FIELD_OUR_GUILDSHIP_DOWN = 6
	FIELD_ENEMY_GUILDSHIP_DOWN = 7

	FIELD_OUR_VG = 8
	FIELD_ENEMY_VG = 9
	FIELD_OUR_RG = 10
	FIELD_ENEMY_RG = 11

	FIELD_OUR_TOTAL_RECOVER = 12
	FIELD_ENEMY_TOTAL_RECOVER = 13
	FIELD_OUR_TOTAL_ALLY_ATK_SUPPORT = 14
	FIELD_ENEMY_TOTAL_ALLY_ATK_SUPPORT = 15
	FIELD_OUR_TOTAL_ALLY_DEF_SUPPORT = 16
	FIELD_ENEMY_TOTAL_ALLY_DEF_SUPPORT = 17
	FIELD_OUR_TOTAL_ENEMY_ATK_DEBUFF = 18
	FIELD_ENEMY_TOTAL_ENEMY_ATK_DEBUFF = 19
	FIELD_OUR_TOTAL_ENEMY_DEF_DEBUFF = 20
	FIELD_ENEMY_TOTAL_ENEMY_DEF_DEBUFF = 21

	FIELD_FIRST_DEMON = 22
	FIELD_FIRST_DEMON_GOTTEN = 23
	FIELD_FIRST_DEMON_OUR_SCORE = 24
	FIELD_FIRST_DEMON_ENEMY_SCORE = 25

	FIELD_SECOND_DEMON = 26
	FIELD_SECOND_DEMON_GOTTEN = 27
	FIELD_SECOND_DEMON_OUR_SCORE = 28
	FIELD_SECOND_DEMON_ENEMY_SCORE = 29

	FIELD_OUR_NIGHTMARE_USED = 30
	FIELD_ENEMY_NIGHTMARE_USED = 31
	FIELD_COMMENTS = 32

	sheet = wb.sheet_by_index(7)

	for rows in range(START_ROW,sheet.nrows):
		guild_name = sheet.cell_value(rows,FIELD_GUILD_NAME)
		is_victory = int(sheet.cell_value(rows,FIELD_IS_VICTORY))
		our_score = int(sheet.cell_value(rows,FIELD_OUR_SCORE))
		enemy_score = int(sheet.cell_value(rows,FIELD_ENEMY_SCORE))
		our_combo = int(sheet.cell_value(rows,FIELD_OUR_COMBO))
		enemy_combo = int(sheet.cell_value(rows,FIELD_ENEMY_COMBO))
		our_guildship_down = int(sheet.cell_value(rows,FIELD_OUR_GUILDSHIP_DOWN))
		enemy_guildship_down = int(sheet.cell_value(rows,FIELD_ENEMY_GUILDSHIP_DOWN))

		our_vg = int(sheet.cell_value(rows,FIELD_OUR_VG))
		enemy_vg = int(sheet.cell_value(rows,FIELD_ENEMY_VG))
		our_rg = int(sheet.cell_value(rows,FIELD_OUR_RG))
		enemy_rg = int(sheet.cell_value(rows,FIELD_ENEMY_RG))

		our_total_recover = int(sheet.cell_value(rows,FIELD_OUR_TOTAL_RECOVER))
		enemy_total_recover = int(sheet.cell_value(rows,FIELD_ENEMY_TOTAL_RECOVER))
		our_total_ally_atk_support = int(sheet.cell_value(rows,FIELD_OUR_TOTAL_ALLY_ATK_SUPPORT))
		enemy_total_ally_atk_support = int(sheet.cell_value(rows,FIELD_ENEMY_TOTAL_ALLY_ATK_SUPPORT))
		our_total_ally_def_support = int(sheet.cell_value(rows,FIELD_OUR_TOTAL_ALLY_DEF_SUPPORT))
		enemy_total_ally_def_support = int(sheet.cell_value(rows,FIELD_ENEMY_TOTAL_ALLY_DEF_SUPPORT))
		our_total_enemy_atk_debuff = sheet.cell_value(rows,FIELD_OUR_TOTAL_ENEMY_ATK_DEBUFF)
		enemy_total_enemy_atk_debuff = sheet.cell_value(rows,FIELD_ENEMY_TOTAL_ENEMY_ATK_DEBUFF)
		our_total_enemy_def_debuff = int(sheet.cell_value(rows,FIELD_OUR_TOTAL_ENEMY_DEF_DEBUFF))
		enemy_total_enemy_def_debuff = int(sheet.cell_value(rows,FIELD_ENEMY_TOTAL_ENEMY_DEF_DEBUFF))

		first_demon = sheet.cell_value(rows,FIELD_FIRST_DEMON)
		first_demon_gotten = int(sheet.cell_value(rows,FIELD_FIRST_DEMON_GOTTEN))
		first_demon_our_score = int(sheet.cell_value(rows,FIELD_FIRST_DEMON_OUR_SCORE))
		first_demon_enemy_score = int(sheet.cell_value(rows,FIELD_FIRST_DEMON_ENEMY_SCORE))

		second_demon = sheet.cell_value(rows,FIELD_SECOND_DEMON)
		second_demon_gotten = int(sheet.cell_value(rows,FIELD_SECOND_DEMON_GOTTEN))
		second_demon_our_score = int(sheet.cell_value(rows,FIELD_SECOND_DEMON_OUR_SCORE))
		second_demon_enemy_score = int(sheet.cell_value(rows,FIELD_SECOND_DEMON_ENEMY_SCORE))

		our_nightmare_used = sheet.cell_value(rows,FIELD_OUR_NIGHTMARE_USED)
		enemy_nightmare_used = sheet.cell_value(rows,FIELD_ENEMY_NIGHTMARE_USED)
		comments = sheet.cell_value(rows,FIELD_COMMENTS)

		dbinsertImporterObj.InsertToWinLoseDB(guild_name, is_victory, our_score, enemy_score, our_combo, enemy_combo, our_guildship_down, enemy_guildship_down,
						  our_vg, enemy_vg, our_rg, enemy_rg,
						  our_total_recover,enemy_total_recover,our_total_ally_atk_support,enemy_total_ally_atk_support,
						  our_total_ally_def_support,enemy_total_ally_def_support,our_total_enemy_atk_debuff,enemy_total_enemy_atk_debuff,
						  our_total_enemy_def_debuff,enemy_total_enemy_def_debuff,
						  first_demon, first_demon_gotten, first_demon_our_score, first_demon_enemy_score,
						  second_demon, second_demon_gotten, second_demon_our_score, second_demon_enemy_score,
						  our_nightmare_used, enemy_nightmare_used,
						  str_now, comments)

loc = ("LoadoutList.xlsx")

now = datetime.datetime.now()
str_now = now.date().isoformat()
one_day = datetime.timedelta(days=1)
yesterday = now - one_day
str_now = yesterday.date().isoformat()


downloadLoadoutList()

wb = xlrd.open_workbook(loc)

dbinsertImporterObj = dbinsertImporter()

# ReadAndInsertGuildDB(1)
# ReadAndInsertGuildDB(0)
ReadAndInsertGuildDBWithWeaponDetails(1)
ReadAndInsertGuildDBWithWeaponDetails(0)
ReadAndInsertToPlayerDB(1)
ReadAndInsertToPlayerDB(0)
ReadAndInsertToWinLoseDB()
# UpdateGuildDBWithWeaponDetails(0,290)

# dbinsertImporterObj.CommitDB()
