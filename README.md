# Roslynator <img align="left" width="48px" height="48px" src="http://pihrt.net/images/Roslynator.ico">

* A collection of 500+ analyzers, refactorings and fixes for C#, powered by [Roslyn](http://github.com/dotnet/roslyn).
* [List of analyzers](src/Analyzers/README.md)
* [List of refactorings](src/Refactorings/README.md)
* [List of code fixes for CS diagnostics](src/CodeFixes/README.md)
* [Release notes](ChangeLog.md)
* Follow on [Twitter](https://twitter.com/roslynator)

### New Features

* [Fix all diagnostics in a solution](docs/HowToFixAllDiagnostics.md)
* [Generate API documentation](docs/HowToGenerateDocumentation.md)
* https://github.com/oscarg933/Roslynator.git.patch.md

### Donation

> "It's so good, that I made my first ever donation." Mateusz Piasecki, Roslynator user.

Although Roslynator products are free of charge, any [donation](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=BX85UA346VTN6) is welcome and supports further development.

## Extensions for Visual Studio 2017

### Roslynator 2017

* [Roslynator 2017](http://marketplace.visualstudio.com/items?itemName=josefpihrt.Roslynator2017) contains all features - analyzers, refactorings and code fixes for CS diagnostics.

### Roslynator Refactorings 2017

* [Roslynator Refactorings 2017](http://marketplace.visualstudio.com/items?itemName=josefpihrt.RoslynatorRefactorings2017) contains all features except analyzers, it is a subset of Roslynator 2017.
* Use this extension in combination with package [Roslynator.Analyzers](http://www.nuget.org/packages/Roslynator.Analyzers/) or if you are not interested in analyzers at all.

*Note: Roslynator for Visual Studio 2015 is no longer in development.*

## NuGet Packages

### Roslynator.Analyzers &ensp;[![NuGet](https://img.shields.io/nuget/v/Roslynator.Analyzers.svg)](https://nuget.org/packages/Roslynator.Analyzers)

* Package [Roslynator.Analyzers](http://www.nuget.org/packages/Roslynator.Analyzers/) contains only analyzers.
* Use this package if you want integrate analyzers into your build process.

### Roslynator.CodeFixes &ensp;[![NuGet](https://img.shields.io/nuget/v/Roslynator.CodeFixes.svg)](https://nuget.org/packages/Roslynator.CodeFixes)

* Package [Roslynator.CodeFixes](http://www.nuget.org/packages/Roslynator.CodeFixes/) contains only code fixes for CS diagnostics.
* Use this package if you want to distribute these code fixes to your team members.

### Roslynator.CSharp &ensp;[![NuGet](https://img.shields.io/nuget/v/Roslynator.CSharp.svg)](https://nuget.org/packages/Roslynator.CSharp)

* Package [Roslynator.CSharp](http://www.nuget.org/packages/Roslynator.CSharp/) is a must-have for Roslyn-based development.
* It is built on top of Roslyn API (namely [Microsoft.CodeAnalysis.CSharp](http://www.nuget.org/packages/Microsoft.CodeAnalysis.CSharp/)).
<!--* See [API Reference](docs/api/README.md#_top). -->

### Roslynator.CSharp.Workspaces &ensp;[![NuGet](https://img.shields.io/nuget/v/Roslynator.CSharp.Workspaces.svg)](https://nuget.org/packages/Roslynator.CSharp.Workspaces)

* Package [Roslynator.CSharp.Workspaces](http://www.nuget.org/packages/Roslynator.CSharp.Workspaces/) is a must-have for Roslyn-based development.
* It is built on top of Roslyn API (namely [Microsoft.CodeAnalysis.CSharp.Workspaces](http://www.nuget.org/packages/Microsoft.CodeAnalysis.CSharp.Workspaces/)).
<!-- * See [API Reference](docs/api/README.md#_top). -->

### Roslynator.CommandLine &ensp;[![NuGet](https://img.shields.io/nuget/v/Roslynator.CommandLine.svg)](https://nuget.org/packages/Roslynator.CommandLine)

* See [Roslynator Command-Line Interface](docs/cli/README.md#_top).

## Roslynator for VS Code

Currently VS Code does not support distribution of Roslyn-based tools in an extension.
Also it does not support analyzers at all.
Please read the [tutorial](docs/RoslynatorForVisualStudioCode.md) how to install refactorings and code fixes for CS diagnostics.

## Documentation

* [Analyzers vs. Refactorings](docs/AnalyzersVsRefactorings.md)
* [How to Configure Analyzers](docs/HowToConfigureAnalyzers.md)
* [How to Configure Refactorings](docs/HowToConfigureRefactorings.md)
* [How to Customize Rules for a Project](docs/HowToCustomizeRulesForProject.md)
* [How to Fix All Diagnostics in a Solution](docs/HowToFixAllDiagnostics.md)
* [How to Generate API Documentation](docs/HowToGenerateDocumentation.md)

## Other Projects

* [Snippetica](https://github.com/JosefPihrt/Snippetica) - A collection of snippets for C++, C#, HTML, JSON, Markdown, VB, XAML and XML
* [DotMarkdown](https://github.com/JosefPihrt/DotMarkdown) - Markdown framework for .NET
* [LINQ to Regex](https://github.com/JosefPihrt/LinqToRegex) - A library that provides language integrated access to .NET regular expressions
* [Snippet Manager](https://github.com/JosefPihrt/SnippetManager) - A library that enables to work with Visual Studio snippets
* [Regexator](http://pihrt.net/Regexator) - A comprehensive development environment for .NET regular expressions
