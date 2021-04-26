# TestOptions Class

[Home](../../../README.md) &#x2022; [Properties](#properties) &#x2022; [Methods](#methods)

**Namespace**: [Roslynator.Testing](../README.md)

**Assembly**: Roslynator\.Testing\.Common\.dll

\
Represents options for a code verifier\.

```csharp
public abstract class TestOptions
```

### Inheritance

[Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) &#x2192; TestOptions

### Derived

* [CSharpTestOptions](../CSharp/CSharpTestOptions/README.md)

## Properties

| Property | Summary |
| -------- | ------- |
| [AllowedCompilerDiagnosticIds](AllowedCompilerDiagnosticIds/README.md) | Gets a collection of compiler diagnostic IDs\. |
| [AllowedCompilerDiagnosticSeverity](AllowedCompilerDiagnosticSeverity/README.md) | Gets a maximal allowed compiler diagnostic severity\. |
| [CommonCompilationOptions](CommonCompilationOptions/README.md) | Gets a common compilation options\. |
| [CommonParseOptions](CommonParseOptions/README.md) | Gets a common parse options\. |
| [CompilationOptions](CompilationOptions/README.md) | Gets a compilation options that should be used to compile test project\. |
| [Language](Language/README.md) | Gets a programming language identifier\. |
| [MetadataReferences](MetadataReferences/README.md) | Gets metadata references of a test project\. |
| [ParseOptions](ParseOptions/README.md) | Gets a parse options that should be used to parse tested source code\. |

## Methods

| Method | Summary |
| ------ | ------- |
| [CommonWithAllowedCompilerDiagnosticIds(IEnumerable\<String>)](CommonWithAllowedCompilerDiagnosticIds/README.md) | |
| [CommonWithAllowedCompilerDiagnosticSeverity(DiagnosticSeverity)](CommonWithAllowedCompilerDiagnosticSeverity/README.md) | |
| [CommonWithMetadataReferences(IEnumerable\<MetadataReference>)](CommonWithMetadataReferences/README.md) | |
| [Equals(Object)](https://docs.microsoft.com/en-us/dotnet/api/system.object.equals) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [GetHashCode()](https://docs.microsoft.com/en-us/dotnet/api/system.object.gethashcode) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [GetType()](https://docs.microsoft.com/en-us/dotnet/api/system.object.gettype) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [MemberwiseClone()](https://docs.microsoft.com/en-us/dotnet/api/system.object.memberwiseclone) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [ToString()](https://docs.microsoft.com/en-us/dotnet/api/system.object.tostring) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [WithAllowedCompilerDiagnosticIds(IEnumerable\<String>)](WithAllowedCompilerDiagnosticIds/README.md) | |
| [WithAllowedCompilerDiagnosticSeverity(DiagnosticSeverity)](WithAllowedCompilerDiagnosticSeverity/README.md) | |
| [WithMetadataReferences(IEnumerable\<MetadataReference>)](WithMetadataReferences/README.md) | |

