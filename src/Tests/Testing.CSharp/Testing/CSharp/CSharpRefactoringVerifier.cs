// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeRefactorings;

namespace Roslynator.Testing.CSharp
{
    /// <summary>
    /// Represents verifier for a C# code refactoring.
    /// </summary>
    public abstract class CSharpRefactoringVerifier<TRefactoringProvider> : RefactoringVerifier<TRefactoringProvider>
        where TRefactoringProvider : CodeRefactoringProvider, new()
    {
        /// <summary>
        /// Initializes a new instance of <see cref="CSharpRefactoringVerifier{TRefactoringProvider}"/>.
        /// </summary>
        /// <param name="assert"></param>
        internal CSharpRefactoringVerifier(IAssert assert) : base(assert)
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
