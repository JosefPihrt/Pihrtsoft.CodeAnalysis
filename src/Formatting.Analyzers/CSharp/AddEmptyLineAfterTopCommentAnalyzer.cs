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
    public class AddEmptyLineAfterTopCommentAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.AddEmptyLineAfterTopComment); }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeCompilationUnit(f), SyntaxKind.CompilationUnit);
        }

        private static void AnalyzeCompilationUnit(SyntaxNodeAnalysisContext context)
        {
            var compilationUnit = (CompilationUnitSyntax)context.Node;

            SyntaxNode node = compilationUnit.Externs.FirstOrDefault()
                ?? compilationUnit.Usings.FirstOrDefault()
                ?? (SyntaxNode)compilationUnit.AttributeLists.FirstOrDefault()
                ?? compilationUnit.Members.FirstOrDefault();

            if (node == null)
                return;

            SyntaxTriviaList.Enumerator en = node.GetLeadingTrivia().GetEnumerator();

            if (!en.MoveNext())
                return;

            if (en.Current.SpanStart != 0)
                return;

            if (en.Current.IsWhitespaceTrivia()
                && !en.MoveNext())
            {
                return;
            }

            if (!en.Current.IsKind(SyntaxKind.SingleLineCommentTrivia, SyntaxKind.MultiLineCommentTrivia))
                return;

            if (!en.MoveNext()
                || !en.Current.IsEndOfLineTrivia()
                || en.MoveNext())
            {
                return;
            }

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticDescriptors.AddEmptyLineAfterTopComment,
                Location.Create(compilationUnit.SyntaxTree, new TextSpan(node.GetLeadingTrivia().Last().SpanStart, 0)));
        }
    }
}
