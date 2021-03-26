
# Roslynator Command-Line Interface

There are two NuGet packages that provide command-line interface (CLI):

* [Roslynator.CommandLine](https://nuget.org/packages/Roslynator.CommandLine) &ensp;[![NuGet](https://img.shields.io/nuget/v/Roslynator.CommandLine.svg)](https://nuget.org/packages/Roslynator.CommandLine)

* [Roslynator.DotNet.Cli](https://nuget.org/packages/Roslynator.DotNet.Cli) &ensp;[![NuGet](https://img.shields.io/nuget/v/Roslynator.DotNet.Cli.svg)](https://nuget.org/packages/Roslynator.DotNet.Cli)

## Which Package to Use

[Roslynator.CommandLine](https://nuget.org/packages/Roslynator.CommandLine) is stand-alone application that requires .NET Framework (runs on Windows only).
If you are on Windows it is definitely recommended to use this package.

[Roslynator.DotNet.Cli](https://nuget.org/packages/Roslynator.DotNet.Cli) is [.NET Core Global Tool](https://docs.microsoft.com/dotnet/core/tools/global-tools).
It is cross-platform tool that can be run directly from command line.
It is recommended to use it if you cannot use [Roslynator.CommandLine](https://nuget.org/packages/Roslynator.CommandLine) (non-Windows environment).

## Which MSBuild Instance to Use

If there are multiple instances of MSBuild on a machine it is neccesary to use parameter `--msbuild-path` to specify one that should be used.

If you are using [Roslynator.CommandLine](https://nuget.org/packages/Roslynator.CommandLine) you should specify MSBuild
instance that is part of Visual Studio installation. It should be similar to:

```shell
C:/Program Files/Microsoft Visual Studio/2019/Community/MSBuild/Current/Bin
```

If you are using [Roslynator.DotNet.Cli](https://nuget.org/packages/Roslynator.DotNet.Cli) you should specify MSBuild
instance that is part of .NET Core SDK installation. It should be similar to:

```shell
C:/Program Files/dotnet/sdk/3.1.200
```

## Commands

* [analyze](analyze-command.md)
* [fix](fix-command.md)
* [format](format-command.md)
* [generate-doc](generate-doc-command.md)
* [generate-doc-root](generate-doc-root-command.md)
* [list-symbols](list-symbols-command.md)
* [loc](loc-command.md)
* [lloc](lloc-command.md)

## Exit Code

Value | Comment
--- | ---
0 | Success\*
1 | Not a success\*\*
2 | Error occured or execution canceled

\* A diagnostic was found (`analyze` command ) or a diagnostic was fixed (`fix` command) etc.
\*\* No diagnostic was found (`analyze` command) or no diagnostic was fixed (`fix` command) etc.

## See Also

* [How to Fix All Diagnostics in a Solution](../HowToFixAllDiagnostics.md)
* [How to Generate API Documentation](../HowToGenerateDocumentation.md)
