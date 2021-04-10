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
    public sealed class SimplifyLogicalNegationAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.SimplifyLogicalNegation);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeLogicalNotExpression(f), SyntaxKind.LogicalNotExpression);
        }

        private static void AnalyzeLogicalNotExpression(SyntaxNodeAnalysisContext context)
        {
            var logicalNot = (PrefixUnaryExpressionSyntax)context.Node;

            ExpressionSyntax expression = logicalNot.Operand?.WalkDownParentheses();

            if (expression?.IsMissing != false)
                return;

            switch (expression.Kind())
            {
                case SyntaxKind.TrueLiteralExpression:
                case SyntaxKind.FalseLiteralExpression:
                case SyntaxKind.LogicalNotExpression:
                    {
                        ReportDiagnostic();
                        break;
                    }
                case SyntaxKind.EqualsExpression:
                    {
                        MemberDeclarationSyntax memberDeclaration = logicalNot.FirstAncestor<MemberDeclarationSyntax>();

                        if (memberDeclaration is OperatorDeclarationSyntax operatorDeclaration
                            && operatorDeclaration.OperatorToken.IsKind(SyntaxKind.ExclamationEqualsToken))
                        {
                            return;
                        }

                        ReportDiagnostic();
                        break;
                    }
                case SyntaxKind.NotEqualsExpression:
                    {
                        MemberDeclarationSyntax memberDeclaration = logicalNot.FirstAncestor<MemberDeclarationSyntax>();

                        if (memberDeclaration is OperatorDeclarationSyntax operatorDeclaration
                            && operatorDeclaration.OperatorToken.IsKind(SyntaxKind.EqualsEqualsToken))
                        {
                            return;
                        }

                        ReportDiagnostic();
                        break;
                    }
                case SyntaxKind.LessThanExpression:
                case SyntaxKind.LessThanOrEqualExpression:
                case SyntaxKind.GreaterThanExpression:
                case SyntaxKind.GreaterThanOrEqualExpression:
                    {
                        var binaryExpression = (BinaryExpressionSyntax)expression;

                        if (IsNumericType(binaryExpression.Left, context.SemanticModel, context.CancellationToken)
                            && IsNumericType(binaryExpression.Right, context.SemanticModel, context.CancellationToken))
                        {
                            ReportDiagnostic();
                        }

                        break;
                    }
            }

            void ReportDiagnostic()
            {
                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.SimplifyLogicalNegation, logicalNot);
            }
        }

        public static bool IsNumericType(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (expression?.IsMissing == false)
            {
                if (expression.IsKind(SyntaxKind.NumericLiteralExpression))
                    return true;

                switch (semanticModel
                    .GetTypeInfo(expression, cancellationToken)
                    .ConvertedType?
                    .SpecialType)
                {
                    case SpecialType.System_SByte:
                    case SpecialType.System_Byte:
                    case SpecialType.System_Int16:
                    case SpecialType.System_UInt16:
                    case SpecialType.System_Int32:
                    case SpecialType.System_UInt32:
                    case SpecialType.System_Int64:
                    case SpecialType.System_UInt64:
                    case SpecialType.System_Decimal:
                        {
                            return true;
                        }
                    case SpecialType.System_Single:
                        {
                            Optional<object> optional = semanticModel.GetConstantValue(expression, cancellationToken);

                            return optional.HasValue
                                && optional.Value is float value
                                && !float.IsNaN(value);
                        }
                    case SpecialType.System_Double:
                        {
                            Optional<object> optional = semanticModel.GetConstantValue(expression, cancellationToken);

                            return optional.HasValue
                                && optional.Value is double value
                                && !double.IsNaN(value);
                        }
                }
            }

            return false;
        }

        public static void Analyze(SyntaxNodeAnalysisContext context, in SimpleMemberInvocationExpressionInfo invocationInfo)
        {
            SyntaxNode parent = invocationInfo.InvocationExpression.WalkUpParentheses().Parent;

            if (parent.Kind() != SyntaxKind.LogicalNotExpression)
                return;

            SingleParameterLambdaExpressionInfo lambdaInfo = SyntaxInfo.SingleParameterLambdaExpressionInfo(invocationInfo.Arguments[0].Expression.WalkDownParentheses());

            if (!lambdaInfo.Success)
                return;

            ExpressionSyntax expression = GetReturnExpression(lambdaInfo.Body)?.WalkDownParentheses();

            if (expression?.IsKind(SyntaxKind.LogicalNotExpression) != true)
                return;

            IMethodSymbol methodSymbol = context.SemanticModel.GetReducedExtensionMethodInfo(invocationInfo.InvocationExpression, context.CancellationToken).Symbol;

            if (methodSymbol == null)
                return;

            if (!SymbolUtility.IsLinqExtensionOfIEnumerableOfTWithPredicate(methodSymbol))
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.SimplifyLogicalNegation, parent);
        }

        internal static ExpressionSyntax GetReturnExpression(CSharpSyntaxNode node)
        {
            if (node is BlockSyntax block)
            {
                StatementSyntax statement = block.Statements.SingleOrDefault(shouldThrow: false);

                return (statement as ReturnStatementSyntax)?.Expression;
            }

            return node as ExpressionSyntax;
        }
    }
}
