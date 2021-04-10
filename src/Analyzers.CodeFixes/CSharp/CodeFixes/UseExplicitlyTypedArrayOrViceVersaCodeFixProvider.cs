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
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseExplicitlyTypedArrayOrViceVersaCodeFixProvider))]
    [Shared]
    public sealed class UseExplicitlyTypedArrayOrViceVersaCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.UseExplicitlyTypedArrayOrViceVersa); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(
                root,
                context.Span,
                out SyntaxNode node,
                predicate: f => f.IsKind(SyntaxKind.ImplicitArrayCreationExpression, SyntaxKind.ArrayCreationExpression)))
            {
                return;
            }

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];

            switch (node)
            {
                case ImplicitArrayCreationExpressionSyntax implicitArrayCreation:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            "Use explicitly typed array",
                            ct => ChangeArrayTypeToExplicitAsync(document, implicitArrayCreation, ct),
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
                case ArrayCreationExpressionSyntax arrayCreation:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            "Use implicitly typed array",
                            ct => ChangeArrayTypeToImplicitAsync(document, arrayCreation, ct),
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
            }
        }

        private static async Task<Document> ChangeArrayTypeToExplicitAsync(
            Document document,
            ImplicitArrayCreationExpressionSyntax implicitArrayCreation,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(implicitArrayCreation, cancellationToken);

            var arrayType = (ArrayTypeSyntax)typeSymbol.ToTypeSyntax().WithSimplifierAnnotation();

            SyntaxToken newKeyword = implicitArrayCreation.NewKeyword;

            if (!newKeyword.HasTrailingTrivia)
                newKeyword = newKeyword.WithTrailingTrivia(Space);

            InitializerExpressionSyntax initializer = implicitArrayCreation.Initializer;

            InitializerExpressionSyntax newInitializer = initializer.ReplaceNodes(
                initializer.Expressions,
                (node, _) => (node.IsKind(SyntaxKind.CastExpression)) ? node.WithSimplifierAnnotation() : node);

            ArrayCreationExpressionSyntax newNode = ArrayCreationExpression(
                newKeyword,
                arrayType
                    .WithLeadingTrivia(implicitArrayCreation.OpenBracketToken.LeadingTrivia)
                    .WithTrailingTrivia(implicitArrayCreation.CloseBracketToken.TrailingTrivia),
                newInitializer);

            return await document.ReplaceNodeAsync(implicitArrayCreation, newNode, cancellationToken).ConfigureAwait(false);
        }

        private static async Task<Document> ChangeArrayTypeToImplicitAsync(
            Document document,
            ArrayCreationExpressionSyntax arrayCreation,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            ArrayTypeSyntax arrayType = arrayCreation.Type;
            SyntaxList<ArrayRankSpecifierSyntax> rankSpecifiers = arrayType.RankSpecifiers;
            InitializerExpressionSyntax initializer = arrayCreation.Initializer;

            ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(arrayType.ElementType, cancellationToken);
            TypeSyntax castType;

            if (rankSpecifiers.Count > 1)
            {
                castType = ParseTypeName(arrayType.ToString().Remove(rankSpecifiers.Last().SpanStart - arrayType.SpanStart));
            }
            else
            {
                castType = arrayType.ElementType;
            }

            InitializerExpressionSyntax newInitializer = initializer.ReplaceNodes(
                initializer.Expressions,
                (node, _) => CastExpression(castType, node.WithoutTrivia())
                    .WithTriviaFrom(node)
                    .WithSimplifierAnnotation());

            ImplicitArrayCreationExpressionSyntax implicitArrayCreation = ImplicitArrayCreationExpression(
                arrayCreation.NewKeyword.WithTrailingTrivia(arrayCreation.NewKeyword.TrailingTrivia.EmptyIfWhitespace()),
                rankSpecifiers[0].OpenBracketToken,
                default(SyntaxTokenList),
                rankSpecifiers.Last().CloseBracketToken,
                newInitializer);

            return await document.ReplaceNodeAsync(arrayCreation, implicitArrayCreation, cancellationToken).ConfigureAwait(false);
        }
    }
}
