# DiagnosticTestData\(DiagnosticDescriptor, String, IEnumerable\<TextSpan>, IEnumerable\<TextSpan>, IEnumerable\<AdditionalFile>, String, IFormatProvider, String, Boolean\) Constructor

[Home](../../../../README.md)

**Containing Type**: [DiagnosticTestData](../README.md)

**Assembly**: Roslynator\.Testing\.Common\.dll

\
Initializes a new instance of [DiagnosticTestData](../README.md)\.

```csharp
public DiagnosticTestData(Microsoft.CodeAnalysis.DiagnosticDescriptor descriptor, string source, System.Collections.Generic.IEnumerable<Microsoft.CodeAnalysis.Text.TextSpan> spans, System.Collections.Generic.IEnumerable<Microsoft.CodeAnalysis.Text.TextSpan> additionalSpans = null, System.Collections.Generic.IEnumerable<Roslynator.Testing.AdditionalFile> additionalFiles = null, string diagnosticMessage = null, IFormatProvider formatProvider = null, string equivalenceKey = null, bool alwaysVerifyAdditionalLocations = false)
```

### Parameters

**descriptor** &ensp; [DiagnosticDescriptor](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.diagnosticdescriptor)

**source** &ensp; [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)

**spans** &ensp; [IEnumerable](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1)\<[TextSpan](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.text.textspan)>

**additionalSpans** &ensp; [IEnumerable](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1)\<[TextSpan](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.text.textspan)>

**additionalFiles** &ensp; [IEnumerable](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1)\<[AdditionalFile](../../AdditionalFile/README.md)>

**diagnosticMessage** &ensp; [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)

**formatProvider** &ensp; [IFormatProvider](https://docs.microsoft.com/en-us/dotnet/api/system.iformatprovider)

**equivalenceKey** &ensp; [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)

**alwaysVerifyAdditionalLocations** &ensp; [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)