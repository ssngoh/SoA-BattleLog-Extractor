#!/usr/bin/env python
# coding: utf-8

#Taken from https://qatesttech.wordpress.com/2012/06/21/python-script-to-convert-hexadecimal-to-decimal-for-sendevent/

# From hex_to_dec.py (which I created a while ago)

# Tweaking for Mac
#
# Fixed indenting that wordpress was breaking
#
# You need python installed – more info here:
# Python – http://www.python.org/download/
# The Purpose of this script
#
# Script name: hex_to_dec.py
#
# To turn the getevent output from this:
# /dev/input/event0: 0003 0001 00000092
# to this (something can be run)
# sendevent /dev/input/event0 3 1 92
# Outfile of what to do
# List input file on command line when running
# Open file
# prefix “adb shell sendevent” command to line
# remove colon from input
# convert 3 hexidecimal values to decimal values
# Assemble new complete line
# write out line to new file,
# named the original filename +suffix “bat” – windows batch file ready to run
#

# Importing Stuff
import sys
import fileinput
###################################################################################################
#
# Variables
#

# Command prefix
prefix = "adb.exe shell sendevent "

inputline = ""
complete = ""
part1len = 0
part1 = ""
part2 = ""

num1 = 0
num2 = 0
num3 = 0

# File stuff
rawfile = ""
outfile = ""
filename = ""
###################################################################################################
#
# File operations
#

# Grabs the file name from the command line when you run the python script
rawfile = sys.argv[-1]

# Open the input file
# r – Opens a file for reading only. The file pointer is placed at the beginning of the file. This is the default mode.
# w – Opens a file for writing only. Overwrites the file if the file exists. If the file does not exist, creates a new file for writing.
#
fo = open(rawfile, "r")

# Create output file, taking everything in front of the . (kind of windows centric)
filename = rawfile.find(".");
outfile = rawfile[:filename] + ".scr"

# Output file
fw = open(outfile, "w")

# Stuff at the top of the script (first commands to run)
# Puts into bash
fw.write("#!/bin/sh" + "\n")

# User Message
fw.write("echo Running – drawing function " + "\n")

###################################################################################################
#
# Process input string, creating new formatted command
#

for inputline in fo.read().split("\n"):
        # Used for testing/debug
	#print inputline

	# Otherwise let"s process the line
	# Find the location of the colon
	part1len = inputline.find(":");

	# -1 means end of file so quit processing
	if part1len > -1:
		# Only take the first part of the string up to the string
		part1 = inputline[:part1len]

		# Finding the hexidecimal values
		# Splits the string based on spaces, creating an array with 4 strings,
		part2 = inputline.split(" ")

		# we want strings 1,2,3 (since arrays start at 0 (zero))
		# we need to specify the base explicitly (based 16, so hex gets converted to decimal)
		num1 = int(part2[1], 16)
		num2 = int(part2[2], 16)
		num3 = int(part2[3], 16)

		# Put is all together
		complete = prefix + part1 + " " + str(num1) + " " + str(num2) + " " + str(num3)

		# Write out to the file, with a new line feed
		fw.write(complete + "\n")

# For testing
#print complete

# Info for the user
# print(Processing complete)
# print(File created:, outfile)
# print
# print "Copy file to the device"
# print "adb push " + outfile + " /sdcard/" + outfile
# print
# print "Run the script"
# print "adb shell sh /sdcard/" + outfile
###################################################################################################
#
# File clean up
#

# Close all files
fo.close()
fw.close()