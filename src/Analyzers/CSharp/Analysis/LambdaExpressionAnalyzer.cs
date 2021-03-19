// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LambdaExpressionAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticDescriptors.ConvertLambdaExpressionBodyToExpressionBody,
                    DiagnosticDescriptors.ConvertLambdaExpressionBodyToExpressionBodyFadeOut);
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
            if (!DiagnosticDescriptors.ConvertLambdaExpressionBodyToExpressionBody.IsEffective(context))
                return;

            var lambda = (LambdaExpressionSyntax)context.Node;

            if (lambda.ContainsDiagnostics)
                return;

            if (!ConvertLambdaExpressionBodyToExpressionBodyAnalysis.IsFixable(lambda))
                return;

            CSharpSyntaxNode body = lambda.Body;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.ConvertLambdaExpressionBodyToExpressionBody, body);

            var block = (BlockSyntax)body;

            CSharpDiagnosticHelpers.ReportBraces(context, DiagnosticDescriptors.ConvertLambdaExpressionBodyToExpressionBodyFadeOut, block);

            StatementSyntax statement = block.Statements[0];

            if (statement.Kind() == SyntaxKind.ReturnStatement)
                DiagnosticHelpers.ReportToken(context, DiagnosticDescriptors.ConvertLambdaExpressionBodyToExpressionBodyFadeOut, ((ReturnStatementSyntax)statement).ReturnKeyword);
        }
    }
}
