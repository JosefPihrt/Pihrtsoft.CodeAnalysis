// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class OverridingMethodCannotChangeAccessModifiers
    {
        public class Foo : FooBase
        {
            /// <summary></summary>
            protected internal override string Method() => null;

            internal override string Property => null;

            internal override string this[int index] => null;

            internal override event EventHandler Event;

            internal override event EventHandler Event2;

            internal override string Method2() => null;

            override string Method3() => null;
        }

        public abstract class FooBase
        {
            public abstract string Method();

            public abstract string Property { get; }

            public abstract string this[int index] { get; }

            public abstract event EventHandler Event;

            public abstract event EventHandler Event2;

            protected internal abstract string Method2();

            internal protected abstract string Method3();
        }
    }
}
