// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS0693_TypeParameterHasSameNameAsTypeParameterFromOuterType
    {
        private class Foo<T>
        {
            private void Bar<T>()
            {
            }

            private void Bar<T, T2>()
                where T : class
                where T2 : class
            {
            }
        }
    }
}
