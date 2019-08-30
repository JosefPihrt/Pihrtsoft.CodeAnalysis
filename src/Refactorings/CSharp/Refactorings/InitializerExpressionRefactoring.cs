// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.CSharp.Refactorings
{
    internal static class InitializerExpressionRefactoring
    {
        public static async Task ComputeRefactoringsAsync(RefactoringContext context, InitializerExpressionSyntax initializer)
        {
            if (initializer.IsKind(SyntaxKind.ComplexElementInitializerExpression)
                && initializer.IsParentKind(SyntaxKind.CollectionInitializerExpression))
            {
                initializer = (InitializerExpressionSyntax)initializer.Parent;
            }

            if (context.Span.IsEmptyAndContainedInSpanOrBetweenSpans(initializer)
                || context.Span.IsEmptyAndContainedInSpanOrBetweenSpans(initializer.Expressions))
            {
                SeparatedSyntaxList<ExpressionSyntax> expressions = initializer.Expressions;

                if (context.IsRefactoringEnabled(RefactoringIdentifiers.FormatInitializer)
                    && expressions.Any()
                    && !initializer.IsKind(SyntaxKind.ComplexElementInitializerExpression)
                    && initializer.IsParentKind(
                        SyntaxKind.ArrayCreationExpression,
                        SyntaxKind.ImplicitArrayCreationExpression,
                        SyntaxKind.ObjectCreationExpression,
                        SyntaxKind.CollectionInitializerExpression))
                {
                    if (initializer.IsSingleLine(includeExteriorTrivia: false, cancellationToken: context.CancellationToken))
                    {
                        context.RegisterRefactoring(
                            "Format initializer on multiple lines",
                            cancellationToken => SyntaxFormatter.ToMultiLineAsync(
                                context.Document,
                                initializer,
                                cancellationToken),
                            RefactoringIdentifiers.FormatInitializer);
                    }
                    else if (expressions.All(expression => expression.IsSingleLine(cancellationToken: context.CancellationToken))
                        && initializer.DescendantTrivia(initializer.Span).All(f => f.IsWhitespaceOrEndOfLineTrivia()))
                    {
                        context.RegisterRefactoring(
                            "Format initializer on a single line",
                            cancellationToken => SyntaxFormatter.ToSingleLineAsync(
                                context.Document,
                                initializer.Parent,
                                TextSpan.FromBounds(initializer.OpenBraceToken.GetPreviousToken().Span.End, initializer.CloseBraceToken.Span.End),
                                cancellationToken),
                            RefactoringIdentifiers.FormatInitializer);
                    }
                }

                if (context.IsRefactoringEnabled(RefactoringIdentifiers.ExpandInitializer))
                    await ExpandInitializerRefactoring.ComputeRefactoringsAsync(context, initializer).ConfigureAwait(false);

                if (context.IsRefactoringEnabled(RefactoringIdentifiers.UseCSharp6DictionaryInitializer)
                    && context.SupportsCSharp6)
                {
                    await UseCSharp6DictionaryInitializerRefactoring.ComputeRefactoringAsync(context, initializer).ConfigureAwait(false);
                }
            }
        }
    }
}
