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
    public class FormatDeclarationBracesAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.FormatDeclarationBraces); }
        }

        public override void Initialize(AnalysisContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            base.Initialize(context);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeStructDeclaration, SyntaxKind.StructDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeInterfaceDeclaration, SyntaxKind.InterfaceDeclaration);
        }

        public static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;

            if (!classDeclaration.Members.Any())
                Analyze(context, classDeclaration, classDeclaration.OpenBraceToken, classDeclaration.CloseBraceToken);
        }

        public static void AnalyzeStructDeclaration(SyntaxNodeAnalysisContext context)
        {
            var structDeclaration = (StructDeclarationSyntax)context.Node;

            if (!structDeclaration.Members.Any())
                Analyze(context, structDeclaration, structDeclaration.OpenBraceToken, structDeclaration.CloseBraceToken);
        }

        public static void AnalyzeInterfaceDeclaration(SyntaxNodeAnalysisContext context)
        {
            var interfaceDeclaration = (InterfaceDeclarationSyntax)context.Node;

            if (!interfaceDeclaration.Members.Any())
                Analyze(context, interfaceDeclaration, interfaceDeclaration.OpenBraceToken, interfaceDeclaration.CloseBraceToken);
        }

        private static void Analyze(
            SyntaxNodeAnalysisContext context,
            MemberDeclarationSyntax declaration,
            SyntaxToken openBrace,
            SyntaxToken closeBrace)
        {
            if (openBrace.IsMissing)
                return;

            if (closeBrace.IsMissing)
                return;

            if (declaration.SyntaxTree.GetLineCount(TextSpan.FromBounds(openBrace.SpanStart, closeBrace.Span.End), context.CancellationToken) != 1)
                return;

            if (!openBrace.TrailingTrivia.IsEmptyOrWhitespace())
                return;

            if (closeBrace.LeadingTrivia.Any())
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.FormatDeclarationBraces, openBrace);
        }
    }
}
