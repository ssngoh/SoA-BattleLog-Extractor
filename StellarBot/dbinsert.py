import mysql.connector
import datetime
import csv

class dbinsertImporter():

	def __init__(self):
		print("dbInsert init")
		global mydb 
		mydb = mysql.connector.connect(
			host="localhost",
			user="root",
			password="operationcwal",
			database="sinoalice_playerdb")

		global mycursor 
		mycursor = mydb.cursor(buffered=True, dictionary=True)

		# now = datetime.datetime.now()
		# str_now = now.date().isoformat()
		# one_day = datetime.timedelta(days=1)
		# yesterday = now - one_day
		print(mycursor)

	def GetDateTime(self,delta):
		now = datetime.datetime.now()
		str_now = now.date().isoformat()
		delta_day = datetime.timedelta(days=delta)
		return now - delta_day

	def CreateSinoAliceDB(self):
		mycursor.execute("CREATE DATABASE sinoalice_playerdb")

	def ShowDatabase(self):
		mycursor.execute("SHOW DATABASES")
		for x in mycursor:
			print(x)

	def DropTable(self,table):
		sql = "DROP TABLE " + table
		mycursor.execute(sql)

	def DropAllTables(self):
		sql = "DROP TABLE IF EXISTS playerdb, guildsdb, winlosedb"
		mycursor.execute(sql)

	def CommitDB(self):
		mydb.commit()

	def CreateTables(self):
		mycursor.execute("""CREATE TABLE playerdb 
			(ID char(9) NOT NULL, name varchar(255), cp INT NOT NULL, player_rank INT NOT NULL, guild_id INT, guild_name varchar(255),
			patk INT NOT NULL, pdef INT NOT NULL, matk INT NOT NULL, mdef INT NOT NULL, hero_class varchar(255) NOT NULL,
			recover INT, ally_atk_support INT, ally_def_support INT, enemy_atk_debuff INT, enemy_def_debuff INT, 
			harp_demon INT, tome_demon INT, staff_demon INT,  last_updated DATE,
			CONSTRAINT PK_PLAYER PRIMARY KEY (ID, patk, pdef, matk, mdef, hero_class, last_updated))""")

		mycursor.execute("""CREATE TABLE guildsdb
			(guild_id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, guild_name VARCHAR(255), guild_rank VARCHAR(255), guild_total_cp INT, guild_vg_cp INT, guild_rg_cp INT, 
			total_recover INT, total_ally_atk_support INT, total_ally_def_support INT, total_enemy_atk_debuff INT, total_enemy_def_debuff INT, 
			harp_demon INT, tome_demon INT, staff_demon INT, 
			total_fire_harptome INT, 
			total_fire_weapons_patk_buff INT, total_fire_weapons_pdef_buff INT, total_fire_weapons_matk_buff INT, total_fire_weapons_mdef_buff INT, total_fire_weapons_buff INT,
			total_fire_weapons_patk_debuff INT, total_fire_weapons_pdef_debuff INT, total_fire_weapons_matk_debuff INT, total_fire_weapons_mdef_debuff INT, total_fire_weapons_debuff INT,
			total_water_harptome INT, 
			total_water_weapons_patk_buff INT, total_water_weapons_pdef_buff INT, total_water_weapons_matk_buff INT, total_water_weapons_mdef_buff INT, total_water_weapons_buff INT,
			total_water_weapons_patk_debuff INT, total_water_weapons_pdef_debuff INT, total_water_weapons_matk_debuff INT, total_water_weapons_mdef_debuff INT, total_water_weapons_debuff INT,
			total_wind_harptome INT, 
			total_wind_weapons_patk_buff INT, total_wind_weapons_pdef_buff INT, total_wind_weapons_matk_buff INT, total_wind_weapons_mdef_buff INT, total_wind_weapons_buff INT,
			total_wind_weapons_patk_debuff INT, total_wind_weapons_pdef_debuff INT, total_wind_weapons_matk_debuff INT, total_wind_weapons_mdef_debuff INT, total_wind_weapons_debuff INT,
			total_fire_staves INT, total_fire_heal INT, total_water_staves INT, total_water_heal INT, total_wind_staves INT, total_wind_heal INT,
			total_fire_vg_weapons INT, total_water_vg_weapons INT, total_wind_vg_weapons INT, total_bonus_combo_weapons INT,
			comments text, last_updated DATE NOT NULL,
			CONSTRAINT PK_GUILD PRIMARY KEY(guild_id,last_updated))""")

		mycursor.execute("""CREATE TABLE winlosedb
			(guild_id INT NOT NULL PRIMARY KEY, guild_name varchar(255), is_victory BOOLEAN NOT NULL, our_score INT, enemy_score INT, our_combo INT, enemy_combo INT, our_guildship_down INT, enemy_guildship_down INT,
			our_vg INT NOT NULL, enemy_vg INT NOT NULL,  our_rg INT NOT NULL, enemy_rg INT NOT NULL, our_cp INT NOT NULL, enemy_cp INT NOT NULL, size_diff DECIMAL(4,2) NOT NULL,
			total_recover INT, total_ally_atk_support INT, total_ally_def_support INT, total_enemy_atk_debuff INT, total_enemy_def_debuff INT,
			first_demon varchar(255), first_demon_gotten BOOLEAN, first_demon_our_score INT, first_demon_enemy_score INT,
			second_demon varchar(255), second_demon_gotten BOOLEAN, second_demon_our_score INT, second_demon_enemy_score INT,
			our_nightmare_used text, enemy_nightmare_used text,
			colo_date DATE, comments text,
			FOREIGN KEY (guild_id) REFERENCES guildsdb(guild_id) )""")

	def InsertToPlayerDB(self, id, name, player_rank, guild_name,
						 patk, pdef, matk, mdef, hero_class, 
						 recover, ally_atk_support, ally_def_support, enemy_atk_debuff, enemy_def_debuff, 
						 harp_demon, tome_demon, staff_demon, last_updated):

		sql = "SELECT guild_id from guildsdb where guild_name = %s and last_updated = %s"
		adr = (guild_name,last_updated)
		mycursor.execute(sql,adr)
		guild_id=mycursor.fetchone()

		if not guild_id:
			guild_id=0
		else:
			guild_id = guild_id['guild_id']

		print("Inserting to player db",id,name,player_rank,guild_id, guild_name,
									   patk,pdef,matk,mdef,hero_class,
									   recover, ally_atk_support, ally_def_support, enemy_atk_debuff, enemy_def_debuff, 
									   harp_demon, tome_demon, staff_demon, last_updated)

		sql = """INSERT INTO playerdb (ID, name, player_rank, cp, guild_id, guild_name, 
		patk, pdef, matk, mdef, hero_class,
		recover, ally_atk_support, ally_def_support, enemy_atk_debuff, enemy_def_debuff,
		harp_demon, tome_demon, staff_demon, last_updated) 
		VALUES (%s,%s,%s,%s,%s,%s,
				%s,%s,%s,%s,%s,
				%s,%s,%s,%s,%s,
				%s,%s,%s,%s)"""

		cp = patk+pdef+matk+mdef

		val = (id,name,player_rank,cp,guild_id,guild_name,
			   patk,pdef,matk,mdef,hero_class,
			   recover,ally_atk_support,ally_def_support,enemy_atk_debuff,enemy_def_debuff,
			   harp_demon,tome_demon,staff_demon,last_updated)

		mycursor.execute(sql,val)


	def InsertToGuildsDBWithoutWeapons(self, guild_name, guild_rank, guild_total_cp, guild_vg_cp, guild_rg_cp,
						total_recover, total_ally_atk_support, total_ally_def_support, total_enemy_atk_debuff, total_enemy_def_debuff,
						harp_demon, tome_demon, staff_demon, last_updated, comments):


		print("Inserting to guildsdb ",guild_name, guild_rank, guild_total_cp, guild_vg_cp, guild_rg_cp,
						total_recover, total_ally_atk_support, total_ally_def_support, total_enemy_atk_debuff, total_enemy_def_debuff,
						harp_demon, tome_demon, staff_demon, last_updated, comments)

		sql = """INSERT INTO guildsdb (guild_id, guild_name, guild_rank, guild_total_cp, guild_vg_cp, guild_rg_cp,
		total_recover, total_ally_atk_support, total_ally_def_support, total_enemy_atk_debuff, total_enemy_def_debuff,
		harp_demon, tome_demon, staff_demon, last_updated, comments) 
		VALUES (%s,%s,%s,%s,%s,%s,
				%s,%s,%s,%s,%s,
				%s,%s,%s,%s,%s)"""

		guild_id = None #Auto increment
		val = (guild_id,guild_name, guild_rank, guild_total_cp, guild_vg_cp, guild_rg_cp,
			   total_recover, total_ally_atk_support, total_ally_def_support, total_enemy_atk_debuff, total_enemy_def_debuff,
			   harp_demon, tome_demon, staff_demon, last_updated, comments)

		mycursor.execute(sql,val)


	def InsertToGuildsDBWithWeaponsDetails(self, guild_name, guild_rank, guild_total_cp, guild_vg_cp, guild_rg_cp,
						total_recover, total_ally_atk_support, total_ally_def_support, total_enemy_atk_debuff, total_enemy_def_debuff,
						harp_demon, tome_demon, staff_demon,
						total_fire_harptome,
						total_fire_weapons_patk_buff,total_fire_weapons_pdef_buff,total_fire_weapons_matk_buff,total_fire_weapons_mdef_buff,total_fire_weapons_buff,
						total_fire_weapons_patk_debuff,total_fire_weapons_pdef_debuff,total_fire_weapons_matk_debuff,total_fire_weapons_mdef_debuff,total_fire_weapons_debuff,
						total_water_harptome,
						total_water_weapons_patk_buff,total_water_weapons_pdef_buff,total_water_weapons_matk_buff,total_water_weapons_mdef_buff,total_water_weapons_buff,
						total_water_weapons_patk_debuff,total_water_weapons_pdef_debuff,total_water_weapons_matk_debuff,total_water_weapons_mdef_debuff,total_water_weapons_debuff,
						total_wind_harptome,
						total_wind_weapons_patk_buff,total_wind_weapons_pdef_buff,total_wind_weapons_matk_buff,total_wind_weapons_mdef_buff,total_wind_weapons_buff,
						total_wind_weapons_patk_debuff,total_wind_weapons_pdef_debuff,total_wind_weapons_matk_debuff,total_wind_weapons_mdef_debuff,total_wind_weapons_debuff,
						total_fire_staves,total_fire_heal,total_water_staves,total_water_heal,total_wind_staves,total_wind_heal,
						total_fire_vg_weapons,total_water_vg_weapons,total_wind_vg_weapons,total_bonus_combo_weapons,
						last_updated, comments):


		print("Inserting to guildsdb ",guild_name, guild_rank, guild_total_cp, guild_vg_cp, guild_rg_cp,
						total_recover, total_ally_atk_support, total_ally_def_support, total_enemy_atk_debuff, total_enemy_def_debuff,
						harp_demon, tome_demon, staff_demon, 
						total_fire_harptome,
						total_fire_weapons_patk_buff,total_fire_weapons_pdef_buff,total_fire_weapons_matk_buff,total_fire_weapons_mdef_buff,total_fire_weapons_buff,
						total_fire_weapons_patk_debuff,total_fire_weapons_pdef_debuff,total_fire_weapons_matk_debuff,total_fire_weapons_mdef_debuff,total_fire_weapons_debuff,
						total_water_harptome,
						total_water_weapons_patk_buff,total_water_weapons_pdef_buff,total_water_weapons_matk_buff,total_water_weapons_mdef_buff,total_water_weapons_buff,
						total_water_weapons_patk_debuff,total_water_weapons_pdef_debuff,total_water_weapons_matk_debuff,total_water_weapons_mdef_debuff,total_water_weapons_debuff,
						total_wind_harptome,
						total_wind_weapons_patk_buff,total_wind_weapons_pdef_buff,total_wind_weapons_matk_buff,total_wind_weapons_mdef_buff,total_wind_weapons_buff,
						total_wind_weapons_patk_debuff,total_wind_weapons_pdef_debuff,total_wind_weapons_matk_debuff,total_wind_weapons_mdef_debuff,total_wind_weapons_debuff,
						total_fire_staves,total_fire_heal,total_water_staves,total_water_heal,total_wind_staves,total_wind_heal,
						total_fire_vg_weapons,total_water_vg_weapons,total_wind_vg_weapons,total_bonus_combo_weapons,
						last_updated, comments)

		sql = """INSERT INTO guildsdb (guild_id, guild_name, guild_rank, guild_total_cp, guild_vg_cp, guild_rg_cp,
		total_recover, total_ally_atk_support, total_ally_def_support, total_enemy_atk_debuff, total_enemy_def_debuff,
		harp_demon, tome_demon, staff_demon, 
		total_fire_harptome,
		total_fire_weapons_patk_buff,total_fire_weapons_pdef_buff,total_fire_weapons_matk_buff,total_fire_weapons_mdef_buff,total_fire_weapons_buff,
		total_fire_weapons_patk_debuff,total_fire_weapons_pdef_debuff,total_fire_weapons_matk_debuff,total_fire_weapons_mdef_debuff,total_fire_weapons_debuff,
		total_water_harptome,
		total_water_weapons_patk_buff,total_water_weapons_pdef_buff,total_water_weapons_matk_buff,total_water_weapons_mdef_buff,total_water_weapons_buff,
		total_water_weapons_patk_debuff,total_water_weapons_pdef_debuff,total_water_weapons_matk_debuff,total_water_weapons_mdef_debuff,total_water_weapons_debuff,
		total_wind_harptome,
		total_wind_weapons_patk_buff,total_wind_weapons_pdef_buff,total_wind_weapons_matk_buff,total_wind_weapons_mdef_buff,total_wind_weapons_buff,
		total_wind_weapons_patk_debuff,total_wind_weapons_pdef_debuff,total_wind_weapons_matk_debuff,total_wind_weapons_mdef_debuff,total_wind_weapons_debuff,
		total_fire_staves,total_fire_heal,total_water_staves,total_water_heal,total_wind_staves,total_wind_heal,
		total_fire_vg_weapons,total_water_vg_weapons,total_wind_vg_weapons,total_bonus_combo_weapons,
		last_updated, comments) 
		VALUES (%s,%s,%s,%s,%s,%s,
				%s,%s,%s,%s,%s,
				%s,%s,%s,
				%s,
				%s,%s,%s,%s,%s,
				%s,%s,%s,%s,%s,
				%s,
				%s,%s,%s,%s,%s,
				%s,%s,%s,%s,%s,
				%s,
				%s,%s,%s,%s,%s,
				%s,%s,%s,%s,%s,
				%s,%s,%s,%s,%s,%s,
				%s,%s,%s,%s,
				%s,%s)"""

		guild_id = None #Auto increment
		val = (guild_id,guild_name, guild_rank, guild_total_cp, guild_vg_cp, guild_rg_cp,
			   total_recover, total_ally_atk_support, total_ally_def_support, total_enemy_atk_debuff, total_enemy_def_debuff,
			   harp_demon, tome_demon, staff_demon,
			   total_fire_harptome,
			   total_fire_weapons_patk_buff,total_fire_weapons_pdef_buff,total_fire_weapons_matk_buff,total_fire_weapons_mdef_buff,total_fire_weapons_buff,
			   total_fire_weapons_patk_debuff,total_fire_weapons_pdef_debuff,total_fire_weapons_matk_debuff,total_fire_weapons_mdef_debuff,total_fire_weapons_debuff,
			   total_water_harptome,
			   total_water_weapons_patk_buff,total_water_weapons_pdef_buff,total_water_weapons_matk_buff,total_water_weapons_mdef_buff,total_water_weapons_buff,
			   total_water_weapons_patk_debuff,total_water_weapons_pdef_debuff,total_water_weapons_matk_debuff,total_water_weapons_mdef_debuff,total_water_weapons_debuff,
			   total_wind_harptome,
			   total_wind_weapons_patk_buff,total_wind_weapons_pdef_buff,total_wind_weapons_matk_buff,total_wind_weapons_mdef_buff,total_wind_weapons_buff,
			   total_wind_weapons_patk_debuff,total_wind_weapons_pdef_debuff,total_wind_weapons_matk_debuff,total_wind_weapons_mdef_debuff,total_wind_weapons_debuff,
			   total_fire_staves,total_fire_heal,total_water_staves,total_water_heal,total_wind_staves,total_wind_heal,
			   total_fire_vg_weapons,total_water_vg_weapons,total_wind_vg_weapons,total_bonus_combo_weapons,
			   last_updated, comments)

		mycursor.execute(sql,val)

	def InsertToWinLoseDB(self, guild_name, is_victory, our_score, enemy_score, our_combo, enemy_combo, our_guildship_down, enemy_guildship_down,
						  our_vg, enemy_vg, our_rg, enemy_rg,
						  our_total_recover, enemy_total_recover, our_total_ally_atk_support, enemy_total_ally_atk_support,
						  our_total_ally_def_support, enemy_total_ally_def_support, our_total_enemy_atk_debuff, enemy_total_enemy_atk_debuff,
						  our_total_enemy_def_debuff, enemy_total_enemy_def_debuff,
						  first_demon, first_demon_gotten, first_demon_our_score, first_demon_enemy_score,
						  second_demon, second_demon_gotten, second_demon_our_score, second_demon_enemy_score,
						  our_nightmare_used, enemy_nightmare_used,
						  colo_date, comments):

		sql = "SELECT guild_id from guildsdb where guild_name = %s and last_updated =%s"
		adr = (guild_name,colo_date)
		mycursor.execute(sql,adr)
		guild_id=mycursor.fetchone()

		if not guild_id:
			guild_id=0
		else:
			guild_id = guild_id['guild_id']

		our_cp = our_vg + our_rg
		enemy_cp = enemy_vg + enemy_rg
		size_diff = our_cp / enemy_cp if our_cp > enemy_cp else enemy_cp / our_cp

		print("Inserting to winlose db",guild_id, guild_name, is_victory, our_score, enemy_score, our_combo, enemy_combo, our_guildship_down, enemy_guildship_down,
										our_vg, enemy_vg, our_rg, enemy_rg, our_cp, enemy_cp, size_diff,
										our_total_recover,enemy_total_recover,our_total_ally_atk_support,enemy_total_ally_atk_support,
										our_total_ally_def_support,enemy_total_ally_def_support,our_total_enemy_atk_debuff,enemy_total_enemy_atk_debuff,
										our_total_enemy_def_debuff,enemy_total_enemy_def_debuff,
										first_demon, first_demon_gotten, first_demon_our_score, first_demon_enemy_score,
										second_demon, second_demon_gotten, second_demon_our_score, second_demon_enemy_score,
										our_nightmare_used, enemy_nightmare_used,
										colo_date, comments)
			
		if guild_id:
			sql = """INSERT INTO winlosedb (guild_id, guild_name, is_victory, our_score, enemy_score, our_combo, enemy_combo, our_guildship_down, enemy_guildship_down,
			our_vg, enemy_vg, our_rg, enemy_rg, our_cp, enemy_cp, size_diff,
			our_total_recover,enemy_total_recover,our_total_ally_atk_support,enemy_total_ally_atk_support,
			our_total_ally_def_support,enemy_total_ally_def_support,our_total_enemy_atk_debuff,enemy_total_enemy_atk_debuff,
			our_total_enemy_def_debuff,enemy_total_enemy_def_debuff,
			first_demon, first_demon_gotten, first_demon_our_score, first_demon_enemy_score,
			second_demon, second_demon_gotten, second_demon_our_score, second_demon_enemy_score,
		    our_nightmare_used, enemy_nightmare_used,
		    colo_date, comments) 
			VALUES (%s,%s,%s,%s,%s,%s,%s,%s,%s,
					%s,%s,%s,%s,%s,%s,%s,
					%s,%s,%s,%s,
					%s,%s,%s,%s,
					%s,%s,
					%s,%s,%s,%s,
					%s,%s,%s,%s,
					%s,%s,
					%s,%s)"""

			val = (guild_id, guild_name, is_victory, our_score, enemy_score, our_combo, enemy_combo, our_guildship_down, enemy_guildship_down,
				   our_vg, enemy_vg, our_rg, enemy_rg, our_cp, enemy_cp, size_diff,
				   our_total_recover,enemy_total_recover,our_total_ally_atk_support,enemy_total_ally_atk_support,
				   our_total_ally_def_support,enemy_total_ally_def_support,our_total_enemy_atk_debuff,enemy_total_enemy_atk_debuff,
				   our_total_enemy_def_debuff,enemy_total_enemy_def_debuff,
				   first_demon, first_demon_gotten, first_demon_our_score, first_demon_enemy_score,
				   second_demon, second_demon_gotten, second_demon_our_score, second_demon_enemy_score,
				   our_nightmare_used, enemy_nightmare_used,
				   colo_date, comments)

			mycursor.execute(sql,val)

		else:
			print("No matching rows found")

	def UpdateGuildsDBWithWeaponsDetails(self, guild_id,
						total_fire_harptome,
						total_fire_weapons_patk_buff,total_fire_weapons_pdef_buff,total_fire_weapons_matk_buff,total_fire_weapons_mdef_buff,total_fire_weapons_buff,
						total_fire_weapons_patk_debuff,total_fire_weapons_pdef_debuff,total_fire_weapons_matk_debuff,total_fire_weapons_mdef_debuff,total_fire_weapons_debuff,
						total_water_harptome,
						total_water_weapons_patk_buff,total_water_weapons_pdef_buff,total_water_weapons_matk_buff,total_water_weapons_mdef_buff,total_water_weapons_buff,
						total_water_weapons_patk_debuff,total_water_weapons_pdef_debuff,total_water_weapons_matk_debuff,total_water_weapons_mdef_debuff,total_water_weapons_debuff,
						total_wind_harptome,
						total_wind_weapons_patk_buff,total_wind_weapons_pdef_buff,total_wind_weapons_matk_buff,total_wind_weapons_mdef_buff,total_wind_weapons_buff,
						total_wind_weapons_patk_debuff,total_wind_weapons_pdef_debuff,total_wind_weapons_matk_debuff,total_wind_weapons_mdef_debuff,total_wind_weapons_debuff,
						total_fire_staves,total_fire_heal,total_water_staves,total_water_heal,total_wind_staves,total_wind_heal,
						total_fire_vg_weapons,total_water_vg_weapons,total_wind_vg_weapons,total_bonus_combo_weapons):


		print("Update guildsdb ", guild_id,
						total_fire_harptome,
						total_fire_weapons_patk_buff,total_fire_weapons_pdef_buff,total_fire_weapons_matk_buff,total_fire_weapons_mdef_buff,total_fire_weapons_buff,
						total_fire_weapons_patk_debuff,total_fire_weapons_pdef_debuff,total_fire_weapons_matk_debuff,total_fire_weapons_mdef_debuff,total_fire_weapons_debuff,
						total_water_harptome,
						total_water_weapons_patk_buff,total_water_weapons_pdef_buff,total_water_weapons_matk_buff,total_water_weapons_mdef_buff,total_water_weapons_buff,
						total_water_weapons_patk_debuff,total_water_weapons_pdef_debuff,total_water_weapons_matk_debuff,total_water_weapons_mdef_debuff,total_water_weapons_debuff,
						total_wind_harptome,
						total_wind_weapons_patk_buff,total_wind_weapons_pdef_buff,total_wind_weapons_matk_buff,total_wind_weapons_mdef_buff,total_wind_weapons_buff,
						total_wind_weapons_patk_debuff,total_wind_weapons_pdef_debuff,total_wind_weapons_matk_debuff,total_wind_weapons_mdef_debuff,total_wind_weapons_debuff,
						total_fire_staves,total_fire_heal,total_water_staves,total_water_heal,total_wind_staves,total_wind_heal,
						total_fire_vg_weapons,total_water_vg_weapons,total_wind_vg_weapons,total_bonus_combo_weapons)

		sql = """UPDATE guildsdb SET 
		total_fire_harptome = %s, 
		total_fire_weapons_patk_buff = %s,total_fire_weapons_pdef_buff = %s,total_fire_weapons_matk_buff = %s,total_fire_weapons_mdef_buff = %s,total_fire_weapons_buff = %s,
		total_fire_weapons_patk_debuff = %s,total_fire_weapons_pdef_debuff = %s,total_fire_weapons_matk_debuff = %s,total_fire_weapons_mdef_debuff = %s,total_fire_weapons_debuff = %s,
		total_water_harptome = %s,
		total_water_weapons_patk_buff = %s,total_water_weapons_pdef_buff = %s,total_water_weapons_matk_buff = %s,total_water_weapons_mdef_buff = %s,total_water_weapons_buff = %s,
		total_water_weapons_patk_debuff = %s,total_water_weapons_pdef_debuff = %s,total_water_weapons_matk_debuff = %s,total_water_weapons_mdef_debuff = %s,total_water_weapons_debuff = %s,
		total_wind_harptome = %s,
		total_wind_weapons_patk_buff = %s,total_wind_weapons_pdef_buff = %s,total_wind_weapons_matk_buff = %s,total_wind_weapons_mdef_buff = %s,total_wind_weapons_buff = %s,
		total_wind_weapons_patk_debuff = %s,total_wind_weapons_pdef_debuff = %s,total_wind_weapons_matk_debuff = %s,total_wind_weapons_mdef_debuff = %s,total_wind_weapons_debuff = %s,
		total_fire_staves = %s,total_fire_heal = %s,total_water_staves = %s,total_water_heal = %s,total_wind_staves = %s,total_wind_heal = %s,
		total_fire_vg_weapons = %s,total_water_vg_weapons = %s,total_wind_vg_weapons = %s,total_bonus_combo_weapons = %s where guild_id = %s"""

		val = (total_fire_harptome,
			   total_fire_weapons_patk_buff,total_fire_weapons_pdef_buff,total_fire_weapons_matk_buff,total_fire_weapons_mdef_buff,total_fire_weapons_buff,
			   total_fire_weapons_patk_debuff,total_fire_weapons_pdef_debuff,total_fire_weapons_matk_debuff,total_fire_weapons_mdef_debuff,total_fire_weapons_debuff,
			   total_water_harptome,
			   total_water_weapons_patk_buff,total_water_weapons_pdef_buff,total_water_weapons_matk_buff,total_water_weapons_mdef_buff,total_water_weapons_buff,
			   total_water_weapons_patk_debuff,total_water_weapons_pdef_debuff,total_water_weapons_matk_debuff,total_water_weapons_mdef_debuff,total_water_weapons_debuff,
			   total_wind_harptome,
			   total_wind_weapons_patk_buff,total_wind_weapons_pdef_buff,total_wind_weapons_matk_buff,total_wind_weapons_mdef_buff,total_wind_weapons_buff,
			   total_wind_weapons_patk_debuff,total_wind_weapons_pdef_debuff,total_wind_weapons_matk_debuff,total_wind_weapons_mdef_debuff,total_wind_weapons_debuff,
			   total_fire_staves,total_fire_heal,total_water_staves,total_water_heal,total_wind_staves,total_wind_heal,
			   total_fire_vg_weapons,total_water_vg_weapons,total_wind_vg_weapons,total_bonus_combo_weapons,
			   guild_id)

		mycursor.execute(sql,val)

	# def ReadAndInsertPlayerCSV(self, fileName):
	# 	with open(fileName,encoding='utf-8') as csv_file:
	# 		csv_reader = csv.reader(csv_file, delimiter=',')
	# 		row_num = 0
	# 		for row in csv_reader:
	# 			if row_num > 0:
	# 				print("Inserting",row)
	# 				InsertToPlayerDB(int(row[0]),row[1],int(row[2]),row[3],
	# 								 int(row[4]),int(row[5]),int(row[6]),int(row[7]),row[8],
	# 								 int(row[9]),int(row[10]),int(row[11]),int(row[12]),int(row[13]),
	# 								 int(row[14]),int(row[15]),int(row[16]),str_now)
	# 			else:
	# 				row_num = 1

	# def ReadAndInsertWinLoseDB(self, fileName):
	# 	with open(fileName,encoding='utf-8') as csv_file:
	# 		csv_reader = csv.reader(csv_file, delimiter=',')
	# 		row_num = 0
	# 		for row in csv_reader:
	# 			if row_num > 0:
	# 				print("Inserting",row)
	# 				InsertToWinLoseDB(row[0],int(row[1]),int(row[2]),int(row[3]),int(row[4]),int(row[5]),int(row[6]),int(row[7]),
	# 								  int(row[8]),int(row[9]),int(row[10]),int(row[11]),
	# 								  row[12],int(row[13]),int(row[14]),int(row[15]),
	# 								  row[16],int(row[17]),int(row[18]),int(row[19]),
	# 								  row[20], row[21],
	# 								  str_now,row[22])
	# 			else:
	# 				row_num = 1

##########DB QUERY FUNCTIONS HERE####################
	def GetPlayer(self, parameter, byName,limit):
		sql =""

		if byName:
			sql = "SELECT * FROM playerdb WHERE name = %s order by last_updated desc limit %s"
		else:
			sql = "SELECT * FROM playerdb where ID = %s order by last_updated desc limit %s"
		
		adr = (parameter,limit)
		mycursor.execute(sql,adr)
		players = mycursor.fetchall()

		return players

	def GetPlayerByDate(self, parameter, byName, startDate, endDate):
		sql =""

		if byName:
			sql = "SELECT * FROM playerdb where name = %s and last_updated >= %s and last_updated <= %s order by last_updated desc"
		else:
			sql = "SELECT * FROM playerdb where ID = %s and last_updated >= %s and last_updated <= %s order by last_updated desc"

		adr = (parameter,startDate,endDate)
		mycursor.execute(sql,adr)
		players = mycursor.fetchall()

		return players

	def GetStatsByGuildId(self, guildId):
		sql = "SELECT * FROM playerdb where guild_id = %s"

		adr = (guildId,)
		mycursor.execute(sql,adr)
		players = mycursor.fetchall()

		return players

	def GetColo(self,limit):

		sql = "SELECT * FROM winlosedb order by colo_date desc limit %s"
		
		adr = (limit,)
		mycursor.execute(sql,adr)
		colo = mycursor.fetchall()

		return colo

	def GetColoByDate(self, startDate, endDate):

		sql = "SELECT * FROM winlosedb where colo_date >= %s and colo_date <= %s order by colo_date desc"
		
		adr = (startDate,endDate)
		mycursor.execute(sql,adr)
		colo = mycursor.fetchall()

		return colo

	def GetColoById(self, id):

		sql = "SELECT * FROM winlosedb where guild_id = %s order by colo_date desc"
		
		adr = (id,)
		mycursor.execute(sql,adr)
		colo = mycursor.fetchall()

		return colo

	def GetColoByName(self, guildName):

		sql = "SELECT * FROM winlosedb where guild_name = %s order by colo_date desc"
		
		adr = (guildName,)
		mycursor.execute(sql,adr)
		colo = mycursor.fetchall()

		return colo

	def CompareColoById(self, id,id2):

		sql = "SELECT * FROM winlosedb where guild_id = %s or guild_id = %s order by colo_date desc"
		
		adr = (id,id2)
		mycursor.execute(sql,adr)
		colo = mycursor.fetchall()

		return colo

	def GetBuffDebuffSummary(self, id):

		# sql = "SELECT 'total_fire_harptome',total_fire_weapons_patk_buff,total_fire_weapons_pdef_buff,total_fire_weapons_mdef_buff," \
		# "total_fire_weapons_buff, total_fire_weapons_patk_debuff, total_fire_weapons_pdef_debuff, total_fire_weapons_matk_debuff," \
		# "total_fire_weapons_mdef_debuff, total_fire_weapons_debuff," \
		# "total_water_harptome, total_water_weapons_patk_buff, total_water_weapons_pdef_buff, total_water_weapons_mdef_buff," \
		# "total_water_weapons_buff, total_water_weapons_patk_debuff, total_water_weapons_pdef_debuff, total_water_weapons_matk_debuff," \
		# "total_water_weapons_mdef_debuff, total_water_weapons_debuff," \
		# "total_wind_harptome, total_wind_weapons_patk_buff, total_wind_weapons_pdef_buff, total_wind_weapons_mdef_buff," \
		# "total_wind_weapons_buff, total_wind_weapons_patk_debuff, total_wind_weapons_pdef_debuff, total_wind_weapons_matk_debuff," \
		# "total_wind_weapons_mdef_debuff, total_wind_weapons_debuff," \
		# "total_fire_staves,total_fire_heal,total_water_staves,total_water_heal,total_wind_staves,total_wind_heal," \
		# "total_fire_vg_weapons,total_water_vg_weapons,total_wind_vg_weapons,total_bonus_combo_weapons" \
		# "FROM guildsdb where guild_id = %s"

		sql = "SELECT * FROM guildsdb where guild_id = %s"

		adr = (id,)
		mycursor.execute(sql,adr)
		colo = mycursor.fetchall()

		return colo








