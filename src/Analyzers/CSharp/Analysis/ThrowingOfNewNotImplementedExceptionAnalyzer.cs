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
    public sealed class ThrowingOfNewNotImplementedExceptionAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.ThrowingOfNewNotImplementedException);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterCompilationStartAction(startContext =>
            {
                INamedTypeSymbol exceptionSymbol = startContext.Compilation.GetTypeByMetadataName("System.NotImplementedException");

                if (exceptionSymbol == null)
                    return;

                startContext.RegisterSyntaxNodeAction(f => AnalyzeThrowStatement(f, exceptionSymbol), SyntaxKind.ThrowStatement);
                startContext.RegisterSyntaxNodeAction(f => AnalyzeThrowExpression(f, exceptionSymbol), SyntaxKind.ThrowExpression);
            });
        }

        private static void AnalyzeThrowStatement(SyntaxNodeAnalysisContext context, INamedTypeSymbol exceptionSymbol)
        {
            var throwStatement = (ThrowStatementSyntax)context.Node;

            Analyze(context, throwStatement.Expression, exceptionSymbol);
        }

        private static void AnalyzeThrowExpression(SyntaxNodeAnalysisContext context, INamedTypeSymbol exceptionSymbol)
        {
            var throwExpression = (ThrowExpressionSyntax)context.Node;

            Analyze(context, throwExpression.Expression, exceptionSymbol);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context, ExpressionSyntax expression, INamedTypeSymbol exceptionSymbol)
        {
            if (expression?.Kind() != SyntaxKind.ObjectCreationExpression)
                return;

            var objectCreationExpression = (ObjectCreationExpressionSyntax)expression;

            ITypeSymbol typeSymbol = context.SemanticModel.GetTypeSymbol(objectCreationExpression, context.CancellationToken);

            if (typeSymbol == null)
                return;

            if (!SymbolEqualityComparer.Default.Equals(typeSymbol, exceptionSymbol))
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.ThrowingOfNewNotImplementedException,
                expression);
        }
    }
}
