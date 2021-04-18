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
using Roslynator.CSharp;
using Roslynator.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CodeAnalysis.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InvocationExpressionCodeFixProvider))]
    [Shared]
    public sealed class InvocationExpressionCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.UseElementAccess,
                    DiagnosticIdentifiers.UseReturnValue);
            }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out InvocationExpressionSyntax invocationExpression))
                return;

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];

            switch (diagnostic.Id)
            {
                case DiagnosticIdentifiers.UseElementAccess:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            "Use [] instead of calling 'First'",
                            ct => UseElementAccessInsteadOfCallingFirstAsync(document, invocationExpression, ct),
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
                case DiagnosticIdentifiers.UseReturnValue:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            $"Introduce local for '{invocationExpression}'",
                            ct => IntroduceLocalForExpressionAsync(document, invocationExpression, ct),
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);

                        break;
                    }
            }
        }

        private static Task<Document> UseElementAccessInsteadOfCallingFirstAsync(
            Document document,
            InvocationExpressionSyntax invocationExpression,
            CancellationToken cancellationToken)
        {
            SimpleMemberInvocationExpressionInfo invocationInfo = SyntaxInfo.SimpleMemberInvocationExpressionInfo(invocationExpression);

            ArgumentListSyntax argumentList = invocationInfo.ArgumentList;

            SyntaxToken openParenToken = argumentList.OpenParenToken;
            SyntaxToken closeParenToken = argumentList.CloseParenToken;

            ElementAccessExpressionSyntax elementAccessExpression = ElementAccessExpression(
                invocationInfo.Expression.WithoutTrailingTrivia(),
                BracketedArgumentList(
                    Token(SyntaxTriviaList.Empty, SyntaxKind.OpenBracketToken, openParenToken.TrailingTrivia),
                    SingletonSeparatedList(argumentList.Arguments.FirstOrDefault() ?? Argument(NumericLiteralExpression(0))),
                    Token(closeParenToken.LeadingTrivia, SyntaxKind.CloseBracketToken, closeParenToken.TrailingTrivia)));

            return document.ReplaceNodeAsync(invocationExpression, elementAccessExpression, cancellationToken);
        }

        private static async Task<Document> IntroduceLocalForExpressionAsync(
            Document document,
            InvocationExpressionSyntax invocationExpression,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            string name = NameGenerator.Default.EnsureUniqueLocalName(DefaultNames.Variable, semanticModel, invocationExpression.SpanStart, cancellationToken: cancellationToken);

            LocalDeclarationStatementSyntax localDeclarationStatement = LocalDeclarationStatement(
                VarType(),
                Identifier(name).WithRenameAnnotation(),
                EqualsValueClause(invocationExpression).WithFormatterAnnotation());

            return await document.ReplaceNodeAsync(invocationExpression.Parent, localDeclarationStatement, cancellationToken).ConfigureAwait(false);
        }
    }
}
