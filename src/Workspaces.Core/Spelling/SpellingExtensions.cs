// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Roslynator.Spelling
{
    internal static class SpellingExtensions
    {
        public static SpellingData AddIgnoredValues(this SpellingData spellingData, IEnumerable<SpellingDiagnostic> diagnostics)
        {
            return spellingData.AddIgnoredValues(diagnostics.Select(f => f.Value));
        }
    }
}