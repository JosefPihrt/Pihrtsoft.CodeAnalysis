// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RemoveRedundantCommaInInitializerAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.RemoveRedundantCommaInInitializer);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(
                f => AnalyzeInitializerExpression(f),
                SyntaxKind.ArrayInitializerExpression,
                SyntaxKind.ObjectInitializerExpression,
                SyntaxKind.CollectionInitializerExpression);
        }

        private static void AnalyzeInitializerExpression(SyntaxNodeAnalysisContext context)
        {
            var initializer = (InitializerExpressionSyntax)context.Node;

            SeparatedSyntaxList<ExpressionSyntax> expressions = initializer.Expressions;

            if (!expressions.Any())
                return;

            int count = expressions.Count;

            if (count != expressions.SeparatorCount)
                return;

            SyntaxToken token = expressions.GetSeparator(count - 1);

            Debug.Assert(!token.IsMissing);

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.RemoveRedundantCommaInInitializer, token);
        }
    }
}
