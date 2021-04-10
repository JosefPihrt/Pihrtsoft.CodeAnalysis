// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AbstractTypeShouldNotHavePublicConstructorsAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.AbstractTypeShouldNotHavePublicConstructors);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeConstructorDeclaration(f), SyntaxKind.ConstructorDeclaration);
        }

        private static void AnalyzeConstructorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var constructorDeclaration = (ConstructorDeclarationSyntax)context.Node;

            if (!SyntaxAccessibility<ConstructorDeclarationSyntax>.Instance.GetExplicitAccessibility(constructorDeclaration).Is(Accessibility.Public, Accessibility.ProtectedOrInternal))
                return;

            if (!constructorDeclaration.IsParentKind(SyntaxKind.ClassDeclaration))
                return;

            var classDeclaration = (ClassDeclarationSyntax)constructorDeclaration.Parent;

            SyntaxTokenList modifiers = classDeclaration.Modifiers;

            bool isAbstract = modifiers.Contains(SyntaxKind.AbstractKeyword);

            if (!isAbstract
                && modifiers.Contains(SyntaxKind.PartialKeyword))
            {
                INamedTypeSymbol classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration, context.CancellationToken);

                if (classSymbol != null)
                    isAbstract = classSymbol.IsAbstract;
            }

            if (!isAbstract)
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.AbstractTypeShouldNotHavePublicConstructors, constructorDeclaration.Identifier);
        }
    }
}
