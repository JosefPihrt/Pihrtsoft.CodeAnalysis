# DiagnosticVerifier\<TAnalyzer, TFixProvider>\.VerifyNoDiagnosticAsync\(DiagnosticTestData, TestOptions, CancellationToken\) Method

[Home](../../../../README.md)

**Containing Type**: [DiagnosticVerifier\<TAnalyzer, TFixProvider>](../README.md)

**Assembly**: Roslynator\.Testing\.Common\.dll

\
Verifies that specified source will not produce specified diagnostic\.

```csharp
public System.Threading.Tasks.Task VerifyNoDiagnosticAsync(Roslynator.Testing.DiagnosticTestData data, Roslynator.Testing.TestOptions options = null, System.Threading.CancellationToken cancellationToken = default)
```

### Parameters

**data** &ensp; [DiagnosticTestData](../../DiagnosticTestData/README.md)

**options** &ensp; [TestOptions](../../TestOptions/README.md)

**cancellationToken** &ensp; [CancellationToken](https://docs.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken)

### Returns

[Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task)

