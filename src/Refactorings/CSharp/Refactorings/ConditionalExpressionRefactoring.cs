// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeActions;

namespace Roslynator.CSharp.Refactorings
{
    internal static class ConditionalExpressionRefactoring
    {
        internal static readonly string ConvertConditionalOperatorToIfElseRecursiveEquivalenceKey = EquivalenceKey.Join(RefactoringIdentifiers.ConvertConditionalOperatorToIfElse, "Recursive");

        public static async Task ComputeRefactoringsAsync(RefactoringContext context, ConditionalExpressionSyntax conditionalExpression)
        {
            if (context.Span.IsEmptyAndContainedInSpanOrBetweenSpans(conditionalExpression))
            {
                if (context.IsRefactoringEnabled(RefactoringIdentifiers.FormatConditionalExpression))
                {
                    if (conditionalExpression.IsSingleLine(cancellationToken: context.CancellationToken))
                    {
                        context.RegisterRefactoring(
                            "Format ?: on separate lines",
                            ct => SyntaxFormatter.ToMultiLineAsync(context.Document, conditionalExpression, ct),
                            RefactoringIdentifiers.FormatConditionalExpression);
                    }
                    else if (conditionalExpression.DescendantTrivia(conditionalExpression.Span).All(f => f.IsWhitespaceOrEndOfLineTrivia()))
                    {
                        context.RegisterRefactoring(
                            "Format ?: on a single line",
                            ct => SyntaxFormatter.ToSingleLineAsync(context.Document, conditionalExpression, ct),
                            RefactoringIdentifiers.FormatConditionalExpression);
                    }
                }

                if (context.IsRefactoringEnabled(RefactoringIdentifiers.ConvertConditionalOperatorToIfElse))
                {
                    SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                    (CodeAction codeAction, CodeAction recursiveCodeAction) = ConvertConditionalOperatorToIfElseRefactoring.ComputeRefactoring(
                        context.Document,
                        conditionalExpression,
                        new CodeActionData(ConvertConditionalOperatorToIfElseRefactoring.Title, RefactoringIdentifiers.ConvertConditionalOperatorToIfElse),
                        new CodeActionData(ConvertConditionalOperatorToIfElseRefactoring.RecursiveTitle, ConvertConditionalOperatorToIfElseRecursiveEquivalenceKey),
                        semanticModel,
                        context.CancellationToken);

                    if (codeAction != null)
                        context.RegisterRefactoring(codeAction);

                    if (recursiveCodeAction != null)
                        context.RegisterRefactoring(recursiveCodeAction);
                }
            }

            if (context.IsRefactoringEnabled(RefactoringIdentifiers.InvertConditionalExpression)
                && (context.Span.IsBetweenSpans(conditionalExpression)
                    || context.Span.IsEmptyAndContainedInSpan(conditionalExpression.QuestionToken, conditionalExpression.ColonToken))
                && InvertConditionalExpressionRefactoring.CanRefactor(conditionalExpression))
            {
                context.RegisterRefactoring(
                    "Invert ?:",
                    ct => InvertConditionalExpressionRefactoring.RefactorAsync(context.Document, conditionalExpression, ct),
                RefactoringIdentifiers.InvertConditionalExpression);
            }
        }
    }
}