@echo off
REM linux-x64 build, cross-compiled from Windows (default .NET 10).
REM See publish.cmd for details. On Linux use ./publish_linux.sh instead.
call "%~dp0publish.cmd" linux-x64 %*
exit /b %ERRORLEVEL%
