@echo off
if "%~1" == "" goto error

rem Создаём папку с новой версией и две папки для компонентов
md %~1
mkdir "%~1"\"Frontend"
mkdir "%~1"\"Backend"
mkdir "%~1"\"TextListener"
mkdir "%~1"\"config"

rem Компилируем два компонента
start /wait /d Frontend dotnet publish
start /wait /d Backend dotnet publish
start /wait /d TextListener dotnet publish

rem Копируем компоненты в созданную папку в соответствующие директории
start /wait xcopy Frontend\bin\Debug\netcoreapp2.0\publish "%~1"\"Frontend"
start /wait xcopy Backend\bin\Debug\netcoreapp2.0\publish "%~1"\"Backend"
start /wait xcopy TextListener\bin\Debug\netcoreapp2.0\publish "%~1"\"TextListener"

rem Копируем папку конфига в папку с проектом
start /wait xcopy config "%~1"\config

rem Копируем запускатор в папку с проектом
start /wait xcopy run.cmd "%~1"

rem Копируем останавливатор в папку с проектом
start /wait xcopy stop.cmd "%~1"

rem Копируем запускатор редиса
rem start /wait xcopy start_redis.bat "%~1"

echo "Project created"
exit 0

:error
echo "Empty argument"
exit 1