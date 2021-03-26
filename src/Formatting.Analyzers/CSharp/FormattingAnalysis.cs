// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp;

namespace Roslynator.Formatting.CSharp
{
    internal static class FormattingAnalysis
    {
        public static FormattingSuggestion AnalyzeNewLineBeforeOrAfter(
            SyntaxNodeAnalysisContext context,
            SyntaxToken token,
            ExpressionSyntax expression,
            DiagnosticDescriptor afterDescriptor)
        {
            SyntaxToken previousToken = token.GetPreviousToken();

            if (SyntaxTriviaAnalysis.IsEmptyOrSingleWhitespaceTrivia(previousToken.TrailingTrivia))
            {
                if (!token.LeadingTrivia.Any()
                    && SyntaxTriviaAnalysis.IsOptionalWhitespaceThenEndOfLineTrivia(token.TrailingTrivia)
                    && SyntaxTriviaAnalysis.IsEmptyOrSingleWhitespaceTrivia(expression.GetLeadingTrivia())
                    && !afterDescriptor.IsEffective(context))
                {
                    return FormattingSuggestion.AddNewLineBefore;
                }
            }
            else if (SyntaxTriviaAnalysis.IsOptionalWhitespaceThenEndOfLineTrivia(previousToken.TrailingTrivia))
            {
                if (SyntaxTriviaAnalysis.IsEmptyOrSingleWhitespaceTrivia(token.LeadingTrivia)
                    && SyntaxTriviaAnalysis.IsEmptyOrSingleWhitespaceTrivia(token.TrailingTrivia)
                    && !expression.GetLeadingTrivia().Any()
                    && afterDescriptor.IsEffective(context))
                {
                    return FormattingSuggestion.AddNewLineAfter;
                }
            }

            return FormattingSuggestion.None;
        }
    }
}
