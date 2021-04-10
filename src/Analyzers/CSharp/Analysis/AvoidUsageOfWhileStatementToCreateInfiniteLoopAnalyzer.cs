// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidUsageOfWhileStatementToCreateInfiniteLoopAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.AvoidUsageOfWhileStatementToCreateInfiniteLoop);

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

            if (whileStatement.Condition?.Kind() != SyntaxKind.TrueLiteralExpression)
                return;

            TextSpan span = TextSpan.FromBounds(
                whileStatement.OpenParenToken.Span.End,
                whileStatement.CloseParenToken.SpanStart);

            if (!whileStatement
                .DescendantTrivia(span)
                .All(f => f.IsWhitespaceOrEndOfLineTrivia()))
            {
                return;
            }

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.AvoidUsageOfWhileStatementToCreateInfiniteLoop, whileStatement.WhileKeyword);
        }
    }
}
