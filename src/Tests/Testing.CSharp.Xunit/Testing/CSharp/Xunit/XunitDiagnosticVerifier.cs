// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.Testing.CSharp.Xunit
{
    /// <summary>
    /// Represents a verifier for a C# diagnostic that is produced by <see cref="DiagnosticAnalyzer"/>.
    /// </summary>
    public abstract class XunitDiagnosticVerifier<TAnalyzer, TFixProvider> : CSharpDiagnosticVerifier<TAnalyzer, TFixProvider>
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TFixProvider : CodeFixProvider, new()
    {
        /// <summary>
        /// Initializes a new instance of <see cref="XunitDiagnosticVerifier{TAnalyzer, TFixProvider}"/>.
        /// </summary>
        protected XunitDiagnosticVerifier() : base(XunitAssert.Instance)
        {
        }
    }
}
