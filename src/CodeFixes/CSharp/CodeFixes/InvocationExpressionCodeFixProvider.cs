// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InvocationExpressionCodeFixProvider))]
    [Shared]
    public sealed class InvocationExpressionCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CompilerDiagnosticIdentifiers.NonInvocableMemberCannotBeUsedLikeMethod); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics[0];

            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveArgumentList))
                return;

            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out InvocationExpressionSyntax invocationExpression))
                return;

            CodeAction codeAction = CodeAction.Create(
                "Remove argument list",
                cancellationToken =>
                {
                    ExpressionSyntax newNode = invocationExpression.Expression
                        .AppendToTrailingTrivia(invocationExpression.ArgumentList.GetTrailingTrivia())
                        .WithFormatterAnnotation();

                    return context.Document.ReplaceNodeAsync(invocationExpression, newNode, cancellationToken);
                },
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
        }
    }
}
