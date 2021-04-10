// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UseStringIsNullOrEmptyMethodAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.UseStringIsNullOrEmptyMethod);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(
                f => AnalyzeBinaryExpression(f),
                SyntaxKind.LogicalOrExpression,
                SyntaxKind.LogicalAndExpression);
        }

        private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
        {
            var binaryExpression = (BinaryExpressionSyntax)context.Node;

            if (binaryExpression.ContainsDiagnostics)
                return;

            if (binaryExpression.SpanContainsDirectives())
                return;

            BinaryExpressionInfo info = SyntaxInfo.BinaryExpressionInfo(binaryExpression);

            if (!info.Success)
                return;

            SyntaxKind kind = binaryExpression.Kind();

            if (kind == SyntaxKind.LogicalOrExpression)
            {
                if (info.Left.IsKind(SyntaxKind.EqualsExpression, SyntaxKind.IsPatternExpression)
                    && info.Right.IsKind(SyntaxKind.EqualsExpression)
                    && IsFixable(
                        info.Left,
                        (BinaryExpressionSyntax)info.Right,
                        context.SemanticModel,
                        context.CancellationToken))
                {
                    DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UseStringIsNullOrEmptyMethod, binaryExpression);
                }
            }
            else if (kind == SyntaxKind.LogicalAndExpression)
            {
                if (info.Left.IsKind(SyntaxKind.NotEqualsExpression, SyntaxKind.LogicalNotExpression)
                    && info.Right.IsKind(SyntaxKind.NotEqualsExpression, SyntaxKind.GreaterThanExpression)
                    && IsFixable(
                        info.Left,
                        (BinaryExpressionSyntax)info.Right,
                        context.SemanticModel,
                        context.CancellationToken))
                {
                    DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UseStringIsNullOrEmptyMethod, binaryExpression);
                }
            }
        }

        private static bool IsFixable(
            ExpressionSyntax left,
            BinaryExpressionSyntax right,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default)
        {
            NullCheckExpressionInfo nullCheck = SyntaxInfo.NullCheckExpressionInfo(left);

            if (!nullCheck.Success)
                return false;

            ExpressionSyntax expression = nullCheck.Expression;

            if (CSharpFactory.AreEquivalent(expression, right.Left))
            {
                return right.IsKind(SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression)
                    && SymbolEquals(expression, right.Left, semanticModel, cancellationToken)
                    && CSharpUtility.IsEmptyStringExpression(right.Right, semanticModel, cancellationToken);
            }

            if (right.Left.IsKind(SyntaxKind.SimpleMemberAccessExpression))
            {
                var memberAccess = (MemberAccessExpressionSyntax)right.Left;

                return string.Equals(memberAccess.Name.Identifier.ValueText, "Length", StringComparison.Ordinal)
                    && right.Right.IsNumericLiteralExpression("0")
                    && semanticModel.GetSymbol(memberAccess, cancellationToken) is IPropertySymbol propertySymbol
                    && !propertySymbol.IsIndexer
                    && SymbolUtility.IsPublicInstance(propertySymbol, "Length")
                    && propertySymbol.Type.SpecialType == SpecialType.System_Int32
                    && propertySymbol.ContainingType?.SpecialType == SpecialType.System_String
                    && CSharpFactory.AreEquivalent(expression, memberAccess.Expression)
                    && SymbolEquals(expression, memberAccess.Expression, semanticModel, cancellationToken);
            }

            return false;
        }

        private static bool SymbolEquals(
            ExpressionSyntax expression1,
            ExpressionSyntax expression2,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            return SymbolEqualityComparer.Default.Equals(
                semanticModel.GetSymbol(expression1, cancellationToken),
                semanticModel.GetSymbol(expression2, cancellationToken));
        }
    }
}
