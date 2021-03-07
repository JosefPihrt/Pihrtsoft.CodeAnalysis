﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.Testing
{
    /// <summary>
    /// Represents base type for verifying a diagnostic, a code fix and a refactoring.
    /// </summary>
    public abstract class CodeVerifier
    {
        internal CodeVerifier(IAssert assert)
        {
            Assert = assert;
        }

        /// <summary>
        /// Gets a common code verification options.
        /// </summary>
        protected abstract TestOptions CommonOptions { get; }

        /// <summary>
        /// Gets a code verification options.
        /// </summary>
        public TestOptions Options => CommonOptions;

        internal IAssert Assert { get; }

        internal void VerifyCompilerDiagnostics(
            ImmutableArray<Diagnostic> diagnostics,
            TestOptions options)
        {
            foreach (Diagnostic diagnostic in diagnostics)
            {
                if (!options.IsAllowedCompilerDiagnostic(diagnostic))
                {
                    Assert.True(false, $"No compiler diagnostics with severity higher than '{options.AllowedCompilerDiagnosticSeverity}' expected"
                        + diagnostics.Where(d => !options.IsAllowedCompilerDiagnostic(d)).ToDebugString());
                }
            }
        }

        internal void VerifyNoNewCompilerDiagnostics(
            ImmutableArray<Diagnostic> diagnostics,
            ImmutableArray<Diagnostic> newDiagnostics,
            TestOptions options)
        {
            foreach (Diagnostic newDiagnostic in newDiagnostics)
            {
                if (!options.IsAllowedCompilerDiagnostic(newDiagnostic)
                    && IsNewCompilerDiagnostic(newDiagnostic))
                {
                    IEnumerable<Diagnostic> diff = newDiagnostics
                        .Where(diagnostic => !options.IsAllowedCompilerDiagnostic(diagnostic))
                        .Except(diagnostics, DiagnosticDeepEqualityComparer.Instance);

                    var message = "Code fix introduced new compiler diagnostic";

                    if (diff.Count() > 1)
                        message += "s";

                    message += ".";

                    Assert.True(false, message + diff.ToDebugString());
                }
            }

            bool IsNewCompilerDiagnostic(Diagnostic newDiagnostic)
            {
                foreach (Diagnostic diagnostic in diagnostics)
                {
                    if (DiagnosticDeepEqualityComparer.Instance.Equals(diagnostic, newDiagnostic))
                        return false;
                }

                return true;
            }
        }

        internal async Task VerifyAdditionalDocumentsAsync(
            Project project,
            ImmutableArray<ExpectedDocument> expectedDocuments,
            CancellationToken cancellationToken = default)
        {
            foreach (ExpectedDocument expectedDocument in expectedDocuments)
            {
                Document document = project.GetDocument(expectedDocument.Id);

                SyntaxNode root = await document.GetSyntaxRootAsync(simplify: true, format: true, cancellationToken);

                string actual = root.ToFullString();

                Assert.Equal(expectedDocument.Text, actual);
            }
        }

        internal async Task<Document> VerifyAndApplyCodeActionAsync(
            Document document,
            CodeAction codeAction,
            string title)
        {
            if (title != null)
                Assert.Equal(title, codeAction.Title);

            ImmutableArray<CodeActionOperation> operations = await codeAction.GetOperationsAsync(CancellationToken.None);

            return operations
                .OfType<ApplyChangesOperation>()
                .Single()
                .ChangedSolution
                .GetDocument(document.Id);
        }

        internal void VerifySupportedDiagnostics(
            DiagnosticAnalyzer analyzer,
            ImmutableArray<Diagnostic> diagnostics)
        {
            foreach (Diagnostic diagnostic in diagnostics)
                VerifySupportedDiagnostics(analyzer, diagnostic);
        }

        internal void VerifySupportedDiagnostics(DiagnosticAnalyzer analyzer, Diagnostic diagnostic)
        {
            if (analyzer.SupportedDiagnostics.IndexOf(diagnostic.Descriptor, DiagnosticDescriptorComparer.Id) == -1)
                Assert.True(false, $"Diagnostic \"{diagnostic.Id}\" is not supported by '{analyzer.GetType().Name}'.");
        }

        internal void VerifyFixableDiagnostics(CodeFixProvider fixProvider, string diagnosticId)
        {
            ImmutableArray<string> fixableDiagnosticIds = fixProvider.FixableDiagnosticIds;

            if (!fixableDiagnosticIds.Contains(diagnosticId))
                Assert.True(false, $"Diagnostic '{diagnosticId}' is not fixable by '{fixProvider.GetType().Name}'.");
        }

        internal async Task VerifyExpectedDocument(
            ExpectedTestState expected,
            Document document,
            CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(simplify: true, format: true, cancellationToken);

            string actual = root.ToFullString();

            Assert.Equal(expected.Source, actual);

            ImmutableDictionary<string, ImmutableArray<TextSpan>> expectedSpans = expected.Spans;

            if (!expectedSpans.IsEmpty)
                VerifyAnnotations(expectedSpans, root, actual);
        }

        private void VerifyAnnotations(
            ImmutableDictionary<string, ImmutableArray<TextSpan>> expectedSpans,
            SyntaxNode root,
            string source)
        {
            foreach (KeyValuePair<string, ImmutableArray<TextSpan>> kvp in expectedSpans)
            {
                string kind = GetAnnotationKind(kvp.Key);

                ImmutableArray<TextSpan> spans = kvp.Value;

                ImmutableArray<SyntaxToken> tokens = root.GetAnnotatedTokens(kind).OrderBy(f => f.SpanStart).ToImmutableArray();

                if (spans.Length != tokens.Length)
                    Assert.True(false, $"{spans.Length} '{kind}' annotation(s) expected, actual: {tokens.Length}");

                for (int i = 0; i < spans.Length; i++)
                {
                    TextSpan expected = spans[i];
                    TextSpan actual = tokens[i].Span;

                    if (expected != actual)
                    {
                        string message = VerifyLinePositionSpan(
                            expected.ToLinePositionSpan(source),
                            actual.ToLinePositionSpan(source));

                        if (message != null)
                            Assert.True(false, $"Annotation '{kind}'{message}");
                    }
                }
            }

            static string GetAnnotationKind(string value)
            {
                if (string.Equals(value, "n", StringComparison.OrdinalIgnoreCase))
                    return "CodeAction_Navigation";

                if (string.Equals(value, "r", StringComparison.OrdinalIgnoreCase))
                    return RenameAnnotation.Kind;

                return value;
            }
        }

        internal static string VerifyLinePositionSpan(LinePositionSpan expected, LinePositionSpan actual)
        {
            return VerifyLinePosition(expected.Start, actual.Start, "start")
                ?? VerifyLinePosition(expected.End, actual.End, "end");
        }

        private static string VerifyLinePosition(
            LinePosition expected,
            LinePosition actual,
            string startOrEnd)
        {
            int expectedLine = expected.Line;
            int actualLine = actual.Line;

            if (expectedLine != actualLine)
                return $" expected to {startOrEnd} on line {expectedLine + 1}, actual: {actualLine + 1}";

            int expectedCharacter = expected.Character;
            int actualCharacter = actual.Character;

            if (expectedCharacter != actualCharacter)
                return $" expected to {startOrEnd} at column {expectedCharacter + 1}, actual: {actualCharacter + 1}";

            return null;
        }

        internal static (Document document, ImmutableArray<ExpectedDocument> expectedDocuments)
            CreateDocument(Solution solution, TestState state, TestOptions options)
        {
            const string DefaultProjectName = "TestProject";

            CompilationOptions compilationOptions = options.CompilationOptions;

            Project project = solution
                .AddProject(DefaultProjectName, DefaultProjectName, options.Language)
                .WithMetadataReferences(options.MetadataReferences)
                .WithCompilationOptions(compilationOptions)
                .WithParseOptions(options.ParseOptions);

            Document document = project.AddDocument(options.DocumentName, SourceText.From(state.Source));

            ImmutableArray<ExpectedDocument>.Builder expectedDocuments = null;

            ImmutableArray<AdditionalFile> additionalFiles = state.AdditionalFiles;

            if (!additionalFiles.IsEmpty)
            {
                expectedDocuments = ImmutableArray.CreateBuilder<ExpectedDocument>();
                project = document.Project;

                for (int i = 0; i < additionalFiles.Length; i++)
                {
                    Document additionalDocument = project.AddDocument(AppendNumberToFileName(options.DocumentName, i + 2), SourceText.From(additionalFiles[i].Source));
                    expectedDocuments.Add(new ExpectedDocument(additionalDocument.Id, additionalFiles[i].ExpectedSource));
                    project = additionalDocument.Project;
                }

                document = project.GetDocument(document.Id);
            }

            return (document, expectedDocuments?.ToImmutableArray() ?? ImmutableArray<ExpectedDocument>.Empty);

            static string AppendNumberToFileName(string fileName, int number)
            {
                int index = fileName.LastIndexOf(".");

                return fileName.Insert(index, (number).ToString(CultureInfo.InvariantCulture));
            }
        }
    }
}
