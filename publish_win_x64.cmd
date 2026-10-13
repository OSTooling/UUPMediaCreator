@echo off
REM 64-bit Windows build (default .NET 10, Windows 10 1607+).
REM See publish.cmd for details.
call "%~dp0publish.cmd" win-x64 %*
exit /b %ERRORLEVEL%
