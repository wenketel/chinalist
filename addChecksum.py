#! /usr/bin/env python 2.6.6
#coding=utf-8
#version=1.0

#############################################################################
# This is a reference script to add checksums to downloadable               #
# subscriptions. The checksum will be validated by Adblock Plus on download #
# and checksum mismatches (broken downloads) will be rejected.              #
#                                                                           #
# To add a checksum to a subscription file, run the script like this:       #
#                                                                           #
#   python addChecksum.py adblock.txt adblock-lazy.txt                      #
#                                                                           #
# Note: your subscription file should be saved in UTF-8 encoding, otherwise #
# the generated checksum might be incorrect.                                #
#                                                                           #
# This script is wrote by Gythialy for ChinaList Project                    #
#############################################################################

import os
import time
import codecs
import hashlib
import base64
import re
import string
import sys
from urllib import urlretrieve

def download(url,filename):
	urlretrieve(url,filename)

def fileexist(filename):
	path = os.path.join(os.getcwd(),filename)
	if os.path.exists(path):
		fileinfo = os.stat(path)
		current =  time.strftime('%Y%m%d',time.localtime(time.time()))
		filetime = time.strftime('%Y%m%d',time.localtime(fileinfo.st_mtime))
		if current == filetime:
			return {"success":True,"reasion":'well done'}
		else:
			return {"success":False,"reason":'file is too old'}
	else:
		return {"success":False,"reason":'file is not exist'}

def updatelist(url,filename):
	result = fileexist(easylist)
	if not result["success"]:
		print '{0} , start update {1}'.format(result["reason"],filename)
		download(url,filename)
		print 'End of update ' + filename

def read(filename):
	path = os.path.join(os.getcwd(),filename)
	f =  codecs.open(filename,'rt',encoding = 'utf-8')
	data = f.read()
	f.close()
	return data

def currenttime():
	#Mon, 13 Dec 2010 10:40:58 +0800
	return time.strftime('%a, %d %b %Y %X ',time.localtime(time.time())) + '+0800'

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

def updatetime(content):
	prog = re.compile(r"Last Modified:.*$",re.MULTILINE)
	match =  prog.search(content)
	if match:
		temp = match.group().strip()
		content = string.replace(content,temp,'Last Modified:  ' + currenttime())

	return content

def validate(filename):
	path = os.path.join(os.getcwd(),filename)
	if not os.path.exists(path):
		print  filename + 'is not exist.'
	f =  codecs.open(filename,'rt',encoding = 'utf-8')
	data = f.read()
	f.close()
	prog = re.compile(r"\s*!\s*checksum[\s\-:]+([\w\+\/=]+).*\n",re.I)
	match =  prog.search(data)
	checksum = ''
	if not match:
		print 'Could not find a checksum in the file {0}'.format(filename)
	else:
		temp = match.group().strip()
		checksum = temp.split(':')[1].strip()
		data = string.replace(data,temp,"")
	data = re.sub(r"\r\n","\n",data)
	data = re.sub(r"\n+","\n",data)
	'''generate checksum'''
	m = hashlib.md5()
	m.update(data)
	validate = base64.b64encode(m.digest())
	validate = re.sub(r"=+$","",validate)
	if validate == checksum:
		print filename + " 's checksum is valid."
	else:
		print 'Wrong checksum: found {0}, expected {1}'.format(checksum,validate)

if __name__ == '__main__':
	reload(sys)
	sys.setdefaultencoding( "utf-8" )
	chinalist = ''
	chinalazy = ''
	chinalistcontent = ''
	chinalazycontent = ''
	if len(sys.argv) != 3:
		print 'Pls input file names.'
		sys.exit(0)
	chinalist = sys.argv[1]
	chinalazy = sys.argv[2]

	easylist = 'easylist.txt'
	easylisturl = 'https://easylist-downloads.adblockplus.org/easylist.txt'
	easyprivacy = 'easyprivacy.txt'
	easyprivacyurl = 'https://easylist-downloads.adblockplus.org/easyprivacy.txt'
	updatelist(easylisturl,easylist)
	updatelist(easyprivacyurl,easyprivacy)
	chinalistcontent = read(chinalist)
	chinalistcontent = updatetime(chinalistcontent)
	chinalistcontent = removechecksum(chinalistcontent)
	chinalazycontent = chinalistcontent
	easylistcontent = read(easylist)
	index = string.index(easylistcontent,'!-----------------General advert blocking filters-----------------!')
	easylistcontent = easylistcontent[index:len(easylistcontent)]
	#print easylistcontent
	easyprivacycontent = read(easyprivacy)
	index= string.index(easyprivacycontent,'!-----------------General tracking systems-----------------!')
	easyprivacycontent = easyprivacycontent[index:len(easyprivacycontent)]
	#print easyprivacycontent

	checksum = '!  Checksum: {0}'.format(calculatchecksum(chinalistcontent))
	chinalistcontent = insert(chinalistcontent,checksum)
	save(chinalistcontent,chinalist)

	chinalazycontent += (easylistcontent + easyprivacycontent)
	checksum = '!  Checksum: {0}'.format(calculatchecksum(chinalazycontent))
	chinalazycontent = insert(chinalazycontent,checksum)
	save(chinalazycontent,chinalazy)
	
	#validate
	validate(chinalist)
	validate(chinalazy)