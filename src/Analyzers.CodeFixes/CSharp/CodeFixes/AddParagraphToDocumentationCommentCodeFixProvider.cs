// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Analysis;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddParagraphToDocumentationCommentCodeFixProvider))]
    [Shared]
    public sealed class AddParagraphToDocumentationCommentCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.AddParagraphToDocumentationComment); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out XmlElementSyntax xmlElement, findInsideTrivia: true))
                return;

            Diagnostic diagnostic = context.Diagnostics[0];

            CodeAction codeAction = CodeAction.Create(
                "Add paragraphs",
                ct => RefactorAsync(context.Document, xmlElement, ct),
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        private static Task<Document> RefactorAsync(
            Document document,
            XmlElementSyntax xmlElement,
            CancellationToken cancellationToken)
        {
            SyntaxList<XmlNodeSyntax> nodes = xmlElement.Content;

            (TextSpan span1, TextSpan span2, List<TextSpan> spans) = AddParagraphToDocumentationCommentAnalyzer.FindFixableSpan(nodes, stopOnFirstMatch: false, cancellationToken: cancellationToken);

            var textChanges = new List<TextChange>();

            string newLine = nodes
                .OfType<XmlTextSyntax>()
                .SelectMany(f => f.TextTokens)
                .First(f => f.IsKind(SyntaxKind.XmlTextLiteralNewLineToken))
                .ValueText;

            string indentation = SyntaxTriviaAnalysis.DetermineIndentation(xmlElement, cancellationToken).ToString();

            string s = $"{newLine}{indentation}/// ";

            int prevEnd = -1;

            foreach (TextSpan span in spans)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (prevEnd != -1)
                    textChanges.Add(TextSpan.FromBounds(prevEnd, span.Start), s);

                SyntaxToken token = xmlElement.FindToken(span.Start);
                SyntaxToken endToken = xmlElement.FindToken(span.End - 1);

                bool isMultiline = xmlElement.SyntaxTree.IsMultiLineSpan(span, cancellationToken);

                var text = "<para>";

                if (isMultiline)
                    text += s;

                int start = token.SpanStart;
                int length = 0;

                if (token.IsKind(SyntaxKind.XmlTextLiteralToken)
                    && token.ValueText[0] == ' ')
                {
                    if (prevEnd == -1)
                    {
                        start++;
                    }
                    else
                    {
                        length++;
                    }
                }

                textChanges.Add(new TextSpan(start, length), text);

                text = "";

                if (isMultiline)
                    text += s;

                text += "</para>";

                textChanges.Add(new TextSpan(span.End, 0), text);

                prevEnd = endToken.Span.End;
            }

            cancellationToken.ThrowIfCancellationRequested();

            return document.WithTextChangesAsync(textChanges, cancellationToken);
        }
    }
}
