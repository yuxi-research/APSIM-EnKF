::IF NOT EXIST Met\*.met call ..\..\CreateFiles\Weather\bin\Debug\Weather.exe .


set "this=%~dp0"
set "drive=%this:~0,2%"
set "git=%drive%\Works\GitHub_Publish\APSIM-EnKF"

call %git%\CreateFiles\Weather\bin\Debug\Weather.exe . 50


timeout 30