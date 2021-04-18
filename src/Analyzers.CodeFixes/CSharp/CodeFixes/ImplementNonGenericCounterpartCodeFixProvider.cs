// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ImplementNonGenericCounterpartCodeFixProvider))]
    [Shared]
    public sealed class ImplementNonGenericCounterpartCodeFixProvider : BaseCodeFixProvider
    {
        public ImplementNonGenericCounterpartCodeFixProvider()
        {
            ExplicitEquivalenceKey = GetEquivalenceKey(DiagnosticIdentifiers.ImplementNonGenericCounterpart, "Explicit");
        }

        internal readonly string ExplicitEquivalenceKey;

        private const string IComparableCompareText = @"
public int global::System.IComparable.CompareTo(object obj)
{
    if (obj == null)
    {
        return 1;
    }

    if (obj is T x)
    {
        return CompareTo(x);
    }

    throw new global::System.ArgumentException("""", nameof(obj));
}
";

        private const string IComparerCompareText = @"
public int global::System.Collections.IComparer.Compare(object x, object y)
{
    if (x == y)
    {
        return 0;
    }

    if (x == null)
    {
        return -1;
    }

    if (y == null)
    {
        return 1;
    }

    if (x is T a
        && y is T b)
    {
        return Compare(a, b);
    }

    throw new global::System.ArgumentException("""", nameof(x));
}
";

        private const string IEqualityComparerEqualsText = @"
new public bool global::System.Collections.IEqualityComparer.Equals(object x, object y)
{
    if (x == y)
    {
        return true;
    }

    if (x == null || y == null)
    {
        return false;
    }

    if (x is T a
        && y is T b)
    {
        return Equals(a, b);
    }

    throw new global::System.ArgumentException("""", nameof(x));
}
";

        private const string IEqualityComparerGetHashCodeText = @"
public int global::System.Collections.IEqualityComparer.GetHashCode(object obj)
{
    if (obj == null)
    {
        return 0;
    }

    if (obj is T x)
    {
        return GetHashCode(x);
    }

    throw new global::System.ArgumentException("""", nameof(obj));
}
";

        private static readonly Lazy<MethodDeclarationSyntax> _lazyIComparableCompare = new Lazy<MethodDeclarationSyntax>(() => CreateMethodDeclaration(IComparableCompareText, explicitInterfaceImplementation: false));
        private static readonly Lazy<MethodDeclarationSyntax> _lazyIComparerCompare = new Lazy<MethodDeclarationSyntax>(() => CreateMethodDeclaration(IComparerCompareText, explicitInterfaceImplementation: false));
        private static readonly Lazy<MethodDeclarationSyntax> _lazyIEqualityComparerEquals = new Lazy<MethodDeclarationSyntax>(() => CreateMethodDeclaration(IEqualityComparerEqualsText, explicitInterfaceImplementation: false));
        private static readonly Lazy<MethodDeclarationSyntax> _lazyIEqualityComparerGetHashCode = new Lazy<MethodDeclarationSyntax>(() => CreateMethodDeclaration(IEqualityComparerGetHashCodeText, explicitInterfaceImplementation: false));

        private static readonly Lazy<MethodDeclarationSyntax> _lazyIComparableCompareExplicit = new Lazy<MethodDeclarationSyntax>(() => CreateMethodDeclaration(IComparableCompareText, explicitInterfaceImplementation: true));
        private static readonly Lazy<MethodDeclarationSyntax> _lazyIComparerCompareExplicit = new Lazy<MethodDeclarationSyntax>(() => CreateMethodDeclaration(IComparerCompareText, explicitInterfaceImplementation: true));
        private static readonly Lazy<MethodDeclarationSyntax> _lazyIEqualityComparerEqualsExplicit = new Lazy<MethodDeclarationSyntax>(() => CreateMethodDeclaration(IEqualityComparerEqualsText, explicitInterfaceImplementation: true));
        private static readonly Lazy<MethodDeclarationSyntax> _lazyIEqualityComparerGetHashCodeExplicit = new Lazy<MethodDeclarationSyntax>(() => CreateMethodDeclaration(IEqualityComparerGetHashCodeText, explicitInterfaceImplementation: true));

        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.ImplementNonGenericCounterpart); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out TypeDeclarationSyntax typeDeclaration))
                return;

            Diagnostic diagnostic = context.Diagnostics[0];

            Document document = context.Document;

            string interfaceName = diagnostic.Properties["InterfaceName"];

            CodeAction codeAction = CodeAction.Create(
                $"Implement {interfaceName}",
                ct => RefactorAsync(document, typeDeclaration, interfaceName, explicitImplementation: false, ct),
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);

            codeAction = CodeAction.Create(
                $"Implement {interfaceName} explicitly",
                ct => RefactorAsync(document, typeDeclaration, interfaceName, explicitImplementation: true, ct),
                GetEquivalenceKey(diagnostic, "Explicit"));

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        private static MethodDeclarationSyntax CreateMethodDeclaration(string text, bool explicitInterfaceImplementation)
        {
            CompilationUnitSyntax compilationUnit = SyntaxFactory.ParseCompilationUnit($@"class C<T>
{{
    {text}
}}");

            var classDeclaration = (ClassDeclarationSyntax)compilationUnit.Members[0];

            var methodDeclaration = (MethodDeclarationSyntax)classDeclaration.Members[0];

            MethodDeclarationSyntax newMethodDeclaration = methodDeclaration;

            if (explicitInterfaceImplementation)
            {
                newMethodDeclaration = newMethodDeclaration
                    .WithModifiers(default)
                    .WithLeadingTrivia(methodDeclaration.GetLeadingTrivia());
            }
            else
            {
                newMethodDeclaration = newMethodDeclaration.WithExplicitInterfaceSpecifier(null);
            }

            newMethodDeclaration = (MethodDeclarationSyntax)AddSimplifierAnnotationRewriter.Instance.VisitMethodDeclaration(newMethodDeclaration);

            return newMethodDeclaration.WithFormatterAnnotation();
        }

        private static async Task<Document> RefactorAsync(
            Document document,
            TypeDeclarationSyntax typeDeclaration,
            string interfaceName,
            bool explicitImplementation,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync().ConfigureAwait(false);

            INamedTypeSymbol symbol = semanticModel.GetDeclaredSymbol(typeDeclaration, cancellationToken);

            ImmutableArray<INamedTypeSymbol> interfaces = symbol.Interfaces;

            TypeDeclarationSyntax newTypeDeclaration = typeDeclaration;

            TypeSyntax interfaceType = null;

            switch (interfaceName)
            {
                case "IComparable":
                    {
                        TypeSyntax type = interfaces
                            .First(f => f.HasMetadataName(MetadataNames.System_IComparable_T))
                            .TypeArguments
                            .Single()
                            .ToTypeSyntax()
                            .WithSimplifierAnnotation();

                        var rewriter = new AddTypeNameRewriter(type);

                        MethodDeclarationSyntax methodDeclaration = (explicitImplementation) ? _lazyIComparableCompareExplicit.Value : _lazyIComparableCompare.Value;

                        methodDeclaration = (MethodDeclarationSyntax)rewriter.VisitMethodDeclaration(methodDeclaration);

                        newTypeDeclaration = MemberDeclarationInserter.Default.Insert(typeDeclaration, methodDeclaration);

                        interfaceType = SyntaxFactory.ParseTypeName("global::System.IComparable").WithSimplifierAnnotation();
                        break;
                    }
                case "IComparer":
                    {
                        TypeSyntax type = interfaces
                            .First(f => f.HasMetadataName(MetadataNames.System_Collections_Generic_IComparer_T))
                            .TypeArguments
                            .Single()
                            .ToTypeSyntax()
                            .WithSimplifierAnnotation();

                        var rewriter = new AddTypeNameRewriter(type);

                        MethodDeclarationSyntax methodDeclaration = (explicitImplementation) ? _lazyIComparerCompareExplicit.Value : _lazyIComparerCompare.Value;

                        methodDeclaration = (MethodDeclarationSyntax)rewriter.VisitMethodDeclaration(methodDeclaration);

                        newTypeDeclaration = MemberDeclarationInserter.Default.Insert(typeDeclaration, methodDeclaration);

                        interfaceType = SyntaxFactory.ParseTypeName("global::System.Collections.IComparer").WithSimplifierAnnotation();
                        break;
                    }
                case "IEqualityComparer":
                    {
                        TypeSyntax type = interfaces
                            .First(f => f.HasMetadataName(MetadataNames.System_Collections_Generic_IEqualityComparer_T))
                            .TypeArguments
                            .Single()
                            .ToTypeSyntax()
                            .WithSimplifierAnnotation();

                        var rewriter = new AddTypeNameRewriter(type);

                        MethodDeclarationSyntax equalsMethod = (explicitImplementation) ? _lazyIEqualityComparerEqualsExplicit.Value : _lazyIEqualityComparerEquals.Value;

                        equalsMethod = (MethodDeclarationSyntax)rewriter.VisitMethodDeclaration(equalsMethod);

                        newTypeDeclaration = MemberDeclarationInserter.Default.Insert(typeDeclaration, equalsMethod);

                        MethodDeclarationSyntax getHashCodeMethod = (explicitImplementation) ? _lazyIEqualityComparerGetHashCodeExplicit.Value : _lazyIEqualityComparerGetHashCode.Value;

                        getHashCodeMethod = (MethodDeclarationSyntax)rewriter.VisitMethodDeclaration(getHashCodeMethod);

                        newTypeDeclaration = MemberDeclarationInserter.Default.Insert(newTypeDeclaration, getHashCodeMethod);

                        interfaceType = SyntaxFactory.ParseTypeName("global::System.Collections.IEqualityComparer").WithSimplifierAnnotation();
                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException();
                    }
            }

            SimpleBaseTypeSyntax baseType = SyntaxFactory.SimpleBaseType(interfaceType);

            SyntaxKind kind = newTypeDeclaration.Kind();

            if (kind == SyntaxKind.ClassDeclaration)
            {
                var classDeclaration = (ClassDeclarationSyntax)newTypeDeclaration;

                BaseListSyntax baseList = classDeclaration.BaseList;

                SeparatedSyntaxList<BaseTypeSyntax> baseTypes = baseList.Types;

                baseTypes = AddBaseType(baseTypes, baseType);

                newTypeDeclaration = classDeclaration.WithBaseList(baseList.WithTypes(baseTypes));
            }
            else if (kind == SyntaxKind.StructDeclaration)
            {
                var structDeclaration = (StructDeclarationSyntax)newTypeDeclaration;

                BaseListSyntax baseList = structDeclaration.BaseList;

                SeparatedSyntaxList<BaseTypeSyntax> baseTypes = baseList.Types;

                baseTypes = AddBaseType(baseTypes, baseType);

                newTypeDeclaration = structDeclaration.WithBaseList(baseList.WithTypes(baseTypes));
            }

            return await document.ReplaceNodeAsync(typeDeclaration, newTypeDeclaration, cancellationToken).ConfigureAwait(false);
        }

        private static SeparatedSyntaxList<BaseTypeSyntax> AddBaseType(SeparatedSyntaxList<BaseTypeSyntax> baseTypes, SimpleBaseTypeSyntax baseType)
        {
            SyntaxTriviaList trailingTrivia;

            SyntaxToken trailingSeparator = baseTypes.GetTrailingSeparator();

            if (trailingSeparator.IsKind(SyntaxKind.CommaToken))
            {
                baseTypes = baseTypes.ReplaceSeparator(trailingSeparator, trailingSeparator.WithoutTrailingTrivia());

                trailingTrivia = trailingSeparator.TrailingTrivia;
            }
            else
            {
                BaseTypeSyntax last = baseTypes.Last();

                baseTypes = baseTypes.Replace(last, last.WithoutTrailingTrivia());

                trailingTrivia = last.GetTrailingTrivia();
            }

            return baseTypes.Add(baseType.WithTrailingTrivia(trailingTrivia));
        }

        private class AddSimplifierAnnotationRewriter : CSharpSyntaxRewriter
        {
            public static AddSimplifierAnnotationRewriter Instance { get; } = new AddSimplifierAnnotationRewriter();

            public override SyntaxNode VisitQualifiedName(QualifiedNameSyntax node)
            {
                return node.WithSimplifierAnnotation();
            }
        }

        private class RemoveExplicitInterfaceSpecifierAddSimplifierAnnotationRewriter : CSharpSyntaxRewriter
        {
            public static AddSimplifierAnnotationRewriter Instance { get; } = new AddSimplifierAnnotationRewriter();

            public override SyntaxNode VisitQualifiedName(QualifiedNameSyntax node)
            {
                return node.WithSimplifierAnnotation();
            }
        }

        private class AddTypeNameRewriter : CSharpSyntaxRewriter
        {
            private readonly TypeSyntax _type;

            public AddTypeNameRewriter(TypeSyntax type)
            {
                _type = type;
            }

            public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
            {
                if (node.Identifier.ValueText == "T")
                {
                    return _type.WithTriviaFrom(node);
                }

                return base.VisitIdentifierName(node);
            }
        }
    }
}
