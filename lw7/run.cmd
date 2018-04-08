@echo off
setlocal enabledelayedexpansion

rem Запускаем редис
for %%i in ("%~dp0\..") do set "parent=%%~fi"
for %%i in ("%parent%\..") do set "big_parent=%%~fi"
start /d %big_parent%\Redis redis-server.exe

start /d Frontend dotnet Frontend.dll
start /d Backend dotnet Backend.dll 
start /d TextRankCalc dotnet TextRankCalc.dll
start /d TextListener dotnet TextListener.dll
start /d TextStatistics dotnet TextStatistics.dll

rem Эти компоненты могут запускаться в N экземплярах
set file=config\components_config.txt
for /f "tokens=1,2 delims=:" %%a in (%file%) do (
for /l %%i in (1, 1, %%b) do start /d %%a dotnet %%a.dll
)

