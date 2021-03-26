// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
using Roslynator.CSharp;
using Roslynator.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CodeAnalysis.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UsePatternMatchingCodeFixProvider))]
    [Shared]
    public class UsePatternMatchingCodeFixProvider : BaseCodeFixProvider
    {
        private const string Title = "Use pattern matching";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.UsePatternMatching); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out SyntaxNode node, predicate: f => f.IsKind(SyntaxKind.SwitchStatement, SyntaxKind.IfStatement)))
                return;

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];

            switch (node)
            {
                case SwitchStatementSyntax switchStatement:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            Title,
                            ct => UsePatternMatchingAsync(document, switchStatement, ct),
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
                case IfStatementSyntax ifStatement:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            Title,
                            ct => UsePatternMatchingAsync(document, ifStatement, ct),
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
            }
        }

        private static Task<Document> UsePatternMatchingAsync(
            Document document,
            SwitchStatementSyntax switchStatement,
            CancellationToken cancellationToken)
        {
            SyntaxList<SwitchSectionSyntax> newSections = switchStatement.Sections.Select(section =>
            {
                if (!(section.Labels.Single() is CaseSwitchLabelSyntax label))
                    return section;

                SyntaxList<StatementSyntax> statements = section.Statements;

                StatementSyntax statement = statements[0];

                if (statement is BlockSyntax block)
                    statement = block.Statements.FirstOrDefault();

                SingleLocalDeclarationStatementInfo localInfo = SyntaxInfo.SingleLocalDeclarationStatementInfo((LocalDeclarationStatementSyntax)statement);

                var castExpression = (CastExpressionSyntax)localInfo.Value;

                CasePatternSwitchLabelSyntax newLabel = CasePatternSwitchLabel(
                    DeclarationPattern(
                        castExpression.Type,
                        SingleVariableDesignation(localInfo.Identifier)),
                    label.ColonToken);

                SwitchSectionSyntax newSection = section.RemoveStatement(localInfo.Statement);

                newSection = newSection.WithLabels(newSection.Labels.ReplaceAt(0, newLabel));

                return newSection.WithFormatterAnnotation();
            })
                .ToSyntaxList();

            ExpressionSyntax expression = switchStatement.Expression;

            ExpressionSyntax newExpression = expression;

            LocalDeclarationStatementSyntax localDeclaration = null;

            if (expression.IsKind(SyntaxKind.InvocationExpression))
            {
                SimpleMemberInvocationExpressionInfo invocationInfo = SyntaxInfo.SimpleMemberInvocationExpressionInfo(expression);

                newExpression = invocationInfo.Expression;
            }
            else
            {
                localDeclaration = (LocalDeclarationStatementSyntax)switchStatement.PreviousStatement();

                SingleLocalDeclarationStatementInfo localInfo = SyntaxInfo.SingleLocalDeclarationStatementInfo(localDeclaration);

                SimpleMemberInvocationExpressionInfo invocationInfo = SyntaxInfo.SimpleMemberInvocationExpressionInfo(localInfo.Value);

                newExpression = invocationInfo.Expression;
            }

            SwitchStatementSyntax newSwitchStatement = switchStatement
                .WithExpression(newExpression.WithTriviaFrom(expression))
                .WithSections(newSections);

            if (localDeclaration != null)
            {
                StatementListInfo statementsInfo = SyntaxInfo.StatementListInfo(switchStatement);

                newSwitchStatement = newSwitchStatement.WithLeadingTrivia(localDeclaration.GetLeadingTrivia());

                SyntaxList<StatementSyntax> newStatements = statementsInfo.Statements
                    .Replace(switchStatement, newSwitchStatement)
                    .RemoveAt(statementsInfo.IndexOf(localDeclaration));

                return document.ReplaceStatementsAsync(statementsInfo, newStatements, cancellationToken);
            }
            else
            {
                return document.ReplaceNodeAsync(switchStatement, newSwitchStatement, cancellationToken);
            }
        }

        private static async Task<Document> UsePatternMatchingAsync(
            Document document,
            IfStatementSyntax ifStatement,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            IsKindExpressionInfo isKindExpression = IsKindExpressionInfo.Create(ifStatement.Condition, semanticModel, cancellationToken: cancellationToken);

            switch (isKindExpression.Style)
            {
                case IsKindExpressionStyle.IsKind:
                case IsKindExpressionStyle.IsKindConditional:
                case IsKindExpressionStyle.Kind:
                case IsKindExpressionStyle.KindConditional:
                    {
                        var block = (BlockSyntax)ifStatement.Statement;

                        IsPatternExpressionSyntax isPatternExpression = CreateIsPatternExpression(block.Statements[0]);

                        BlockSyntax newBlock = block.WithStatements(block.Statements.RemoveAt(0));

                        IfStatementSyntax newIfStatement = ifStatement.Update(
                            ifStatement.IfKeyword,
                            ifStatement.OpenParenToken,
                            isPatternExpression,
                            ifStatement.CloseParenToken,
                            newBlock,
                            ifStatement.Else);

                        newIfStatement = newIfStatement.WithFormatterAnnotation();

                        return await document.ReplaceNodeAsync(ifStatement, newIfStatement, cancellationToken).ConfigureAwait(false);
                    }
                case IsKindExpressionStyle.NotIsKind:
                case IsKindExpressionStyle.NotIsKindConditional:
                case IsKindExpressionStyle.NotKind:
                case IsKindExpressionStyle.NotKindConditional:
                    {
                        StatementListInfo statementsInfo = SyntaxInfo.StatementListInfo(ifStatement);

                        SyntaxList<StatementSyntax> statements = statementsInfo.Statements;

                        int index = statements.IndexOf(ifStatement);

                        IsPatternExpressionSyntax isPatternExpression = CreateIsPatternExpression(statements[index + 1]);

                        IfStatementSyntax newIfStatement = ifStatement.WithCondition(LogicalNotExpression(isPatternExpression.Parenthesize()).WithTriviaFrom(ifStatement.Condition));

                        SyntaxList<StatementSyntax> newStatements = statements
                            .ReplaceAt(index, newIfStatement)
                            .RemoveAt(index + 1);

                        return await document.ReplaceStatementsAsync(statementsInfo, newStatements, cancellationToken).ConfigureAwait(false);
                    }
                default:
                    {
                        throw new InvalidOperationException();
                    }
            }

            IsPatternExpressionSyntax CreateIsPatternExpression(StatementSyntax statement)
            {
                SingleLocalDeclarationStatementInfo localInfo = SyntaxInfo.SingleLocalDeclarationStatementInfo((LocalDeclarationStatementSyntax)statement);

                var castExpression = (CastExpressionSyntax)localInfo.Value;

                return IsPatternExpression(
                    isKindExpression.Expression,
                    DeclarationPattern(castExpression.Type, SingleVariableDesignation(localInfo.Identifier)));
            }
        }
    }
}
