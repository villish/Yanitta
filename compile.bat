"C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" /m Yanitta.sln /p:Configuration=Release "/p:Platform=Any CPU"
@IF %ERRORLEVEL% NEQ 0 GOTO err
@exit /B 0
:err
@PAUSE
@exit /B 1