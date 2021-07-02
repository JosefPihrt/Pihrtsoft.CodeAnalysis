// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Roslynator.Spelling;

namespace Roslynator.CommandLine
{
    internal class SpellcheckCommandResult : CommandResult
    {
        public SpellcheckCommandResult(CommandStatus status, ImmutableArray<SpellingFixResult> spellingResults)
            : base(status)
        {
            SpellingResults = spellingResults;
        }

        public ImmutableArray<SpellingFixResult> SpellingResults { get; }
    }
}
