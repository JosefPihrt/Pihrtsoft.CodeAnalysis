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
    public class AddEmptyLineBeforeTopDeclarationAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.AddEmptyLineBeforeTopDeclaration); }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeCompilationUnit(f), SyntaxKind.CompilationUnit);
        }

        private static void AnalyzeCompilationUnit(SyntaxNodeAnalysisContext context)
        {
            var compilationUnit = (CompilationUnitSyntax)context.Node;

            MemberDeclarationSyntax declaration = compilationUnit.Members.FirstOrDefault();

            if (declaration == null)
                return;

            if (!SyntaxTriviaAnalysis.IsEmptyOrSingleWhitespaceTrivia(declaration.GetLeadingTrivia()))
                return;

            SyntaxNode node = compilationUnit.AttributeLists.LastOrDefault()
                ?? (SyntaxNode)compilationUnit.Usings.LastOrDefault()
                ?? compilationUnit.Externs.LastOrDefault();

            if (node == null)
                return;

            if (!SyntaxTriviaAnalysis.IsOptionalWhitespaceThenOptionalSingleLineCommentThenEndOfLineTrivia(node.GetTrailingTrivia()))
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticDescriptors.AddEmptyLineBeforeTopDeclaration,
                Location.Create(compilationUnit.SyntaxTree, new TextSpan(node.GetTrailingTrivia().Last().SpanStart, 0)));
        }
    }
}
