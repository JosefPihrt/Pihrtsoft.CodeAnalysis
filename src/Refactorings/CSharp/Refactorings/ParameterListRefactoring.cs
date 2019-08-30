// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp.Refactorings.IntroduceAndInitialize;
using Roslynator.CSharp.Refactorings.NodeInList;

namespace Roslynator.CSharp.Refactorings
{
    internal static class ParameterListRefactoring
    {
        public static async Task ComputeRefactoringsAsync(RefactoringContext context, ParameterListSyntax parameterList)
        {
            SeparatedSyntaxList<ParameterSyntax> parameters = parameterList.Parameters;

            if (parameters.Any())
            {
                if (context.IsRefactoringEnabled(RefactoringIdentifiers.DuplicateParameter))
                {
                    var refactoring = new DuplicateParameterRefactoring(parameterList);
                    refactoring.ComputeRefactoring(context, RefactoringIdentifiers.DuplicateParameter);
                }

                if (context.IsRefactoringEnabled(RefactoringIdentifiers.CheckParameterForNull)
                    && SeparatedSyntaxListSelection<ParameterSyntax>.TryCreate(parameterList.Parameters, context.Span, out SeparatedSyntaxListSelection<ParameterSyntax> selectedParameters))
                {
                    SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);
                    CheckParameterForNullRefactoring.ComputeRefactoring(context, selectedParameters, semanticModel);
                }

                if (context.IsAnyRefactoringEnabled(
                    RefactoringIdentifiers.IntroduceAndInitializeField,
                    RefactoringIdentifiers.IntroduceAndInitializeProperty))
                {
                    IntroduceAndInitializeRefactoring.ComputeRefactoring(context, parameterList);
                }

                if (context.IsRefactoringEnabled(RefactoringIdentifiers.FormatParameterList)
                    && (context.Span.IsEmptyAndContainedInSpanOrBetweenSpans(parameterList)))
                {
                    if (parameterList.IsSingleLine(cancellationToken: context.CancellationToken))
                    {
                        if (parameters.Count > 1)
                        {
                            context.RegisterRefactoring(
                                "Format parameters on separate lines",
                                cancellationToken => SyntaxFormatter.ToMultiLineAsync(context.Document, parameterList, cancellationToken),
                                RefactoringIdentifiers.FormatParameterList);
                        }
                    }
                    else if (parameterList.DescendantTrivia(parameterList.Span).All(f => f.IsWhitespaceOrEndOfLineTrivia()))
                    {
                        context.RegisterRefactoring(
                            "Format parameters on a single line",
                            cancellationToken => SyntaxFormatter.ToSingleLineAsync(context.Document, parameterList, cancellationToken),
                            RefactoringIdentifiers.FormatParameterList);
                    }
                }
            }
        }
    }
}