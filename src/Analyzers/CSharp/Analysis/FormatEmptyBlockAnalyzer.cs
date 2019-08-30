// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FormatEmptyBlockAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.FormatEmptyBlock); }
        }

        public override void Initialize(AnalysisContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            base.Initialize(context);

            context.RegisterSyntaxNodeAction(AnalyzeBlock, SyntaxKind.Block);
        }

        public static void AnalyzeBlock(SyntaxNodeAnalysisContext context)
        {
            var block = (BlockSyntax)context.Node;

            SyntaxList<StatementSyntax> statements = block.Statements;

            if (statements.Any())
                return;

            if (block.Parent is AccessorDeclarationSyntax)
                return;

            if (block.Parent is AnonymousFunctionExpressionSyntax)
                return;

            SyntaxToken openBrace = block.OpenBraceToken;
            SyntaxToken closeBrace = block.CloseBraceToken;

            if (block.SyntaxTree.GetLineCount(TextSpan.FromBounds(openBrace.SpanStart, closeBrace.Span.End), context.CancellationToken) != 1)
                return;

            if (!openBrace.TrailingTrivia.IsEmptyOrWhitespace())
                return;

            if (closeBrace.LeadingTrivia.Any())
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.FormatEmptyBlock, block);
        }
    }
}
