// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.Formatting.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AddNewLineAfterOpeningBraceOfAccessorAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.AddNewLineAfterOpeningBraceOfAccessor); }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeAccessorDeclaration(f), SyntaxKind.GetAccessorDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeAccessorDeclaration(f), SyntaxKind.SetAccessorDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeAccessorDeclaration(f), SyntaxKind.InitAccessorDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeAccessorDeclaration(f), SyntaxKind.AddAccessorDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeAccessorDeclaration(f), SyntaxKind.RemoveAccessorDeclaration);
        }

        private static void AnalyzeAccessorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var accessor = (AccessorDeclarationSyntax)context.Node;

            BlockSyntax block = accessor.Body;

            if (block == null)
                return;

            SyntaxToken openBrace = block.OpenBraceToken;

            if (openBrace.IsMissing)
                return;

            if (!accessor.SyntaxTree.IsSingleLineSpan(TextSpan.FromBounds(openBrace.Span.End, openBrace.GetNextToken().SpanStart), context.CancellationToken))
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticDescriptors.AddNewLineAfterOpeningBraceOfAccessor,
                Location.Create(accessor.SyntaxTree, new TextSpan(openBrace.Span.End, 0)));
        }
    }
}
