// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator
{
    internal static class TestExtensions
    {
        public static string ToDebugString(this IEnumerable<Diagnostic> diagnostics)
        {
            string s = string.Join("\r\n", diagnostics.Select(d => d.ToString()));

            if (s.Length == 0)
                s = "no diagnostic";

            return $"\r\n\r\nDiagnostics:\r\n{s}\r\n";
        }

        public static async Task<SyntaxNode> GetSyntaxRootAsync(
            this Document document,
            bool simplify,
            bool format,
            CancellationToken cancellationToken = default)
        {
            if (simplify)
                document = await Simplifier.ReduceAsync(document, Simplifier.Annotation, cancellationToken: cancellationToken);

            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);

            if (format)
                root = Formatter.Format(root, Formatter.Annotation, document.Project.Solution.Workspace);

            return root;
        }

        public static LinePositionSpan ToLinePositionSpan(this TextSpan span, string s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            int length = s.Length;

            if (span.Start + span.Length > length)
                throw new ArgumentOutOfRangeException(nameof(span), span, "");

            LinePosition start = LinePosition.Zero;

            start = GetLinePosition(0, span.Start);

            LinePosition end = GetLinePosition(span.Start, span.End);

            return new LinePositionSpan(start, end);

            LinePosition GetLinePosition(int startIndex, int endIndex)
            {
                int i = endIndex - 1;

                while (i >= startIndex)
                {
                    if (s[i] == '\r'
                        || s[i] == '\n')
                    {
                        int character = endIndex - i - 1;

                        int line = start.Line;

                        while (i >= startIndex)
                        {
                            switch (s[i])
                            {
                                case '\n':
                                    {
                                        if (i == startIndex
                                            && i > 0
                                            && s[i - 1] == '\r')
                                        {
                                            throw new InvalidOperationException("Span cannot start of end between '\r' and '\n'.");
                                        }

                                        if (i > startIndex
                                            && s[i - 1] == '\r')
                                        {
                                            i--;
                                        }

                                        line++;
                                        break;
                                    }
                                case '\r':
                                    {
                                        if (i == endIndex - 1
                                            && i < length - 1
                                            && s[i + 1] == '\n')
                                        {
                                            throw new InvalidOperationException("Span cannot start of end between '\r' and '\n'.");
                                        }

                                        line++;
                                        break;
                                    }
                            }

                            i--;
                        }

                        return new LinePosition(line, character);
                    }

                    i--;
                }

                return new LinePosition(start.Line, start.Character + endIndex - startIndex);
            }
        }

        public static CompilationOptions EnsureDiagnosticEnabled(this CompilationOptions compilationOptions, DiagnosticDescriptor descriptor)
        {
            ImmutableDictionary<string, ReportDiagnostic> specificDiagnosticOptions = compilationOptions.SpecificDiagnosticOptions;

            specificDiagnosticOptions = specificDiagnosticOptions.SetItem(
                descriptor.Id,
                descriptor.DefaultSeverity.ToReportDiagnostic());

            return compilationOptions.WithSpecificDiagnosticOptions(specificDiagnosticOptions);
        }
    }
}
