# CodeVerifier Class

[Home](../../../README.md) &#x2022; [Properties](#properties) &#x2022; [Methods](#methods)

**Namespace**: [Roslynator.Testing](../README.md)

**Assembly**: Roslynator\.Testing\.Common\.dll

\
Represents base type for verifying a diagnostic, a code fix and a refactoring\.

```csharp
public abstract class CodeVerifier
```

### Inheritance

[Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) &#x2192; CodeVerifier

### Derived

* [CompilerDiagnosticFixVerifier\<TFixProvider>](../CompilerDiagnosticFixVerifier-1/README.md)
* [DiagnosticVerifier\<TAnalyzer, TFixProvider>](../DiagnosticVerifier-2/README.md)
* [RefactoringVerifier\<TRefactoringProvider>](../RefactoringVerifier-1/README.md)

## Properties

| Property | Summary |
| -------- | ------- |
| [CommonOptions](CommonOptions/README.md) | Gets a common code verification options\. |
| [Options](Options/README.md) | Gets a code verification options\. |

## Methods

| Method | Summary |
| ------ | ------- |
| [Equals(Object)](https://docs.microsoft.com/en-us/dotnet/api/system.object.equals) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [GetHashCode()](https://docs.microsoft.com/en-us/dotnet/api/system.object.gethashcode) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [GetType()](https://docs.microsoft.com/en-us/dotnet/api/system.object.gettype) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [MemberwiseClone()](https://docs.microsoft.com/en-us/dotnet/api/system.object.memberwiseclone) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [ToString()](https://docs.microsoft.com/en-us/dotnet/api/system.object.tostring) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |

