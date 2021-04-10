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
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BlockCodeFixProvider))]
    [Shared]
    public sealed class BlockCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.SimplifyLazyInitialization,
                    DiagnosticIdentifiers.RemoveUnnecessaryBraces);
            }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out BlockSyntax block))
                return;

            Document document = context.Document;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.SimplifyLazyInitialization:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Simplify lazy initialization",
                                ct => SimplifyLazyInitializationAsync(document, block, ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.RemoveUnnecessaryBraces:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Remove braces",
                                ct => RemoveBracesAsync(document, block, ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }
        }

        private static Task<Document> RemoveBracesAsync(
            Document document,
            BlockSyntax block,
            CancellationToken cancellationToken)
        {
            var switchSection = (SwitchSectionSyntax)block.Parent;

            SyntaxList<StatementSyntax> statements = block.Statements;

            SyntaxTriviaList leadingTrivia = block.OpenBraceToken.LeadingTrivia;

            leadingTrivia = AddTriviaIfNecessary(leadingTrivia, block.OpenBraceToken.TrailingTrivia);
            leadingTrivia = AddTriviaIfNecessary(leadingTrivia, statements[0].GetLeadingTrivia());

            SyntaxTriviaList trailingTrivia = statements.Last().GetTrailingTrivia();

            trailingTrivia = AddTriviaIfNecessary(trailingTrivia, block.CloseBraceToken.LeadingTrivia);
            trailingTrivia = AddTriviaIfNecessary(trailingTrivia, block.CloseBraceToken.TrailingTrivia);

            trailingTrivia = trailingTrivia.TrimEnd().Add(CSharpFactory.NewLine());

            var switchStatement = (SwitchStatementSyntax)switchSection.Parent;

            if (!switchStatement.Sections.IsLast(switchSection))
                trailingTrivia = trailingTrivia.Add(CSharpFactory.NewLine());

            SyntaxList<StatementSyntax> newStatements = statements.ReplaceAt(0, statements[0].WithLeadingTrivia(leadingTrivia));

            newStatements = newStatements.ReplaceAt(newStatements.Count - 1, newStatements.Last().WithTrailingTrivia(trailingTrivia));

            SwitchSectionSyntax newSwitchSection = switchSection
                .WithStatements(newStatements)
                .WithFormatterAnnotation();

            return document.ReplaceNodeAsync(switchSection, newSwitchSection, cancellationToken);

            static SyntaxTriviaList AddTriviaIfNecessary(SyntaxTriviaList trivia, SyntaxTriviaList triviaToAdd)
            {
                if (triviaToAdd.Any(f => f.IsKind(SyntaxKind.SingleLineCommentTrivia)))
                    trivia = trivia.AddRange(triviaToAdd);

                return trivia;
            }
        }

        private static Task<Document> SimplifyLazyInitializationAsync(
            Document document,
            BlockSyntax block,
            CancellationToken cancellationToken)
        {
            SyntaxList<StatementSyntax> statements = block.Statements;

            var ifStatement = (IfStatementSyntax)statements[0];

            var returnStatement = (ReturnStatementSyntax)statements[1];

            var expressionStatement = (ExpressionStatementSyntax)ifStatement.SingleNonBlockStatementOrDefault();

            var assignment = (AssignmentExpressionSyntax)expressionStatement.Expression;

            ExpressionSyntax expression = returnStatement.Expression;

            IdentifierNameSyntax valueName = null;

            if (expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
            {
                var memberAccess = (MemberAccessExpressionSyntax)expression;

                if ((memberAccess.Name is IdentifierNameSyntax identifierName)
                    && string.Equals(identifierName.Identifier.ValueText, "Value", StringComparison.Ordinal))
                {
                    expression = memberAccess.Expression;
                    valueName = identifierName;
                }
            }

            expression = expression.WithoutTrivia();

            ExpressionSyntax coalesceExpression;

            if (document.SupportsLanguageFeature(CSharpLanguageFeature.NullCoalescingAssignmentOperator))
            {
                coalesceExpression = CoalesceAssignmentExpression(expression, assignment.Right.WithoutTrivia());
            }
            else
            {
                ExpressionSyntax right = SimpleAssignmentExpression(expression, assignment.Right.WithoutTrivia()).Parenthesize();

                if (valueName != null)
                    right = SimpleMemberAccessExpression(right.Parenthesize(), valueName);

                coalesceExpression = CoalesceExpression(expression, right);
            }

            ReturnStatementSyntax newReturnStatement = returnStatement
                .WithExpression(coalesceExpression)
                .WithLeadingTrivia(ifStatement.GetLeadingTrivia());

            SyntaxList<StatementSyntax> newStatements = statements
                .Replace(returnStatement, newReturnStatement)
                .RemoveAt(0);

            BlockSyntax newBlock = block.WithStatements(newStatements);

            return document.ReplaceNodeAsync(block, newBlock, cancellationToken);
        }
    }
}