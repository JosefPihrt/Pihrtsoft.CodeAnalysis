// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp;
using Roslynator.Formatting.CSharp;

namespace Roslynator.Formatting.CodeFixes.LineIsTooLong
{
    internal class WrapLineNodeFinder
    {
        private Dictionary<SyntaxGroup, SyntaxNode> _nodes;
        private readonly HashSet<SyntaxNode> _processedNodes;

        public Document Document { get; }

        public SemanticModel SemanticModel { get; }

        public TextSpan Span { get; }

        public int MaxLineLength { get; }

        public WrapLineNodeFinder(Document document, SemanticModel semanticModel, TextSpan span, int maxLineLength)
        {
            Document = document;
            SemanticModel = semanticModel;
            Span = span;
            MaxLineLength = maxLineLength;

            _processedNodes = new HashSet<SyntaxNode>();
        }

        public SyntaxNode FindNodeToFix(SyntaxNode root)
        {
            int position = Span.End;

            while (position >= Span.Start)
            {
                SyntaxToken token = root.FindToken(position);

                for (SyntaxNode node = token.Parent; node?.SpanStart >= Span.Start; node = node.Parent)
                {
                    if (_processedNodes.Contains(node))
                        continue;

                    if (TryGetSyntaxGroup(node, out SyntaxGroup syntaxGroup)
                        && ShouldAnalyze(node, syntaxGroup))
                    {
                        SyntaxNode fixableNode = GetFixableNode(node);

                        if (fixableNode != null)
                            (_nodes ??= new Dictionary<SyntaxGroup, SyntaxNode>())[syntaxGroup] = node;
                    }

                    _processedNodes.Add(node);
                    break;
                }

                position = Math.Min(position, token.FullSpan.Start) - 1;
            }

            if (_nodes == null)
                return null;

            if (TryGetNode(SyntaxGroup.ArgumentList, out SyntaxNode argumentList)
                && TryGetNode(SyntaxGroup.MemberExpression, out SyntaxNode memberExpression))
            {
                SyntaxNode argumentListOrMemberExpression = ChooseBetweenArgumentListAndMemberExpression(
                    argumentList,
                    memberExpression);

                _nodes.Remove((GetSyntaxGroup(argumentListOrMemberExpression) == SyntaxGroup.ArgumentList)
                    ? SyntaxGroup.MemberExpression
                    : SyntaxGroup.ArgumentList);
            }

            List<SyntaxGroup> nodes = _nodes
                .Where(f =>
                {
                    switch (f.Key)
                    {
                        case SyntaxGroup.BinaryExpression:
                        case SyntaxGroup.MemberExpression:
                        case SyntaxGroup.ArgumentList:
                            return true;
                        default:
                            return false;
                    }
                })
                .Select(f => f.Key)
                .ToList();

            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                for (int j = i; j >= 0; j--)
                {
                    if (nodes[i] != nodes[j])
                    {
                        SyntaxNode nodeToRemove = ChooseNode(_nodes[nodes[i]], _nodes[nodes[j]]);

                        _nodes.Remove(GetSyntaxGroup(nodeToRemove));
                    }
                }
            }

            if (TryGetNode(SyntaxGroup.AssignmentExpression, out SyntaxNode assignmentNode))
            {
                var assignmentExpression = (AssignmentExpressionSyntax)assignmentNode;

                foreach (KeyValuePair<SyntaxGroup, SyntaxNode> kvp in _nodes.ToList())
                {
                    if (assignmentExpression.Left.Contains(kvp.Value))
                        _nodes.Remove(kvp.Key);
                }
            }

            return _nodes
                .Select(f => f.Value)
                .OrderBy(f => f, SyntaxKindComparer.Instance)
                .First();
        }

        private SyntaxNode GetFixableNode(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.ArrowExpressionClause:
                    {
                        var expressionBody = (ArrowExpressionClauseSyntax)node;

                        SyntaxToken arrowToken = expressionBody.ArrowToken;
                        SyntaxToken previousToken = arrowToken.GetPreviousToken();

                        if (previousToken.SpanStart < Span.Start)
                            return null;

                        bool addNewLineAfter = AnalyzerOptions.AddNewLineAfterExpressionBodyArrowInsteadOfBeforeIt.IsEnabled(Document, node);

                        int wrapPosition = (addNewLineAfter) ? arrowToken.Span.End : previousToken.Span.End;
                        int start = (addNewLineAfter) ? expressionBody.Expression.SpanStart : arrowToken.SpanStart;
                        int longestLength = expressionBody.GetLastToken().GetNextToken().Span.End - start;

                        if (!CanWrap(expressionBody, wrapPosition, longestLength))
                            return null;

                        return expressionBody;
                    }
                case SyntaxKind.EqualsValueClause:
                    {
                        var equalsValueClause = (EqualsValueClauseSyntax)node;

                        SyntaxToken equalsToken = equalsValueClause.EqualsToken;
                        SyntaxToken previousToken = equalsToken.GetPreviousToken();

                        if (previousToken.SpanStart < Span.Start)
                            return null;

                        bool addNewLineAfter = AnalyzerOptions.AddNewLineAfterEqualsSignInsteadOfBeforeIt.IsEnabled(Document, node);

                        int wrapPosition = (addNewLineAfter) ? equalsToken.Span.End : previousToken.Span.End;
                        int start = (addNewLineAfter) ? equalsValueClause.Value.SpanStart : equalsToken.SpanStart;
                        int longestLength = Span.End - start;

                        if (!CanWrap(equalsValueClause, wrapPosition, longestLength))
                            return null;

                        return equalsValueClause;
                    }
                case SyntaxKind.AttributeList:
                    {
                        var attributeList = (AttributeListSyntax)node;

                        if (!CanWrap(attributeList.Attributes, attributeList.OpenBracketToken.Span.End, 2))
                            return null;

                        return attributeList;
                    }
                case SyntaxKind.BaseList:
                    {
                        var baseList = (BaseListSyntax)node;

                        if (!CanWrap(baseList.Types, baseList.ColonToken.Span.End, 2))
                            return null;

                        return baseList;
                    }
                case SyntaxKind.ParameterList:
                    {
                        if (node.Parent is AnonymousFunctionExpressionSyntax)
                            return null;

                        var parameterList = (ParameterListSyntax)node;

                        if (!CanWrap(parameterList.Parameters, parameterList.OpenParenToken.Span.End))
                            return null;

                        return parameterList;
                    }
                case SyntaxKind.BracketedParameterList:
                    {
                        var parameterList = (BracketedParameterListSyntax)node;

                        if (!CanWrap(parameterList.Parameters, parameterList.OpenBracketToken.Span.End))
                            return null;

                        return parameterList;
                    }
                case SyntaxKind.ForStatement:
                    {
                        var forStatement = (ForStatementSyntax)node;

                        if (forStatement.CloseParenToken.Span.End > Span.End)
                            return null;

                        if (forStatement.OpenParenToken.Span.End - Span.Start > MaxLineLength)
                            return null;

                        int longestLength = Math.Max(
                            forStatement.FirstSemicolonToken.Span.End - forStatement.Declaration.SpanStart,
                            Math.Max(
                                forStatement.SecondSemicolonToken.Span.End - forStatement.Condition.SpanStart,
                                forStatement.CloseParenToken.Span.End - forStatement.Incrementors.Span.Start));

                        int indentationLength = SyntaxTriviaAnalysis.GetIncreasedIndentationLength(node);

                        if (indentationLength + longestLength > MaxLineLength)
                            return null;

                        return forStatement;
                    }
                case SyntaxKind.ArgumentList:
                    {
                        var argumentList = (ArgumentListSyntax)node;

                        if (argumentList.Arguments.Count == 1
                            && argumentList.Parent is InvocationExpressionSyntax invocationExpression
                            && invocationExpression.Expression is IdentifierNameSyntax identifierName
                            && identifierName.Identifier.ValueText == "nameof")
                        {
                            return null;
                        }

                        if (!CanWrap(argumentList.Arguments, argumentList.OpenParenToken.Span.End))
                            return null;

                        return argumentList;
                    }
                case SyntaxKind.BracketedArgumentList:
                    {
                        var argumentList = (BracketedArgumentListSyntax)node;

                        if (!CanWrap(argumentList.Arguments, argumentList.OpenBracketToken.Span.End))
                            return null;

                        return argumentList;
                    }
                case SyntaxKind.AttributeArgumentList:
                    {
                        var argumentList = (AttributeArgumentListSyntax)node;

                        if (!CanWrap(argumentList.Arguments, argumentList.OpenParenToken.Span.End))
                            return null;

                        return argumentList;
                    }
                case SyntaxKind.SimpleMemberAccessExpression:
                    {
                        var memberAccessExpression = (MemberAccessExpressionSyntax)node;

                        if (!CanWrap(memberAccessExpression))
                            return null;

                        SyntaxToken dotToken = memberAccessExpression.OperatorToken;

                        if (!CanWrap(memberAccessExpression, dotToken.SpanStart, Span.End - dotToken.SpanStart))
                            return null;

                        return memberAccessExpression;
                    }
                case SyntaxKind.MemberBindingExpression:
                    {
                        var memberBindingExpression = (MemberBindingExpressionSyntax)node;

                        SyntaxToken dotToken = memberBindingExpression.OperatorToken;

                        if (!CanWrap(memberBindingExpression, dotToken.SpanStart, Span.End - dotToken.SpanStart))
                            return null;

                        return memberBindingExpression;
                    }
                case SyntaxKind.ConditionalExpression:
                    {
                        var conditionalExpression = (ConditionalExpressionSyntax)node;

                        SyntaxToken questionToken = conditionalExpression.QuestionToken;
                        SyntaxToken colonToken = conditionalExpression.ColonToken;

                        bool addNewLineAfter = AnalyzerOptions.AddNewLineAfterConditionalOperatorInsteadOfBeforeIt.IsEnabled(Document, node);

                        int wrapPosition = (addNewLineAfter)
                            ? questionToken.Span.End
                            : conditionalExpression.Condition.Span.End;

                        int start = (addNewLineAfter) ? conditionalExpression.WhenTrue.SpanStart : questionToken.SpanStart;
                        int end = (addNewLineAfter) ? colonToken.Span.End : conditionalExpression.WhenTrue.Span.End;
                        int longestLength = end - start;

                        start = (addNewLineAfter) ? conditionalExpression.WhenFalse.SpanStart : colonToken.SpanStart;
                        int longestLength2 = Span.End - start;

                        if (!CanWrap(conditionalExpression, wrapPosition, Math.Max(longestLength, longestLength2)))
                            return null;

                        return conditionalExpression;
                    }
                case SyntaxKind.ArrayInitializerExpression:
                case SyntaxKind.CollectionInitializerExpression:
                case SyntaxKind.ComplexElementInitializerExpression:
                case SyntaxKind.ObjectInitializerExpression:
                    {
                        var initializer = (InitializerExpressionSyntax)node;

                        if (!CanWrap(initializer.Expressions, initializer.OpenBraceToken.Span.End))
                            return null;

                        return initializer;
                    }
                case SyntaxKind.AddExpression:
                case SyntaxKind.SubtractExpression:
                case SyntaxKind.MultiplyExpression:
                case SyntaxKind.DivideExpression:
                case SyntaxKind.ModuloExpression:
                case SyntaxKind.LeftShiftExpression:
                case SyntaxKind.RightShiftExpression:
                case SyntaxKind.LogicalOrExpression:
                case SyntaxKind.LogicalAndExpression:
                case SyntaxKind.BitwiseOrExpression:
                case SyntaxKind.BitwiseAndExpression:
                case SyntaxKind.ExclusiveOrExpression:
                case SyntaxKind.CoalesceExpression:
                case SyntaxKind.AsExpression:
                case SyntaxKind.IsExpression:
                case SyntaxKind.EqualsExpression:
                case SyntaxKind.NotEqualsExpression:
                case SyntaxKind.LessThanExpression:
                case SyntaxKind.LessThanOrEqualExpression:
                case SyntaxKind.GreaterThanExpression:
                case SyntaxKind.GreaterThanOrEqualExpression:
                    {
                        var binaryExpression = (BinaryExpressionSyntax)node;

                        SyntaxToken operatorToken = binaryExpression.OperatorToken;

                        bool addNewLineAfter = AnalyzerOptions.AddNewLineAfterBinaryOperatorInsteadOfBeforeIt.IsEnabled(Document, node);

                        int wrapPosition = (addNewLineAfter)
                            ? operatorToken.Span.End
                            : binaryExpression.Left.Span.End;

                        int longestLength = 0;

                        while (true)
                        {
                            BinaryExpressionSyntax parentBinaryExpression = null;
                            if (binaryExpression.Parent.IsKind(binaryExpression.Kind()))
                            {
                                parentBinaryExpression = (BinaryExpressionSyntax)binaryExpression.Parent;
                            }

                            int end;
                            if (parentBinaryExpression != null)
                            {
                                end = (addNewLineAfter)
                                    ? parentBinaryExpression.OperatorToken.Span.End
                                    : binaryExpression.Right.Span.End;
                            }
                            else
                            {
                                end = Span.End;
                            }

                            int start = (addNewLineAfter)
                                ? binaryExpression.Right.SpanStart
                                : binaryExpression.OperatorToken.SpanStart;

                            longestLength = Math.Max(longestLength, end - start);

                            if (parentBinaryExpression == null)
                                break;

                            binaryExpression = parentBinaryExpression;
                        }

                        if (!CanWrap(node, wrapPosition, longestLength))
                            return null;

                        return node;
                    }
                case SyntaxKind.AddAssignmentExpression:
                case SyntaxKind.AndAssignmentExpression:
                case SyntaxKind.CoalesceAssignmentExpression:
                case SyntaxKind.DivideAssignmentExpression:
                case SyntaxKind.ExclusiveOrAssignmentExpression:
                case SyntaxKind.LeftShiftAssignmentExpression:
                case SyntaxKind.ModuloAssignmentExpression:
                case SyntaxKind.MultiplyAssignmentExpression:
                case SyntaxKind.OrAssignmentExpression:
                case SyntaxKind.RightShiftAssignmentExpression:
                case SyntaxKind.SimpleAssignmentExpression:
                case SyntaxKind.SubtractAssignmentExpression:
                    {
                        var assignment = (AssignmentExpressionSyntax)node;

                        SyntaxToken operatorToken = assignment.OperatorToken;
                        SyntaxNode left = assignment.Left;

                        if (left.SpanStart < Span.Start)
                            return null;

                        bool addNewLineAfter = AnalyzerOptions.AddNewLineAfterEqualsSignInsteadOfBeforeIt.IsEnabled(Document, node);

                        int wrapPosition = (addNewLineAfter) ? operatorToken.Span.End : left.Span.End;
                        int start = (addNewLineAfter) ? assignment.Right.SpanStart : operatorToken.SpanStart;
                        int longestLength = Span.End - start;

                        if (!CanWrap(assignment, wrapPosition, longestLength))
                            return null;

                        return assignment;
                    }
            }

            return null;
        }

        private static SyntaxGroup GetSyntaxGroup(SyntaxNode node)
        {
            if (TryGetSyntaxGroup(node, out SyntaxGroup syntaxGroup))
                return syntaxGroup;

            throw new ArgumentException("", nameof(node));
        }

        private static bool TryGetSyntaxGroup(SyntaxNode node, out SyntaxGroup syntaxGroup)
        {
            SyntaxKind kind = node.Kind();

            if (SyntaxGroupMap.Value.TryGetValue(kind, out syntaxGroup))
                return true;

            if (kind == SyntaxKind.EqualsValueClause)
            {
                SyntaxNode parent = node.Parent;

                if (parent.IsKind(SyntaxKind.PropertyDeclaration))
                {
                    syntaxGroup = SyntaxGroup.PropertyInitializer;
                    return true;
                }

                if (parent.IsKind(SyntaxKind.VariableDeclarator))
                {
                    parent = parent.Parent;

                    if (parent.IsKind(SyntaxKind.VariableDeclaration))
                    {
                        parent = parent.Parent;

                        if (parent.IsKind(SyntaxKind.FieldDeclaration, SyntaxKind.LocalDeclarationStatement))
                        {
                            syntaxGroup = SyntaxGroup.FieldOrLocalInitializer;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool TryGetNode(SyntaxGroup syntaxGroup, out SyntaxNode node)
        {
            if (_nodes != null)
            {
                return _nodes.TryGetValue(syntaxGroup, out node);
            }
            else
            {
                node = null;
                return false;
            }
        }

        private bool ShouldAnalyze(SyntaxNode node, SyntaxGroup syntaxGroup)
        {
            switch (syntaxGroup)
            {
                case SyntaxGroup.MemberExpression:
                case SyntaxGroup.ArgumentList:
                case SyntaxGroup.InitializerExpression:
                case SyntaxGroup.BinaryExpression:
                case SyntaxGroup.Is_As_EqualityExpression:
                case SyntaxGroup.ConditionalExpression:
                    {
                        if (IsInsideInterpolation(node.Parent))
                            return false;

                        break;
                    }
            }

            if (_nodes == null)
                return true;

            foreach (KeyValuePair<SyntaxGroup, SyntaxNode> kvp in _nodes)
            {
                switch (kvp.Key)
                {
                    case SyntaxGroup.ConditionalExpression:
                    case SyntaxGroup.InitializerExpression:
                    case SyntaxGroup.BinaryExpression:
                    case SyntaxGroup.MemberExpression:
                    case SyntaxGroup.ArgumentList:
                        {
                            if (kvp.Value.FullSpan.Contains(node.FullSpan))
                                return false;

                            break;
                        }
                }
            }

            if (!_nodes.TryGetValue(syntaxGroup, out SyntaxNode node2))
                return true;

            if (node.FullSpan.Contains(node2.FullSpan))
                return true;

            return false;

            static bool IsInsideInterpolation(SyntaxNode node)
            {
                for (SyntaxNode n = node; n != null; n = n.Parent)
                {
                    switch (n)
                    {
                        case MemberDeclarationSyntax _:
                        case StatementSyntax _:
                        case AccessorDeclarationSyntax _:
                            return false;
                        case InterpolationSyntax _:
                            return true;
                    }
                }

                return false;
            }
        }

        private bool CanWrap<TNode>(
            SeparatedSyntaxList<TNode> nodes,
            int wrapPosition,
            int minCount = 1) where TNode : SyntaxNode
        {
            if (nodes.Count < minCount)
                return false;

            int longestLength = nodes.Max(f => f.Span.Length);

            return CanWrap(nodes[0], wrapPosition, longestLength);
        }

        private bool CanWrap(
            SyntaxNode node,
            int wrapPosition,
            int longestLength)
        {
            if (wrapPosition - Span.Start > MaxLineLength)
                return false;

            int indentationLength = SyntaxTriviaAnalysis.GetIncreasedIndentationLength(node);

            return indentationLength + longestLength <= MaxLineLength;
        }

        private bool CanWrap(MemberAccessExpressionSyntax memberAccessExpression)
        {
            switch (memberAccessExpression.Expression.Kind())
            {
                case SyntaxKind.ThisExpression:
                case SyntaxKind.BaseExpression:
                    {
                        return false;
                    }
                case SyntaxKind.IdentifierName:
                case SyntaxKind.SimpleMemberAccessExpression:
                case SyntaxKind.AliasQualifiedName:
                    {
                        ISymbol symbol = SemanticModel.GetSymbol(memberAccessExpression);

                        if (symbol != null)
                        {
                            if (symbol.IsKind(SymbolKind.Namespace))
                                return false;

                            if (symbol.IsKind(SymbolKind.Field)
                                && symbol.ContainingType?.TypeKind == TypeKind.Enum)
                            {
                                return false;
                            }
                        }

                        symbol = SemanticModel.GetSymbol(memberAccessExpression.Expression);

                        if (symbol.IsKind(SymbolKind.Namespace))
                            return false;

                        break;
                    }
            }

            return true;
        }

        private static SyntaxNode ChooseNode(SyntaxNode node1, SyntaxNode node2)
        {
            if (node1.FullSpan.Contains(node2.FullSpan))
                return node2;

            if (node2.FullSpan.Contains(node1.FullSpan))
                return node1;

            if (node1.SpanStart > node2.SpanStart)
                return node2;

            return node1;
        }

        private SyntaxNode ChooseBetweenArgumentListAndMemberExpression(SyntaxNode argumentList, SyntaxNode memberExpression)
        {
            if (argumentList.FullSpan.Contains(memberExpression.FullSpan))
                return argumentList;

            if (memberExpression.FullSpan.Contains(argumentList.FullSpan))
                return memberExpression;

            if (memberExpression.Span.End == argumentList.SpanStart)
            {
                SyntaxToken dotToken = (memberExpression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                    ? ((MemberAccessExpressionSyntax)memberExpression).OperatorToken
                    : ((MemberBindingExpressionSyntax)memberExpression).OperatorToken;

                var expression = (ExpressionSyntax)memberExpression;

                if (memberExpression is MemberBindingExpressionSyntax memberBinding)
                {
                    SyntaxToken token = memberBinding.OperatorToken.GetPreviousToken();

                    if (token.IsKind(SyntaxKind.QuestionToken)
                        && token.FullSpan.End == memberBinding.OperatorToken.SpanStart
                        && token.Parent.IsKind(SyntaxKind.ConditionalAccessExpression))
                    {
                        expression = (ExpressionSyntax)token.Parent;
                    }
                }

                foreach (SyntaxNode node in new MethodChain(expression))
                {
                    SyntaxKind kind = node.Kind();

                    if (kind == SyntaxKind.SimpleMemberAccessExpression)
                    {
                        if (((MemberAccessExpressionSyntax)node).OperatorToken.SpanStart < dotToken.SpanStart)
                            return memberExpression;
                    }
                    else if (kind == SyntaxKind.MemberBindingExpression)
                    {
                        if (((MemberBindingExpressionSyntax)node).OperatorToken.SpanStart < dotToken.SpanStart)
                            return memberExpression;
                    }
                }

                return argumentList;
            }

            return memberExpression;
        }

        private class SyntaxKindComparer : IComparer<SyntaxNode>
        {
            public static SyntaxKindComparer Instance { get; } = new SyntaxKindComparer();

            public int Compare(SyntaxNode x, SyntaxNode y)
            {
                if (object.ReferenceEquals(x, y))
                    return 0;

                if (x == null)
                    return -1;

                if (y == null)
                    return 1;

                return GetSyntaxGroup(x).CompareTo(GetSyntaxGroup(y));
            }
        }
    }
}

