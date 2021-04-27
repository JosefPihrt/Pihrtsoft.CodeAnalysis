// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    public static class RemoveImplementationFromAbstractOrInterfaceMember
    {
        public interface Foo
        {
            string MethodName()
            {
                return "";
            }

            string MethodName2() => null;

            string PropertyName
            {
                get { }
                set { }
            }

            string PropertyName2
            {
                get => null;
                set => value = null;
            }

            string PropertyName3 => null;

            object this[int index]
            {
                get {  }
                set {  }
            }

            object this[long index]
            {
                get => null;
                set => value = null;
            }

            object this[ulong index] => null;

            event EventHandler EventName
            {
                add { }
                remove { }
            }
        }

        public abstract class Bar
        {
            public abstract string MethodName()
            {
                return "";
            }

            public abstract string MethodName2() => null;

            public abstract string PropertyName
            {
                get {  }
                set {  }
            }

            public abstract string PropertyName2
            {
                get => null;
                set => value = null;
            }

            public abstract string PropertyName3 => null;

            public abstract object this[int index]
            {
                get { }
                set { }
            }

            public abstract object this[long index]
            {
                get => null;
                set => value = null;
            }

            public abstract object this[ulong index] => null;


            public abstract event EventHandler EventName
            {
                add { }
                remove { }
            }
        }
    }
}
