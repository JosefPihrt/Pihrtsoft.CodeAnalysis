// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RemoveRedundantAsOperatorAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.RemoveRedundantAsOperator);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeAsExpression(f), SyntaxKind.AsExpression);
        }

        private static void AnalyzeAsExpression(SyntaxNodeAnalysisContext context)
        {
            var binaryExpression = (BinaryExpressionSyntax)context.Node;

            AsExpressionInfo info = SyntaxInfo.AsExpressionInfo(binaryExpression);

            if (!info.Success)
                return;

            ITypeSymbol typeSymbol = context.SemanticModel.GetTypeSymbol(info.Expression, context.CancellationToken);

            if (typeSymbol?.IsKind(SymbolKind.ErrorType, SymbolKind.DynamicType) != false)
                return;

            typeSymbol = context.SemanticModel.GetTypeSymbol(info.Type, context.CancellationToken);

            if (typeSymbol?.IsKind(SymbolKind.ErrorType, SymbolKind.DynamicType) != false)
                return;

            Conversion conversion = context.SemanticModel.ClassifyConversion(info.Expression, typeSymbol);

            if (!conversion.IsIdentity)
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.RemoveRedundantAsOperator,
                Location.Create(binaryExpression.SyntaxTree, TextSpan.FromBounds(binaryExpression.OperatorToken.SpanStart, info.Type.Span.End)));
        }
    }
}
