// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Syntax;
using Roslynator.CSharp.SyntaxWalkers;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UseForStatementInsteadOfWhileStatementAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.UseForStatementInsteadOfWhileStatement);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeWhileStatement(f), SyntaxKind.WhileStatement);
        }

        private static void AnalyzeWhileStatement(SyntaxNodeAnalysisContext context)
        {
            var whileStatement = (WhileStatementSyntax)context.Node;

            ExpressionSyntax condition = whileStatement.Condition;

            if (condition.IsMissing)
                return;

            if (!condition.IsSingleLine())
                return;

            StatementSyntax statement = whileStatement.Statement;

            if (!(statement is BlockSyntax block))
                return;

            SyntaxList<StatementSyntax> innerStatements = block.Statements;

            if (innerStatements.Count <= 1)
                return;

            ExpressionSyntax incrementedExpression = GetIncrementedExpression(innerStatements.Last());

            if (!incrementedExpression.IsKind(SyntaxKind.IdentifierName))
                return;

            SyntaxList<StatementSyntax> outerStatements = SyntaxInfo.StatementListInfo(whileStatement).Statements;

            int index = outerStatements.IndexOf(whileStatement);

            if (index <= 0)
                return;

            SingleLocalDeclarationStatementInfo localInfo = GetLocalInfo(outerStatements[index - 1]);

            if (!localInfo.Success)
                return;

            if (index > 1)
            {
                SingleLocalDeclarationStatementInfo localInfo2 = GetLocalInfo(outerStatements[index - 2]);

                if (localInfo2.Success)
                {
                    ExpressionSyntax incrementedExpression2 = GetIncrementedExpression(innerStatements[innerStatements.Count - 2]);

                    if (incrementedExpression2 is IdentifierNameSyntax identifierName2
                        && string.Equals(localInfo2.Identifier.ValueText, identifierName2.Identifier.ValueText, StringComparison.Ordinal))
                    {
                        return;
                    }
                }
            }

            var identifierName = (IdentifierNameSyntax)incrementedExpression;

            if (!string.Equals(localInfo.Identifier.ValueText, identifierName.Identifier.ValueText, StringComparison.Ordinal))
                return;

            if (ContainsContinueStatement())
                return;

            SemanticModel semanticModel = context.SemanticModel;
            CancellationToken cancellationToken = context.CancellationToken;

            ISymbol symbol = semanticModel.GetDeclaredSymbol(localInfo.Declarator, cancellationToken);

            if (symbol?.Kind != SymbolKind.Local)
                return;

            if (IsLocalVariableReferencedAfterWhileStatement())
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UseForStatementInsteadOfWhileStatement, whileStatement.WhileKeyword);

            bool ContainsContinueStatement()
            {
                ContainsContinueStatementWalker walker = ContainsContinueStatementWalker.GetInstance();
                walker.ContainsContinueStatement = false;

                var containsContinueStatement = false;

                foreach (StatementSyntax innerStatement in innerStatements)
                {
                    walker.Visit(innerStatement);

                    if (walker.ContainsContinueStatement)
                    {
                        containsContinueStatement = true;
                        break;
                    }
                }

                ContainsContinueStatementWalker.Free(walker);

                return containsContinueStatement;
            }

            bool IsLocalVariableReferencedAfterWhileStatement()
            {
                ContainsLocalOrParameterReferenceWalker walker = ContainsLocalOrParameterReferenceWalker.GetInstance(symbol, semanticModel, cancellationToken);

                walker.VisitList(outerStatements, index + 1);

                return ContainsLocalOrParameterReferenceWalker.GetResultAndFree(walker);
            }
        }

        private static ExpressionSyntax GetIncrementedExpression(StatementSyntax statement)
        {
            if (statement is ExpressionStatementSyntax expressionStatement)
            {
                ExpressionSyntax expression = expressionStatement.Expression;

                if (expression.IsKind(SyntaxKind.PostIncrementExpression))
                {
                    var postIncrementExpression = (PostfixUnaryExpressionSyntax)expression;

                    return postIncrementExpression.Operand;
                }
            }

            return null;
        }

        private static SingleLocalDeclarationStatementInfo GetLocalInfo(StatementSyntax statement)
        {
            return (statement.IsKind(SyntaxKind.LocalDeclarationStatement))
                ? SyntaxInfo.SingleLocalDeclarationStatementInfo((LocalDeclarationStatementSyntax)statement)
                : default;
        }

        private class ContainsContinueStatementWalker : CSharpSyntaxNodeWalker
        {
            [ThreadStatic]
            private static ContainsContinueStatementWalker _cachedInstance;

            public bool ContainsContinueStatement { get; set; }

            protected override bool ShouldVisit => !ContainsContinueStatement;

            public override void VisitContinueStatement(ContinueStatementSyntax node)
            {
                ContainsContinueStatement = true;
            }

            public override void VisitDoStatement(DoStatementSyntax node)
            {
            }

            public override void VisitForStatement(ForStatementSyntax node)
            {
            }

            public override void VisitForEachStatement(ForEachStatementSyntax node)
            {
            }

            public override void VisitForEachVariableStatement(ForEachVariableStatementSyntax node)
            {
            }

            public override void VisitWhileStatement(WhileStatementSyntax node)
            {
            }

            public static ContainsContinueStatementWalker GetInstance()
            {
                ContainsContinueStatementWalker walker = _cachedInstance;

                if (walker != null)
                {
                    _cachedInstance = null;
                    return walker;
                }

                return new ContainsContinueStatementWalker();
            }

            public static void Free(ContainsContinueStatementWalker walker)
            {
                _cachedInstance = walker;
            }
        }
    }
}
