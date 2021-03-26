// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Roslynator.CSharp;
using Roslynator.Formatting.CSharp;

namespace Roslynator.Formatting.CodeFixes.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SyntaxTriviaCodeFixProvider))]
    [Shared]
    public class SyntaxTriviaCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.AddEmptyLineAfterTopComment,
                    DiagnosticIdentifiers.AddEmptyLineBeforeTopDeclaration,
                    DiagnosticIdentifiers.AddEmptyLineBetweenAccessors,
                    DiagnosticIdentifiers.AddEmptyLineBetweenSingleLineAccessorsOrViceVersa,
                    DiagnosticIdentifiers.AddEmptyLineBetweenUsingDirectivesWithDifferentRootNamespaceOrViceVersa,
                    DiagnosticIdentifiers.RemoveEmptyLineBetweenUsingDirectivesWithSameRootNamespace,
                    DiagnosticIdentifiers.RemoveNewLineBetweenIfKeywordAndElseKeyword,
                    DiagnosticIdentifiers.RemoveNewLineBeforeBaseList);
            }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];

            if (!TryFindTrivia(root, context.Span.Start, out SyntaxTrivia trivia, findInsideTrivia: false))
                return;

            switch (diagnostic.Id)
            {
                case DiagnosticIdentifiers.AddEmptyLineAfterTopComment:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            CodeFixTitles.AddEmptyLine,
                            ct => document.ReplaceTokenAsync(trivia.Token, trivia.Token.AppendEndOfLineToLeadingTrivia(), ct),
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
                case DiagnosticIdentifiers.AddEmptyLineBeforeTopDeclaration:
                case DiagnosticIdentifiers.AddEmptyLineBetweenAccessors:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            CodeFixTitles.AddEmptyLine,
                            ct => CodeFixHelpers.AppendEndOfLineAsync(document, trivia.Token, ct),
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
                case DiagnosticIdentifiers.AddEmptyLineBetweenSingleLineAccessorsOrViceVersa:
                case DiagnosticIdentifiers.AddEmptyLineBetweenUsingDirectivesWithDifferentRootNamespaceOrViceVersa:
                    {
                        if (DiagnosticProperties.ContainsInvert(diagnostic.Properties))
                        {
                            CodeAction codeAction = CodeAction.Create(
                                CodeFixTitles.RemoveEmptyLine,
                                ct => CodeFixHelpers.RemoveEmptyLinesBeforeAsync(document, trivia.Token, ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                        }
                        else
                        {
                            CodeAction codeAction = CodeAction.Create(
                                CodeFixTitles.AddEmptyLine,
                                ct => CodeFixHelpers.AppendEndOfLineAsync(document, trivia.Token, ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                        }

                        break;
                    }
                case DiagnosticIdentifiers.RemoveEmptyLineBetweenUsingDirectivesWithSameRootNamespace:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            CodeFixTitles.RemoveEmptyLine,
                            ct => CodeFixHelpers.RemoveEmptyLinesBeforeAsync(document, trivia.Token, ct),
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
                case DiagnosticIdentifiers.RemoveNewLineBeforeBaseList:
                case DiagnosticIdentifiers.RemoveNewLineBetweenIfKeywordAndElseKeyword:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            CodeFixTitles.RemoveNewLine,
                            ct => CodeFixHelpers.ReplaceTriviaBetweenAsync(document, trivia.Token, trivia.Token.GetNextToken(), cancellationToken: ct),
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
            }
        }
    }
}
