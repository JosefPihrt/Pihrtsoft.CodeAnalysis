// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AnonymousMethodAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                {
                    Immutable.InterlockedInitialize(
                        ref _supportedDiagnostics,
                        DiagnosticRules.UseLambdaExpressionInsteadOfAnonymousMethod,
                        DiagnosticRules.UseLambdaExpressionInsteadOfAnonymousMethodFadeOut);
                }

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (DiagnosticRules.UseLambdaExpressionInsteadOfAnonymousMethod.IsEffective(c))
                        AnalyzeAnonymousMethod(c);
                },
                SyntaxKind.AnonymousMethodExpression);
        }

        private static void AnalyzeAnonymousMethod(SyntaxNodeAnalysisContext context)
        {
            var anonymousMethod = (AnonymousMethodExpressionSyntax)context.Node;

            if (UseLambdaExpressionInsteadOfAnonymousMethodAnalysis.IsFixable(anonymousMethod))
            {
                DiagnosticHelpers.ReportDiagnostic(
                    context,
                    DiagnosticRules.UseLambdaExpressionInsteadOfAnonymousMethod,
                    anonymousMethod);

                FadeOut(context, anonymousMethod);
            }
        }

        private static void FadeOut(SyntaxNodeAnalysisContext context, AnonymousMethodExpressionSyntax anonymousMethod)
        {
            DiagnosticHelpers.ReportToken(context, DiagnosticRules.UseLambdaExpressionInsteadOfAnonymousMethodFadeOut, anonymousMethod.DelegateKeyword);

            BlockSyntax block = anonymousMethod.Block;

            SyntaxList<StatementSyntax> statements = block.Statements;

            if (statements.Count == 1
                && block.IsSingleLine())
            {
                StatementSyntax statement = statements[0];

                if (statement.IsKind(SyntaxKind.ReturnStatement, SyntaxKind.ExpressionStatement))
                {
                    CSharpDiagnosticHelpers.ReportBraces(context, DiagnosticRules.UseLambdaExpressionInsteadOfAnonymousMethodFadeOut, block);

                    if (statement.IsKind(SyntaxKind.ReturnStatement))
                        DiagnosticHelpers.ReportToken(context, DiagnosticRules.UseLambdaExpressionInsteadOfAnonymousMethodFadeOut, ((ReturnStatementSyntax)statement).ReturnKeyword);
                }
            }
        }
    }
}
