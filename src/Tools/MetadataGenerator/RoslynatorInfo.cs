// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;

namespace Roslynator.CodeGeneration
{
    public class RoslynatorInfo
    {
        public RoslynatorInfo(
            Solution solution,
            IEnumerable<ISymbol> diagnosticDescriptors,
            IEnumerable<ISymbol> diagnosticIdentifiers,
            IEnumerable<ISymbol> refactoringIdentifiers,
            IEnumerable<ISymbol> compilerDiagnosticIdentifiers)
        {
            Solution = solution;
            DiagnosticDescriptors = diagnosticDescriptors.ToImmutableArray();
            DiagnosticIdentifiers = diagnosticIdentifiers.ToImmutableArray();
            RefactoringIdentifiers = refactoringIdentifiers.ToImmutableArray();
            CompilerDiagnosticIdentifiers = compilerDiagnosticIdentifiers.ToImmutableArray();

            SolutionDirectory = Path.GetDirectoryName(solution.FilePath);
        }

        public Solution Solution { get; }

        public string SolutionDirectory { get; }

        public ImmutableArray<ISymbol> DiagnosticDescriptors { get; }

        public ImmutableArray<ISymbol> DiagnosticIdentifiers { get; }

        public ImmutableArray<ISymbol> RefactoringIdentifiers { get; }

        public ImmutableArray<ISymbol> CompilerDiagnosticIdentifiers { get; }

        public static async Task<RoslynatorInfo> Create(Solution solution, CancellationToken cancellationToken = default)
        {
            AnalyzersInfo analyzersInfo = await AnalyzersInfo.Create(solution, "Analyzers", "Roslynator.CSharp.DiagnosticRules", "Roslynator.CSharp.DiagnosticIdentifiers").ConfigureAwait(false);
            AnalyzersInfo codeAnalysisAnalyzersInfo = await AnalyzersInfo.Create(solution, "CodeAnalysis.Analyzers", "Roslynator.CodeAnalysis.CSharp.DiagnosticRules", "Roslynator.CodeAnalysis.CSharp.DiagnosticIdentifiers").ConfigureAwait(false);
            AnalyzersInfo formattingAnalyzersInfo = await AnalyzersInfo.Create(solution, "Formatting.Analyzers", "Roslynator.Formatting.CSharp.DiagnosticRules", "Roslynator.Formatting.CSharp.DiagnosticIdentifiers").ConfigureAwait(false);

            Compilation refactoringsCompilation = await solution.Projects.First(f => f.Name == "Refactorings").GetCompilationAsync(cancellationToken).ConfigureAwait(false);

            ImmutableArray<ISymbol> refactoringIdentifiers = refactoringsCompilation.GetTypeByMetadataName("Roslynator.CSharp.Refactorings.RefactoringIdentifiers").GetMembers();

            Compilation csharpCompilation = await solution.Projects.First(f => f.Name == "CSharp").GetCompilationAsync(cancellationToken).ConfigureAwait(false);

            ImmutableArray<ISymbol> compilerDiagnosticIdentifiers = csharpCompilation.GetTypeByMetadataName("Roslynator.CSharp.CompilerDiagnosticIdentifiers").GetMembers();

            return new RoslynatorInfo(
                solution,
                analyzersInfo.Descriptors.Concat(codeAnalysisAnalyzersInfo.Descriptors).Concat(formattingAnalyzersInfo.Descriptors),
                analyzersInfo.Identifiers.Concat(codeAnalysisAnalyzersInfo.Identifiers).Concat(formattingAnalyzersInfo.Identifiers),
                refactoringIdentifiers,
                compilerDiagnosticIdentifiers);
        }

        public async Task<IEnumerable<string>> GetAnalyzerFilesAsync(string identifier, CancellationToken cancellationToken = default)
        {
            ISymbol diagnosticDescriptor = DiagnosticDescriptors.FirstOrDefault(f => f.Name == identifier);

            if (diagnosticDescriptor == null)
                throw new InvalidOperationException($"Diagnostic descriptor symbol not found for identifier '{identifier}'.");

            IEnumerable<ReferencedSymbol> referencedSymbols = await SymbolFinder.FindReferencesAsync(diagnosticDescriptor, Solution, cancellationToken).ConfigureAwait(false);

            ISymbol diagnosticIdentifier = DiagnosticIdentifiers.FirstOrDefault(f => f.Name == identifier);

            if (diagnosticIdentifier == null)
                throw new InvalidOperationException($"Diagnostic identifier symbol not found for identifier '{identifier}'.");

            IEnumerable<ReferencedSymbol> referencedSymbols2 = await SymbolFinder.FindReferencesAsync(diagnosticIdentifier, Solution, cancellationToken).ConfigureAwait(false);

            return GetFilePaths(referencedSymbols.Concat(referencedSymbols2));
        }

        public async Task<IEnumerable<string>> GetRefactoringFilesAsync(string identifier, CancellationToken cancellationToken = default)
        {
            ISymbol symbol = RefactoringIdentifiers.FirstOrDefault(f => f.Name == identifier);

            if (symbol == null)
                throw new InvalidOperationException($"Refactoring identifier symbol not found for identifier '{identifier}'.");

            IEnumerable<ReferencedSymbol> referencedSymbols = await SymbolFinder.FindReferencesAsync(symbol, Solution, cancellationToken).ConfigureAwait(false);

            return GetFilePaths(referencedSymbols);
        }

        public async Task<IEnumerable<string>> GetCompilerDiagnosticFilesAsync(string identifier, CancellationToken cancellationToken = default)
        {
            ISymbol symbol = CompilerDiagnosticIdentifiers.FirstOrDefault(f => f.Name == identifier);

            if (symbol == null)
                throw new InvalidOperationException($"Compiler diagnostic identifier symbol not found for identifier '{identifier}'.");

            IEnumerable<ReferencedSymbol> referencedSymbols = await SymbolFinder.FindReferencesAsync(symbol, Solution, cancellationToken).ConfigureAwait(false);

            return GetFilePaths(referencedSymbols);
        }

        private IEnumerable<string> GetFilePaths(IEnumerable<ReferencedSymbol> referencedSymbols)
        {
            return referencedSymbols
                .SelectMany(f => f.Locations)
                .Where(f => !f.IsCandidateLocation && !f.IsImplicit && !GeneratedCodeUtility.IsGeneratedCodeFile(f.Document.FilePath))
                .Select(f => f.Document.FilePath.Replace(SolutionDirectory, ""))
                .Distinct()
                .OrderBy(f => f);
        }

        private class AnalyzersInfo
        {
            public AnalyzersInfo(ImmutableArray<ISymbol> diagnosticDescriptors, ImmutableArray<ISymbol> diagnosticIdentifiers)
            {
                Descriptors = diagnosticDescriptors;
                Identifiers = diagnosticIdentifiers;
            }

            public static async Task<AnalyzersInfo> Create(
                Solution solution,
                string projectName,
                string descriptorsTypeName,
                string identifiersTypeName,
                CancellationToken cancellationToken = default)
            {
                Project analyzersProject = solution.Projects.First(f => f.Name == projectName);

                Compilation analyzersCompilation = await analyzersProject.GetCompilationAsync(cancellationToken).ConfigureAwait(false);

                ImmutableArray<ISymbol> diagnosticDescriptors = analyzersCompilation.GetTypeByMetadataName(descriptorsTypeName).GetMembers();

                ImmutableArray<ISymbol> diagnosticIdentifiers = analyzersCompilation.GetTypeByMetadataName(identifiersTypeName).GetMembers();

                return new AnalyzersInfo(diagnosticDescriptors, diagnosticIdentifiers);
            }

            public ImmutableArray<ISymbol> Descriptors { get; }

            public ImmutableArray<ISymbol> Identifiers { get; }
        }
    }
}
