// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using CommandLine;

namespace Roslynator.CommandLine
{
    [Verb("analyze", HelpText = "Analyzes specified project or solution and reports diagnostics.")]
    public class AnalyzeCommandLineOptions : AbstractAnalyzeCommandLineOptions
    {
        [Option(
            longName: "execution-time",
            HelpText = "Indicates whether to measure execution time of each analyzer.")]
        public bool ExecutionTime { get; set; }

        [Option(
            longName: "ignore-compiler-diagnostics",
            HelpText = "Indicates whether to display compiler diagnostics.")]
        public bool IgnoreCompilerDiagnostics { get; set; }

        [Option(
            shortName: OptionShortNames.Output,
            longName: "output",
            HelpText = "Defines path to file that will store reported diagnostics in XML format.",
            MetaValue = "<OUTPUT_FILE>")]
        public string Output { get; set; }

        [Option(
            longName: "report-not-configurable",
            HelpText = "Indicates whether diagnostics that have tag 'NotConfigurable' should be reported.")]
        public bool ReportNotConfigurable { get; set; }

        [Option(
            longName: "report-suppressed-diagnostics",
            HelpText = "Indicates whether suppressed diagnostics should be reported.")]
        public bool ReportSuppressedDiagnostics { get; set; }
    }
}
