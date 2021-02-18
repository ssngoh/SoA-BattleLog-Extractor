#! python
import xlrd
from quickstart import downloadLoadoutList
from quickstart import downloadWeaponList


def ReadDB(isFriendly):

	START_ROW = 1

	FIELD_PLAYER_ID = 0
	FIELD_PLAYER_NAME = 1
	FIELD_PATK = 4
	FIELD_PDEF = 5
	FIELD_MATK = 6
	FIELD_MDEF = 7
	FIELD_ISVG = 17
	


	if isFriendly:
		fileName = "friendInitialStatsConfig.txt"
		sheet = wb.sheet_by_index(5)
	else:
		fileName = "enemyInitialStatsConfig.txt"
		sheet = wb.sheet_by_index(6)

	with open(fileName, "w") as openedFile:
		for rows in range(START_ROW,sheet.nrows):
			if sheet.cell_value(rows,FIELD_PLAYER_ID):
				player_id = int(sheet.cell_value(rows,FIELD_PLAYER_ID))
				player_name = sheet.cell_value(rows,FIELD_PLAYER_NAME)
				patk = int(sheet.cell_value(rows,FIELD_PATK))
				pdef = int(sheet.cell_value(rows,FIELD_PDEF))
				matk = int(sheet.cell_value(rows,FIELD_MATK))
				mdef = int(sheet.cell_value(rows,FIELD_MDEF))

				if sheet.cell_value(rows,FIELD_ISVG):
					is_vg = int(sheet.cell_value(rows,FIELD_ISVG))
				else:
					is_vg = 0

				openedFile.write("{0},{1},{2},{3},{4},{5},{6}\n".format(player_id,player_name,patk,pdef,matk,mdef,is_vg))

			



		

loc = ("LoadoutList.xlsx")
downloadLoadoutList()
downloadWeaponList()

wb = xlrd.open_workbook(loc)
ReadDB(1)
ReadDB(0)


