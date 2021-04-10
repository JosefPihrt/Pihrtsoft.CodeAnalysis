// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp;

namespace Roslynator.Formatting.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AddEmptyLineBetweenBlockAndStatementAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.AddEmptyLineBetweenBlockAndStatement);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeBlock(f), SyntaxKind.Block);
            context.RegisterSyntaxNodeAction(f => AnalyzeTryStatement(f), SyntaxKind.TryStatement);
            context.RegisterSyntaxNodeAction(f => AnalyzeSwitchStatement(f), SyntaxKind.SwitchStatement);
        }

        private static void AnalyzeBlock(SyntaxNodeAnalysisContext context)
        {
            var block = (BlockSyntax)context.Node;

            StatementSyntax blockOrStatement;

            switch (block.Parent.Kind())
            {
                case SyntaxKind.Block:
                    {
                        blockOrStatement = block;
                        break;
                    }
                case SyntaxKind.FixedStatement:
                case SyntaxKind.ForEachStatement:
                case SyntaxKind.ForEachVariableStatement:
                case SyntaxKind.ForStatement:
                case SyntaxKind.CheckedStatement:
                case SyntaxKind.LockStatement:
                case SyntaxKind.UncheckedStatement:
                case SyntaxKind.UnsafeStatement:
                case SyntaxKind.UsingStatement:
                case SyntaxKind.WhileStatement:
                    {
                        blockOrStatement = (StatementSyntax)block.Parent;
                        break;
                    }
                case SyntaxKind.IfStatement:
                    {
                        var ifStatement = (IfStatementSyntax)block.Parent;

                        if (ifStatement.Else == null)
                        {
                            blockOrStatement = ifStatement;
                            break;
                        }
                        else
                        {
                            return;
                        }
                    }
                case SyntaxKind.ElseClause:
                    {
                        var elseClause = (ElseClauseSyntax)block.Parent;

                        blockOrStatement = elseClause.GetTopmostIf();
                        break;
                    }
                default:
                    {
                        return;
                    }
            }

            Analyze(context, block.CloseBraceToken, blockOrStatement);
        }

        private static void AnalyzeTryStatement(SyntaxNodeAnalysisContext context)
        {
            var tryStatement = (TryStatementSyntax)context.Node;

            BlockSyntax block = tryStatement.Finally?.Block ?? tryStatement.Catches.LastOrDefault()?.Block;

            if (block == null)
                return;

            SyntaxToken closeBrace = block.CloseBraceToken;

            if (closeBrace.IsMissing)
                return;

            Analyze(context, closeBrace, tryStatement);
        }

        private static void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext context)
        {
            var switchStatement = (SwitchStatementSyntax)context.Node;

            SyntaxToken closeBrace = switchStatement.CloseBraceToken;

            if (closeBrace.IsMissing)
                return;

            Analyze(context, closeBrace, switchStatement);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context, SyntaxToken closeBrace, StatementSyntax blockOrStatement)
        {
            StatementSyntax nextStatement = (blockOrStatement is IfStatementSyntax ifStatement)
                ? ifStatement.GetTopmostIf().NextStatement()
                : blockOrStatement.NextStatement();

            if (nextStatement != null
                && closeBrace.SyntaxTree.GetLineCount(TextSpan.FromBounds(closeBrace.Span.End, nextStatement.SpanStart)) == 2)
            {
                SyntaxTrivia endOfLine = closeBrace
                    .TrailingTrivia
                    .FirstOrDefault(f => f.IsEndOfLineTrivia());

                if (endOfLine.IsEndOfLineTrivia())
                {
                    DiagnosticHelpers.ReportDiagnostic(
                        context,
                        DiagnosticRules.AddEmptyLineBetweenBlockAndStatement,
                        Location.Create(endOfLine.SyntaxTree, endOfLine.Span.WithLength(0)));
                }
            }
        }
    }
}
