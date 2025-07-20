@echo off
setlocal

(fsutil dirty query %systemdrive% 1>nul 2>nul) || (echo Start-Process $env:ComSpec '/s /c "cd /d "%cd%" && "%~f0" %*"' -Verb RunAs -PassThru ^| Wait-Process> "%temp%\getadmin.ps1") && (powershell -ExecutionPolicy Bypass -File "%temp%\getadmin.ps1") && (exit /b)

powershell.exe -ExecutionPolicy Bypass -File "%~dpn0.ps1" 8 WindowsDesktop

start "" "%~dp0HellDivers2OneKeyStratagem.exe"

endlocal
