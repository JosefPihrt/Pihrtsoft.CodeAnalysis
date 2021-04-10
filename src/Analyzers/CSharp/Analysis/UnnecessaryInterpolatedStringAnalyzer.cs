// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using static Roslynator.DiagnosticHelpers;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UnnecessaryInterpolatedStringAnalyzer : BaseDiagnosticAnalyzer
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
                        DiagnosticRules.UnnecessaryInterpolatedString,
                        DiagnosticRules.UnnecessaryInterpolatedStringFadeOut);
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
                    if (DiagnosticRules.UnnecessaryInterpolatedString.IsEffective(c))
                        AnalyzeInterpolatedStringExpression(c);
                },
                SyntaxKind.InterpolatedStringExpression);
        }

        private static void AnalyzeInterpolatedStringExpression(SyntaxNodeAnalysisContext context)
        {
            SyntaxNode node = context.Node;

            if (node.ContainsDiagnostics)
                return;

            if (node.ContainsDirectives)
                return;

            var interpolatedString = (InterpolatedStringExpressionSyntax)node;

            SyntaxList<InterpolatedStringContentSyntax> contents = interpolatedString.Contents;

            if (ConvertInterpolatedStringToStringLiteralAnalysis.IsFixable(contents))
            {
                if (IsFormattableString(context))
                    return;

                ReportDiagnostic(
                    context,
                    DiagnosticRules.UnnecessaryInterpolatedString,
                    Location.Create(interpolatedString.SyntaxTree, GetDollarSpan(interpolatedString)));
            }
            else
            {
                if (!(contents.SingleOrDefault(shouldThrow: false) is InterpolationSyntax interpolation))
                    return;

                if (interpolation.AlignmentClause != null)
                    return;

                if (interpolation.FormatClause != null)
                    return;

                ExpressionSyntax expression = interpolation.Expression?.WalkDownParentheses();

                if (expression == null)
                    return;

                if (!IsNonNullStringExpression(expression))
                    return;

                if (IsFormattableString(context))
                    return;

                ReportDiagnostic(context, DiagnosticRules.UnnecessaryInterpolatedString, interpolatedString);

                ReportToken(context, DiagnosticRules.UnnecessaryInterpolatedStringFadeOut, interpolatedString.StringStartToken);
                ReportToken(context, DiagnosticRules.UnnecessaryInterpolatedStringFadeOut, interpolation.OpenBraceToken);
                ReportToken(context, DiagnosticRules.UnnecessaryInterpolatedStringFadeOut, interpolation.CloseBraceToken);
                ReportToken(context, DiagnosticRules.UnnecessaryInterpolatedStringFadeOut, interpolatedString.StringEndToken);
            }

            bool IsNonNullStringExpression(ExpressionSyntax expression)
            {
                if (expression.IsKind(SyntaxKind.StringLiteralExpression, SyntaxKind.InterpolatedStringExpression))
                    return true;

                Optional<object> constantValue = context.SemanticModel.GetConstantValue(expression, context.CancellationToken);

                return constantValue.HasValue
                    && constantValue.Value is string value
                    && value != null;
            }

            static bool IsFormattableString(SyntaxNodeAnalysisContext context)
            {
                return context
                    .SemanticModel
                    .GetTypeInfo(context.Node, context.CancellationToken)
                    .ConvertedType?
                    .HasMetadataName(MetadataNames.System_FormattableString) == true;
            }
        }

        private static TextSpan GetDollarSpan(InterpolatedStringExpressionSyntax interpolatedString)
        {
            SyntaxToken token = interpolatedString.StringStartToken;

            return new TextSpan(token.SpanStart + token.Text.IndexOf('$'), 1);
        }
    }
}
