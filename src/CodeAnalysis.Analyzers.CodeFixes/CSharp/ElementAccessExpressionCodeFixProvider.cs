// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CodeAnalysis.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ElementAccessExpressionCodeFixProvider))]
    [Shared]
    public class ElementAccessExpressionCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.CallLastInsteadOfUsingElementAccess); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out ElementAccessExpressionSyntax elementAccessExpression))
                return;

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];

            CodeAction codeAction = CodeAction.Create(
                "Call 'Last' instead of using []",
                ct => CallLastInsteadOfUsingElementAccessAsync(document, elementAccessExpression, ct),
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        private static Task<Document> CallLastInsteadOfUsingElementAccessAsync(
            Document document,
            ElementAccessExpressionSyntax elementAccessExpression,
            CancellationToken cancellationToken)
        {
            InvocationExpressionSyntax invocationExpression = SimpleMemberInvocationExpression(
                elementAccessExpression.Expression,
                IdentifierName("Last"),
                ArgumentList().WithTriviaFrom(elementAccessExpression.ArgumentList));

            return document.ReplaceNodeAsync(elementAccessExpression, invocationExpression, cancellationToken);
        }
    }
}
