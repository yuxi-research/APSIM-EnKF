set "this=%~dp0"
set "drive=%this:~0,2%"
set "git=%drive%\Works\GitHub_Publish\APSIM-EnKF"

call %git%\CreateFiles\Cultivar\bin\Debug\Cultivar.exe . 50
copy Cultivar\Wheat.xml %git%\DABranch1\ApsimX.DA\Models\Resources\Wheat.xml