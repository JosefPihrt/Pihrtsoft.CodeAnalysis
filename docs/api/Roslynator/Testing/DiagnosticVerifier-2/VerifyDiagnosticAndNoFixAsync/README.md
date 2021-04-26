# DiagnosticVerifier\<TAnalyzer, TFixProvider>\.VerifyDiagnosticAndNoFixAsync\(DiagnosticTestData, TestOptions, CancellationToken\) Method

[Home](../../../../README.md)

**Containing Type**: [DiagnosticVerifier\<TAnalyzer, TFixProvider>](../README.md)

**Assembly**: Roslynator\.Testing\.Common\.dll

\
Verifies that specified source will produce specified diagnostic and that the diagnostic will not be fixed\.

```csharp
public System.Threading.Tasks.Task VerifyDiagnosticAndNoFixAsync(Roslynator.Testing.DiagnosticTestData data, Roslynator.Testing.TestOptions options = null, System.Threading.CancellationToken cancellationToken = default)
```

### Parameters

**data** &ensp; [DiagnosticTestData](../../DiagnosticTestData/README.md)

**options** &ensp; [TestOptions](../../TestOptions/README.md)

**cancellationToken** &ensp; [CancellationToken](https://docs.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken)

### Returns

[Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task)

