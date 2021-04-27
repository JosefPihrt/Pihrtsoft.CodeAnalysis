// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using static System.Diagnostics.Debug;

#pragma warning disable RCS1016, RCS1163

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class CallDebugFailInsteadOfDebugAssert
    {
        private static void Foo()
        {
            Debug.Assert(false);
            Debug.Assert(false, "message");
            Debug.Assert(false, "message", "detailMessage");

            Assert(false);
            Assert(false, "message");
            Assert(false, "message", "detailMessage");

            // n

            Debug.Assert(true, "message");
            Debug.Assert(true, "message", "detailMessage");
        }

        public static class CallDebugFailInsteadOfDebugAssert2
        {
            private static void Foo()
            {
                Assert(false, "message");
            }

            private static void Fail(string message)
            {
            }
        }
    }
}
