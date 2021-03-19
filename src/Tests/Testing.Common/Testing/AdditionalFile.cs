// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Roslynator.Testing
{
    /// <summary>
    /// Represents additional code file.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly struct AdditionalFile
    {
        /// <summary>
        /// Initializes a new instance of <see cref="AdditionalFile"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="expectedSource"></param>
        public AdditionalFile(string source, string expectedSource = null)
        {
            Source = source;
            ExpectedSource = expectedSource;
        }

        /// <summary>
        /// Gets a source code.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Gets expected source code.
        /// </summary>
        public string ExpectedSource { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => Source;

        internal static ImmutableArray<AdditionalFile> CreateRange(IEnumerable<string> additionalFiles)
        {
            return additionalFiles?.Select(f => new AdditionalFile(f)).ToImmutableArray()
                ?? ImmutableArray<AdditionalFile>.Empty;
        }

        internal static ImmutableArray<AdditionalFile> CreateRange(IEnumerable<(string source, string expectedSource)> additionalFiles)
        {
            return additionalFiles?.Select(f => new AdditionalFile(f.source, f.expectedSource)).ToImmutableArray()
                ?? ImmutableArray<AdditionalFile>.Empty;
        }
    }
}
