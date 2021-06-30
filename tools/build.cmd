@echo off

set _programFiles=%ProgramFiles(x86)%
if not defined _programFiles set _programFiles=%ProgramFiles%

set _msbuildPath="%_programFiles%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild"
set _properties=Configuration=Release,Deterministic=true,TreatWarningsAsErrors=true,WarningsNotAsErrors=1591
set _outDir=..\out\Release
set _version=3.2.1

orang delete "..\src" -a d -n "bin,obj" l li e -i "packages,node_modules" l li e ne -t n --content-only -y su s

echo.

dotnet restore --force "..\src\Roslynator.sln"

dotnet restore "..\src\Tools\Tools.sln"

%_msbuildPath% "..\src\Tools\Tools.sln" ^
 /t:Clean,Build ^
 /p:%_properties% ^
 /v:normal ^
 /m

if errorlevel 1 (
 pause
 exit
)

"..\src\Tools\MetadataGenerator\bin\Release\net48\Roslynator.MetadataGenerator.exe" "..\src"
dotnet "..\src\Tools\CodeGenerator\bin\Release\netcoreapp2.1\CodeGenerator.dll" "..\src"

%_msbuildPath% "..\src\Roslynator.sln" ^
 /t:Clean ^
 /p:Configuration=Debug ^
 /v:minimal ^
 /m

%_msbuildPath% "..\src\Roslynator.sln" ^
 /t:Clean,Build ^
 /p:%_properties% ^
 /v:normal ^
 /m

if errorlevel 1 (
 pause
 exit
)

dotnet test -c Release --no-build "..\src\Tests\Core.Tests\Core.Tests.csproj"

if errorlevel 1 (
 pause
 exit
)

dotnet test -c Release --no-build "..\src\Tests\CSharp.Tests\CSharp.Tests.csproj"

if errorlevel 1 (
 pause
 exit
)

dotnet test -c Release --no-build "..\src\Tests\CSharp.Workspaces.Tests\CSharp.Workspaces.Tests.csproj"

if errorlevel 1 (
 pause
 exit
)

dotnet test -c Release --no-build "..\src\Tests\Analyzers.Tests\Analyzers.Tests.csproj"

if errorlevel 1 (
 pause
 exit
)

dotnet test -c Release --no-build "..\src\Tests\CodeAnalysis.Analyzers.Tests\CodeAnalysis.Analyzers.Tests.csproj"

if errorlevel 1 (
 pause
 exit
)

dotnet test -c Release --no-build "..\src\Tests\Formatting.Analyzers.Tests\Formatting.Analyzers.Tests.csproj"

if errorlevel 1 (
 pause
 exit
)

dotnet test -c Release --no-build "..\src\Tests\CodeFixes.Tests\CodeFixes.Tests.csproj"

if errorlevel 1 (
 pause
 exit
)

dotnet test -c Release --no-build "..\src\Tests\Refactorings.Tests\Refactorings.Tests.csproj"

if errorlevel 1 (
 pause
 exit
)

del /Q "..\src\Analyzers.CodeFixes\bin\Release\Roslynator.Analyzers.*.nupkg"
del /Q "..\src\CodeAnalysis.Analyzers.CodeFixes\bin\Release\Roslynator.CodeAnalysis.Analyzers.*.nupkg"
del /Q "..\src\Formatting.Analyzers.CodeFixes\bin\Release\Roslynator.Formatting.Analyzers.*.nupkg"
del /Q "..\src\Core\bin\Release\Roslynator.Core.*.nupkg"
del /Q "..\src\Workspaces.Core\bin\Release\Roslynator.Workspaces.Core.*.nupkg"
del /Q "..\src\CSharp\bin\Release\Roslynator.CSharp.*.nupkg"
del /Q "..\src\CSharp.Workspaces\bin\Release\Roslynator.CSharp.Workspaces.*.nupkg"
del /Q "..\src\Tests\Testing.Common\bin\Release\Roslynator.Testing.Common.*.nupkg"
del /Q "..\src\Tests\Testing.CSharp\bin\Release\Roslynator.Testing.CSharp.*.nupkg"
del /Q "..\src\Tests\Testing.CSharp.Xunit\bin\Release\Roslynator.Testing.CSharp.Xunit.*.nupkg"

dotnet pack -c Release --no-build -v normal "..\src\Analyzers.CodeFixes\Analyzers.CodeFixes.csproj"
dotnet pack -c Release --no-build -v normal "..\src\CodeAnalysis.Analyzers.CodeFixes\CodeAnalysis.Analyzers.CodeFixes.csproj"
dotnet pack -c Release --no-build -v normal "..\src\Formatting.Analyzers.CodeFixes\Formatting.Analyzers.CodeFixes.csproj"
dotnet pack -c Release --no-build -v normal "..\src\Core\Core.csproj"
dotnet pack -c Release --no-build -v normal "..\src\Workspaces.Core\Workspaces.Core.csproj"
dotnet pack -c Release --no-build -v normal "..\src\CSharp\CSharp.csproj"
dotnet pack -c Release --no-build -v normal "..\src\CSharp.Workspaces\CSharp.Workspaces.csproj"
dotnet pack -c Release --no-build -v normal "..\src\Tests\Testing.Common\Testing.Common.csproj"
dotnet pack -c Release --no-build -v normal "..\src\Tests\Testing.CSharp\Testing.CSharp.csproj"
dotnet pack -c Release --no-build -v normal "..\src\Tests\Testing.CSharp.Xunit\Testing.CSharp.Xunit.csproj"

del /Q "..\src\VisualStudio\bin\Release\Roslynator.VisualStudio.*.vsix"
ren    "..\src\VisualStudio\bin\Release\Roslynator.VisualStudio.vsix" "Roslynator.VisualStudio.%_version%.vsix"

md "%_outDir%"

del /Q "%_outDir%\*"

copy "..\src\VisualStudio\bin\Release\Roslynator.VisualStudio.%_version%.vsix" "%_outDir%"

copy "..\src\Analyzers.CodeFixes\bin\Release\Roslynator.Analyzers.*.nupkg" "%_outDir%"
copy "..\src\CodeAnalysis.Analyzers.CodeFixes\bin\Release\Roslynator.CodeAnalysis.Analyzers.*.nupkg" "%_outDir%"
copy "..\src\Formatting.Analyzers.CodeFixes\bin\Release\Roslynator.Formatting.Analyzers.*.nupkg" "%_outDir%"
copy "..\src\Core\bin\Release\Roslynator.Core.*.nupkg" "%_outDir%"
copy "..\src\Workspaces.Core\bin\Release\Roslynator.Workspaces.Core.*.nupkg" "%_outDir%"
copy "..\src\CSharp\bin\Release\Roslynator.CSharp.*.nupkg" "%_outDir%"
copy "..\src\CSharp.Workspaces\bin\Release\Roslynator.CSharp.Workspaces.*.nupkg" "%_outDir%"
copy "..\src\Tests\Testing.Common\bin\Release\Roslynator.Testing.Common.*.nupkg" "%_outDir%"
copy "..\src\Tests\Testing.CSharp\bin\Release\Roslynator.Testing.CSharp.*.nupkg" "%_outDir%"
copy "..\src\Tests\Testing.CSharp.Xunit\bin\Release\Roslynator.Testing.CSharp.Xunit.*.nupkg" "%_outDir%"

echo OK
pause
