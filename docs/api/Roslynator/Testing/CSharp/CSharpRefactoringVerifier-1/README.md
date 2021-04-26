# CSharpRefactoringVerifier\<TRefactoringProvider> Class

[Home](../../../../README.md) &#x2022; [Properties](#properties) &#x2022; [Methods](#methods)

**Namespace**: [Roslynator.Testing.CSharp](../README.md)

**Assembly**: Roslynator\.Testing\.CSharp\.dll

\
Represents verifier for a C\# code refactoring\.

```csharp
public abstract class CSharpRefactoringVerifier<TRefactoringProvider> : Roslynator.Testing.RefactoringVerifier<TRefactoringProvider> where TRefactoringProvider : Microsoft.CodeAnalysis.CodeRefactorings.CodeRefactoringProvider, new()
```

### Type Parameters

**TRefactoringProvider**

### Inheritance

[Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) &#x2192; [CodeVerifier](../../CodeVerifier/README.md) &#x2192; [RefactoringVerifier\<TRefactoringProvider>](../../RefactoringVerifier-1/README.md) &#x2192; CSharpRefactoringVerifier\<TRefactoringProvider>

### Derived

* [XunitRefactoringVerifier\<TRefactoringProvider>](../Xunit/XunitRefactoringVerifier-1/README.md)

## Properties

| Property | Summary |
| -------- | ------- |
| [CommonOptions](CommonOptions/README.md) | Gets common test options\. \(Overrides [CodeVerifier.CommonOptions](../../CodeVerifier/CommonOptions/README.md)\) |
| [Options](Options/README.md) | Gets a test options\. |

## Methods

| Method | Summary |
| ------ | ------- |
| [Equals(Object)](https://docs.microsoft.com/en-us/dotnet/api/system.object.equals) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [GetHashCode()](https://docs.microsoft.com/en-us/dotnet/api/system.object.gethashcode) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [GetType()](https://docs.microsoft.com/en-us/dotnet/api/system.object.gettype) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [MemberwiseClone()](https://docs.microsoft.com/en-us/dotnet/api/system.object.memberwiseclone) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [ToString()](https://docs.microsoft.com/en-us/dotnet/api/system.object.tostring) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [VerifyNoRefactoringAsync(RefactoringTestData, TestOptions, CancellationToken)](../../RefactoringVerifier-1/VerifyNoRefactoringAsync/README.md) | Verifies that refactoring will not be applied using specified **TRefactoringProvider**\. \(Inherited from [RefactoringVerifier\<TRefactoringProvider>](../../RefactoringVerifier-1/README.md)\) |
| [VerifyRefactoringAsync(RefactoringTestData, ExpectedTestState, TestOptions, CancellationToken)](../../RefactoringVerifier-1/VerifyRefactoringAsync/README.md) | Verifies that refactoring will be applied correctly using specified **TRefactoringProvider**\. \(Inherited from [RefactoringVerifier\<TRefactoringProvider>](../../RefactoringVerifier-1/README.md)\) |

