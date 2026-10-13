@echo off
REM 32-bit Windows build (default .NET 10, Windows 10 1607+).
REM See publish.cmd for details.
call "%~dp0publish.cmd" win-x86 %*
exit /b %ERRORLEVEL%
