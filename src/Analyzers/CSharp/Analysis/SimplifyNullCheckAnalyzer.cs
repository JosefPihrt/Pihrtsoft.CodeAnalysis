// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SimplifyNullCheckAnalyzer : BaseDiagnosticAnalyzer
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
                        DiagnosticRules.UseCoalesceExpressionInsteadOfConditionalExpression,
                        DiagnosticRules.UseConditionalAccessInsteadOfConditionalExpression);
                }

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeConditionalExpression(f), SyntaxKind.ConditionalExpression);
        }

        private static void AnalyzeConditionalExpression(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.SpanContainsDirectives())
                return;

            var conditionalExpression = (ConditionalExpressionSyntax)context.Node;

            ConditionalExpressionInfo conditionalExpressionInfo = SyntaxInfo.ConditionalExpressionInfo(conditionalExpression);

            if (!conditionalExpressionInfo.Success)
                return;

            SemanticModel semanticModel = context.SemanticModel;
            CancellationToken cancellationToken = context.CancellationToken;

            NullCheckExpressionInfo nullCheck = SyntaxInfo.NullCheckExpressionInfo(conditionalExpressionInfo.Condition, semanticModel: semanticModel, cancellationToken: cancellationToken);

            if (!nullCheck.Success)
                return;

            ExpressionSyntax whenNotNull = (nullCheck.IsCheckingNotNull) ? conditionalExpressionInfo.WhenTrue : conditionalExpressionInfo.WhenFalse;

            ExpressionSyntax whenNull = (nullCheck.IsCheckingNotNull) ? conditionalExpressionInfo.WhenFalse : conditionalExpressionInfo.WhenTrue;

            if (CSharpFactory.AreEquivalent(nullCheck.Expression, whenNotNull))
            {
                if (DiagnosticRules.UseCoalesceExpressionInsteadOfConditionalExpression.IsEffective(context)
                    && semanticModel
                        .GetTypeSymbol(nullCheck.Expression, cancellationToken)?
                        .IsReferenceTypeOrNullableType() == true)
                {
                    DiagnosticHelpers.ReportDiagnostic(
                        context,
                        DiagnosticRules.UseCoalesceExpressionInsteadOfConditionalExpression,
                        conditionalExpression);
                }
            }
            else if (whenNotNull.IsKind(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxKind.ElementAccessExpression,
                SyntaxKind.ConditionalAccessExpression,
                SyntaxKind.InvocationExpression))
            {
                ExpressionSyntax expression = UseConditionalAccessAnalyzer.FindExpressionThatCanBeConditionallyAccessed(
                    nullCheck.Expression,
                    whenNotNull,
                    semanticModel,
                    cancellationToken);

                if (expression == null)
                    return;

                ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(nullCheck.Expression, cancellationToken);

                if (typeSymbol == null)
                    return;

                if (typeSymbol.IsReferenceType)
                {
                    Analyze(context, conditionalExpressionInfo, whenNull, whenNotNull, semanticModel, cancellationToken);
                }
                else if (typeSymbol.IsNullableType())
                {
                    if (expression.IsParentKind(SyntaxKind.SimpleMemberAccessExpression))
                    {
                        var memberAccessExpression = (MemberAccessExpressionSyntax)expression.Parent;

                        if (!memberAccessExpression.IsParentKind(SyntaxKind.InvocationExpression)
                            && (memberAccessExpression.Name as IdentifierNameSyntax)?.Identifier.ValueText == "Value")
                        {
                            if (memberAccessExpression == whenNotNull)
                            {
                                if (DiagnosticRules.UseCoalesceExpressionInsteadOfConditionalExpression.IsEffective(context))
                                {
                                    DiagnosticHelpers.ReportDiagnostic(
                                        context,
                                        DiagnosticRules.UseCoalesceExpressionInsteadOfConditionalExpression,
                                        conditionalExpression);
                                }
                            }
                            else
                            {
                                Analyze(context, conditionalExpressionInfo, whenNull, whenNotNull, semanticModel, cancellationToken);
                            }
                        }
                    }
                }
            }
            else if (DiagnosticRules.UseConditionalAccessInsteadOfConditionalExpression.IsEffective(context)
                && ((CSharpCompilation)context.Compilation).LanguageVersion >= LanguageVersion.CSharp6
                && whenNotNull.IsKind(SyntaxKind.CastExpression)
                && whenNull.IsKind(SyntaxKind.NullLiteralExpression, SyntaxKind.DefaultLiteralExpression))
            {
                var castExpression = (CastExpressionSyntax)whenNotNull;

                if (castExpression.Type.IsKind(SyntaxKind.NullableType)
                    && castExpression.Expression.IsKind(SyntaxKind.InvocationExpression, SyntaxKind.SimpleMemberAccessExpression, SyntaxKind.ElementAccessExpression))
                {
                    ExpressionSyntax expression = UseConditionalAccessAnalyzer.FindExpressionThatCanBeConditionallyAccessed(
                        nullCheck.Expression,
                        castExpression.Expression,
                        isNullable: true,
                        semanticModel,
                        cancellationToken);

                    if (expression != null)
                    {
                        ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(nullCheck.Expression, cancellationToken);

                        if (typeSymbol?.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T
                            && !conditionalExpression.IsInExpressionTree(semanticModel, cancellationToken))
                        {
                            DiagnosticHelpers.ReportDiagnostic(
                                context,
                                DiagnosticRules.UseConditionalAccessInsteadOfConditionalExpression,
                                conditionalExpression);
                        }
                    }
                }
            }
        }

        private static void Analyze(
            SyntaxNodeAnalysisContext context,
            in ConditionalExpressionInfo conditionalExpressionInfo,
            ExpressionSyntax whenNull,
            ExpressionSyntax whenNotNull,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            if (DiagnosticRules.UseConditionalAccessInsteadOfConditionalExpression.IsEffective(context)
                && ((CSharpCompilation)context.Compilation).LanguageVersion >= LanguageVersion.CSharp6)
            {
                ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(whenNotNull, cancellationToken);

                if (typeSymbol?.IsErrorType() == false
                    && (typeSymbol.IsReferenceType || typeSymbol.IsValueType)
                    && (semanticModel.IsDefaultValue(typeSymbol, whenNull, cancellationToken)
                        || IsDefaultOfNullableStruct(typeSymbol, whenNull, semanticModel, cancellationToken))
                    && !CSharpUtility.ContainsOutArgumentWithLocalOrParameter(whenNotNull, semanticModel, cancellationToken)
                    && !conditionalExpressionInfo.ConditionalExpression.IsInExpressionTree(semanticModel, cancellationToken))
                {
                    DiagnosticHelpers.ReportDiagnostic(
                        context,
                        DiagnosticRules.UseConditionalAccessInsteadOfConditionalExpression,
                        conditionalExpressionInfo.ConditionalExpression);
                }
            }
        }

        private static bool IsDefaultOfNullableStruct(
            ITypeSymbol typeSymbol,
            ExpressionSyntax whenNull,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            if (typeSymbol.IsValueType
                && !typeSymbol.IsNullableType()
                && whenNull.IsKind(SyntaxKind.DefaultExpression))
            {
                var defaultExpression = (DefaultExpressionSyntax)whenNull;

                TypeSyntax type = defaultExpression.Type;

                if (type.IsKind(SyntaxKind.NullableType)
                    && semanticModel.GetTypeSymbol(type, cancellationToken)?.IsNullableOf(typeSymbol) == true)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
