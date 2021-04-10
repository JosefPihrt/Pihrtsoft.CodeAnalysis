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
    public sealed class RemoveRedundantAutoPropertyInitializationAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.RemoveRedundantAutoPropertyInitialization);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzePropertyDeclaration(f), SyntaxKind.PropertyDeclaration);
        }

        private static void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.ContainsDiagnostics)
                return;

            var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

            EqualsValueClauseSyntax initializer = propertyDeclaration.Initializer;

            ExpressionSyntax value = initializer?.Value?.WalkDownParentheses();

            if (value?.IsKind(SyntaxKind.SuppressNullableWarningExpression) != false)
                return;

            if (initializer.SpanOrLeadingTriviaContainsDirectives())
                return;

            if (propertyDeclaration.AccessorList?.Accessors.Any(f => !f.IsAutoImplemented()) != false)
                return;

            ITypeSymbol typeSymbol = context.SemanticModel.GetTypeSymbol(propertyDeclaration.Type, context.CancellationToken);

            if (typeSymbol == null)
                return;

            if (!context.SemanticModel.IsDefaultValue(typeSymbol, value, context.CancellationToken))
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.RemoveRedundantAutoPropertyInitialization, value);
        }
    }
}
