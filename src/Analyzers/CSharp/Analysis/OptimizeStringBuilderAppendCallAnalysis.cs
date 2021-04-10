// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    internal static class OptimizeStringBuilderAppendCallAnalysis
    {
        public static void Analyze(SyntaxNodeAnalysisContext context, in SimpleMemberInvocationExpressionInfo invocationInfo)
        {
            IMethodSymbol methodSymbol = context.SemanticModel.GetMethodSymbol(invocationInfo.InvocationExpression, context.CancellationToken);

            if (methodSymbol == null)
                return;

            if (methodSymbol.IsExtensionMethod)
                return;

            if (methodSymbol.ContainingType?.HasMetadataName(MetadataNames.System_Text_StringBuilder) != true)
                return;

            int parameterCount = methodSymbol.Parameters.Length;

            if (parameterCount == 0)
            {
                if (!methodSymbol.IsName("AppendLine"))
                    return;

                SimpleMemberInvocationExpressionInfo invocationInfo2 = SyntaxInfo.SimpleMemberInvocationExpressionInfo(invocationInfo.Expression);

                if (invocationInfo2.NameText != "Append")
                    return;

                if (invocationInfo2.Arguments.Count != 1)
                    return;

                IMethodSymbol methodInfo2 = context.SemanticModel.GetMethodSymbol(invocationInfo2.InvocationExpression, context.CancellationToken);

                if (methodInfo2?.IsStatic != false)
                    return;

                if (methodInfo2.ContainingType?.HasMetadataName(MetadataNames.System_Text_StringBuilder) != true)
                    return;

                if (!methodInfo2.HasSingleParameter(SpecialType.System_String))
                    return;

                ReportDiagnostic(invocationInfo.Name);
            }
            else if (parameterCount == 1)
            {
                if (!methodSymbol.IsName("Append", "AppendLine"))
                    return;

                ArgumentSyntax argument = invocationInfo.Arguments.SingleOrDefault(shouldThrow: false);

                if (argument == null)
                    return;

                ExpressionSyntax expression = argument.Expression;

                SyntaxKind expressionKind = expression.Kind();

                switch (expressionKind)
                {
                    case SyntaxKind.InterpolatedStringExpression:
                        {
                            ReportDiagnostic(argument);
                            return;
                        }
                    case SyntaxKind.AddExpression:
                        {
                            BinaryExpressionInfo binaryExpressionInfo = SyntaxInfo.BinaryExpressionInfo((BinaryExpressionSyntax)expression);

                            if (binaryExpressionInfo.Success
                                && binaryExpressionInfo.AsChain().Reverse().IsStringConcatenation(context.SemanticModel, context.CancellationToken)
                                && !context.SemanticModel.GetConstantValue(expression, context.CancellationToken).HasValue)
                            {
                                ReportDiagnostic(argument);
                            }

                            return;
                        }
                }

                if (expressionKind != SyntaxKind.InvocationExpression)
                    return;

                var invocationExpression = (InvocationExpressionSyntax)expression;

                SimpleMemberInvocationExpressionInfo invocationInfo2 = SyntaxInfo.SimpleMemberInvocationExpressionInfo(invocationExpression);

                if (!invocationInfo2.Success)
                    return;

                IMethodSymbol methodSymbol2 = context.SemanticModel.GetMethodSymbol(invocationInfo2.InvocationExpression, context.CancellationToken);

                if (methodSymbol2 == null)
                    return;

                if (!methodSymbol2.IsContainingType(SpecialType.System_String))
                    return;

                if (!methodSymbol2.IsReturnType(SpecialType.System_String))
                    return;

                switch (methodSymbol2.Name)
                {
                    case "Substring":
                        {
                            ImmutableArray<IParameterSymbol> parameters = methodSymbol2.Parameters;

                            switch (parameters.Length)
                            {
                                case 1:
                                    {
                                        if (parameters[0].Type.SpecialType == SpecialType.System_Int32
                                            && invocationInfo2.Expression.IsKind(SyntaxKind.IdentifierName)
                                            && context.SemanticModel.GetSymbol(invocationInfo2.Expression, context.CancellationToken).IsKind(SymbolKind.Field, SymbolKind.Local, SymbolKind.Parameter))
                                        {
                                            ReportDiagnostic(argument);
                                        }

                                        break;
                                    }
                                case 2:
                                    {
                                        if (parameters[0].Type.SpecialType == SpecialType.System_Int32
                                            && parameters[1].Type.SpecialType == SpecialType.System_Int32)
                                        {
                                            ReportDiagnostic(argument);
                                        }

                                        break;
                                    }
                            }

                            break;
                        }
                    case "Remove":
                        {
                            if (methodSymbol2.HasSingleParameter(SpecialType.System_Int32))
                                ReportDiagnostic(argument);

                            break;
                        }
                    case "Format":
                        {
                            ReportDiagnostic(argument);
                            break;
                        }
                    case "Join":
                        {
                            if (methodSymbol.ContainingType.ContainsMember<IMethodSymbol>("AppendJoin"))
                                ReportDiagnostic(argument);

                            break;
                        }
                }
            }

            void ReportDiagnostic(SyntaxNode node)
            {
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.OptimizeStringBuilderAppendCall, node, methodSymbol.Name);
            }
        }
    }
}