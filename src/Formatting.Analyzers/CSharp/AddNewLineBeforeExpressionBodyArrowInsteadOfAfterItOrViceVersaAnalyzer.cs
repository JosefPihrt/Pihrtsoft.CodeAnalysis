// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.Formatting.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AddNewLineBeforeExpressionBodyArrowInsteadOfAfterItOrViceVersaAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.AddNewLineBeforeExpressionBodyArrowInsteadOfAfterItOrViceVersa); }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeArrowExpressionClause(f), SyntaxKind.ArrowExpressionClause);
        }

        private static void AnalyzeArrowExpressionClause(SyntaxNodeAnalysisContext context)
        {
            var arrowExpressionClause = (ArrowExpressionClauseSyntax)context.Node;

            SyntaxToken arrowToken = arrowExpressionClause.ArrowToken;

            FormattingSuggestion suggestion = FormattingAnalysis.AnalyzeNewLineBeforeOrAfter(context, arrowToken, arrowExpressionClause.Expression, AnalyzerOptionDiagnosticDescriptors.AddNewLineAfterExpressionBodyArrowInsteadOfBeforeIt);

            if (suggestion == FormattingSuggestion.AddNewLineBefore)
            {
                DiagnosticHelpers.ReportDiagnostic(
                    context,
                    DiagnosticDescriptors.AddNewLineBeforeExpressionBodyArrowInsteadOfAfterItOrViceVersa,
                    arrowToken.GetLocation());
            }
            else if (suggestion == FormattingSuggestion.AddNewLineAfter)
            {
                DiagnosticHelpers.ReportDiagnostic(
                    context,
                    DiagnosticDescriptors.ReportOnly.AddNewLineAfterExpressionBodyArrowInsteadOfBeforeIt,
                    arrowToken.GetLocation(),
                    properties: DiagnosticProperties.AnalyzerOption_Invert);
            }
        }
    }
}
