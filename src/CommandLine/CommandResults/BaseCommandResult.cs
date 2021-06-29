// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CommandLine
{
    internal class BaseCommandResult
    {
        public BaseCommandResult(CommandStatus status)
        {
            Status = status;
        }

        public static BaseCommandResult Fail { get; } = new BaseCommandResult(CommandStatus.Fail);

        public static BaseCommandResult Success { get; } = new BaseCommandResult(CommandStatus.Success);

        public static BaseCommandResult NotSuccess { get; } = new BaseCommandResult(CommandStatus.NotSuccess);

        public CommandStatus Status { get; }
    }
}
