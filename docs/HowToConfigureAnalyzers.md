# How to: Configure Analyzers

## Content

* [Configure Analyzers in a Ruleset File](#configure-analyzers-in-a-ruleset-file)
* [Configure Analyzers in an EditorConfig File](#configure-analyzers-in-an-editorconfig-file)
* [Change Default Configuration of Analyzers](#change-default-configuration-of-analyzers)
* [Suppress Diagnostics](#suppress-diagnostics)


## Configure Analyzers in a Ruleset File

* Ruleset is a group of rules where each rule define "Action" for a specific analyzer.
* Allowed actions are **None** (disabled), **Hidden** (not visible), **Info**, **Warning** or **Error**. 
* Rules are stored in a XML file that has extension **ruleset** and following structure:

```xml
<?xml version="1.0" encoding="utf-8"?>
<RuleSet Name="My Rules" ToolsVersion="15.0">

  <!-- Specify zero or more paths to other rulesets that should be included. -->
  <Include Path="parent.ruleset" Action="Default,None,Hidden,Info,Warning,Error" />

  <Rules AnalyzerId="Roslynator.CSharp.Analyzers" RuleNamespace="Roslynator.CSharp.Analyzers">
    <Rule Id="RCS...." Action="None,Hidden,Info,Warning,Error" />
  </Rules>

  <!-- Specify default action that should be applied to all analyzers except those explicitly specified. -->
  <IncludeAll Action="None,Hidden,Info,Warning,Error" />

</RuleSet>
```

To enforce ruleset it is necessary to reference it in a csproj file:

```xml
<PropertyGroup>
  <CodeAnalysisRuleSet>relative_or_absolute_path_to_ruleset_file</CodeAnalysisRuleSet>
</PropertyGroup>
```

Please see step-by-step tutorial [how to configure ruleset file](HowToConfigureRulesetFile.md).


## Configure Analyzers in an EditorConfig File

*Note: This option is applicable for Visual Studio 2019 version 16.3 and later.*

Severity of a rule can be changed by adding following line to an EditorConfig file:
```
dotnet_diagnostic.<RULE_ID>.severity = <default|none|silent|info|warning|error>
```

For further information please see how to [set rule severity in an EditorConfig file](https://docs.microsoft.com/en-us/visualstudio/code-quality/use-roslyn-analyzers#set-rule-severity-in-an-editorconfig-file).


## Change Default Configuration of Analyzers

*Note: This option is applicable for Roslynator 2019 or later (Visual Studio) and for Roslynator for VS Code.*

Roslynator ruleset file can be used to:

 1) Enable/disable analyzer(s) by DEFAULT.
 2) Change DEFAULT severity (action) of the analyzer(s).
 
Default configuration is applied once when analyzers are loaded.
Therefore, it may be necessary to restart IDE for changes to take effect.

### Location of Roslynator Ruleset File

Roslynator ruleset file must be named **roslynator.ruleset** and must be located in one of the following folders:

| Folder Path | Scope |
| -------- | ------- |
| `LOCAL_APP_DATA/JosefPihrt/Roslynator` | Visual Studio installations and VS Code installation |
| `LOCAL_APP_DATA/JosefPihrt/Roslynator/VisualStudio` | Visual Studio installations |
| `LOCAL_APP_DATA/JosefPihrt/Roslynator/VisualStudio/2019` | Visual Studio 2019 installations |
| `LOCAL_APP_DATA/JosefPihrt/Roslynator/VisualStudioCode` | VS Code installation |

### Location of LOCAL_APP_DATA Folder

| OS | Path |
| -------- | ------- |
| Windows | `C:\Users\USER_NAME\AppData\Local` |
| Linux | `/home/USER_NAME/.local/share` |
| OSX | `/Users/USER_NAME/.local/share` |


## Suppress Diagnostics

Suppression of diagnostics is useful to suppress rare cases that are not or cannot be covered by an analyzer.

This approach should not be used as a replacement for configuration of analyzers since analyzers that produce diagnostics still execute even if diagnostics are suppressed.

### Suppress Diagnostic for a Declaration

```csharp
using System.Diagnostics.CodeAnalysis;

class C
{
    [SuppressMessage("Readability", "RCS1008", Justification = "<Pending>")]
    void M()
    {
        var x = Foo(); // no RCS1008 here
    }
}
```

```csharp
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Readability", "RCS1008", Justification = "<Pending>", Scope = "member", Target = "~M:C.M")]

class C
{
    void M()
    {
        var x = Foo(); // no RCS1008 here
    }
}
```

### Suppress Diagnostic for Selected Lines

```csharp
using System.Diagnostics.CodeAnalysis;

class C
{
    void M()
    {
#pragma warning disable RCS1008
        var x = Foo(); // no RCS1008 here
#pragma warning restore RCS1008
    }
}
```

### Suppress Diagnostic for Namespace

```csharp
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Readability", "RCS1008", Justification = "<Pending>", Scope = "NamespaceAndDescendants", Target = "N1.N2")]

namespace N1.N2
{
    class C
    {
        void M()
        {
            var x = Foo(); // no RCS1008 here
        }
    }
}
```

### Suppress Diagnostic Globally

*Note: this option is applicable for Roslynator 2017*

Go to Visual Studio Tools > Options > Roslynator > Global Suppressions

![Global Suppressions](/images/GlobalSuppressionsOptions.png)

## See Also

* [Use code analyzers](https://docs.microsoft.com/en-us/visualstudio/code-quality/use-roslyn-analyzers)
