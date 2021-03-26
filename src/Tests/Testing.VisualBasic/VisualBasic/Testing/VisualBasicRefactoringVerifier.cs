// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeRefactorings;
using Roslynator.Testing;

namespace Roslynator.VisualBasic.Testing
{
    public abstract class VisualBasicRefactoringVerifier<TRefactoringProvider> : RefactoringVerifier<TRefactoringProvider>
        where TRefactoringProvider : CodeRefactoringProvider, new()
    {
        internal VisualBasicRefactoringVerifier(IAssert assert) : base(assert)
        {
        }

        new public virtual VisualBasicTestOptions Options => VisualBasicTestOptions.Default;

        protected override TestOptions CommonOptions => Options;
    }
}
