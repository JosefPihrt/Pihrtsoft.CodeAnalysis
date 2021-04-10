// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class FormatSummaryOnSingleLineAnalyzer : BaseDiagnosticAnalyzer
    {
        private static readonly Regex _regex = new Regex(
            @"
            ^
            (
                [\s-[\r\n]]*
                \r?\n
                [\s-[\r\n]]*
                ///
                [\s-[\r\n]]*
            )?
            (?<1>[^\r\n]*)
            (
                [\s-[\r\n]]*
                \r?\n
                [\s-[\r\n]]*
                ///
                [\s-[\r\n]]*
            )?
            $
            ",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture);

        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.FormatDocumentationSummaryOnSingleLine);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeSingleLineDocumentationCommentTrivia(f), SyntaxKind.SingleLineDocumentationCommentTrivia);
        }

        private static void AnalyzeSingleLineDocumentationCommentTrivia(SyntaxNodeAnalysisContext context)
        {
            var documentationComment = (DocumentationCommentTriviaSyntax)context.Node;

            XmlElementSyntax summaryElement = documentationComment.SummaryElement();

            if (summaryElement != null)
            {
                XmlElementStartTagSyntax startTag = summaryElement?.StartTag;

                if (startTag?.IsMissing == false)
                {
                    XmlElementEndTagSyntax endTag = summaryElement.EndTag;

                    if (endTag?.IsMissing == false
                        && startTag.GetSpanEndLine() < endTag.GetSpanStartLine())
                    {
                        Match match = _regex.Match(
                            summaryElement.ToString(),
                            startTag.Span.End - summaryElement.SpanStart,
                            endTag.SpanStart - startTag.Span.End);

                        if (match.Success)
                        {
                            DiagnosticHelpers.ReportDiagnostic(
                                context,
                                DiagnosticRules.FormatDocumentationSummaryOnSingleLine,
                                summaryElement);
                        }
                    }
                }
            }
        }
    }
}
