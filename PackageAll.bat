@echo off

mkdir build
del /q build\*.*

echo Packing SimulatableApi.StreamStore...
pushd SimulatableApi.StreamStore
call Package.bat
if ERRORLEVEL 1 goto failed
popd




goto done


:failed

popd

echo.
echo.
echo An error occurred
pause

:done
