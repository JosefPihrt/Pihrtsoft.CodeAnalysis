// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.Formatting.CSharp;

namespace Roslynator.Formatting
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class UseLinefeedAsNewLineAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.UseLinefeedAsNewLine); }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxTreeAction(f => AnalyzeSyntaxTree(f));
        }

        private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            if (!context.Tree.TryGetText(out SourceText sourceText))
                return;

            foreach (TextLine textLine in sourceText.Lines)
            {
                int end = textLine.End;

                if (textLine.EndIncludingLineBreak - end == 2
                    && textLine.Text[end] == '\r'
                    && textLine.Text[end + 1] == '\n')
                {
                    DiagnosticHelpers.ReportDiagnostic(
                        context,
                        DiagnosticDescriptors.UseLinefeedAsNewLine,
                        Location.Create(context.Tree, new TextSpan(end, 2)));
                }
            }
        }
    }
}
