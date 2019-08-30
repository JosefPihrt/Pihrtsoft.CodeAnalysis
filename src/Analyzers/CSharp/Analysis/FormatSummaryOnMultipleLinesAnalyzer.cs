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
    public class FormatSummaryOnMultipleLinesAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.FormatDocumentationSummaryOnMultipleLines); }
        }

        public override void Initialize(AnalysisContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            base.Initialize(context);

            context.RegisterSyntaxNodeAction(
                AnalyzeSingleLineDocumentationCommentTrivia,
                SyntaxKind.SingleLineDocumentationCommentTrivia);
        }

        public static void AnalyzeSingleLineDocumentationCommentTrivia(SyntaxNodeAnalysisContext context)
        {
            var documentationComment = (DocumentationCommentTriviaSyntax)context.Node;

            XmlElementSyntax summaryElement = documentationComment.SummaryElement();

            if (summaryElement?.StartTag?.IsMissing == false
                && summaryElement.EndTag?.IsMissing == false
                && summaryElement.IsSingleLine(includeExteriorTrivia: false, trim: false, cancellationToken: context.CancellationToken))
            {
                DiagnosticHelpers.ReportDiagnostic(context,
                    DiagnosticDescriptors.FormatDocumentationSummaryOnMultipleLines,
                    summaryElement);
            }
        }
    }
}
