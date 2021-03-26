// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.Formatting
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class BaseDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        protected BaseDiagnosticAnalyzer()
        {
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay
        {
            get { return $"{GetType()} {{{string.Join(", ", SupportedDiagnostics.Select(f => f.Id))}}}"; }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        }
    }
}
