# RefactoringVerifier\<TRefactoringProvider> Class

[Home](../../../README.md) &#x2022; [Properties](#properties) &#x2022; [Methods](#methods)

**Namespace**: [Roslynator.Testing](../README.md)

**Assembly**: Roslynator\.Testing\.Common\.dll

\
Represents verifier for a code refactoring\.

```csharp
public abstract class RefactoringVerifier<TRefactoringProvider> : Roslynator.Testing.CodeVerifier where TRefactoringProvider : Microsoft.CodeAnalysis.CodeRefactorings.CodeRefactoringProvider, new()
```

### Type Parameters

**TRefactoringProvider**

### Inheritance

[Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) &#x2192; [CodeVerifier](../CodeVerifier/README.md) &#x2192; RefactoringVerifier\<TRefactoringProvider>

### Derived

* [CSharpRefactoringVerifier\<TRefactoringProvider>](../CSharp/CSharpRefactoringVerifier-1/README.md)

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
| [VerifyNoRefactoringAsync(RefactoringTestData, TestOptions, CancellationToken)](VerifyNoRefactoringAsync/README.md) | Verifies that refactoring will not be applied using specified **TRefactoringProvider**\. |
| [VerifyRefactoringAsync(RefactoringTestData, ExpectedTestState, TestOptions, CancellationToken)](VerifyRefactoringAsync/README.md) | Verifies that refactoring will be applied correctly using specified **TRefactoringProvider**\. |

