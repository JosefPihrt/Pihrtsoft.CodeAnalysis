// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.Testing.CSharp
{
    /// <summary>
    /// Represents a verifier for a C# diagnostic that is produced by <see cref="DiagnosticAnalyzer"/>.
    /// </summary>
    public abstract class CSharpDiagnosticVerifier<TAnalyzer, TFixProvider> : DiagnosticVerifier<TAnalyzer, TFixProvider>
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TFixProvider : CodeFixProvider, new()
    {
        /// <summary>
        /// Initializes a new instance of <see cref="CSharpDiagnosticVerifier{TAnalyzer, TFixProvider}"/>.
        /// </summary>
        /// <param name="assert"></param>
        internal CSharpDiagnosticVerifier(IAssert assert) : base(assert)
        {
        }

        /// <summary>
        /// Gets a test options.
        /// </summary>
        new public virtual CSharpTestOptions Options => CSharpTestOptions.Default;

        /// <summary>
        /// Gets common test options.
        /// </summary>
        protected override TestOptions CommonOptions => Options;
    }
}
