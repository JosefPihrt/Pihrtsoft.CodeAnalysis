// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.Spelling
{
    internal abstract class SpellingService : ISpellingService
    {
        public abstract ISyntaxFactsService SyntaxFacts { get; }

        public abstract DiagnosticAnalyzer CreateAnalyzer(
            SpellingData spellingData,
            SpellingFixerOptions options);

        public abstract ImmutableArray<Diagnostic> AnalyzeSpelling(
            SyntaxNode node,
            SpellingData spellingData,
            SpellingFixerOptions options,
            CancellationToken cancellationToken);

        public abstract SpellingDiagnostic CreateSpellingDiagnostic(Diagnostic diagnostic);
    }
}
