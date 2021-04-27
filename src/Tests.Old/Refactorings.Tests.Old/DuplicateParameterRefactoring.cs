// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class DuplicateParameterRefactoring
    {
        public void SomeMethod(IEnumerable<string> values, )
        {
        }

        public void SomeMethod(, )
        {
        }

        public void SomeMethod2(
            IEnumerable<string> values,
            )
        {
        }
    }
}
