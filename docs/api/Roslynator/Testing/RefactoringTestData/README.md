# RefactoringTestData Class

[Home](../../../README.md) &#x2022; [Constructors](#constructors) &#x2022; [Properties](#properties) &#x2022; [Methods](#methods)

**Namespace**: [Roslynator.Testing](../README.md)

**Assembly**: Roslynator\.Testing\.Common\.dll

\
Gets test data for a code refactoring\.

```csharp
public sealed class RefactoringTestData
```

### Inheritance

[Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) &#x2192; RefactoringTestData

## Constructors

| Constructor | Summary |
| ----------- | ------- |
| [RefactoringTestData(String, IEnumerable\<TextSpan>, IEnumerable\<AdditionalFile>, String)](-ctor/README.md) | Initializes a new instance of [RefactoringTestData](./README.md)\. |

## Properties

| Property | Summary |
| -------- | ------- |
| [AdditionalFiles](AdditionalFiles/README.md) | Gets additional source files\. |
| [EquivalenceKey](EquivalenceKey/README.md) | Gets code action's equivalence key\. |
| [Source](Source/README.md) | Gets a source code to be refactored\. |
| [Spans](Spans/README.md) | Gets text spans on which a code refactoring will be applied\. |

## Methods

| Method | Summary |
| ------ | ------- |
| [Equals(Object)](https://docs.microsoft.com/en-us/dotnet/api/system.object.equals) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [GetHashCode()](https://docs.microsoft.com/en-us/dotnet/api/system.object.gethashcode) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [GetType()](https://docs.microsoft.com/en-us/dotnet/api/system.object.gettype) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [MemberwiseClone()](https://docs.microsoft.com/en-us/dotnet/api/system.object.memberwiseclone) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [ToString()](https://docs.microsoft.com/en-us/dotnet/api/system.object.tostring) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [Update(String, IEnumerable\<TextSpan>, IEnumerable\<AdditionalFile>, String)](Update/README.md) | Creates and return new instance of [RefactoringTestData](./README.md) updated with specified values\. |

