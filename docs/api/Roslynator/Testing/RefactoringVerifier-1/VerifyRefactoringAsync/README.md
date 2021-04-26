# RefactoringVerifier\<TRefactoringProvider>\.VerifyRefactoringAsync\(RefactoringTestData, ExpectedTestState, TestOptions, CancellationToken\) Method

[Home](../../../../README.md)

**Containing Type**: [RefactoringVerifier\<TRefactoringProvider>](../README.md)

**Assembly**: Roslynator\.Testing\.Common\.dll

\
Verifies that refactoring will be applied correctly using specified **TRefactoringProvider**\.

```csharp
public System.Threading.Tasks.Task VerifyRefactoringAsync(Roslynator.Testing.RefactoringTestData data, Roslynator.Testing.ExpectedTestState expected, Roslynator.Testing.TestOptions options = null, System.Threading.CancellationToken cancellationToken = default)
```

### Parameters

**data** &ensp; [RefactoringTestData](../../RefactoringTestData/README.md)

**expected** &ensp; [ExpectedTestState](../../ExpectedTestState/README.md)

**options** &ensp; [TestOptions](../../TestOptions/README.md)

**cancellationToken** &ensp; [CancellationToken](https://docs.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken)

### Returns

[Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task)

