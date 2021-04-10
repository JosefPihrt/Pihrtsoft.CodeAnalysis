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
    public sealed class OrderModifiersAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.OrderModifiers);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeClassDeclaration(f), SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeConstructorDeclaration(f), SyntaxKind.ConstructorDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeConversionOperatorDeclaration(f), SyntaxKind.ConversionOperatorDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeDelegateDeclaration(f), SyntaxKind.DelegateDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeEnumDeclaration(f), SyntaxKind.EnumDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeEventDeclaration(f), SyntaxKind.EventDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeEventFieldDeclaration(f), SyntaxKind.EventFieldDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeFieldDeclaration(f), SyntaxKind.FieldDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeIndexerDeclaration(f), SyntaxKind.IndexerDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeInterfaceDeclaration(f), SyntaxKind.InterfaceDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeMethodDeclaration(f), SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeOperatorDeclaration(f), SyntaxKind.OperatorDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzePropertyDeclaration(f), SyntaxKind.PropertyDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeStructDeclaration(f), SyntaxKind.StructDeclaration);
        }

        private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;

            Analyze(context, classDeclaration.Modifiers);
        }

        private static void AnalyzeConstructorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var constructorDeclaration = (ConstructorDeclarationSyntax)context.Node;

            Analyze(context, constructorDeclaration.Modifiers);
        }

        private static void AnalyzeConversionOperatorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var conversionOperatorDeclaration = (ConversionOperatorDeclarationSyntax)context.Node;

            Analyze(context, conversionOperatorDeclaration.Modifiers);
        }

        private static void AnalyzeDelegateDeclaration(SyntaxNodeAnalysisContext context)
        {
            var delegateDeclaration = (DelegateDeclarationSyntax)context.Node;

            Analyze(context, delegateDeclaration.Modifiers);
        }

        private static void AnalyzeEnumDeclaration(SyntaxNodeAnalysisContext context)
        {
            var enumDeclaration = (EnumDeclarationSyntax)context.Node;

            Analyze(context, enumDeclaration.Modifiers);
        }

        private static void AnalyzeEventDeclaration(SyntaxNodeAnalysisContext context)
        {
            var eventDeclaration = (EventDeclarationSyntax)context.Node;

            Analyze(context, eventDeclaration.Modifiers);
        }

        private static void AnalyzeEventFieldDeclaration(SyntaxNodeAnalysisContext context)
        {
            var eventFieldDeclaration = (EventFieldDeclarationSyntax)context.Node;

            Analyze(context, eventFieldDeclaration.Modifiers);
        }

        private static void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
        {
            var fieldDeclaration = (FieldDeclarationSyntax)context.Node;

            Analyze(context, fieldDeclaration.Modifiers);
        }

        private static void AnalyzeIndexerDeclaration(SyntaxNodeAnalysisContext context)
        {
            var indexerDeclaration = (IndexerDeclarationSyntax)context.Node;

            Analyze(context, indexerDeclaration.Modifiers);
        }

        private static void AnalyzeInterfaceDeclaration(SyntaxNodeAnalysisContext context)
        {
            var interfaceDeclaration = (InterfaceDeclarationSyntax)context.Node;

            Analyze(context, interfaceDeclaration.Modifiers);
        }

        private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            Analyze(context, methodDeclaration.Modifiers);
        }

        private static void AnalyzeOperatorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var operatorDeclaration = (OperatorDeclarationSyntax)context.Node;

            Analyze(context, operatorDeclaration.Modifiers);
        }

        private static void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
        {
            var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

            Analyze(context, propertyDeclaration.Modifiers);
        }

        private static void AnalyzeStructDeclaration(SyntaxNodeAnalysisContext context)
        {
            var structDeclaration = (StructDeclarationSyntax)context.Node;

            Analyze(context, structDeclaration.Modifiers);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context, SyntaxTokenList modifiers)
        {
            if (modifiers.Count <= 1)
                return;

            if (modifiers.IsSorted(ModifierComparer.Default))
                return;

            if (modifiers.SpanContainsDirectives())
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.OrderModifiers,
                Location.Create(context.Node.SyntaxTree, modifiers.Span));
        }
    }
}
