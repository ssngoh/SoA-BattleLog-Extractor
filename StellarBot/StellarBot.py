# bot.py
import os
import random
import discord
import datetime

from dotenv import load_dotenv
from dbinsert import dbinsertImporter
from discord.ext import commands

import GetStatsQuery
import GetColoQuery

load_dotenv()
TOKEN = os.getenv('DISCORD_TOKEN')
GUILD = os.getenv('DISCORD_GUILD')

bot = commands.Bot(command_prefix='!stellarbot ', help_command=None)


helpStr=[]
helpStr.append("""***This bot is used to grab colo stats***

***COMMANDS (End with pm to have the bot pm you instead of showing to everyone. Works for ALL commands)***
***Use !stellarbot help_pm to pm you this message!***
__Displays most recent 3 (MAX 5) colo stats for this user__
```
!stellarbot get_stats_by_name <username> <optional days (cap at 5)>
!stellarbot get_stats_by_id <userid> <optional days (cap at 5)>
!stellarbot get_stats_by_name_date <userid> <start date> <end date>
!stellarbot get_stats_by_id_date <userid> <start date> <end date>
!stellarbot get_stats_by_guild_id <guild id> (This can be gotten by get_colo_short)

Eg. !stellarbot get_stats_by_name Ibis
Eg. !stellarbot get_stats_by_name Ibis 5
Eg. !stellarbot get_stats_by_id 214304673
Eg. !stellarbot get_stats_by_name_date Ibis 2020-09-20 2020-09-22
Eg. !stellarbot get_stats_by_id_date 214304673 2020-08-23 2020-09-10

To pm yourself, just add _pm to the end. Works for all commands
Eg. !stellarbot get_stats_by_name_pm Ibis
Eg. !stellarbot get_stats_by_name_date_pm Ibis 2020-09-20 2020-09-22
```
__Displays past 5 days (MAX 30) summary of colo details from start to end date__
```
!stellarbot get_colo_short <optional days (cap at 30)>
!stellarbot get_colo_short_date <start date> <end date>

Eg. !stellarbot get_colo_short
Eg. !stellarbot get_colo_short 16
Eg. !stellarbot get_colo_short_by_date 2020-09-20 2020-09-25

To pm yourself, just add _pm to the end. Works for all commands
Eg. !stellarbot get_colo_short_pm
Eg. !stellarbot get_colo_short_by_date_pm 2020-08-21 2020-09-26

````""")

helpStr.append("""
__Displays past 3 days (MAX 5) days of colo details from start to end date__
```
!stellarbot get_colo 
!stellarbot get_colo_by_date <start date> <end date>
!stellarbot get_colo_by_id 1 (The id refers to the guild id)
!stellarbot get_colo_by_name (The name refers to the guild name. Won't be there for KR guilds)

Eg. !stellarbot get_colo
Eg. !stellarbot get_colo 2
Eg. !stellarbot get_colo_by_date 2020-09-20 2020-09-25
Eg. !stellarbot get_colo_by_id 4
Eg. !stellarbot get_colo_by_name TeaCats

To pm yourself, just add _pm to the end. Works for all commands
Eg. !stellarbot get_colo_pm
Eg. !stellarbot get_colo_by_id_pm 4
```
__Compares 2 colo results__ (TBA)
```
!stellarbot compare_colo_by_date <colo date 1> <colo date 2> 
!stellarbot compare_colo_by_id <guild id 1> <guild id 2>

Eg. !stellarbot compare_colo_by_date 2020-09-20 2020-09-25
Eg. !stellarbot compare_colo_by_id 4 50
```
""")

@bot.command(name='help', help='shows list of commands')
async def help(ctx):

	for s in helpStr:
		await ctx.send(s)

@bot.command(name='help_pm', help='shows list of commands')
async def help(ctx):

	await ctx.author.create_dm()
	for s in helpStr:
		await ctx.author.dm_channel.send(s)

#################################################GET STATS###################################################
@bot.command(name='get_stats_by_name')
async def GetStatsByName(ctx, playerName, limit=3):

	display_str = GetStatsQuery.GetStatsByName(dbinsertImporterObj,playerName,limit)
	await ctx.send(display_str)

@bot.command(name='get_stats_by_name_pm')
async def GetStatsByNamePM(ctx, playerName, limit=3):

	display_str = GetStatsQuery.GetStatsByName(dbinsertImporterObj,playerName,limit)
	await ctx.author.create_dm()
	await ctx.author.dm_channel.send(display_str)

@bot.command(name='get_stats_by_name_date')
async def GetStatsByNameAndDate(ctx, playerName, startDate, endDate):

	display_str = GetStatsQuery.GetStatsByNameAndDate(dbinsertImporterObj,playerName,startDate,endDate)
	await ctx.send(display_str)

@bot.command(name='get_stats_by_name_date_pm')
async def GetStatsByNameAndDatePM(ctx, playerName, startDate, endDate):

	display_str = GetStatsQuery.GetStatsByNameAndDate(dbinsertImporterObj,playerName,startDate,endDate)
	await ctx.author.create_dm()
	await ctx.author.dm_channel.send(display_str)

@bot.command(name='get_stats_by_id')
async def GetStatsById(ctx, id, limit=3):

	display_str = GetStatsQuery.GetStatsById(dbinsertImporterObj,id,limit)
	await ctx.send(display_str)

@bot.command(name='get_stats_by_id_pm')
async def GetStatsByIdPM(ctx, id, limit=3):

	display_str = GetStatsQuery.GetStatsById(dbinsertImporterObj,id,limit)
	await ctx.author.create_dm()
	await ctx.author.dm_channel.send(display_str)

@bot.command(name='get_stats_by_id_date')
async def GetStatsByIdAndDate(ctx, id, startDate, endDate):

	display_str = GetStatsQuery.GetStatsByIdAndDate(dbinsertImporterObj,id,startDate,endDate)
	await ctx.send(display_str)

@bot.command(name='get_stats_by_id_date_pm')
async def GetStatsByIdAndDatePM(ctx, id, startDate, endDate):

	display_str = GetStatsQuery.GetStatsByIdAndDate(dbinsertImporterObj,id,startDate,endDate)
	await ctx.author.create_dm()
	await ctx.author.dm_channel.send(display_str)

@bot.command(name='get_stats_by_guild_id')
async def GetStatsByGuildId(ctx, guildId):

	display_str = GetStatsQuery.GetStatsByGuildId(dbinsertImporterObj,guildId)
	for txt in display_str:
		await ctx.send(txt)

@bot.command(name='get_stats_by_guild_id_pm')
async def GetStatsByGuildId(ctx, guildId):

	display_str = GetStatsQuery.GetStatsByGuildId(dbinsertImporterObj,guildId)
	
	await ctx.author.create_dm()
	for txt in display_str:
		await ctx.author.dm_channel.send(txt)

##################################################END GET STATS###################################################

@bot.command(name='get_colo_short')
async def GetColoShort(ctx,limit=5):

	display_str = GetColoQuery.GetColoDetailsShort(dbinsertImporterObj,limit)

	for txt in display_str:
		await ctx.send(txt)

@bot.command(name='get_colo_short_pm')
async def GetColoShortPM(ctx,limit=5):

	display_str = GetColoQuery.GetColoDetailsShort(dbinsertImporterObj,limit)
	await ctx.author.create_dm()
	for txt in display_str:
		await ctx.author.dm_channel.send(txt)

@bot.command(name='get_colo_short_by_date')
async def GetColoShortByDate(ctx, startDate, endDate):

	display_str = GetColoQuery.GetColoDetailsShortByDate(dbinsertImporterObj,startDate,endDate)
	for txt in display_str:
		await ctx.send(txt)

@bot.command(name='get_colo_short_by_date_pm')
async def GetColoShortByDatePM(ctx, startDate, endDate):

	display_str = GetColoQuery.GetColoDetailsShortByDate(dbinsertImporterObj,startDate,endDate)
	await ctx.author.create_dm()

	for txt in display_str:
		await ctx.author.dm_channel.send(txt)

@bot.command(name='get_colo')
async def GetColo(ctx,limit=3):

	display_str, nightmare_display_str = GetColoQuery.GetColoDetails(dbinsertImporterObj,limit)

	for txt in display_str:
		await ctx.send(txt)

	for txt in nightmare_display_str:
		await ctx.send(txt)

@bot.command(name='get_colo_pm')
async def GetColoPM(ctx,limit=3):

	display_str, nightmare_display_str = GetColoQuery.GetColoDetails(dbinsertImporterObj,limit)
	await ctx.author.create_dm()

	for txt in display_str:
		await ctx.author.dm_channel.send(txt)

	for txt in nightmare_display_str:
		await ctx.author.dm_channel.send(txt)

@bot.command(name='get_colo_by_date')
async def GetColoShortByDate(ctx, startDate, endDate):

	display_str, nightmare_display_str = GetColoQuery.GetColoDetailsByDate(dbinsertImporterObj,startDate,endDate)
	
	for txt in display_str:
		await ctx.send(txt)

	for txt in nightmare_display_str:
		await ctx.send(txt)

@bot.command(name='get_colo_by_date_pm')
async def GetColoShortByDatePM(ctx, startDate, endDate):

	display_str, nightmare_display_str = GetColoQuery.GetColoDetailsByDate(dbinsertImporterObj,startDate,endDate)
	await ctx.author.create_dm()

	for txt in display_str:
		await ctx.author.dm_channel.send(txt)

	for txt in nightmare_display_str:
		await ctx.author.dm_channel.send(txt)

@bot.command(name='get_colo_by_id')
async def GetColoById(ctx,id):

	display_str, nightmare_display_str = GetColoQuery.GetColoDetailsById(dbinsertImporterObj,id)

	for txt in display_str:
		await ctx.send(txt)

	for txt in nightmare_display_str:
		await ctx.send(txt)

@bot.command(name='get_colo_by_id_pm')
async def GetColoByIdPM(ctx,id):

	display_str, nightmare_display_str = GetColoQuery.GetColoDetailsById(dbinsertImporterObj,id)
	await ctx.author.create_dm()

	for txt in display_str:
		await ctx.author.dm_channel.send(txt)

	for txt in nightmare_display_str:
		await ctx.author.dm_channel.send(txt)

@bot.command(name='get_colo_by_name')
async def GetColoByName(ctx,guildName):

	display_str, nightmare_display_str = GetColoQuery.GetColoDetailsByName(dbinsertImporterObj,guildName)

	for txt in display_str:
		await ctx.send(txt)

	for txt in nightmare_display_str:
		await ctx.send(txt)

@bot.command(name='get_colo_by_name_pm')
async def GetColoByNamePM(ctx,guildName):

	display_str, nightmare_display_str = GetColoQuery.GetColoDetailsByName(dbinsertImporterObj,guildName)
	await ctx.author.create_dm()

	for txt in display_str:
		await ctx.author.dm_channel.send(txt)

	for txt in nightmare_display_str:
		await ctx.author.dm_channel.send(txt)

@bot.command(name='get_colo_buff_debuff_by_id')
async def GetColoBuffDebuffById(ctx,id):

	display_str = GetColoQuery.GetBuffDebuffSummary(dbinsertImporterObj,id)

	for txt in display_str:
		await ctx.send(txt)


@bot.command(name='compare_colo_by_id')
async def CompareColoById(ctx,id,id2):

	display_str, nightmare_display_str = GetColoQuery.CompareColoDetailsById(dbinsertImporterObj,id,id2)
	for txt in display_str:
		await ctx.send(txt)

	for txt in nightmare_display_str:
		await ctx.send(txt)

@bot.command(name='compare_colo_by_id_pm')
async def CompareColoByIdPM(ctx,id,id2):

	display_str, nightmare_display_str = GetColoQuery.CompareColoDetailsById(dbinsertImporterObj,id,id2)
	await ctx.author.create_dm()

	for txt in display_str:
		await ctx.author.dm_channel.send(txt)

	for txt in nightmare_display_str:
		await ctx.author.dm_channel.send(txt)

@bot.command(name='who_is_your_best_friend')
async def bestFriend(ctx):

	await ctx.send("Arco is my best friend")

@bot.command(name='who_sucks_the_most')
async def SucksTheMost(ctx):

	await ctx.send("Arco sucks the most")


dbinsertImporterObj = dbinsertImporter()
bot.run(TOKEN)






