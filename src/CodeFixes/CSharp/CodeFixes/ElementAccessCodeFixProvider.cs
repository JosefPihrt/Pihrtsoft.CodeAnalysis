// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ElementAccessCodeFixProvider))]
    [Shared]
    public sealed class ElementAccessCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CompilerDiagnosticIdentifiers.CannotApplyIndexingToExpression); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics[0];

            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.ReplaceElementAccessWithInvocation))
                return;

            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out ElementAccessExpressionSyntax elementAccess))
                return;

            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

            BracketedArgumentListSyntax argumentList = elementAccess.ArgumentList;

            SyntaxToken openBracket = argumentList.OpenBracketToken;
            SyntaxToken closeBracket = argumentList.CloseBracketToken;

            InvocationExpressionSyntax invocationExpression = InvocationExpression(
                elementAccess.Expression,
                ArgumentList(
                    Token(openBracket.LeadingTrivia, SyntaxKind.OpenParenToken, openBracket.TrailingTrivia),
                    argumentList.Arguments,
                    Token(closeBracket.LeadingTrivia, SyntaxKind.CloseParenToken, closeBracket.TrailingTrivia)));

            if (semanticModel.GetSpeculativeMethodSymbol(elementAccess.SpanStart, invocationExpression) == null)
                return;

            CodeAction codeAction = CodeAction.Create(
                "Replace [] with ()",
                cancellationToken => context.Document.ReplaceNodeAsync(elementAccess, invocationExpression, cancellationToken),
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
        }
    }
}
