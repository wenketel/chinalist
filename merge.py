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
#   python merge.py adblock.txt adblock-lazy.txt                      		#
#   or  python merge.py adblock-lazy.txt                      				#
#                                                                           #
# Note: your subscription file should be saved in UTF-8 encoding, otherwise #
# the generated checksum might be incorrect.                                #
#                                                                           #
# This script is wrote by Gythialy for ChinaList Project                    #
#############################################################################

import codecs
import os
import string
import sys
import time
from urllib import urlretrieve

EASYLIST = 'easylist.txt'
EASYLIST_URL = 'https://easylist-downloads.adblockplus.org/easylist.txt'
EASYLIST_HEAD = '\n! EasyList - https://easylist.adblockplus.org/\n\n'
EASYLIST_MARK = '!-----------------General advert blocking filters-----------------!'
EASYPRIVACY = 'easyprivacy.txt'
EASYPRIVACY_URL = 'https://easylist-downloads.adblockplus.org/easyprivacy.txt'
EASYPRIVACY_HEAD = '\n! EasyPrivacy - https://easylist.adblockplus.org/\n\n'
EASYPRIVACY_MARK = '!-----------------General tracking systems-----------------!'

def download(url, filename):
    urlretrieve(url, filename)

def fileexist(filename):
    path = os.path.join(os.getcwd(), filename)
    if os.path.exists(path):
        fileinfo = os.stat(path)
        current = time.strftime('%Y%m%d', time.localtime(time.time()))
        filetime = time.strftime('%Y%m%d', time.localtime(fileinfo.st_mtime))
        if current == filetime:
            return {"success":True, "reason":'nice'}
        else:
            return {"success":False, "reason":'file is out of date'}
    else:
        return {"success":False, "reason":'file is not exist'}

def updatelist(url, filename):
    result = fileexist(filename)
    if not result["success"]:
        print '{0}, {1} to start the update.'.format(result["reason"], filename)
        download(url, filename)
        print 'Update {0} completed'.format(filename)

def read(filename):
    path = os.path.join(os.getcwd(), filename)
    f = codecs.open(path, 'rt', encoding='utf-8')
    data = f.read()
    f.close()

    return data

def save(content, filename):
    path = os.path.join(os.getcwd(), filename)
    f = codecs.open(path, 'w', encoding='utf-8')
    f.write(content)
    f.close()

def merge(chinalazy, chinalist='adblock.txt'):
    print 'Began the merge.'
    updatelist(EASYLIST_URL, EASYLIST)
    updatelist(EASYPRIVACY_URL, EASYPRIVACY)
    chinalazycontent = read(chinalist)
    easylistcontent = read(EASYLIST)
    index = string.index(easylistcontent, EASYLIST_MARK)
    easylistcontent = EASYLIST_HEAD + easylistcontent[index:]
    easyprivacycontent = read(EASYPRIVACY)
    index = string.index(easyprivacycontent, EASYPRIVACY_MARK)
    easyprivacycontent = EASYPRIVACY_HEAD + easyprivacycontent[index:]

    chinalazycontent += (easylistcontent + easyprivacycontent)
    save(chinalazycontent, chinalazy)
    print 'End of the merger'

if __name__ == '__main__':
    chinalist = ''
    chinalazy = ''

    if len(sys.argv) == 2:
        chinalazy = sys.argv[1]
        merge(chinalazy)
    elif len(sys.argv) == 3:
        chinalist = sys.argv[1]
        chinalazy = sys.argv[2]
        merge(chinalazy, chinalazy)
    else:
        print 'Pls input file names.'
        sys.exit(0)
