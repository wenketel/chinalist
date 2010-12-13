#! /usr/bin/env python 2.6.6
#coding=utf-8
#version=1.0

#############################################################################
# This is a reference script to validate the checksum in downloadable       #
# subscription. This performs the same validation as Adblock Plus when it   #
# downloads the subscription.                                               #
#                                                                           #
# To validate a subscription file, run the script like this:                #
#                                                                           #
#   python validateChecksum.py adblock.txt                                  #
#                                                                           #
# Note: your subscription file should be saved in UTF-8 encoding, otherwise #
# the validation result might be incorrect.                                 #
#                                                                           #
# This script is wrote by Gythialy for ChinaList Project                    #
#############################################################################

import base64
import codecs
import hashlib
import os
import re
import sys
import string

if __name__ == '__main__':
	reload(sys)
	sys.setdefaultencoding( "utf-8" )
	#print sys.getdefaultencoding()
	filename = ''
	if len(sys.argv) != 2:
		print 'Pls input validate file name.'
		sys.exit(0)
	else:
		filename = sys.argv[1]
		path = os.path.join(os.getcwd(),filename)
		#print path
		if not os.path.exists(path):
			print  filename + 'is not exist.'
			sys.exit(0)
		f =  codecs.open(filename,'rt',encoding = 'utf-8')
		#data = [line.strip for line in f.readlines()]
		data = f.read()
		f.close()
		#print data
		prog = re.compile(r"\s*!\s*checksum[\s\-:]+([\w\+\/=]+).*\n",re.I)
		match =  prog.search(data)
		checksum = ''
		if not match:
			print 'Could not find a checksum in the file {0}'.format(filename)
			sys.exit(0)
		else:
			'''get checksum'''
			temp = match.group().strip()
			#print temp
			checksum = temp.split(':')[1].strip()
			#print checksum
			data = string.replace(data,temp,"")
		data = re.sub(r"\r\n","\n",data)
		data = re.sub(r"\n+","\n",data)
		#print data
		'''generate checksum'''
		m = hashlib.md5()
		m.update(data)
		validate = base64.b64encode(m.digest())
		validate = re.sub(r"=+$","",validate)
		#print validate
		if validate == checksum:
			print 'Checksum is valid.'
			sys.exit(0)
		else:
			print 'Wrong checksum: found {0}, expected {1}'.format(checksum,validate)
			sys.exit(1)