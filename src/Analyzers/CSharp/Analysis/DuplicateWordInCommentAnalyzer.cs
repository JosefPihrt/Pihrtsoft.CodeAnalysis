// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
    public sealed class DuplicateWordInCommentAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.DuplicateWordInComment);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeXmlText(f), SyntaxKind.XmlText);
        }

        private static void AnalyzeXmlText(SyntaxNodeAnalysisContext context)
        {
            var xmlText = (XmlTextSyntax)context.Node;

            string s;
            int length;
            int i;
            int index1;
            int len;

            foreach (SyntaxToken token in xmlText.TextTokens)
            {
                if (!token.IsKind(SyntaxKind.XmlTextLiteralToken))
                    continue;

                s = token.Text;
                length = s.Length;
                i = 0;

                while (i < length)
                {
                    if ((i == 0 && char.IsLetter(s[i]))
                        || ScanToWord())
                    {
                        index1 = i;

                        if (ScanWord())
                        {
                            len = i - index1;

                            while (true)
                            {
                                if (ScanToNextWord())
                                {
                                    int index2 = i;

                                    if (ScanWord())
                                    {
                                        int len2 = i - index2;

                                        if (len == len2
                                            && string.Compare(s, index1, s, index2, len, StringComparison.Ordinal) == 0)
                                        {
                                            ReportDiagnostic(
                                                context,
                                                DiagnosticRules.DuplicateWordInComment,
                                                Location.Create(context.Node.SyntaxTree, TextSpan.FromBounds(token.SpanStart + index2, token.SpanStart + index2 + len2)));
                                        }

                                        index1 = index2;
                                        len = len2;
                                        continue;
                                    }
                                }

                                break;
                            }
                        }
                    }
                }
            }

            bool ScanWord()
            {
                int j = i + 1;
                while (j < length)
                {
                    if (char.IsLetterOrDigit(s[j]))
                    {
                        j++;

                        if (j == length)
                        {
                            i = j;
                            return true;
                        }

                        continue;
                    }

                    if (char.IsWhiteSpace(s[j])
                        || j == length - 1)
                    {
                        if (j > i + 1)
                        {
                            i = j;
                            return true;
                        }
                    }

                    break;
                }

                i = j;
                return false;
            }

            bool ScanToWord()
            {
                int j = i;
                while (j < length)
                {
                    if (char.IsWhiteSpace(s[j]))
                    {
                        i = j;

                        if (ScanToNextWord())
                            return true;
                    }

                    j++;
                }

                i = j;
                return false;
            }

            bool ScanToNextWord()
            {
                int j = i + 1;
                while (j < length)
                {
                    if (char.IsWhiteSpace(s[j]))
                    {
                        j++;
                        continue;
                    }

                    if (char.IsLetter(s[j]))
                    {
                        i = j;
                        return true;
                    }

                    return false;
                }

                i = j;
                return false;
            }
        }
    }
}
