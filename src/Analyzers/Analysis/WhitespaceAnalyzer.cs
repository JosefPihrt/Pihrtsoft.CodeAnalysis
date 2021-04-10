// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class WhitespaceAnalyzer : BaseDiagnosticAnalyzer
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
                        DiagnosticRules.RemoveTrailingWhitespace,
                        DiagnosticRules.RemoveRedundantEmptyLine);
                }

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxTreeAction(f => AnalyzeTrailingTrivia(f));
        }

        private static void AnalyzeTrailingTrivia(SyntaxTreeAnalysisContext context)
        {
            if (!context.Tree.TryGetText(out SourceText sourceText))
                return;

            if (!context.Tree.TryGetRoot(out SyntaxNode root))
                return;

            var emptyLines = default(TextSpan);
            var previousLineIsEmpty = false;
            int i = 0;

            foreach (TextLine textLine in sourceText.Lines)
            {
                var lineIsEmpty = false;

                if (textLine.Span.Length == 0)
                {
                    SyntaxTrivia endOfLine = root.FindTrivia(textLine.End);

                    if (endOfLine.IsEndOfLineTrivia())
                    {
                        lineIsEmpty = true;

                        if (previousLineIsEmpty)
                        {
                            if (emptyLines.IsEmpty)
                            {
                                emptyLines = endOfLine.Span;
                            }
                            else
                            {
                                emptyLines = TextSpan.FromBounds(emptyLines.Start, endOfLine.Span.End);
                            }
                        }
                    }
                    else
                    {
                        emptyLines = default;
                    }
                }
                else
                {
                    if (!emptyLines.IsEmpty)
                    {
                        DiagnosticHelpers.ReportDiagnostic(
                            context,
                            DiagnosticRules.RemoveRedundantEmptyLine,
                            Location.Create(context.Tree, emptyLines));
                    }

                    emptyLines = default;

                    int end = textLine.End - 1;

                    if (char.IsWhiteSpace(sourceText[end]))
                    {
                        int start = end;

                        while (start > textLine.Span.Start && char.IsWhiteSpace(sourceText[start - 1]))
                            start--;

                        TextSpan whitespace = TextSpan.FromBounds(start, end + 1);

                        if (root.FindTrivia(start).IsWhitespaceTrivia()
                            || root.FindToken(start, findInsideTrivia: true).IsKind(SyntaxKind.XmlTextLiteralToken))
                        {
                            if (previousLineIsEmpty && start == textLine.Start)
                            {
                                whitespace = TextSpan.FromBounds(
                                    sourceText.Lines[i - 1].End,
                                    whitespace.End);
                            }

                            DiagnosticHelpers.ReportDiagnostic(
                                context,
                                DiagnosticRules.RemoveTrailingWhitespace,
                                Location.Create(context.Tree, whitespace));
                        }
                    }
                }

                previousLineIsEmpty = lineIsEmpty;
                i++;
            }
        }
    }
}
