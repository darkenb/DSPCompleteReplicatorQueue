@echo off
cd /d "%~dp0"

echo DSPCompleteReplicatorQueue Build + Install
echo =================================

echo [1/4] Eski build temizleniyor...
if exist "bin" (
    rmdir /S /Q "bin"
    echo      bin klasoru silindi.
)

set TARGET=%APPDATA%\com.kesomannen.gale\dyson-sphere-program\profiles\DSP\BepInEx\plugins\DSPCompleteReplicatorQueue
if exist "%TARGET%" (
    rmdir /S /Q "%TARGET%"
    echo      DSPCompleteReplicatorQueue klasoru silindi.
)

echo [2/4] Derleniyor...
dotnet build DSPCompleteReplicatorQueue.csproj -c Release

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [HATA] Derleme basarisiz!
    pause
    exit /b 1
)

echo [3/4] Plugin klasoru olusturuluyor...
mkdir "%TARGET%"

echo [4/4] DLL kopyalaniyor...
copy /Y "bin\Release\DSPCompleteReplicatorQueue.dll" "%TARGET%\DSPCompleteReplicatorQueue.dll"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo [OK] Basarili!
    echo DLL konumu: %TARGET%\DSPCompleteReplicatorQueue.dll
) else (
    echo [HATA] Kopyalama basarisiz!
)

pause
