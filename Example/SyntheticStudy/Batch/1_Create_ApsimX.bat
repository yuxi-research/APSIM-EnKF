set "this=%~dp0"
set "drive=%this:~0,2%"
REM set "git=%drive%\Works\GitHub"
set "git=%drive%\Works\GitHub_Publish\APSIM-EnKF"

call %git%\CreateFiles\EnKF\bin\Debug\EnKF.exe .

timeout 10