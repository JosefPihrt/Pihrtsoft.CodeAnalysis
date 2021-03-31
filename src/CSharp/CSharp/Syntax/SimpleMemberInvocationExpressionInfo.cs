// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Roslynator.CSharp.Syntax.SyntaxInfoHelpers;

namespace Roslynator.CSharp.Syntax
{
    /// <summary>
    /// Provides information about invocation expression.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly struct SimpleMemberInvocationExpressionInfo
    {
        private SimpleMemberInvocationExpressionInfo(
            InvocationExpressionSyntax invocationExpression,
            MemberAccessExpressionSyntax memberAccessExpression)
        {
            InvocationExpression = invocationExpression;
            MemberAccessExpression = memberAccessExpression;
        }

        /// <summary>
        /// The invocation expression.
        /// </summary>
        public InvocationExpressionSyntax InvocationExpression { get; }

        /// <summary>
        /// The member access expression.
        /// </summary>
        public MemberAccessExpressionSyntax MemberAccessExpression { get; }

        /// <summary>
        /// The expression that contains the member being invoked.
        /// </summary>
        public ExpressionSyntax Expression
        {
            get { return MemberAccessExpression?.Expression; }
        }

        /// <summary>
        /// The name of the member being invoked.
        /// </summary>
        public SimpleNameSyntax Name
        {
            get { return MemberAccessExpression?.Name; }
        }

        /// <summary>
        /// The argument list.
        /// </summary>
        public ArgumentListSyntax ArgumentList
        {
            get { return InvocationExpression?.ArgumentList; }
        }

        /// <summary>
        /// The list of the arguments.
        /// </summary>
        public SeparatedSyntaxList<ArgumentSyntax> Arguments
        {
            get { return InvocationExpression?.ArgumentList.Arguments ?? default; }
        }

        /// <summary>
        /// The operator in the member access expression.
        /// </summary>
        public SyntaxToken OperatorToken
        {
            get { return MemberAccessExpression?.OperatorToken ?? default; }
        }

        /// <summary>
        /// The name of the member being invoked.
        /// </summary>
        public string NameText
        {
            get { return Name?.Identifier.ValueText; }
        }

        /// <summary>
        /// Determines whether this struct was initialized with an actual syntax.
        /// </summary>
        public bool Success
        {
            get { return InvocationExpression != null; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay
        {
            get { return SyntaxInfoHelpers.ToDebugString(Success, this, InvocationExpression); }
        }

        internal static SimpleMemberInvocationExpressionInfo Create(
            SyntaxNode node,
            bool walkDownParentheses = true,
            bool allowMissing = false)
        {
            return CreateImpl(
                Walk(node, walkDownParentheses) as InvocationExpressionSyntax,
                allowMissing);
        }

        internal static SimpleMemberInvocationExpressionInfo Create(
            InvocationExpressionSyntax invocationExpression,
            bool allowMissing = false)
        {
            return CreateImpl(invocationExpression, allowMissing);
        }

        private static SimpleMemberInvocationExpressionInfo CreateImpl(
            InvocationExpressionSyntax invocationExpression,
            bool allowMissing = false)
        {
            if (!(invocationExpression?.Expression is MemberAccessExpressionSyntax memberAccessExpression))
                return default;

            if (memberAccessExpression.Kind() != SyntaxKind.SimpleMemberAccessExpression)
                return default;

            ExpressionSyntax expression = memberAccessExpression.Expression;

            if (!Check(expression, allowMissing))
                return default;

            SimpleNameSyntax name = memberAccessExpression.Name;

            if (!Check(name, allowMissing))
                return default;

            ArgumentListSyntax argumentList = invocationExpression.ArgumentList;

            if (argumentList == null)
                return default;

            return new SimpleMemberInvocationExpressionInfo(invocationExpression, memberAccessExpression);
        }

        internal SimpleMemberInvocationExpressionInfo WithName(string name)
        {
            MemberAccessExpressionSyntax newMemberAccess = MemberAccessExpression.WithName(SyntaxFactory.IdentifierName(name).WithTriviaFrom(Name));

            InvocationExpressionSyntax newInvocation = InvocationExpression.WithExpression(newMemberAccess);

            return new SimpleMemberInvocationExpressionInfo(newInvocation, newMemberAccess);
        }
    }
}
