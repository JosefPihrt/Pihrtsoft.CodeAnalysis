// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp;
using Roslynator.CSharp.Syntax;
using Roslynator.CSharp.SyntaxWalkers;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class InlineLocalVariableAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticDescriptors.InlineLocalVariable,
                    DiagnosticDescriptors.InlineLocalVariableFadeOut);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (DiagnosticDescriptors.InlineLocalVariable.IsEffective(c))
                        AnalyzeLocalDeclarationStatement(c);
                },
                SyntaxKind.LocalDeclarationStatement);
        }

        private static void AnalyzeLocalDeclarationStatement(SyntaxNodeAnalysisContext context)
        {
            var localDeclarationStatement = (LocalDeclarationStatementSyntax)context.Node;

            if (localDeclarationStatement.ContainsDiagnostics)
                return;

            if (localDeclarationStatement.SpanOrTrailingTriviaContainsDirectives())
                return;

            SingleLocalDeclarationStatementInfo localDeclarationInfo = SyntaxInfo.SingleLocalDeclarationStatementInfo(localDeclarationStatement);

            if (!localDeclarationInfo.Success)
                return;

            ExpressionSyntax value = localDeclarationInfo.Value;

            if (value == null)
                return;

            SyntaxList<StatementSyntax> statements = SyntaxInfo.StatementListInfo(localDeclarationStatement).Statements;

            if (!statements.Any())
                return;

            int index = statements.IndexOf(localDeclarationStatement);

            if (index == statements.Count - 1)
                return;

            StatementSyntax nextStatement = statements[index + 1];

            if (nextStatement.ContainsDiagnostics)
                return;

            switch (nextStatement.Kind())
            {
                case SyntaxKind.ExpressionStatement:
                    {
                        Analyze(context, statements, localDeclarationInfo, index, (ExpressionStatementSyntax)nextStatement);
                        break;
                    }
                case SyntaxKind.LocalDeclarationStatement:
                    {
                        Analyze(context, statements, localDeclarationInfo, index, (LocalDeclarationStatementSyntax)nextStatement);
                        break;
                    }
                case SyntaxKind.ReturnStatement:
                    {
                        var returnStatement = (ReturnStatementSyntax)nextStatement;

                        if (!returnStatement.SpanOrLeadingTriviaContainsDirectives())
                        {
                            ExpressionSyntax expression = returnStatement.Expression;

                            if (expression?.Kind() == SyntaxKind.IdentifierName)
                            {
                                var identifierName = (IdentifierNameSyntax)expression;

                                if (string.Equals(localDeclarationInfo.IdentifierText, identifierName.Identifier.ValueText, StringComparison.Ordinal))
                                    ReportDiagnostic(context, localDeclarationInfo, expression);
                            }
                        }

                        break;
                    }
                case SyntaxKind.YieldReturnStatement:
                    {
                        var yieldStatement = (YieldStatementSyntax)nextStatement;

                        if (index == statements.Count - 2
                            && !yieldStatement.SpanOrLeadingTriviaContainsDirectives())
                        {
                            ExpressionSyntax expression = yieldStatement.Expression;

                            if (expression?.Kind() == SyntaxKind.IdentifierName)
                            {
                                var identifierName = (IdentifierNameSyntax)expression;

                                if (string.Equals(localDeclarationInfo.IdentifierText, identifierName.Identifier.ValueText, StringComparison.Ordinal))
                                    ReportDiagnostic(context, localDeclarationInfo, expression);
                            }
                        }

                        break;
                    }
                case SyntaxKind.ForEachStatement:
                    {
                        if (value.WalkDownParentheses().IsKind(SyntaxKind.AwaitExpression))
                            return;

                        if (!value.IsSingleLine())
                            return;

                        if (value.IsKind(SyntaxKind.ArrayInitializerExpression))
                            return;

                        var forEachStatement = (ForEachStatementSyntax)nextStatement;

                        ISymbol localSymbol = GetLocalSymbol(localDeclarationInfo, forEachStatement.Expression, context.SemanticModel, context.CancellationToken);

                        if (localSymbol?.IsErrorType() != false)
                            return;

                        ContainsLocalOrParameterReferenceWalker walker = ContainsLocalOrParameterReferenceWalker.GetInstance(localSymbol, context.SemanticModel, context.CancellationToken);

                        walker.Visit(forEachStatement.Statement);

                        if (!walker.Result
                            && index < statements.Count - 2)
                        {
                            walker.VisitList(statements, index + 2);
                        }

                        if (ContainsLocalOrParameterReferenceWalker.GetResultAndFree(walker))
                            return;

                        ReportDiagnostic(context, localDeclarationInfo, forEachStatement.Expression);

                        break;
                    }
                case SyntaxKind.SwitchStatement:
                    {
                        if (value.WalkDownParentheses().IsKind(SyntaxKind.AwaitExpression))
                            return;

                        if (!value.IsSingleLine())
                            return;

                        var switchStatement = (SwitchStatementSyntax)nextStatement;

                        ISymbol localSymbol = GetLocalSymbol(localDeclarationInfo, switchStatement.Expression, context.SemanticModel, context.CancellationToken);

                        if (localSymbol?.IsErrorType() != false)
                            return;

                        ContainsLocalOrParameterReferenceWalker walker = ContainsLocalOrParameterReferenceWalker.GetInstance(localSymbol, context.SemanticModel, context.CancellationToken);

                        walker.VisitList(switchStatement.Sections);

                        if (!walker.Result
                            && index < statements.Count - 2)
                        {
                            walker.VisitList(statements, index + 2);
                        }

                        if (ContainsLocalOrParameterReferenceWalker.GetResultAndFree(walker))
                            return;

                        ReportDiagnostic(context, localDeclarationInfo, switchStatement.Expression);

                        break;
                    }
            }
        }

        private static void Analyze(
            SyntaxNodeAnalysisContext context,
            SyntaxList<StatementSyntax> statements,
            in SingleLocalDeclarationStatementInfo localDeclarationInfo,
            int index,
            ExpressionStatementSyntax expressionStatement)
        {
            SimpleAssignmentExpressionInfo assignment = SyntaxInfo.SimpleAssignmentExpressionInfo(expressionStatement.Expression);

            if (!assignment.Success)
                return;

            if (assignment.Right.Kind() != SyntaxKind.IdentifierName)
                return;

            var identifierName = (IdentifierNameSyntax)assignment.Right;

            if (!string.Equals(localDeclarationInfo.IdentifierText, identifierName.Identifier.ValueText, StringComparison.Ordinal))
                return;

            if (expressionStatement.SpanOrLeadingTriviaContainsDirectives())
                return;

            ISymbol localSymbol = context.SemanticModel.GetDeclaredSymbol(localDeclarationInfo.Declarator, context.CancellationToken);

            if (localSymbol?.IsErrorType() != false)
                return;

            ContainsLocalOrParameterReferenceWalker walker = ContainsLocalOrParameterReferenceWalker.GetInstance(localSymbol, context.SemanticModel, context.CancellationToken);

            walker.Visit(assignment.Left);

            if (!walker.Result
                && index < statements.Count - 2)
            {
                walker.VisitList(statements, index + 2);
            }

            if (ContainsLocalOrParameterReferenceWalker.GetResultAndFree(walker))
                return;

            ReportDiagnostic(context, localDeclarationInfo, identifierName);
        }

        private static void Analyze(
            SyntaxNodeAnalysisContext context,
            SyntaxList<StatementSyntax> statements,
            in SingleLocalDeclarationStatementInfo localDeclarationInfo,
            int index,
            LocalDeclarationStatementSyntax localDeclaration2)
        {
            if (localDeclaration2.SpanOrLeadingTriviaContainsDirectives())
                return;

            ExpressionSyntax value = localDeclaration2
                .Declaration?
                .Variables
                .SingleOrDefault(shouldThrow: false)?
                .Initializer?
                .Value;

            if (value?.Kind() != SyntaxKind.IdentifierName)
                return;

            var identifierName = (IdentifierNameSyntax)value;

            if (!string.Equals(localDeclarationInfo.IdentifierText, identifierName.Identifier.ValueText, StringComparison.Ordinal))
                return;

            ISymbol localSymbol = context.SemanticModel.GetDeclaredSymbol(localDeclarationInfo.Declarator, context.CancellationToken);

            if (localSymbol?.IsErrorType() != false)
                return;

            if (index < statements.Count - 2)
            {
                ContainsLocalOrParameterReferenceWalker walker = ContainsLocalOrParameterReferenceWalker.GetInstance(localSymbol, context.SemanticModel, context.CancellationToken);

                walker.VisitList(statements, index + 2);

                if (ContainsLocalOrParameterReferenceWalker.GetResultAndFree(walker))
                    return;
            }

            ReportDiagnostic(context, localDeclarationInfo, identifierName);
        }

        private static ISymbol GetLocalSymbol(
            in SingleLocalDeclarationStatementInfo localDeclarationInfo,
            ExpressionSyntax expression,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            if (expression?.Kind() != SyntaxKind.IdentifierName)
                return null;

            var identifierName = (IdentifierNameSyntax)expression;

            if (!string.Equals(localDeclarationInfo.IdentifierText, identifierName.Identifier.ValueText, StringComparison.Ordinal))
                return null;

            SyntaxNode parent = localDeclarationInfo.Statement.Parent;

            if (parent.ContainsDirectives(TextSpan.FromBounds(expression.Parent.FullSpan.Start, expression.Span.End)))
                return null;

            return semanticModel.GetDeclaredSymbol(localDeclarationInfo.Declarator, cancellationToken);
        }

        private static void ReportDiagnostic(
            SyntaxNodeAnalysisContext context,
            in SingleLocalDeclarationStatementInfo localDeclarationInfo,
            ExpressionSyntax expression)
        {
            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.InlineLocalVariable, localDeclarationInfo.Statement);

            foreach (SyntaxToken modifier in localDeclarationInfo.Modifiers)
                DiagnosticHelpers.ReportToken(context, DiagnosticDescriptors.InlineLocalVariableFadeOut, modifier);

            DiagnosticHelpers.ReportNode(context, DiagnosticDescriptors.InlineLocalVariableFadeOut, localDeclarationInfo.Type);
            DiagnosticHelpers.ReportToken(context, DiagnosticDescriptors.InlineLocalVariableFadeOut, localDeclarationInfo.Identifier);
            DiagnosticHelpers.ReportToken(context, DiagnosticDescriptors.InlineLocalVariableFadeOut, localDeclarationInfo.EqualsToken);
            DiagnosticHelpers.ReportToken(context, DiagnosticDescriptors.InlineLocalVariableFadeOut, localDeclarationInfo.SemicolonToken);
            DiagnosticHelpers.ReportNode(context, DiagnosticDescriptors.InlineLocalVariableFadeOut, expression);
        }
    }
}
