@echo off
REM ===========================================================================
REM  UUPMediaCreator - build/publish helper (Windows).
REM
REM  Usage:  publish.cmd ^<rid^> [tfm]
REM
REM  RIDs:   win-x64  win-x86  win-arm64  linux-x64  linux-arm64  osx-x64  osx-arm64
REM  TFMs:   net10.0 (default, Windows 10 1607+) ^| net6.0 (legacy, Windows 7 SP1+)
REM
REM  Examples:
REM    publish.cmd win-x64            .NET 10 win-x64
REM    publish.cmd win-x86            .NET 10 win-x86 (32-bit)
REM    publish.cmd win-arm64          .NET 10 win-arm64
REM    publish.cmd win-x64 net6.0     legacy Windows 7 SP1+ (win-legacy-x64)
REM    publish.cmd linux-x64          cross-build linux-x64 from Windows
REM
REM  Output: publish\artifacts^<name^>\CLI
REM
REM  Only the .NET SDK is required (no Visual Studio): it is invoked through
REM  "dotnet msbuild" and works from any current directory.
REM ===========================================================================
setlocal EnableExtensions
pushd "%~dp0"

set "RID=%~1"
set "TFM=%~2"
if "%TFM%"=="" set "TFM=net10.0"
if "%RID%"=="" ( call :usage & popd & exit /b 2 )

set "KNOWN=win-x64;win-x86;win-arm64;linux-x64;linux-arm64;osx-x64;osx-arm64"
set "VALID="
for %%A in (%KNOWN:;= %) do if /I "%%A"=="%RID%" set "VALID=1"
if not defined VALID (
    echo Unsupported runtime identifier "%RID%".
    echo Supported: %KNOWN:;=, %
    call :usage
    popd
    exit /b 2
)

set "NAME=%RID%"
if /I "%TFM%"=="net6.0" set "NAME=%RID:win-=win-legacy-%"
if /I "%TFM%"=="net6.0" if "%NAME%"=="%RID%" (
    echo The legacy ^(net6.0^) build is only supported for Windows runtimes.
    popd
    exit /b 2
)

set "OUTDIR=%~dp0publish\artifacts\%NAME%\CLI"

where dotnet >nul 2>nul
if errorlevel 1 (
    echo The .NET SDK was not found in PATH. Install the .NET 10 SDK first.
    popd
    exit /b 1
)

set "EXTRA_ARGS="
if /I "%TFM%"=="net6.0" set "EXTRA_ARGS=/p:LangVersion=latest"

echo ==^> Publishing %NAME% (runtime %RID%, %TFM%)
echo ==^> Output: %OUTDIR%
if not exist "%OUTDIR%" mkdir "%OUTDIR%"

for %%P in (
    "src\Applications\UUPDownload\UUPDownload.csproj"
    "src\Applications\UUPMediaConverter\UUPMediaConverter.csproj"
) do (
    echo --^> %%~P
    dotnet msbuild "%%~P" "/t:restore;publish" /p:Configuration=Release /p:RuntimeIdentifier=%RID% /p:UUPMediaTargetFramework=%TFM% %EXTRA_ARGS% /p:SelfContained=true /p:PublishSingleFile=true /p:PublishTrimmed=false "/p:PublishDir=%OUTDIR%\"
    if errorlevel 1 (
        echo ERROR: publishing %%~P failed.
        popd
        exit /b 1
    )
)

echo.
echo Done. Binaries are in: %OUTDIR%
popd
exit /b 0

:usage
echo Usage: publish.cmd ^<rid^> [tfm]
echo   rid: win-x64 ^| win-x86 ^| win-arm64 ^| linux-x64 ^| linux-arm64 ^| osx-x64 ^| osx-arm64
echo   tfm: net10.0 ^(default^) ^| net6.0 ^(legacy Windows 7 SP1+^)
exit /b 0
