// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UseCoalesceExpressionAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                {
                    Immutable.InterlockedInitialize(
                        ref _supportedDiagnostics,
                        DiagnosticRules.UseCoalesceExpression,
                        DiagnosticRules.InlineLazyInitialization);
                }

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeIfStatement(f), SyntaxKind.IfStatement);
        }

        private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;

            if (ifStatement.ContainsDiagnostics)
                return;

            if (ifStatement.SpanContainsDirectives())
                return;

            if (!ifStatement.IsSimpleIf())
                return;

            SyntaxList<StatementSyntax> statements = SyntaxInfo.StatementListInfo(ifStatement).Statements;

            if (!statements.Any())
                return;

            if (IsPartOfLazyInitialization())
                return;

            NullCheckExpressionInfo nullCheck = SyntaxInfo.NullCheckExpressionInfo(
                ifStatement.Condition,
                semanticModel: context.SemanticModel,
                allowedStyles: NullCheckStyles.CheckingNull,
                cancellationToken: context.CancellationToken);

            if (!nullCheck.Success)
                return;

            SimpleAssignmentStatementInfo assignmentInfo = SyntaxInfo.SimpleAssignmentStatementInfo(ifStatement.SingleNonBlockStatementOrDefault());

            if (!assignmentInfo.Success)
                return;

            if (!CSharpFactory.AreEquivalent(assignmentInfo.Left, nullCheck.Expression))
                return;

            if (!assignmentInfo.Right.IsSingleLine())
                return;

            int index = statements.IndexOf(ifStatement);

            if (index > 0
                && DiagnosticRules.UseCoalesceExpression.IsEffective(context))
            {
                StatementSyntax previousStatement = statements[index - 1];

                if (!previousStatement.ContainsDiagnostics
                    && !previousStatement.GetTrailingTrivia().Any(f => f.IsDirective)
                    && !ifStatement.GetLeadingTrivia().Any(f => f.IsDirective)
                    && CanUseCoalesceExpression(previousStatement, nullCheck.Expression))
                {
                    DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UseCoalesceExpression, previousStatement);
                }
            }

            if (!DiagnosticRules.InlineLazyInitialization.IsEffective(context))
                return;

            if (index == statements.Count - 1)
                return;

            StatementSyntax nextStatement = statements[index + 1];

            if (nextStatement.ContainsDiagnostics)
                return;

            SimpleMemberInvocationStatementInfo invocationInfo = SyntaxInfo.SimpleMemberInvocationStatementInfo(nextStatement);

            if (!invocationInfo.Success)
                return;

            if (!CSharpFactory.AreEquivalent(nullCheck.Expression, invocationInfo.Expression))
                return;

            if (ifStatement.GetTrailingTrivia().Any(f => f.IsDirective))
                return;

            if (nextStatement.SpanOrLeadingTriviaContainsDirectives())
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.InlineLazyInitialization, ifStatement);

            bool IsPartOfLazyInitialization()
            {
                return statements.Count == 2
                    && statements.IndexOf(ifStatement) == 0
                    && statements[1].IsKind(SyntaxKind.ReturnStatement);
            }
        }

        private static bool CanUseCoalesceExpression(StatementSyntax statement, ExpressionSyntax expression)
        {
            SyntaxKind kind = statement.Kind();

            if (kind == SyntaxKind.LocalDeclarationStatement)
            {
                SingleLocalDeclarationStatementInfo localInfo = SyntaxInfo.SingleLocalDeclarationStatementInfo((LocalDeclarationStatementSyntax)statement);

                return localInfo.Success
                    && !localInfo.Type.IsKind(SyntaxKind.RefType)
                    && expression.IsKind(SyntaxKind.IdentifierName)
                    && string.Equals(localInfo.IdentifierText, ((IdentifierNameSyntax)expression).Identifier.ValueText, StringComparison.Ordinal)
                    && !localInfo.Value.GetTrailingTrivia().Any(f => f.IsDirective)
                    && !localInfo.SemicolonToken.ContainsDirectives;
            }
            else if (kind == SyntaxKind.ExpressionStatement)
            {
                var expressionStatement = (ExpressionStatementSyntax)statement;

                SimpleAssignmentStatementInfo assignmentInfo = SyntaxInfo.SimpleAssignmentStatementInfo(expressionStatement);

                return assignmentInfo.Success
                    && CSharpFactory.AreEquivalent(expression, assignmentInfo.Left)
                    && !assignmentInfo.Right.GetTrailingTrivia().Any(f => f.IsDirective)
                    && !expressionStatement.SemicolonToken.ContainsDirectives;
            }

            return false;
        }
    }
}
