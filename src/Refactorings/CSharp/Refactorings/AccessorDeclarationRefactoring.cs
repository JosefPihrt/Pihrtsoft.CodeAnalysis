// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Refactorings
{
    internal static class AccessorDeclarationRefactoring
    {
        public static void ComputeRefactorings(RefactoringContext context, AccessorDeclarationSyntax accessor)
        {
            if (context.IsRefactoringEnabled(RefactoringIdentifiers.FormatAccessorBraces))
            {
                BlockSyntax body = accessor.Body;

                if (body?.Span.Contains(context.Span) == true
                    && !body.OpenBraceToken.IsMissing
                    && !body.CloseBraceToken.IsMissing)
                {
                    if (body.IsSingleLine(cancellationToken: context.CancellationToken))
                    {
                        if (accessor.Parent?.IsMultiLine(cancellationToken: context.CancellationToken) == true)
                        {
                            context.RegisterRefactoring(
                                "Format braces on separate lines",
                                ct => SyntaxFormatter.ToMultiLineAsync(context.Document, accessor, ct),
                                RefactoringIdentifiers.FormatAccessorBraces);
                        }
                    }
                    else if (body.Statements.SingleOrDefault(shouldThrow: false)?.IsSingleLine(cancellationToken: context.CancellationToken) == true
                        && accessor.DescendantTrivia(accessor.Span).All(f => f.IsWhitespaceOrEndOfLineTrivia()))
                    {
                        context.RegisterRefactoring(
                            "Format braces on a single line",
                            ct => SyntaxFormatter.ToSingleLineAsync(context.Document, accessor, ct),
                            RefactoringIdentifiers.FormatAccessorBraces);
                    }
                }
            }

            if (context.IsRefactoringEnabled(RefactoringIdentifiers.UseExpressionBodiedMember)
                && context.SupportsCSharp6
                && UseExpressionBodiedMemberRefactoring.CanRefactor(accessor, context.Span))
            {
                context.RegisterRefactoring(
                    UseExpressionBodiedMemberRefactoring.Title,
                    ct => UseExpressionBodiedMemberRefactoring.RefactorAsync(context.Document, accessor, ct),
                    RefactoringIdentifiers.UseExpressionBodiedMember);
            }
        }
    }
}
