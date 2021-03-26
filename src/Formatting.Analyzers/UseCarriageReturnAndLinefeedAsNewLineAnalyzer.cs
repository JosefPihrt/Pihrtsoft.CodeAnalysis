// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.Formatting.CSharp;

namespace Roslynator.Formatting
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class UseCarriageReturnAndLinefeedAsNewLineAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.UseCarriageReturnAndLinefeedAsNewLine); }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxTreeAction(f => Analyze(f));
        }

        private static void Analyze(SyntaxTreeAnalysisContext context)
        {
            if (!context.Tree.TryGetText(out SourceText sourceText))
                return;

            foreach (TextLine textLine in sourceText.Lines)
            {
                int end = textLine.End;

                if (textLine.EndIncludingLineBreak - end == 1
                    && textLine.Text[end] == '\n')
                {
                    DiagnosticHelpers.ReportDiagnostic(
                        context,
                        DiagnosticDescriptors.UseCarriageReturnAndLinefeedAsNewLine,
                        Location.Create(context.Tree, new TextSpan(end, 1)));
                }
            }
        }
    }
}
