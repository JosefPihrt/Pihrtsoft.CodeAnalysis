# Roslynator API Reference

[Namespaces](#namespaces) &#x2022; [Class Hierarchy](#class-hierarchy) &#x2022; [Other](#other)

## Namespaces

* [Roslynator](Roslynator/README.md)
* [Roslynator.CSharp](Roslynator/CSharp/README.md)
* [Roslynator.CSharp.Syntax](Roslynator/CSharp/Syntax/README.md)
* [Roslynator.Testing](Roslynator/Testing/README.md)
* [Roslynator.Testing.CSharp](Roslynator/Testing/CSharp/README.md)
* [Roslynator.Testing.CSharp.Xunit](Roslynator/Testing/CSharp/Xunit/README.md)
* [Roslynator.Text](Roslynator/Text/README.md)

## Class Hierarchy

* &ensp; \([Object](https://docs.microsoft.com/en-us/dotnet/api/system.object)\)<a id="class-hierarchy-System_Object"></a>
* &ensp; [&bull;](#class-hierarchy-System_Object "Object") &ensp; [CodeVerifier](Roslynator/Testing/CodeVerifier/README.md)<a id="class-hierarchy-Roslynator_Testing_CodeVerifier"></a>
* &ensp; [&bull;](#class-hierarchy-System_Object "Object") &ensp; [&bull;](#class-hierarchy-Roslynator_Testing_CodeVerifier "CodeVerifier") &ensp; [CompilerDiagnosticFixVerifier\<TFixProvider>](Roslynator/Testing/CompilerDiagnosticFixVerifier-1/README.md)<a id="class-hierarchy-Roslynator_Testing_CompilerDiagnosticFixVerifier_1"></a>
* &ensp; [&bull;](#class-hierarchy-System_Object "Object") &ensp; [&bull;](#class-hierarchy-Roslynator_Testing_CodeVerifier "CodeVerifier") &ensp; [&bull;](#class-hierarchy-Roslynator_Testing_CompilerDiagnosticFixVerifier_1 "CompilerDiagnosticFixVerifier<TFixProvider>") &ensp; [CSharpCompilerDiagnosticFixVerifier\<TFixProvider>](Roslynator/Testing/CSharp/CSharpCompilerDiagnosticFixVerifier-1/README.md)<a id="class-hierarchy-Roslynator_Testing_CSharp_CSharpCompilerDiagnosticFixVerifier_1"></a>
* &ensp; [&bull;](#class-hierarchy-System_Object "Object") &ensp; [&bull;](#class-hierarchy-Roslynator_Testing_CodeVerifier "CodeVerifier") &ensp; [&bull;](#class-hierarchy-Roslynator_Testing_CompilerDiagnosticFixVerifier_1 "CompilerDiagnosticFixVerifier<TFixProvider>") &ensp; [&bull;](#class-hierarchy-Roslynator_Testing_CSharp_CSharpCompilerDiagnosticFixVerifier_1 "CSharpCompilerDiagnosticFixVerifier<TFixProvider>") &ensp; [XunitCompilerDiagnosticFixVerifier\<TFixProvider>](Roslynator/Testing/CSharp/Xunit/XunitCompilerDiagnosticFixVerifier-1/README.md)<a id="class-hierarchy-Roslynator_Testing_CSharp_Xunit_XunitCompilerDiagnosticFixVerifier_1"></a>
* &ensp; [&bull;](#class-hierarchy-System_Object "Object") &ensp; [&bull;](#class-hierarchy-Roslynator_Testing_CodeVerifier "CodeVerifier") &ensp; [DiagnosticVerifier\<TAnalyzer, TFixProvider>](Roslynator/Testing/DiagnosticVerifier-2/README.md)<a id="class-hierarchy-Roslynator_Testing_DiagnosticVerifier_2"></a>
* &ensp; [&bull;](#class-hierarchy-System_Object "Object") &ensp; [&bull;](#class-hierarchy-Roslynator_Testing_CodeVerifier "CodeVerifier") &ensp; [&bull;](#class-hierarchy-Roslynator_Testing_DiagnosticVerifier_2 "DiagnosticVerifier<TAnalyzer, TFixProvider>") &ensp; [CSharpDiagnosticVerifier\<TAnalyzer, TFixProvider>](Roslynator/Testing/CSharp/CSharpDiagnosticVerifier-2/README.md)<a id="class-hierarchy-Roslynator_Testing_CSharp_CSharpDiagnosticVerifier_2"></a>
* &ensp; [&bull;](#class-hierarchy-System_Object "Object") &ensp; [&bull;](#class-hierarchy-Roslynator_Testing_CodeVerifier "CodeVerifier") &ensp; [&bull;](#class-hierarchy-Roslynator_Testing_DiagnosticVerifier_2 "DiagnosticVerifier<TAnalyzer, TFixProvider>") &ensp; [&bull;](#class-hierarchy-Roslynator_Testing_CSharp_CSharpDiagnosticVerifier_2 "CSharpDiagnosticVerifier<TAnalyzer, TFixProvider>") &ensp; [XunitDiagnosticVerifier\<TAnalyzer, TFixProvider>](Roslynator/Testing/CSharp/Xunit/XunitDiagnosticVerifier-2/README.md)<a id="class-hierarchy-Roslynator_Testing_CSharp_Xunit_XunitDiagnosticVerifier_2"></a>
* &ensp; [&bull;](#class-hierarchy-System_Object "Object") &ensp; [&bull;](#class-hierarchy-Roslynator_Testing_CodeVerifier "CodeVerifier") &ensp; [RefactoringVerifier\<TRefactoringProvider>](Roslynator/Testing/RefactoringVerifier-1/README.md)<a id="class-hierarchy-Roslynator_Testing_RefactoringVerifier_1"></a>
* &ensp; [&bull;](#class-hierarchy-System_Object "Object") &ensp; [&bull;](#class-hierarchy-Roslynator_Testing_CodeVerifier "CodeVerifier") &ensp; [&bull;](#class-hierarchy-Roslynator_Testing_RefactoringVerifier_1 "RefactoringVerifier<TRefactoringProvider>") &ensp; [CSharpRefactoringVerifier\<TRefactoringProvider>](Roslynator/Testing/CSharp/CSharpRefactoringVerifier-1/README.md)<a id="class-hierarchy-Roslynator_Testing_CSharp_CSharpRefactoringVerifier_1"></a>
* &ensp; [&bull;](#class-hierarchy-System_Object "Object") &ensp; [&bull;](#class-hierarchy-Roslynator_Testing_CodeVerifier "CodeVerifier") &ensp; [&bull;](#class-hierarchy-Roslynator_Testing_RefactoringVerifier_1 "RefactoringVerifier<TRefactoringProvider>") &ensp; [&bull;](#class-hierarchy-Roslynator_Testing_CSharp_CSharpRefactoringVerifier_1 "CSharpRefactoringVerifier<TRefactoringProvider>") &ensp; [XunitRefactoringVerifier\<TRefactoringProvider>](Roslynator/Testing/CSharp/Xunit/XunitRefactoringVerifier-1/README.md)<a id="class-hierarchy-Roslynator_Testing_CSharp_Xunit_XunitRefactoringVerifier_1"></a>
* &ensp; [&bull;](#class-hierarchy-System_Object "Object") &ensp; [CompilerDiagnosticFixTestData](Roslynator/Testing/CompilerDiagnosticFixTestData/README.md)<a id="class-hierarchy-Roslynator_Testing_CompilerDiagnosticFixTestData"></a>
* &ensp; [&bull;](#class-hierarchy-System_Object "Object") &ensp; [DiagnosticTestData](Roslynator/Testing/DiagnosticTestData/README.md)<a id="class-hierarchy-Roslynator_Testing_DiagnosticTestData"></a>
* &ensp; [&bull;](#class-hierarchy-System_Object "Object") &ensp; \([EqualityComparer\<T>](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.equalitycomparer-1)\)<a id="class-hierarchy-System_Collections_Generic_EqualityComparer_1"></a>
* &ensp; [&bull;](#class-hierarchy-System_Object "Object") &ensp; [&bull;](#class-hierarchy-System_Collections_Generic_EqualityComparer_1 "EqualityComparer<T>") &ensp; [MetadataNameEqualityComparer\<TSymbol>](Roslynator/MetadataNameEqualityComparer-1/README.md)<a id="class-hierarchy-Roslynator_MetadataNameEqualityComparer_1"></a>
* &ensp; [&bull;](#class-hierarchy-System_Object "Object") &ensp; [ExpectedTestState](Roslynator/Testing/ExpectedTestState/README.md)<a id="class-hierarchy-Roslynator_Testing_ExpectedTestState"></a>
* &ensp; [&bull;](#class-hierarchy-System_Object "Object") &ensp; [ModifierList\<TNode>](Roslynator/CSharp/ModifierList-1/README.md)<a id="class-hierarchy-Roslynator_CSharp_ModifierList_1"></a>
* &ensp; [&bull;](#class-hierarchy-System_Object "Object") &ensp; [NameGenerator](Roslynator/NameGenerator/README.md)<a id="class-hierarchy-Roslynator_NameGenerator"></a>
* &ensp; [&bull;](#class-hierarchy-System_Object "Object") &ensp; [RefactoringTestData](Roslynator/Testing/RefactoringTestData/README.md)<a id="class-hierarchy-Roslynator_Testing_RefactoringTestData"></a>
* &ensp; [&bull;](#class-hierarchy-System_Object "Object") &ensp; [SeparatedSyntaxListSelection\<TNode>](Roslynator/SeparatedSyntaxListSelection-1/README.md)<a id="class-hierarchy-Roslynator_SeparatedSyntaxListSelection_1"></a>
* &ensp; [&bull;](#class-hierarchy-System_Object "Object") &ensp; [SyntaxListSelection\<TNode>](Roslynator/SyntaxListSelection-1/README.md)<a id="class-hierarchy-Roslynator_SyntaxListSelection_1"></a>
* &ensp; [&bull;](#class-hierarchy-System_Object "Object") &ensp; [&bull;](#class-hierarchy-Roslynator_SyntaxListSelection_1 "SyntaxListSelection<TNode>") &ensp; [MemberDeclarationListSelection](Roslynator/CSharp/MemberDeclarationListSelection/README.md)<a id="class-hierarchy-Roslynator_CSharp_MemberDeclarationListSelection"></a>
* &ensp; [&bull;](#class-hierarchy-System_Object "Object") &ensp; [&bull;](#class-hierarchy-Roslynator_SyntaxListSelection_1 "SyntaxListSelection<TNode>") &ensp; [StatementListSelection](Roslynator/CSharp/StatementListSelection/README.md)<a id="class-hierarchy-Roslynator_CSharp_StatementListSelection"></a>
* &ensp; [&bull;](#class-hierarchy-System_Object "Object") &ensp; [SyntaxNodeTextBuilder](Roslynator/Text/SyntaxNodeTextBuilder/README.md)<a id="class-hierarchy-Roslynator_Text_SyntaxNodeTextBuilder"></a>
* &ensp; [&bull;](#class-hierarchy-System_Object "Object") &ensp; [TestOptions](Roslynator/Testing/TestOptions/README.md)<a id="class-hierarchy-Roslynator_Testing_TestOptions"></a>
* &ensp; [&bull;](#class-hierarchy-System_Object "Object") &ensp; [&bull;](#class-hierarchy-Roslynator_Testing_TestOptions "TestOptions") &ensp; [CSharpTestOptions](Roslynator/Testing/CSharp/CSharpTestOptions/README.md)<a id="class-hierarchy-Roslynator_Testing_CSharp_CSharpTestOptions"></a>
* &ensp; [&bull;](#class-hierarchy-System_Object "Object") &ensp; [TextLineCollectionSelection](Roslynator/Text/TextLineCollectionSelection/README.md)<a id="class-hierarchy-Roslynator_Text_TextLineCollectionSelection"></a>

## [Roslynator](Roslynator/README.md) Namespace

### Classes

* [DiagnosticsExtensions](Roslynator/DiagnosticsExtensions/README.md)
* [EnumExtensions](Roslynator/EnumExtensions/README.md)
* [FileLinePositionSpanExtensions](Roslynator/FileLinePositionSpanExtensions/README.md)
* [MetadataNameEqualityComparer\<TSymbol>](Roslynator/MetadataNameEqualityComparer-1/README.md)
* [NameGenerator](Roslynator/NameGenerator/README.md)
* [SemanticModelExtensions](Roslynator/SemanticModelExtensions/README.md)
* [SeparatedSyntaxListSelection\<TNode>](Roslynator/SeparatedSyntaxListSelection-1/README.md)
* [SymbolExtensions](Roslynator/SymbolExtensions/README.md)
* [SyntaxExtensions](Roslynator/SyntaxExtensions/README.md)
* [SyntaxListSelection\<TNode>](Roslynator/SyntaxListSelection-1/README.md)
* [SyntaxTreeExtensions](Roslynator/SyntaxTreeExtensions/README.md)
* [WorkspaceExtensions](Roslynator/WorkspaceExtensions/README.md)

### Structs

* [ExtensionMethodSymbolInfo](Roslynator/ExtensionMethodSymbolInfo/README.md)
* [MetadataName](Roslynator/MetadataName/README.md)
* [SeparatedSyntaxListSelection\<TNode>.Enumerator](Roslynator/SeparatedSyntaxListSelection-1/Enumerator/README.md)
* [SyntaxListSelection\<TNode>.Enumerator](Roslynator/SyntaxListSelection-1/Enumerator/README.md)

### Interfaces

* [ISelection\<T>](Roslynator/ISelection-1/README.md)

### Enums

* [Visibility](Roslynator/Visibility/README.md)

## [Roslynator.CSharp](Roslynator/CSharp/README.md) Namespace

### Classes

* [CSharpExtensions](Roslynator/CSharp/CSharpExtensions/README.md)
* [CSharpFactory](Roslynator/CSharp/CSharpFactory/README.md)
* [CSharpFacts](Roslynator/CSharp/CSharpFacts/README.md)
* [MemberDeclarationListSelection](Roslynator/CSharp/MemberDeclarationListSelection/README.md)
* [ModifierList](Roslynator/CSharp/ModifierList/README.md)
* [ModifierList\<TNode>](Roslynator/CSharp/ModifierList-1/README.md)
* [Modifiers](Roslynator/CSharp/Modifiers/README.md)
* [StatementListSelection](Roslynator/CSharp/StatementListSelection/README.md)
* [SymbolExtensions](Roslynator/CSharp/SymbolExtensions/README.md)
* [SyntaxAccessibility](Roslynator/CSharp/SyntaxAccessibility/README.md)
* [SyntaxExtensions](Roslynator/CSharp/SyntaxExtensions/README.md)
* [SyntaxInfo](Roslynator/CSharp/SyntaxInfo/README.md)
* [SyntaxInverter](Roslynator/CSharp/SyntaxInverter/README.md)
* [WorkspaceExtensions](Roslynator/CSharp/WorkspaceExtensions/README.md)
* [WorkspaceSymbolExtensions](Roslynator/CSharp/WorkspaceSymbolExtensions/README.md)
* [WorkspaceSyntaxExtensions](Roslynator/CSharp/WorkspaceSyntaxExtensions/README.md)

### Structs

* [ExpressionChain](Roslynator/CSharp/ExpressionChain/README.md)
* [ExpressionChain.Enumerator](Roslynator/CSharp/ExpressionChain/Enumerator/README.md)
* [ExpressionChain.Reversed](Roslynator/CSharp/ExpressionChain/Reversed/README.md)
* [ExpressionChain.Reversed.Enumerator](Roslynator/CSharp/ExpressionChain/Reversed/Enumerator/README.md)
* [IfStatementCascade](Roslynator/CSharp/IfStatementCascade/README.md)
* [IfStatementCascade.Enumerator](Roslynator/CSharp/IfStatementCascade/Enumerator/README.md)
* [IfStatementCascadeInfo](Roslynator/CSharp/IfStatementCascadeInfo/README.md)
* [IfStatementOrElseClause](Roslynator/CSharp/IfStatementOrElseClause/README.md)

### Enums

* [CommentFilter](Roslynator/CSharp/CommentFilter/README.md)
* [DefaultSyntaxOptions](Roslynator/CSharp/DefaultSyntaxOptions/README.md)
* [ModifierFilter](Roslynator/CSharp/ModifierFilter/README.md)
* [NullCheckStyles](Roslynator/CSharp/NullCheckStyles/README.md)
* [PreprocessorDirectiveFilter](Roslynator/CSharp/PreprocessorDirectiveFilter/README.md)

## [Roslynator.CSharp.Syntax](Roslynator/CSharp/Syntax/README.md) Namespace

### Structs

* [AsExpressionInfo](Roslynator/CSharp/Syntax/AsExpressionInfo/README.md)
* [AssignmentExpressionInfo](Roslynator/CSharp/Syntax/AssignmentExpressionInfo/README.md)
* [BinaryExpressionInfo](Roslynator/CSharp/Syntax/BinaryExpressionInfo/README.md)
* [ConditionalExpressionInfo](Roslynator/CSharp/Syntax/ConditionalExpressionInfo/README.md)
* [GenericInfo](Roslynator/CSharp/Syntax/GenericInfo/README.md)
* [IsExpressionInfo](Roslynator/CSharp/Syntax/IsExpressionInfo/README.md)
* [LocalDeclarationStatementInfo](Roslynator/CSharp/Syntax/LocalDeclarationStatementInfo/README.md)
* [MemberDeclarationListInfo](Roslynator/CSharp/Syntax/MemberDeclarationListInfo/README.md)
* [ModifierListInfo](Roslynator/CSharp/Syntax/ModifierListInfo/README.md)
* [NullCheckExpressionInfo](Roslynator/CSharp/Syntax/NullCheckExpressionInfo/README.md)
* [RegionInfo](Roslynator/CSharp/Syntax/RegionInfo/README.md)
* [SimpleAssignmentExpressionInfo](Roslynator/CSharp/Syntax/SimpleAssignmentExpressionInfo/README.md)
* [SimpleAssignmentStatementInfo](Roslynator/CSharp/Syntax/SimpleAssignmentStatementInfo/README.md)
* [SimpleIfStatementInfo](Roslynator/CSharp/Syntax/SimpleIfStatementInfo/README.md)
* [SimpleMemberInvocationExpressionInfo](Roslynator/CSharp/Syntax/SimpleMemberInvocationExpressionInfo/README.md)
* [SimpleMemberInvocationStatementInfo](Roslynator/CSharp/Syntax/SimpleMemberInvocationStatementInfo/README.md)
* [SingleLocalDeclarationStatementInfo](Roslynator/CSharp/Syntax/SingleLocalDeclarationStatementInfo/README.md)
* [SingleParameterLambdaExpressionInfo](Roslynator/CSharp/Syntax/SingleParameterLambdaExpressionInfo/README.md)
* [StatementListInfo](Roslynator/CSharp/Syntax/StatementListInfo/README.md)
* [StringConcatenationExpressionInfo](Roslynator/CSharp/Syntax/StringConcatenationExpressionInfo/README.md)
* [StringLiteralExpressionInfo](Roslynator/CSharp/Syntax/StringLiteralExpressionInfo/README.md)
* [UsingDirectiveListInfo](Roslynator/CSharp/Syntax/UsingDirectiveListInfo/README.md)
* [XmlElementInfo](Roslynator/CSharp/Syntax/XmlElementInfo/README.md)

## [Roslynator.Testing](Roslynator/Testing/README.md) Namespace

### Classes

* [CodeVerifier](Roslynator/Testing/CodeVerifier/README.md)
* [CompilerDiagnosticFixTestData](Roslynator/Testing/CompilerDiagnosticFixTestData/README.md)
* [CompilerDiagnosticFixVerifier\<TFixProvider>](Roslynator/Testing/CompilerDiagnosticFixVerifier-1/README.md)
* [DiagnosticTestData](Roslynator/Testing/DiagnosticTestData/README.md)
* [DiagnosticVerifier\<TAnalyzer, TFixProvider>](Roslynator/Testing/DiagnosticVerifier-2/README.md)
* [ExpectedTestState](Roslynator/Testing/ExpectedTestState/README.md)
* [RefactoringTestData](Roslynator/Testing/RefactoringTestData/README.md)
* [RefactoringVerifier\<TRefactoringProvider>](Roslynator/Testing/RefactoringVerifier-1/README.md)
* [TestOptions](Roslynator/Testing/TestOptions/README.md)

### Structs

* [AdditionalFile](Roslynator/Testing/AdditionalFile/README.md)
* [TestCode](Roslynator/Testing/TestCode/README.md)

## [Roslynator.Testing.CSharp](Roslynator/Testing/CSharp/README.md) Namespace

### Classes

* [CSharpCompilerDiagnosticFixVerifier\<TFixProvider>](Roslynator/Testing/CSharp/CSharpCompilerDiagnosticFixVerifier-1/README.md)
* [CSharpDiagnosticVerifier\<TAnalyzer, TFixProvider>](Roslynator/Testing/CSharp/CSharpDiagnosticVerifier-2/README.md)
* [CSharpRefactoringVerifier\<TRefactoringProvider>](Roslynator/Testing/CSharp/CSharpRefactoringVerifier-1/README.md)
* [CSharpTestOptions](Roslynator/Testing/CSharp/CSharpTestOptions/README.md)

## [Roslynator.Testing.CSharp.Xunit](Roslynator/Testing/CSharp/Xunit/README.md) Namespace

### Classes

* [XunitCompilerDiagnosticFixVerifier\<TFixProvider>](Roslynator/Testing/CSharp/Xunit/XunitCompilerDiagnosticFixVerifier-1/README.md)
* [XunitDiagnosticVerifier\<TAnalyzer, TFixProvider>](Roslynator/Testing/CSharp/Xunit/XunitDiagnosticVerifier-2/README.md)
* [XunitRefactoringVerifier\<TRefactoringProvider>](Roslynator/Testing/CSharp/Xunit/XunitRefactoringVerifier-1/README.md)

## [Roslynator.Text](Roslynator/Text/README.md) Namespace

### Classes

* [SyntaxNodeTextBuilder](Roslynator/Text/SyntaxNodeTextBuilder/README.md)
* [TextLineCollectionSelection](Roslynator/Text/TextLineCollectionSelection/README.md)

### Structs

* [TextLineCollectionSelection.Enumerator](Roslynator/Text/TextLineCollectionSelection/Enumerator/README.md)

## Other

* [Extensions of External Types](_Extensions.md)
