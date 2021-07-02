// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Roslynator.CodeFixes;

namespace Roslynator.CommandLine
{
    internal class FixCommandResult : CommandResult
    {
        public FixCommandResult(CommandStatus status, ImmutableArray<ProjectFixResult> fixResults)
            : base(status)
        {
            FixResults = fixResults;
        }

        public ImmutableArray<ProjectFixResult> FixResults { get; }
    }
}
