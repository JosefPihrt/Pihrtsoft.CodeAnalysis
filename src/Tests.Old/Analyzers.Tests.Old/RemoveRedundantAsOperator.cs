// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

#pragma warning disable RCS1010

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class RemoveRedundantAsOperator
    {
        private static void Foo()
        {
            string s = null;

            string s2 = s as string;
        }
    }
}
