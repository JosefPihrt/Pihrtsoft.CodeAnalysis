# Roslynator for Visual Studio Code

A collection of 500+ [analyzers](https://github.com/JosefPihrt/Roslynator/blob/master/src/Analyzers/README.md), [refactorings](https://github.com/JosefPihrt/Roslynator/blob/master/src/Refactorings/README.md) and [fixes](https://github.com/JosefPihrt/Roslynator/blob/master/src/CodeFixes/README.md) for C#, powered by [Roslyn](https://github.com/dotnet/roslyn).

For further information please with Roslynator [repo](https://github.com/JosefPihrt/Roslynator).

## Configuration of Analyzers

Standard rulesets are used to configure analyzers on a project-wide basis.

If you want to configure analyzers on a user-wide basis you have to use Roslynator ruleset.

How to open ruleset:

1) Press Ctrl + Shift + P
2) Type "roslynator"
3) Select "Roslynator: Open Configuration of Analyzers (roslynator.ruleset)"

Ruleset can be used to:

1. Enable/disable analyzer(s) by DEFAULT.
2. Change DEFAULT severity (action) of the analyzer(s).

Ruleset is applied once when the extension is loaded. Therefore, it may be necessary to restart IDE for changes to take effect.

## Configuration of Refactorings and Fixes

How to open config file:

1) Press Ctrl + Shift + P
2) Type "roslynator"
3) Select "Roslynator: Open Configuration of Refactorings and Fixes (roslynator.config)"

Config file is applied once when the extension is loaded. Therefore, it may be necessary to restart IDE for changes to take effect.

Full list of refactorings identifiers can be found [here](https://github.com/JosefPihrt/Roslynator/blob/master/src/Refactorings/README.md). Full list of fixes identifiers can be found [here](https://github.com/JosefPihrt/Roslynator/blob/master/src/CodeFixes/README.md).

## Location of Configuration Files

Configuration files are located at `%LOCALAPPDATA%\JosefPihrt\Roslynator\VisualStudioCode`.
Location of `%LOCALAPPDATA%` depends on the operating system:

| OS | Path |
| -------- | ------- |
| Windows | `C:\Users\JohnDoe\AppData\Local` |
| Linux | `/home/JohnDoe/.local/share` |
| OSX | `/Users/JohnDoe/.local/share` |

Ruleset file is located at **%LOCALAPPDATA%\JosefPihrt\Roslynator\VisualStudioCode\roslynator.ruleset**.
Config file is located at **%LOCALAPPDATA%\JosefPihrt\Roslynator\VisualStudioCode\roslynator.config**.

## Requirements

This extension requires [C# for Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp) 1.21.13 or higher.

## Donation

Although Roslynator products are free of charge, any [donation](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=BX85UA346VTN6) is welcome and supports further development.

## Thanks

* Thanks to [Pekka Savolainen](https://github.com/savpek) who pioneered the way for Roslyn analyzers on Visual Studio Code.
* Thanks to [Adrian Wilczynski](https://github.com/AdrianWilczynski) who added several great [PRs](https://github.com/JosefPihrt/Roslynator/pulls?q=author%3AAdrianWilczynski).
