# DiagnosticVerifier\<TAnalyzer, TFixProvider> Class

[Home](../../../README.md) &#x2022; [Properties](#properties) &#x2022; [Methods](#methods)

**Namespace**: [Roslynator.Testing](../README.md)

**Assembly**: Roslynator\.Testing\.Common\.dll

\
Represents verifier for a diagnostic that is produced by [DiagnosticAnalyzer](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.diagnostics.diagnosticanalyzer)\.

```csharp
public abstract class DiagnosticVerifier<TAnalyzer, TFixProvider> : Roslynator.Testing.CodeVerifier
    where TAnalyzer : Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer, new() 
    where TFixProvider : Microsoft.CodeAnalysis.CodeFixes.CodeFixProvider, new()
```

### Type Parameters

**TAnalyzer**

**TFixProvider**

### Inheritance

[Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) &#x2192; [CodeVerifier](../CodeVerifier/README.md) &#x2192; DiagnosticVerifier\<TAnalyzer, TFixProvider>

### Derived

* [CSharpDiagnosticVerifier\<TAnalyzer, TFixProvider>](../CSharp/CSharpDiagnosticVerifier-2/README.md)

## Properties

| Property | Summary |
| -------- | ------- |
| [CommonOptions](../CodeVerifier/CommonOptions/README.md) | Gets a common code verification options\. \(Inherited from [CodeVerifier](../CodeVerifier/README.md)\) |
| [Options](../CodeVerifier/Options/README.md) | Gets a code verification options\. \(Inherited from [CodeVerifier](../CodeVerifier/README.md)\) |

## Methods

| Method | Summary |
| ------ | ------- |
| [Equals(Object)](https://docs.microsoft.com/en-us/dotnet/api/system.object.equals) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [GetHashCode()](https://docs.microsoft.com/en-us/dotnet/api/system.object.gethashcode) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [GetType()](https://docs.microsoft.com/en-us/dotnet/api/system.object.gettype) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [MemberwiseClone()](https://docs.microsoft.com/en-us/dotnet/api/system.object.memberwiseclone) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [ToString()](https://docs.microsoft.com/en-us/dotnet/api/system.object.tostring) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [VerifyDiagnosticAndFixAsync(DiagnosticTestData, ExpectedTestState, TestOptions, CancellationToken)](VerifyDiagnosticAndFixAsync/README.md) | Verifies that specified source will produce specified diagnostic and that the diagnostic will be fixed\. |
| [VerifyDiagnosticAndNoFixAsync(DiagnosticTestData, TestOptions, CancellationToken)](VerifyDiagnosticAndNoFixAsync/README.md) | Verifies that specified source will produce specified diagnostic and that the diagnostic will not be fixed\. |
| [VerifyDiagnosticAsync(DiagnosticTestData, TestOptions, CancellationToken)](VerifyDiagnosticAsync/README.md) | Verifies that specified source will produce specified diagnostic\(s\)\. |
| [VerifyNoDiagnosticAsync(DiagnosticTestData, TestOptions, CancellationToken)](VerifyNoDiagnosticAsync/README.md) | Verifies that specified source will not produce specified diagnostic\. |

