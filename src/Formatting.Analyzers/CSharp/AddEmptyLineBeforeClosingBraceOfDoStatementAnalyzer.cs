// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp;

namespace Roslynator.Formatting.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AddEmptyLineBeforeClosingBraceOfDoStatementAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.AddEmptyLineBeforeClosingBraceOfDoStatement); }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeDoStatement(f), SyntaxKind.DoStatement);
        }

        private static void AnalyzeDoStatement(SyntaxNodeAnalysisContext context)
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

            int closeBraceLine = closeBrace.GetSpanEndLine();

            if (closeBraceLine != whileKeyword.GetSpanStartLine())
                return;

            int line = lastStatement.GetSpanEndLine(context.CancellationToken);

            if (closeBraceLine - line != 1)
                return;

            SyntaxTrivia trivia = lastStatement
                .GetTrailingTrivia()
                .FirstOrDefault(f => f.IsEndOfLineTrivia());

            if (!trivia.IsEndOfLineTrivia())
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticDescriptors.AddEmptyLineBeforeClosingBraceOfDoStatement,
                Location.Create(trivia.SyntaxTree, trivia.Span.WithLength(0)));
        }
    }
}
