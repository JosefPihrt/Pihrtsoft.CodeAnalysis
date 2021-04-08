// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Roslynator.CSharp.Syntax.SyntaxInfoHelpers;

namespace Roslynator.CSharp.Syntax
{
    /// <summary>
    /// Provides information about a null check expression.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly struct NullCheckExpressionInfo
    {
        private NullCheckExpressionInfo(
            ExpressionSyntax nullCheckExpression,
            ExpressionSyntax expression,
            NullCheckStyles style)
        {
            NullCheckExpression = nullCheckExpression;
            Expression = expression;
            Style = style;
        }

        /// <summary>
        /// The null check expression, e.g. "x == null".
        /// </summary>
        public ExpressionSyntax NullCheckExpression { get; }

        /// <summary>
        /// The expression that is evaluated whether is (not) null. for example "x" in "x == null".
        /// </summary>
        public ExpressionSyntax Expression { get; }

        /// <summary>
        /// The style of this null check.
        /// </summary>
        public NullCheckStyles Style { get; }

        /// <summary>
        /// Determines whether this null check is checking if the expression is null.
        /// </summary>
        public bool IsCheckingNull
        {
            get { return (Style & NullCheckStyles.CheckingNull) != 0; }
        }

        /// <summary>
        /// Determines whether this null check is checking if the expression is not null.
        /// </summary>
        public bool IsCheckingNotNull
        {
            get { return (Style & NullCheckStyles.CheckingNotNull) != 0; }
        }

        /// <summary>
        /// Determines whether this struct was initialized with an actual syntax.
        /// </summary>
        public bool Success
        {
            get { return Style != NullCheckStyles.None; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay
        {
            get
            {
                return (Success)
                    ? $"{GetType().Name} {Style} {Expression} {NullCheckExpression}"
                    : "Uninitialized";
            }
        }

        internal static NullCheckExpressionInfo Create(
            SyntaxNode node,
            NullCheckStyles allowedStyles = NullCheckStyles.ComparisonToNull | NullCheckStyles.IsPattern,
            bool walkDownParentheses = true,
            bool allowMissing = false)
        {
            if ((allowedStyles & NullCheckStyles.HasValue) != 0)
                throw new ArgumentException($"'{nameof(NullCheckStyles.HasValue)}' style requires a SemanticModel to be provided.", nameof(allowedStyles));

            if ((allowedStyles & NullCheckStyles.NotHasValue) != 0)
                throw new ArgumentException($"'{nameof(NullCheckStyles.NotHasValue)}' style requires a SemanticModel to be provided.", nameof(allowedStyles));

            return CreateImpl(node, default(SemanticModel), allowedStyles, walkDownParentheses, allowMissing, default(CancellationToken));
        }

        internal static NullCheckExpressionInfo Create(
            SyntaxNode node,
            SemanticModel semanticModel,
            NullCheckStyles allowedStyles = NullCheckStyles.All,
            bool walkDownParentheses = true,
            bool allowMissing = false,
            CancellationToken cancellationToken = default)
        {
            if (semanticModel == null)
                throw new ArgumentNullException(nameof(semanticModel));

            return CreateImpl(node, semanticModel, allowedStyles, walkDownParentheses, allowMissing, cancellationToken);
        }

        private static NullCheckExpressionInfo CreateImpl(
            SyntaxNode node,
            SemanticModel semanticModel,
            NullCheckStyles allowedStyles,
            bool walkDownParentheses,
            bool allowMissing,
            CancellationToken cancellationToken)
        {
            ExpressionSyntax expression = WalkAndCheck(node, walkDownParentheses, allowMissing);

            if (expression == null)
                return default;

            SyntaxKind kind = expression.Kind();

            switch (kind)
            {
                case SyntaxKind.EqualsExpression:
                case SyntaxKind.NotEqualsExpression:
                    {
                        var binaryExpression = (BinaryExpressionSyntax)expression;

                        ExpressionSyntax left = WalkAndCheck(binaryExpression.Left, walkDownParentheses, allowMissing);

                        if (left == null)
                            break;

                        ExpressionSyntax right = WalkAndCheck(binaryExpression.Right, walkDownParentheses, allowMissing);

                        if (right == null)
                            break;

                        NullCheckExpressionInfo info = Create(binaryExpression, kind, left, right, allowedStyles, allowMissing, semanticModel, cancellationToken);

                        if (info.Success)
                        {
                            return info;
                        }
                        else
                        {
                            return Create(binaryExpression, kind, right, left, allowedStyles, allowMissing, semanticModel, cancellationToken);
                        }
                    }
                case SyntaxKind.SimpleMemberAccessExpression:
                    {
                        if ((allowedStyles & NullCheckStyles.HasValue) == 0)
                            break;

                        var memberAccessExpression = (MemberAccessExpressionSyntax)expression;

                        if (!IsPropertyOfNullableOfT(memberAccessExpression.Name, "HasValue", semanticModel, cancellationToken))
                            break;

                        return new NullCheckExpressionInfo(expression, memberAccessExpression.Expression, NullCheckStyles.HasValue);
                    }
                case SyntaxKind.IsPatternExpression:
                    {
                        var isPatternExpression = (IsPatternExpressionSyntax)expression;

                        PatternSyntax pattern = isPatternExpression.Pattern;

                        bool isNotPattern = pattern.IsKind(SyntaxKind.NotPattern);

                        if (isNotPattern)
                            pattern = ((UnaryPatternSyntax)pattern).Pattern;

                        if (!(pattern is ConstantPatternSyntax constantPattern))
                            break;

                        if (!constantPattern.Expression.IsKind(SyntaxKind.NullLiteralExpression))
                            break;

                        ExpressionSyntax e = WalkAndCheck(isPatternExpression.Expression, walkDownParentheses, allowMissing);

                        if (e == null)
                            break;

                        return new NullCheckExpressionInfo(
                            expression,
                            e,
                            (isNotPattern) ? NullCheckStyles.IsNotNull : NullCheckStyles.IsNull);
                    }
                case SyntaxKind.LogicalNotExpression:
                    {
                        bool isNotHasValueAllowed = (allowedStyles & (NullCheckStyles.NotHasValue)) != 0;
                        bool isNotIsNullAllowed = (allowedStyles & (NullCheckStyles.NotIsNull)) != 0;

                        if (!isNotHasValueAllowed && !isNotIsNullAllowed)
                            break;

                        var logicalNotExpression = (PrefixUnaryExpressionSyntax)expression;

                        ExpressionSyntax operand = WalkAndCheck(logicalNotExpression.Operand, walkDownParentheses, allowMissing);

                        if (operand == null)
                            break;

                        switch (operand.Kind())
                        {
                            case SyntaxKind.SimpleMemberAccessExpression:
                                {
                                    if (!isNotHasValueAllowed)
                                        break;

                                    var memberAccessExpression = (MemberAccessExpressionSyntax)operand;

                                    if (!IsPropertyOfNullableOfT(memberAccessExpression.Name, "HasValue", semanticModel, cancellationToken))
                                        break;

                                    return new NullCheckExpressionInfo(expression, memberAccessExpression.Expression, NullCheckStyles.NotHasValue);
                                }
                            case SyntaxKind.IsPatternExpression:
                                {
                                    if (!isNotIsNullAllowed)
                                        break;

                                    var isPatternExpression = (IsPatternExpressionSyntax)operand;

                                    if (!(isPatternExpression.Pattern is ConstantPatternSyntax constantPattern))
                                        break;

                                    if (!constantPattern.Expression.IsKind(SyntaxKind.NullLiteralExpression))
                                        break;

                                    ExpressionSyntax e = WalkAndCheck(isPatternExpression.Expression, walkDownParentheses, allowMissing);

                                    if (e == null)
                                        break;

                                    return new NullCheckExpressionInfo(expression, e, NullCheckStyles.NotIsNull);
                                }
                        }

                        break;
                    }
            }

            return default;
        }

        private static NullCheckExpressionInfo Create(
            BinaryExpressionSyntax binaryExpression,
            SyntaxKind binaryExpressionKind,
            ExpressionSyntax expression1,
            ExpressionSyntax expression2,
            NullCheckStyles allowedStyles,
            bool allowMissing,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            switch (expression1.Kind())
            {
                case SyntaxKind.NullLiteralExpression:
                case SyntaxKind.DefaultLiteralExpression:
                case SyntaxKind.DefaultExpression:
                    {
                        NullCheckStyles style = (binaryExpressionKind == SyntaxKind.EqualsExpression) ? NullCheckStyles.EqualsToNull : NullCheckStyles.NotEqualsToNull;

                        if ((allowedStyles & style) == 0)
                            break;

                        if (!IsNullOrDefault(expression2, expression1, semanticModel, cancellationToken))
                            break;

                        return new NullCheckExpressionInfo(
                            binaryExpression,
                            expression2,
                            style);
                    }
                case SyntaxKind.TrueLiteralExpression:
                    {
                        NullCheckStyles style = (binaryExpressionKind == SyntaxKind.EqualsExpression) ? NullCheckStyles.HasValue : NullCheckStyles.NotHasValue;

                        return Create(
                            binaryExpression,
                            expression2,
                            style,
                            allowedStyles,
                            allowMissing,
                            semanticModel,
                            cancellationToken);
                    }
                case SyntaxKind.FalseLiteralExpression:
                    {
                        NullCheckStyles style = (binaryExpressionKind == SyntaxKind.EqualsExpression) ? NullCheckStyles.NotHasValue : NullCheckStyles.HasValue;

                        return Create(
                            binaryExpression,
                            expression2,
                            style,
                            allowedStyles,
                            allowMissing,
                            semanticModel,
                            cancellationToken);
                    }
            }

            return default;
        }

        private static NullCheckExpressionInfo Create(
            BinaryExpressionSyntax binaryExpression,
            ExpressionSyntax expression,
            NullCheckStyles style,
            NullCheckStyles allowedStyles,
            bool allowMissing,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            if ((allowedStyles & (NullCheckStyles.HasValueProperty)) == 0)
                return default;

            if (!(expression is MemberAccessExpressionSyntax memberAccessExpression))
                return default;

            if (memberAccessExpression.Kind() != SyntaxKind.SimpleMemberAccessExpression)
                return default;

            if (!IsPropertyOfNullableOfT(memberAccessExpression.Name, "HasValue", semanticModel, cancellationToken))
                return default;

            if ((allowedStyles & style) == 0)
                return default;

            ExpressionSyntax expression2 = memberAccessExpression.Expression;

            if (!Check(expression2, allowMissing))
                return default;

            return new NullCheckExpressionInfo(binaryExpression, expression2, style);
        }

        private static bool IsPropertyOfNullableOfT(
            ExpressionSyntax expression,
            string name,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            return expression?.Kind() == SyntaxKind.IdentifierName
                && string.Equals(((IdentifierNameSyntax)expression).Identifier.ValueText, name, StringComparison.Ordinal)
                && SyntaxUtility.IsPropertyOfNullableOfT(expression, name, semanticModel, cancellationToken);
        }

        private static bool IsNullOrDefault(
            ExpressionSyntax left,
            ExpressionSyntax right,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            switch (right?.Kind())
            {
                case SyntaxKind.NullLiteralExpression:
                    {
                        return true;
                    }
                case SyntaxKind.DefaultExpression:
                    {
                        if (semanticModel == null)
                            return false;

                        ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(left, cancellationToken);

                        if (!typeSymbol.IsReferenceType)
                            return false;

                        ITypeSymbol typeSymbol2 = semanticModel.GetTypeSymbol(right, cancellationToken);

                        return SymbolEqualityComparer.Default.Equals(typeSymbol, typeSymbol2);
                    }
                case SyntaxKind.DefaultLiteralExpression:
                    {
                        return semanticModel?
                            .GetTypeSymbol(left, cancellationToken)
                            .IsReferenceType == true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }
    }
}
