@echo off
setlocal
cd /d "%~dp0"

echo Building DSPCompleteReplicatorQueue 1.0.3...
dotnet build "DSPCompleteReplicatorQueue.csproj" -c Release
if errorlevel 1 (
    echo Build failed. No installed files were changed.
    exit /b 1
)

if "%~1"=="" (
    echo Build complete: bin\Release\DSPCompleteReplicatorQueue.dll
    echo To install, close the game and run this script with the existing plugin directory as its first argument.
    exit /b 0
)

set "TARGET_DIR=%~f1"
if not exist "%TARGET_DIR%" mkdir "%TARGET_DIR%"
if errorlevel 1 exit /b 1
if exist "%TARGET_DIR%\DSPCompleteReplicatorQueue.dll" (
    copy /Y "%TARGET_DIR%\DSPCompleteReplicatorQueue.dll" "%TARGET_DIR%\DSPCompleteReplicatorQueue.dll.bak" >nul
    if errorlevel 1 (
        echo Could not back up the installed DLL. Installation cancelled.
        exit /b 1
    )
)
copy /Y "bin\Release\DSPCompleteReplicatorQueue.dll" "%TARGET_DIR%\DSPCompleteReplicatorQueue.dll" >nul
if errorlevel 1 (
    if exist "%TARGET_DIR%\DSPCompleteReplicatorQueue.dll.bak" copy /Y "%TARGET_DIR%\DSPCompleteReplicatorQueue.dll.bak" "%TARGET_DIR%\DSPCompleteReplicatorQueue.dll" >nul
    echo Installation failed. The previous DLL was retained or restored from its backup.
    exit /b 1
)
echo Installed: %TARGET_DIR%\DSPCompleteReplicatorQueue.dll
exit /b 0
