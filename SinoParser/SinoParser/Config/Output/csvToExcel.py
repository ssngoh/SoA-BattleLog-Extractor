import pyexcel

def ResetRelationValues():
	patkIncreaseGiven=0
	pdefIncreaseGiven=0
	matkIncreaseGiven=0
	mdefIncreaseGiven=0

	patkDecreaseGiven=0
	pdefDecreaseGiven=0
	matkDecreaseGiven=0
	mdefDecreaseGiven=0

	healingGiven=0

	patkIncreaseReceived=0
	pdefIncreaseReceived=0
	matkIncreaseReceived=0
	mdefIncreaseReceived=0

	patkDecreaseReceived=0
	pdefDecreaseReceived=0
	matkDecreaseReceived=0
	mdefDecreaseReceived=0

	healingReceived=0




patkIncreaseGiven=0
pdefIncreaseGiven=0
matkIncreaseGiven=0
mdefIncreaseGiven=0

patkDecreaseGiven=0
pdefDecreaseGiven=0
matkDecreaseGiven=0
mdefDecreaseGiven=0

healingGiven=0

patkIncreaseReceived=0
pdefIncreaseReceived=0
matkIncreaseReceived=0
mdefIncreaseReceived=0

patkDecreaseReceived=0
pdefDecreaseReceived=0
matkDecreaseReceived=0
mdefDecreaseReceived=0

healingReceived=0

content = {}
newRow = False
name= None
array= {}

csvToRead = pyexcel.get_sheet(file_name='relationshipTable.csv')


#Note that excel sheet naming is not case sensitive. 
#If you have a sheet named A, you cannot have another named a
#So make sure you check the relationshipCorrectionTable.txt if you get a duplicate worksheet name error.
	#if there are really 2 names that differ by case, then you'll need to go to the friend/enemy NameSelfCorrectingConfig.txt to change them

for row in csvToRead:
	print(row)
	if "**" in str(row[0]):
		name = row[0]
		name = name.replace("*","")
		name = name.replace("\\","")
		name = name.replace("[","")
		name = name.replace("]","")
		name = name.replace("?","")
		name = name.replace(":","")
		name = name.replace("/","")

		content[name] = [["","PATK Increase Given", "PDEF Increase Given", "MATK Increase Given", "MDEF Increase Given",
						     "PATK Decrease Given", "PDEF Decrease Given", "MATK Decrease Given", "MDEF Decrease Given",
						     "Healing Given", 
						     "PATK Increase Received", "PDEF Increase Received", "MATK Increase Received", "MDEF Increase Received",
						     "PATK Decrease Received", "PDEF Decrease Received", "MATK Decrease Received", "MDEF Decrease Received",
						     "Healing Received"]]

		ResetRelationValues()
		continue

	array = [ [row[0], row[1], row[2], row[3], row[4], row[5], row[6], row[7], row[8], row[9], row[10], 
			   row[11],row[12], row[13], row[14], row[15], row[16], row[17], row[18] ] ]
	content[name] += array
	

print(content)
book = pyexcel.get_book(bookdict=content)
book.save_as("output.xls")

