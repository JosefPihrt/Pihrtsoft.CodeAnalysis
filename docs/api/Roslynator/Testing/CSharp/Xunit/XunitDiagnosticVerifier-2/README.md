# XunitDiagnosticVerifier\<TAnalyzer, TFixProvider> Class

[Home](../../../../../README.md) &#x2022; [Constructors](#constructors) &#x2022; [Properties](#properties) &#x2022; [Methods](#methods)

**Namespace**: [Roslynator.Testing.CSharp.Xunit](../README.md)

**Assembly**: Roslynator\.Testing\.CSharp\.Xunit\.dll

\
Represents a verifier for a C\# diagnostic that is produced by [DiagnosticAnalyzer](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.diagnostics.diagnosticanalyzer)\.

```csharp
public abstract class XunitDiagnosticVerifier<TAnalyzer, TFixProvider> : Roslynator.Testing.CSharp.CSharpDiagnosticVerifier<TAnalyzer, TFixProvider>
    where TAnalyzer : Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer, new() 
    where TFixProvider : Microsoft.CodeAnalysis.CodeFixes.CodeFixProvider, new()
```

### Type Parameters

**TAnalyzer**

**TFixProvider**

### Inheritance

[Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) &#x2192; [CodeVerifier](../../../CodeVerifier/README.md) &#x2192; [DiagnosticVerifier\<TAnalyzer, TFixProvider>](../../../DiagnosticVerifier-2/README.md) &#x2192; [CSharpDiagnosticVerifier\<TAnalyzer, TFixProvider>](../../CSharpDiagnosticVerifier-2/README.md) &#x2192; XunitDiagnosticVerifier\<TAnalyzer, TFixProvider>

## Constructors

| Constructor | Summary |
| ----------- | ------- |
| [XunitDiagnosticVerifier()](-ctor/README.md) | Initializes a new instance of [XunitDiagnosticVerifier\<TAnalyzer, TFixProvider>](./README.md)\. |

## Properties

| Property | Summary |
| -------- | ------- |
| [CommonOptions](../../CSharpDiagnosticVerifier-2/CommonOptions/README.md) | Gets common test options\. \(Inherited from [CSharpDiagnosticVerifier\<TAnalyzer, TFixProvider>](../../CSharpDiagnosticVerifier-2/README.md)\) |
| [Options](../../CSharpDiagnosticVerifier-2/Options/README.md) | Gets a test options\. \(Inherited from [CSharpDiagnosticVerifier\<TAnalyzer, TFixProvider>](../../CSharpDiagnosticVerifier-2/README.md)\) |

## Methods

| Method | Summary |
| ------ | ------- |
| [Equals(Object)](https://docs.microsoft.com/en-us/dotnet/api/system.object.equals) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [GetHashCode()](https://docs.microsoft.com/en-us/dotnet/api/system.object.gethashcode) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [GetType()](https://docs.microsoft.com/en-us/dotnet/api/system.object.gettype) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [MemberwiseClone()](https://docs.microsoft.com/en-us/dotnet/api/system.object.memberwiseclone) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [ToString()](https://docs.microsoft.com/en-us/dotnet/api/system.object.tostring) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [VerifyDiagnosticAndFixAsync(DiagnosticTestData, ExpectedTestState, TestOptions, CancellationToken)](../../../DiagnosticVerifier-2/VerifyDiagnosticAndFixAsync/README.md) | Verifies that specified source will produce specified diagnostic and that the diagnostic will be fixed\. \(Inherited from [DiagnosticVerifier\<TAnalyzer, TFixProvider>](../../../DiagnosticVerifier-2/README.md)\) |
| [VerifyDiagnosticAndNoFixAsync(DiagnosticTestData, TestOptions, CancellationToken)](../../../DiagnosticVerifier-2/VerifyDiagnosticAndNoFixAsync/README.md) | Verifies that specified source will produce specified diagnostic and that the diagnostic will not be fixed\. \(Inherited from [DiagnosticVerifier\<TAnalyzer, TFixProvider>](../../../DiagnosticVerifier-2/README.md)\) |
| [VerifyDiagnosticAsync(DiagnosticTestData, TestOptions, CancellationToken)](../../../DiagnosticVerifier-2/VerifyDiagnosticAsync/README.md) | Verifies that specified source will produce specified diagnostic\(s\)\. \(Inherited from [DiagnosticVerifier\<TAnalyzer, TFixProvider>](../../../DiagnosticVerifier-2/README.md)\) |
| [VerifyNoDiagnosticAsync(DiagnosticTestData, TestOptions, CancellationToken)](../../../DiagnosticVerifier-2/VerifyNoDiagnosticAsync/README.md) | Verifies that specified source will not produce specified diagnostic\. \(Inherited from [DiagnosticVerifier\<TAnalyzer, TFixProvider>](../../../DiagnosticVerifier-2/README.md)\) |

