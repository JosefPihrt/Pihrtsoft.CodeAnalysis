// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS0133_ExpressionBeingAssignedMustBeConstant
    {
        public const int x = y;

        private static int y = 0;

        public static void Bar()
        {
            const Foo foo = new Foo();
        }

        private class Foo
        {
        }
    }
}
