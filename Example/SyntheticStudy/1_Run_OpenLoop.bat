set "this=%~dp0"
set "drive=%this:~0,2%"
set "git=%drive%\Works\GitHub_Publish\APSIM-EnKF"
set "bin=%git%\ApsimX.DA\Bin"
set "plot=%git%\Example\SyntheticStudy\Output\SyntheticModule\control	

REM =====================================
	set "out=Output\OpenLoop" 
	del Obs\_Obs.csv
	copy Obs\General\*_Obs.csv Obs\
	timeout 2
	call %bin%\Models.exe Output\OpenLoop.apsimx
	timeout 2
	if not exist %out% md %out%
	copy Output\OpenLoop.* %out%\OpenLoop.*
	copy Output\States.sqlite %out%\States.sqlite
	echo Finished!
REM =====================================
	call activate synthetic_study
	cd Output\OpenLoop
	call python %plot%\plot_states_ol.py
	call python %plot%\plot_soil_profile_ol.py

	
	timeout 3