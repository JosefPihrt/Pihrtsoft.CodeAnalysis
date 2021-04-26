# SyntaxExtensions\.PrependToTrailingTrivia Method

[Home](../../../README.md)

**Containing Type**: [SyntaxExtensions](../README.md)

**Assembly**: Roslynator\.Core\.dll

## Overloads

| Method | Summary |
| ------ | ------- |
| [PrependToTrailingTrivia(SyntaxToken, IEnumerable\<SyntaxTrivia>)](../PrependToTrailingTrivia/README.md#Roslynator_SyntaxExtensions_PrependToTrailingTrivia_Microsoft_CodeAnalysis_SyntaxToken_System_Collections_Generic_IEnumerable_Microsoft_CodeAnalysis_SyntaxTrivia__) | Creates a new token from this token with the trailing trivia replaced with a new trivia where the specified trivia is inserted at the beginning of the trailing trivia\. |
| [PrependToTrailingTrivia(SyntaxToken, SyntaxTrivia)](../PrependToTrailingTrivia/README.md#Roslynator_SyntaxExtensions_PrependToTrailingTrivia_Microsoft_CodeAnalysis_SyntaxToken_Microsoft_CodeAnalysis_SyntaxTrivia_) | Creates a new token from this token with the trailing trivia replaced with a new trivia where the specified trivia is inserted at the beginning of the trailing trivia\. |
| [PrependToTrailingTrivia\<TNode>(TNode, IEnumerable\<SyntaxTrivia>)](#Roslynator_SyntaxExtensions_PrependToTrailingTrivia__1___0_System_Collections_Generic_IEnumerable_Microsoft_CodeAnalysis_SyntaxTrivia__) | Creates a new node from this node with the trailing trivia replaced with a new trivia where the specified trivia is inserted at the beginning of the trailing trivia\. |
| [PrependToTrailingTrivia\<TNode>(TNode, SyntaxTrivia)](#Roslynator_SyntaxExtensions_PrependToTrailingTrivia__1___0_Microsoft_CodeAnalysis_SyntaxTrivia_) | Creates a new node from this node with the trailing trivia replaced with a new trivia where the specified trivia is inserted at the beginning of the trailing trivia\. |

## PrependToTrailingTrivia\(SyntaxToken, IEnumerable\<SyntaxTrivia>\) <a id="Roslynator_SyntaxExtensions_PrependToTrailingTrivia_Microsoft_CodeAnalysis_SyntaxToken_System_Collections_Generic_IEnumerable_Microsoft_CodeAnalysis_SyntaxTrivia__"></a>

\
Creates a new token from this token with the trailing trivia replaced with a new trivia where the specified trivia is inserted at the beginning of the trailing trivia\.

```csharp
public static Microsoft.CodeAnalysis.SyntaxToken PrependToTrailingTrivia(this Microsoft.CodeAnalysis.SyntaxToken token, System.Collections.Generic.IEnumerable<Microsoft.CodeAnalysis.SyntaxTrivia> trivia)
```

### Parameters

**token** &ensp; [SyntaxToken](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.syntaxtoken)

**trivia** &ensp; [IEnumerable](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1)\<[SyntaxTrivia](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.syntaxtrivia)>

### Returns

[SyntaxToken](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.syntaxtoken)

## PrependToTrailingTrivia\(SyntaxToken, SyntaxTrivia\) <a id="Roslynator_SyntaxExtensions_PrependToTrailingTrivia_Microsoft_CodeAnalysis_SyntaxToken_Microsoft_CodeAnalysis_SyntaxTrivia_"></a>

\
Creates a new token from this token with the trailing trivia replaced with a new trivia where the specified trivia is inserted at the beginning of the trailing trivia\.

```csharp
public static Microsoft.CodeAnalysis.SyntaxToken PrependToTrailingTrivia(this Microsoft.CodeAnalysis.SyntaxToken token, Microsoft.CodeAnalysis.SyntaxTrivia trivia)
```

### Parameters

**token** &ensp; [SyntaxToken](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.syntaxtoken)

**trivia** &ensp; [SyntaxTrivia](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.syntaxtrivia)

### Returns

[SyntaxToken](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.syntaxtoken)

## PrependToTrailingTrivia\<TNode>\(TNode, IEnumerable\<SyntaxTrivia>\) <a id="Roslynator_SyntaxExtensions_PrependToTrailingTrivia__1___0_System_Collections_Generic_IEnumerable_Microsoft_CodeAnalysis_SyntaxTrivia__"></a>

\
Creates a new node from this node with the trailing trivia replaced with a new trivia where the specified trivia is inserted at the beginning of the trailing trivia\.

```csharp
public static TNode PrependToTrailingTrivia<TNode>(this TNode node, System.Collections.Generic.IEnumerable<Microsoft.CodeAnalysis.SyntaxTrivia> trivia) where TNode : Microsoft.CodeAnalysis.SyntaxNode
```

### Type Parameters

**TNode**

### Parameters

**node** &ensp; TNode

**trivia** &ensp; [IEnumerable](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1)\<[SyntaxTrivia](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.syntaxtrivia)>

### Returns

TNode

## PrependToTrailingTrivia\<TNode>\(TNode, SyntaxTrivia\) <a id="Roslynator_SyntaxExtensions_PrependToTrailingTrivia__1___0_Microsoft_CodeAnalysis_SyntaxTrivia_"></a>

\
Creates a new node from this node with the trailing trivia replaced with a new trivia where the specified trivia is inserted at the beginning of the trailing trivia\.

```csharp
public static TNode PrependToTrailingTrivia<TNode>(this TNode node, Microsoft.CodeAnalysis.SyntaxTrivia trivia) where TNode : Microsoft.CodeAnalysis.SyntaxNode
```

### Type Parameters

**TNode**

### Parameters

**node** &ensp; TNode

**trivia** &ensp; [SyntaxTrivia](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.syntaxtrivia)

### Returns

TNode

