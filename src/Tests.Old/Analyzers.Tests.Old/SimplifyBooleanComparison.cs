// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable RCS1118, RCS1023

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class SimplifyBooleanComparison
    {
        public static void Foo()
        {
            bool f = false;

            if (f == false) { }

            if (f != true) { }

            if (false == f) { }

            if (true != f) { }

            if (f
#if DEBUG
                == false) { }
#endif

            bool? x = null;

            if (!x == true) { } // !x == false

            if (!x != true) { } // !x != false

            if (!x == false) { } // !x == true

            if (!x != false) { } // !x != true
        }
    }
}
