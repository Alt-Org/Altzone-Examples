@echo off
set ENV_FILE=%1
echo.
echo ~~~~~ BUILD start with %ENV_FILE% ~~~~~
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
    del /Q %LOG_FILE%
)
echo.
echo ~~~~~ BUILD execute %ENV_FILE% ~~~~~
echo "%UNITY_EXE%" -quit -batchmode -projectPath ./ -executeMethod %BUILD_METHOD% -buildTarget %BUILD_TARGET% -logFile "%LOG_FILE%"
set RESULT=%ERRORLEVEL%
echo Build returns %RESULT%
if not "%RESULT%" == "0" (
    echo *
    echo * Build FAILED with %RESULT%, check log file %LOG_FILE% errors!
    echo *
    goto :done
)
:done
echo.
echo ~~~~~ BUILD  done with %ENV_FILE% ~~~~~