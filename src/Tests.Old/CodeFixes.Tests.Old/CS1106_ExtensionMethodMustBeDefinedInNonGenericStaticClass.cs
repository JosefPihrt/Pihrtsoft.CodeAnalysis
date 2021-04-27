// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal class CS1106_ExtensionMethodMustBeDefinedInNonGenericStaticClass
    {
        private static void Foo(this object value)
        {
        }

        private static void Foo(this object value, object value2)
        {
        }
    }
}
