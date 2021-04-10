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
    public sealed class ConvertInterpolatedStringToConcatenationAnalyzer : BaseDiagnosticAnalyzer
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
                        DiagnosticRules.ConvertInterpolatedStringToConcatenation,
                        DiagnosticRules.ConvertInterpolatedStringToConcatenationFadeOut);
                }

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (DiagnosticRules.ConvertInterpolatedStringToConcatenation.IsEffective(c))
                        AnalyzeInterpolatedStringExpression(c);
                },
                SyntaxKind.InterpolatedStringExpression);
        }

        private static void AnalyzeInterpolatedStringExpression(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.ContainsDiagnostics)
                return;

            if (context.Node.ContainsDirectives)
                return;

            var interpolatedString = (InterpolatedStringExpressionSyntax)context.Node;

            SyntaxList<InterpolatedStringContentSyntax> contents = interpolatedString.Contents;

            if (contents.Count <= 1)
                return;

            if (contents.Any(f => f.Kind() != SyntaxKind.Interpolation))
                return;

            foreach (InterpolatedStringContentSyntax content in contents)
            {
                var interpolation = (InterpolationSyntax)content;

                ExpressionSyntax expression = interpolation.Expression;

                if (expression == null)
                    return;

                if (interpolation.AlignmentClause != null)
                    return;

                if (interpolation.FormatClause != null)
                    return;

                ITypeSymbol typeSymbol = context.SemanticModel.GetTypeSymbol(expression, context.CancellationToken);

                if (typeSymbol?.SpecialType != SpecialType.System_String)
                    return;
            }

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ConvertInterpolatedStringToConcatenation, interpolatedString);

            DiagnosticHelpers.ReportToken(context, DiagnosticRules.ConvertInterpolatedStringToConcatenationFadeOut, interpolatedString.StringStartToken);

            foreach (InterpolatedStringContentSyntax content in contents)
            {
                var interpolation = (InterpolationSyntax)content;

                DiagnosticHelpers.ReportToken(context, DiagnosticRules.ConvertInterpolatedStringToConcatenationFadeOut, interpolation.OpenBraceToken);
                DiagnosticHelpers.ReportToken(context, DiagnosticRules.ConvertInterpolatedStringToConcatenationFadeOut, interpolation.CloseBraceToken);
            }

            DiagnosticHelpers.ReportToken(context, DiagnosticRules.ConvertInterpolatedStringToConcatenationFadeOut, interpolatedString.StringEndToken);
        }
    }
}
