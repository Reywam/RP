@echo off
if "%~1" == "" goto error

rem ������ ����� � ����� ������� � ��� ����� ��� �����������
md %~1
mkdir "%~1"\"Frontend"
mkdir "%~1"\"Backend"
mkdir "%~1"\"config"

rem ����������� ��� ����������
start /wait /d Frontend dotnet publish
start /wait /d Backend dotnet publish

rem �������� ���������� � ��������� ����� � ��������������� ����������
start /wait xcopy Frontend\bin\Debug\netcoreapp2.0\publish "%~1"\"Frontend"
start /wait xcopy Backend\bin\Debug\netcoreapp2.0\publish "%~1"\"Backend"

rem �������� ����� ������� � ����� � ��������
start /wait xcopy config "%~1"\config

rem �������� ���������� � ����� � ��������
start /wait xcopy run.cmd "%~1"

rem �������� �������������� � ����� � ��������
start /wait xcopy stop.cmd "%~1"

echo "Project created"
exit 0

:error
echo "Empty argument"
exit 1