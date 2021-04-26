@echo off

set _programFiles=%ProgramFiles(x86)%
if not defined _programFiles set _programFiles=%ProgramFiles%

orang delete "../src" -a d -n "bin,obj" l li e -i "packages,node_modules" l li e ne -t n --content-only -y su s

orang replace "../src" -n "AssemblyInfo.cs" e -c "patterns/assembly_names_to_be_prefixed.txt" f -r "_"

dotnet restore --force "..\src\CommandLine.sln"

rd /S /Q "..\src\CommandLine\bin\Release\publish"

"%_programFiles%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild" "..\src\CommandLine.sln" ^
 /t:Clean,Publish ^
 /p:Configuration=Release,RoslynatorCommandLine=true,Deterministic=true,TreatWarningsAsErrors=true,WarningsNotAsErrors="1591" ^
 /v:normal ^
 /m

if errorlevel 1 (
 pause
 exit
)

del /Q "..\src\CommandLine\bin\Release\Roslynator.CommandLine.*.nupkg"

dotnet pack -c Release --no-build -v normal /p:RoslynatorCommandLine=true "..\src\CommandLine\CommandLine.csproj"

orang replace "../src" -n "AssemblyInfo.cs" e -c "patterns/assembly_names_to_be_prefixed.txt" f -r ""

set _outDir=..\out\Release
md "%_outDir%"
del /Q "%_outDir%\Release\Roslynator.CommandLine.*.nupkg"
copy "..\src\CommandLine\bin\Release\Roslynator.CommandLine.*.nupkg" "%_outDir%"

echo OK
pause
