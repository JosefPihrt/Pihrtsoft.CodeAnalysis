// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.Testing.Text;

namespace Roslynator.Testing
{
    /// <summary>
    /// Represents expected test data.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class ExpectedTestState
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ExpectedTestState"/>.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="spans"></param>
        /// <param name="codeActionTitle"></param>
        /// <param name="alwaysVerifyAnnotations"></param>
        public ExpectedTestState(
            string source,
            IEnumerable<KeyValuePair<string, ImmutableArray<TextSpan>>> spans = null,
            string codeActionTitle = null,
            IEnumerable<string> alwaysVerifyAnnotations = null)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Spans = spans?.ToImmutableDictionary() ?? ImmutableDictionary<string, ImmutableArray<TextSpan>>.Empty;
            CodeActionTitle = codeActionTitle;
            AlwaysVerifyAnnotations = alwaysVerifyAnnotations?.ToImmutableArray() ?? ImmutableArray<string>.Empty;
        }

        /// <summary>
        /// Gets expected source code.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Gets expected text spans grouped by annotations.
        /// </summary>
        public ImmutableDictionary<string, ImmutableArray<TextSpan>> Spans { get; }

        /// <summary>
        /// Gets expected code action's title.
        /// </summary>
        public string CodeActionTitle { get; }

        /// <summary>
        /// Gets annotations that should be always verified.
        /// </summary>
        public ImmutableArray<string> AlwaysVerifyAnnotations { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => Source;

        internal static ExpectedTestState Parse(string text)
        {
            (string source, ImmutableDictionary<string, ImmutableArray<TextSpan>> spans) = TextProcessor.FindAnnotatedSpansAndRemove(text);

            return new ExpectedTestState(source, spans);
        }
    }
}
