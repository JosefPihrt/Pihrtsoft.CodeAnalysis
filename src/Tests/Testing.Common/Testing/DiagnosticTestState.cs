// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.Testing
{
    /// <summary>
    /// Represents test data for a diagnostic and its fix.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class DiagnosticTestState
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DiagnosticTestState"/>.
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="source"></param>
        /// <param name="spans"></param>
        /// <param name="additionalSpans"></param>
        /// <param name="additionalFiles"></param>
        /// <param name="diagnosticMessage"></param>
        /// <param name="formatProvider"></param>
        /// <param name="equivalenceKey"></param>
        /// <param name="alwaysVerifyAdditionalLocations"></param>
        public DiagnosticTestState(
            DiagnosticDescriptor descriptor,
            string source,
            IEnumerable<TextSpan> spans,
            IEnumerable<TextSpan> additionalSpans = null,
            IEnumerable<AdditionalFile> additionalFiles = null,
            string diagnosticMessage = null,
            IFormatProvider formatProvider = null,
            string equivalenceKey = null,
            bool alwaysVerifyAdditionalLocations = false)
        {
            Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Spans = spans?.ToImmutableArray() ?? ImmutableArray<TextSpan>.Empty;
            AdditionalSpans = additionalSpans?.ToImmutableArray() ?? ImmutableArray<TextSpan>.Empty;
            AdditionalFiles = additionalFiles?.ToImmutableArray() ?? ImmutableArray<AdditionalFile>.Empty;
            DiagnosticMessage = diagnosticMessage;
            FormatProvider = formatProvider;
            EquivalenceKey = equivalenceKey;
            AlwaysVerifyAdditionalLocations = alwaysVerifyAdditionalLocations;

            if (Spans.Length > 1
                && !AdditionalSpans.IsEmpty)
            {
                throw new ArgumentException("", nameof(additionalSpans));
            }
        }

        internal DiagnosticTestState(DiagnosticTestState other)
            : this(
                descriptor: other.Descriptor,
                source: other.Source,
                spans: other.Spans,
                additionalSpans: other.AdditionalSpans,
                additionalFiles: other.AdditionalFiles,
                diagnosticMessage: other.DiagnosticMessage,
                formatProvider: other.FormatProvider,
                equivalenceKey: other.EquivalenceKey,
                alwaysVerifyAdditionalLocations: other.AlwaysVerifyAdditionalLocations)
        {
        }

        /// <summary>
        /// Gets diagnostic's descriptor.
        /// </summary>
        public DiagnosticDescriptor Descriptor { get; }

        /// <summary>
        /// Gets source that will report specified diagnostic.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Gets diagnostic's locations.
        /// </summary>
        public ImmutableArray<TextSpan> Spans { get; }

        /// <summary>
        /// Gets diagnostic's additional locations.
        /// </summary>
        public ImmutableArray<TextSpan> AdditionalSpans { get; }

        /// <summary>
        /// Gets additional source files.
        /// </summary>
        public ImmutableArray<AdditionalFile> AdditionalFiles { get; }

        /// <summary>
        /// Gets diagnostic's message
        /// </summary>
        public string DiagnosticMessage { get; }

        /// <summary>
        /// Gets format provider to be used to format diagnostic's message.
        /// </summary>
        public IFormatProvider FormatProvider { get; }

        /// <summary>
        /// Gets code action's equivalence key.
        /// </summary>
        public string EquivalenceKey { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{Descriptor.Id}  {Source}";

        /// <summary>
        /// True if additional locations should be always verified.
        /// </summary>
        public bool AlwaysVerifyAdditionalLocations { get; }

        internal ImmutableArray<Diagnostic> GetDiagnostics(SyntaxTree tree)
        {
            return ImmutableArray.CreateRange(
                Spans,
                span => Diagnostic.Create(
                    Descriptor,
                    Location.Create(tree, span),
                    additionalLocations: AdditionalSpans.Select(span => Location.Create(tree, span)).ToImmutableArray()));
        }

        /// <summary>
        /// Creates and return new instance of <see cref="DiagnosticTestState"/> updated with specified values.
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="source"></param>
        /// <param name="spans"></param>
        /// <param name="additionalSpans"></param>
        /// <param name="additionalFiles"></param>
        /// <param name="diagnosticMessage"></param>
        /// <param name="formatProvider"></param>
        /// <param name="equivalenceKey"></param>
        /// <param name="alwaysVerifyAdditionalLocations"></param>
        public DiagnosticTestState Update(
            DiagnosticDescriptor descriptor,
            string source,
            IEnumerable<TextSpan> spans,
            IEnumerable<TextSpan> additionalSpans,
            IEnumerable<AdditionalFile> additionalFiles,
            string diagnosticMessage,
            IFormatProvider formatProvider,
            string equivalenceKey,
            bool alwaysVerifyAdditionalLocations)
        {
            return new DiagnosticTestState(
                descriptor: descriptor,
                source: source,
                spans: spans,
                additionalSpans: additionalSpans,
                additionalFiles: additionalFiles,
                diagnosticMessage: diagnosticMessage,
                formatProvider: formatProvider,
                equivalenceKey: equivalenceKey,
                alwaysVerifyAdditionalLocations: alwaysVerifyAdditionalLocations);
        }
    }
}
