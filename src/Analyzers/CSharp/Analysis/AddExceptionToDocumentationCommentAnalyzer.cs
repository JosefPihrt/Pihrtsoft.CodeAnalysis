// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Analysis.AddExceptionToDocumentationComment;
using static Roslynator.CSharp.Analysis.AddExceptionToDocumentationComment.AddExceptionToDocumentationCommentAnalysis;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AddExceptionToDocumentationCommentAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.AddExceptionToDocumentationComment);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterCompilationStartAction(startContext =>
            {
                INamedTypeSymbol exceptionSymbol = startContext.Compilation.GetTypeByMetadataName("System.Exception");

                if (exceptionSymbol == null)
                    return;

                startContext.RegisterSyntaxNodeAction(f => AnalyzeThrowStatement(f, exceptionSymbol), SyntaxKind.ThrowStatement);
                startContext.RegisterSyntaxNodeAction(f => AnalyzeThrowExpression(f, exceptionSymbol), SyntaxKind.ThrowExpression);
            });
        }

        private static void AnalyzeThrowStatement(SyntaxNodeAnalysisContext context, INamedTypeSymbol exceptionSymbol)
        {
            var throwStatement = (ThrowStatementSyntax)context.Node;

            AddExceptionToDocumentationCommentAnalysisResult analysis = Analyze(throwStatement, exceptionSymbol, context.SemanticModel, context.CancellationToken);

            if (!analysis.Success)
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.AddExceptionToDocumentationComment, throwStatement);
        }

        private static void AnalyzeThrowExpression(SyntaxNodeAnalysisContext context, INamedTypeSymbol exceptionSymbol)
        {
            var throwExpression = (ThrowExpressionSyntax)context.Node;

            AddExceptionToDocumentationCommentAnalysisResult analysis = Analyze(throwExpression, exceptionSymbol, context.SemanticModel, context.CancellationToken);

            if (!analysis.Success)
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.AddExceptionToDocumentationComment, throwExpression);
        }
    }
}
