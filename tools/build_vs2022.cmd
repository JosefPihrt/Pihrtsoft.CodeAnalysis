@echo off

set _programFiles=%ProgramFiles%

set _msbuildPath="%_programFiles%\Microsoft Visual Studio\2022\Preview\MSBuild\Current\Bin\MSBuild"
set _properties=Configuration=Release,Deterministic=true,TreatWarningsAsErrors=true,WarningsNotAsErrors=1591
set _outDir=..\out\Release
set _version=3.2.1

orang delete "..\src" -a d -n "bin,obj" l li e -i "packages,node_modules" l li e ne -t n --content-only -y su s

echo.

dotnet restore --force "..\src\VisualStudio2022.sln"

if errorlevel 1 (
 pause
 exit
)

%_msbuildPath% "..\src\VisualStudio2022.sln" ^
 /t:Clean,Build ^
 /p:%_properties% ^
 /v:normal ^
 /m

if errorlevel 1 (
 pause
 exit
)

set _vsixPath=..\src\VisualStudio.2022\bin\Release\Roslynator.VisualStudio.

del /Q "%_vsixPath%*.vsix"
ren    "%_vsixPath%vsix" "Roslynator.VisualStudio.2022.%_version%.vsix"
md "%_outDir%"
del /Q "%_outDir%\*"
copy "%_vsixPath%2022.%_version%.vsix" "%_outDir%"

echo OK
pause
