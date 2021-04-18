// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UsePredefinedTypeCodeFixProvider))]
    [Shared]
    public sealed class UsePredefinedTypeCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.UsePredefinedType); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindNode(
                root,
                context.Span,
                out SyntaxNode node,
                findInsideTrivia: true,
                predicate: f => f.IsKind(
                    SyntaxKind.QualifiedName,
                    SyntaxKind.IdentifierName,
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxKind.NameMemberCref,
                    SyntaxKind.QualifiedCref)))
            {
                return;
            }

            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

            var typeSymbol = semanticModel.GetSymbol(node, context.CancellationToken) as INamedTypeSymbol;

            CodeAction codeAction = CodeAction.Create(
                $"Use predefined type '{SymbolDisplay.ToDisplayString(typeSymbol, SymbolDisplayFormats.DisplayName_WithoutNullableReferenceTypeModifier)}'",
                cancellationToken => UsePredefinedTypeAsync(context.Document, node, typeSymbol, cancellationToken),
                GetEquivalenceKey(DiagnosticIdentifiers.UsePredefinedType));

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }

        private static Task<Document> UsePredefinedTypeAsync(
            Document document,
            SyntaxNode node,
            ITypeSymbol typeSymbol,
            CancellationToken cancellationToken = default)
        {
            SyntaxNode newNode = GetNewNode(node, typeSymbol.ToTypeSyntax(SymbolDisplayFormats.FullName_WithoutNullableReferenceTypeModifier))
                .WithTriviaFrom(node)
                .WithFormatterAnnotation();

            return document.ReplaceNodeAsync(node, newNode, cancellationToken);
        }

        private static SyntaxNode GetNewNode(SyntaxNode node, TypeSyntax type)
        {
            switch (node.Kind())
            {
                case SyntaxKind.NameMemberCref:
                case SyntaxKind.QualifiedCref:
                    return SyntaxFactory.NameMemberCref(type);
                default:
                    return type;
            }
        }
    }
}
