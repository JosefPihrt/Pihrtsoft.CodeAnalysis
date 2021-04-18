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
using Roslynator.CSharp.Analysis;
using Roslynator.CSharp.Refactorings;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConvertAnonymousFunctionToMethodGroupOrViceVersaCodeFixProvider))]
    [Shared]
    public sealed class ConvertAnonymousFunctionToMethodGroupOrViceVersaCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.ConvertAnonymousFunctionToMethodGroupOrViceVersa); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(
                root,
                context.Span,
                out SyntaxNode node,
                predicate: f => f is AnonymousFunctionExpressionSyntax || f.IsKind(SyntaxKind.IdentifierName, SyntaxKind.SimpleMemberAccessExpression)))
            {
                return;
            }

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];

            if (node is AnonymousFunctionExpressionSyntax anonymousFunction)
            {
                CodeAction codeAction = CodeAction.Create(
                    "Convert to method group",
                    ct => ConvertAnonymousFunctionToMethodGroupAsync(document, anonymousFunction, ct),
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
            else
            {
                CodeAction codeAction = CodeAction.Create(
                    "Convert to lambda",
                    ct => ConvertMethodGroupToAnonymousFunctionAsync(document, (ExpressionSyntax)node, ct),
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
        }

        private static async Task<Document> ConvertAnonymousFunctionToMethodGroupAsync(
            Document document,
            AnonymousFunctionExpressionSyntax anonymousFunction,
            CancellationToken cancellationToken)
        {
            InvocationExpressionSyntax invocationExpression = ConvertAnonymousFunctionToMethodGroupOrViceVersaAnalyzer.GetInvocationExpression(anonymousFunction.Body);

            ExpressionSyntax newNode = invocationExpression.Expression;

            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            var methodSymbol = (IMethodSymbol)semanticModel.GetSymbol(invocationExpression, cancellationToken);

            if (methodSymbol.IsReducedExtensionMethod())
                newNode = ((MemberAccessExpressionSyntax)newNode).Name;

            newNode = newNode.WithTriviaFrom(anonymousFunction);

            return await document.ReplaceNodeAsync(anonymousFunction, newNode, cancellationToken).ConfigureAwait(false);
        }

        private static async Task<Document> ConvertMethodGroupToAnonymousFunctionAsync(
            Document document,
            ExpressionSyntax expression,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            LambdaExpressionSyntax lambda = ConvertMethodGroupToAnonymousFunctionRefactoring.ConvertMethodGroupToAnonymousFunction(expression, semanticModel, cancellationToken);

            return await document.ReplaceNodeAsync(expression, lambda, cancellationToken).ConfigureAwait(false);
        }
    }
}
