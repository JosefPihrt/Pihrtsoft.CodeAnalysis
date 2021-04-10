// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OrderElementsInDocumentationCommentCodeFixProvider))]
    [Shared]
    public sealed class OrderElementsInDocumentationCommentCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.OrderElementsInDocumentationComment); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out XmlNodeSyntax xmlNode, findInsideTrivia: true))
                return;

            Diagnostic diagnostic = context.Diagnostics[0];

            Document document = context.Document;

            CodeAction codeAction = CodeAction.Create(
                "Order elements",
                ct => OrderElementsAsync(document, xmlNode, ct),
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        private static Task<Document> OrderElementsAsync(
            Document document,
            XmlNodeSyntax xmlNode,
            CancellationToken cancellationToken)
        {
            var documentationComment = (DocumentationCommentTriviaSyntax)xmlNode.Parent;

            SyntaxList<XmlNodeSyntax> content = documentationComment.Content;

            MemberDeclarationSyntax memberDeclaration = xmlNode.FirstAncestor<MemberDeclarationSyntax>();

            int firstIndex = content.IndexOf(xmlNode);

            SyntaxList<XmlNodeSyntax> newContent = GetNewContent();

            DocumentationCommentTriviaSyntax newDocumentationComment = documentationComment.WithContent(newContent);

            return document.ReplaceNodeAsync(documentationComment, newDocumentationComment, cancellationToken);

            SyntaxList<XmlNodeSyntax> GetNewContent()
            {
                switch (SyntaxInfo.XmlElementInfo(xmlNode).GetTag())
                {
                    case XmlTag.Param:
                        {
                            SeparatedSyntaxList<ParameterSyntax> parameters = CSharpUtility.GetParameters(memberDeclaration);

                            return SortElements(parameters, content, firstIndex, XmlTag.Param, (nodes, name) => nodes.IndexOf(name));
                        }
                    case XmlTag.TypeParam:
                        {
                            SeparatedSyntaxList<TypeParameterSyntax> typeParameters = CSharpUtility.GetTypeParameters(memberDeclaration);

                            return SortElements(typeParameters, content, firstIndex, XmlTag.TypeParam, (nodes, name) => nodes.IndexOf(name));
                        }
                    default:
                        {
                            throw new InvalidOperationException();
                        }
                }
            }
        }

        private static SyntaxList<XmlNodeSyntax> SortElements<TNode>(
            SeparatedSyntaxList<TNode> nodes,
            SyntaxList<XmlNodeSyntax> content,
            int firstIndex,
            XmlTag tag,
            Func<SeparatedSyntaxList<TNode>, string, int> indexOf) where TNode : SyntaxNode
        {
            var xmlNodes = new List<XmlNodeSyntax>();

            var ranks = new Dictionary<XmlNodeSyntax, int>();

            for (int i = firstIndex; i < content.Count; i++)
            {
                XmlElementInfo elementInfo = SyntaxInfo.XmlElementInfo(content[i]);

                if (elementInfo.Success)
                {
                    if (elementInfo.HasTag(tag))
                    {
                        XmlNodeSyntax element = elementInfo.Element;

                        string value = (element.IsKind(SyntaxKind.XmlElement))
                            ? ((XmlElementSyntax)element).GetAttributeValue("name")
                            : ((XmlEmptyElementSyntax)element).GetAttributeValue("name");

                        if (value != null)
                        {
                            ranks[element] = indexOf(nodes, value);
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                xmlNodes.Add(content[i]);
            }

            for (int i = 0; i < xmlNodes.Count - 1; i++)
            {
                for (int j = 0; j < xmlNodes.Count - i - 1; j++)
                {
                    XmlNodeSyntax node1 = xmlNodes[j];

                    if (ranks.TryGetValue(node1, out int rank1))
                    {
                        for (int k = j + 1; k < xmlNodes.Count - i - 1; k++)
                        {
                            XmlNodeSyntax node2 = xmlNodes[k];
                            if (ranks.TryGetValue(node2, out int rank2))
                            {
                                if (rank1 > rank2)
                                {
                                    xmlNodes[k] = node1;
                                    xmlNodes[j] = node2;
                                }

                                break;
                            }
                        }
                    }
                }
            }

            return content.ReplaceRange(firstIndex, xmlNodes.Count, xmlNodes);
        }
    }
}
