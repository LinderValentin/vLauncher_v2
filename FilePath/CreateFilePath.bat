@echo off
set "DOC=%USERPROFILE%\Documents"

cd "%DOC%"

if not exist vLauncher mkdir vLauncher
if not exist vLauncher\Saves mkdir vLauncher\Saves

echo unbenutzt unbenutzt unbenutzt unbenutzt > vLauncher\Saves\Headlines.vdata