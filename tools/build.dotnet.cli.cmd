@echo off

rd /S /Q "..\src\CommandLine\bin\Release\netcoreapp3.1"

del /Q "..\src\CommandLine\bin\Release\Roslynator.DotNet.Cli.*.nupkg"

orang delete "../src" -a d -n "bin,obj" l li e -i "packages,node_modules" l li e ne -t n --content-only -y su s

orang replace "../src" -n "AssemblyInfo.cs" e -c "patterns/assembly_names_to_be_prefixed.txt" f -r "_"

dotnet restore --force "..\src\CommandLine.sln"

dotnet clean "..\src\CommandLine.sln"

dotnet pack "..\src\CommandLine\CommandLine.csproj" -c Release -v normal ^
 /p:RoslynatorDotNetCli=true,Deterministic=true,TreatWarningsAsErrors=true,WarningsNotAsErrors="1591"

orang replace "../src" -n "AssemblyInfo.cs" e -c "patterns/assembly_names_to_be_prefixed.txt" f -r ""

set _outDir=..\out\Release
md "%_outDir%"
del /Q "%_outDir%\Release\Roslynator.DotNet.Cli.*.nupkg"
copy "..\src\CommandLine\bin\Release\Roslynator.DotNet.Cli.*.nupkg" "%_outDir%"

echo OK
pause
