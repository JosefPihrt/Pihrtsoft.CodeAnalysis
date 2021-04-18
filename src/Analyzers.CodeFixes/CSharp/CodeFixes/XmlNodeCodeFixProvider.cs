// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(XmlNodeCodeFixProvider))]
    [Shared]
    public sealed class XmlNodeCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.UnusedElementInDocumentationComment,
                    DiagnosticIdentifiers.FixDocumentationCommentTag);
            }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out XmlNodeSyntax xmlNode, findInsideTrivia: true))
                return;

            Document document = context.Document;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.UnusedElementInDocumentationComment:
                        {
                            XmlElementInfo elementInfo = SyntaxInfo.XmlElementInfo(xmlNode);

                            string name = elementInfo.LocalName;

                            CodeAction codeAction = CodeAction.Create(
                                $"Remove element '{name}'",
                                ct => RemoveUnusedElementInDocumentationCommentAsync(document, elementInfo, ct),
                                GetEquivalenceKey(diagnostic, name));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.FixDocumentationCommentTag:
                        {
                            XmlElementInfo elementInfo = SyntaxInfo.XmlElementInfo(xmlNode);

                            CodeAction codeAction = CodeAction.Create(
                                (elementInfo.GetTag() == XmlTag.C)
                                    ? "Rename tag to 'code'"
                                    : "Rename tag to 'c'",
                                ct => FixDocumentationCommentTagAsync(document, elementInfo, ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }
        }

        private static Task<Document> RemoveUnusedElementInDocumentationCommentAsync(
            Document document,
            in XmlElementInfo elementInfo,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            XmlNodeSyntax element = elementInfo.Element;

            var documentationComment = (DocumentationCommentTriviaSyntax)element.Parent;

            SyntaxList<XmlNodeSyntax> content = documentationComment.Content;

            int count = content.Count;
            int index = content.IndexOf(element);

            if (index == 0)
            {
                if (count == 2
                    && content[1] is XmlTextSyntax xmlText
                    && IsNewLine(xmlText))
                {
                    return document.RemoveSingleLineDocumentationComment(documentationComment, cancellationToken);
                }

                if (content[index + 1] is XmlTextSyntax xmlText2
                    && IsXmlTextBetweenLines(xmlText2))
                {
                    return document.RemoveNodesAsync(new XmlNodeSyntax[] { element, xmlText2 }, SyntaxRefactorings.DefaultRemoveOptions, cancellationToken);
                }
            }
            else if (index == 1)
            {
                if (count == 3
                    && content[0] is XmlTextSyntax xmlText
                    && IsWhitespace(xmlText)
                    && content[2] is XmlTextSyntax xmlText2
                    && IsNewLine(xmlText2))
                {
                    return document.RemoveSingleLineDocumentationComment(documentationComment, cancellationToken);
                }

                if (content[2] is XmlTextSyntax xmlText3
                    && IsXmlTextBetweenLines(xmlText3))
                {
                    return document.RemoveNodesAsync(new XmlNodeSyntax[] { element, xmlText3 }, SyntaxRefactorings.DefaultRemoveOptions, cancellationToken);
                }
            }
            else if (content[index - 1] is XmlTextSyntax xmlText
                && IsXmlTextBetweenLines(xmlText))
            {
                return document.RemoveNodesAsync(new XmlNodeSyntax[] { xmlText, element }, SyntaxRefactorings.DefaultRemoveOptions, cancellationToken);
            }

            return document.RemoveNodeAsync(element, cancellationToken);

            static bool IsXmlTextBetweenLines(XmlTextSyntax xmlText)
            {
                SyntaxTokenList tokens = xmlText.TextTokens;

                SyntaxTokenList.Enumerator en = tokens.GetEnumerator();

                if (!en.MoveNext())
                    return false;

                if (IsEmptyOrWhitespace(en.Current)
                    && !en.MoveNext())
                {
                    return false;
                }

                if (!en.Current.IsKind(SyntaxKind.XmlTextLiteralNewLineToken))
                    return false;

                if (en.MoveNext())
                {
                    if (!IsEmptyOrWhitespace(en.Current))
                        return false;

                    if (en.MoveNext())
                        return false;
                }

                return true;

                static bool IsEmptyOrWhitespace(SyntaxToken token)
                {
                    return token.IsKind(SyntaxKind.XmlTextLiteralToken)
                        && StringUtility.IsEmptyOrWhitespace(token.ValueText);
                }
            }

            static bool IsWhitespace(XmlTextSyntax xmlText)
            {
                string text = xmlText.TextTokens.SingleOrDefault(shouldThrow: false).ValueText;

                return text.Length > 0
                    && StringUtility.IsEmptyOrWhitespace(text);
            }

            static bool IsNewLine(XmlTextSyntax xmlText)
            {
                return xmlText.TextTokens.SingleOrDefault(shouldThrow: false).IsKind(SyntaxKind.XmlTextLiteralNewLineToken);
            }
        }

        private static Task<Document> FixDocumentationCommentTagAsync(
            Document document,
            in XmlElementInfo elementInfo,
            CancellationToken cancellationToken)
        {
            var element = (XmlElementSyntax)elementInfo.Element;

            XmlTag xmlTag = elementInfo.GetTag();

            XmlElementSyntax newElement;

            if (xmlTag == XmlTag.C)
            {
                newElement = element.UpdateName("code");
            }
            else if (xmlTag == XmlTag.Code)
            {
                newElement = element.UpdateName("c");
            }
            else
            {
                throw new InvalidOperationException();
            }

            return document.ReplaceNodeAsync(element, newElement, cancellationToken);
        }
    }
}
