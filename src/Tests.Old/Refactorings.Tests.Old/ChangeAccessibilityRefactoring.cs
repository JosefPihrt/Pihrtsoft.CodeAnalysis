// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable RCS1170

using System;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public partial class ChangeAccessibilityRefactoring
    {
        //method
        public void Method() { }

        public string Property { get; private set; }

        protected internal string Property2 { get; protected set; }

        public string Property3 { get; protected set; }

        public string Property4 { get; internal set; }

        //public
        public partial class Foo { }
        public partial class Foo { }
        partial class Foo { }

        //protected internal
        protected internal partial class Foo2 { }
        protected internal partial class Foo2 { }
        partial class Foo2 { }

        protected internal partial class Foo3 { }
        protected internal partial class Foo3 { }

        private abstract class FooAbstract
        {
            public abstract void Method();

            public abstract string Property { get; }

            public abstract string this[int index] { get; }

            public abstract event EventHandler Event;

            public abstract event EventHandler Event2;

            public override string ToString()
            {
                return null;
            }
        }

        // abstract, virtual, override
        private class FooDerived : FooAbstract
        {
            public override string this[int index] => throw new NotImplementedException();

            public override string Property => throw new NotImplementedException();

            public override void Method()
            {
                throw new NotImplementedException();
            }

            public override event EventHandler Event;

            public override event EventHandler Event2
            {
                add { }
                remove { }
            }

            public override string ToString()
            {
                return null;
            }
        }
    }
}
