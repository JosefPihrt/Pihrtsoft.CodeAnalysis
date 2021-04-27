// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable RCS1123

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class UseExclusiveOrOperator
    {
        private class Foo
        {
            private void Bar()
            {
                bool x = false;
                bool y = false;

                if ((x && !y) || (!x && y)) { }

                if (x && !y || (!x && y)) { }

                if ((x && !y) || !x && y) { }

                if (x && !y || !x && y) { }

                if (((x) && !(y)) || (!(x) && (y))) { }

                if ((!x && y) || (x && !y)) { }
            }
        }
    }
}
