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
    public sealed class RemoveRedundantDisposeOrCloseCallAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.RemoveRedundantDisposeOrCloseCall);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeUsingStatement(f), SyntaxKind.UsingStatement);
        }

        private static void AnalyzeUsingStatement(SyntaxNodeAnalysisContext context)
        {
            var usingStatement = (UsingStatementSyntax)context.Node;

            StatementSyntax statement = usingStatement.Statement;

            if (statement?.Kind() != SyntaxKind.Block)
                return;

            var block = (BlockSyntax)statement;

            StatementSyntax lastStatement = block.Statements.LastOrDefault();

            if (lastStatement == null)
                return;

            if (lastStatement.SpanContainsDirectives())
                return;

            SimpleMemberInvocationStatementInfo info = SyntaxInfo.SimpleMemberInvocationStatementInfo(lastStatement);

            if (!info.Success)
                return;

            if (info.Arguments.Any())
                return;

            string methodName = info.NameText;

            if (methodName != "Dispose" && methodName != "Close")
                return;

            ExpressionSyntax usingExpression = usingStatement.Expression;

            if (usingExpression != null)
            {
                if (CSharpFactory.AreEquivalent(info.Expression, usingExpression))
                    ReportDiagnostic(context, info.Statement, methodName);
            }
            else
            {
                VariableDeclarationSyntax usingDeclaration = usingStatement.Declaration;

                if (usingDeclaration != null
                    && info.Expression.Kind() == SyntaxKind.IdentifierName)
                {
                    var identifierName = (IdentifierNameSyntax)info.Expression;

                    VariableDeclaratorSyntax declarator = usingDeclaration.Variables.LastOrDefault();

                    if (declarator != null
                        && declarator.Identifier.ValueText == identifierName.Identifier.ValueText)
                    {
                        ISymbol symbol = context.SemanticModel.GetDeclaredSymbol(declarator, context.CancellationToken);

                        if (SymbolEqualityComparer.Default.Equals(symbol, context.SemanticModel.GetSymbol(identifierName, context.CancellationToken)))
                            ReportDiagnostic(context, info.Statement, methodName);
                    }
                }
            }
        }

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, ExpressionStatementSyntax expressionStatement, string methodName)
        {
            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.RemoveRedundantDisposeOrCloseCall,
                expressionStatement,
                methodName);
        }
    }
}
