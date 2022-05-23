set "this=%~dp0"
set "drive=%this:~0,2%"
set set "git=%drive%\Works\GitHub_Publish\APSIM-EnKF"


REM only create Perturb_Control once.
REM call %git%\CreateFiles\Shared\bin\Debug\Shared.exe .

timeout 10