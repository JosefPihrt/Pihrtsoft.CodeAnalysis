# DiagnosticVerifier\<TAnalyzer, TFixProvider>\.VerifyDiagnosticAndFixAsync\(DiagnosticTestData, ExpectedTestState, TestOptions, CancellationToken\) Method

[Home](../../../../README.md)

**Containing Type**: [DiagnosticVerifier\<TAnalyzer, TFixProvider>](../README.md)

**Assembly**: Roslynator\.Testing\.Common\.dll

\
Verifies that specified source will produce specified diagnostic and that the diagnostic will be fixed\.

```csharp
public System.Threading.Tasks.Task VerifyDiagnosticAndFixAsync(Roslynator.Testing.DiagnosticTestData data, Roslynator.Testing.ExpectedTestState expected, Roslynator.Testing.TestOptions options = null, System.Threading.CancellationToken cancellationToken = default)
```

### Parameters

**data** &ensp; [DiagnosticTestData](../../DiagnosticTestData/README.md)

**expected** &ensp; [ExpectedTestState](../../ExpectedTestState/README.md)

**options** &ensp; [TestOptions](../../TestOptions/README.md)

**cancellationToken** &ensp; [CancellationToken](https://docs.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken)

### Returns

[Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task)

