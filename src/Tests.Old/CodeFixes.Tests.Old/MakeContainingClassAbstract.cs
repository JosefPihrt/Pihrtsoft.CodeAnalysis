// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class MakeContainingClassAbstract
    {
        public class Foo
        {
            public abstract string MethodName();
        }

        public class Foo2
        {
            public abstract string PropertyName { get; set; }
        }

        public class Foo3
        {
            public abstract string this[int index] { get; set; }
        }

        public class Foo4
        {
            public abstract event EventHandler EventName;
        }
    }
}
