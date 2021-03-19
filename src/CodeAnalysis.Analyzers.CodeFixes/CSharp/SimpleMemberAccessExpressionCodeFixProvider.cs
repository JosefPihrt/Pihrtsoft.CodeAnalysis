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

namespace Roslynator.CodeAnalysis.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SimpleMemberAccessExpressionCodeFixProvider))]
    [Shared]
    public class SimpleMemberAccessExpressionCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.UsePropertySyntaxNodeSpanStart); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out MemberAccessExpressionSyntax memberAccess))
                return;

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];

            CodeAction codeAction = CodeAction.Create(
                "Use property 'SpanStart'",
                ct => UsePropertySyntaxNodeSpanStartAsync(document, memberAccess, ct),
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        private static Task<Document> UsePropertySyntaxNodeSpanStartAsync(
            Document document,
            MemberAccessExpressionSyntax memberAccess,
            CancellationToken cancellationToken)
        {
            var memberAccess2 = (MemberAccessExpressionSyntax)memberAccess.Expression;

            MemberAccessExpressionSyntax newMemberAccess = memberAccess2.WithName(SyntaxFactory.IdentifierName("SpanStart").WithTriviaFrom(memberAccess2.Name));

            return document.ReplaceNodeAsync(memberAccess, newMemberAccess, cancellationToken);
        }
    }
}
