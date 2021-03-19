// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Roslynator.Testing;

namespace Roslynator.VisualBasic.Testing
{
    public abstract class VisualBasicCompilerDiagnosticFixVerifier<TFixProvider> : CompilerDiagnosticFixVerifier<TFixProvider>
        where TFixProvider : CodeFixProvider, new()
    {
        internal VisualBasicCompilerDiagnosticFixVerifier(IAssert assert) : base(assert)
        {
        }

        new public virtual VisualBasicTestOptions Options => VisualBasicTestOptions.Default;

        protected override TestOptions CommonOptions => Options;
    }
}
