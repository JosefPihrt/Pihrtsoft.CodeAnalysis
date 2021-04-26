// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;

namespace Roslynator.Testing.CSharp.Xunit
{
    /// <summary>
    /// Represents a verifier for C# compiler diagnostics.
    /// </summary>
    public abstract class XunitCompilerDiagnosticFixVerifier<TFixProvider> : CSharpCompilerDiagnosticFixVerifier<TFixProvider>
        where TFixProvider : CodeFixProvider, new()
    {
        /// <summary>
        /// Initializes a new instance of <see cref="XunitCompilerDiagnosticFixVerifier{TFixProvider}"/>
        /// </summary>
        protected XunitCompilerDiagnosticFixVerifier() : base(XunitAssert.Instance)
        {
        }
    }
}
