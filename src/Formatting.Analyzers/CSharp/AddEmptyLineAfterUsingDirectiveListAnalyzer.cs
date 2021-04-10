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
    public sealed class AddEmptyLineAfterUsingDirectiveListAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.AddEmptyLineAfterUsingDirectiveList);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeCompilationUnit(f), SyntaxKind.CompilationUnit);
            context.RegisterSyntaxNodeAction(f => AnalyzeNamespaceDeclaration(f), SyntaxKind.NamespaceDeclaration);
        }

        private static void AnalyzeCompilationUnit(SyntaxNodeAnalysisContext context)
        {
            var compilationUnit = (CompilationUnitSyntax)context.Node;

            UsingDirectiveSyntax usingDirective = compilationUnit.Usings.LastOrDefault();

            if (usingDirective == null)
                return;

            SyntaxToken nextToken = compilationUnit.AttributeLists.FirstOrDefault()?.OpenBracketToken
                ?? compilationUnit.Members.FirstOrDefault()?.GetFirstToken()
                ?? default;

            if (nextToken.IsKind(SyntaxKind.None))
                return;

            Analyze(context, usingDirective, nextToken);
        }

        private static void AnalyzeNamespaceDeclaration(SyntaxNodeAnalysisContext context)
        {
            var namespaceDeclaration = (NamespaceDeclarationSyntax)context.Node;

            UsingDirectiveSyntax usingDirective = namespaceDeclaration.Usings.LastOrDefault();

            if (usingDirective == null)
                return;

            SyntaxToken nextToken = namespaceDeclaration.Members.FirstOrDefault()?.GetFirstToken() ?? default;

            if (nextToken.IsKind(SyntaxKind.None))
                return;

            Analyze(context, usingDirective, nextToken);
        }

        private static void Analyze(
            SyntaxNodeAnalysisContext context,
            UsingDirectiveSyntax usingDirective,
            SyntaxToken nextToken)
        {
            SyntaxTriviaList trailingTrivia = usingDirective.GetTrailingTrivia();

            if (!SyntaxTriviaAnalysis.IsOptionalWhitespaceThenOptionalSingleLineCommentThenEndOfLineTrivia(triviaList: trailingTrivia))
                return;

            SyntaxTriviaList.Enumerator en = nextToken.LeadingTrivia.GetEnumerator();

            if (en.MoveNext())
            {
                if (en.Current.IsWhitespaceTrivia()
                    && !en.MoveNext())
                {
                    ReportDiagnostic(trailingTrivia.Last().SpanStart);
                }
                else
                {
                    switch (en.Current.Kind())
                    {
                        case SyntaxKind.SingleLineCommentTrivia:
                        case SyntaxKind.SingleLineDocumentationCommentTrivia:
                        case SyntaxKind.MultiLineDocumentationCommentTrivia:
                            {
                                ReportDiagnostic(trailingTrivia.Last().SpanStart);
                                break;
                            }
                    }
                }
            }
            else
            {
                ReportDiagnostic(trailingTrivia.Last().SpanStart);
            }

            void ReportDiagnostic(int position)
            {
                DiagnosticHelpers.ReportDiagnostic(
                    context,
                    DiagnosticRules.AddEmptyLineAfterUsingDirectiveList,
                    Location.Create(usingDirective.SyntaxTree, new TextSpan(position, 0)));
            }
        }
    }
}
