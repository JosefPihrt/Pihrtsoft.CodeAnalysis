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
    public class ConvertInterpolatedStringToConcatenationAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticDescriptors.ConvertInterpolatedStringToConcatenation,
                    DiagnosticDescriptors.ConvertInterpolatedStringToConcatenationFadeOut);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (DiagnosticDescriptors.ConvertInterpolatedStringToConcatenation.IsEffective(c))
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

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.ConvertInterpolatedStringToConcatenation, interpolatedString);

            DiagnosticHelpers.ReportToken(context, DiagnosticDescriptors.ConvertInterpolatedStringToConcatenationFadeOut, interpolatedString.StringStartToken);

            foreach (InterpolatedStringContentSyntax content in contents)
            {
                var interpolation = (InterpolationSyntax)content;

                DiagnosticHelpers.ReportToken(context, DiagnosticDescriptors.ConvertInterpolatedStringToConcatenationFadeOut, interpolation.OpenBraceToken);
                DiagnosticHelpers.ReportToken(context, DiagnosticDescriptors.ConvertInterpolatedStringToConcatenationFadeOut, interpolation.CloseBraceToken);
            }

            DiagnosticHelpers.ReportToken(context, DiagnosticDescriptors.ConvertInterpolatedStringToConcatenationFadeOut, interpolatedString.StringEndToken);
        }
    }
}
