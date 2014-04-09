@ECHO off
Title=Yanitta Project
color 1A

ECHO Clenuaps...

RD /Q /S .\Yanitta\obj
RD /Q /S .\Yanitta.Hantchk\obj
RD /Q /S .\ICSharpCode.AvalonEdit\obj
RD /Q /S .\ICSharpCode.AvalonEdit\bin

DEL /F /Q /S /A:H *.suo
DEL /F /Q /S *.sdf
DEL /F /Q /S *.bak
DEL /F /Q /S *.log
DEL /F /Q /S *.vshost.*
DEL /F /Q /S .\bin\Debug\*.pdb
DEL /F /Q /S .\bin\Release\*.pdb
DEL /F /Q /S ..\Yanitta.rar
DEL /F /Q /S ..\YanittaBin.rar

ECHO Done

ECHO Create arhive...
"c:\Program Files (x86)\WinRAR\WinRAR.exe" a -t -r -ed ..\Yanitta.rar ..\Yanitta\*.*
ECHO Done

PAUSE