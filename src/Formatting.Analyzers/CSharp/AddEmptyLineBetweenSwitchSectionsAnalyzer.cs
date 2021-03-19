// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp;

namespace Roslynator.Formatting.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AddEmptyLineBetweenSwitchSectionsAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.AddEmptyLineBetweenSwitchSections); }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeSwitchStatement(f), SyntaxKind.SwitchStatement);
        }

        private static void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext context)
        {
            var switchStatement = (SwitchStatementSyntax)context.Node;

            SyntaxList<SwitchSectionSyntax> sections = switchStatement.Sections;

            SyntaxList<SwitchSectionSyntax>.Enumerator en = sections.GetEnumerator();

            if (!en.MoveNext())
                return;

            SwitchSectionSyntax previousSection = en.Current;

            while (en.MoveNext())
            {
                SyntaxTriviaList leadingTrivia = en.Current.Labels[0].GetLeadingTrivia();

                if (SyntaxTriviaAnalysis.IsEmptyOrSingleWhitespaceTrivia(leadingTrivia))
                {
                    SyntaxTriviaList trailingTrivia = previousSection.GetTrailingTrivia();

                    if (SyntaxTriviaAnalysis.IsOptionalWhitespaceThenOptionalSingleLineCommentThenEndOfLineTrivia(trailingTrivia))
                    {
                        DiagnosticHelpers.ReportDiagnostic(
                            context,
                            DiagnosticDescriptors.AddEmptyLineBetweenSwitchSections,
                            Location.Create(switchStatement.SyntaxTree, trailingTrivia.Last().Span.WithLength(0)));
                    }
                }

                previousSection = en.Current;
            }
        }
    }
}
