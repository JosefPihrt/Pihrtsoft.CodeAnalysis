// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AddEmptyLineBeforeWhileInDoStatementAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.AddEmptyLineBeforeWhileInDoStatement); }
        }

        public override void Initialize(AnalysisContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            base.Initialize(context);

            context.RegisterSyntaxNodeAction(AnalyzeDoStatement, SyntaxKind.DoStatement);
        }

        public static void AnalyzeDoStatement(SyntaxNodeAnalysisContext context)
        {
            var doStatement = (DoStatementSyntax)context.Node;

            StatementSyntax statement = doStatement.Statement;

            if (statement?.Kind() != SyntaxKind.Block)
                return;

            var block = (BlockSyntax)statement;

            StatementSyntax lastStatement = block.Statements.LastOrDefault();

            if (lastStatement == null)
                return;

            SyntaxToken closeBrace = block.CloseBraceToken;

            if (closeBrace.IsMissing)
                return;

            SyntaxToken whileKeyword = doStatement.WhileKeyword;

            if (whileKeyword.IsMissing)
                return;

            int closeBraceLine = closeBrace.GetSpanEndLine(context.CancellationToken);

            if (closeBraceLine != whileKeyword.GetSpanStartLine(context.CancellationToken))
                return;

            int line = lastStatement.GetSpanEndLine(context.CancellationToken);

            if (closeBraceLine - line != 1)
                return;

            SyntaxTrivia trivia = lastStatement
                .GetTrailingTrivia()
                .FirstOrDefault(f => f.IsEndOfLineTrivia());

            if (!trivia.IsEndOfLineTrivia())
                return;

            DiagnosticHelpers.ReportDiagnostic(context,
                DiagnosticDescriptors.AddEmptyLineBeforeWhileInDoStatement,
                Location.Create(doStatement.SyntaxTree, trivia.Span));
        }
    }
}
