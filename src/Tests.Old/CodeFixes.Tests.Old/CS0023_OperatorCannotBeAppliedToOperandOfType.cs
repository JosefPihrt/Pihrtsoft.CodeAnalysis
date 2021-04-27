// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS0023_OperatorCannotBeAppliedToOperandOfType
    {
        private class Foo
        {
            public void Bar()
            {
                DateTime dt = DateTime.Now;

                DateTime date = dt?.Date;

                string s = null;

                s = s?.ToString;
            }
        }
    }
}
