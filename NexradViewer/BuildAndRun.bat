@echo off
echo ======================================
echo Building and Running NEXRAD Viewer...
echo ======================================

REM Perform cleanup first
echo Cleaning previous build artifacts...
dotnet clean --nologo > nul 2>&1 

REM Kill any running instances
tasklist /fi "ImageName eq NexradViewer.exe" /fo csv 2>NUL | find /I "nexradviewer.exe" >NUL
if %ERRORLEVEL% EQU 0 (
    echo Stopping running instances of NexradViewer...
    taskkill /F /IM NexradViewer.exe > nul 2>&1
)

REM Manual cleanup of problematic files
if exist "obj\Debug\net9.0\Avalonia\original.pdb" (
    del /F "obj\Debug\net9.0\Avalonia\original.pdb" > nul 2>&1
)
if exist "bin\Debug\net9.0\NexradViewer.pdb" (
    del /F "bin\Debug\net9.0\NexradViewer.pdb" > nul 2>&1
)

REM Create Assets directory if it doesn't exist
if not exist "Assets" mkdir Assets

REM Create a blank icon file if it doesn't exist
if not exist "Assets\nexrad-icon.ico" (
    echo Creating placeholder icon file...
    copy nul "Assets\nexrad-icon.ico" > nul
)

REM Create Data directory if it doesn't exist
if not exist "Data" mkdir Data

REM Ensure stations.json is in the Data directory
if not exist "Data\stations.json" (
    echo Copying stations.json to Data directory...
    copy "stations.json" "Data\stations.json" > nul 2>&1
)

echo.
echo Building application...
dotnet build -c Debug --nologo

IF %ERRORLEVEL% NEQ 0 (
    echo.
    echo Build failed with error code %ERRORLEVEL%
    echo.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Starting NEXRAD Viewer...
echo.
dotnet run -c Debug

pause
