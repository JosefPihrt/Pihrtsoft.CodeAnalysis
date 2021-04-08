// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator
{
    /// <summary>
    /// A set of extension method for a syntax.
    /// </summary>
    public static class SyntaxExtensions
    {
        #region SeparatedSyntaxList<T>
        /// <summary>
        /// Creates a new list with a node at the specified index replaced with a new node.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <param name="newNode"></param>
        public static SeparatedSyntaxList<TNode> ReplaceAt<TNode>(this SeparatedSyntaxList<TNode> list, int index, TNode newNode) where TNode : SyntaxNode
        {
            return list.Replace(list[index], newNode);
        }

        /// <summary>
        /// Returns true if the specified node is a first node in the list.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="list"></param>
        /// <param name="node"></param>
        public static bool IsFirst<TNode>(this SeparatedSyntaxList<TNode> list, TNode node) where TNode : SyntaxNode
        {
            return list.Any()
                && list[0] == node;
        }

        /// <summary>
        /// Returns true if the specified node is a last node in the list.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="list"></param>
        /// <param name="node"></param>
        public static bool IsLast<TNode>(this SeparatedSyntaxList<TNode> list, TNode node) where TNode : SyntaxNode
        {
            return list.Any()
                && list.Last() == node;
        }

        /// <summary>
        /// Returns true if any node in a list matches the predicate.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="list"></param>
        /// <param name="predicate"></param>
        public static bool Any<TNode>(this SeparatedSyntaxList<TNode> list, Func<TNode, bool> predicate) where TNode : SyntaxNode
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            for (int i = 0; i < list.Count; i++)
            {
                if (predicate(list[i]))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if all nodes in a list matches the predicate.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="list"></param>
        /// <param name="predicate"></param>
        public static bool All<TNode>(this SeparatedSyntaxList<TNode> list, Func<TNode, bool> predicate) where TNode : SyntaxNode
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            for (int i = 0; i < list.Count; i++)
            {
                if (!predicate(list[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if the specified node is in the <see cref="SeparatedSyntaxList{TNode}"/>.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="list"></param>
        /// <param name="node"></param>
        public static bool Contains<TNode>(this SeparatedSyntaxList<TNode> list, TNode node) where TNode : SyntaxNode
        {
            return list.IndexOf(node) != -1;
        }

        internal static TNode SingleOrDefault<TNode>(this SeparatedSyntaxList<TNode> list, bool shouldThrow) where TNode : SyntaxNode
        {
            return (shouldThrow) ? list.SingleOrDefault() : (list.Count == 1) ? list[0] : default;
        }

        internal static TNode SingleOrDefault<TNode>(this SeparatedSyntaxList<TNode> list, Func<TNode, bool> predicate, bool shouldThrow) where TNode : SyntaxNode
        {
            if (shouldThrow)
                return list.SingleOrDefault(predicate);

            SeparatedSyntaxList<TNode>.Enumerator en = list.GetEnumerator();

            while (en.MoveNext())
            {
                TNode result = en.Current;

                if (predicate(result))
                {
                    while (en.MoveNext())
                    {
                        if (predicate(en.Current))
                            return default;
                    }

                    return result;
                }
            }

            return default;
        }

        internal static TNode LastButOne<TNode>(this SeparatedSyntaxList<TNode> list) where TNode : SyntaxNode
        {
            return list[list.Count - 2];
        }

        internal static TNode LastButOneOrDefault<TNode>(this SeparatedSyntaxList<TNode> list) where TNode : SyntaxNode
        {
            return (list.Count > 1) ? list.LastButOne() : default;
        }

        /// <summary>
        /// Creates a new separated list with both leading and trailing trivia of the specified node.
        /// If the list contains more than one item, first item is updated with leading trivia and last item is updated with trailing trivia.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="list"></param>
        /// <param name="node"></param>
        public static SeparatedSyntaxList<TNode> WithTriviaFrom<TNode>(this SeparatedSyntaxList<TNode> list, SyntaxNode node) where TNode : SyntaxNode
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            int count = list.Count;

            if (count == 0)
                return list;

            int separatorCount = list.SeparatorCount;

            if (count == 1)
            {
                if (separatorCount == 0)
                {
                    return list.ReplaceAt(0, list[0].WithTriviaFrom(node));
                }
                else
                {
                    list = list.ReplaceAt(0, list[0].WithLeadingTrivia(node.GetLeadingTrivia()));

                    SyntaxToken separator = list.GetSeparator(0);

                    return list.ReplaceSeparator(separator, separator.WithTrailingTrivia(node.GetTrailingTrivia()));
                }
            }
            else
            {
                list = list.ReplaceAt(0, list[0].WithLeadingTrivia(node.GetLeadingTrivia()));

                if (separatorCount == count - 1)
                {
                    return list.ReplaceAt(count - 1, list[count - 1].WithTrailingTrivia(node.GetTrailingTrivia()));
                }
                else
                {
                    SyntaxToken separator = list.GetSeparator(separatorCount - 1);

                    return list.ReplaceSeparator(separator, separator.WithTrailingTrivia(node.GetTrailingTrivia()));
                }
            }
        }

        /// <summary>
        /// Returns the trailing separator, if any.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="list"></param>
        public static SyntaxToken GetTrailingSeparator<TNode>(this SeparatedSyntaxList<TNode> list) where TNode : SyntaxNode
        {
            int count = list.Count;

            if (count > 0
                && count == list.SeparatorCount)
            {
                return list.GetSeparator(count - 1);
            }

            return default;
        }

        /// <summary>
        /// Returns true if the specified list contains trailing separator.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="list"></param>
        public static bool HasTrailingSeparator<TNode>(this SeparatedSyntaxList<TNode> list) where TNode : SyntaxNode
        {
            int count = list.Count;

            return count > 0
                && count == list.SeparatorCount;
        }

        internal static string ToString<TNode>(this SeparatedSyntaxList<TNode> list, TextSpan span) where TNode : SyntaxNode
        {
            TextSpan listFullSpan = list.FullSpan;

            if (!listFullSpan.Contains(span))
                throw new ArgumentException("", nameof(span));

            if (span == listFullSpan)
                return list.ToFullString();

            TextSpan listSpan = list.Span;

            if (span == listSpan)
                return list.ToString();

            if (listSpan.Contains(span))
            {
                return list.ToString().Substring(span.Start - listSpan.Start, span.Length);
            }
            else
            {
                return list.ToFullString().Substring(span.Start - listFullSpan.Start, span.Length);
            }
        }

        internal static SyntaxTriviaList GetTrailingTrivia<TNode>(this SeparatedSyntaxList<TNode> list) where TNode : SyntaxNode
        {
            int count = list.Count;

            if (count == 0)
                return default;

            if (count == list.SeparatorCount)
                return list.GetSeparator(count - 1).TrailingTrivia;

            return list[count - 1].GetTrailingTrivia();
        }
        #endregion SeparatedSyntaxList<T>

        #region SyntaxList<T>
        /// <summary>
        /// Creates a new list with the node at the specified index replaced with a new node.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <param name="newNode"></param>
        public static SyntaxList<TNode> ReplaceAt<TNode>(this SyntaxList<TNode> list, int index, TNode newNode) where TNode : SyntaxNode
        {
            return list.Replace(list[index], newNode);
        }

        /// <summary>
        /// Returns true if the specified node is a first node in the list.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="list"></param>
        /// <param name="node"></param>
        public static bool IsFirst<TNode>(this SyntaxList<TNode> list, TNode node) where TNode : SyntaxNode
        {
            return list.Any()
                && list[0] == node;
        }

        /// <summary>
        /// Returns true if the specified node is a last node in the list.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="list"></param>
        /// <param name="node"></param>
        public static bool IsLast<TNode>(this SyntaxList<TNode> list, TNode node) where TNode : SyntaxNode
        {
            return list.Any()
                && list.Last() == node;
        }

        /// <summary>
        /// Returns true if any node in a list matches the predicate.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="list"></param>
        /// <param name="predicate"></param>
        public static bool Any<TNode>(this SyntaxList<TNode> list, Func<TNode, bool> predicate) where TNode : SyntaxNode
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            for (int i = 0; i < list.Count; i++)
            {
                if (predicate(list[i]))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if all nodes in a list matches the predicate.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="list"></param>
        /// <param name="predicate"></param>
        public static bool All<TNode>(this SyntaxList<TNode> list, Func<TNode, bool> predicate) where TNode : SyntaxNode
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            for (int i = 0; i < list.Count; i++)
            {
                if (!predicate(list[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if the specified node is in the <see cref="SyntaxList{TNode}"/>.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="list"></param>
        /// <param name="node"></param>
        public static bool Contains<TNode>(this SyntaxList<TNode> list, TNode node) where TNode : SyntaxNode
        {
            return list.IndexOf(node) != -1;
        }

        internal static TNode SingleOrDefault<TNode>(this SyntaxList<TNode> list, bool shouldThrow) where TNode : SyntaxNode
        {
            return (shouldThrow) ? list.SingleOrDefault() : ((list.Count == 1) ? list[0] : default);
        }

        internal static TNode SingleOrDefault<TNode>(this SyntaxList<TNode> list, Func<TNode, bool> predicate, bool shouldThrow) where TNode : SyntaxNode
        {
            if (shouldThrow)
                return list.SingleOrDefault(predicate);

            SyntaxList<TNode>.Enumerator en = list.GetEnumerator();

            while (en.MoveNext())
            {
                TNode result = en.Current;

                if (predicate(result))
                {
                    while (en.MoveNext())
                    {
                        if (predicate(en.Current))
                            return default;
                    }

                    return result;
                }
            }

            return default;
        }

        internal static bool SpanContainsDirectives<TNode>(this SyntaxList<TNode> list) where TNode : SyntaxNode
        {
            int count = list.Count;

            if (count == 0)
                return false;

            if (count == 1)
                return list[0].SpanContainsDirectives();

            for (int i = 1; i < count - 1; i++)
            {
                if (list[i].ContainsDirectives)
                    return true;
            }

            return list[0].SpanOrTrailingTriviaContainsDirectives()
                || list.Last().SpanOrLeadingTriviaContainsDirectives();
        }

        internal static TNode LastButOne<TNode>(this SyntaxList<TNode> list) where TNode : SyntaxNode
        {
            return list[list.Count - 2];
        }

        internal static TNode LastButOneOrDefault<TNode>(this SyntaxList<TNode> list) where TNode : SyntaxNode
        {
            return (list.Count > 1) ? list.LastButOne() : default;
        }

        /// <summary>
        /// Creates a new list with both leading and trailing trivia of the specified node.
        /// If the list contains more than one item, first item is updated with leading trivia and last item is updated with trailing trivia.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="list"></param>
        /// <param name="node"></param>
        public static SyntaxList<TNode> WithTriviaFrom<TNode>(this SyntaxList<TNode> list, SyntaxNode node) where TNode : SyntaxNode
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            int count = list.Count;

            if (count == 0)
                return list;

            if (count == 1)
                return list.ReplaceAt(0, list[0].WithTriviaFrom(node));

            return list
                .ReplaceAt(0, list[0].WithLeadingTrivia(node.GetLeadingTrivia()))
                .ReplaceAt(count - 1, list[count - 1].WithTrailingTrivia(node.GetTrailingTrivia()));
        }

        /// <summary>
        /// Get a list of all the trivia associated with the nodes in the list.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="list"></param>
        /// <param name="descendIntoChildren"></param>
        /// <param name="descendIntoTrivia"></param>
        public static IEnumerable<SyntaxTrivia> DescendantTrivia<TNode>(
            this SyntaxList<TNode> list,
            Func<SyntaxNode, bool> descendIntoChildren = null,
            bool descendIntoTrivia = false) where TNode : SyntaxNode
        {
            foreach (TNode node in list)
            {
                foreach (SyntaxTrivia trivia in node.DescendantTrivia(descendIntoChildren, descendIntoTrivia))
                {
                    yield return trivia;
                }
            }
        }

        /// <summary>
        /// Get a list of all the trivia associated with the nodes in the list.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="list"></param>
        /// <param name="span"></param>
        /// <param name="descendIntoChildren"></param>
        /// <param name="descendIntoTrivia"></param>
        public static IEnumerable<SyntaxTrivia> DescendantTrivia<TNode>(
            this SyntaxList<TNode> list,
            TextSpan span,
            Func<SyntaxNode, bool> descendIntoChildren = null,
            bool descendIntoTrivia = false) where TNode : SyntaxNode
        {
            foreach (TNode node in list)
            {
                foreach (SyntaxTrivia trivia in node.DescendantTrivia(span, descendIntoChildren, descendIntoTrivia))
                {
                    yield return trivia;
                }
            }
        }

        internal static string ToString<TNode>(this SyntaxList<TNode> list, TextSpan span) where TNode : SyntaxNode
        {
            TextSpan listFullSpan = list.FullSpan;

            if (!listFullSpan.Contains(span))
                throw new ArgumentException("", nameof(span));

            if (span == listFullSpan)
                return list.ToFullString();

            TextSpan listSpan = list.Span;

            if (span == listSpan)
                return list.ToString();

            if (listSpan.Contains(span))
            {
                return list.ToString().Substring(span.Start - listSpan.Start, span.Length);
            }
            else
            {
                return list.ToFullString().Substring(span.Start - listFullSpan.Start, span.Length);
            }
        }
        #endregion SyntaxList<T>

        #region SyntaxNode
        /// <summary>
        /// Returns leading and trailing trivia of the specified node in a single list.
        /// </summary>
        /// <param name="node"></param>
        public static SyntaxTriviaList GetLeadingAndTrailingTrivia(this SyntaxNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            SyntaxTriviaList leadingTrivia = node.GetLeadingTrivia();
            SyntaxTriviaList trailingTrivia = node.GetTrailingTrivia();

            if (leadingTrivia.Any())
            {
                if (trailingTrivia.Any())
                    return leadingTrivia.AddRange(trailingTrivia);

                return leadingTrivia;
            }

            if (trailingTrivia.Any())
                return trailingTrivia;

            return SyntaxTriviaList.Empty;
        }

        /// <summary>
        /// Creates a new node from this node with the leading trivia replaced with a new trivia where the specified trivia is inserted at the beginning of the leading trivia.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        /// <param name="trivia"></param>
        public static TNode PrependToLeadingTrivia<TNode>(this TNode node, IEnumerable<SyntaxTrivia> trivia) where TNode : SyntaxNode
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (trivia == null)
                throw new ArgumentNullException(nameof(trivia));

            return node.WithLeadingTrivia(node.GetLeadingTrivia().InsertRange(0, trivia));
        }

        /// <summary>
        /// Creates a new node from this node with the leading trivia replaced with a new trivia where the specified trivia is inserted at the beginning of the leading trivia.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        /// <param name="trivia"></param>
        public static TNode PrependToLeadingTrivia<TNode>(this TNode node, SyntaxTrivia trivia) where TNode : SyntaxNode
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return node.WithLeadingTrivia(node.GetLeadingTrivia().Insert(0, trivia));
        }

        /// <summary>
        /// Creates a new node from this node with the trailing trivia replaced with a new trivia where the specified trivia is inserted at the beginning of the trailing trivia.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        /// <param name="trivia"></param>
        public static TNode PrependToTrailingTrivia<TNode>(this TNode node, IEnumerable<SyntaxTrivia> trivia) where TNode : SyntaxNode
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (trivia == null)
                throw new ArgumentNullException(nameof(trivia));

            return node.WithTrailingTrivia(node.GetTrailingTrivia().InsertRange(0, trivia));
        }

        /// <summary>
        /// Creates a new node from this node with the trailing trivia replaced with a new trivia where the specified trivia is inserted at the beginning of the trailing trivia.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        /// <param name="trivia"></param>
        public static TNode PrependToTrailingTrivia<TNode>(this TNode node, SyntaxTrivia trivia) where TNode : SyntaxNode
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return node.WithTrailingTrivia(node.GetTrailingTrivia().Insert(0, trivia));
        }

        /// <summary>
        /// Creates a new node from this node with the leading trivia replaced with a new trivia where the specified trivia is added at the end of the leading trivia.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        /// <param name="trivia"></param>
        public static TNode AppendToLeadingTrivia<TNode>(this TNode node, IEnumerable<SyntaxTrivia> trivia) where TNode : SyntaxNode
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (trivia == null)
                throw new ArgumentNullException(nameof(trivia));

            return node.WithLeadingTrivia(node.GetLeadingTrivia().AddRange(trivia));
        }

        /// <summary>
        /// Creates a new node from this node with the leading trivia replaced with a new trivia where the specified trivia is added at the end of the leading trivia.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        /// <param name="trivia"></param>
        public static TNode AppendToLeadingTrivia<TNode>(this TNode node, SyntaxTrivia trivia) where TNode : SyntaxNode
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return node.WithLeadingTrivia(node.GetLeadingTrivia().Add(trivia));
        }

        /// <summary>
        /// Creates a new node from this node with the trailing trivia replaced with a new trivia where the specified trivia is added at the end of the trailing trivia.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        /// <param name="trivia"></param>
        public static TNode AppendToTrailingTrivia<TNode>(this TNode node, IEnumerable<SyntaxTrivia> trivia) where TNode : SyntaxNode
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (trivia == null)
                throw new ArgumentNullException(nameof(trivia));

            return node.WithTrailingTrivia(node.GetTrailingTrivia().AddRange(trivia));
        }

        /// <summary>
        /// Creates a new node from this node with the trailing trivia replaced with a new trivia where the specified trivia is added at the end of the trailing trivia.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        /// <param name="trivia"></param>
        public static TNode AppendToTrailingTrivia<TNode>(this TNode node, SyntaxTrivia trivia) where TNode : SyntaxNode
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return node.WithTrailingTrivia(node.GetTrailingTrivia().Add(trivia));
        }

        /// <summary>
        /// Returns true if the node's span contains any preprocessor directives.
        /// </summary>
        /// <param name="node"></param>
        public static bool SpanContainsDirectives(this SyntaxNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return node.ContainsDirectives
                && !node.GetLeadingTrivia().Any(f => f.IsDirective)
                && !node.GetTrailingTrivia().Any(f => f.IsDirective);
        }

        internal static bool SpanOrLeadingTriviaContainsDirectives(this SyntaxNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return node.ContainsDirectives
                && !node.GetTrailingTrivia().Any(f => f.IsDirective);
        }

        internal static bool SpanOrTrailingTriviaContainsDirectives(this SyntaxNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return node.ContainsDirectives
                && !node.GetLeadingTrivia().Any(f => f.IsDirective);
        }

        /// <summary>
        /// Returns true if the node contains any preprocessor directives inside the specified span.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="span"></param>
        public static bool ContainsDirectives(this SyntaxNode node, TextSpan span)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return node.ContainsDirectives
                && node.DescendantTrivia(span).Any(f => f.IsDirective);
        }

        /// <summary>
        /// Creates a new node from this node with both the leading and trailing trivia of the specified token.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        /// <param name="token"></param>
        public static TNode WithTriviaFrom<TNode>(this TNode node, SyntaxToken token) where TNode : SyntaxNode
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return node
                .WithLeadingTrivia(token.LeadingTrivia)
                .WithTrailingTrivia(token.TrailingTrivia);
        }

        internal static int GetSpanStartLine(this SyntaxNode node, CancellationToken cancellationToken = default)
        {
            return node.SyntaxTree.GetLineSpan(node.Span, cancellationToken).StartLine();
        }

        internal static int GetFullSpanStartLine(this SyntaxNode node, CancellationToken cancellationToken = default)
        {
            return node.SyntaxTree.GetLineSpan(node.FullSpan, cancellationToken).StartLine();
        }

        internal static int GetSpanEndLine(this SyntaxNode node, CancellationToken cancellationToken = default)
        {
            return node.SyntaxTree.GetLineSpan(node.Span, cancellationToken).EndLine();
        }

        internal static int GetFullSpanEndLine(this SyntaxNode node, CancellationToken cancellationToken = default)
        {
            return node.SyntaxTree.GetLineSpan(node.FullSpan, cancellationToken).EndLine();
        }

        /// <summary>
        /// Returns the first node of type <typeparamref name="TNode"/> that matches the predicate.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        /// <param name="predicate"></param>
        /// <param name="ascendOutOfTrivia"></param>
        public static TNode FirstAncestor<TNode>(
            this SyntaxNode node,
            Func<TNode, bool> predicate = null,
            bool ascendOutOfTrivia = true) where TNode : SyntaxNode
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return GetParent(node, ascendOutOfTrivia: ascendOutOfTrivia)?.FirstAncestorOrSelf(predicate, ascendOutOfTrivia: ascendOutOfTrivia);
        }

        internal static string ToString(this SyntaxNode node, TextSpan span)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            TextSpan nodeFullSpan = node.FullSpan;

            if (!nodeFullSpan.Contains(span))
                throw new ArgumentException("", nameof(span));

            if (span == nodeFullSpan)
                return node.ToFullString();

            TextSpan nodeSpan = node.Span;

            if (span == nodeSpan)
                return node.ToString();

            if (nodeSpan.Contains(span))
            {
                return node.ToString().Substring(span.Start - nodeSpan.Start, span.Length);
            }
            else
            {
                return node.ToFullString().Substring(span.Start - nodeFullSpan.Start, span.Length);
            }
        }

        internal static TextSpan LeadingTriviaSpan(this SyntaxNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return TextSpan.FromBounds(node.FullSpan.Start, node.SpanStart);
        }

        internal static TextSpan TrailingTriviaSpan(this SyntaxNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return TextSpan.FromBounds(node.Span.End, node.FullSpan.End);
        }

        /// <summary>
        /// Searches a list of descendant nodes in prefix document order and returns first descendant of type <typeparamref name="TNode"/>.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        /// <param name="descendIntoChildren"></param>
        /// <param name="descendIntoTrivia"></param>
        public static TNode FirstDescendant<TNode>(
            this SyntaxNode node,
            Func<SyntaxNode, bool> descendIntoChildren = null,
            bool descendIntoTrivia = false) where TNode : SyntaxNode
        {
            foreach (SyntaxNode descendant in node.DescendantNodes(descendIntoChildren: descendIntoChildren, descendIntoTrivia: descendIntoTrivia))
            {
                if (descendant is TNode tnode)
                    return tnode;
            }

            return default;
        }

        /// <summary>
        /// Searches a list of descendant nodes in prefix document order and returns first descendant of type <typeparamref name="TNode"/>.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        /// <param name="span"></param>
        /// <param name="descendIntoChildren"></param>
        /// <param name="descendIntoTrivia"></param>
        public static TNode FirstDescendant<TNode>(
            this SyntaxNode node,
            TextSpan span,
            Func<SyntaxNode, bool> descendIntoChildren = null,
            bool descendIntoTrivia = false) where TNode : SyntaxNode
        {
            foreach (SyntaxNode descendant in node.DescendantNodes(span, descendIntoChildren: descendIntoChildren, descendIntoTrivia: descendIntoTrivia))
            {
                if (descendant is TNode tnode)
                    return tnode;
            }

            return default;
        }

        /// <summary>
        /// Searches a list of descendant nodes (including this node) in prefix document order and returns first descendant of type <typeparamref name="TNode"/>.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        /// <param name="descendIntoChildren"></param>
        /// <param name="descendIntoTrivia"></param>
        public static TNode FirstDescendantOrSelf<TNode>(
            this SyntaxNode node,
            Func<SyntaxNode, bool> descendIntoChildren = null,
            bool descendIntoTrivia = false) where TNode : SyntaxNode
        {
            foreach (SyntaxNode descendant in node.DescendantNodesAndSelf(descendIntoChildren: descendIntoChildren, descendIntoTrivia: descendIntoTrivia))
            {
                if (descendant is TNode tnode)
                    return tnode;
            }

            return default;
        }

        /// <summary>
        /// Searches a list of descendant nodes (including this node) in prefix document order and returns first descendant of type <typeparamref name="TNode"/>.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        /// <param name="span"></param>
        /// <param name="descendIntoChildren"></param>
        /// <param name="descendIntoTrivia"></param>
        public static TNode FirstDescendantOrSelf<TNode>(
            this SyntaxNode node,
            TextSpan span,
            Func<SyntaxNode, bool> descendIntoChildren = null,
            bool descendIntoTrivia = false) where TNode : SyntaxNode
        {
            foreach (SyntaxNode descendant in node.DescendantNodesAndSelf(span, descendIntoChildren: descendIntoChildren, descendIntoTrivia: descendIntoTrivia))
            {
                if (descendant is TNode tnode)
                    return tnode;
            }

            return default;
        }

        internal static SyntaxNode GetParent(this SyntaxNode node, bool ascendOutOfTrivia)
        {
            SyntaxNode parent = node.Parent;

            if (parent == null
                && ascendOutOfTrivia
                && (node is IStructuredTriviaSyntax structuredTrivia))
            {
                parent = structuredTrivia.ParentTrivia.Token.Parent;
            }

            return parent;
        }

        internal static SyntaxNode WalkUp(this SyntaxNode node, Func<SyntaxNode, bool> predicate)
        {
            while (true)
            {
                SyntaxNode parent = node.Parent;

                if (parent != null
                    && predicate(parent))
                {
                    node = parent;
                }
                else
                {
                    break;
                }
            }

            return node;
        }
        #endregion SyntaxNode

        #region SyntaxNodeOrToken
        /// <summary>
        /// Creates a new <see cref="SyntaxNodeOrToken"/> from this node without leading and trailing trivia.
        /// </summary>
        /// <param name="nodeOrToken"></param>
        public static SyntaxNodeOrToken WithoutTrivia(this SyntaxNodeOrToken nodeOrToken)
        {
            if (nodeOrToken.IsNode)
            {
                return nodeOrToken.AsNode().WithoutTrivia();
            }
            else
            {
                return nodeOrToken.AsToken().WithoutTrivia();
            }
        }

        /// <summary>
        /// Creates a new <see cref="SyntaxNodeOrToken"/> with the leading trivia removed.
        /// </summary>
        /// <param name="nodeOrToken"></param>
        public static SyntaxNodeOrToken WithoutLeadingTrivia(this SyntaxNodeOrToken nodeOrToken)
        {
            if (nodeOrToken.IsNode)
            {
                return nodeOrToken.AsNode().WithoutLeadingTrivia();
            }
            else
            {
                return nodeOrToken.AsToken().WithoutLeadingTrivia();
            }
        }

        /// <summary>
        /// Creates a new <see cref="SyntaxNodeOrToken"/> with the trailing trivia removed.
        /// </summary>
        /// <param name="nodeOrToken"></param>
        public static SyntaxNodeOrToken WithoutTrailingTrivia(this SyntaxNodeOrToken nodeOrToken)
        {
            if (nodeOrToken.IsNode)
            {
                return nodeOrToken.AsNode().WithoutTrailingTrivia();
            }
            else
            {
                return nodeOrToken.AsToken().WithoutTrailingTrivia();
            }
        }
        #endregion SyntaxNodeOrToken

        #region SyntaxToken
        /// <summary>
        /// Creates a new token from this token with the leading trivia replaced with a new trivia where the specified trivia is inserted at the beginning of the leading trivia.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="trivia"></param>
        public static SyntaxToken PrependToLeadingTrivia(this SyntaxToken token, IEnumerable<SyntaxTrivia> trivia)
        {
            if (trivia == null)
                throw new ArgumentNullException(nameof(trivia));

            return token.WithLeadingTrivia(token.LeadingTrivia.InsertRange(0, trivia));
        }

        /// <summary>
        /// Creates a new token from this token with the leading trivia replaced with a new trivia where the specified trivia is inserted at the beginning of the leading trivia.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="trivia"></param>
        public static SyntaxToken PrependToLeadingTrivia(this SyntaxToken token, SyntaxTrivia trivia)
        {
            return token.WithLeadingTrivia(token.LeadingTrivia.Insert(0, trivia));
        }

        /// <summary>
        /// Creates a new token from this token with the trailing trivia replaced with a new trivia where the specified trivia is inserted at the beginning of the trailing trivia.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="trivia"></param>
        public static SyntaxToken PrependToTrailingTrivia(this SyntaxToken token, IEnumerable<SyntaxTrivia> trivia)
        {
            if (trivia == null)
                throw new ArgumentNullException(nameof(trivia));

            return token.WithTrailingTrivia(token.TrailingTrivia.InsertRange(0, trivia));
        }

        /// <summary>
        /// Creates a new token from this token with the trailing trivia replaced with a new trivia where the specified trivia is inserted at the beginning of the trailing trivia.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="trivia"></param>
        public static SyntaxToken PrependToTrailingTrivia(this SyntaxToken token, SyntaxTrivia trivia)
        {
            return token.WithTrailingTrivia(token.TrailingTrivia.Insert(0, trivia));
        }

        /// <summary>
        /// Creates a new token from this token with the trailing trivia replaced with a new trivia where the specified trivia is added at the end of the trailing trivia.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="trivia"></param>
        public static SyntaxToken AppendToTrailingTrivia(this SyntaxToken token, IEnumerable<SyntaxTrivia> trivia)
        {
            if (trivia == null)
                throw new ArgumentNullException(nameof(trivia));

            return token.WithTrailingTrivia(token.TrailingTrivia.AddRange(trivia));
        }

        /// <summary>
        /// Creates a new token from this token with the trailing trivia replaced with a new trivia where the specified trivia is added at the end of the trailing trivia.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="trivia"></param>
        public static SyntaxToken AppendToTrailingTrivia(this SyntaxToken token, SyntaxTrivia trivia)
        {
            return token.WithTrailingTrivia(token.TrailingTrivia.Add(trivia));
        }

        /// <summary>
        /// Creates a new token from this token with the leading trivia replaced with a new trivia where the specified trivia is added at the end of the leading trivia.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="trivia"></param>
        public static SyntaxToken AppendToLeadingTrivia(this SyntaxToken token, IEnumerable<SyntaxTrivia> trivia)
        {
            if (trivia == null)
                throw new ArgumentNullException(nameof(trivia));

            return token.WithLeadingTrivia(token.LeadingTrivia.AddRange(trivia));
        }

        /// <summary>
        /// Creates a new token from this token with the leading trivia replaced with a new trivia where the specified trivia is added at the end of the leading trivia.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="trivia"></param>
        public static SyntaxToken AppendToLeadingTrivia(this SyntaxToken token, SyntaxTrivia trivia)
        {
            return token.WithLeadingTrivia(token.LeadingTrivia.Add(trivia));
        }

        /// <summary>
        /// Returns leading and trailing trivia of the specified node in a single list.
        /// </summary>
        /// <param name="token"></param>
        public static SyntaxTriviaList LeadingAndTrailingTrivia(this SyntaxToken token)
        {
            SyntaxTriviaList leadingTrivia = token.LeadingTrivia;
            SyntaxTriviaList trailingTrivia = token.TrailingTrivia;

            if (leadingTrivia.Any())
            {
                if (trailingTrivia.Any())
                    return leadingTrivia.AddRange(trailingTrivia);

                return leadingTrivia;
            }

            if (trailingTrivia.Any())
                return trailingTrivia;

            return SyntaxTriviaList.Empty;
        }

        internal static int GetSpanStartLine(this SyntaxToken token, CancellationToken cancellationToken = default)
        {
            return token.SyntaxTree.GetLineSpan(token.Span, cancellationToken).StartLine();
        }

        internal static int GetFullSpanStartLine(this SyntaxToken token, CancellationToken cancellationToken = default)
        {
            return token.SyntaxTree.GetLineSpan(token.FullSpan, cancellationToken).StartLine();
        }

        internal static int GetSpanEndLine(this SyntaxToken token, CancellationToken cancellationToken = default)
        {
            return token.SyntaxTree.GetLineSpan(token.Span, cancellationToken).EndLine();
        }

        internal static int GetFullSpanEndLine(this SyntaxToken token, CancellationToken cancellationToken = default)
        {
            return token.SyntaxTree.GetLineSpan(token.FullSpan, cancellationToken).EndLine();
        }

        /// <summary>
        /// Creates a new token from this token with the leading trivia removed.
        /// </summary>
        /// <param name="token"></param>
        public static SyntaxToken WithoutLeadingTrivia(this SyntaxToken token)
        {
            return token.WithLeadingTrivia(default(SyntaxTriviaList));
        }

        /// <summary>
        /// Creates a new token from this token with the trailing trivia removed.
        /// </summary>
        /// <param name="token"></param>
        public static SyntaxToken WithoutTrailingTrivia(this SyntaxToken token)
        {
            return token.WithTrailingTrivia(default(SyntaxTriviaList));
        }

        /// <summary>
        /// Creates a new token from this token with both the leading and trailing trivia of the specified node.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="node"></param>
        public static SyntaxToken WithTriviaFrom(this SyntaxToken token, SyntaxNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return token
                .WithLeadingTrivia(node.GetLeadingTrivia())
                .WithTrailingTrivia(node.GetTrailingTrivia());
        }

        internal static TextSpan LeadingTriviaSpan(this SyntaxToken token)
        {
            return TextSpan.FromBounds(token.FullSpan.Start, token.SpanStart);
        }

        internal static TextSpan TrailingTriviaSpan(this SyntaxToken token)
        {
            return TextSpan.FromBounds(token.Span.End, token.FullSpan.End);
        }

        internal static string ToString(this SyntaxToken token, TextSpan span)
        {
            TextSpan tokenFullSpan = token.FullSpan;

            if (!tokenFullSpan.Contains(span))
                throw new ArgumentException("", nameof(span));

            if (span == tokenFullSpan)
                return token.ToFullString();

            TextSpan tokenSpan = token.Span;

            if (span == tokenSpan)
                return token.ToString();

            if (tokenSpan.Contains(span))
            {
                return token.ToString().Substring(span.Start - tokenSpan.Start, span.Length);
            }
            else
            {
                return token.ToFullString().Substring(span.Start - tokenFullSpan.Start, span.Length);
            }
        }
        #endregion SyntaxToken

        #region SyntaxTokenList
        /// <summary>
        /// Creates a new <see cref="SyntaxTokenList"/> with a token at the specified index replaced with a new token.
        /// </summary>
        /// <param name="tokenList"></param>
        /// <param name="index"></param>
        /// <param name="newToken"></param>
        public static SyntaxTokenList ReplaceAt(this SyntaxTokenList tokenList, int index, SyntaxToken newToken)
        {
            return tokenList.Replace(tokenList[index], newToken);
        }

        /// <summary>
        /// Returns true if any token in a <see cref="SyntaxTokenList"/> matches the predicate.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="predicate"></param>
        public static bool Any(this SyntaxTokenList list, Func<SyntaxToken, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            for (int i = 0; i < list.Count; i++)
            {
                if (predicate(list[i]))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if all tokens in a <see cref="SyntaxTokenList"/> matches the predicate.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="predicate"></param>
        public static bool All(this SyntaxTokenList list, Func<SyntaxToken, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            for (int i = 0; i < list.Count; i++)
            {
                if (!predicate(list[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if the specified token is in the <see cref="SyntaxTokenList"/>.
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="token"></param>
        public static bool Contains(this SyntaxTokenList tokens, SyntaxToken token)
        {
            return tokens.IndexOf(token) != -1;
        }

        /// <summary>
        /// Searches for a token that matches the predicate and returns the zero-based index of the first occurrence within the entire <see cref="SyntaxTokenList"/>.
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="predicate"></param>
        public static int IndexOf(this SyntaxTokenList tokens, Func<SyntaxToken, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            int index = 0;
            foreach (SyntaxToken token in tokens)
            {
                if (predicate(token))
                    return index;

                index++;
            }

            return -1;
        }

        internal static bool IsSorted(this SyntaxTokenList modifiers, IComparer<SyntaxToken> comparer)
        {
            int count = modifiers.Count;

            if (count > 1)
            {
                for (int i = 0; i < count - 1; i++)
                {
                    if (comparer.Compare(modifiers[i], modifiers[i + 1]) > 0)
                        return false;
                }
            }

            return true;
        }

        internal static bool SpanContainsDirectives(this SyntaxTokenList tokens)
        {
            int count = tokens.Count;

            if (count <= 1)
                return false;

            for (int i = 1; i < count - 1; i++)
            {
                if (tokens[i].ContainsDirectives)
                    return true;
            }

            return tokens[0].TrailingTrivia.Any(f => f.IsDirective)
                || tokens.Last().LeadingTrivia.Any(f => f.IsDirective);
        }
        #endregion SyntaxTokenList

        #region SyntaxTrivia
        /// <summary>
        /// Gets a <see cref="SyntaxTriviaList"/> the specified trivia is contained in.
        /// </summary>
        /// <param name="trivia"></param>
        /// <param name="triviaList"></param>
        /// <param name="allowLeading">If true, trivia can be part of leading trivia.</param>
        /// <param name="allowTrailing">If true, trivia can be part of trailing trivia.</param>
        public static bool TryGetContainingList(this SyntaxTrivia trivia, out SyntaxTriviaList triviaList, bool allowLeading = true, bool allowTrailing = true)
        {
            SyntaxToken token = trivia.Token;

            if (allowLeading)
            {
                SyntaxTriviaList leading = token.LeadingTrivia;

                int index = leading.IndexOf(trivia);

                if (index != -1)
                {
                    triviaList = leading;
                    return true;
                }
            }

            if (allowTrailing)
            {
                SyntaxTriviaList trailing = token.TrailingTrivia;

                int index = trailing.IndexOf(trivia);

                if (index != -1)
                {
                    triviaList = trailing;
                    return true;
                }
            }

            triviaList = default;
            return false;
        }

        internal static SyntaxTriviaList GetContainingList(this SyntaxTrivia trivia)
        {
            if (!TryGetContainingList(trivia, out SyntaxTriviaList list))
                throw new ArgumentException("Trivia is not contained in a list.", nameof(trivia));

            return list;
        }

        internal static TextSpan LeadingTriviaSpan(this SyntaxTrivia trivia)
        {
            return TextSpan.FromBounds(trivia.FullSpan.Start, trivia.SpanStart);
        }

        internal static TextSpan TrailingTriviaSpan(this SyntaxTrivia trivia)
        {
            return TextSpan.FromBounds(trivia.Span.End, trivia.FullSpan.End);
        }
        #endregion SyntaxTrivia

        #region SyntaxTriviaList
        /// <summary>
        /// Creates a new <see cref="SyntaxTriviaList"/> with a trivia at the specified index replaced with new trivia.
        /// </summary>
        /// <param name="triviaList"></param>
        /// <param name="index"></param>
        /// <param name="newTrivia"></param>
        public static SyntaxTriviaList ReplaceAt(this SyntaxTriviaList triviaList, int index, SyntaxTrivia newTrivia)
        {
            return triviaList.Replace(triviaList[index], newTrivia);
        }

        /// <summary>
        /// Returns true if any trivia in a <see cref="SyntaxTriviaList"/> matches the predicate.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="predicate"></param>
        public static bool Any(this SyntaxTriviaList list, Func<SyntaxTrivia, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            for (int i = 0; i < list.Count; i++)
            {
                if (predicate(list[i]))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if all trivia in a <see cref="SyntaxTriviaList"/> matches the predicate.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="predicate"></param>
        public static bool All(this SyntaxTriviaList list, Func<SyntaxTrivia, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            for (int i = 0; i < list.Count; i++)
            {
                if (!predicate(list[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Searches for a trivia that matches the predicate and returns the zero-based index of the first occurrence within the entire <see cref="SyntaxTriviaList"/>.
        /// </summary>
        /// <param name="triviaList"></param>
        /// <param name="predicate"></param>
        public static int IndexOf(this SyntaxTriviaList triviaList, Func<SyntaxTrivia, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            int index = 0;
            foreach (SyntaxTrivia trivia in triviaList)
            {
                if (predicate(trivia))
                    return index;

                index++;
            }

            return -1;
        }
        #endregion SyntaxTriviaList
    }
}
