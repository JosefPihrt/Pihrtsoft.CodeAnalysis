// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CommandLine
{
    internal class FormatCommandResult : BaseCommandResult
    {
        public FormatCommandResult(CommandStatus status, int count)
            : base(status)
        {
            Count = count;
        }

        public int Count { get; }
    }
}
