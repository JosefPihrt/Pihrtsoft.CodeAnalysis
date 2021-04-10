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
    public sealed class DeclareUsingDirectiveOnTopLevelAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.DeclareUsingDirectiveOnTopLevel);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeNamespaceDeclaration(f), SyntaxKind.NamespaceDeclaration);
        }

        private static void AnalyzeNamespaceDeclaration(SyntaxNodeAnalysisContext context)
        {
            var namespaceDeclaration = (NamespaceDeclarationSyntax)context.Node;

            SyntaxList<UsingDirectiveSyntax> usings = namespaceDeclaration.Usings;

            if (!usings.Any())
                return;

            int count = usings.Count;

            for (int i = 0; i < count; i++)
            {
                if (usings[i].ContainsDiagnostics)
                    return;

                if (i == 0)
                {
                    if (usings[i].SpanOrTrailingTriviaContainsDirectives())
                        return;
                }
                else if (i == count - 1)
                {
                    if (usings[i].SpanOrLeadingTriviaContainsDirectives())
                        return;
                }
                else if (usings[i].ContainsDirectives)
                {
                    return;
                }
            }

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.DeclareUsingDirectiveOnTopLevel,
                Location.Create(namespaceDeclaration.SyntaxTree, usings.Span));
        }
    }
}
