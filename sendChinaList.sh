#!/bin/bash
#
# A simple script help to commit Adblock Plus ChinaList easily
#
# Function:
#   Update "Last Modified" field automatically
#   Update "Checksum" field automatically
#   Commit to svn repository automatically
# Usage:
#   sendChinaList commit log
################################################################

# you may want to change this to fit your situation
listdir=~/svn/abp-chinalist/trunk;

for file in $listdir/*
do
    filename=`echo $file | sed s/"\/.*\/"//`;
    if [ $filename == "sendChinaList.sh" ]; then
	continue;
    fi
    if [ $filename == "addChecksum.pl" ]; then
	continue;
    fi
    sed -i s/"Last Modified:.*$"/"Last Modified:  `date -R -r $file`"/ $file;
    $listdir/addChecksum.pl $file;
done

svn ci -m "$*";

# you can simply run this script, or throw it to your ~/.bash_alias like:
#
# sendChinalist()
# {
#     listdir=~/svn/abp-chinalist/trunk;
#     ......
#     ......
#     svn ci -m "$*";
# }
### End ###