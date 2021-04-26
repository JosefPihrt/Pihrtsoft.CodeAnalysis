# CSharpTestOptions Class

[Home](../../../../README.md) &#x2022; [Constructors](#constructors) &#x2022; [Properties](#properties) &#x2022; [Methods](#methods)

**Namespace**: [Roslynator.Testing.CSharp](../README.md)

**Assembly**: Roslynator\.Testing\.CSharp\.dll

\
Represents options for a C\# code verifier\.

```csharp
public sealed class CSharpTestOptions : Roslynator.Testing.TestOptions
```

### Inheritance

[Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) &#x2192; [TestOptions](../../TestOptions/README.md) &#x2192; CSharpTestOptions

## Constructors

| Constructor | Summary |
| ----------- | ------- |
| [CSharpTestOptions(CSharpCompilationOptions, CSharpParseOptions, IEnumerable\<MetadataReference>, IEnumerable\<String>, DiagnosticSeverity)](-ctor/README.md) | Initializes a new instance of [CSharpTestOptions](./README.md)\. |

## Properties

| Property | Summary |
| -------- | ------- |
| [AllowedCompilerDiagnosticIds](../../TestOptions/AllowedCompilerDiagnosticIds/README.md) | Gets a collection of compiler diagnostic IDs\. \(Inherited from [TestOptions](../../TestOptions/README.md)\) |
| [AllowedCompilerDiagnosticSeverity](../../TestOptions/AllowedCompilerDiagnosticSeverity/README.md) | Gets a maximal allowed compiler diagnostic severity\. \(Inherited from [TestOptions](../../TestOptions/README.md)\) |
| [CommonCompilationOptions](CommonCompilationOptions/README.md) | Gets a common compilation options\. \(Overrides [TestOptions.CommonCompilationOptions](../../TestOptions/CommonCompilationOptions/README.md)\) |
| [CommonParseOptions](CommonParseOptions/README.md) | Gets a common parse options\. \(Overrides [TestOptions.CommonParseOptions](../../TestOptions/CommonParseOptions/README.md)\) |
| [CompilationOptions](CompilationOptions/README.md) | Gets a compilation options that should be used to compile test project\. |
| [Default](Default/README.md) | Gets a default code verification options\. |
| [Language](Language/README.md) | Gets C\# programming language identifier\. \(Overrides [TestOptions.Language](../../TestOptions/Language/README.md)\) |
| [MetadataReferences](../../TestOptions/MetadataReferences/README.md) | Gets metadata references of a test project\. \(Inherited from [TestOptions](../../TestOptions/README.md)\) |
| [ParseOptions](ParseOptions/README.md) | Gets a parse options that should be used to parse tested source code\. |

## Methods

| Method | Summary |
| ------ | ------- |
| [AddAllowedCompilerDiagnosticId(String)](AddAllowedCompilerDiagnosticId/README.md) | Adds specified compiler diagnostic ID to the list of allowed compiler diagnostic IDs\. |
| [AddAllowedCompilerDiagnosticIds(IEnumerable\<String>)](AddAllowedCompilerDiagnosticIds/README.md) | Adds a list of specified compiler diagnostic IDs to the list of allowed compiler diagnostic IDs\. |
| [CommonWithAllowedCompilerDiagnosticIds(IEnumerable\<String>)](CommonWithAllowedCompilerDiagnosticIds/README.md) |  \(Overrides [TestOptions.CommonWithAllowedCompilerDiagnosticIds](../../TestOptions/CommonWithAllowedCompilerDiagnosticIds/README.md)\) |
| [CommonWithAllowedCompilerDiagnosticSeverity(DiagnosticSeverity)](CommonWithAllowedCompilerDiagnosticSeverity/README.md) |  \(Overrides [TestOptions.CommonWithAllowedCompilerDiagnosticSeverity](../../TestOptions/CommonWithAllowedCompilerDiagnosticSeverity/README.md)\) |
| [CommonWithMetadataReferences(IEnumerable\<MetadataReference>)](CommonWithMetadataReferences/README.md) |  \(Overrides [TestOptions.CommonWithMetadataReferences](../../TestOptions/CommonWithMetadataReferences/README.md)\) |
| [Equals(Object)](https://docs.microsoft.com/en-us/dotnet/api/system.object.equals) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [GetHashCode()](https://docs.microsoft.com/en-us/dotnet/api/system.object.gethashcode) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [GetType()](https://docs.microsoft.com/en-us/dotnet/api/system.object.gettype) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [MemberwiseClone()](https://docs.microsoft.com/en-us/dotnet/api/system.object.memberwiseclone) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [ToString()](https://docs.microsoft.com/en-us/dotnet/api/system.object.tostring) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [WithAllowedCompilerDiagnosticIds(IEnumerable\<String>)](WithAllowedCompilerDiagnosticIds/README.md) | |
| [WithAllowedCompilerDiagnosticSeverity(DiagnosticSeverity)](WithAllowedCompilerDiagnosticSeverity/README.md) | |
| [WithCompilationOptions(CSharpCompilationOptions)](WithCompilationOptions/README.md) | |
| [WithMetadataReferences(IEnumerable\<MetadataReference>)](WithMetadataReferences/README.md) | |
| [WithParseOptions(CSharpParseOptions)](WithParseOptions/README.md) | |

