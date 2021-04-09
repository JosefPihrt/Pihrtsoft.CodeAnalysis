// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.Testing
{
    /// <summary>
    /// Represents verifier for a code refactoring.
    /// </summary>
    public abstract class RefactoringVerifier<TRefactoringProvider> : CodeVerifier
        where TRefactoringProvider : CodeRefactoringProvider, new()
    {
        internal RefactoringVerifier(IAssert assert) : base(assert)
        {
        }

        /// <summary>
        /// Verifies that refactoring will be applied correctly using specified <typeparamref name="TRefactoringProvider"/>.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="expected"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyRefactoringAsync(
            RefactoringTestData data,
            ExpectedTestState expected,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            if (data.Spans.IsEmpty)
                Fail("Span on which a refactoring should be invoked was not found.");

            options ??= Options;

            TRefactoringProvider refactoringProvider = Activator.CreateInstance<TRefactoringProvider>();

            foreach (TextSpan span in data.Spans)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (Workspace workspace = new AdhocWorkspace())
                {
                    (Document document, ImmutableArray<ExpectedDocument> expectedDocuments) = CreateDocument(workspace.CurrentSolution, data.Source, data.AdditionalFiles, options);

                    SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken);

                    ImmutableArray<Diagnostic> compilerDiagnostics = semanticModel.GetDiagnostics(cancellationToken: cancellationToken);

                    VerifyCompilerDiagnostics(compilerDiagnostics, options);

                    CodeAction action = null;
                    List<CodeAction> candidateActions = null;

                    var context = new CodeRefactoringContext(
                        document,
                        span,
                        a =>
                        {
                            if (data.EquivalenceKey == null
                                || string.Equals(a.EquivalenceKey, data.EquivalenceKey, StringComparison.Ordinal))
                            {
                                if (action != null)
                                    Fail($"Multiple refactorings registered by '{refactoringProvider.GetType().Name}'.", new CodeAction[] { action, a });

                                action = a;
                            }
                            else
                            {
                                (candidateActions ??= new List<CodeAction>()).Add(a);
                            }
                        },
                        cancellationToken);

                    await refactoringProvider.ComputeRefactoringsAsync(context);

                    if (action == null)
                        Fail("No code refactoring has been registered.", candidateActions);

                    document = await VerifyAndApplyCodeActionAsync(document, action, expected.CodeActionTitle);
                    semanticModel = await document.GetSemanticModelAsync(cancellationToken);

                    ImmutableArray<Diagnostic> newCompilerDiagnostics = semanticModel.GetDiagnostics(cancellationToken: cancellationToken);

                    VerifyCompilerDiagnostics(newCompilerDiagnostics, options);
                    VerifyNoNewCompilerDiagnostics(compilerDiagnostics, newCompilerDiagnostics, options);

                    await VerifyExpectedDocument(expected, document, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Verifies that refactoring will not be applied using specified <typeparamref name="TRefactoringProvider"/>.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyNoRefactoringAsync(
            RefactoringTestData data,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            if (data.Spans.IsEmpty)
                Fail("Span on which a refactoring should be invoked was not found.");

            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;

            TRefactoringProvider refactoringProvider = Activator.CreateInstance<TRefactoringProvider>();

            using (Workspace workspace = new AdhocWorkspace())
            {
                (Document document, ImmutableArray<ExpectedDocument> expectedDocuments) = CreateDocument(workspace.CurrentSolution, data.Source, data.AdditionalFiles, options);

                SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken);

                ImmutableArray<Diagnostic> compilerDiagnostics = semanticModel.GetDiagnostics(cancellationToken: cancellationToken);

                VerifyCompilerDiagnostics(compilerDiagnostics, options);

                foreach (TextSpan span in data.Spans)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var context = new CodeRefactoringContext(
                        document,
                        span,
                        a =>
                        {
                            if (data.EquivalenceKey == null
                                || string.Equals(a.EquivalenceKey, data.EquivalenceKey, StringComparison.Ordinal))
                            {
                                Fail("No code refactoring expected.");
                            }
                        },
                        cancellationToken);

                    await refactoringProvider.ComputeRefactoringsAsync(context);
                }
            }
        }
    }
}
