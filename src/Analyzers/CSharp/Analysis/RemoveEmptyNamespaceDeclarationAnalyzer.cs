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
    public sealed class RemoveEmptyNamespaceDeclarationAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.RemoveEmptyNamespaceDeclaration);

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
            var declaration = (NamespaceDeclarationSyntax)context.Node;

            if (declaration.Members.Any())
                return;

            SyntaxToken openBrace = declaration.OpenBraceToken;
            SyntaxToken closeBrace = declaration.CloseBraceToken;

            if (openBrace.IsMissing)
                return;

            if (closeBrace.IsMissing)
                return;

            if (!openBrace.TrailingTrivia.IsEmptyOrWhitespace())
                return;

            if (!closeBrace.LeadingTrivia.IsEmptyOrWhitespace())
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.RemoveEmptyNamespaceDeclaration, declaration);
        }
    }
}
