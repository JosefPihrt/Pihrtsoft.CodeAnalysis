// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable CS0660, CS0661

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS0216_OperatorRequiresMatchingOperatorToAlsoBeDefined
    {
        private class FooEquals
        {
            public static bool operator ==(FooEquals left, FooEquals right)
            {
                return false;
            }
        }

        private class FooNotEquals
        {
            public static bool operator !=(FooNotEquals left, FooNotEquals right)
            {
                return !(left == right);
            }
        }

        private class FooTrue
        {
            public static bool operator true(FooTrue value)
            {
                return false;
            }
        }

        private class FooFalse
        {
            public static bool operator false(FooFalse value)
            {
                return false;
            }
        }
    }
}
