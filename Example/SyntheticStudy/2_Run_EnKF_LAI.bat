set "this=%~dp0"
set "drive=%this:~0,2%"
set "git=%drive%\Works\GitHub_Publish\APSIM-EnKF"
set "bin=%git%\ApsimX.DA\Bin"
set "plot=%git%\Example\SyntheticStudy\Output\SyntheticModule\control	

REM -----------------------------------------------------------	
	set out=Output\LAI
	del Obs\*_Obs.csv
	copy Obs\General\LAI_Obs.csv Obs\LAI_Obs.csv
	timeout 2
	call %bin%\Models.exe Output\EnKF.apsimx
	timeout 2
	if not exist %out% md %out%
	copy Output\EnKF.apsimx %out%\EnKF.apsimx
	copy Output\EnKF.db %out%\EnKF.db
	copy Output\States.sqlite %out%\States.sqlite
	echo ---------- Finished! ----------

REM -----------------------------------------------------------	
	call activate synthetic_study
	cd Output\OpenLoop
	call python %plot%\plot_states.py
	call python %plot%\plot_soil_profile.py
