#! /usr/bin/env python
#coding=utf-8
#version=1.0

#############################################################################
# This is a reference script to add checksums to downloadable               #
# subscriptions. The checksum will be validated by Adblock Plus on download #
# and checksum mismatches (broken downloads) will be rejected.              #
#                                                                           #
# To add a checksum to a subscription file, run the script like this:       #
#                                                                           #
#   python addChecksum.py adblock.txt  					                    #
#                                                                           #
# Note: your subscription file should be saved in UTF-8 encoding, otherwise #
# the generated checksum might be incorrect.                                #
#                                                                           #
# This script is wrote by Gythialy for ChinaList Project                    #
#############################################################################

import os
import codecs
import hashlib
import base64
import re
import string
import sys

def read(filename):
	path = os.path.join(os.getcwd(),filename)
	f =  codecs.open(filename,'rt',encoding = 'utf-8')
	data = f.read()
	f.close()
	return data

def save(content,filename):
	path = os.path.join(os.getcwd(),filename)
	f =  codecs.open(filename,'w',encoding = 'utf-8')
	f.write(content)
	f.close()

def calculatchecksum(content):
	content = re.sub(r"\r\n","\n",content)
	content = re.sub(r"\n+","\n",content)
	m = hashlib.md5()
	m.update(content)
	validate = base64.b64encode(m.digest())
	return re.sub(r"=+$","",validate)

def insert(original, new):
	'''Inserts new inside original at pos.'''
	pos = original.index(']') + 2
	return original[:pos] + new + original[pos:]

def removechecksum(content):
	prog = re.compile(r"\s*!\s*checksum[\s\-:]+([\w\+\/=]+).*\n",re.I)
	match =  prog.search(content)
	if match:
		temp = match.group().strip()
		content = string.replace(content,temp,"")

	return content

if __name__ == '__main__':
	reload(sys)
	sys.setdefaultencoding("utf-8")

	if len(sys.argv) != 2:
		print 'Pls input file names.'
		sys.exit(0)
	filename = sys.argv[1]

	content = read(filename)
	content = removechecksum(content)

	checksum = '!  Checksum: {0}'.format(calculatchecksum(content))
	content = insert(content,checksum)
	save(content,filename)