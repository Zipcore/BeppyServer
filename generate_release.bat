@echo off
REM Eventually going to make this a proper script, just wanted to rename the files for now.

del .\bin\Release\BeppyServer.Win32.dll
del .\bin\Release\BeppyServer.Linux.dll

copy .\bin\Win32\BeppyServer.dll .\bin\Release\BeppyServer.Win32.dll
copy .\bin\Linux\BeppyServer.dll .\bin\Release\BeppyServer.Linux.dll