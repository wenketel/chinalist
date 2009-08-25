@echo off
set fileName=%1
ABPUtils.exe %fileName%
if %errorlevel%==0 (addChecksum.pl %fileName%)

validateChecksum.pl %fileName%

@pause