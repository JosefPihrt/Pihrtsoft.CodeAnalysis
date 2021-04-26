# XunitCompilerDiagnosticFixVerifier\<TFixProvider> Class

[Home](../../../../../README.md) &#x2022; [Constructors](#constructors) &#x2022; [Properties](#properties) &#x2022; [Methods](#methods)

**Namespace**: [Roslynator.Testing.CSharp.Xunit](../README.md)

**Assembly**: Roslynator\.Testing\.CSharp\.Xunit\.dll

\
Represents a verifier for C\# compiler diagnostics\.

```csharp
public abstract class XunitCompilerDiagnosticFixVerifier<TFixProvider> : Roslynator.Testing.CSharp.CSharpCompilerDiagnosticFixVerifier<TFixProvider> where TFixProvider : Microsoft.CodeAnalysis.CodeFixes.CodeFixProvider, new()
```

### Type Parameters

**TFixProvider**

### Inheritance

[Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) &#x2192; [CodeVerifier](../../../CodeVerifier/README.md) &#x2192; [CompilerDiagnosticFixVerifier\<TFixProvider>](../../../CompilerDiagnosticFixVerifier-1/README.md) &#x2192; [CSharpCompilerDiagnosticFixVerifier\<TFixProvider>](../../CSharpCompilerDiagnosticFixVerifier-1/README.md) &#x2192; XunitCompilerDiagnosticFixVerifier\<TFixProvider>

## Constructors

| Constructor | Summary |
| ----------- | ------- |
| [XunitCompilerDiagnosticFixVerifier()](-ctor/README.md) | Initializes a new instance of [XunitCompilerDiagnosticFixVerifier\<TFixProvider>](./README.md) |

## Properties

| Property | Summary |
| -------- | ------- |
| [CommonOptions](../../CSharpCompilerDiagnosticFixVerifier-1/CommonOptions/README.md) | Gets common test options\. \(Inherited from [CSharpCompilerDiagnosticFixVerifier\<TFixProvider>](../../CSharpCompilerDiagnosticFixVerifier-1/README.md)\) |
| [Options](../../CSharpCompilerDiagnosticFixVerifier-1/Options/README.md) | Gets a test options\. \(Inherited from [CSharpCompilerDiagnosticFixVerifier\<TFixProvider>](../../CSharpCompilerDiagnosticFixVerifier-1/README.md)\) |

## Methods

| Method | Summary |
| ------ | ------- |
| [Equals(Object)](https://docs.microsoft.com/en-us/dotnet/api/system.object.equals) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [GetHashCode()](https://docs.microsoft.com/en-us/dotnet/api/system.object.gethashcode) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [GetType()](https://docs.microsoft.com/en-us/dotnet/api/system.object.gettype) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [MemberwiseClone()](https://docs.microsoft.com/en-us/dotnet/api/system.object.memberwiseclone) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [ToString()](https://docs.microsoft.com/en-us/dotnet/api/system.object.tostring) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [VerifyFixAsync(CompilerDiagnosticFixTestData, ExpectedTestState, TestOptions, CancellationToken)](../../../CompilerDiagnosticFixVerifier-1/VerifyFixAsync/README.md) | Verifies that specified source will produce compiler diagnostic\. \(Inherited from [CompilerDiagnosticFixVerifier\<TFixProvider>](../../../CompilerDiagnosticFixVerifier-1/README.md)\) |
| [VerifyNoFixAsync(CompilerDiagnosticFixTestData, TestOptions, CancellationToken)](../../../CompilerDiagnosticFixVerifier-1/VerifyNoFixAsync/README.md) | Verifies that specified source will not produce compiler diagnostic\. \(Inherited from [CompilerDiagnosticFixVerifier\<TFixProvider>](../../../CompilerDiagnosticFixVerifier-1/README.md)\) |

