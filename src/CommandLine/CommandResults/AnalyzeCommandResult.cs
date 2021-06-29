// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Roslynator.Diagnostics;

namespace Roslynator.CommandLine
{
    internal class AnalyzeCommandResult : BaseCommandResult
    {
        public AnalyzeCommandResult(CommandStatus status, SimpleAnalysisResult analysisResult)
            : base(status)
        {
            AnalysisResult = analysisResult;
        }

        public SimpleAnalysisResult AnalysisResult { get; }
    }
}
