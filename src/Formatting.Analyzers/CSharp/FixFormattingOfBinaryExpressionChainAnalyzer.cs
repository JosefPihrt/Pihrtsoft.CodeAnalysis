// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp;
using static Roslynator.CSharp.SyntaxTriviaAnalysis;

namespace Roslynator.Formatting.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class FixFormattingOfBinaryExpressionChainAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.FixFormattingOfBinaryExpressionChain, CommonDiagnosticRules.AnalyzerOptionIsObsolete);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(
                f => AnalyzeBinaryExpression(f),
                SyntaxKind.AddExpression,
                SyntaxKind.SubtractExpression,
                SyntaxKind.MultiplyExpression,
                SyntaxKind.DivideExpression,
                SyntaxKind.ModuloExpression,
                SyntaxKind.LeftShiftExpression,
                SyntaxKind.RightShiftExpression,
                SyntaxKind.LogicalOrExpression,
                SyntaxKind.LogicalAndExpression,
                SyntaxKind.BitwiseOrExpression,
                SyntaxKind.BitwiseAndExpression,
                SyntaxKind.ExclusiveOrExpression,
                SyntaxKind.CoalesceExpression);
        }

        private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
        {
            var topBinaryExpression = (BinaryExpressionSyntax)context.Node;

            SyntaxKind binaryKind = topBinaryExpression.Kind();

            if (topBinaryExpression.Parent.IsKind(binaryKind))
                return;

            if (topBinaryExpression.IsSingleLine(includeExteriorTrivia: false))
                return;

            int? indentationLength = null;

            BinaryExpressionSyntax binaryExpression = topBinaryExpression;

            while (true)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                ExpressionSyntax left = binaryExpression.Left;
                SyntaxToken token = binaryExpression.OperatorToken;

                SyntaxTriviaList leftTrailing = left.GetTrailingTrivia();
                SyntaxTriviaList tokenTrailing = token.TrailingTrivia;

                if (IsOptionalWhitespaceThenOptionalSingleLineCommentThenEndOfLineTrivia(leftTrailing))
                {
                    if (Analyze(token))
                        return;
                }
                else if (IsOptionalWhitespaceThenOptionalSingleLineCommentThenEndOfLineTrivia(tokenTrailing))
                {
                    if (Analyze(binaryExpression.Right))
                        return;
                }
                else if (leftTrailing.IsEmptyOrWhitespace()
                    && tokenTrailing.IsEmptyOrWhitespace())
                {
                    ReportDiagnostic();
                    return;
                }

                if (!left.IsKind(binaryKind))
                    break;

                binaryExpression = (BinaryExpressionSyntax)left;
            }

            bool Analyze(SyntaxNodeOrToken nodeOrToken)
            {
                SyntaxTriviaList.Reversed.Enumerator en = nodeOrToken.GetLeadingTrivia().Reverse().GetEnumerator();

                if (!en.MoveNext())
                {
                    if ((indentationLength ??= GetIndentationLength()) == -1)
                        return true;

                    ReportDiagnostic();
                    return true;
                }

                switch (en.Current.Kind())
                {
                    case SyntaxKind.WhitespaceTrivia:
                        {
                            if ((indentationLength ??= GetIndentationLength()) == -1)
                                return true;

                            if (en.Current.Span.Length != indentationLength)
                            {
                                if (!en.MoveNext()
                                    || en.Current.IsEndOfLineTrivia())
                                {
                                    if (topBinaryExpression.FindTrivia(nodeOrToken.FullSpan.Start - 1).IsEndOfLineTrivia())
                                    {
                                        ReportDiagnostic();
                                        return true;
                                    }
                                }

                                break;
                            }

                            break;
                        }
                    case SyntaxKind.EndOfLineTrivia:
                        {
                            if (topBinaryExpression.FindTrivia(nodeOrToken.FullSpan.Start - 1).IsEndOfLineTrivia())
                            {
                                if ((indentationLength ??= GetIndentationLength()) == -1)
                                    return true;

                                ReportDiagnostic();
                                return true;
                            }

                            break;
                        }
                }

                return false;
            }

            int GetIndentationLength()
            {
                IndentationAnalysis indentationAnalysis = AnalyzeIndentation(topBinaryExpression);

                if (indentationAnalysis.IndentSize == 0)
                    return -1;

                SyntaxTriviaList leadingTrivia = topBinaryExpression.GetLeadingTrivia();

                if (leadingTrivia.Any()
                    && leadingTrivia.Last() == indentationAnalysis.Indentation
                    && AnalyzerOptions.AddNewLineAfterBinaryOperatorInsteadOfBeforeIt.IsEnabled(context, checkParent: true) == true)
                {
                    return indentationAnalysis.IndentationLength;
                }
                else
                {
                    return indentationAnalysis.IncreasedIndentationLength;
                }
            }

            void ReportDiagnostic()
            {
                DiagnosticHelpers.ReportDiagnostic(
                    context,
                    DiagnosticRules.FixFormattingOfBinaryExpressionChain,
                    topBinaryExpression,
                    AnalyzerOptions.AddNewLineAfterBinaryOperatorInsteadOfBeforeIt);
            }
        }
    }
}
