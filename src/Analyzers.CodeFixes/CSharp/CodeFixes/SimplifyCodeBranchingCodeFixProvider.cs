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
using Roslynator.CSharp.Analysis;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SimplifyCodeBranchingCodeFixProvider))]
    [Shared]
    public class SimplifyCodeBranchingCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.SimplifyCodeBranching); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out IfStatementSyntax ifStatement))
                return;

            Diagnostic diagnostic = context.Diagnostics[0];

            Document document = context.Document;

            CodeAction codeAction = CodeAction.Create(
                "Simplify code branching",
                ct => RefactorAsync(document, ifStatement, ct),
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        private static async Task<Document> RefactorAsync(
            Document document,
            IfStatementSyntax ifStatement,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ExpressionSyntax condition = ifStatement.Condition;

            ElseClauseSyntax elseClause = ifStatement.Else;

            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            SimplifyCodeBranchingKind kind = SimplifyCodeBranchingAnalyzer.GetKind(ifStatement, semanticModel, cancellationToken).Value;

            if (kind == SimplifyCodeBranchingKind.IfElseWithEmptyIf)
            {
                ExpressionSyntax newCondition = SyntaxInverter.LogicallyInvert(condition, semanticModel, cancellationToken);

                StatementSyntax statement = elseClause.Statement;

                if (statement is IfStatementSyntax nestedIf)
                {
                    newCondition = LogicalAndExpression(newCondition.Parenthesize(), nestedIf.Condition.Parenthesize());

                    statement = nestedIf.Statement;
                }

                IfStatementSyntax newIfStatement = ifStatement.Update(
                    ifStatement.IfKeyword,
                    ifStatement.OpenParenToken,
                    newCondition,
                    ifStatement.CloseParenToken,
                    statement,
                    default(ElseClauseSyntax));

                newIfStatement = newIfStatement.WithFormatterAnnotation();

                return await document.ReplaceNodeAsync(ifStatement, newIfStatement, cancellationToken).ConfigureAwait(false);
            }
            else if (kind == SimplifyCodeBranchingKind.IfElseInsideWhile)
            {
                bool elseContainsBreak = elseClause.SingleNonBlockStatementOrDefault()?.Kind() == SyntaxKind.BreakStatement;

                SyntaxList<StatementSyntax> statements;

                if (elseContainsBreak)
                {
                    statements = (ifStatement.Statement is BlockSyntax block2)
                        ? block2.Statements
                        : SingletonList(ifStatement.Statement);
                }
                else
                {
                    statements = (elseClause.Statement is BlockSyntax block2)
                        ? block2.Statements
                        : SingletonList(elseClause.Statement);
                }

                WhileStatementSyntax whileStatement;

                if (ifStatement.Parent is BlockSyntax block)
                {
                    whileStatement = (WhileStatementSyntax)block.Parent;

                    block = block.WithStatements(block.Statements.ReplaceRange(ifStatement, statements));
                }
                else
                {
                    whileStatement = (WhileStatementSyntax)ifStatement.Parent;

                    block = Block(statements);
                }

                if (!elseContainsBreak)
                    condition = SyntaxInverter.LogicallyInvert(condition, semanticModel, cancellationToken);

                WhileStatementSyntax newWhileStatement = whileStatement.Update(
                    whileStatement.WhileKeyword,
                    whileStatement.OpenParenToken,
                    condition,
                    whileStatement.CloseParenToken,
                    block);

                newWhileStatement = newWhileStatement.WithFormatterAnnotation();

                return await document.ReplaceNodeAsync(whileStatement, newWhileStatement, cancellationToken).ConfigureAwait(false);
            }
            else if (kind == SimplifyCodeBranchingKind.SimplifyIfInsideWhileOrDo)
            {
                var block = (BlockSyntax)ifStatement.Parent;

                SyntaxList<StatementSyntax> statements = block.Statements;

                BlockSyntax newBlock = block.WithStatements(statements.Remove(ifStatement));

                ExpressionSyntax newCondition = SyntaxInverter.LogicallyInvert(condition, semanticModel, cancellationToken);

                SyntaxNode newNode;

                switch (block.Parent)
                {
                    case WhileStatementSyntax whileStatement:
                        {
                            if (statements.IsFirst(ifStatement))
                            {
                                newNode = whileStatement.Update(
                                    whileStatement.WhileKeyword,
                                    whileStatement.OpenParenToken,
                                    newCondition,
                                    whileStatement.CloseParenToken,
                                    newBlock);
                            }
                            else
                            {
                                newNode = DoStatement(
                                    Token(whileStatement.WhileKeyword.LeadingTrivia, SyntaxKind.DoKeyword, whileStatement.CloseParenToken.TrailingTrivia),
                                    newBlock.WithoutTrailingTrivia(),
                                    Token(SyntaxKind.WhileKeyword),
                                    OpenParenToken(),
                                    newCondition,
                                    CloseParenToken(),
                                    SemicolonToken().WithTrailingTrivia(newBlock.GetTrailingTrivia()));
                            }

                            break;
                        }
                    case DoStatementSyntax doStatement:
                        {
                            if (statements.IsLast(ifStatement))
                            {
                                newNode = doStatement.Update(
                                    doStatement.DoKeyword,
                                    newBlock,
                                    doStatement.WhileKeyword,
                                    doStatement.OpenParenToken,
                                    newCondition,
                                    doStatement.CloseParenToken,
                                    doStatement.SemicolonToken);
                            }
                            else
                            {
                                newNode = WhileStatement(
                                    Token(doStatement.DoKeyword.LeadingTrivia, SyntaxKind.WhileKeyword, SyntaxTriviaList.Empty),
                                    OpenParenToken(),
                                    newCondition,
                                    Token(SyntaxTriviaList.Empty, SyntaxKind.CloseParenToken, doStatement.DoKeyword.TrailingTrivia),
                                    newBlock.WithTrailingTrivia(doStatement.GetTrailingTrivia()));
                            }

                            break;
                        }
                    default:
                        {
                            throw new InvalidOperationException();
                        }
                }

                newNode = newNode.WithFormatterAnnotation();

                return await document.ReplaceNodeAsync(block.Parent, newNode, cancellationToken).ConfigureAwait(false);
            }
            else if (kind == SimplifyCodeBranchingKind.SimpleIfContainingOnlyDo)
            {
                StatementSyntax statement = ifStatement.SingleNonBlockStatementOrDefault();

                var doStatement = (DoStatementSyntax)statement;

                WhileStatementSyntax whileStatement = WhileStatement(
                    Token(ifStatement.GetLeadingTrivia(), SyntaxKind.WhileKeyword, SyntaxTriviaList.Empty),
                    OpenParenToken(),
                    doStatement.Condition,
                    Token(SyntaxTriviaList.Empty, SyntaxKind.CloseParenToken, doStatement.DoKeyword.TrailingTrivia),
                    doStatement.Statement.WithTrailingTrivia(ifStatement.GetTrailingTrivia()));

                whileStatement = whileStatement.WithFormatterAnnotation();

                return await document.ReplaceNodeAsync(ifStatement, whileStatement, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
