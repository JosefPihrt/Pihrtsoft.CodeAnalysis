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
    public class AddNewLineAfterSwitchLabelAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.AddNewLineAfterSwitchLabel); }
        }

        public override void Initialize(AnalysisContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            base.Initialize(context);

            context.RegisterSyntaxNodeAction(AnalyzeSwitchSection, SyntaxKind.SwitchSection);
        }

        public static void AnalyzeSwitchSection(SyntaxNodeAnalysisContext context)
        {
            var switchSection = (SwitchSectionSyntax)context.Node;

            SyntaxList<StatementSyntax> statements = switchSection.Statements;

            if (!statements.Any())
                return;

            SyntaxList<SwitchLabelSyntax> labels = switchSection.Labels;

            if (!labels.Any())
                return;

            StatementSyntax statement = statements[0];

            if (!switchSection.SyntaxTree.IsSingleLineSpan(TextSpan.FromBounds(labels.Last().Span.End, statement.SpanStart), context.CancellationToken))
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.AddNewLineAfterSwitchLabel, statement);
        }
    }
}
