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
    public sealed class RemoveRedundantDefaultSwitchSectionAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.RemoveRedundantDefaultSwitchSection);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeSwitchStatement(f), SyntaxKind.SwitchStatement);
        }

        private static void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext context)
        {
            var switchStatement = (SwitchStatementSyntax)context.Node;

            if (switchStatement.ContainsDiagnostics)
                return;

            SwitchSectionSyntax defaultSection = switchStatement.DefaultSection();

            if (defaultSection == null)
                return;

            if (!ContainsOnlyBreakStatement(defaultSection))
                return;

            if (switchStatement.DescendantNodes(switchStatement.Sections.Span).Any(f => f.IsKind(SyntaxKind.GotoDefaultStatement)))
                return;

            if (!defaultSection
                .DescendantTrivia(defaultSection.Span)
                .All(f => f.IsWhitespaceOrEndOfLineTrivia()))
            {
                return;
            }

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.RemoveRedundantDefaultSwitchSection, defaultSection);
        }

        private static bool ContainsOnlyBreakStatement(SwitchSectionSyntax switchSection)
        {
            StatementSyntax statement = switchSection.Statements.SingleOrDefault(shouldThrow: false);

            switch (statement?.Kind())
            {
                case SyntaxKind.Block:
                    {
                        return ((BlockSyntax)statement)
                            .Statements
                            .SingleOrDefault(shouldThrow: false)?
                            .Kind() == SyntaxKind.BreakStatement;
                    }
                case SyntaxKind.BreakStatement:
                    {
                        return true;
                    }
            }

            return false;
        }
    }
}
