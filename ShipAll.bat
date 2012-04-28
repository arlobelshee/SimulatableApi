@echo off

pushd build

..\.nuget\nuget.exe push SimulatableApi.StreamStore.*.0.nupkg
if ERRORLEVEL 1 goto fail



goto done


:fail

echo.
echo.
echo an error occurred
pause


:done

popd