@echo off
setlocal enabledelayedexpansion

set "rootDir=%~dp0bin\runtimes"

for /d %%i in ("%rootDir%\*") do (
    set "dirName=%%~nxi"
    echo !dirName!
    if not "!dirName!"=="win-x64" (
        if not "!dirName!"=="win" (
            echo Deleting %%i
            rd /s /q "%%i"
        )
    )
)

endlocal
