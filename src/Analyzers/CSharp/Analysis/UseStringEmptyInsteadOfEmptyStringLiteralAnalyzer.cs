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
    public class UseStringEmptyInsteadOfEmptyStringLiteralAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.UseStringEmptyInsteadOfEmptyStringLiteral); }
        }

        public override void Initialize(AnalysisContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            base.Initialize(context);

            context.RegisterSyntaxNodeAction(AnalyzeStringLiteralExpression, SyntaxKind.StringLiteralExpression);
        }

        private static void AnalyzeStringLiteralExpression(SyntaxNodeAnalysisContext context)
        {
            var expressionSyntax = (LiteralExpressionSyntax)context.Node;

            if (UseStringEmptyInsteadOfEmptyStringLiteralAnalysis.IsFixable(expressionSyntax))
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.UseStringEmptyInsteadOfEmptyStringLiteral, expressionSyntax);
        }
    }
}
