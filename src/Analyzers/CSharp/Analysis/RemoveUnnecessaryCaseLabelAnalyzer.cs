// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RemoveUnnecessaryCaseLabelAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.RemoveUnnecessaryCaseLabel);

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

            if (!switchSection.IsParentKind(SyntaxKind.SwitchStatement))
                return;

            SyntaxList<SwitchLabelSyntax> labels = switchSection.Labels;

            if (labels.Count <= 1)
                return;

            if (!labels.Any(SyntaxKind.DefaultSwitchLabel))
                return;

            foreach (SwitchLabelSyntax label in labels)
            {
                if (!label.IsKind(SyntaxKind.DefaultSwitchLabel)
                    && label.Keyword.TrailingTrivia.IsEmptyOrWhitespace()
                    && label.ColonToken.LeadingTrivia.IsEmptyOrWhitespace())
                {
                    DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.RemoveUnnecessaryCaseLabel, label);
                }
            }
        }
    }
}
