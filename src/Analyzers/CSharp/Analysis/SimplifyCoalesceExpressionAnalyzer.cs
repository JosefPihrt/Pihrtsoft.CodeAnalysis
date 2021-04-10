// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SimplifyCoalesceExpressionAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.SimplifyCoalesceExpression);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeCoalesceExpression(f), SyntaxKind.CoalesceExpression);
        }

        private static void AnalyzeCoalesceExpression(SyntaxNodeAnalysisContext context)
        {
            var coalesceExpression = (BinaryExpressionSyntax)context.Node;

            if (coalesceExpression.SpanContainsDirectives())
                return;

            BinaryExpressionInfo info = SyntaxInfo.BinaryExpressionInfo(coalesceExpression);

            if (!info.Success)
                return;

            TextSpan span = GetRedundantSpan(coalesceExpression, info.Left, info.Right, context.SemanticModel, context.CancellationToken);

            if (span == default)
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.SimplifyCoalesceExpression,
                Location.Create(coalesceExpression.SyntaxTree, span));
        }

        private static TextSpan GetRedundantSpan(
            BinaryExpressionSyntax coalesceExpression,
            ExpressionSyntax left,
            ExpressionSyntax right,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            switch (GetRedundantPart(left, right, semanticModel, cancellationToken))
            {
                case BinaryExpressionPart.Left:
                    return TextSpan.FromBounds(left.SpanStart, coalesceExpression.OperatorToken.Span.End);
                case BinaryExpressionPart.Right:
                    return TextSpan.FromBounds(coalesceExpression.OperatorToken.SpanStart, coalesceExpression.Right.Span.End);
                default:
                    return default;
            }
        }

        private static BinaryExpressionPart GetRedundantPart(
            ExpressionSyntax left,
            ExpressionSyntax right,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            SyntaxKind leftKind = left.Kind();
            SyntaxKind rightKind = right.Kind();

            switch (leftKind)
            {
                case SyntaxKind.ObjectCreationExpression:
                case SyntaxKind.AnonymousObjectCreationExpression:
                case SyntaxKind.ArrayCreationExpression:
                case SyntaxKind.ImplicitArrayCreationExpression:
                case SyntaxKind.InterpolatedStringExpression:
                case SyntaxKind.ThisExpression:
                case SyntaxKind.StringLiteralExpression:
                case SyntaxKind.TypeOfExpression:
                    return BinaryExpressionPart.Right;
                case SyntaxKind.NullLiteralExpression:
                    return BinaryExpressionPart.Left;
                case SyntaxKind.DefaultExpression:
                    {
                        if (IsDefaultOfReferenceOrNullableType((DefaultExpressionSyntax)left, semanticModel, cancellationToken))
                            return BinaryExpressionPart.Left;

                        break;
                    }
            }

            Optional<object> optional = semanticModel.GetConstantValue(left, cancellationToken);

            if (optional.HasValue)
            {
                object value = optional.Value;

                if (value != null)
                {
                    return BinaryExpressionPart.Right;
                }
                else
                {
                    return BinaryExpressionPart.Left;
                }
            }

            ITypeSymbol leftSymbol = semanticModel.GetTypeSymbol(left, cancellationToken);

            if (leftSymbol?.IsErrorType() == false
                && leftSymbol.IsValueType
                && !leftSymbol.IsNullableType())
            {
                return BinaryExpressionPart.Right;
            }

            switch (rightKind)
            {
                case SyntaxKind.NullLiteralExpression:
                    {
                        return BinaryExpressionPart.Right;
                    }
                case SyntaxKind.DefaultExpression:
                    {
                        if (IsDefaultOfReferenceOrNullableType((DefaultExpressionSyntax)right, semanticModel, cancellationToken))
                            return BinaryExpressionPart.Right;

                        break;
                    }
            }

            if (leftKind == rightKind
                && CSharpFactory.AreEquivalent(left, right))
            {
                return BinaryExpressionPart.Right;
            }

            return BinaryExpressionPart.None;
        }

        private static bool IsDefaultOfReferenceOrNullableType(DefaultExpressionSyntax defaultExpression, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            TypeSyntax type = defaultExpression.Type;

            if (type != null)
            {
                if (type.IsKind(SyntaxKind.NullableType))
                    return true;

                ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(type, cancellationToken);

                if (typeSymbol?.IsErrorType() == false
                    && typeSymbol.IsReferenceType)
                {
                    return true;
                }
            }

            return false;
        }

        private enum BinaryExpressionPart
        {
            None,
            Left,
            Right,
        }
    }
}
