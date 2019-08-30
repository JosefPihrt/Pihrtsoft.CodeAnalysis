// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp.SyntaxRewriters;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp
{
    internal static class SyntaxFormatter
    {
        public static Task<Document> ToSingleLineAsync<TNode>(
            Document document,
            TNode node,
            CancellationToken cancellationToken = default(CancellationToken)) where TNode : SyntaxNode
        {
            TextSpan span = (node.GetTrailingTrivia().IsEmptyOrWhitespace())
                ? TextSpan.FromBounds(node.SpanStart, node.FullSpan.End)
                : node.Span;

            TNode newNode = node.ReplaceWhitespace(ElasticSpace, span).WithFormatterAnnotation();

            return document.ReplaceNodeAsync(node, newNode, cancellationToken);
        }

        public static Task<Document> ToSingleLineAsync<TNode>(
            Document document,
            TNode node,
            TextSpan span,
            CancellationToken cancellationToken = default(CancellationToken)) where TNode : SyntaxNode
        {
            TNode newNode = node.ReplaceWhitespace(ElasticSpace, span).WithFormatterAnnotation();

            return document.ReplaceNodeAsync(node, newNode, cancellationToken);
        }

        public static Task<Document> ToSingleLineAsync(
            Document document,
            InitializerExpressionSyntax initializer,
            bool removeTrailingComma = false,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            InitializerExpressionSyntax newInitializer = initializer
                .ReplaceWhitespace(ElasticSpace, TextSpan.FromBounds(initializer.SpanStart, initializer.Span.End))
                .WithFormatterAnnotation();

            newInitializer = newInitializer.WithOpenBraceToken(newInitializer.OpenBraceToken.WithoutLeadingTrivia().WithTrailingTrivia(Space));

            newInitializer = newInitializer.WithCloseBraceToken(newInitializer.CloseBraceToken.WithoutLeadingTrivia());

            SeparatedSyntaxList<ExpressionSyntax> expressions = newInitializer.Expressions;

            if (expressions.Any())
            {
                ExpressionSyntax firstExpression = expressions[0];

                newInitializer = newInitializer.WithExpressions(expressions.Replace(firstExpression, firstExpression.WithoutLeadingTrivia()));

                expressions = newInitializer.Expressions;

                SyntaxToken trailingComma = expressions.GetTrailingSeparator();

                if (trailingComma.IsKind(SyntaxKind.CommaToken))
                {
                    if (removeTrailingComma)
                    {
                        expressions = expressions.ReplaceSeparator(trailingComma, MissingToken(SyntaxKind.CommaToken));

                        ExpressionSyntax lastExpression = expressions.Last();

                        expressions = expressions.Replace(lastExpression, lastExpression.WithTrailingTrivia(Space));

                        newInitializer = newInitializer.WithExpressions(expressions);
                    }
                    else
                    {
                        newInitializer = newInitializer.WithExpressions(expressions.ReplaceSeparator(trailingComma, trailingComma.WithTrailingTrivia(Space)));
                    }
                }
                else
                {
                    ExpressionSyntax lastExpression = expressions.Last();

                    newInitializer = newInitializer.WithExpressions(expressions.Replace(lastExpression, lastExpression.WithTrailingTrivia(Space)));
                }
            }

            SyntaxNode parent = initializer.Parent;

            SyntaxNode newParent;

            switch (parent.Kind())
            {
                case SyntaxKind.ObjectCreationExpression:
                    {
                        var expression = (ObjectCreationExpressionSyntax)parent;

                        expression = expression.WithInitializer(newInitializer);

                        ArgumentListSyntax argumentList = expression.ArgumentList;

                        if (argumentList != null)
                        {
                            newParent = expression.WithArgumentList(argumentList.WithTrailingTrivia(Space));
                        }
                        else
                        {
                            newParent = expression.WithType(expression.Type.WithTrailingTrivia(Space));
                        }

                        break;
                    }
                case SyntaxKind.ArrayCreationExpression:
                    {
                        var expression = (ArrayCreationExpressionSyntax)parent;

                        newParent = expression
                            .WithInitializer(newInitializer)
                            .WithType(expression.Type.WithTrailingTrivia(Space));

                        break;
                    }
                case SyntaxKind.ImplicitArrayCreationExpression:
                    {
                        var expression = (ImplicitArrayCreationExpressionSyntax)parent;

                        newParent = expression
                            .WithInitializer(newInitializer)
                            .WithCloseBracketToken(expression.CloseBracketToken.WithTrailingTrivia(Space));

                        break;
                    }
                case SyntaxKind.EqualsValueClause:
                    {
                        var equalsValueClause = (EqualsValueClauseSyntax)parent;

                        newParent = equalsValueClause
                            .WithValue(newInitializer)
                            .WithEqualsToken(equalsValueClause.EqualsToken.WithTrailingTrivia(Space));

                        break;
                    }
                case SyntaxKind.SimpleAssignmentExpression:
                    {
                        var simpleAssignment = (AssignmentExpressionSyntax)parent;

                        newParent = simpleAssignment
                            .WithRight(newInitializer)
                            .WithOperatorToken(simpleAssignment.OperatorToken.WithTrailingTrivia(Space));

                        break;
                    }
                default:
                    {
                        Debug.Fail(parent.Kind().ToString());

                        return document.ReplaceNodeAsync(initializer, newInitializer, cancellationToken);
                    }
            }

            return document.ReplaceNodeAsync(parent, newParent, cancellationToken);
        }

        public static Task<Document> ToMultiLineAsync(
            Document document,
            ConditionalExpressionSyntax conditionalExpression,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ConditionalExpressionSyntax newNode = ToMultiLine(conditionalExpression, cancellationToken);

            return document.ReplaceNodeAsync(conditionalExpression, newNode, cancellationToken);
        }

        public static ConditionalExpressionSyntax ToMultiLine(ConditionalExpressionSyntax conditionalExpression, CancellationToken cancellationToken = default(CancellationToken))
        {
            SyntaxTriviaList leadingTrivia = conditionalExpression.GetIncreasedIndentation(cancellationToken);

            leadingTrivia = leadingTrivia.Insert(0, NewLine());

            return ConditionalExpression(
                    conditionalExpression.Condition.WithoutTrailingTrivia(),
                    Token(leadingTrivia, SyntaxKind.QuestionToken, TriviaList(Space)),
                    conditionalExpression.WhenTrue.WithoutTrailingTrivia(),
                    Token(leadingTrivia, SyntaxKind.ColonToken, TriviaList(Space)),
                    conditionalExpression.WhenFalse.WithoutTrailingTrivia());
        }

        public static Task<Document> ToMultiLineAsync(
            Document document,
            ParameterListSyntax parameterList,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ParameterListSyntax newNode = ToMultiLine(parameterList, cancellationToken);

            return document.ReplaceNodeAsync(parameterList, newNode, cancellationToken);
        }

        public static ParameterListSyntax ToMultiLine(ParameterListSyntax parameterList, CancellationToken cancellationToken = default(CancellationToken))
        {
            SyntaxTriviaList leadingTrivia = parameterList.GetIncreasedIndentation(cancellationToken);

            var nodesAndTokens = new List<SyntaxNodeOrToken>();

            SeparatedSyntaxList<ParameterSyntax>.Enumerator en = parameterList.Parameters.GetEnumerator();

            if (en.MoveNext())
            {
                nodesAndTokens.Add(en.Current.WithLeadingTrivia(leadingTrivia));

                while (en.MoveNext())
                {
                    nodesAndTokens.Add(CommaToken().WithTrailingTrivia(NewLine()));

                    nodesAndTokens.Add(en.Current.WithLeadingTrivia(leadingTrivia));
                }
            }

            return ParameterList(
                OpenParenToken().WithTrailingTrivia(NewLine()),
                SeparatedList<ParameterSyntax>(nodesAndTokens),
                parameterList.CloseParenToken);
        }

        public static Task<Document> ToMultiLineAsync(
            Document document,
            InitializerExpressionSyntax initializer,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            InitializerExpressionSyntax newNode = ToMultiLine(initializer, cancellationToken)
                .WithFormatterAnnotation();

            return document.ReplaceNodeAsync(initializer, newNode, cancellationToken);
        }

        public static InitializerExpressionSyntax ToMultiLine(InitializerExpressionSyntax initializer, CancellationToken cancellationToken)
        {
            SyntaxNode parent = initializer.Parent;

            if (parent.IsKind(SyntaxKind.ObjectCreationExpression)
                && !initializer.IsKind(SyntaxKind.CollectionInitializerExpression))
            {
                return initializer
                    .WithExpressions(
                        SeparatedList(
                            initializer.Expressions.Select(expression => expression.WithLeadingTrivia(NewLine()))));
            }
            else
            {
                SyntaxTrivia trivia = initializer.GetIndentation(cancellationToken);

                SyntaxTriviaList braceTrivia = TriviaList(NewLine(), trivia);
                SyntaxTriviaList expressionTrivia = TriviaList(NewLine(), trivia, SingleIndentation(trivia));

                return initializer
                    .WithExpressions(
                        SeparatedList(
                            initializer.Expressions.Select(expression => expression.WithLeadingTrivia(expressionTrivia))))
                    .WithOpenBraceToken(initializer.OpenBraceToken.WithLeadingTrivia(braceTrivia))
                    .WithCloseBraceToken(initializer.CloseBraceToken.WithLeadingTrivia(braceTrivia));
            }
        }

        public static Task<Document> ToMultiLineAsync(
            Document document,
            ArgumentListSyntax argumentList,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ArgumentListSyntax newNode = ToMultiLine(argumentList, cancellationToken);

            return document.ReplaceNodeAsync(argumentList, newNode, cancellationToken);
        }

        public static ArgumentListSyntax ToMultiLine(ArgumentListSyntax argumentList, CancellationToken cancellationToken = default(CancellationToken))
        {
            SyntaxTriviaList leadingTrivia = argumentList.GetIncreasedIndentation(cancellationToken);

            var nodesAndTokens = new List<SyntaxNodeOrToken>();

            SeparatedSyntaxList<ArgumentSyntax>.Enumerator en = argumentList.Arguments.GetEnumerator();

            if (en.MoveNext())
            {
                nodesAndTokens.Add(en.Current
                    .TrimTrailingTrivia()
                    .WithLeadingTrivia(leadingTrivia));

                while (en.MoveNext())
                {
                    nodesAndTokens.Add(CommaToken().WithTrailingTrivia(NewLine()));

                    nodesAndTokens.Add(en.Current
                        .TrimTrailingTrivia()
                        .WithLeadingTrivia(leadingTrivia));
                }
            }

            return ArgumentList(
                OpenParenToken().WithTrailingTrivia(NewLine()),
                SeparatedList<ArgumentSyntax>(nodesAndTokens),
                argumentList.CloseParenToken.WithoutLeadingTrivia());
        }

        public static Task<Document> ToMultiLineAsync(
            Document document,
            ExpressionSyntax expression,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            SyntaxTrivia trivia = expression.GetIndentation(cancellationToken);

            string indentation = Environment.NewLine + trivia.ToString() + SingleIndentation(trivia).ToString();

            TextChange? textChange = null;
            List<TextChange> textChanges = null;

            foreach (SyntaxNode node in CSharpUtility.EnumerateExpressionChain(expression))
            {
                switch (node.Kind())
                {
                    case SyntaxKind.SimpleMemberAccessExpression:
                        {
                            var memberAccess = (MemberAccessExpressionSyntax)node;

                            if (memberAccess.Expression.IsKind(SyntaxKind.ThisExpression))
                                break;

                            if (semanticModel
                                .GetSymbol(memberAccess.Expression, cancellationToken)?
                                .Kind == SymbolKind.Namespace)
                            {
                                break;
                            }

                            AddTextChange(memberAccess.OperatorToken);
                            break;
                        }
                    case SyntaxKind.MemberBindingExpression:
                        {
                            var memberBinding = (MemberBindingExpressionSyntax)node;

                            AddTextChange(memberBinding.OperatorToken);
                            break;
                        }
                }
            }

            if (textChanges != null)
            {
                TextChange[] arr = textChanges.ToArray();
                Array.Reverse(arr);
                return document.WithTextChangesAsync(arr, cancellationToken);
            }
            else
            {
                return document.WithTextChangeAsync(textChange.Value, cancellationToken);
            }

            void AddTextChange(SyntaxToken operatorToken)
            {
                var tc = new TextChange(new TextSpan(operatorToken.SpanStart, 0), indentation);

                if (textChange == null)
                {
                    textChange = tc;
                }
                else
                {
                    (textChanges ?? (textChanges = new List<TextChange>() { textChange.Value })).Add(tc);
                }
            }
        }

        public static Task<Document> ToMultiLineAsync(
            Document document,
            AttributeArgumentListSyntax argumentList,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            AttributeArgumentListSyntax newNode = ToMultiLine(argumentList, cancellationToken);

            return document.ReplaceNodeAsync(argumentList, newNode, cancellationToken);
        }

        private static AttributeArgumentListSyntax ToMultiLine(AttributeArgumentListSyntax argumentList, CancellationToken cancellationToken = default(CancellationToken))
        {
            SyntaxTriviaList leadingTrivia = argumentList.GetIncreasedIndentation(cancellationToken);

            var nodesAndTokens = new List<SyntaxNodeOrToken>();

            SeparatedSyntaxList<AttributeArgumentSyntax>.Enumerator en = argumentList.Arguments.GetEnumerator();

            if (en.MoveNext())
            {
                nodesAndTokens.Add(en.Current
                    .TrimTrailingTrivia()
                    .WithLeadingTrivia(leadingTrivia));

                while (en.MoveNext())
                {
                    nodesAndTokens.Add(CommaToken().WithTrailingTrivia(NewLine()));

                    nodesAndTokens.Add(en.Current
                        .TrimTrailingTrivia()
                        .WithLeadingTrivia(leadingTrivia));
                }
            }

            return AttributeArgumentList(
                OpenParenToken().WithTrailingTrivia(NewLine()),
                SeparatedList<AttributeArgumentSyntax>(nodesAndTokens),
                argumentList.CloseParenToken.WithoutLeadingTrivia());
        }

        public static Task<Document> ToMultiLineAsync(
            Document document,
            BinaryExpressionSyntax condition,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            SyntaxTriviaList leadingTrivia = condition.GetIncreasedIndentation(cancellationToken);

            leadingTrivia = leadingTrivia.Insert(0, NewLine());

            var rewriter = new BinaryExpressionToMultiLineRewriter(leadingTrivia);

            var newCondition = (ExpressionSyntax)rewriter.Visit(condition);

            return document.ReplaceNodeAsync(condition, newCondition, cancellationToken);
        }

        public static Task<Document> ToMultiLineAsync(
            Document document,
            AccessorDeclarationSyntax accessor,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            AccessorDeclarationSyntax newAccessor = ToMultiLine(accessor);

            return document.ReplaceNodeAsync(accessor, newAccessor, cancellationToken);
        }

        private static AccessorDeclarationSyntax ToMultiLine(AccessorDeclarationSyntax accessor)
        {
            BlockSyntax body = accessor.Body;

            if (body != null)
            {
                SyntaxToken closeBrace = body.CloseBraceToken;

                if (!closeBrace.IsMissing)
                {
                    AccessorDeclarationSyntax newAccessor = accessor
                   .WithBody(
                       body.WithCloseBraceToken(
                           closeBrace.WithLeadingTrivia(
                               closeBrace.LeadingTrivia.Add(NewLine()))));

                    return newAccessor.WithFormatterAnnotation();
                }
            }

            return accessor;
        }
    }
}
