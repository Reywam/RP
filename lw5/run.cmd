@echo off
rem Запускаем редис
for %%i in ("%~dp0\..") do set "parent=%%~fi"
for %%i in ("%parent%\..") do set "big_parent=%%~fi"
start /d %big_parent%\Redis redis-server.exe

start /d Frontend dotnet Frontend.dll
start /d Backend dotnet Backend.dll 
start /d TextRankCalc dotnet TextRankCalc.dll
start /d TextListener dotnet TextListener.dll
start /d VowelConsCounter dotnet VowelConsCounter.dll
start /d VowelConsRater dotnet VowelConsRater.dll

