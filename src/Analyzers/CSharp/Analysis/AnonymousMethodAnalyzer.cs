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
    public class AnonymousMethodAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticDescriptors.UseLambdaExpressionInsteadOfAnonymousMethod,
                    DiagnosticDescriptors.UseLambdaExpressionInsteadOfAnonymousMethodFadeOut);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (DiagnosticDescriptors.UseLambdaExpressionInsteadOfAnonymousMethod.IsEffective(c))
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
                    DiagnosticDescriptors.UseLambdaExpressionInsteadOfAnonymousMethod,
                    anonymousMethod);

                FadeOut(context, anonymousMethod);
            }
        }

        private static void FadeOut(SyntaxNodeAnalysisContext context, AnonymousMethodExpressionSyntax anonymousMethod)
        {
            DiagnosticHelpers.ReportToken(context, DiagnosticDescriptors.UseLambdaExpressionInsteadOfAnonymousMethodFadeOut, anonymousMethod.DelegateKeyword);

            BlockSyntax block = anonymousMethod.Block;

            SyntaxList<StatementSyntax> statements = block.Statements;

            if (statements.Count == 1
                && block.IsSingleLine())
            {
                StatementSyntax statement = statements[0];

                if (statement.IsKind(SyntaxKind.ReturnStatement, SyntaxKind.ExpressionStatement))
                {
                    CSharpDiagnosticHelpers.ReportBraces(context, DiagnosticDescriptors.UseLambdaExpressionInsteadOfAnonymousMethodFadeOut, block);

                    if (statement.IsKind(SyntaxKind.ReturnStatement))
                        DiagnosticHelpers.ReportToken(context, DiagnosticDescriptors.UseLambdaExpressionInsteadOfAnonymousMethodFadeOut, ((ReturnStatementSyntax)statement).ReturnKeyword);
                }
            }
        }
    }
}
