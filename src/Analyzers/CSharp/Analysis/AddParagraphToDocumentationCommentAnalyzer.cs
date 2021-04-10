// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics.CodeAnalysis;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AddParagraphToDocumentationCommentAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.AddParagraphToDocumentationComment);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeSingleLineDocumentationCommentTrivia(f), SyntaxKind.SingleLineDocumentationCommentTrivia);
        }

        private static void AnalyzeSingleLineDocumentationCommentTrivia(SyntaxNodeAnalysisContext context)
        {
            var documentationComment = (DocumentationCommentTriviaSyntax)context.Node;

            if (!documentationComment.IsPartOfMemberDeclaration())
                return;

            foreach (XmlNodeSyntax node in documentationComment.Content)
            {
                if (!(node is XmlElementSyntax element))
                    continue;

                string localName = element.StartTag.Name?.LocalName.ValueText;

                if (!string.Equals(localName, "summary", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(localName, "remarks", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(localName, "returns", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                SyntaxList<XmlNodeSyntax> content = element.Content;

                if (!content.Any())
                    continue;

                (TextSpan span1, TextSpan span2, IList<TextSpan> _) = FindFixableSpan(content, stopOnFirstMatch: true, context.CancellationToken);

                if (span2.End > 0)
                {
                    DiagnosticHelpers.ReportDiagnostic(
                        context,
                        DiagnosticRules.AddParagraphToDocumentationComment,
                        Location.Create(documentationComment.SyntaxTree, TextSpan.FromBounds(span1.Start, span2.End)));
                }
            }
        }

        [SuppressMessage("Simplification", "RCS1180:Inline lazy initialization.")]
        internal static (TextSpan span1, TextSpan span2, List<TextSpan> spans) FindFixableSpan(
            SyntaxList<XmlNodeSyntax> nodes,
            bool stopOnFirstMatch = false,
            CancellationToken cancellationToken = default)
        {
            int index = -1;
            int endIndex = -1;
            int index2 = -1;
            int endIndex2 = -1;

            List<TextSpan> spans = null;

            SyntaxNodeOrToken last = default;

            var state = State.BeforeParagraph;

            foreach (XmlNodeSyntax node in nodes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string name = null;

                SyntaxKind kind = node.Kind();

                if (kind == SyntaxKind.XmlElement)
                {
                    var xmlElement = (XmlElementSyntax)node;

                    name = xmlElement.StartTag?.Name?.LocalName.ValueText;

                    if (!string.Equals(
                        name,
                        xmlElement.EndTag?.Name?.LocalName.ValueText,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return (default, default, spans);
                    }
                }
                else if (kind == SyntaxKind.XmlEmptyElement)
                {
                    var xmlEmptyElement = (XmlEmptyElementSyntax)node;

                    name = xmlEmptyElement.Name?.LocalName.ValueText;
                }

                if (name != null)
                {
                    if (string.Equals(name, "code", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(name, "inheritdoc", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(name, "list", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(name, "para", StringComparison.OrdinalIgnoreCase))
                    {
                        (TextSpan span1, TextSpan span2, List<TextSpan> spans) result = CreateResult();

                        if (result != default)
                            return result;

                        index = -1;
                        endIndex = -1;
                        index2 = -1;
                        spans = null;
                        last = default;
                        state = State.BeforeParagraph;

                        continue;
                    }

                    switch (state)
                    {
                        case State.BeforeParagraph:
                            {
                                state = State.Paragraph;
                                index = node.SpanStart;
                                last = node;
                                break;
                            }
                        case State.Paragraph:
                            {
                                last = node;
                                break;
                            }
                        case State.NewLine:
                        case State.WhiteSpaceAfterNewLine:
                            {
                                state = State.Paragraph;
                                last = node;
                                break;
                            }
                        case State.WhiteSpaceBetweenParagraphs:
                            {
                                state = State.Paragraph;
                                index2 = node.SpanStart;
                                last = node;
                                break;
                            }
                    }
                }
                else if (kind == SyntaxKind.XmlText)
                {
                    var xmlText = (XmlTextSyntax)node;

                    foreach (SyntaxToken token in xmlText.TextTokens)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        switch (token.Kind())
                        {
                            case SyntaxKind.XmlTextLiteralToken:
                                {
                                    switch (state)
                                    {
                                        case State.BeforeParagraph:
                                            {
                                                if (!StringUtility.IsEmptyOrWhitespace(token.ValueText))
                                                {
                                                    state = State.Paragraph;
                                                    index = token.SpanStart;
                                                    last = token;
                                                }

                                                break;
                                            }
                                        case State.Paragraph:
                                            {
                                                last = token;
                                                break;
                                            }
                                        case State.NewLine:
                                            {
                                                if (StringUtility.IsEmptyOrWhitespace(token.ValueText))
                                                {
                                                    state = State.WhiteSpaceAfterNewLine;
                                                }
                                                else
                                                {
                                                    state = State.Paragraph;
                                                    last = token;
                                                }

                                                break;
                                            }
                                        case State.WhiteSpaceAfterNewLine:
                                            {
                                                if (!StringUtility.IsEmptyOrWhitespace(token.ValueText))
                                                {
                                                    state = State.Paragraph;
                                                    last = token;
                                                }

                                                break;
                                            }
                                        case State.WhiteSpaceBetweenParagraphs:
                                            {
                                                if (!StringUtility.IsEmptyOrWhitespace(token.ValueText))
                                                {
                                                    state = State.Paragraph;
                                                    index2 = token.SpanStart;
                                                    last = token;
                                                }

                                                break;
                                            }
                                    }

                                    break;
                                }
                            case SyntaxKind.XmlTextLiteralNewLineToken:
                                {
                                    switch (state)
                                    {
                                        case State.BeforeParagraph:
                                            {
                                                break;
                                            }
                                        case State.Paragraph:
                                            {
                                                state = State.NewLine;
                                                break;
                                            }
                                        case State.NewLine:
                                        case State.WhiteSpaceAfterNewLine:
                                            {
                                                if (index2 != -1)
                                                {
                                                    endIndex2 = last.Span.End;

                                                    if (!stopOnFirstMatch)
                                                    {
                                                        if (spans == null)
                                                            spans = new List<TextSpan>() { TextSpan.FromBounds(index, endIndex) };

                                                        spans.Add(TextSpan.FromBounds(index2, endIndex2));
                                                        index = index2;
                                                        index2 = -1;
                                                    }
                                                    else
                                                    {
                                                        return (TextSpan.FromBounds(index, endIndex), TextSpan.FromBounds(index2, endIndex2), default);
                                                    }
                                                }

                                                endIndex = last.Span.End;
                                                last = default;
                                                state = State.WhiteSpaceBetweenParagraphs;
                                                break;
                                            }
                                        case State.WhiteSpaceBetweenParagraphs:
                                            {
                                                break;
                                            }
                                    }

                                    break;
                                }
                        }
                    }
                }
            }

            return CreateResult();

            (TextSpan span1, TextSpan span2, List<TextSpan> spans) CreateResult()
            {
                if (index2 == -1)
                    return (default, default, spans);

                Debug.Assert(last != default);

                endIndex2 = last.Span.End;

                if (stopOnFirstMatch)
                    return (TextSpan.FromBounds(index, endIndex), TextSpan.FromBounds(index2, endIndex2), default);

                if (spans == null)
                    spans = new List<TextSpan>() { TextSpan.FromBounds(index, endIndex) };

                spans.Add(TextSpan.FromBounds(index2, endIndex2));

                return (default, default, spans);
            }
        }

        private enum State
        {
            BeforeParagraph,
            Paragraph,
            NewLine,
            WhiteSpaceAfterNewLine,
            WhiteSpaceBetweenParagraphs,
        }
    }
}
