// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp;

namespace Roslynator.Formatting.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AddNewLineBetweenClosingBraceAndWhileKeywordOrViceVersaAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.AddNewLineBetweenClosingBraceAndWhileKeywordOrViceVersa);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeDoStatement(f), SyntaxKind.DoStatement);
        }

        private static void AnalyzeDoStatement(SyntaxNodeAnalysisContext context)
        {
            var doStatement = (DoStatementSyntax)context.Node;

            StatementSyntax statement = doStatement.Statement;

            if (!statement.IsKind(SyntaxKind.Block))
                return;

            SyntaxTriviaList trailingTrivia = statement.GetTrailingTrivia();

            if (!trailingTrivia.Any()
                || trailingTrivia.SingleOrDefault(shouldThrow: false).IsWhitespaceTrivia())
            {
                if (!doStatement.WhileKeyword.LeadingTrivia.Any()
                    && !AnalyzerOptions.RemoveNewLineBetweenClosingBraceAndWhileKeyword.IsEnabled(context))
                {
                    context.ReportDiagnostic(
                        DiagnosticRules.AddNewLineBetweenClosingBraceAndWhileKeywordOrViceVersa,
                        Location.Create(doStatement.SyntaxTree, new TextSpan(statement.FullSpan.End, 0)));
                }
            }
            else if (SyntaxTriviaAnalysis.IsOptionalWhitespaceThenEndOfLineTrivia(trailingTrivia))
            {
                if (doStatement.WhileKeyword.LeadingTrivia.IsEmptyOrWhitespace()
                    && AnalyzerOptions.RemoveNewLineBetweenClosingBraceAndWhileKeyword.IsEnabled(context))
                {
                    context.ReportDiagnostic(
                        DiagnosticRules.ReportOnly.RemoveNewLineBetweenClosingBraceAndWhileKeyword,
                        Location.Create(doStatement.SyntaxTree, new TextSpan(trailingTrivia.Last().SpanStart, 0)),
                        properties: DiagnosticProperties.AnalyzerOption_Invert);
                }
            }
        }
    }
}
