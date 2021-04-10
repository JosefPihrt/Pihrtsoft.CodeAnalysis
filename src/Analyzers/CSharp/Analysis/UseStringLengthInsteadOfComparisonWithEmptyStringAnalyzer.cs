// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UseStringLengthInsteadOfComparisonWithEmptyStringAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.UseStringLengthInsteadOfComparisonWithEmptyString);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeEqualsExpression(f), SyntaxKind.EqualsExpression);
        }

        private static void AnalyzeEqualsExpression(SyntaxNodeAnalysisContext context)
        {
            var equalsExpression = (BinaryExpressionSyntax)context.Node;

            if (equalsExpression.ContainsDirectives)
                return;

            BinaryExpressionInfo equalsExpressionInfo = SyntaxInfo.BinaryExpressionInfo(equalsExpression);

            if (!equalsExpressionInfo.Success)
                return;

            ExpressionSyntax left = equalsExpressionInfo.Left;
            ExpressionSyntax right = equalsExpressionInfo.Right;

            SemanticModel semanticModel = context.SemanticModel;
            CancellationToken cancellationToken = context.CancellationToken;

            if (CSharpUtility.IsEmptyStringExpression(left, semanticModel, cancellationToken))
            {
                if (CSharpUtility.IsStringExpression(right, semanticModel, cancellationToken)
                    && !equalsExpression.IsInExpressionTree(semanticModel, cancellationToken))
                {
                    DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UseStringLengthInsteadOfComparisonWithEmptyString, equalsExpression);
                }
            }
            else if (CSharpUtility.IsEmptyStringExpression(right, semanticModel, cancellationToken)
                && CSharpUtility.IsStringExpression(left, semanticModel, cancellationToken)
                && !equalsExpression.IsInExpressionTree(semanticModel, cancellationToken))
            {
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UseStringLengthInsteadOfComparisonWithEmptyString, equalsExpression);
            }
        }
    }
}
