// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp;
using Roslynator.CSharp.Analysis.UseMethodChaining;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class InvocationExpressionAnalyzer : BaseDiagnosticAnalyzer
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
                        DiagnosticRules.OptimizeLinqMethodCall,
                        DiagnosticRules.UseElementAccess,
                        DiagnosticRules.UseCountOrLengthPropertyInsteadOfAnyMethod,
                        DiagnosticRules.RemoveRedundantToStringCall,
                        DiagnosticRules.RemoveRedundantStringToCharArrayCall,
                        DiagnosticRules.CombineEnumerableWhereMethodChain,
                        DiagnosticRules.CombineEnumerableWhereMethodChainFadeOut,
                        DiagnosticRules.UseRegexInstanceInsteadOfStaticMethod,
                        DiagnosticRules.OptimizeStringBuilderAppendCall,
                        DiagnosticRules.AvoidBoxingOfValueType,
                        DiagnosticRules.CallThenByInsteadOfOrderBy,
                        DiagnosticRules.UseMethodChaining,
                        DiagnosticRules.AvoidNullReferenceException,
                        DiagnosticRules.UseStringComparison,
                        DiagnosticRules.UseNameOfOperator,
                        DiagnosticRules.RemoveRedundantCast,
                        DiagnosticRules.SimplifyLogicalNegation,
                        DiagnosticRules.UseCoalesceExpression,
                        DiagnosticRules.OptimizeMethodCall,
                        CommonDiagnosticRules.AnalyzerOptionIsObsolete);
                }

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeInvocationExpression(f), SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            if (invocation.ContainsDiagnostics)
                return;

            SimpleMemberInvocationExpressionInfo invocationInfo = SyntaxInfo.SimpleMemberInvocationExpressionInfo(invocation);

            if (!invocationInfo.Success)
                return;

            string methodName = invocationInfo.NameText;

            int argumentCount = invocationInfo.Arguments.Count;

            switch (argumentCount)
            {
                case 0:
                    {
                        switch (methodName)
                        {
                            case "Any":
                                {
                                    if (DiagnosticRules.UseCountOrLengthPropertyInsteadOfAnyMethod.IsEffective(context))
                                        UseCountOrLengthPropertyInsteadOfAnyMethodAnalysis.Analyze(context, invocationInfo);

                                    if (DiagnosticRules.OptimizeLinqMethodCall.IsEffective(context))
                                    {
                                        OptimizeLinqMethodCallAnalysis.AnalyzeWhere(context, invocationInfo);
                                        OptimizeLinqMethodCallAnalysis.AnalyzeAny(context, invocationInfo);
                                    }

                                    break;
                                }
                            case "Cast":
                                {
                                    if (DiagnosticRules.OptimizeLinqMethodCall.IsEffective(context))
                                        OptimizeLinqMethodCallAnalysis.AnalyzeWhereAndCast(context, invocationInfo);

                                    if (DiagnosticRules.RemoveRedundantCast.IsEffective(context))
                                        RemoveRedundantCastAnalyzer.Analyze(context, invocationInfo);

                                    break;
                                }
                            case "Count":
                                {
                                    if (DiagnosticRules.OptimizeLinqMethodCall.IsEffective(context)
                                        && !OptimizeLinqMethodCallAnalysis.AnalyzeSelectManyAndCount(context, invocationInfo))
                                    {
                                        OptimizeLinqMethodCallAnalysis.AnalyzeCount(context, invocationInfo);
                                        OptimizeLinqMethodCallAnalysis.AnalyzeWhere(context, invocationInfo);
                                    }

                                    break;
                                }
                            case "First":
                                {
                                    if (DiagnosticRules.OptimizeLinqMethodCall.IsEffective(context))
                                    {
                                        if (CanUseElementAccess(context, invocationInfo)
                                            && UseElementAccessAnalysis.IsFixableFirst(invocationInfo, context.SemanticModel, context.CancellationToken))
                                        {
                                            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UseElementAccess, Location.Create(invocation.SyntaxTree, TextSpan.FromBounds(invocationInfo.Name.SpanStart, invocationInfo.ArgumentList.Span.End)), AnalyzerOptions.DoNotUseElementAccessWhenExpressionIsInvocation);
                                        }

                                        OptimizeLinqMethodCallAnalysis.AnalyzeWhere(context, invocationInfo);
                                        OptimizeLinqMethodCallAnalysis.AnalyzeFirst(context, invocationInfo);
                                    }

                                    break;
                                }
                            case "Max":
                            case "Min":
                                {
                                    if (DiagnosticRules.OptimizeLinqMethodCall.IsEffective(context))
                                        OptimizeLinqMethodCallAnalysis.AnalyzeSelectAndMinOrMax(context, invocationInfo);

                                    break;
                                }
                            case "Reverse":
                                {
                                    if (DiagnosticRules.OptimizeLinqMethodCall.IsEffective(context))
                                        OptimizeLinqMethodCallAnalysis.AnalyzeOrderByAndReverse(context, invocationInfo);

                                    break;
                                }
                            case "ToString":
                                {
                                    if (DiagnosticRules.RemoveRedundantToStringCall.IsEffective(context))
                                        RemoveRedundantToStringCallAnalysis.Analyze(context, invocationInfo);

                                    if (DiagnosticRules.UseNameOfOperator.IsEffective(context)
                                        && ((CSharpCompilation)context.Compilation).LanguageVersion >= LanguageVersion.CSharp6)
                                    {
                                        UseNameOfOperatorAnalyzer.Analyze(context, invocationInfo);
                                    }

                                    break;
                                }
                            case "ToList":
                                {
                                    if (DiagnosticRules.OptimizeLinqMethodCall.IsEffective(context))
                                        OptimizeLinqMethodCallAnalysis.AnalyzeSelectAndToList(context, invocationInfo);

                                    break;
                                }
                            case "ToLower":
                            case "ToLowerInvariant":
                            case "ToUpper":
                            case "ToUpperInvariant":
                                {
                                    if (DiagnosticRules.UseStringComparison.IsEffective(context))
                                        UseStringComparisonAnalysis.Analyze(context, invocationInfo);

                                    break;
                                }
                            case "FirstOrDefault":
                                {
                                    if (DiagnosticRules.OptimizeLinqMethodCall.IsEffective(context))
                                    {
                                        OptimizeLinqMethodCallAnalysis.AnalyzeWhere(context, invocationInfo);
                                        OptimizeLinqMethodCallAnalysis.AnalyzeFirstOrDefault(context, invocationInfo);
                                    }

                                    break;
                                }
                            case "Last":
                            case "LastOrDefault":
                            case "LongCount":
                            case "Single":
                            case "SingleOrDefault":
                                {
                                    if (DiagnosticRules.OptimizeLinqMethodCall.IsEffective(context))
                                        OptimizeLinqMethodCallAnalysis.AnalyzeWhere(context, invocationInfo);

                                    break;
                                }
                            case "OfType":
                                {
                                    if (DiagnosticRules.OptimizeLinqMethodCall.IsEffective(context)
                                        && !invocation.SpanContainsDirectives())
                                    {
                                        OptimizeLinqMethodCallAnalysis.AnalyzeOfType(context, invocationInfo);
                                    }

                                    break;
                                }
                            case "ToCharArray":
                                {
                                    if (DiagnosticRules.RemoveRedundantStringToCharArrayCall.IsEffective(context))
                                        RemoveRedundantStringToCharArrayCallAnalysis.Analyze(context, invocationInfo);

                                    break;
                                }
                        }

                        break;
                    }
                case 1:
                    {
                        switch (methodName)
                        {
                            case "All":
                            case "Any":
                                {
                                    if (DiagnosticRules.SimplifyLogicalNegation.IsEffective(context))
                                        SimplifyLogicalNegationAnalyzer.Analyze(context, invocationInfo);

                                    if (DiagnosticRules.OptimizeLinqMethodCall.IsEffective(context)
                                        && !invocation.SpanContainsDirectives())
                                    {
                                        OptimizeLinqMethodCallAnalysis.AnalyzeWhereAndAny(context, invocationInfo);
                                    }

                                    break;
                                }
                            case "ContainsKey":
                                {
                                    if (DiagnosticRules.OptimizeMethodCall.IsEffective(context))
                                        OptimizeMethodCallAnalysis.OptimizeDictionaryContainsKey(context, invocationInfo);

                                    break;
                                }
                            case "ElementAt":
                                {
                                    if (DiagnosticRules.OptimizeLinqMethodCall.IsEffective(context)
                                        && CanUseElementAccess(context, invocationInfo)
                                        && UseElementAccessAnalysis.IsFixableElementAt(invocationInfo, context.SemanticModel, context.CancellationToken))
                                    {
                                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UseElementAccess, Location.Create(invocation.SyntaxTree, TextSpan.FromBounds(invocationInfo.Name.SpanStart, invocationInfo.ArgumentList.Span.End)), AnalyzerOptions.DoNotUseElementAccessWhenExpressionIsInvocation);
                                    }

                                    break;
                                }
                            case "FirstOrDefault":
                                {
                                    if (DiagnosticRules.OptimizeLinqMethodCall.IsEffective(context))
                                        OptimizeLinqMethodCallAnalysis.AnalyzeFirstOrDefault(context, invocationInfo);

                                    break;
                                }
                            case "GetValueOrDefault":
                                {
                                    if (DiagnosticRules.UseCoalesceExpression.IsEffective(context)
                                        && invocationInfo.Name.IsKind(SyntaxKind.IdentifierName)
                                        && !invocation.IsParentKind(SyntaxKind.InvocationExpression, SyntaxKind.SimpleMemberAccessExpression, SyntaxKind.ElementAccessExpression)
                                        && context.SemanticModel
                                            .GetMethodSymbol(invocationInfo.InvocationExpression, context.CancellationToken)?
                                            .ContainingType
                                            .OriginalDefinition
                                            .SpecialType == SpecialType.System_Nullable_T)
                                    {
                                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UseCoalesceExpression, invocationInfo.Name);
                                    }

                                    break;
                                }
                            case "Where":
                                {
                                    if (DiagnosticRules.CombineEnumerableWhereMethodChain.IsEffective(context))
                                        CombineEnumerableWhereMethodChainAnalysis.Analyze(context, invocationInfo);

                                    if (DiagnosticRules.OptimizeLinqMethodCall.IsEffective(context))
                                        OptimizeLinqMethodCallAnalysis.AnalyzeOrderByAndWhere(context, invocationInfo);

                                    break;
                                }
                            case "Select":
                                {
                                    if (DiagnosticRules.OptimizeLinqMethodCall.IsEffective(context))
                                        CallCastInsteadOfSelectAnalysis.Analyze(context, invocationInfo);

                                    break;
                                }
                            case "OrderBy":
                                {
                                    if (DiagnosticRules.CallThenByInsteadOfOrderBy.IsEffective(context))
                                        CallThenByInsteadOfOrderByAnalysis.Analyze(context, invocationInfo);

                                    break;
                                }
                        }

                        break;
                    }
                case 2:
                    {
                        switch (invocationInfo.NameText)
                        {
                            case "IsMatch":
                            case "Match":
                            case "Matches":
                            case "Split":
                                {
                                    if (DiagnosticRules.UseRegexInstanceInsteadOfStaticMethod.IsEffective(context)
                                        && !invocation.SpanContainsDirectives())
                                    {
                                        UseRegexInstanceInsteadOfStaticMethodAnalysis.Analyze(context, invocationInfo);
                                    }

                                    break;
                                }
                            case "Select":
                                {
                                    if (DiagnosticRules.OptimizeLinqMethodCall.IsEffective(context))
                                        CallCastInsteadOfSelectAnalysis.Analyze(context, invocationInfo);

                                    break;
                                }
                            case "OrderBy":
                                {
                                    if (DiagnosticRules.CallThenByInsteadOfOrderBy.IsEffective(context))
                                        CallThenByInsteadOfOrderByAnalysis.Analyze(context, invocationInfo);

                                    break;
                                }
                        }

                        break;
                    }
                case 3:
                    {
                        switch (invocationInfo.NameText)
                        {
                            case "IsMatch":
                            case "Match":
                            case "Matches":
                            case "Split":
                            case "Replace":
                                {
                                    if (DiagnosticRules.UseRegexInstanceInsteadOfStaticMethod.IsEffective(context)
                                        && !invocation.SpanContainsDirectives())
                                    {
                                        UseRegexInstanceInsteadOfStaticMethodAnalysis.Analyze(context, invocationInfo);
                                    }

                                    break;
                                }
                            case "OrderBy":
                                {
                                    if (DiagnosticRules.CallThenByInsteadOfOrderBy.IsEffective(context))
                                        CallThenByInsteadOfOrderByAnalysis.Analyze(context, invocationInfo);

                                    break;
                                }
                            case "Compare":
                                {
                                    if (DiagnosticRules.OptimizeMethodCall.IsEffective(context))
                                        OptimizeMethodCallAnalysis.OptimizeStringCompare(context, invocationInfo);

                                    break;
                                }
                        }

                        break;
                    }
                case 4:
                    {
                        switch (invocationInfo.NameText)
                        {
                            case "Replace":
                                {
                                    if (DiagnosticRules.UseRegexInstanceInsteadOfStaticMethod.IsEffective(context)
                                        && !invocation.SpanContainsDirectives())
                                    {
                                        UseRegexInstanceInsteadOfStaticMethodAnalysis.Analyze(context, invocationInfo);
                                    }

                                    break;
                                }
                        }

                        break;
                    }
            }

            switch (methodName)
            {
                case "ElementAtOrDefault":
                case "FirstOrDefault":
                case "LastOrDefault":
                case "SingleOrDefault":
                    {
                        if (DiagnosticRules.AvoidNullReferenceException.IsEffective(context))
                            AvoidNullReferenceExceptionAnalyzer.Analyze(context, invocationInfo);

                        break;
                    }
                case "Append":
                case "AppendLine":
                case "AppendFormat":
                case "Insert":
                    {
                        if (DiagnosticRules.OptimizeStringBuilderAppendCall.IsEffective(context))
                            OptimizeStringBuilderAppendCallAnalysis.Analyze(context, invocationInfo);

                        if (DiagnosticRules.AvoidBoxingOfValueType.IsEffective(context))
                            AvoidBoxingOfValueTypeAnalysis.Analyze(context, invocationInfo);

                        break;
                    }
                case "Assert":
                    {
                        if (DiagnosticRules.OptimizeMethodCall.IsEffective(context)
                            && (argumentCount >= 1 && argumentCount <= 3))
                        {
                            OptimizeMethodCallAnalysis.OptimizeDebugAssert(context, invocationInfo);
                        }

                        break;
                    }
                case "Join":
                    {
                        if (DiagnosticRules.OptimizeMethodCall.IsEffective(context)
                            && argumentCount >= 2)
                        {
                            OptimizeMethodCallAnalysis.OptimizeStringJoin(context, invocationInfo);
                        }

                        break;
                    }
                default:
                    {
                        if (methodName.Length > "OrDefault".Length
                            && methodName.EndsWith("OrDefault", StringComparison.Ordinal)
                            && DiagnosticRules.AvoidNullReferenceException.IsEffective(context))
                        {
                            AvoidNullReferenceExceptionAnalyzer.Analyze(context, invocationInfo);
                        }

                        break;
                    }
            }

            if (DiagnosticRules.UseMethodChaining.IsEffective(context)
                && UseMethodChainingAnalysis.IsFixable(invocationInfo, context.SemanticModel, context.CancellationToken))
            {
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UseMethodChaining, invocationInfo.InvocationExpression);
            }
        }

        public static bool CanUseElementAccess(SyntaxNodeAnalysisContext context, in SimpleMemberInvocationExpressionInfo invocationInfo)
        {
            return !invocationInfo.Expression.IsKind(SyntaxKind.ElementAccessExpression)
                && (!invocationInfo.Expression.IsKind(SyntaxKind.InvocationExpression)
                    || !AnalyzerOptions.DoNotUseElementAccessWhenExpressionIsInvocation.IsEnabled(context));
        }
    }
}
