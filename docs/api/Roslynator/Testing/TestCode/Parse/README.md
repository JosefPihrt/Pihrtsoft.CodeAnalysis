# TestCode\.Parse Method

[Home](../../../../README.md)

**Containing Type**: [TestCode](../README.md)

**Assembly**: Roslynator\.Testing\.Common\.dll

## Overloads

| Method | Summary |
| ------ | ------- |
| [Parse(String)](#Roslynator_Testing_TestCode_Parse_System_String_) | Finds and removes spans that are marked with `[|` and `|]` tokens\. |
| [Parse(String, String, String)](#Roslynator_Testing_TestCode_Parse_System_String_System_String_System_String_) | Finds and replace span that is marked with `[||]` token\. |

## Parse\(String\) <a id="Roslynator_Testing_TestCode_Parse_System_String_"></a>

\
Finds and removes spans that are marked with `[|` and `|]` tokens\.

```csharp
public static Roslynator.Testing.TestCode Parse(string value)
```

### Parameters

**value** &ensp; [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)

### Returns

[TestCode](../README.md)

## Parse\(String, String, String\) <a id="Roslynator_Testing_TestCode_Parse_System_String_System_String_System_String_"></a>

\
Finds and replace span that is marked with `[||]` token\.

```csharp
public static Roslynator.Testing.TestCode Parse(string value, string replacement1, string replacement2 = null)
```

### Parameters

**value** &ensp; [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)

**replacement1** &ensp; [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)

**replacement2** &ensp; [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)

### Returns

[TestCode](../README.md)

