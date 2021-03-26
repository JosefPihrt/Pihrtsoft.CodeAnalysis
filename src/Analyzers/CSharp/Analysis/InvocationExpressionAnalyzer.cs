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
    public class InvocationExpressionAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticDescriptors.OptimizeLinqMethodCall,
                    DiagnosticDescriptors.UseElementAccess,
                    DiagnosticDescriptors.UseCountOrLengthPropertyInsteadOfAnyMethod,
                    DiagnosticDescriptors.RemoveRedundantToStringCall,
                    DiagnosticDescriptors.RemoveRedundantStringToCharArrayCall,
                    DiagnosticDescriptors.CombineEnumerableWhereMethodChain,
                    DiagnosticDescriptors.CombineEnumerableWhereMethodChainFadeOut,
                    DiagnosticDescriptors.UseRegexInstanceInsteadOfStaticMethod,
                    DiagnosticDescriptors.OptimizeStringBuilderAppendCall,
                    DiagnosticDescriptors.AvoidBoxingOfValueType,
                    DiagnosticDescriptors.CallThenByInsteadOfOrderBy,
                    DiagnosticDescriptors.UseMethodChaining,
                    DiagnosticDescriptors.AvoidNullReferenceException,
                    DiagnosticDescriptors.UseStringComparison,
                    DiagnosticDescriptors.UseNameOfOperator,
                    DiagnosticDescriptors.RemoveRedundantCast,
                    DiagnosticDescriptors.SimplifyLogicalNegation,
                    DiagnosticDescriptors.UseCoalesceExpression,
                    DiagnosticDescriptors.OptimizeMethodCall);
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
                                    if (DiagnosticDescriptors.UseCountOrLengthPropertyInsteadOfAnyMethod.IsEffective(context))
                                        UseCountOrLengthPropertyInsteadOfAnyMethodAnalysis.Analyze(context, invocationInfo);

                                    if (DiagnosticDescriptors.OptimizeLinqMethodCall.IsEffective(context))
                                    {
                                        OptimizeLinqMethodCallAnalysis.AnalyzeWhere(context, invocationInfo);
                                        OptimizeLinqMethodCallAnalysis.AnalyzeAny(context, invocationInfo);
                                    }

                                    break;
                                }
                            case "Cast":
                                {
                                    if (DiagnosticDescriptors.OptimizeLinqMethodCall.IsEffective(context))
                                        OptimizeLinqMethodCallAnalysis.AnalyzeWhereAndCast(context, invocationInfo);

                                    if (DiagnosticDescriptors.RemoveRedundantCast.IsEffective(context))
                                        RemoveRedundantCastAnalyzer.Analyze(context, invocationInfo);

                                    break;
                                }
                            case "Count":
                                {
                                    if (DiagnosticDescriptors.OptimizeLinqMethodCall.IsEffective(context)
                                        && !OptimizeLinqMethodCallAnalysis.AnalyzeSelectManyAndCount(context, invocationInfo))
                                    {
                                        OptimizeLinqMethodCallAnalysis.AnalyzeCount(context, invocationInfo);
                                        OptimizeLinqMethodCallAnalysis.AnalyzeWhere(context, invocationInfo);
                                    }

                                    break;
                                }
                            case "First":
                                {
                                    if (DiagnosticDescriptors.OptimizeLinqMethodCall.IsEffective(context))
                                    {
                                        if (CanUseElementAccess(context, invocationInfo)
                                            && UseElementAccessAnalysis.IsFixableFirst(invocationInfo, context.SemanticModel, context.CancellationToken))
                                        {
                                            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.UseElementAccess, Location.Create(invocation.SyntaxTree, TextSpan.FromBounds(invocationInfo.Name.SpanStart, invocationInfo.ArgumentList.Span.End)));
                                        }

                                        OptimizeLinqMethodCallAnalysis.AnalyzeWhere(context, invocationInfo);
                                        OptimizeLinqMethodCallAnalysis.AnalyzeFirst(context, invocationInfo);
                                    }

                                    break;
                                }
                            case "Max":
                            case "Min":
                                {
                                    if (DiagnosticDescriptors.OptimizeLinqMethodCall.IsEffective(context))
                                        OptimizeLinqMethodCallAnalysis.AnalyzeSelectAndMinOrMax(context, invocationInfo);

                                    break;
                                }
                            case "Reverse":
                                {
                                    if (DiagnosticDescriptors.OptimizeLinqMethodCall.IsEffective(context))
                                        OptimizeLinqMethodCallAnalysis.AnalyzeOrderByAndReverse(context, invocationInfo);

                                    break;
                                }
                            case "ToString":
                                {
                                    if (DiagnosticDescriptors.RemoveRedundantToStringCall.IsEffective(context))
                                        RemoveRedundantToStringCallAnalysis.Analyze(context, invocationInfo);

                                    if (DiagnosticDescriptors.UseNameOfOperator.IsEffective(context)
                                        && ((CSharpCompilation)context.Compilation).LanguageVersion >= LanguageVersion.CSharp6)
                                    {
                                        UseNameOfOperatorAnalyzer.Analyze(context, invocationInfo);
                                    }

                                    break;
                                }
                            case "ToList":
                                {
                                    if (DiagnosticDescriptors.OptimizeLinqMethodCall.IsEffective(context))
                                        OptimizeLinqMethodCallAnalysis.AnalyzeSelectAndToList(context, invocationInfo);

                                    break;
                                }
                            case "ToLower":
                            case "ToLowerInvariant":
                            case "ToUpper":
                            case "ToUpperInvariant":
                                {
                                    if (DiagnosticDescriptors.UseStringComparison.IsEffective(context))
                                        UseStringComparisonAnalysis.Analyze(context, invocationInfo);

                                    break;
                                }
                            case "FirstOrDefault":
                                {
                                    if (DiagnosticDescriptors.OptimizeLinqMethodCall.IsEffective(context))
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
                                    if (DiagnosticDescriptors.OptimizeLinqMethodCall.IsEffective(context))
                                        OptimizeLinqMethodCallAnalysis.AnalyzeWhere(context, invocationInfo);

                                    break;
                                }
                            case "OfType":
                                {
                                    if (DiagnosticDescriptors.OptimizeLinqMethodCall.IsEffective(context)
                                        && !invocation.SpanContainsDirectives())
                                    {
                                        OptimizeLinqMethodCallAnalysis.AnalyzeOfType(context, invocationInfo);
                                    }

                                    break;
                                }
                            case "ToCharArray":
                                {
                                    if (DiagnosticDescriptors.RemoveRedundantStringToCharArrayCall.IsEffective(context))
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
                                    if (DiagnosticDescriptors.SimplifyLogicalNegation.IsEffective(context))
                                        SimplifyLogicalNegationAnalyzer.Analyze(context, invocationInfo);

                                    if (DiagnosticDescriptors.OptimizeLinqMethodCall.IsEffective(context)
                                        && !invocation.SpanContainsDirectives())
                                    {
                                        OptimizeLinqMethodCallAnalysis.AnalyzeWhereAndAny(context, invocationInfo);
                                    }

                                    break;
                                }
                            case "ContainsKey":
                                {
                                    if (DiagnosticDescriptors.OptimizeMethodCall.IsEffective(context))
                                        OptimizeMethodCallAnalysis.OptimizeDictionaryContainsKey(context, invocationInfo);

                                    break;
                                }
                            case "ElementAt":
                                {
                                    if (DiagnosticDescriptors.OptimizeLinqMethodCall.IsEffective(context)
                                        && CanUseElementAccess(context, invocationInfo)
                                        && UseElementAccessAnalysis.IsFixableElementAt(invocationInfo, context.SemanticModel, context.CancellationToken))
                                    {
                                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.UseElementAccess, Location.Create(invocation.SyntaxTree, TextSpan.FromBounds(invocationInfo.Name.SpanStart, invocationInfo.ArgumentList.Span.End)));
                                    }

                                    break;
                                }
                            case "FirstOrDefault":
                                {
                                    if (DiagnosticDescriptors.OptimizeLinqMethodCall.IsEffective(context))
                                        OptimizeLinqMethodCallAnalysis.AnalyzeFirstOrDefault(context, invocationInfo);

                                    break;
                                }
                            case "GetValueOrDefault":
                                {
                                    if (DiagnosticDescriptors.UseCoalesceExpression.IsEffective(context)
                                        && invocationInfo.Name.IsKind(SyntaxKind.IdentifierName)
                                        && !invocation.IsParentKind(SyntaxKind.InvocationExpression, SyntaxKind.SimpleMemberAccessExpression, SyntaxKind.ElementAccessExpression)
                                        && context.SemanticModel
                                            .GetMethodSymbol(invocationInfo.InvocationExpression, context.CancellationToken)?
                                            .ContainingType
                                            .OriginalDefinition
                                            .SpecialType == SpecialType.System_Nullable_T)
                                    {
                                        DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.UseCoalesceExpression, invocationInfo.Name);
                                    }

                                    break;
                                }
                            case "Where":
                                {
                                    if (DiagnosticDescriptors.CombineEnumerableWhereMethodChain.IsEffective(context))
                                        CombineEnumerableWhereMethodChainAnalysis.Analyze(context, invocationInfo);

                                    if (DiagnosticDescriptors.OptimizeLinqMethodCall.IsEffective(context))
                                        OptimizeLinqMethodCallAnalysis.AnalyzeOrderByAndWhere(context, invocationInfo);

                                    break;
                                }
                            case "Select":
                                {
                                    if (DiagnosticDescriptors.OptimizeLinqMethodCall.IsEffective(context))
                                        CallCastInsteadOfSelectAnalysis.Analyze(context, invocationInfo);

                                    break;
                                }
                            case "OrderBy":
                                {
                                    if (DiagnosticDescriptors.CallThenByInsteadOfOrderBy.IsEffective(context))
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
                                    if (DiagnosticDescriptors.UseRegexInstanceInsteadOfStaticMethod.IsEffective(context)
                                        && !invocation.SpanContainsDirectives())
                                    {
                                        UseRegexInstanceInsteadOfStaticMethodAnalysis.Analyze(context, invocationInfo);
                                    }

                                    break;
                                }
                            case "Select":
                                {
                                    if (DiagnosticDescriptors.OptimizeLinqMethodCall.IsEffective(context))
                                        CallCastInsteadOfSelectAnalysis.Analyze(context, invocationInfo);

                                    break;
                                }
                            case "OrderBy":
                                {
                                    if (DiagnosticDescriptors.CallThenByInsteadOfOrderBy.IsEffective(context))
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
                                    if (DiagnosticDescriptors.UseRegexInstanceInsteadOfStaticMethod.IsEffective(context)
                                        && !invocation.SpanContainsDirectives())
                                    {
                                        UseRegexInstanceInsteadOfStaticMethodAnalysis.Analyze(context, invocationInfo);
                                    }

                                    break;
                                }
                            case "OrderBy":
                                {
                                    if (DiagnosticDescriptors.CallThenByInsteadOfOrderBy.IsEffective(context))
                                        CallThenByInsteadOfOrderByAnalysis.Analyze(context, invocationInfo);

                                    break;
                                }
                            case "Compare":
                                {
                                    if (DiagnosticDescriptors.OptimizeMethodCall.IsEffective(context))
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
                                    if (DiagnosticDescriptors.UseRegexInstanceInsteadOfStaticMethod.IsEffective(context)
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
                        if (DiagnosticDescriptors.AvoidNullReferenceException.IsEffective(context))
                            AvoidNullReferenceExceptionAnalyzer.Analyze(context, invocationInfo);

                        break;
                    }
                case "Append":
                case "AppendLine":
                case "AppendFormat":
                case "Insert":
                    {
                        if (DiagnosticDescriptors.OptimizeStringBuilderAppendCall.IsEffective(context))
                            OptimizeStringBuilderAppendCallAnalysis.Analyze(context, invocationInfo);

                        if (DiagnosticDescriptors.AvoidBoxingOfValueType.IsEffective(context))
                            AvoidBoxingOfValueTypeAnalysis.Analyze(context, invocationInfo);

                        break;
                    }
                case "Assert":
                    {
                        if (DiagnosticDescriptors.OptimizeMethodCall.IsEffective(context)
                            && (argumentCount >= 1 && argumentCount <= 3))
                        {
                            OptimizeMethodCallAnalysis.OptimizeDebugAssert(context, invocationInfo);
                        }

                        break;
                    }
                case "Join":
                    {
                        if (DiagnosticDescriptors.OptimizeMethodCall.IsEffective(context)
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
                            && DiagnosticDescriptors.AvoidNullReferenceException.IsEffective(context))
                        {
                            AvoidNullReferenceExceptionAnalyzer.Analyze(context, invocationInfo);
                        }

                        break;
                    }
            }

            if (DiagnosticDescriptors.UseMethodChaining.IsEffective(context)
                && UseMethodChainingAnalysis.IsFixable(invocationInfo, context.SemanticModel, context.CancellationToken))
            {
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.UseMethodChaining, invocationInfo.InvocationExpression);
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
