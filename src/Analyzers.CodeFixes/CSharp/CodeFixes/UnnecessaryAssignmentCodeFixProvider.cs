// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
using Microsoft.CodeAnalysis.FindSymbols;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UnnecessaryAssignmentCodeFixProvider))]
    [Shared]
    public sealed class UnnecessaryAssignmentCodeFixProvider : BaseCodeFixProvider
    {
        private const string Title = "Remove unnecessary assignment";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.UnnecessaryAssignment); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out StatementSyntax statement, predicate: f => f.IsKind(SyntaxKind.IfStatement, SyntaxKind.SwitchStatement)))
                return;

            Document document = context.Document;

            Diagnostic diagnostic = context.Diagnostics[0];

            switch (statement)
            {
                case IfStatementSyntax ifStatement:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            Title,
                            ct => RefactorAsync(document, ifStatement, ct),
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
                case SwitchStatementSyntax switchStatement:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            Title,
                            ct => RefactorAsync(document, switchStatement, ct),
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
            }
        }

        private static async Task<Document> RefactorAsync(
            Document document,
            IfStatementSyntax ifStatement,
            CancellationToken cancellationToken)
        {
            StatementListInfo statementsInfo = SyntaxInfo.StatementListInfo(ifStatement);

            IfStatementCascadeInfo ifCascadeInfo = ifStatement.GetCascadeInfo();

            StatementListInfo newStatementsInfo = await RefactorAsync(
                document,
                ifStatement,
                statementsInfo,
                f => CreateNewIfStatement(f),
                ifCascadeInfo.Count,
                ifCascadeInfo.EndsWithElse,
                cancellationToken)
                .ConfigureAwait(false);

            return await document.ReplaceNodeAsync(statementsInfo.Parent, newStatementsInfo.Parent, cancellationToken).ConfigureAwait(false);
        }

        private static IfStatementSyntax CreateNewIfStatement(IfStatementSyntax ifStatement)
        {
            IEnumerable<StatementSyntax> statements = ifStatement
                .AsCascade()
                .Select(ifOrElse =>
                {
                    StatementSyntax statement = ifOrElse.Statement;

                    if (statement is BlockSyntax block)
                        statement = block.Statements.Last();

                    return statement;
                });

            return ifStatement.ReplaceNodes(
                statements,
                (statement, _) =>
                {
                    if (statement.IsKind(SyntaxKind.ThrowStatement))
                        return statement;

                    var expressionStatement = (ExpressionStatementSyntax)statement;

                    var assignment = (AssignmentExpressionSyntax)expressionStatement.Expression;

                    return ReturnStatement(assignment.Right).WithTriviaFrom(statement);
                });
        }

        private static async Task<Document> RefactorAsync(
            Document document,
            SwitchStatementSyntax switchStatement,
            CancellationToken cancellationToken)
        {
            StatementListInfo statementsInfo = SyntaxInfo.StatementListInfo(switchStatement);

            StatementListInfo newStatementsInfo = await RefactorAsync(
                document,
                switchStatement,
                statementsInfo,
                f => CreateNewSwitchStatement(f),
                switchStatement.Sections.Count,
                switchStatement.Sections.Any(f => f.ContainsDefaultLabel()),
                cancellationToken)
                .ConfigureAwait(false);

            return await document.ReplaceNodeAsync(statementsInfo.Parent, newStatementsInfo.Parent, cancellationToken).ConfigureAwait(false);
        }

        private static SwitchStatementSyntax CreateNewSwitchStatement(SwitchStatementSyntax switchStatement)
        {
            SyntaxList<SwitchSectionSyntax> newSections = switchStatement
                .Sections
                .Select(f => CreateNewSection(f))
                .ToSyntaxList();

            return switchStatement.WithSections(newSections);

            static SwitchSectionSyntax CreateNewSection(SwitchSectionSyntax section)
            {
                SyntaxList<StatementSyntax> statements = section.GetStatements();

                if (statements.Last().IsKind(SyntaxKind.ThrowStatement))
                    return section;

                var expressionStatement = (ExpressionStatementSyntax)statements.LastButOne();

                var assignment = (AssignmentExpressionSyntax)expressionStatement.Expression;

                section = section.ReplaceNode(expressionStatement, ReturnStatement(assignment.Right).WithTriviaFrom(expressionStatement));

                return section.RemoveStatement(section.GetStatements().Last());
            }
        }

        private static async Task<StatementListInfo> RefactorAsync<TStatement>(
            Document document,
            TStatement statement,
            StatementListInfo statementsInfo,
            Func<TStatement, TStatement> createNewStatement,
            int count,
            bool removeReturnStatement,
            CancellationToken cancellationToken) where TStatement : StatementSyntax
        {
            int statementIndex = statementsInfo.IndexOf(statement);

            var returnStatement = (ReturnStatementSyntax)statementsInfo[statementIndex + 1];

            ExpressionSyntax returnExpression = returnStatement.Expression;
            ExpressionSyntax newReturnExpression = null;
            SyntaxTriviaList newTrailingTrivia = default;

            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            ISymbol symbol = semanticModel.GetSymbol(returnExpression, cancellationToken);

            if (symbol.Kind == SymbolKind.Local
                && statementIndex > 0
                && statementsInfo[statementIndex - 1] is LocalDeclarationStatementSyntax localDeclarationStatement
                && !localDeclarationStatement.ContainsDiagnostics
                && !localDeclarationStatement.SpanOrTrailingTriviaContainsDirectives()
                && !statement.GetLeadingTrivia().Any(f => f.IsDirective))
            {
                SeparatedSyntaxList<VariableDeclaratorSyntax> declarators = localDeclarationStatement.Declaration.Variables;

                VariableDeclaratorSyntax declarator = declarators.FirstOrDefault(f => SymbolEqualityComparer.Default.Equals(semanticModel.GetDeclaredSymbol(f, cancellationToken), symbol));

                if (declarator != null)
                {
                    ExpressionSyntax value = declarator.Initializer?.Value;

                    if (removeReturnStatement || value != null)
                    {
                        IEnumerable<ReferencedSymbol> referencedSymbols = await SymbolFinder.FindReferencesAsync(symbol, document.Solution(), cancellationToken).ConfigureAwait(false);

                        if (referencedSymbols.First().Locations.Count() == count + 1)
                        {
                            newReturnExpression = value;

                            if (declarators.Count == 1)
                            {
                                if (!removeReturnStatement
                                    && returnStatement.GetTrailingTrivia().IsEmptyOrWhitespace())
                                {
                                    SyntaxTriviaList trailingTrivia = localDeclarationStatement.GetTrailingTrivia();

                                    if (trailingTrivia
                                        .SkipWhile(f => f.IsWhitespaceTrivia())
                                        .FirstOrDefault()
                                        .IsKind(SyntaxKind.SingleLineCommentTrivia))
                                    {
                                        newTrailingTrivia = trailingTrivia;
                                    }
                                }

                                SyntaxRemoveOptions removeOptions = SyntaxRefactorings.GetRemoveOptions(localDeclarationStatement);

                                if (newTrailingTrivia.Any())
                                    removeOptions &= ~SyntaxRemoveOptions.KeepTrailingTrivia;

                                statementsInfo = statementsInfo.RemoveNode(localDeclarationStatement, removeOptions);
                                statementIndex--;
                            }
                            else
                            {
                                statementsInfo = statementsInfo.ReplaceNode(localDeclarationStatement, localDeclarationStatement.RemoveNode(declarator, SyntaxRefactorings.GetRemoveOptions(declarator)));
                            }

                            returnStatement = (ReturnStatementSyntax)statementsInfo[statementIndex + 1];
                        }
                    }
                }
            }

            if (removeReturnStatement)
            {
                statementsInfo = statementsInfo.RemoveNode(returnStatement, SyntaxRefactorings.GetRemoveOptions(returnStatement));
            }
            else if (newReturnExpression != null)
            {
                ReturnStatementSyntax newReturnStatement = returnStatement.WithExpression(newReturnExpression.WithTriviaFrom(returnExpression));

                if (newTrailingTrivia.Any())
                    newReturnStatement = newReturnStatement.WithTrailingTrivia(newTrailingTrivia);

                statementsInfo = statementsInfo.ReplaceNode(returnStatement, newReturnStatement);
            }

            StatementSyntax oldNode = statementsInfo[statementIndex];

            TStatement newNode = createNewStatement((TStatement)oldNode).WithFormatterAnnotation();

            return statementsInfo.ReplaceNode(oldNode, newNode);
        }
    }
}
