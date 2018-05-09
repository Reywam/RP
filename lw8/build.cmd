@echo off
if "%~1" == "" goto error

rem ������ ����� � ����� ������� � ��� ����� ��� �����������
md %~1
mkdir "%~1"\"Frontend"
mkdir "%~1"\"Backend"
mkdir "%~1"\"TextRankCalc"
mkdir "%~1"\"TextListener"
mkdir "%~1"\"VowelConsCounter"
mkdir "%~1"\"VowelConsRater"
mkdir "%~1"\"TextStatistics"
mkdir "%~1"\"config"

rem ����������� ��� ����������
start /wait /d Frontend dotnet publish
start /wait /d Backend dotnet publish
start /wait /d TextRankCalc dotnet publish
start /wait /d TextListener dotnet publish
start /wait /d VowelConsCounter dotnet publish
start /wait /d VowelConsRater dotnet publish
start /wait /d TextStatistics dotnet publish
start /wait /d TextProcessingLimiter dotnet publish
start /wait /d TextSuccessMarker dotnet publish


rem �������� ���������� � ��������� ����� � ��������������� ����������
start /wait xcopy Frontend\bin\Debug\netcoreapp2.0\publish "%~1"\"Frontend"
start /wait xcopy Backend\bin\Debug\netcoreapp2.0\publish "%~1"\"Backend"
start /wait xcopy TextRankCalc\bin\Debug\netcoreapp2.0\publish "%~1"\"TextRankCalc"
start /wait xcopy TextListener\bin\Debug\netcoreapp2.0\publish "%~1"\"TextListener"
start /wait xcopy VowelConsCounter\bin\Debug\netcoreapp2.0\publish "%~1"\"VowelConsCounter"
start /wait xcopy VowelConsRater\bin\Debug\netcoreapp2.0\publish "%~1"\"VowelConsRater"
start /wait xcopy TextStatistics\bin\Debug\netcoreapp2.0\publish "%~1"\"TextStatistics"
start /wait xcopy TextProcessingLimiter\bin\Debug\netcoreapp2.0\publish "%~1"\"TextProcessingLimiter"
start /wait xcopy TextSuccessMarker\bin\Debug\netcoreapp2.0\publish "%~1"\"TextSuccessMarker"

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