// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UsePatternMatchingInsteadOfAsAndNullCheckCodeFixProvider))]
    [Shared]
    public sealed class UsePatternMatchingInsteadOfAsAndNullCheckCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.UsePatternMatchingInsteadOfAsAndNullCheck); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out LocalDeclarationStatementSyntax localDeclaration))
                return;

            Diagnostic diagnostic = context.Diagnostics[0];

            CodeAction codeAction = CodeAction.Create(
                "Use pattern matching",
                ct => RefactorAsync(context.Document, localDeclaration, ct),
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        private static Task<Document> RefactorAsync(
            Document document,
            LocalDeclarationStatementSyntax localDeclaration,
            CancellationToken cancellationToken)
        {
            VariableDeclaratorSyntax variableDeclarator = localDeclaration
                .Declaration
                .Variables
                .Single();

            ExpressionSyntax expression = variableDeclarator
                .Initializer
                .Value
                .WalkDownParentheses();

            AsExpressionInfo asExpressionInfo = SyntaxInfo.AsExpressionInfo(expression);

            PrefixUnaryExpressionSyntax newCondition = LogicalNotExpression(
                ParenthesizedExpression(
                    IsPatternExpression(
                        asExpressionInfo.Expression,
                        DeclarationPattern(
                            asExpressionInfo.Type,
                            SingleVariableDesignation(variableDeclarator.Identifier)))));

            StatementListInfo statementsInfo = SyntaxInfo.StatementListInfo(localDeclaration);

            int index = statementsInfo.IndexOf(localDeclaration);

            var ifStatement = (IfStatementSyntax)statementsInfo[index + 1];

            SyntaxTriviaList leadingTrivia = statementsInfo.Parent
                .DescendantTrivia(TextSpan.FromBounds(localDeclaration.SpanStart, ifStatement.SpanStart))
                .ToSyntaxTriviaList()
                .EmptyIfWhitespace();

            leadingTrivia = localDeclaration.GetLeadingTrivia().AddRange(leadingTrivia);

            StatementSyntax newStatement = ifStatement.Statement;

            if (ifStatement.SingleNonBlockStatementOrDefault() is ReturnStatementSyntax returnStatement
                && returnStatement.Expression?.WalkDownParentheses() is IdentifierNameSyntax identifierName
                && string.Equals(identifierName.Identifier.ValueText, variableDeclarator.Identifier.ValueText, System.StringComparison.Ordinal))
            {
                newStatement = newStatement.ReplaceNode(returnStatement.Expression, NullLiteralExpression().WithTriviaFrom(returnStatement.Expression));
            }

            IfStatementSyntax newIfStatement = ifStatement
                .WithCondition(newCondition.WithTriviaFrom(ifStatement.Condition))
                .WithStatement(newStatement)
                .WithLeadingTrivia(leadingTrivia)
                .WithFormatterAnnotation();

            SyntaxList<StatementSyntax> newStatements = statementsInfo.Statements.ReplaceRange(index, 2, newIfStatement);

            return document.ReplaceStatementsAsync(statementsInfo, newStatements, cancellationToken);
        }
    }
}