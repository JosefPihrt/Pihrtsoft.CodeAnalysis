# Roslynator <img align="left" width="48px" height="48px" src="http://pihrt.net/images/Roslynator.ico">

A collection of 500+ [analyzers](src/Analyzers/README.md), [refactorings](src/Refactorings/README.md) and [fixes](src/CodeFixes/README.md) for C#, powered by [Roslyn](http://github.com/dotnet/roslyn).

**PROMO: Try [Orang](https://github.com/JosefPihrt/Orang), cross-platform command-line tool from the author of Roslynator. Orang can search, replace, rename and delete directories, files and its content using the power of .NET regular expressions.**

### Features

* [Extensions for Visual Studio](#extensions-for-visual-studio)
* [NuGet Analyzers](#nuget-analyzers)
* [Roslynator API](#roslynator-api)
* [Roslynator Command-Line Interface](#roslynator-command-line-interface)
* [Roslynator for VS Code](#extensions-for-visual-studio-code)
* [Release notes](ChangeLog.md)
* Follow on [Twitter](https://twitter.com/roslynator)

### New Features

* [Roslynator for Visual Studio Code](#extensions-for-visual-studio-code)
* [Fix all diagnostics in a solution](docs/HowToFixAllDiagnostics.md)
* [Generate API documentation](docs/HowToGenerateDocumentation.md)

### Donation

> "It's so good, that I made my first ever donation." Mateusz Piasecki, Roslynator user.

Although Roslynator products are free of charge, any [donation](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=BX85UA346VTN6) is welcome and supports further development.

## Extensions for Visual Studio

| Extension | Comment |
| --- | --- |
| [Roslynator 2019](https://marketplace.visualstudio.com/items?itemName=josefpihrt.Roslynator2019) | contains analyzers, refactorings and fixes for CS diagnostics. |
| [Roslynator 2017](https://marketplace.visualstudio.com/items?itemName=josefpihrt.Roslynator2017) | contains analyzers, refactorings and fixes for CS diagnostics. This extension is no longer in development. |

## Extensions for Visual Studio Code

| Extension | Comment |
| --- | --- |
| [Roslynator](https://marketplace.visualstudio.com/items?itemName=josefpihrt-vscode.roslynator) | contains analyzers, refactorings and fixes for CS diagnostics. |

## NuGet Analyzers

| Package | Version | Comment |
| --- | --- | --- |
| [Roslynator.Analyzers](https://www.nuget.org/packages/Roslynator.Analyzers) | [![NuGet](https://img.shields.io/nuget/v/Roslynator.Analyzers.svg)](https://www.nuget.org/packages/Roslynator.Analyzers) | common analyzers (RCS1xxx) ([list](http://pihrt.net/Roslynator/Analyzers?Query=RCS1)) |
| [Roslynator.CodeAnalysis.Analyzers](https://www.nuget.org/packages/Roslynator.CodeAnalysis.Analyzers) | [![NuGet](https://img.shields.io/nuget/v/Roslynator.CodeAnalysis.Analyzers.svg)](https://www.nuget.org/packages/Roslynator.CodeAnalysis.Analyzers) | analyzers for Roslyn API (RCS9xxx) ([list](http://pihrt.net/Roslynator/Analyzers?Query=RCS9)) |
| [Roslynator.Formatting.Analyzers](https://www.nuget.org/packages/Roslynator.Formatting.Analyzers) | [![NuGet](https://img.shields.io/nuget/v/Roslynator.Formatting.Analyzers.svg)](https://www.nuget.org/packages/Roslynator.Formatting.Analyzers) | formatting analyzers (RCS0xxx) ([list](http://pihrt.net/Roslynator/Analyzers?Query=RCS0)) |

*Note: All analyzers in package Roslynator.Formatting.Analyzers are disabled by default.*

## Roslynator API

* Roslynator API is meant be used for development of your own analyzers/refactorings.
* It does not contain any analyzers/refactorings itself.
* See [API Reference](docs/api/README.md).

| Package | Version | Built on top of |
| --- | --- | --- |
| [Roslynator.Core](https://www.nuget.org/packages/Roslynator.Core) | [![NuGet](https://img.shields.io/nuget/v/Roslynator.Core.svg)](https://www.nuget.org/packages/Roslynator.Core) | [Microsoft.CodeAnalysis.Common](https://www.nuget.org/packages/Microsoft.CodeAnalysis.Common) |
| [Roslynator.Workspaces.Core](https://www.nuget.org/packages/Roslynator.Workspaces.Core) | [![NuGet](https://img.shields.io/nuget/v/Roslynator.Workspaces.Core.svg)](https://www.nuget.org/packages/Roslynator.Workspaces.Core) | [Microsoft.CodeAnalysis.Workspaces.Common](https://www.nuget.org/packages/Microsoft.CodeAnalysis.Workspaces.Common) |
| [Roslynator.CSharp](https://www.nuget.org/packages/Roslynator.CSharp) | [![NuGet](https://img.shields.io/nuget/v/Roslynator.CSharp.svg)](https://www.nuget.org/packages/Roslynator.CSharp) | [Microsoft.CodeAnalysis.CSharp](https://www.nuget.org/packages/Microsoft.CodeAnalysis.CSharp) |
| [Roslynator.CSharp.Workspaces](https://www.nuget.org/packages/Roslynator.CSharp.Workspaces) | [![NuGet](https://img.shields.io/nuget/v/Roslynator.CSharp.Workspaces.svg)](https://www.nuget.org/packages/Roslynator.CSharp.Workspaces) | [Microsoft.CodeAnalysis.CSharp.Workspaces](https://www.nuget.org/packages/Microsoft.CodeAnalysis.CSharp.Workspaces) |

## Roslynator Command-Line Interface

* Roslynator CLI is distributed as NuGet package [Roslynator.CommandLine](https://www.nuget.org/packages/Roslynator.CommandLine). &ensp;[![NuGet](https://img.shields.io/nuget/v/Roslynator.CommandLine.svg)](https://www.nuget.org/packages/Roslynator.CommandLine)
* Roslynator CLI is also distributed as .NET Core Global Tool [Roslynator.DotNet.Cli](https://www.nuget.org/packages/Roslynator.DotNet.Cli). &ensp;[![NuGet](https://img.shields.io/nuget/v/Roslynator.DotNet.Cli.svg)](https://www.nuget.org/packages/Roslynator.DotNet.Cli)
* See [documentation](docs/cli/README.md).

## Roslynator Testing Framework

* Roslynator Testing Framework can be used for unit testing of analyzers, refactorings and code fixes.
* Framework is distributed as NuGet [package](https://www.nuget.org/packages/Roslynator.Testing.CSharp.Xunit). &ensp;[![NuGet](https://img.shields.io/nuget/v/Roslynator.Testing.CSharp.Xunit.svg)](https://www.nuget.org/packages/Roslynator.Testing.CSharp.Xunit)
* Learn how to use the framework from actual usages in Roslynator repo:
  * Tests of analyzers are [here](src/Tests/Analyzers.Tests), [here](src/Tests/CodeAnalysis.Analyzers.Tests) and [here](src/Tests/Formatting.Analyzers.Tests)
  * Tests of refactorings are [here](src/Tests/Refactorings.Tests)
  * Tests of fixes of compiler diagnostics are [here](src/Tests/CodeFixes.Tests)

## Documentation

* [Analyzers vs. Refactorings](docs/AnalyzersVsRefactorings.md)
* [How to Configure Analyzers](docs/HowToConfigureAnalyzers.md)
* [How to Configure Refactorings](docs/HowToConfigureRefactorings.md)
* [How to Customize Rules for a Project](docs/HowToCustomizeRulesForProject.md)
* [How to Fix All Diagnostics in a Solution](docs/HowToFixAllDiagnostics.md)
* [How to Generate API Documentation](docs/HowToGenerateDocumentation.md)

Would you like to improve Roslynator documentation? Please see [how to update documentation](docs/HowToUpdateDocumentation.md).

## Other Projects

* [Snippetica](https://github.com/JosefPihrt/Snippetica) - A collection of snippets for C++, C#, HTML, JSON, Markdown, VB, XAML and XML
* [DotMarkdown](https://github.com/JosefPihrt/DotMarkdown) - Markdown framework for .NET
* [LINQ to Regex](https://github.com/JosefPihrt/LinqToRegex) - A library that provides language integrated access to .NET regular expressions
* [Snippet Manager](https://github.com/JosefPihrt/SnippetManager) - A library that enables to work with Visual Studio snippets
* [Regexator](http://pihrt.net/Regexator) - A comprehensive development environment for .NET regular expressions
