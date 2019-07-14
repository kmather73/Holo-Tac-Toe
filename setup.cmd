@echo off
setlocal
set SourceRoot=%~dp0

call :CreateDirLink "%sourceRoot%\src\Tic-Tac-Toe-MR\Assets\Addons\MixedRealityToolkit-Unity" "..\..\..\..\External\MixedRealityToolkit-Unity"
GOTO End
:CreateDirLink

if exist %1\* (
    rmdir %1
) else (
    if exist %1 del /q %1
)
mklink /d %1 %2

exit /b

:End
endlocal