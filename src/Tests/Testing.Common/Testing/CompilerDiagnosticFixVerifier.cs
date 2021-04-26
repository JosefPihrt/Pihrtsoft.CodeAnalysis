// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Roslynator.Testing
{
    /// <summary>
    /// Represents a verifier for compiler diagnostic.
    /// </summary>
    public abstract class CompilerDiagnosticFixVerifier<TFixProvider> : CodeVerifier
        where TFixProvider : CodeFixProvider, new()
    {
        internal CompilerDiagnosticFixVerifier(IAssert assert) : base(assert)
        {
        }

        /// <summary>
        /// Verifies that specified source will produce compiler diagnostic.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="expected"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyFixAsync(
            CompilerDiagnosticFixTestData data,
            ExpectedTestState expected,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (expected == null)
                throw new ArgumentNullException(nameof(expected));

            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;

            TFixProvider fixProvider = Activator.CreateInstance<TFixProvider>();

            VerifyFixableDiagnostics(fixProvider, data.DiagnosticId);

            using (Workspace workspace = new AdhocWorkspace())
            {
                (Document document, ImmutableArray<ExpectedDocument> expectedDocuments) = CreateDocument(workspace.CurrentSolution, data.Source, data.AdditionalFiles, options);

                Project project = document.Project;

                document = project.GetDocument(document.Id);

                ImmutableArray<Diagnostic> previousDiagnostics = ImmutableArray<Diagnostic>.Empty;

                var fixRegistered = false;

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    Compilation compilation = await document.Project.GetCompilationAsync(cancellationToken);

                    ImmutableArray<Diagnostic> diagnostics = compilation.GetDiagnostics(cancellationToken: cancellationToken);

                    int length = diagnostics.Length;

                    if (length == 0)
                    {
                        if (!fixRegistered)
                            Fail("No compiler diagnostic found.");

                        break;
                    }

                    if (previousDiagnostics.Any())
                        VerifyNoNewCompilerDiagnostics(previousDiagnostics, diagnostics, options);

                    if (DiagnosticDeepEqualityComparer.Equals(diagnostics, previousDiagnostics))
                        Fail("Same diagnostics returned before and after the fix was applied.", diagnostics);

                    Diagnostic diagnostic = FindDiagnosticToFix(diagnostics);

                    if (diagnostic == null)
                    {
                        if (!fixRegistered)
                            Fail($"No compiler diagnostic with ID '{data.DiagnosticId}' found.", diagnostics);

                        break;
                    }

                    CodeAction action = null;
                    List<CodeAction> candidateActions = null;

                    var context = new CodeFixContext(
                        document,
                        diagnostic,
                        (a, d) =>
                        {
                            if ((data.EquivalenceKey == null
                                || string.Equals(data.EquivalenceKey, a.EquivalenceKey, StringComparison.Ordinal))
                                && d.Contains(diagnostic))
                            {
                                if (action != null)
                                    Fail($"Multiple fixes registered by '{fixProvider.GetType().Name}'.", new CodeAction[] { action, a });

                                action = a;
                            }
                            else
                            {
                                (candidateActions ??= new List<CodeAction>()).Add(a);
                            }
                        },
                        cancellationToken);

                    await fixProvider.RegisterCodeFixesAsync(context);

                    if (action == null)
                        Fail("No code fix has been registered.", candidateActions);

                    fixRegistered = true;

                    document = await VerifyAndApplyCodeActionAsync(document, action, expected.CodeActionTitle);

                    previousDiagnostics = diagnostics;
                }

                await VerifyExpectedDocument(expected, document, cancellationToken);

                if (expectedDocuments.Any())
                    await VerifyAdditionalDocumentsAsync(document.Project, expectedDocuments, cancellationToken);
            }

            Diagnostic FindDiagnosticToFix(ImmutableArray<Diagnostic> diagnostics)
            {
                Diagnostic match = null;

                foreach (Diagnostic diagnostic in diagnostics)
                {
                    if (string.Equals(diagnostic.Id, data.DiagnosticId, StringComparison.Ordinal))
                    {
                        if (match == null
                            || diagnostic.Location.SourceSpan.Start > match.Location.SourceSpan.Start)
                        {
                            match = diagnostic;
                        }
                    }
                }

                return match;
            }
        }

        /// <summary>
        /// Verifies that specified source will not produce compiler diagnostic.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyNoFixAsync(
            CompilerDiagnosticFixTestData data,
            TestOptions options = null,
            CancellationToken cancellationToken = default)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;

            TFixProvider fixProvider = Activator.CreateInstance<TFixProvider>();
            ImmutableArray<string> fixableDiagnosticIds = fixProvider.FixableDiagnosticIds;

            using (Workspace workspace = new AdhocWorkspace())
            {
                (Document document, ImmutableArray<ExpectedDocument> _) = CreateDocument(workspace.CurrentSolution, data.Source, data.AdditionalFiles, options);

                Compilation compilation = await document.Project.GetCompilationAsync(cancellationToken);

                foreach (Diagnostic diagnostic in compilation.GetDiagnostics(cancellationToken: cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!fixableDiagnosticIds.Contains(diagnostic.Id))
                        continue;

                    var context = new CodeFixContext(
                        document,
                        diagnostic,
                        (a, d) =>
                        {
                            if (data.EquivalenceKey != null
                                && !string.Equals(a.EquivalenceKey, data.EquivalenceKey, StringComparison.Ordinal))
                            {
                                return;
                            }

                            if (!d.Contains(diagnostic))
                                return;

                            Fail("No code fix expected.");
                        },
                        cancellationToken);

                    await fixProvider.RegisterCodeFixesAsync(context);
                }
            }
        }
    }
}
