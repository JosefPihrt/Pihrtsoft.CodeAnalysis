// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp;
using static Roslynator.CSharp.SyntaxTriviaAnalysis;

namespace Roslynator.Formatting.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class FixFormattingOfCallChainAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.FixFormattingOfCallChain);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeExpression(f), SyntaxKind.InvocationExpression);
            context.RegisterSyntaxNodeAction(f => AnalyzeExpression(f), SyntaxKind.ElementAccessExpression);
            context.RegisterSyntaxNodeAction(f => AnalyzeExpression(f), SyntaxKind.ConditionalAccessExpression);
        }

        private static void AnalyzeExpression(SyntaxNodeAnalysisContext context)
        {
            var expression = (ExpressionSyntax)context.Node;

            if (expression.IsParentKind(
                SyntaxKind.ConditionalAccessExpression,
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxKind.ElementAccessExpression,
                SyntaxKind.MemberBindingExpression,
                SyntaxKind.InvocationExpression))
            {
                return;
            }

            MethodChain.Enumerator en = new MethodChain(expression).GetEnumerator();

            if (!en.MoveNext())
                return;

            TextLineCollection lines = null;
            int startLine = -1;
            IndentationAnalysis indentationAnalysis = default;

            do
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                SyntaxKind kind = en.Current.Kind();

                if (kind == SyntaxKind.SimpleMemberAccessExpression)
                {
                    var memberAccess = (MemberAccessExpressionSyntax)en.Current;

                    if (AnalyzeToken(memberAccess.OperatorToken))
                        return;
                }
                else if (en.Current.Kind() == SyntaxKind.MemberBindingExpression)
                {
                    var memberBinding = (MemberBindingExpressionSyntax)en.Current;

                    if (AnalyzeToken(memberBinding.OperatorToken))
                        return;
                }

            } while (en.MoveNext());

            bool AnalyzeToken(SyntaxToken token)
            {
                SyntaxTriviaList.Reversed.Enumerator en = token.LeadingTrivia.Reverse().GetEnumerator();

                if (!en.MoveNext())
                {
                    if (lines == null)
                    {
                        lines = expression.SyntaxTree.GetText().Lines;
                        startLine = lines.IndexOf(expression.SpanStart);
                    }

                    int endLine = lines.IndexOf(token.SpanStart);

                    if (startLine != endLine)
                        ReportDiagnostic();

                    return true;
                }

                switch (en.Current.Kind())
                {
                    case SyntaxKind.WhitespaceTrivia:
                        {
                            if (indentationAnalysis.IsDefault)
                                indentationAnalysis = AnalyzeIndentation(expression);

                            if (en.Current.Span.Length != indentationAnalysis.IncreasedIndentationLength)
                            {
                                if (!en.MoveNext()
                                    || en.Current.IsEndOfLineTrivia())
                                {
                                    if (expression.FindTrivia(token.FullSpan.Start - 1).IsEndOfLineTrivia())
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
                            if (expression.FindTrivia(token.FullSpan.Start - 1).IsEndOfLineTrivia())
                            {
                                ReportDiagnostic();
                                return true;
                            }

                            break;
                        }
                }

                return false;
            }

            void ReportDiagnostic()
            {
                DiagnosticHelpers.ReportDiagnostic(
                    context,
                    DiagnosticRules.FixFormattingOfCallChain,
                    expression);
            }
        }
    }
}
