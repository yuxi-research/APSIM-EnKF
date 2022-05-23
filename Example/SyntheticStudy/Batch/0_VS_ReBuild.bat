set "this=%~dp0"
set "drive=%this:~0,2%"
set "git=%drive%\Works\GitHub_Publish\APSIM-EnKF"

call "D:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe" "%git%\ApsimX.DA\ApsimX.sln" /rebuild

pause