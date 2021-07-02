// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Roslynator.Diagnostics;

namespace Roslynator.CommandLine
{
    internal class AnalyzeCommandResult : CommandResult
    {
        public AnalyzeCommandResult(CommandStatus status, ImmutableArray<ProjectAnalysisResult> analysisResults)
            : base(status)
        {
            AnalysisResults = analysisResults;
        }

        public ImmutableArray<ProjectAnalysisResult> AnalysisResults { get; }
    }
}
