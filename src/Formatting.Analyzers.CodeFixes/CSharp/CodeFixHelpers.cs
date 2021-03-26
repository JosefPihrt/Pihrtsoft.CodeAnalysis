// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp;
using Roslynator.Formatting.CSharp;
using Roslynator.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.SyntaxTriviaAnalysis;
using static Roslynator.Formatting.CSharp.FixFormattingOfListAnalyzer;

namespace Roslynator.Formatting.CodeFixes.CSharp
{
    internal static class CodeFixHelpers
    {
        public static Task<Document> AppendEndOfLineAsync(
            Document document,
            SyntaxToken token,
            CancellationToken cancellationToken = default)
        {
            SyntaxToken newToken = token.AppendEndOfLineToTrailingTrivia();

            return document.ReplaceTokenAsync(token, newToken, cancellationToken);
        }

        public static Task<Document> AppendEndOfLineAsync(
            Document document,
            SyntaxNode node,
            CancellationToken cancellationToken = default)
        {
            SyntaxNode newNode = node.AppendEndOfLineToTrailingTrivia();

            return document.ReplaceNodeAsync(node, newNode, cancellationToken);
        }

        public static Task<Document> AddNewLineBeforeAsync(
            Document document,
            SyntaxToken token,
            CancellationToken cancellationToken = default)
        {
            SyntaxTrivia indentation = DetermineIndentation(token.Parent, cancellationToken);

            return AddNewLineBeforeAsync(
                document,
                token,
                indentation.ToString(),
                cancellationToken);
        }

        public static Task<Document> AddNewLineAfterAsync(
            Document document,
            SyntaxToken token,
            CancellationToken cancellationToken = default)
        {
            SyntaxTrivia indentation = DetermineIndentation(token.Parent, cancellationToken);

            return AddNewLineAfterAsync(
                document,
                token,
                indentation.ToString(),
                cancellationToken);
        }

        public static Task<Document> AddNewLineBeforeAndIncreaseIndentationAsync(
            Document document,
            SyntaxToken token,
            CancellationToken cancellationToken = default)
        {
            return AddNewLineBeforeAndIncreaseIndentationAsync(
                document,
                token,
                AnalyzeIndentation(token.Parent, cancellationToken),
                cancellationToken);
        }

        public static Task<Document> AddNewLineBeforeAndIncreaseIndentationAsync(
            Document document,
            SyntaxToken token,
            IndentationAnalysis indentation,
            CancellationToken cancellationToken = default)
        {
            return AddNewLineBeforeAsync(
                document,
                token,
                indentation.GetIncreasedIndentation(),
                cancellationToken);
        }

        public static Task<Document> AddNewLineAfterAndIncreaseIndentationAsync(
            Document document,
            SyntaxToken token,
            CancellationToken cancellationToken = default)
        {
            return AddNewLineAfterAndIncreaseIndentationAsync(
                document,
                token,
                AnalyzeIndentation(token.Parent, cancellationToken),
                cancellationToken);
        }

        public static Task<Document> AddNewLineAfterAndIncreaseIndentationAsync(
            Document document,
            SyntaxToken token,
            IndentationAnalysis indentation,
            CancellationToken cancellationToken = default)
        {
            return AddNewLineAfterAsync(
                document,
                token,
                indentation.GetIncreasedIndentation(),
                cancellationToken);
        }

        public static Task<Document> AddNewLineBeforeAsync(
            Document document,
            SyntaxToken token,
            string indentation,
            CancellationToken cancellationToken = default)
        {
            return document.WithTextChangeAsync(
                GetNewLineBeforeTextChange(token, indentation),
                cancellationToken);
        }

        public static TextChange GetNewLineBeforeTextChange(SyntaxToken token, string indentation)
        {
            return new TextChange(
                TextSpan.FromBounds(token.GetPreviousToken().Span.End, token.SpanStart),
                DetermineEndOfLine(token).ToString() + indentation);
        }

        public static Task<Document> AddNewLineAfterAsync(
            Document document,
            SyntaxToken token,
            string indentation,
            CancellationToken cancellationToken = default)
        {
            return document.WithTextChangeAsync(
                GetNewLineAfterTextChange(token, indentation),
                cancellationToken);
        }

        public static TextChange GetNewLineAfterTextChange(SyntaxToken token, string indentation)
        {
            return new TextChange(
                TextSpan.FromBounds(token.Span.End, token.GetNextToken().SpanStart),
                DetermineEndOfLine(token).ToString() + indentation);
        }

        public static (ExpressionSyntax left, SyntaxToken token, ExpressionSyntax right)
            AddNewLineBeforeTokenInsteadOfAfterIt(
                ExpressionSyntax left,
                SyntaxToken token,
                ExpressionSyntax right)
        {
            return (
                left.WithTrailingTrivia(token.TrailingTrivia),
                Token(
                    right.GetLeadingTrivia(),
                    token.Kind(),
                    TriviaList(Space)),
                right.WithoutLeadingTrivia());
        }

        public static (ExpressionSyntax left, SyntaxToken token, ExpressionSyntax right)
            AddNewLineAfterTokenInsteadOfBeforeIt(
                ExpressionSyntax left,
                SyntaxToken token,
                ExpressionSyntax right)
        {
            return (
                left.WithTrailingTrivia(Space),
                Token(
                    SyntaxTriviaList.Empty,
                    token.Kind(),
                    left.GetTrailingTrivia()),
                right.WithLeadingTrivia(token.LeadingTrivia));
        }

        public static Task<Document> AddEmptyLineBeforeDirectiveAsync(
            Document document,
            DirectiveTriviaSyntax directiveTrivia,
            CancellationToken cancellationToken = default)
        {
            SyntaxTrivia parentTrivia = directiveTrivia.ParentTrivia;
            SyntaxToken token = parentTrivia.Token;
            SyntaxTriviaList leadingTrivia = token.LeadingTrivia;

            int index = leadingTrivia.IndexOf(parentTrivia);

            if (index > 0
                && leadingTrivia[index - 1].IsWhitespaceTrivia())
            {
                index--;
            }

            SyntaxTriviaList newLeadingTrivia = leadingTrivia.Insert(index, DetermineEndOfLine(token));

            SyntaxToken newToken = token.WithLeadingTrivia(newLeadingTrivia);

            return document.ReplaceTokenAsync(token, newToken, cancellationToken);
        }

        public static Task<Document> AddEmptyLineAfterDirectiveAsync(
            Document document,
            DirectiveTriviaSyntax directiveTrivia,
            CancellationToken cancellationToken = default)
        {
            SyntaxTrivia parentTrivia = directiveTrivia.ParentTrivia;
            SyntaxToken token = parentTrivia.Token;
            SyntaxTriviaList leadingTrivia = token.LeadingTrivia;

            int index = leadingTrivia.IndexOf(parentTrivia);

            SyntaxTriviaList newLeadingTrivia = leadingTrivia.Insert(index + 1, DetermineEndOfLine(token));

            SyntaxToken newToken = token.WithLeadingTrivia(newLeadingTrivia);

            return document.ReplaceTokenAsync(token, newToken, cancellationToken);
        }

        public static Task<Document> AddNewLineAfterInsteadOfBeforeAsync(
            Document document,
            SyntaxToken token,
            CancellationToken cancellationToken = default)
        {
            ExpressionSyntax right = FindNextExpression(token);

            return AddNewLineAfterInsteadOfBeforeAsync(document, token.GetPreviousToken(), token, right, cancellationToken);
        }

        public static Task<Document> AddNewLineAfterInsteadOfBeforeAsync(
            Document document,
            SyntaxNodeOrToken left,
            SyntaxNodeOrToken middle,
            SyntaxNodeOrToken right,
            CancellationToken cancellationToken = default)
        {
            StringBuilder sb = StringBuilderCache.GetInstance();

            sb.Append(" ");
            sb.Append(middle.ToString());

            SyntaxTriviaList trailingTrivia = left.GetTrailingTrivia();

            if (IsOptionalWhitespaceThenEndOfLineTrivia(trailingTrivia))
            {
                sb.Append(DetermineEndOfLine(left).ToString());
            }
            else
            {
                sb.Append(trailingTrivia.ToString());
            }

            sb.Append(middle.GetLeadingTrivia().ToString());

            return document.WithTextChangeAsync(
                TextSpan.FromBounds(left.Span.End, right.SpanStart),
                StringBuilderCache.GetStringAndFree(sb),
                cancellationToken);
        }

        public static Task<Document> AddNewLineBeforeInsteadOfAfterAsync(
            Document document,
            SyntaxToken token,
            CancellationToken cancellationToken = default)
        {
            ExpressionSyntax right = FindNextExpression(token);

            return AddNewLineBeforeInsteadOfAfterAsync(document, token.GetPreviousToken(), token, right, cancellationToken);
        }

        public static Task<Document> AddNewLineBeforeInsteadOfAfterAsync(
            Document document,
            SyntaxNodeOrToken left,
            SyntaxNodeOrToken middle,
            SyntaxNodeOrToken right,
            CancellationToken cancellationToken = default)
        {
            StringBuilder sb = StringBuilderCache.GetInstance();

            SyntaxTriviaList trailingTrivia = middle.GetTrailingTrivia();

            if (IsOptionalWhitespaceThenEndOfLineTrivia(trailingTrivia))
            {
                sb.Append(DetermineEndOfLine(middle).ToString());
            }
            else
            {
                sb.Append(trailingTrivia.ToString());
            }

            sb.Append(right.GetLeadingTrivia().ToString());
            sb.Append(middle.ToString());
            sb.Append(" ");

            return document.WithTextChangeAsync(
                TextSpan.FromBounds(left.Span.End, right.SpanStart),
                StringBuilderCache.GetStringAndFree(sb),
                cancellationToken);
        }

        private static ExpressionSyntax FindNextExpression(SyntaxToken token)
        {
            switch (token.Parent)
            {
                case ArrowExpressionClauseSyntax arrowExpressionClause:
                    {
                        return arrowExpressionClause.Expression;
                    }
                case AssignmentExpressionSyntax assignmentExpression:
                    {
                        return assignmentExpression.Right;
                    }
                case EqualsValueClauseSyntax equalsValueClause:
                    {
                        return equalsValueClause.Value;
                    }
                case NameEqualsSyntax nameEquals:
                    {
                        if (nameEquals.Parent is AttributeArgumentSyntax attributeArgument)
                        {
                            return attributeArgument.Expression;
                        }
                        else if (nameEquals.Parent is AnonymousObjectMemberDeclaratorSyntax declarator)
                        {
                            return declarator.Expression;
                        }

                        break;
                    }
            }

            throw new InvalidOperationException();
        }

        public static Task<Document> RemoveEmptyLinesBeforeAsync(
            Document document,
            SyntaxToken token,
            CancellationToken cancellationToken = default)
        {
            SyntaxTriviaList leadingTrivia = token.LeadingTrivia;

            int count = 0;

            SyntaxTriviaList.Enumerator en = leadingTrivia.GetEnumerator();
            while (en.MoveNext())
            {
                if (en.Current.IsWhitespaceTrivia())
                {
                    if (!en.MoveNext())
                        break;

                    if (!en.Current.IsEndOfLineTrivia())
                        break;

                    count += 2;
                }
                else if (en.Current.IsEndOfLineTrivia())
                {
                    count++;
                }
                else
                {
                    break;
                }
            }

            SyntaxToken newToken = token.WithLeadingTrivia(leadingTrivia.RemoveRange(0, count));

            return document.ReplaceTokenAsync(token, newToken, cancellationToken);
        }

        public static Task<Document> AddNewLineAfterOpeningBraceAsync(
            Document document,
            BlockSyntax block,
            CancellationToken cancellationToken = default)
        {
            BlockSyntax newBlock = block
                .WithOpenBraceToken(block.OpenBraceToken.AppendEndOfLineToTrailingTrivia())
                .WithFormatterAnnotation();

            return document.ReplaceNodeAsync(block, newBlock, cancellationToken);
        }

        public static Task<Document> ReplaceTriviaBetweenAsync(
            Document document,
            SyntaxToken token1,
            SyntaxToken token2,
            string replacement = " ",
            CancellationToken cancellationToken = default)
        {
            return document.WithTextChangeAsync(
                TextSpan.FromBounds(token1.Span.End, token2.SpanStart),
                replacement,
                cancellationToken);
        }

        public static Task<Document> FixCallChainAsync(
            Document document,
            ExpressionSyntax expression,
            CancellationToken cancellationToken = default)
        {
            return FixCallChainAsync(document, expression, expression.Span, cancellationToken);
        }

        public static Task<Document> FixCallChainAsync(
            Document document,
            ExpressionSyntax expression,
            TextSpan span,
            CancellationToken cancellationToken = default)
        {
            IndentationAnalysis indentationAnalysis = AnalyzeIndentation(expression, cancellationToken);
            string indentation = indentationAnalysis.GetIncreasedIndentation();
            string endOfLineAndIndentation = DetermineEndOfLine(expression).ToString() + indentation;

            var textChanges = new List<TextChange>();
            int prevIndex = expression.Span.End;

            foreach (SyntaxNode node in new MethodChain(expression))
            {
                SyntaxKind kind = node.Kind();

                if (kind == SyntaxKind.SimpleMemberAccessExpression)
                {
                    var memberAccess = (MemberAccessExpressionSyntax)node;

                    if (!SetIndentation(memberAccess.OperatorToken))
                        break;
                }
                else if (kind == SyntaxKind.MemberBindingExpression)
                {
                    var memberBinding = (MemberBindingExpressionSyntax)node;

                    if (!SetIndentation(memberBinding.OperatorToken))
                        break;
                }
            }

            FormattingVerifier.VerifyChangedSpansAreWhitespace(expression, textChanges);

            return document.WithTextChangesAsync(textChanges, cancellationToken);

            bool SetIndentation(SyntaxToken token)
            {
                if (token.Span.End > span.End)
                    return true;

                if (token.SpanStart < span.Start)
                    return false;

                SyntaxTriviaList leading = token.LeadingTrivia;
                SyntaxTriviaList.Reversed.Enumerator en = leading.Reverse().GetEnumerator();

                if (!en.MoveNext())
                {
                    SyntaxTrivia trivia = expression.FindTrivia(token.SpanStart - 1);

                    string newText = (trivia.IsEndOfLineTrivia()) ? indentation : endOfLineAndIndentation;

                    textChanges.Add(new TextSpan(token.SpanStart, 0), newText);

                    SetIndendation(token, prevIndex);
                    prevIndex = (trivia.IsEndOfLineTrivia()) ? trivia.SpanStart : token.SpanStart;
                    return true;
                }

                SyntaxTrivia last = en.Current;

                SyntaxKind kind = en.Current.Kind();

                if (kind == SyntaxKind.WhitespaceTrivia)
                {
                    if (en.Current.Span.Length != indentation.Length)
                    {
                        if (!en.MoveNext()
                            || en.Current.IsEndOfLineTrivia())
                        {
                            SyntaxTrivia trivia = expression.FindTrivia(token.FullSpan.Start - 1);

                            if (trivia.IsEndOfLineTrivia())
                            {
                                textChanges.Add((leading.IsEmptyOrWhitespace()) ? leading.Span : last.Span, indentation);
                                SetIndendation(token, prevIndex);
                                prevIndex = trivia.SpanStart;
                                return true;
                            }
                        }
                    }
                }
                else if (kind == SyntaxKind.EndOfLineTrivia)
                {
                    SyntaxTrivia trivia = expression.FindTrivia(token.FullSpan.Start - 1);

                    if (trivia.IsEndOfLineTrivia())
                    {
                        textChanges.Add((leading.IsEmptyOrWhitespace()) ? leading.Span : last.Span, indentation);
                        SetIndendation(token, prevIndex);
                        prevIndex = trivia.SpanStart;
                        return true;
                    }
                }

                prevIndex = leading.Span.Start - 1;
                return true;
            }

            void SetIndendation(SyntaxToken token, int endIndex)
            {
                ImmutableArray<IndentationInfo> indentations = FindIndentations(
                    expression,
                    TextSpan.FromBounds(token.SpanStart, endIndex))
                    .ToImmutableArray();

                if (!indentations.Any())
                    return;

                int firstIndentationLength = indentations[0].Span.Length;

                for (int j = 0; j < indentations.Length; j++)
                {
                    IndentationInfo indentationInfo = indentations[j];

                    string replacement = indentation + indentationAnalysis.GetSingleIndentation();

                    if (j > 0
                        && indentationInfo.Span.Length > firstIndentationLength)
                    {
                        replacement += indentationInfo.ToString().Substring(firstIndentationLength);
                    }

                    if (indentationInfo.Span.Length != replacement.Length)
                        textChanges.Add(indentationInfo.Span, replacement);
                }
            }
        }

        public static Task<Document> FixBinaryExpressionAsync(
            Document document,
            BinaryExpressionSyntax binaryExpression,
            CancellationToken cancellationToken)
        {
            return FixBinaryExpressionAsync(document, binaryExpression, binaryExpression.Span, cancellationToken);
        }

        public static Task<Document> FixBinaryExpressionAsync(
            Document document,
            BinaryExpressionSyntax binaryExpression,
            TextSpan span,
            CancellationToken cancellationToken)
        {
            IndentationAnalysis indentationAnalysis = AnalyzeIndentation(binaryExpression, cancellationToken);

            string indentation;
            if (indentationAnalysis.Indentation == binaryExpression.GetLeadingTrivia().LastOrDefault()
                && AnalyzerOptions.AddNewLineAfterBinaryOperatorInsteadOfBeforeIt.IsEnabled(document, binaryExpression))
            {
                indentation = indentationAnalysis.Indentation.ToString();
            }
            else
            {
                indentation = indentationAnalysis.GetIncreasedIndentation();
            }

            string endOfLineAndIndentation = DetermineEndOfLine(binaryExpression).ToString() + indentation;

            var textChanges = new List<TextChange>();
            int prevIndex = binaryExpression.Span.End;

            SyntaxKind binaryKind = binaryExpression.Kind();

            while (true)
            {
                SyntaxToken token = binaryExpression.OperatorToken;

                if (token.Span.End > span.End)
                    continue;

                if (token.SpanStart < span.Start)
                    break;

                ExpressionSyntax left = binaryExpression.Left;
                ExpressionSyntax right = binaryExpression.Right;
                SyntaxTriviaList leftTrailing = left.GetTrailingTrivia();
                SyntaxTriviaList tokenTrailing = token.TrailingTrivia;

                if (IsOptionalWhitespaceThenOptionalSingleLineCommentThenEndOfLineTrivia(leftTrailing))
                {
                    if (!SetIndentation(token))
                        break;
                }
                else if (IsOptionalWhitespaceThenOptionalSingleLineCommentThenEndOfLineTrivia(tokenTrailing))
                {
                    if (!SetIndentation(right))
                        break;
                }
                else if (leftTrailing.IsEmptyOrWhitespace()
                    && tokenTrailing.IsEmptyOrWhitespace())
                {
                    if (AnalyzerOptions.AddNewLineAfterBinaryOperatorInsteadOfBeforeIt.IsEnabled(document, binaryExpression))
                    {
                        if (!SetIndentation(right))
                            break;
                    }
                    else if (!SetIndentation(token))
                    {
                        break;
                    }
                }

                if (!left.IsKind(binaryKind))
                    break;

                binaryExpression = (BinaryExpressionSyntax)left;
            }

            if (textChanges.Count > 0)
            {
                SyntaxTriviaList leading = binaryExpression.GetLeadingTrivia();

                if (!leading.Any())
                {
                    SyntaxTrivia trivia = binaryExpression.GetFirstToken().GetPreviousToken().TrailingTrivia.LastOrDefault();

                    if (trivia.IsEndOfLineTrivia()
                        && trivia.Span.End == binaryExpression.SpanStart)
                    {
                        textChanges.Add(new TextSpan(binaryExpression.SpanStart, 0), indentation);
                    }
                }
            }

            FormattingVerifier.VerifyChangedSpansAreWhitespace(binaryExpression, textChanges);

            return document.WithTextChangesAsync(textChanges, cancellationToken);

            bool SetIndentation(SyntaxNodeOrToken nodeOrToken)
            {
                SyntaxTriviaList leading = nodeOrToken.GetLeadingTrivia();
                SyntaxTriviaList.Reversed.Enumerator en = leading.Reverse().GetEnumerator();

                if (!en.MoveNext())
                {
                    SyntaxTrivia trivia = binaryExpression.FindTrivia(nodeOrToken.SpanStart - 1);

                    string newText = (trivia.IsEndOfLineTrivia()) ? indentation : endOfLineAndIndentation;

                    int start = (trivia.IsWhitespaceTrivia()) ? trivia.SpanStart : nodeOrToken.SpanStart;

                    TextSpan span = (trivia.IsWhitespaceTrivia())
                        ? trivia.Span
                        : new TextSpan(nodeOrToken.SpanStart, 0);

                    textChanges.Add(span, newText);
                    SetIndendation(nodeOrToken, prevIndex);
                    prevIndex = start;
                    return true;
                }

                SyntaxTrivia last = en.Current;

                SyntaxKind kind = en.Current.Kind();

                if (kind == SyntaxKind.WhitespaceTrivia)
                {
                    if (en.Current.Span.Length != indentation.Length)
                    {
                        if (!en.MoveNext()
                            || en.Current.IsEndOfLineTrivia())
                        {
                            SyntaxTrivia trivia = binaryExpression.FindTrivia(nodeOrToken.FullSpan.Start - 1);

                            if (trivia.IsEndOfLineTrivia())
                            {
                                AddTextChange((leading.IsEmptyOrWhitespace()) ? leading.Span : last.Span);
                                SetIndendation(nodeOrToken, prevIndex);
                                prevIndex = trivia.SpanStart;
                                return true;
                            }
                        }
                    }
                }
                else if (kind == SyntaxKind.EndOfLineTrivia)
                {
                    SyntaxTrivia trivia = binaryExpression.FindTrivia(nodeOrToken.FullSpan.Start - 1);

                    if (trivia.IsEndOfLineTrivia())
                    {
                        AddTextChange((leading.IsEmptyOrWhitespace()) ? leading.Span : last.Span);
                        SetIndendation(nodeOrToken, prevIndex);
                        prevIndex = trivia.SpanStart;
                        return true;
                    }
                }

                prevIndex = leading.Span.Start - 1;
                return true;

                void AddTextChange(TextSpan span) => textChanges.Add(span, indentation);
            }

            void SetIndendation(SyntaxNodeOrToken nodeOrToken, int endIndex)
            {
                ImmutableArray<IndentationInfo> indentations = FindIndentations(
                    binaryExpression,
                    TextSpan.FromBounds(nodeOrToken.SpanStart, endIndex))
                    .ToImmutableArray();

                if (!indentations.Any())
                    return;

                int firstIndentationLength = indentations[0].Span.Length;

                for (int j = 0; j < indentations.Length; j++)
                {
                    IndentationInfo indentationInfo = indentations[j];

                    string replacement = indentation + indentationAnalysis.GetSingleIndentation();

                    if (j > 0
                        && indentationInfo.Span.Length > firstIndentationLength)
                    {
                        replacement += indentationInfo.ToString().Substring(firstIndentationLength);
                    }

                    if (indentationInfo.Span.Length != replacement.Length)
                        textChanges.Add(indentationInfo.Span, replacement);
                }
            }
        }

        public static Task<Document> FixListAsync(
            Document document,
            ParameterListSyntax parameterList,
            ListFixMode fixMode = ListFixMode.Fix,
            CancellationToken cancellationToken = default)
        {
            return FixListAsync(
                document,
                parameterList,
                parameterList.OpenParenToken,
                parameterList.Parameters,
                fixMode,
                cancellationToken);
        }

        public static Task<Document> FixListAsync(
            Document document,
            BracketedParameterListSyntax bracketedParameterList,
            ListFixMode fixMode = ListFixMode.Fix,
            CancellationToken cancellationToken = default)
        {
            return FixListAsync(
                document,
                bracketedParameterList,
                bracketedParameterList.OpenBracketToken,
                bracketedParameterList.Parameters,
                fixMode,
                cancellationToken);
        }

        public static Task<Document> FixListAsync(
            Document document,
            TypeParameterListSyntax typeParameterList,
            ListFixMode fixMode = ListFixMode.Fix,
            CancellationToken cancellationToken = default)
        {
            return FixListAsync(
                document,
                typeParameterList,
                typeParameterList.LessThanToken,
                typeParameterList.Parameters,
                fixMode,
                cancellationToken);
        }

        public static Task<Document> FixListAsync(
            Document document,
            ArgumentListSyntax argumentList,
            ListFixMode fixMode = ListFixMode.Fix,
            CancellationToken cancellationToken = default)
        {
            return FixListAsync(
                document,
                argumentList,
                argumentList.OpenParenToken,
                argumentList.Arguments,
                fixMode,
                cancellationToken);
        }

        public static Task<Document> FixListAsync(
            Document document,
            BracketedArgumentListSyntax bracketedArgumentList,
            ListFixMode fixMode = ListFixMode.Fix,
            CancellationToken cancellationToken = default)
        {
            return FixListAsync(
                document,
                bracketedArgumentList,
                bracketedArgumentList.OpenBracketToken,
                bracketedArgumentList.Arguments,
                fixMode,
                cancellationToken);
        }

        public static Task<Document> FixListAsync(
            Document document,
            AttributeArgumentListSyntax attributeArgumentList,
            ListFixMode fixMode = ListFixMode.Fix,
            CancellationToken cancellationToken = default)
        {
            return FixListAsync(
                document,
                attributeArgumentList,
                attributeArgumentList.OpenParenToken,
                attributeArgumentList.Arguments,
                fixMode,
                cancellationToken);
        }

        public static Task<Document> FixListAsync(
            Document document,
            TypeArgumentListSyntax typeArgumentList,
            ListFixMode fixMode = ListFixMode.Fix,
            CancellationToken cancellationToken = default)
        {
            return FixListAsync(
                document,
                typeArgumentList,
                typeArgumentList.LessThanToken,
                typeArgumentList.Arguments,
                fixMode,
                cancellationToken);
        }

        public static Task<Document> FixListAsync(
            Document document,
            AttributeListSyntax attributeList,
            ListFixMode fixMode = ListFixMode.Fix,
            CancellationToken cancellationToken = default)
        {
            return FixListAsync(
                document,
                attributeList,
                attributeList.OpenBracketToken,
                attributeList.Attributes,
                fixMode,
                cancellationToken);
        }

        public static Task<Document> FixListAsync(
            Document document,
            BaseListSyntax baseList,
            ListFixMode fixMode = ListFixMode.Fix,
            CancellationToken cancellationToken = default)
        {
            return FixListAsync(document, baseList, baseList.ColonToken, baseList.Types, fixMode, cancellationToken);
        }

        public static Task<Document> FixListAsync(
            Document document,
            TupleTypeSyntax tupleType,
            ListFixMode fixMode = ListFixMode.Fix,
            CancellationToken cancellationToken = default)
        {
            return FixListAsync(
                document,
                tupleType,
                tupleType.OpenParenToken,
                tupleType.Elements,
                fixMode,
                cancellationToken);
        }

        public static Task<Document> FixListAsync(
            Document document,
            TupleExpressionSyntax tupleExpression,
            ListFixMode fixMode = ListFixMode.Fix,
            CancellationToken cancellationToken = default)
        {
            return FixListAsync(
                document,
                tupleExpression,
                tupleExpression.OpenParenToken,
                tupleExpression.Arguments,
                fixMode,
                cancellationToken);
        }

        public static Task<Document> FixListAsync(
            Document document,
            InitializerExpressionSyntax initializerExpression,
            ListFixMode fixMode = ListFixMode.Fix,
            CancellationToken cancellationToken = default)
        {
            return FixListAsync(
                document,
                initializerExpression,
                initializerExpression.OpenBraceToken,
                initializerExpression.Expressions,
                fixMode,
                cancellationToken);
        }

        private static Task<Document> FixListAsync<TNode>(
            Document document,
            SyntaxNode containingNode,
            SyntaxNodeOrToken openNodeOrToken,
            SeparatedSyntaxList<TNode> nodes,
            ListFixMode fixMode = ListFixMode.Fix,
            CancellationToken cancellationToken = default) where TNode : SyntaxNode
        {
            List<TextChange> textChanges = GetFixListChanges(
                containingNode,
                openNodeOrToken,
                nodes,
                fixMode,
                cancellationToken);

            return document.WithTextChangesAsync(
                textChanges,
                cancellationToken);
        }

        internal static List<TextChange> GetFixListChanges<TNode>(
            SyntaxNode containingNode,
            SyntaxNodeOrToken openNodeOrToken,
            IReadOnlyList<TNode> nodes,
            ListFixMode fixMode = ListFixMode.Fix,
            CancellationToken cancellationToken = default) where TNode : SyntaxNode
        {
            IndentationAnalysis indentationAnalysis = AnalyzeIndentation(containingNode, cancellationToken);

            string increasedIndentation = indentationAnalysis.GetIncreasedIndentation();

            bool isSingleLine;
            SeparatedSyntaxList<TNode> separatedList = default;

            if (nodes is SyntaxList<TNode> list)
            {
                isSingleLine = list.IsSingleLine(includeExteriorTrivia: false, cancellationToken: cancellationToken);
            }
            else
            {
                separatedList = (SeparatedSyntaxList<TNode>)nodes;

                isSingleLine = separatedList.IsSingleLine(
                    includeExteriorTrivia: false,
                    cancellationToken: cancellationToken);
            }

            if (isSingleLine
                && fixMode == ListFixMode.Fix)
            {
                TNode node = nodes[0];

                SyntaxTriviaList leading = node.GetLeadingTrivia();

                TextSpan span = (leading.Any() && leading.Last().IsWhitespaceTrivia())
                    ? leading.Last().Span
                    : new TextSpan(node.SpanStart, 0);

                return new List<TextChange>() { new TextChange(span, increasedIndentation) };
            }

            var textChanges = new List<TextChange>();
            TextLineCollection lines = null;
            string endOfLine = DetermineEndOfLine(containingNode).ToString();

            for (int i = 0; i < nodes.Count; i++)
            {
                SyntaxToken token;
                if (i == 0)
                {
                    token = (openNodeOrToken.IsNode)
                        ? openNodeOrToken.AsNode().GetLastToken()
                        : openNodeOrToken.AsToken();
                }
                else
                {
                    token = (list == default)
                        ? separatedList.GetSeparator(i - 1)
                        : list[i - 1].GetLastToken();
                }

                SyntaxTriviaList trailing = token.TrailingTrivia;
                TNode node = nodes[i];
                var indentationAdded = false;

                if (IsOptionalWhitespaceThenOptionalSingleLineCommentThenEndOfLineTrivia(trailing))
                {
                    SyntaxTrivia last = node.GetLeadingTrivia().LastOrDefault();

                    if (last.IsWhitespaceTrivia())
                    {
                        if (last.Span.Length == increasedIndentation.Length)
                            continue;

                        textChanges.Add(last.Span, increasedIndentation);
                    }
                    else
                    {
                        textChanges.Add(new TextSpan(node.SpanStart, 0), increasedIndentation);
                    }

                    indentationAdded = true;
                }
                else
                {
                    if (nodes.Count == 1
                        && node is ArgumentSyntax argument)
                    {
                        LambdaBlock lambdaBlock = GetLambdaBlock(argument, lines ??= argument.SyntaxTree.GetText().Lines);

                        if (lambdaBlock.Block != null)
                            increasedIndentation = indentationAnalysis.Indentation.ToString();
                    }

                    if ((nodes.Count > 1 || fixMode == ListFixMode.Wrap)
                        && ShouldWrapAndIndent(containingNode, i))
                    {
                        textChanges.Add(
                            (trailing.Any() && trailing.Last().IsWhitespaceTrivia())
                                ? trailing.Last().Span
                                : new TextSpan(token.FullSpan.End, 0),
                            endOfLine);

                        textChanges.Add(new TextSpan(node.FullSpan.Start, 0), increasedIndentation);

                        indentationAdded = true;
                    }
                }

                ImmutableArray<IndentationInfo> indentations = FindIndentations(node, node.Span).ToImmutableArray();

                if (!indentations.Any())
                    continue;

                LambdaBlock lambdaBlock2 = GetLambdaBlock(node, lines ??= node.SyntaxTree.GetText().Lines);

                bool isLambdaBlockWithOpenBraceAtEndOfLine = lambdaBlock2.Token == indentations.Last().Token;

                int baseIndentationLength = (isLambdaBlockWithOpenBraceAtEndOfLine)
                    ? indentations.Last().Span.Length
                    : indentations[0].Span.Length;

                for (int j = indentations.Length - 1; j >= 0; j--)
                {
                    IndentationInfo indentationInfo = indentations[j];

                    if (indentationAdded
                        && node is ArgumentSyntax argument
                        && (argument.Expression as AnonymousFunctionExpressionSyntax)?.Block != null)
                    {
                        indentationAdded = false;
                    }

                    string replacement = increasedIndentation;

                    if (indentationAdded)
                        replacement += indentationAnalysis.GetSingleIndentation();

                    if ((j > 0 || isLambdaBlockWithOpenBraceAtEndOfLine)
                        && indentationInfo.Span.Length > baseIndentationLength)
                    {
                        replacement += indentationInfo.ToString().Substring(baseIndentationLength);
                    }

                    if (indentationInfo.Span.Length != replacement.Length)
                        textChanges.Add(indentationInfo.Span, replacement);
                }
            }

            FormattingVerifier.VerifyChangedSpansAreWhitespace(containingNode, textChanges);

            return textChanges;
        }
    }
}
