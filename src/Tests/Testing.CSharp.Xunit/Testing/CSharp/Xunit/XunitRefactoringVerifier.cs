// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeRefactorings;

namespace Roslynator.Testing.CSharp.Xunit
{
    /// <summary>
    /// Represents verifier for a C# refactoring that is provided by <see cref="RefactoringVerifier.RefactoringProvider"/>
    /// </summary>
    public abstract class XunitRefactoringVerifier<TRefactoringProvider> : CSharpRefactoringVerifier<TRefactoringProvider>
        where TRefactoringProvider : CodeRefactoringProvider, new()
    {
        /// <summary>
        /// Initializes a new instance of <see cref="CSharpRefactoringVerifier"/>.
        /// </summary>
        protected XunitRefactoringVerifier() : base(XunitAssert.Instance)
        {
        }
    }
}
