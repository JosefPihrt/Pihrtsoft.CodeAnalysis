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
    public sealed class RemoveEmptyInitializerAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.RemoveEmptyInitializer);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeObjectCreationExpression(f), SyntaxKind.ObjectCreationExpression);
        }

        private static void AnalyzeObjectCreationExpression(SyntaxNodeAnalysisContext context)
        {
            var objectCreationExpression = (ObjectCreationExpressionSyntax)context.Node;

            if (objectCreationExpression.ContainsDiagnostics)
                return;

            TypeSyntax type = objectCreationExpression.Type;

            if (type?.IsMissing != false)
                return;

            InitializerExpressionSyntax initializer = objectCreationExpression.Initializer;

            if (initializer?.Expressions.Any() != false)
                return;

            if (!initializer.OpenBraceToken.TrailingTrivia.IsEmptyOrWhitespace())
                return;

            if (!initializer.CloseBraceToken.LeadingTrivia.IsEmptyOrWhitespace())
                return;

            if (initializer.IsInExpressionTree(context.SemanticModel, context.CancellationToken))
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.RemoveEmptyInitializer, initializer);
        }
    }
}
