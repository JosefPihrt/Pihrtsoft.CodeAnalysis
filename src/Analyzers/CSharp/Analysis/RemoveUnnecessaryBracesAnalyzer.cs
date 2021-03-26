// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RemoveUnnecessaryBracesAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticDescriptors.RemoveUnnecessaryBraces,
                    DiagnosticDescriptors.RemoveUnnecessaryBracesFadeOut);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (DiagnosticDescriptors.RemoveUnnecessaryBraces.IsEffective(c))
                        AnalyzerSwitchSection(c);
                },
                SyntaxKind.SwitchSection);
        }

        public static void AnalyzerSwitchSection(SyntaxNodeAnalysisContext context)
        {
            var switchSection = (SwitchSectionSyntax)context.Node;

            if (!(switchSection.Statements.SingleOrDefault(shouldThrow: false) is BlockSyntax block))
                return;

            SyntaxList<StatementSyntax> statements = block.Statements;

            SyntaxList<StatementSyntax>.Enumerator en = statements.GetEnumerator();

            if (!en.MoveNext())
                return;

            do
            {
                if (en.Current.IsKind(SyntaxKind.LocalDeclarationStatement))
                {
                    var localDeclaration = (LocalDeclarationStatementSyntax)en.Current;

                    if (localDeclaration.UsingKeyword.IsKind(SyntaxKind.UsingKeyword))
                        return;
                }

            } while (en.MoveNext());

            SyntaxToken openBrace = block.OpenBraceToken;

            if (!AnalyzeTrivia(openBrace.LeadingTrivia))
                return;

            if (!AnalyzeTrivia(openBrace.TrailingTrivia))
                return;

            if (!AnalyzeTrivia(statements[0].GetLeadingTrivia()))
                return;

            if (!AnalyzeTrivia(statements.Last().GetTrailingTrivia()))
                return;

            SyntaxToken closeBrace = block.CloseBraceToken;

            if (!AnalyzeTrivia(closeBrace.LeadingTrivia))
                return;

            if (!AnalyzeTrivia(closeBrace.TrailingTrivia))
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.RemoveUnnecessaryBraces, openBrace);
            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.RemoveUnnecessaryBracesFadeOut, closeBrace);

            static bool AnalyzeTrivia(SyntaxTriviaList trivia)
            {
                return trivia.All(f => f.IsKind(SyntaxKind.WhitespaceTrivia, SyntaxKind.EndOfLineTrivia, SyntaxKind.SingleLineCommentTrivia));
            }
        }
    }
}
