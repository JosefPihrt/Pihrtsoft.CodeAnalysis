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
    public class RemoveNewLineBetweenIfKeywordAndElseKeywordAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.RemoveNewLineBetweenIfKeywordAndElseKeyword); }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeElseClause(f), SyntaxKind.ElseClause);
        }

        private static void AnalyzeElseClause(SyntaxNodeAnalysisContext context)
        {
            var elseClause = (ElseClauseSyntax)context.Node;

            StatementSyntax statement = elseClause.Statement;

            if (!statement.IsKind(SyntaxKind.IfStatement))
                return;

            SyntaxTriviaList trailingTrivia = elseClause.ElseKeyword.TrailingTrivia;

            if (!SyntaxTriviaAnalysis.IsOptionalWhitespaceThenEndOfLineTrivia(trailingTrivia))
                return;

            if (!statement.GetLeadingTrivia().IsEmptyOrWhitespace())
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticDescriptors.RemoveNewLineBetweenIfKeywordAndElseKeyword,
                Location.Create(elseClause.SyntaxTree, new TextSpan(trailingTrivia.Last().SpanStart, 0)));
        }
    }
}
