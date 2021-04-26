# DiagnosticTestData Class

[Home](../../../README.md) &#x2022; [Constructors](#constructors) &#x2022; [Properties](#properties) &#x2022; [Methods](#methods)

**Namespace**: [Roslynator.Testing](../README.md)

**Assembly**: Roslynator\.Testing\.Common\.dll

\
Represents test data for a diagnostic and its fix\.

```csharp
public sealed class DiagnosticTestData
```

### Inheritance

[Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) &#x2192; DiagnosticTestData

## Constructors

| Constructor | Summary |
| ----------- | ------- |
| [DiagnosticTestData(DiagnosticDescriptor, String, IEnumerable\<TextSpan>, IEnumerable\<TextSpan>, IEnumerable\<AdditionalFile>, String, IFormatProvider, String, Boolean)](-ctor/README.md) | Initializes a new instance of [DiagnosticTestData](./README.md)\. |

## Properties

| Property | Summary |
| -------- | ------- |
| [AdditionalFiles](AdditionalFiles/README.md) | Gets additional source files\. |
| [AdditionalSpans](AdditionalSpans/README.md) | Gets diagnostic's additional locations\. |
| [AlwaysVerifyAdditionalLocations](AlwaysVerifyAdditionalLocations/README.md) | True if additional locations should be always verified\. |
| [Descriptor](Descriptor/README.md) | Gets diagnostic's descriptor\. |
| [DiagnosticMessage](DiagnosticMessage/README.md) | Gets diagnostic's message |
| [EquivalenceKey](EquivalenceKey/README.md) | Gets code action's equivalence key\. |
| [FormatProvider](FormatProvider/README.md) | Gets format provider to be used to format diagnostic's message\. |
| [Source](Source/README.md) | Gets source that will report specified diagnostic\. |
| [Spans](Spans/README.md) | Gets diagnostic's locations\. |

## Methods

| Method | Summary |
| ------ | ------- |
| [Equals(Object)](https://docs.microsoft.com/en-us/dotnet/api/system.object.equals) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [GetHashCode()](https://docs.microsoft.com/en-us/dotnet/api/system.object.gethashcode) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [GetType()](https://docs.microsoft.com/en-us/dotnet/api/system.object.gettype) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [MemberwiseClone()](https://docs.microsoft.com/en-us/dotnet/api/system.object.memberwiseclone) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [ToString()](https://docs.microsoft.com/en-us/dotnet/api/system.object.tostring) |  \(Inherited from [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\) |
| [Update(DiagnosticDescriptor, String, IEnumerable\<TextSpan>, IEnumerable\<TextSpan>, IEnumerable\<AdditionalFile>, String, IFormatProvider, String, Boolean)](Update/README.md) | Creates and return new instance of [DiagnosticTestData](./README.md) updated with specified values\. |

