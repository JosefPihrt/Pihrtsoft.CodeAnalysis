// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AddAccessibilityModifiersOrViceVersaAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableDictionary<Accessibility, ImmutableDictionary<string, string>> _properties;

        private static ImmutableDictionary<Accessibility, ImmutableDictionary<string, string>> Properties
        {
            get
            {
                if (_properties == null)
                    Interlocked.CompareExchange(ref _properties, CreateProperties(), null);

                return _properties;

                static ImmutableDictionary<Accessibility, ImmutableDictionary<string, string>> CreateProperties()
                {
                    return Enum.GetValues(typeof(Accessibility)).Cast<Accessibility>()
                        .Distinct()
                        .Where(f => f != Accessibility.NotApplicable)
                        .ToImmutableDictionary(
                            f => f,
                            f => ImmutableDictionary.CreateRange(new[] { new KeyValuePair<string, string>(nameof(Microsoft.CodeAnalysis.Accessibility), f.ToString()) }));
                }
            }
        }

        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.AddAccessibilityModifiersOrViceVersa);

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

            Analyze(context, classDeclaration, classDeclaration.Modifiers);
        }

        private static void AnalyzeConstructorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var constructorDeclaration = (ConstructorDeclarationSyntax)context.Node;

            Analyze(context, constructorDeclaration, constructorDeclaration.Modifiers);
        }

        private static void AnalyzeConversionOperatorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var conversionOperatorDeclaration = (ConversionOperatorDeclarationSyntax)context.Node;

            Analyze(context, conversionOperatorDeclaration, conversionOperatorDeclaration.Modifiers);
        }

        private static void AnalyzeDelegateDeclaration(SyntaxNodeAnalysisContext context)
        {
            var delegateDeclaration = (DelegateDeclarationSyntax)context.Node;

            Analyze(context, delegateDeclaration, delegateDeclaration.Modifiers);
        }

        private static void AnalyzeEnumDeclaration(SyntaxNodeAnalysisContext context)
        {
            var enumDeclaration = (EnumDeclarationSyntax)context.Node;

            Analyze(context, enumDeclaration, enumDeclaration.Modifiers);
        }

        private static void AnalyzeEventDeclaration(SyntaxNodeAnalysisContext context)
        {
            var eventDeclaration = (EventDeclarationSyntax)context.Node;

            Analyze(context, eventDeclaration, eventDeclaration.Modifiers);
        }

        private static void AnalyzeEventFieldDeclaration(SyntaxNodeAnalysisContext context)
        {
            var eventFieldDeclaration = (EventFieldDeclarationSyntax)context.Node;

            Analyze(context, eventFieldDeclaration, eventFieldDeclaration.Modifiers);
        }

        private static void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
        {
            var fieldDeclaration = (FieldDeclarationSyntax)context.Node;

            Analyze(context, fieldDeclaration, fieldDeclaration.Modifiers);
        }

        private static void AnalyzeIndexerDeclaration(SyntaxNodeAnalysisContext context)
        {
            var indexerDeclaration = (IndexerDeclarationSyntax)context.Node;

            Analyze(context, indexerDeclaration, indexerDeclaration.Modifiers);
        }

        private static void AnalyzeInterfaceDeclaration(SyntaxNodeAnalysisContext context)
        {
            var interfaceDeclaration = (InterfaceDeclarationSyntax)context.Node;

            Analyze(context, interfaceDeclaration, interfaceDeclaration.Modifiers);
        }

        private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            Analyze(context, methodDeclaration, methodDeclaration.Modifiers);
        }

        private static void AnalyzeOperatorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var operatorDeclaration = (OperatorDeclarationSyntax)context.Node;

            Analyze(context, operatorDeclaration, operatorDeclaration.Modifiers);
        }

        private static void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
        {
            var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

            Analyze(context, propertyDeclaration, propertyDeclaration.Modifiers);
        }

        private static void AnalyzeStructDeclaration(SyntaxNodeAnalysisContext context)
        {
            var structDeclaration = (StructDeclarationSyntax)context.Node;

            Analyze(context, structDeclaration, structDeclaration.Modifiers);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context, MemberDeclarationSyntax declaration, SyntaxTokenList modifiers)
        {
            Accessibility explicitAccessibility = SyntaxAccessibility.GetExplicitAccessibility(modifiers);

            if (explicitAccessibility == Accessibility.NotApplicable)
            {
                if (AnalyzerOptions.RemoveAccessibilityModifiers.IsEnabled(context))
                    return;

                Accessibility accessibility = GetAccessibility(context, declaration, modifiers);

                if (accessibility == Accessibility.NotApplicable)
                    return;

                Location location = GetLocation(declaration);

                if (location == null)
                    return;

                DiagnosticHelpers.ReportDiagnostic(
                    context,
                    DiagnosticRules.AddAccessibilityModifiersOrViceVersa,
                    location,
                    Properties[accessibility]);
            }
            else if (AnalyzerOptions.RemoveAccessibilityModifiers.IsEnabled(context)
                && !declaration.IsKind(SyntaxKind.OperatorDeclaration, SyntaxKind.ConversionOperatorDeclaration))
            {
                Accessibility accessibility = SyntaxAccessibility.GetDefaultAccessibility(declaration);

                if (explicitAccessibility != accessibility)
                    return;

                SyntaxToken first = modifiers.First(f => SyntaxFacts.IsAccessibilityModifier(f.Kind()));
                SyntaxToken last = modifiers.Last(f => SyntaxFacts.IsAccessibilityModifier(f.Kind()));

                DiagnosticHelpers.ReportDiagnostic(
                    context,
                    DiagnosticRules.ReportOnly.RemoveAccessibilityModifiers,
                    Location.Create(declaration.SyntaxTree, TextSpan.FromBounds(first.SpanStart, last.Span.End)));
            }
        }

        private static Accessibility GetAccessibility(SyntaxNodeAnalysisContext context, MemberDeclarationSyntax declaration, SyntaxTokenList modifiers)
        {
            if (modifiers.Any(SyntaxKind.PartialKeyword))
            {
                if (!declaration.IsKind(SyntaxKind.MethodDeclaration))
                {
                    Accessibility? accessibility = GetPartialAccessibility(context, declaration);

                    if (accessibility != null)
                    {
                        if (accessibility == Accessibility.NotApplicable)
                        {
                            return SyntaxAccessibility.GetDefaultExplicitAccessibility(declaration);
                        }
                        else
                        {
                            return accessibility.Value;
                        }
                    }
                }
            }
            else
            {
                return SyntaxAccessibility.GetDefaultExplicitAccessibility(declaration);
            }

            return Accessibility.NotApplicable;
        }

        private static Accessibility? GetPartialAccessibility(
            SyntaxNodeAnalysisContext context,
            MemberDeclarationSyntax declaration)
        {
            var accessibility = Accessibility.NotApplicable;

            ISymbol symbol = context.SemanticModel.GetDeclaredSymbol(declaration, context.CancellationToken);

            if (symbol != null)
            {
                foreach (SyntaxReference syntaxReference in symbol.DeclaringSyntaxReferences)
                {
                    if (syntaxReference.GetSyntax(context.CancellationToken) is MemberDeclarationSyntax declaration2)
                    {
                        Accessibility accessibility2 = SyntaxAccessibility.GetExplicitAccessibility(declaration2);

                        if (accessibility2 != Accessibility.NotApplicable)
                        {
                            if (accessibility == Accessibility.NotApplicable || accessibility == accessibility2)
                            {
                                accessibility = accessibility2;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
            }

            return accessibility;
        }

        private static Location GetLocation(SyntaxNode node)
        {
            SyntaxKind kind = node.Kind();

            if (kind == SyntaxKind.OperatorDeclaration)
                return ((OperatorDeclarationSyntax)node).OperatorToken.GetLocation();

            if (kind == SyntaxKind.ConversionOperatorDeclaration)
                return ((ConversionOperatorDeclarationSyntax)node).Type?.GetLocation();

            SyntaxToken token = CSharpUtility.GetIdentifier(node);

            if (!token.IsKind(SyntaxKind.None))
                return token.GetLocation();

            return null;
        }
    }
}
