// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using static Roslynator.Logger;

namespace Roslynator.CodeFixes
{
    internal static class DiagnosticFixProvider
    {
        public static async Task<DiagnosticFix> GetFixAsync(
            ImmutableArray<Diagnostic> diagnostics,
            DiagnosticDescriptor descriptor,
            ImmutableArray<CodeFixProvider> fixers,
            Project project,
            CodeFixerOptions options,
            IFormatProvider formatProvider = null,
            CancellationToken cancellationToken = default)
        {
            CodeFixProvider fixer = null;
            CodeAction fix = null;
            Document document = null;

            for (int i = 0; i < fixers.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                (CodeAction fixCandidate, Document documentCandidate) = await GetFixAsync(
                    diagnostics,
                    descriptor,
                    fixers[i],
                    project,
                    options,
                    formatProvider,
                    cancellationToken)
                    .ConfigureAwait(false);

                if (fixCandidate != null)
                {
                    if (fix == null)
                    {
                        if (options.DiagnosticFixerMap.IsEmpty
                            || !options.DiagnosticFixerMap.TryGetValue(descriptor.Id, out string fullTypeName)
                            || string.Equals(fixers[i].GetType().FullName, fullTypeName, StringComparison.Ordinal))
                        {
                            fix = fixCandidate;
                            fixer = fixers[i];
                            document = documentCandidate;
                        }
                    }
                    else if (options.DiagnosticFixerMap.IsEmpty
                        || !options.DiagnosticFixerMap.ContainsKey(descriptor.Id))
                    {
                        LogHelpers.WriteMultipleFixersSummary(descriptor.Id, fixer, fixers[i]);
                        return new DiagnosticFix(null, null, fixer, fixers[i]);
                    }
                }
            }

            return new DiagnosticFix(fix, document, fixer, null);
        }

        private static async Task<(CodeAction, Document)> GetFixAsync(
            ImmutableArray<Diagnostic> diagnostics,
            DiagnosticDescriptor descriptor,
            CodeFixProvider fixer,
            Project project,
            CodeFixerOptions options,
            IFormatProvider formatProvider = null,
            CancellationToken cancellationToken = default)
        {
            if (diagnostics.Length == 1)
                return await GetFixAsync(diagnostics[0], fixer, project, options, formatProvider, cancellationToken).ConfigureAwait(false);

            FixAllProvider fixAllProvider = fixer.GetFixAllProvider();

            if (fixAllProvider == null)
            {
                if (options.DiagnosticIdsFixableOneByOne.Contains(descriptor.Id))
                    return await GetFixAsync(diagnostics[0], fixer, project, options, formatProvider, cancellationToken).ConfigureAwait(false);

                WriteLine($"  Diagnostic '{descriptor.Id}' cannot be fixed with '{fixer.GetType().FullName}' because it does not have FixAllProvider and '{descriptor.Id}' is not allowed to be fixed one by one.", ConsoleColor.Yellow, Verbosity.Diagnostic);
                return default;
            }

            if (!fixAllProvider.GetSupportedFixAllDiagnosticIds(fixer).Any(f => f == descriptor.Id))
            {
                WriteLine($"  '{fixAllProvider.GetType().FullName}' does not support diagnostic '{descriptor.Id}'", ConsoleColor.Yellow, Verbosity.Diagnostic);
                return default;
            }

            if (!fixAllProvider.GetSupportedFixAllScopes().Any(f => f == options.FixAllScope))
            {
                WriteLine($"  '{fixAllProvider.GetType().FullName}' does not support scope '{options.FixAllScope}'", ConsoleColor.Yellow, Verbosity.Diagnostic);
                return default;
            }

            var multipleFixesInfos = new HashSet<MultipleFixesInfo>();

            options.DiagnosticFixMap.TryGetValue(descriptor.Id, out ImmutableArray<string> equivalenceKeys);

            foreach (Diagnostic diagnostic in diagnostics)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!diagnostic.Location.IsInSource)
                    continue;

                Document document = project.GetDocument(diagnostic.Location.SourceTree);

                if (document == null)
                    continue;

                CodeAction fix = await GetFixAsync(diagnostic, fixer, document, multipleFixesInfos, equivalenceKeys, cancellationToken).ConfigureAwait(false);

                if (fix == null)
                    continue;

                var fixAllContext = new FixAllContext(
                    document,
                    fixer,
                    options.FixAllScope,
                    fix.EquivalenceKey,
                    new string[] { descriptor.Id },
                    new FixAllDiagnosticProvider(diagnostics),
                    cancellationToken);

                CodeAction fixAll = await fixAllProvider.GetFixAsync(fixAllContext).ConfigureAwait(false);

                if (fixAll != null)
                {
                    WriteLine($"  CodeFixProvider: '{fixer.GetType().FullName}'", ConsoleColor.DarkGray, Verbosity.Diagnostic);

                    if (!string.IsNullOrEmpty(fix.EquivalenceKey))
                        WriteLine($"  EquivalenceKey:  '{fix.EquivalenceKey}'", ConsoleColor.DarkGray, Verbosity.Diagnostic);

                    WriteLine($"  FixAllProvider:  '{fixAllProvider.GetType().FullName}'", ConsoleColor.DarkGray, Verbosity.Diagnostic);

                    return (fixAll, document);
                }

                WriteLine($"  Fixer '{fixer.GetType().FullName}' registered no action for diagnostic '{descriptor.Id}'", ConsoleColor.DarkGray, Verbosity.Diagnostic);
                LogHelpers.WriteDiagnostic(diagnostic, baseDirectoryPath: Path.GetDirectoryName(project.FilePath), formatProvider: formatProvider, indentation: "    ", verbosity: Verbosity.Diagnostic);
            }

            return default;
        }

        private static async Task<(CodeAction, Document)> GetFixAsync(
            Diagnostic diagnostic,
            CodeFixProvider fixer,
            Project project,
            CodeFixerOptions options,
            IFormatProvider formatProvider,
            CancellationToken cancellationToken)
        {
            if (!diagnostic.Location.IsInSource)
                return default;

            Document document = project.GetDocument(diagnostic.Location.SourceTree);

            Debug.Assert(document != null, "");

            if (document == null)
                return default;

            options.DiagnosticFixMap.TryGetValue(diagnostic.Id, out ImmutableArray<string> equivalenceKeys);

            CodeAction action = await GetFixAsync(diagnostic, fixer, document, multipleFixesInfos: default, equivalenceKeys, cancellationToken).ConfigureAwait(false);

            if (action == null)
            {
                WriteLine($"  Fixer '{fixer.GetType().FullName}' registered no action for diagnostic '{diagnostic.Id}'", ConsoleColor.DarkGray, Verbosity.Diagnostic);
                LogHelpers.WriteDiagnostic(diagnostic, baseDirectoryPath: Path.GetDirectoryName(project.FilePath), formatProvider: formatProvider, indentation: "    ", verbosity: Verbosity.Diagnostic);
            }

            return (action, document);
        }

        private static async Task<CodeAction> GetFixAsync(
            Diagnostic diagnostic,
            CodeFixProvider fixer,
            Document document,
            HashSet<MultipleFixesInfo> multipleFixesInfos,
            ImmutableArray<string> equivalenceKeys,
            CancellationToken cancellationToken)
        {
            CodeAction action = null;

            var context = new CodeFixContext(
                document,
                diagnostic,
                (a, _) =>
                {
                    if (!equivalenceKeys.IsDefaultOrEmpty
                        && !equivalenceKeys.Contains(a.EquivalenceKey, StringComparer.Ordinal))
                    {
                        return;
                    }

                    if (action == null)
                    {
                        action = a;
                    }
                    else
                    {
                        var multipleFixesInfo = new MultipleFixesInfo(diagnostic.Id, fixer, action.EquivalenceKey, a.EquivalenceKey);

                        if (multipleFixesInfos == null)
                            multipleFixesInfos = new HashSet<MultipleFixesInfo>();

                        if (multipleFixesInfos.Add(multipleFixesInfo))
                            WriteMultipleActionsSummary(multipleFixesInfo);

                        action = null;
                    }
                },
                cancellationToken);

            await fixer.RegisterCodeFixesAsync(context).ConfigureAwait(false);

            return action;
        }

        private static void WriteMultipleActionsSummary(in MultipleFixesInfo info)
        {
            WriteLine($"  '{info.Fixer.GetType().FullName}' registered multiple actions to fix diagnostic '{info.DiagnosticId}'", ConsoleColor.Yellow, Verbosity.Diagnostic);
            WriteLine($"    EquivalenceKey 1: '{info.EquivalenceKey1}'", ConsoleColor.Yellow, Verbosity.Diagnostic);
            WriteLine($"    EquivalenceKey 2: '{info.EquivalenceKey2}'", ConsoleColor.Yellow, Verbosity.Diagnostic);
        }

        private readonly struct MultipleFixesInfo : IEquatable<MultipleFixesInfo>
        {
            public MultipleFixesInfo(string diagnosticId, CodeFixProvider fixer, string equivalenceKey1, string equivalenceKey2)
            {
                DiagnosticId = diagnosticId;
                Fixer = fixer;
                EquivalenceKey1 = equivalenceKey1;
                EquivalenceKey2 = equivalenceKey2;
            }

            public string DiagnosticId { get; }

            public CodeFixProvider Fixer { get; }

            public string EquivalenceKey1 { get; }

            public string EquivalenceKey2 { get; }

            public override bool Equals(object obj)
            {
                return obj is MultipleFixesInfo other && Equals(other);
            }

            public bool Equals(MultipleFixesInfo other)
            {
                return DiagnosticId == other.DiagnosticId
                    && Fixer == other.Fixer
                    && EquivalenceKey1 == other.EquivalenceKey1
                    && EquivalenceKey2 == other.EquivalenceKey2;
            }

            public override int GetHashCode()
            {
                return Hash.Combine(
                    DiagnosticId,
                    Hash.Combine(
                        Fixer,
                        Hash.Combine(
                            EquivalenceKey1,
                            Hash.Create(EquivalenceKey2))));
            }

            public static bool operator ==(in MultipleFixesInfo info1, in MultipleFixesInfo info2)
            {
                return info1.Equals(info2);
            }

            public static bool operator !=(in MultipleFixesInfo info1, in MultipleFixesInfo info2)
            {
                return !(info1 == info2);
            }
        }
    }
}
