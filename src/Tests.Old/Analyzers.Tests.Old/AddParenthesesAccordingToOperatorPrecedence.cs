// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

#pragma warning disable RCS1176

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class AddParenthesesAccordingToOperatorPrecedence
    {
        private static void Foo()
        {
            const bool f1 = false;
            const bool f2 = false;
            const bool f3 = false;

            if (f1 || f2 && f3)
            {
            }

            if (f1 && f2 || f3)
            {
            }

            const int x = 1 * 1 - 1 * 1;

            int i = x * x + x * (x * x + x * (x * x + x * (x * x + x * x)));
        }
    }
}
