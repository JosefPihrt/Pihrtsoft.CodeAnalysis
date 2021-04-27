// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class DuplicateArgumentRefactoring
    {
        public void SomeMethod2(string value, DateTime dateTime)
        {
            SomeMethod2(
                value,
                );

            SomeMethod(new object());

            SomeMethod(, );

            SomeMethod(string.Empty, );
        }

        public void SomeMethod(params string[] values)
        {
        }
    }
}
