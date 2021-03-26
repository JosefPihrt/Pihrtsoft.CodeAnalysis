// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AddArgumentListToObjectCreationOrViceVersaAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.AddArgumentListToObjectCreationOrViceVersa); }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeObjectCreationExpression(f), SyntaxKind.ObjectCreationExpression);
        }

        private static void AnalyzeObjectCreationExpression(SyntaxNodeAnalysisContext context)
        {
            var objectCreationExpression = (ObjectCreationExpressionSyntax)context.Node;

            if (objectCreationExpression.Type?.IsMissing != false)
                return;

            if (objectCreationExpression.Initializer?.IsMissing != false)
                return;

            ArgumentListSyntax argumentList = objectCreationExpression.ArgumentList;

            if (argumentList == null)
            {
                if (!AnalyzerOptions.RemoveArgumentListFromObjectCreation.IsEnabled(context))
                {
                    var span = new TextSpan(objectCreationExpression.Type.Span.End, 0);

                    DiagnosticHelpers.ReportDiagnostic(
                        context,
                        DiagnosticDescriptors.AddArgumentListToObjectCreationOrViceVersa,
                        Location.Create(objectCreationExpression.SyntaxTree, span));
                }
            }
            else if (!argumentList.Arguments.Any()
                && AnalyzerOptions.RemoveArgumentListFromObjectCreation.IsEnabled(context))
            {
                SyntaxToken openParen = argumentList.OpenParenToken;
                SyntaxToken closeParen = argumentList.CloseParenToken;

                if (!openParen.IsMissing
                    && !closeParen.IsMissing
                    && openParen.TrailingTrivia.IsEmptyOrWhitespace()
                    && closeParen.LeadingTrivia.IsEmptyOrWhitespace())
                {
                    DiagnosticHelpers.ReportDiagnostic(
                        context,
                        DiagnosticDescriptors.ReportOnly.RemoveArgumentListFromObjectCreation,
                        argumentList);
                }
            }
        }
    }
}
