// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp.SyntaxWalkers;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ValidateArgumentsCorrectlyAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.ValidateArgumentsCorrectly);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeMethodDeclaration(f), SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            BlockSyntax body = methodDeclaration.Body;

            if (body == null)
                return;

            ParameterListSyntax parameterList = methodDeclaration.ParameterList;

            if (parameterList == null)
                return;

            if (!parameterList.Parameters.Any())
                return;

            SyntaxList<StatementSyntax> statements = body.Statements;

            int statementCount = statements.Count;

            int index = -1;
            for (int i = 0; i < statementCount; i++)
            {
                if (IsNullCheck(statements[i]))
                {
                    index++;
                }
                else
                {
                    break;
                }
            }

            if (index == -1)
                return;

            if (index == statementCount - 1)
                return;

            TextSpan span = TextSpan.FromBounds(statements[index + 1].SpanStart, statements.Last().Span.End);

            if (body.ContainsUnbalancedIfElseDirectives(span))
                return;

            context.CancellationToken.ThrowIfCancellationRequested();

            ContainsYieldWalker walker = ContainsYieldWalker.GetInstance();

            walker.VisitBlock(body);

            YieldStatementSyntax yieldStatement = walker.YieldStatement;

            ContainsYieldWalker.Free(walker);

            if (yieldStatement == null)
                return;

            if (yieldStatement.SpanStart < statements[index].Span.End)
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.ValidateArgumentsCorrectly,
                Location.Create(body.SyntaxTree, new TextSpan(statements[index + 1].SpanStart, 0)));
        }

        private static bool IsNullCheck(StatementSyntax statement)
        {
            return statement.IsKind(SyntaxKind.IfStatement)
                && ((IfStatementSyntax)statement).SingleNonBlockStatementOrDefault().IsKind(SyntaxKind.ThrowStatement);
        }
    }
}
