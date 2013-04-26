@echo off
if "%1" == "1" goto else
echo resetting database
call %0 1 > nul
echo done
goto done
:else:
rake reset
:done:
