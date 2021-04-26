# CompilerDiagnosticFixTestData Class

[Home](../../../README.md) &#x2022; [Constructors](#constructors) &#x2022; [Properties](#properties) &#x2022; [Methods](#methods)

**Namespace**: [Roslynator.Testing](../README.md)

**Assembly**: Roslynator\.Testing\.Common\.dll

\
Represents test data for a compiler diagnostic fix\.

```csharp
public sealed class CompilerDiagnosticFixTestData
```

### Inheritance

[Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) &#x2192; CompilerDiagnosticFixTestData

## Constructors

| Constructor | Summary |
| ----------- | ------- |
| [CompilerDiagnosticFixTestData(String, String, IEnumerable\<AdditionalFile>, String)](-ctor/README.md) | Initializes a new instance of [CompilerDiagnosticFixTestData](./README.md) |

## Properties

| Property | Summary |
| -------- | ------- |
| [AdditionalFiles](AdditionalFiles/README.md) | Gets additional source files\. |
| [DiagnosticId](DiagnosticId/README.md) | Gets compiler diagnostic ID to be fixed\. |
| [EquivalenceKey](EquivalenceKey/README.md) | Gets code action's equivalence key\. |
| [Source](Source/README.md) | Gets a source code that will report specified compiler diagnostic\. |

## Methods

| Method | Summary |
| ------ | ------- |
| [Equals(Object)](https://docs.microsoft.com/en-us/dotnet/api/system.object.equals) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [GetHashCode()](https://docs.microsoft.com/en-us/dotnet/api/system.object.gethashcode) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [GetType()](https://docs.microsoft.com/en-us/dotnet/api/system.object.gettype) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [MemberwiseClone()](https://docs.microsoft.com/en-us/dotnet/api/system.object.memberwiseclone) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [ToString()](https://docs.microsoft.com/en-us/dotnet/api/system.object.tostring) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [Update(String, String, IEnumerable\<AdditionalFile>, String)](Update/README.md) | Creates and return new instance of [CompilerDiagnosticFixTestData](./README.md) updated with specified values\. |

