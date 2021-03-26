// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.CSharp.Refactorings
{
    internal static class CommentTriviaRefactoring
    {
        public static void ComputeRefactorings(RefactoringContext context, SyntaxTrivia trivia)
        {
            SyntaxKind kind = trivia.Kind();
            Document document = context.Document;
            TextSpan span = context.Span;

            if (context.IsRootCompilationUnit
                && trivia.FullSpan.Contains(span)
                && CSharpFacts.IsCommentTrivia(kind)
                && context.IsRefactoringEnabled(RefactoringIdentifiers.RemoveComment))
            {
                context.RegisterRefactoring(
                    "Remove comment",
                    ct =>
                    {
                        SyntaxToken newToken = RemoveCommentHelper.GetReplacementToken(trivia).WithFormatterAnnotation();

                        return document.ReplaceTokenAsync(trivia.Token, newToken, ct);
                    },
                    RefactoringIdentifiers.RemoveComment);
            }
        }

        public static void ComputeRefactorings(RefactoringContext context, SyntaxNode node)
        {
            TextSpan span = context.Span;

            if (context.IsAnyRefactoringEnabled(
                RefactoringIdentifiers.RemoveAllComments,
                RefactoringIdentifiers.RemoveAllCommentsExceptDocumentationComments,
                RefactoringIdentifiers.RemoveAllDocumentationComments))
            {
                var fComment = false;
                var fDocComment = false;

                foreach (SyntaxTrivia trivia in node.DescendantTrivia(span, descendIntoTrivia: true))
                {
                    if (fComment && fDocComment)
                        break;

                    if (span.Contains(trivia.Span))
                    {
                        switch (trivia.Kind())
                        {
                            case SyntaxKind.SingleLineCommentTrivia:
                            case SyntaxKind.MultiLineCommentTrivia:
                                {
                                    fComment = true;
                                    break;
                                }
                            case SyntaxKind.SingleLineDocumentationCommentTrivia:
                            case SyntaxKind.MultiLineDocumentationCommentTrivia:
                                {
                                    fDocComment = true;
                                    break;
                                }
                        }
                    }
                }

                Document document = context.Document;

                if ((fComment || fDocComment)
                    && context.IsRefactoringEnabled(RefactoringIdentifiers.RemoveAllComments))
                {
                    context.RegisterRefactoring(
                        "Remove comments",
                        ct => document.RemoveCommentsAsync(span, CommentFilter.All, ct),
                        RefactoringIdentifiers.RemoveAllComments);
                }

                if (fComment
                    && fDocComment
                    && context.IsRefactoringEnabled(RefactoringIdentifiers.RemoveAllCommentsExceptDocumentationComments))
                {
                    context.RegisterRefactoring(
                        "Remove comments (except documentation comments)",
                        ct => document.RemoveCommentsAsync(span, CommentFilter.NonDocumentation, ct),
                        RefactoringIdentifiers.RemoveAllCommentsExceptDocumentationComments);
                }

                if (fDocComment
                    && context.IsRefactoringEnabled(RefactoringIdentifiers.RemoveAllDocumentationComments))
                {
                    context.RegisterRefactoring(
                        "Remove documentation comments",
                        ct => document.RemoveCommentsAsync(span, CommentFilter.Documentation, ct),
                        RefactoringIdentifiers.RemoveAllDocumentationComments);
                }
            }
        }
    }
}