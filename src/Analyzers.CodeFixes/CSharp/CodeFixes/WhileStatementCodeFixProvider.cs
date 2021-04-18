// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(WhileStatementCodeFixProvider))]
    [Shared]
    public sealed class WhileStatementCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.AvoidUsageOfWhileStatementToCreateInfiniteLoop,
                    DiagnosticIdentifiers.UseForStatementInsteadOfWhileStatement);
            }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out WhileStatementSyntax whileStatement))
                return;

            Document document = context.Document;

            Diagnostic diagnostic = context.Diagnostics[0];

            switch (diagnostic.Id)
            {
                case DiagnosticIdentifiers.AvoidUsageOfWhileStatementToCreateInfiniteLoop:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            "Convert to 'for'",
                            ct =>
                            {
                                ForStatementSyntax forStatement = SyntaxRefactorings.ConvertWhileStatementToForStatement(whileStatement)
                                    .WithFormatterAnnotation();

                                return document.ReplaceNodeAsync(whileStatement, forStatement, ct);
                            },
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
                case DiagnosticIdentifiers.UseForStatementInsteadOfWhileStatement:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            "Convert to 'for'",
                            ct =>
                            {
                                return ConvertWhileStatementToForStatementAsync(
                                    document,
                                    whileStatement,
                                    ct);
                            },
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
            }
        }

        private static Task<Document> ConvertWhileStatementToForStatementAsync(
            Document document,
            WhileStatementSyntax whileStatement,
            CancellationToken cancellationToken)
        {
            StatementListInfo statementsInfo = SyntaxInfo.StatementListInfo(whileStatement);

            SyntaxList<StatementSyntax> statements = statementsInfo.Statements;

            int index = statements.IndexOf(whileStatement);

            SingleLocalDeclarationStatementInfo localInfo = SyntaxInfo.SingleLocalDeclarationStatementInfo((LocalDeclarationStatementSyntax)statements[index - 1]);

            var block = (BlockSyntax)whileStatement.Statement;

            var expressionStatement = (ExpressionStatementSyntax)block.Statements.Last();

            var postIncrementExpression = (PostfixUnaryExpressionSyntax)expressionStatement.Expression;

            BlockSyntax newBlock = block.WithStatements(block.Statements.Remove(expressionStatement));

            ForStatementSyntax forStatement = CSharpFactory.ForStatement(
                declaration: localInfo.Declaration.TrimTrivia(),
                condition: whileStatement.Condition,
                incrementor: postIncrementExpression.TrimTrivia(),
                statement: newBlock);

            forStatement = forStatement
                .WithLeadingTrivia(localInfo.Statement.GetLeadingTrivia().AddRange(whileStatement.GetLeadingTrivia().EmptyIfWhitespace()))
                .WithFormatterAnnotation();

            SyntaxList<StatementSyntax> newStatements = statements.ReplaceRange(index - 1, 2, new StatementSyntax[] { forStatement });

            return document.ReplaceStatementsAsync(statementsInfo, newStatements, cancellationToken);
        }
    }
}
