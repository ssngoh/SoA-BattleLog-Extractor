# bot.py
import os
import random
import discord

from dotenv import load_dotenv
from dbinsert import dbinsertImporter
from discord.ext import commands

load_dotenv()
TOKEN = os.getenv('DISCORD_TOKEN')
GUILD = os.getenv('DISCORD_GUILD')

# bot = commands.Bot(command_prefix='!')

class CustomClient(discord.Client):
	async def on_ready(self):
		print(f'{self.user} has connected to Discord!')
		guild = discord.utils.get(client.guilds, name=GUILD)

		print(
		f'{client.user} is connected to the following guild:\n'
		f'{guild.name}(id: {guild.id})'
		)


	async def on_member_join(self, member):
		await member.create_dm()
		await member.dm_channel.send(f'Hi {member.name}, welcome to Stellar, please fill in your loadout list here.')
		await member.dm_channel.send('https://docs.google.com/spreadsheets/d/1mXV9jDz5dhTI23nFyXG8X-xfv6do0lg9vry3_GmU1uc/edit#gid=165952375')

	async def on_error(event, *args, **kwargs):
		with open('err.log', 'a') as f:
			if event =='on_message':
				f.write(f'Unhandled message: {args[0]}\n')
			else:
				raise


	async def on_message(self, message):
		if message.author == client.user: #Make sure the bot doesn't listen to its own messages
			return
		response = dbinsertImporterObj.SelectAllWinLose()
		await message.channel.send(response)


dbinsertImporterObj = dbinsertImporter()
client = CustomClient()
client.run(TOKEN)






