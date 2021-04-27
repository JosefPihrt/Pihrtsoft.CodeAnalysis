// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS0077_AsOperatorMustBeUsedWithReferenceTypeOrNullableType
    {
        private static void Foo()
        {
            object o = null;

            int i = o as int;

            //n

            StringBuilder sb = null;

            i = sb as int;
        }
    }
}
