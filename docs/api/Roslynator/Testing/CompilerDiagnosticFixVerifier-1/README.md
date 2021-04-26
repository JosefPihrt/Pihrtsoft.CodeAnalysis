# CompilerDiagnosticFixVerifier\<TFixProvider> Class

[Home](../../../README.md) &#x2022; [Properties](#properties) &#x2022; [Methods](#methods)

**Namespace**: [Roslynator.Testing](../README.md)

**Assembly**: Roslynator\.Testing\.Common\.dll

\
Represents a verifier for compiler diagnostic\.

```csharp
public abstract class CompilerDiagnosticFixVerifier<TFixProvider> : Roslynator.Testing.CodeVerifier where TFixProvider : Microsoft.CodeAnalysis.CodeFixes.CodeFixProvider, new()
```

### Type Parameters

**TFixProvider**

### Inheritance

[Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) &#x2192; [CodeVerifier](../CodeVerifier/README.md) &#x2192; CompilerDiagnosticFixVerifier\<TFixProvider>

### Derived

* [CSharpCompilerDiagnosticFixVerifier\<TFixProvider>](../CSharp/CSharpCompilerDiagnosticFixVerifier-1/README.md)

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
| [VerifyFixAsync(CompilerDiagnosticFixTestData, ExpectedTestState, TestOptions, CancellationToken)](VerifyFixAsync/README.md) | Verifies that specified source will produce compiler diagnostic\. |
| [VerifyNoFixAsync(CompilerDiagnosticFixTestData, TestOptions, CancellationToken)](VerifyNoFixAsync/README.md) | Verifies that specified source will not produce compiler diagnostic\. |

