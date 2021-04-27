// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal partial class SynchronizeAccessibility
    {
        // 0/0
        partial class Foo
        {
        }

        partial class Foo
        {
        }

        // 1/1
        public partial class Foo2
        {
        }

        internal partial class Foo2
        {
        }

        partial class Foo2
        {
        }

        // 1/2
        public partial class Foo3
        {
        }

        protected internal partial class Foo3
        {
        }

        partial class Foo3
        {
        }
    }
}
