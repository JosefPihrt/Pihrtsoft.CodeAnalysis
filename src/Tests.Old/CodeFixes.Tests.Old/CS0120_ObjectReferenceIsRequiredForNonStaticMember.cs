// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

#pragma warning disable RCS1213

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static partial class CS0120_ObjectReferenceIsRequiredForNonStaticMember
    {
        private partial class Foo
        {
            public static void Bar()
            {
                Method();
                string s1 = Property;
                string s2 = _field;
                Event(default(object), EventArgs.Empty);
                Event2 += default(EventHandler);

                MethodInOtherFile();
            }

            private static string StaticProperty
            {
                get { return Property; }
            }

            private static void StaticMethod()
            {
                Method();
            }

            private static string StaticMethod2() => _field;

            private class Derived : Base
            {
                public Derived(string value, string value2)
                    : base(Bar())
                {
                }

                public Derived(string value)
                    : this(Bar(), null)
                {
                }

                public string Bar()
                {
                    return null;
                }
            }

            private class Base
            {
                public Base(string value)
                {
                }
            }

            private string _field;

            public void Method() { }

            public string Property { get; }

            public event EventHandler Event;

            public event EventHandler Event2
            {
                add { }
                remove { }
            }
        }
    }
}
