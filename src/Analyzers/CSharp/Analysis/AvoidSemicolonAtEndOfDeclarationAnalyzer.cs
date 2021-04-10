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
    public sealed class AvoidSemicolonAtEndOfDeclarationAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.AvoidSemicolonAtEndOfDeclaration);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeNamespaceDeclaration(f), SyntaxKind.NamespaceDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeClassDeclaration(f), SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeInterfaceDeclaration(f), SyntaxKind.InterfaceDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeStructDeclaration(f), SyntaxKind.StructDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeEnumDeclaration(f), SyntaxKind.EnumDeclaration);
        }

        private static void AnalyzeNamespaceDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = (NamespaceDeclarationSyntax)context.Node;

            SyntaxToken semicolon = declaration.SemicolonToken;

            if (semicolon.Parent != null
                && !semicolon.IsMissing)
            {
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.AvoidSemicolonAtEndOfDeclaration, semicolon);
            }
        }

        private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = (ClassDeclarationSyntax)context.Node;

            SyntaxToken semicolon = declaration.SemicolonToken;

            if (semicolon.Parent != null
                && !semicolon.IsMissing)
            {
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.AvoidSemicolonAtEndOfDeclaration, semicolon);
            }
        }

        private static void AnalyzeInterfaceDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = (InterfaceDeclarationSyntax)context.Node;

            SyntaxToken semicolon = declaration.SemicolonToken;

            if (semicolon.Parent != null
                && !semicolon.IsMissing)
            {
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.AvoidSemicolonAtEndOfDeclaration, semicolon);
            }
        }

        private static void AnalyzeStructDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = (StructDeclarationSyntax)context.Node;

            SyntaxToken semicolon = declaration.SemicolonToken;

            if (semicolon.Parent != null
                && !semicolon.IsMissing)
            {
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.AvoidSemicolonAtEndOfDeclaration, semicolon);
            }
        }

        private static void AnalyzeEnumDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = (EnumDeclarationSyntax)context.Node;

            SyntaxToken semicolon = declaration.SemicolonToken;

            if (semicolon.Parent != null
                && !semicolon.IsMissing)
            {
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.AvoidSemicolonAtEndOfDeclaration, semicolon);
            }
        }
    }
}
