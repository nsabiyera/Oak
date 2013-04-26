@echo off
if "%1" == "1" goto else
echo building and deploying
call %0 1 > nul
echo done
goto done
:else:
rake
:done:
