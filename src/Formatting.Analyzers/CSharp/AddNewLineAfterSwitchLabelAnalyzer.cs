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
    public sealed class AddNewLineAfterSwitchLabelAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.AddNewLineAfterSwitchLabel);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeSwitchSection(f), SyntaxKind.SwitchSection);
        }

        private static void AnalyzeSwitchSection(SyntaxNodeAnalysisContext context)
        {
            var switchSection = (SwitchSectionSyntax)context.Node;

            SwitchLabelSyntax label = switchSection.Labels.LastOrDefault();

            if (label == null)
                return;

            StatementSyntax statement = switchSection.Statements.FirstOrDefault();

            if (statement == null)
                return;

            if (!switchSection.SyntaxTree.IsSingleLineSpan(TextSpan.FromBounds(label.Span.End, statement.SpanStart)))
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.AddNewLineAfterSwitchLabel,
                Location.Create(statement.SyntaxTree, statement.Span.WithLength(0)));
        }
    }
}
