@echo off
setlocal

pushd "%~dp0"
dotnet run --project ".\src\GameDesignWizard.App\GameDesignWizard.App.csproj" %*
set "exitCode=%ERRORLEVEL%"
popd

exit /b %exitCode%
