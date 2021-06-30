@echo off

rem dotnet install tool -g orang.dotnet.cli

set _apiVersion=2.1.0.2
set _formattingVersion=1.2.0.2
set _version=3.2.1.0
set _version3=3.2.1
set _cliVersion=0.1.4.0
set _cliVersion3=0.1.4
set _root=..\src
set _options=from-file -t m r -y trim-line -v n -o "orang.log" v=di

orang replace ^
  "%_root%\Analyzers\Analyzers.csproj" ^
  "%_root%\Analyzers.CodeFixes\Analyzers.CodeFixes.csproj" ^
  "%_root%\CodeFixes\CodeFixes.csproj" ^
  "%_root%\Common\Common.csproj" ^
  "%_root%\Workspaces.Common\Workspaces.Common.csproj" ^
  "%_root%\Refactorings\Refactorings.csproj" ^
  "%_root%\VisualStudioCode\VisualStudioCode.csproj" ^
 -c "patterns\csproj_version.txt" ^
  %_options% ^
 -r %_version% 

echo.

set _options=%_options% append

orang replace ^
  "%_root%\Core\Core.csproj" ^
  "%_root%\CSharp\CSharp.csproj" ^
  "%_root%\CSharp.Workspaces\CSharp.Workspaces.csproj" ^
  "%_root%\VisualBasic\VisualBasic.csproj" ^
  "%_root%\VisualBasic.Workspaces\VisualBasic.Workspaces.csproj" ^
  "%_root%\Workspaces.Core\Workspaces.Core.csproj" ^
 -c "patterns\csproj_version.txt" ^
  %_options% ^
 -r %_apiVersion%

echo.

orang replace ^
  "%_root%\Formatting.Analyzers\Formatting.Analyzers.csproj" ^
  "%_root%\Formatting.Analyzers.CodeFixes\Formatting.Analyzers.CodeFixes.csproj" ^
 -c "patterns\csproj_version.txt" ^
  %_options% ^
 -r %_formattingVersion%

echo.

orang replace ^
  "%_root%\VisualStudio\source.extension.vsixmanifest" ^
  "%_root%\VisualStudio.2022\source.extension.vsixmanifest" ^
 -c "patterns\vsix_manifest_version.txt" ^
  %_options% ^
 -r %_version3%

echo.

orang replace ^
  "%_root%\VisualStudio\Properties\AssemblyInfo.cs" ^
  "%_root%\VisualStudio.Common\Properties\AssemblyInfo.cs" ^
  "%_root%\VisualStudio.2022\Properties\AssemblyInfo.cs" ^
 -c "patterns\assembly_info_version.txt" ^
  %_options% ^
 -r %_version%

echo.

orang replace ^
  "%_root%\CommandLine\CommandLine.csproj" ^
  "%_root%\Documentation\Documentation.csproj" ^
 -c "patterns\csproj_version.txt" ^
  %_options% ^
 -r %_cliVersion%

orang replace ^
  "%_root%\CommandLine\CommandLine.csproj" ^
  "%_root%\CommandLine\CommandLine.nuspec" ^
 -c "patterns\package_version.txt" ^
  %_options% ^
 -r %_cliVersion3%

echo.

orang replace ^
  "%_root%\VisualStudioCode\package\package.json" ^
 -c "patterns\vscode_version.txt" ^
  %_options% ^
 -r %_version3%

echo.

orang replace ^
  build.cmd ^
  build_vs2022.cmd ^
 -c "patterns\build_script_version.txt" ^
  %_options% ^
 -r %_version3%

pause
