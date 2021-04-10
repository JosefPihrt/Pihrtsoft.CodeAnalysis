// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
    public sealed class BinaryOperatorAnalyzer : BaseDiagnosticAnalyzer
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
                        DiagnosticRules.ExpressionIsAlwaysEqualToTrueOrFalse,
                        DiagnosticRules.UnnecessaryOperator);
                }

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (DiagnosticRules.UnnecessaryOperator.IsEffective(c))
                        AnalyzeLessThanExpression(c);
                },
                SyntaxKind.LessThanExpression);

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (DiagnosticRules.UnnecessaryOperator.IsEffective(c))
                        AnalyzeGreaterThanExpression(c);
                },
                SyntaxKind.GreaterThanExpression);

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (DiagnosticRules.ExpressionIsAlwaysEqualToTrueOrFalse.IsEffective(c))
                        AnalyzeSimpleMemberAccessExpression(c);
                },
                SyntaxKind.SimpleMemberAccessExpression);

            context.RegisterSyntaxNodeAction(c => AnalyzeLessThanOrEqualExpression(c), SyntaxKind.LessThanOrEqualExpression);
            context.RegisterSyntaxNodeAction(c => AnalyzeGreaterThanOrEqualExpression(c), SyntaxKind.GreaterThanOrEqualExpression);
            context.RegisterSyntaxNodeAction(c => AnalyzeLogicalOrExpression(c), SyntaxKind.LogicalOrExpression);
        }

        // x == double.NaN >>> double.IsNaN(x)
        // x != double.NaN >>> !double.IsNaN(x)
        private void AnalyzeSimpleMemberAccessExpression(SyntaxNodeAnalysisContext context)
        {
            var simpleMemberAccess = (MemberAccessExpressionSyntax)context.Node;

            if (!(simpleMemberAccess.Name is IdentifierNameSyntax identifierName))
                return;

            if (identifierName.Identifier.ValueText != "NaN")
                return;

            ExpressionSyntax expression = simpleMemberAccess.WalkUpParentheses();

            SyntaxNode binaryExpression = expression.Parent;

            if (!binaryExpression.IsKind(SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression))
                return;

            ISymbol symbol = context.SemanticModel.GetSymbol(simpleMemberAccess, context.CancellationToken);

            if (symbol?.ContainingType?.SpecialType != SpecialType.System_Double)
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.ExpressionIsAlwaysEqualToTrueOrFalse,
                binaryExpression.GetLocation(),
                ImmutableDictionary.CreateRange(new[] { new KeyValuePair<string, string>("DoubleNaN", (((BinaryExpressionSyntax)binaryExpression).Left == expression) ? "Right" : "Left") }),
                (binaryExpression.IsKind(SyntaxKind.EqualsExpression)) ? "false" : "true");
        }

        // x:
        // byte
        // ushort
        // uint
        // ulong
        // Array.Length
        // string.Length
        // ICollection<T>.Count
        // IReadOnlyCollection<T>.Count

        // x >= 0 >>> true
        // 0 >= x >>> 0 == x
        private static void AnalyzeGreaterThanOrEqualExpression(SyntaxNodeAnalysisContext context)
        {
            var greaterThanOrEqualExpression = (BinaryExpressionSyntax)context.Node;

            BinaryExpressionInfo info = SyntaxInfo.BinaryExpressionInfo(greaterThanOrEqualExpression);

            if (!info.Success)
                return;

            if (DiagnosticRules.ExpressionIsAlwaysEqualToTrueOrFalse.IsEffective(context)
                && IsAlwaysEqualToTrueOrFalse(greaterThanOrEqualExpression, info.Left, info.Right, context.SemanticModel, context.CancellationToken))
            {
                ReportExpressionAlwaysEqualToTrueOrFalse(context, "true");
            }
            else if (DiagnosticRules.UnnecessaryOperator.IsEffective(context)
                && IsUnnecessaryRelationalOperator(info.Right, info.Left, context.SemanticModel, context.CancellationToken))
            {
                ReportUnnecessaryRelationalOperator(context, info.OperatorToken);
            }
        }

        // 0 > x >>> false
        private static void AnalyzeGreaterThanExpression(SyntaxNodeAnalysisContext context)
        {
            var greaterThanExpression = (BinaryExpressionSyntax)context.Node;

            BinaryExpressionInfo info = SyntaxInfo.BinaryExpressionInfo(greaterThanExpression);

            if (!info.Success)
                return;

            if (!IsAlwaysEqualToTrueOrFalse(greaterThanExpression, info.Right, info.Left, context.SemanticModel, context.CancellationToken))
                return;

            ReportExpressionAlwaysEqualToTrueOrFalse(context, "false");
        }

        // 0 <= x >>> true
        // x <= 0 >>> x == 0
        private static void AnalyzeLessThanOrEqualExpression(SyntaxNodeAnalysisContext context)
        {
            var lessThanOrEqualExpression = (BinaryExpressionSyntax)context.Node;

            BinaryExpressionInfo info = SyntaxInfo.BinaryExpressionInfo(lessThanOrEqualExpression);

            if (!info.Success)
                return;

            if (DiagnosticRules.ExpressionIsAlwaysEqualToTrueOrFalse.IsEffective(context)
                && IsAlwaysEqualToTrueOrFalse(lessThanOrEqualExpression, info.Right, info.Left, context.SemanticModel, context.CancellationToken))
            {
                ReportExpressionAlwaysEqualToTrueOrFalse(context, "true");
            }
            else if (DiagnosticRules.UnnecessaryOperator.IsEffective(context)
                && IsUnnecessaryRelationalOperator(info.Left, info.Right, context.SemanticModel, context.CancellationToken))
            {
                ReportUnnecessaryRelationalOperator(context, info.OperatorToken);
            }
        }

        // x < 0 >>> false
        private static void AnalyzeLessThanExpression(SyntaxNodeAnalysisContext context)
        {
            var lessThanExpression = (BinaryExpressionSyntax)context.Node;

            BinaryExpressionInfo info = SyntaxInfo.BinaryExpressionInfo(lessThanExpression);

            if (!info.Success)
                return;

            if (!IsAlwaysEqualToTrueOrFalse(lessThanExpression, info.Left, info.Right, context.SemanticModel, context.CancellationToken))
                return;

            ReportExpressionAlwaysEqualToTrueOrFalse(context, "false");
        }

        private static bool IsAlwaysEqualToTrueOrFalse(
            BinaryExpressionSyntax binaryExpression,
            ExpressionSyntax left,
            ExpressionSyntax right,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            if (!right.IsNumericLiteralExpression("0"))
                return false;

            if (binaryExpression.IsKind(SyntaxKind.LessThanOrEqualExpression, SyntaxKind.GreaterThanOrEqualExpression)
                && IsReversedForStatement())
            {
                return false;
            }

            ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(left, cancellationToken);

            switch (typeSymbol?.SpecialType)
            {
                case SpecialType.System_Byte:
                case SpecialType.System_UInt16:
                case SpecialType.System_UInt32:
                case SpecialType.System_UInt64:
                    return true;
                case SpecialType.System_Int32:
                    return IsCountOrLengthProperty(left, semanticModel, cancellationToken);
                default:
                    return false;
            }

            bool IsReversedForStatement()
            {
                if (!(left is IdentifierNameSyntax identifierName))
                    return false;

                if (!(binaryExpression.WalkUpParentheses().Parent is ForStatementSyntax forStatement))
                    return false;

                VariableDeclarationSyntax declaration = forStatement.Declaration;

                if (declaration == null)
                    return false;

                string name = identifierName.Identifier.ValueText;

                foreach (VariableDeclaratorSyntax declarator in declaration.Variables)
                {
                    if (string.Equals(name, declarator.Identifier.ValueText, StringComparison.Ordinal))
                        return true;
                }

                return false;
            }
        }

        private static bool IsUnnecessaryRelationalOperator(
            ExpressionSyntax left,
            ExpressionSyntax right,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            if (!right.IsNumericLiteralExpression("0"))
                return false;

            if (!IsCountOrLengthProperty(left, semanticModel, cancellationToken))
                return false;

            return true;
        }

        private static bool IsCountOrLengthProperty(
            ExpressionSyntax expression,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            if (expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
            {
                var memberAccessExpression = (MemberAccessExpressionSyntax)expression;

                if (memberAccessExpression.Name is IdentifierNameSyntax identifierName)
                {
                    switch (identifierName.Identifier.ValueText)
                    {
                        case "Count":
                        case "Length":
                            {
                                if (semanticModel.GetSymbol(expression, cancellationToken) is IPropertySymbol propertySymbol
                                    && propertySymbol.Type.SpecialType == SpecialType.System_Int32)
                                {
                                    INamedTypeSymbol containingType = propertySymbol.ContainingType?.OriginalDefinition;

                                    switch (containingType?.SpecialType)
                                    {
                                        case SpecialType.System_String:
                                        case SpecialType.System_Array:
                                        case SpecialType.System_Collections_Generic_ICollection_T:
                                        case SpecialType.System_Collections_Generic_IList_T:
                                        case SpecialType.System_Collections_Generic_IReadOnlyCollection_T:
                                        case SpecialType.System_Collections_Generic_IReadOnlyList_T:
                                            {
                                                return true;
                                            }
                                        default:
                                            {
                                                if (containingType?.ImplementsAny(
                                                    SpecialType.System_Collections_Generic_ICollection_T,
                                                    SpecialType.System_Collections_Generic_IReadOnlyCollection_T,
                                                    allInterfaces: true) == true)
                                                {
                                                    return true;
                                                }

                                                break;
                                            }
                                    }
                                }

                                break;
                            }
                    }
                }
            }

            return false;
        }

        private static void AnalyzeLogicalOrExpression(SyntaxNodeAnalysisContext context)
        {
            var binaryExpression = (BinaryExpressionSyntax)context.Node;

            if (binaryExpression.ContainsDiagnostics)
                return;

            NullCheckExpressionInfo nullCheck = SyntaxInfo.NullCheckExpressionInfo(binaryExpression.Left, context.SemanticModel, allowedStyles: NullCheckStyles.CheckingNull);

            ExpressionSyntax expression = nullCheck.Expression;

            if (expression == null)
                return;

            ExpressionSyntax right = binaryExpression.Right.WalkDownParentheses();

            if (!right.IsKind(SyntaxKind.LogicalAndExpression))
                return;

            var logicalAndExpression = (BinaryExpressionSyntax)right;

            ExpressionChain.Enumerator en = logicalAndExpression.AsChain().GetEnumerator();

            if (!en.MoveNext())
                return;

            NullCheckExpressionInfo nullCheck2 = SyntaxInfo.NullCheckExpressionInfo(en.Current, context.SemanticModel, allowedStyles: NullCheckStyles.CheckingNotNull);

            if (!CSharpFactory.AreEquivalent(expression, nullCheck2.Expression))
                return;

            ReportExpressionAlwaysEqualToTrueOrFalse(context, nullCheck2.NullCheckExpression, "true");
        }

        private static void ReportExpressionAlwaysEqualToTrueOrFalse(SyntaxNodeAnalysisContext context, string booleanName)
        {
            ReportExpressionAlwaysEqualToTrueOrFalse(context, context.Node, booleanName);
        }

        private static void ReportExpressionAlwaysEqualToTrueOrFalse(SyntaxNodeAnalysisContext context, SyntaxNode node, string booleanName)
        {
            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.ExpressionIsAlwaysEqualToTrueOrFalse,
                node,
                booleanName);
        }

        private static void ReportUnnecessaryRelationalOperator(SyntaxNodeAnalysisContext context, SyntaxToken operatorToken)
        {
            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.UnnecessaryOperator,
                Location.Create(operatorToken.SyntaxTree, new TextSpan(operatorToken.SpanStart, 1)),
                operatorToken.ToString());
        }
    }
}
