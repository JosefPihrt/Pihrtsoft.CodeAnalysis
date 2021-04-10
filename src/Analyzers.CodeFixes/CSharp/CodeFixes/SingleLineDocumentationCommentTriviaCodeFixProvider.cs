// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Refactorings.Documentation;
using Microsoft.CodeAnalysis.CSharp;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SingleLineDocumentationCommentTriviaCodeFixProvider))]
    [Shared]
    public sealed class SingleLineDocumentationCommentTriviaCodeFixProvider : BaseCodeFixProvider
    {
        private static readonly Regex _formatSummaryOnSingleLineRegex = new Regex(
            @"
            ^
            (
                [\s-[\r\n]]*
                \r?\n
                [\s-[\r\n]]*
                ///
                [\s-[\r\n]]*
            )?
            (?<1>[^\r\n]*)
            (
                [\s-[\r\n]]*
                \r?\n
                [\s-[\r\n]]*
                ///
                [\s-[\r\n]]*
            )?
            $
            ",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture);

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.FormatDocumentationSummaryOnSingleLine,
                    DiagnosticIdentifiers.FormatDocumentationSummaryOnMultipleLines,
                    DiagnosticIdentifiers.AddParamElementToDocumentationComment,
                    DiagnosticIdentifiers.AddTypeParamElementToDocumentationComment);
            }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out DocumentationCommentTriviaSyntax documentationComment, findInsideTrivia: true, getInnermostNodeForTie: false))
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.FormatDocumentationSummaryOnSingleLine:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Format summary on a single line",
                                ct => FormatSummaryOnSingleLineAsync(context.Document, documentationComment, ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.FormatDocumentationSummaryOnMultipleLines:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Format summary on multiple lines",
                                ct => FormatSummaryOnMultipleLinesAsync(context.Document, documentationComment, ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.AddParamElementToDocumentationComment:
                        {
                            var refactoring = new AddParamElementToDocumentationCommentRefactoring();

                            CodeAction codeAction = CodeAction.Create(
                                "Add 'param' element",
                                cancellationToken => refactoring.RefactorAsync(context.Document, documentationComment, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.AddTypeParamElementToDocumentationComment:
                        {
                            var refactoring = new AddTypeParamElementToDocumentationCommentRefactoring();

                            CodeAction codeAction = CodeAction.Create(
                                "Add 'typeparam' element",
                                cancellationToken => refactoring.RefactorAsync(context.Document, documentationComment, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }
        }

        private static Task<Document> FormatSummaryOnSingleLineAsync(
            Document document,
            DocumentationCommentTriviaSyntax documentationComment,
            CancellationToken cancellationToken)
        {
            XmlElementSyntax summaryElement = documentationComment.SummaryElement();

            XmlElementStartTagSyntax startTag = summaryElement.StartTag;
            XmlElementEndTagSyntax endTag = summaryElement.EndTag;

            Match match = _formatSummaryOnSingleLineRegex.Match(
                summaryElement.ToString(),
                startTag.Span.End - summaryElement.SpanStart,
                endTag.SpanStart - startTag.Span.End);

            return document.WithTextChangeAsync(
                new TextSpan(startTag.Span.End, match.Length),
                match.Groups[1].Value,
                cancellationToken);
        }

        private static Task<Document> FormatSummaryOnMultipleLinesAsync(
            Document document,
            DocumentationCommentTriviaSyntax documentationComment,
            CancellationToken cancellationToken)
        {
            XmlElementSyntax summaryElement = documentationComment.SummaryElement();

            var indentation = "";

            SyntaxTrivia parentTrivia = documentationComment.ParentTrivia;

            SyntaxToken token = parentTrivia.Token;

            SyntaxTriviaList leadingTrivia = token.LeadingTrivia;

            int index = leadingTrivia.IndexOf(parentTrivia);

            if (index > 0)
            {
                SyntaxTrivia previousTrivia = token.LeadingTrivia[index - 1];

                if (previousTrivia.IsWhitespaceTrivia())
                    indentation = previousTrivia.ToString();
            }

            string endOfLine = documentationComment.DescendantTokens().FirstOrDefault(f => f.IsKind(SyntaxKind.XmlTextLiteralNewLineToken)).ToString();

            if (endOfLine.Length == 0)
                endOfLine = Environment.NewLine;

            string startOfLine = endOfLine + indentation + "/// ";

            return document.WithTextChangesAsync(
                new[]
                {
                    new TextChange(new TextSpan(summaryElement.StartTag.Span.End, 0), startOfLine),
                    new TextChange(new TextSpan(summaryElement.EndTag.SpanStart, 0), startOfLine)
                },
                cancellationToken);
        }
    }
}