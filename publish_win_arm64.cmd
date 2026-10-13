@echo off
REM ARM64 Windows build (default .NET 10, Windows 10 1607+).
REM See publish.cmd for details.
call "%~dp0publish.cmd" win-arm64 %*
exit /b %ERRORLEVEL%
