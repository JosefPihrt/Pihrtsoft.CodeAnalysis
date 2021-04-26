// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
        private ImmutableDictionary<string, ImmutableArray<TextSpan>> _annotationsByKind;

        /// <summary>
        /// Initializes a new instance of <see cref="ExpectedTestState"/>.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="codeActionTitle"></param>
        /// <param name="annotations"></param>
        /// <param name="alwaysVerifyAnnotations"></param>
        public ExpectedTestState(
            string source,
            string codeActionTitle = null,
            IEnumerable<(string, TextSpan)> annotations = null,
            IEnumerable<string> alwaysVerifyAnnotations = null)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            CodeActionTitle = codeActionTitle;
            Annotations = annotations?.ToImmutableArray() ?? ImmutableArray<(string, TextSpan)>.Empty;
            AlwaysVerifyAnnotations = alwaysVerifyAnnotations?.ToImmutableArray() ?? ImmutableArray<string>.Empty;
        }

        /// <summary>
        /// Gets expected source code.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Gets expected code action's title.
        /// </summary>
        public string CodeActionTitle { get; }

        /// <summary>
        /// Gets expected annotations.
        /// </summary>
        public ImmutableArray<(string kind, TextSpan span)> Annotations { get; }

        /// <summary>
        /// Gets annotations that should be always verified.
        /// </summary>
        public ImmutableArray<string> AlwaysVerifyAnnotations { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => Source;

        internal ImmutableDictionary<string, ImmutableArray<TextSpan>> AnnotationsByKind
        {
            get
            {
                if (_annotationsByKind == null)
                {
                    Interlocked.CompareExchange(
                        ref _annotationsByKind,
                        Annotations.GroupBy(f => f.kind).ToImmutableDictionary(f => f.Key, f => f.Select(f => f.span).ToImmutableArray()),
                        null);
                }

                return _annotationsByKind;
            }
        }

        internal static ExpectedTestState Parse(string text)
        {
            (string source, ImmutableArray<(string, TextSpan)> annotations) = TextProcessor.FindAnnotatedSpansAndRemove(text);

            return new ExpectedTestState(source, annotations: annotations);
        }
    }
}
