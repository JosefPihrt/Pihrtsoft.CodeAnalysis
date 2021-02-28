﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
    /// Represents verifier for a refactoring that is provided by <see cref="RefactoringProvider"/>
    /// </summary>
    public abstract class RefactoringVerifier<TRefactoringProvider> : CodeVerifier
        where TRefactoringProvider : CodeRefactoringProvider, new()
    {
        internal RefactoringVerifier(IAssert assert) : base(assert)
        {
        }

        public async Task VerifyRefactoringAsync(
            RefactoringTestState state,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            if (state.Spans.IsEmpty)
                Assert.True(false, "Span on which a refactoring should be invoked was not found.");

            options ??= Options;

            TRefactoringProvider refactoringProvider = Activator.CreateInstance<TRefactoringProvider>();

            foreach (TextSpan span in state.Spans)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (Workspace workspace = new AdhocWorkspace())
                {
                    (Document document, ImmutableArray<ExpectedDocument> expectedDocuments) = ProjectHelpers.CreateDocument(workspace.CurrentSolution, state, options);

                    SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken);

                    ImmutableArray<Diagnostic> compilerDiagnostics = semanticModel.GetDiagnostics(cancellationToken: cancellationToken);

                    VerifyCompilerDiagnostics(compilerDiagnostics, options);

                    CodeAction action = null;

                    var context = new CodeRefactoringContext(
                        document,
                        span,
                        a =>
                        {
                            if (state.EquivalenceKey == null
                                || string.Equals(a.EquivalenceKey, state.EquivalenceKey, StringComparison.Ordinal))
                            {
                                if (action != null)
                                    Assert.True(false, "Multiple fixes available.");

                                action = a;
                            }
                        },
                        cancellationToken);

                    await refactoringProvider.ComputeRefactoringsAsync(context);

                    Assert.True(action != null, "No code refactoring has been registered.");

                    document = await VerifyAndApplyCodeActionAsync(document, action, state.CodeActionTitle);
                    semanticModel = await document.GetSemanticModelAsync(cancellationToken);

                    ImmutableArray<Diagnostic> newCompilerDiagnostics = semanticModel.GetDiagnostics(cancellationToken: cancellationToken);

                    VerifyCompilerDiagnostics(newCompilerDiagnostics, options);
                    VerifyNoNewCompilerDiagnostics(compilerDiagnostics, newCompilerDiagnostics, options);

                    string actual = await document.ToFullStringAsync(simplify: true, format: true, cancellationToken);

                    Assert.Equal(state.ExpectedSource, actual);
                }
            }
        }

        public async Task VerifyNoRefactoringAsync(
            RefactoringTestState state,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            if (state.Spans.IsEmpty)
                Assert.True(false, "Span on which a refactoring should be invoked was not found.");

            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;

            TRefactoringProvider refactoringProvider = Activator.CreateInstance<TRefactoringProvider>();

            using (Workspace workspace = new AdhocWorkspace())
            {
                (Document document, ImmutableArray<ExpectedDocument> expectedDocuments) = ProjectHelpers.CreateDocument(workspace.CurrentSolution, state, options);

                SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken);

                ImmutableArray<Diagnostic> compilerDiagnostics = semanticModel.GetDiagnostics(cancellationToken: cancellationToken);

                VerifyCompilerDiagnostics(compilerDiagnostics, options);

                foreach (TextSpan span in state.Spans)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var context = new CodeRefactoringContext(
                        document,
                        span,
                        a =>
                        {
                            if (state.EquivalenceKey == null
                                || string.Equals(a.EquivalenceKey, state.EquivalenceKey, StringComparison.Ordinal))
                            {
                                Assert.True(false, "No code refactoring expected.");
                            }
                        },
                        cancellationToken);

                    await refactoringProvider.ComputeRefactoringsAsync(context);
                }
            }
        }
    }
}
