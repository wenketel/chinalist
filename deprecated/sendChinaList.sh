#!/bin/bash
#
# A simple script help to commit Adblock Plus ChinaList easily
#
# Function:
#   Update "Last Modified" field automatically
#   Update "Checksum" field automatically
#   Update local svn repository automatically
#   Commit to remote svn repository automatically
# Usage:
#   sendChinaList commit log
################################################################

# you may want to change this to fit your situation
listdir=~/svn/abp-chinalist/trunk;

# due to introducing of checksum, it will always be conflicting if
# you work as a team using vcs. With this you needn't update your local
# repository manually every time.
svn update $listdir &&

# we have more than one list
for file in $listdir/*
do
    # avoid changing file properties every time.
    # note: we use a modified validateChecksum.pl
    ! $listdir/validateChecksum.pl -s $file &&
    sed -i s/"Last Modified:.*$"/"Last Modified:  `date -R -r $file`"/ $file &&
    $listdir/addChecksum.pl $file;
done

svn ci -m "$*" $listdir;

# you can simply run this script, or throw it to your ~/.bash_alias like:
#
# sendChinalist()
# {
#     listdir=~/svn/abp-chinalist/trunk;
#     ......
#     ......
#     svn ci -m "$*" $listdir;
# }
### End ###
