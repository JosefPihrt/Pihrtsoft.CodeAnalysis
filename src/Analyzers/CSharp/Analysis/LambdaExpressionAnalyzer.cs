// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class LambdaExpressionAnalyzer : BaseDiagnosticAnalyzer
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
                        DiagnosticRules.ConvertLambdaExpressionBodyToExpressionBody,
                        DiagnosticRules.ConvertLambdaExpressionBodyToExpressionBodyFadeOut);
                }

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeLambdaExpression(f), SyntaxKind.SimpleLambdaExpression);
            context.RegisterSyntaxNodeAction(f => AnalyzeLambdaExpression(f), SyntaxKind.ParenthesizedLambdaExpression);
        }

        private static void AnalyzeLambdaExpression(SyntaxNodeAnalysisContext context)
        {
            if (!DiagnosticRules.ConvertLambdaExpressionBodyToExpressionBody.IsEffective(context))
                return;

            var lambda = (LambdaExpressionSyntax)context.Node;

            if (lambda.ContainsDiagnostics)
                return;

            if (!ConvertLambdaExpressionBodyToExpressionBodyAnalysis.IsFixable(lambda))
                return;

            CSharpSyntaxNode body = lambda.Body;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ConvertLambdaExpressionBodyToExpressionBody, body);

            var block = (BlockSyntax)body;

            CSharpDiagnosticHelpers.ReportBraces(context, DiagnosticRules.ConvertLambdaExpressionBodyToExpressionBodyFadeOut, block);

            StatementSyntax statement = block.Statements[0];

            if (statement.Kind() == SyntaxKind.ReturnStatement)
                DiagnosticHelpers.ReportToken(context, DiagnosticRules.ConvertLambdaExpressionBodyToExpressionBodyFadeOut, ((ReturnStatementSyntax)statement).ReturnKeyword);
        }
    }
}
