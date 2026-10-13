@echo off
REM ===========================================================================
REM  Legacy (old Windows) build: Windows 7 SP1 / 8 / 8.1.
REM
REM  The default build targets .NET 10, which only runs on Windows 10 1607 or
REM  later. This script targets .NET 6, the last runtime that runs on Windows 7
REM  SP1, and produces self-contained single-file executables (the .NET runtime
REM  is NOT required on the target machine).
REM
REM  Requirements on the target machine:
REM    * Windows 7 SP1 (or 8 / 8.1)
REM    * Universal C Runtime        (KB2999226)
REM    * SHA-2 code signing support (KB4474419)
REM    * KB2533623
REM
REM  Output: publish\artifacts\win-legacy-x86\CLI and
REM          publish\artifacts\win-legacy-x64\CLI
REM ===========================================================================
call "%~dp0publish.cmd" win-x86 net6.0 || exit /b 1
call "%~dp0publish.cmd" win-x64 net6.0 || exit /b 1
