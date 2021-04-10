// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SimplifyCodeBranchingAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.SimplifyCodeBranching);

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

            SimplifyCodeBranchingKind? kind = GetKind(ifStatement, context.SemanticModel, context.CancellationToken);

            if (kind == null)
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.SimplifyCodeBranching, ifStatement.IfKeyword);
        }

        internal static SimplifyCodeBranchingKind? GetKind(IfStatementSyntax ifStatement, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            ExpressionSyntax condition = ifStatement.Condition?.WalkDownParentheses();

            if (condition?.IsMissing != false)
                return null;

            StatementSyntax ifStatementStatement = ifStatement.Statement;

            if (ifStatementStatement == null)
                return null;

            ElseClauseSyntax elseClause = ifStatement.Else;

            if (elseClause != null)
            {
                if (IsFixableIfElseInsideWhile(ifStatement, elseClause))
                {
                    return SimplifyCodeBranchingKind.IfElseInsideWhile;
                }
                else
                {
                    var ifStatementBlock = ifStatementStatement as BlockSyntax;

                    if (ifStatementBlock?.Statements.Any() == false
                        && ifStatementBlock.OpenBraceToken.TrailingTrivia.IsEmptyOrWhitespace()
                        && ifStatementBlock.CloseBraceToken.LeadingTrivia.IsEmptyOrWhitespace()
                        && IsFixableIfElseWithEmptyIf(ifStatement, elseClause))
                    {
                        return SimplifyCodeBranchingKind.IfElseWithEmptyIf;
                    }
                    else if (IsFixableIfElseWithReturnOrContinueInsideIf(ifStatement, elseClause))
                    {
                        return SimplifyCodeBranchingKind.LastIfElseWithReturnOrContinueInsideIf;
                    }
                }
            }
            else if (IsFixableSimpleIfInsideWhileOrDo(ifStatement, semanticModel, cancellationToken))
            {
                return SimplifyCodeBranchingKind.SimplifyIfInsideWhileOrDo;
            }
            else if (!ifStatement.IsParentKind(SyntaxKind.ElseClause)
                && (ifStatement.SingleNonBlockStatementOrDefault() is DoStatementSyntax doStatement)
                && CSharpFactory.AreEquivalent(condition, doStatement.Condition?.WalkDownParentheses()))
            {
                return SimplifyCodeBranchingKind.SimpleIfContainingOnlyDo;
            }

            return null;
        }

        private static bool IsFixableIfElseWithEmptyIf(IfStatementSyntax ifStatement, ElseClauseSyntax elseClause)
        {
            if (ifStatement.SpanContainsDirectives())
                return false;

            StatementSyntax whenFalse = elseClause.Statement;

            if (whenFalse == null)
                return false;

            SyntaxKind kind = whenFalse.Kind();

            if (kind == SyntaxKind.IfStatement)
            {
                var nestedIf = (IfStatementSyntax)whenFalse;

                if (nestedIf.Else != null)
                    return false;

                if (nestedIf.Condition?.WalkDownParentheses().IsMissing != false)
                    return false;

                StatementSyntax statement = nestedIf.Statement;

                if (statement == null)
                    return false;

                if ((statement as BlockSyntax)?.Statements.Any() == false)
                    return false;

                //if (x)
                //{
                //}
                //else if (y)
                //{
                //    M();
                //}
            }
            else if (kind == SyntaxKind.Block)
            {
                if (!((BlockSyntax)whenFalse).Statements.Any())
                    return false;

                //if (x)
                //{
                //}
                //else
                //{
                //    M();
                //}
            }

            //void M()
            //{
            //    if (x)
            //    {
            //    }
            //    else
            //    {
            //        M();
            //    }
            //}

            return true;
        }

        private static bool IsFixableIfElseWithReturnOrContinueInsideIf(IfStatementSyntax ifStatement, ElseClauseSyntax elseClause)
        {
            if (elseClause.Statement?.IsKind(SyntaxKind.IfStatement) != false)
                return false;

            IfStatementSyntax topmostIf = ifStatement.GetTopmostIf();

            if (!(topmostIf.Parent is BlockSyntax block))
                return false;

            if (!block.Statements.IsLast(topmostIf, ignoreLocalFunctions: true))
                return false;

            switch (block.Parent.Kind())
            {
                case SyntaxKind.MethodDeclaration:
                case SyntaxKind.LocalFunctionStatement:
                case SyntaxKind.ConstructorDeclaration:
                case SyntaxKind.DestructorDeclaration:
                case SyntaxKind.AddAccessorDeclaration:
                case SyntaxKind.RemoveAccessorDeclaration:
                case SyntaxKind.SetAccessorDeclaration:
                    {
                        //void M()
                        //{
                        //    if (x)
                        //    {
                        //        return;
                        //    }
                        //    else
                        //    {
                        //        M();
                        //    }

                        return ifStatement.SingleNonBlockStatementOrDefault() is ReturnStatementSyntax returnStatement
                            && returnStatement.Expression == null;
                    }
                case SyntaxKind.ForEachStatement:
                case SyntaxKind.ForEachVariableStatement:
                case SyntaxKind.ForStatement:
                case SyntaxKind.WhileStatement:
                    {
                        //while (x)
                        //{
                        //    if (y)
                        //    {
                        //        continue;
                        //    }
                        //    else
                        //    {
                        //        M();
                        //    }
                        //}

                        return ifStatement.SingleNonBlockStatementOrDefault().IsKind(SyntaxKind.ContinueStatement);
                    }
            }

            return false;
        }

        private static bool IsFixableIfElseInsideWhile(
            IfStatementSyntax ifStatement,
            ElseClauseSyntax elseClause)
        {
            StatementSyntax ifStatementStatement = ifStatement.Statement;

            if (ifStatementStatement == null)
                return false;

            StatementSyntax elseClauseStatement = elseClause.Statement;

            if (elseClauseStatement == null)
                return false;

            if (elseClauseStatement.SingleNonBlockStatementOrDefault()?.Kind() != SyntaxKind.BreakStatement
                && ifStatementStatement.SingleNonBlockStatementOrDefault()?.Kind() != SyntaxKind.BreakStatement)
            {
                return false;
            }

            SyntaxNode parent = ifStatement.Parent;

            if (parent is BlockSyntax block)
            {
                if (!object.ReferenceEquals(ifStatement, block.Statements[0]))
                    return false;

                parent = block.Parent;
            }

            if (!(parent is WhileStatementSyntax whileStatement))
                return false;

            if (whileStatement.SpanContainsDirectives())
                return false;

            if (whileStatement.Condition?.WalkDownParentheses().Kind() != SyntaxKind.TrueLiteralExpression)
                return false;

            //while (x)
            //{
            //    if (y)
            //    {
            //        break;
            //    }
            //    else
            //    {
            //    }
            //
            //    M();
            //}

            return true;
        }

        private static bool IsFixableSimpleIfInsideWhileOrDo(
            IfStatementSyntax ifStatement,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            SyntaxNode parent = ifStatement.Parent;

            if (parent.Kind() != SyntaxKind.Block)
                return false;

            if (ifStatement.SingleNonBlockStatementOrDefault()?.Kind() != SyntaxKind.BreakStatement)
                return false;

            var block = (BlockSyntax)parent;

            SyntaxList<StatementSyntax> statements = block.Statements;

            int count = statements.Count;

            if (count == 1)
                return false;

            int index = statements.IndexOf(ifStatement);

            if (index != 0
                && index != count - 1)
            {
                return false;
            }

            parent = block.Parent;

            SyntaxKind kind = parent.Kind();

            if (kind == SyntaxKind.WhileStatement)
            {
                var whileStatement = (WhileStatementSyntax)parent;

                if (whileStatement.SpanContainsDirectives())
                    return false;

                if (whileStatement.Condition?.WalkDownParentheses().Kind() != SyntaxKind.TrueLiteralExpression)
                    return false;

                if (index == count - 1
                    && ContainsLocalDefinedInLoopBody(
                        ifStatement.Condition,
                        TextSpan.FromBounds(block.SpanStart, ifStatement.SpanStart),
                        semanticModel,
                        cancellationToken))
                {
                    return false;
                }

                //while (x)
                //{
                //    M();
                //
                //    if (y)
                //    {
                //        break;
                //    }
                //}

                return true;
            }
            else if (kind == SyntaxKind.DoStatement)
            {
                var doStatement = (DoStatementSyntax)parent;

                if (doStatement.SpanContainsDirectives())
                    return false;

                if (doStatement.Condition?.WalkDownParentheses().Kind() != SyntaxKind.TrueLiteralExpression)
                    return false;

                if (index == count - 1
                    && ContainsLocalDefinedInLoopBody(
                        ifStatement.Condition,
                        TextSpan.FromBounds(block.SpanStart, ifStatement.SpanStart),
                        semanticModel,
                        cancellationToken))
                {
                    return false;
                }

                //do
                //{
                //    if (x)
                //    { 
                //        break;
                //    }
                //
                //    M();
                //}
                //while (y);
                return true;
            }

            return false;
        }

        private static bool ContainsLocalDefinedInLoopBody(
            ExpressionSyntax condition,
            TextSpan span,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            foreach (SyntaxNode node in condition.DescendantNodesAndSelf())
            {
                if (node.IsKind(SyntaxKind.IdentifierName))
                {
                    ISymbol symbol = semanticModel.GetSymbol((ExpressionSyntax)node, cancellationToken);

                    if (symbol?.Kind == SymbolKind.Local
                        && symbol.GetSyntaxOrDefault(cancellationToken)?.Span.IsContainedIn(span) == true)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
