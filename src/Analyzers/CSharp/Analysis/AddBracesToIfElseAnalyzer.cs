// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AddBracesToIfElseAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.AddBracesToIfElse);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeIfStatement(f), SyntaxKind.IfStatement);
            context.RegisterSyntaxNodeAction(f => AnalyzeElseClause(f), SyntaxKind.ElseClause);
        }

        private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;

            if (ifStatement.IsSimpleIf())
                return;

            StatementSyntax statement = ifStatement.EmbeddedStatement();

            if (statement == null)
                return;

            if (statement.ContainsDirectives)
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.AddBracesToIfElse, statement, CSharpFacts.GetTitle(ifStatement));
        }

        private static void AnalyzeElseClause(SyntaxNodeAnalysisContext context)
        {
            var elseClause = (ElseClauseSyntax)context.Node;

            StatementSyntax statement = elseClause.EmbeddedStatement(allowIfStatement: false);

            if (statement == null)
                return;

            if (statement.ContainsDirectives)
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.AddBracesToIfElse, statement, CSharpFacts.GetTitle(elseClause));
        }
    }
}
