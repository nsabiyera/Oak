@echo off
if "%1" == "1" goto else
echo starting iis express
call %0 1 > nul
echo done
goto done
:else:
rake server
:done:
