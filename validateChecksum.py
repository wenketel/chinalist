#! /usr/bin/env python
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

def read(filename):
    path = os.path.join(os.getcwd(),filename)
    f =  codecs.open(filename,'rt',encoding = 'utf-8')
    data = f.read()
    f.close()
    return data

def calculatchecksum(content):
    content = re.sub(r"\r\n","\n",content)
    content = re.sub(r"\n+","\n",content)
    m = hashlib.md5()
    m.update(content)
    validate = base64.b64encode(m.digest())
    	
    return re.sub(r"=+$","",validate)

def validate(filename):
    path = os.path.join(os.getcwd(),filename)
    if not os.path.exists(path):
        print  filename + 'is not exist.'
    data = read(filename)
    prog = re.compile(r"\s*!\s*checksum[\s\-:]+([\w\+\/=]+).*\n",re.I)
    match =  prog.search(data)
    checksum = ''
    if not match:
        print 'Could not find a checksum in the file {0}'.format(filename)
        sys.exit(0)
    else:
        temp = match.group().strip()
        checksum = temp.split(':')[1].strip()
        data = string.replace(data,temp,"")

    validate = calculatchecksum(data)
    if validate == checksum:
        print filename + " 's checksum is valid."
    else:
        print 'Wrong checksum: found {0}, expected {1}'.format(checksum,validate)


if __name__ == '__main__':
    reload(sys)
    sys.setdefaultencoding( "utf-8" )

    filename = ''
    if len(sys.argv) != 2:
        print 'Pls input validate file name.'
        sys.exit(0)
    else:
        filename = sys.argv[1]
        validate(filename)
        sys.exit(1)