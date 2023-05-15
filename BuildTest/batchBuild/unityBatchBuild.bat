@echo off
set ENV_FILE=%1
echo.
echo ~~~~~ BUILD start %TIME% with %ENV_FILE% ~~~~~
cd
if not exist %ENV_FILE% (
    echo *
    echo * Build environment file %ENV_FILE% not found
    echo *
    goto :done
)
FOR /F "eol=# tokens=*" %%i IN (%ENV_FILE%) DO (
	echo env set %%i
	SET %%i
)
if not exist "%UNITY_EXE%" (
    echo *
    echo * UNITY executable %UNITY_EXE% not found
    echo *
    goto :done
)
if exist %LOG_FILE% (
    echo. >%LOG_FILE%
)
echo.
echo ~~~~~ BUILD execute %ENV_FILE% ~~~~~
set BUILD_PARAMS1=-executeMethod %BUILD_METHOD% -quit -batchmode
set BUILD_PARAMS2=-projectPath .\ -buildTarget %BUILD_TARGET% -logFile "%LOG_FILE%"
set BUILD_PARAMS3=-envFile "%ENV_FILE%"
echo set1 %BUILD_PARAMS1%
echo set2 %BUILD_PARAMS2%
echo set3 %BUILD_PARAMS3%
@echo on
"%UNITY_EXE%" %BUILD_PARAMS1% %BUILD_PARAMS2% %BUILD_PARAMS3%
@set RESULT=%ERRORLEVEL%
@echo off
echo Build returns %RESULT%
if not "%RESULT%" == "0" (
    echo *
    echo * Build FAILED with %RESULT%, check log file %LOG_FILE% errors!
    echo *
    goto :done
)
:done
echo.
echo ~~~~~ BUILD done  %TIME% with %ENV_FILE% ~~~~~
