@ECHO off
Title=Yanitta Project
color 1A

ECHO Copy profiles...
copy /Y bin\Debug\Profiles.xml Yanitta\Profiles.Original.xml
copy /Y bin\Debug\offsets.xml Yanitta\offsets.xml
ECHO Done