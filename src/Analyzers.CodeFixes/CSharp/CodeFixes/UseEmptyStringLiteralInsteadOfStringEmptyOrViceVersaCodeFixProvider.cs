// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseEmptyStringLiteralInsteadOfStringEmptyOrViceVersaCodeFixProvider))]
    [Shared]
    public sealed class UseEmptyStringLiteralInsteadOfStringEmptyOrViceVersaCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.UseEmptyStringLiteralInsteadOfStringEmptyOrViceVersa); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(
                root,
                context.Span,
                out SyntaxNode node,
                predicate: f => f.IsKind(SyntaxKind.SimpleMemberAccessExpression, SyntaxKind.StringLiteralExpression, SyntaxKind.InterpolatedStringExpression)))
            {
                return;
            }

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];

            switch (node.Kind())
            {
                case SyntaxKind.SimpleMemberAccessExpression:
                    {
                        var memberAccessExpression = (MemberAccessExpressionSyntax)node;

                        CodeAction codeAction = CodeAction.Create(
                            $"Use \"\" instead of '{memberAccessExpression}'",
                            ct => UseEmptyStringLiteralInsteadOfStringEmptyAsync(document, memberAccessExpression, ct),
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
                case SyntaxKind.StringLiteralExpression:
                case SyntaxKind.InterpolatedStringExpression:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            "Use 'string.Empty' instead of \"\"",
                            ct => UseStringEmptyInsteadOfEmptyStringLiteralAsync(document, (ExpressionSyntax)node, ct),
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException();
                    }
            }
        }

        private static Task<Document> UseEmptyStringLiteralInsteadOfStringEmptyAsync(
            Document document,
            MemberAccessExpressionSyntax memberAccessExpression,
            CancellationToken cancellationToken = default)
        {
            LiteralExpressionSyntax newNode = CSharpFactory.StringLiteralExpression("").WithTriviaFrom(memberAccessExpression);

            return document.ReplaceNodeAsync(memberAccessExpression, newNode, cancellationToken);
        }

        private static Task<Document> UseStringEmptyInsteadOfEmptyStringLiteralAsync(
            Document document,
            ExpressionSyntax expression,
            CancellationToken cancellationToken = default)
        {
            MemberAccessExpressionSyntax memberAccessExpression = CSharpFactory.SimpleMemberAccessExpression(
                CSharpTypeFactory.StringType(),
                SyntaxFactory.IdentifierName("Empty"));

            memberAccessExpression = memberAccessExpression.WithTriviaFrom(expression);

            return document.ReplaceNodeAsync(expression, memberAccessExpression, cancellationToken);
        }
    }
}
