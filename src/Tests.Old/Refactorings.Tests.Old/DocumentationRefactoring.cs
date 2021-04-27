// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.Refactorings.Tests
{
#pragma warning disable RCS1016, RCS1100, RCS1106, RCS1138
    public static class DocumentationRefactoring
    {
        public class Foo
        {
            public class Foo2
            {
                public class Foo3
                {
                }
            }

            public string FieldName;

            public const string ConstantName = null;

            public Foo(object parameter)
            {
            }

            ~Foo()
            {
            }

            public event EventHandler EventName;

            protected virtual void OnEventName(EventArgs e)
            {
                EventName?.Invoke(this, e);
            }

            public event EventHandler<EventArgs> EventName2;

            protected virtual void OnEventName2(EventArgs e)
            {
                EventHandler<EventArgs> handler = EventName2;
                if (handler != null)
                    handler(this, e);
            }

            public string PropertyName { get; set; }

            public string this[int index]
            {
                get { return null; }
                set { }
            }

            public void MethodNameVoid<T>(object parameter)
            {
            }

            public string MethodName<T>(object parameter)
            {
                return null;
            }

            public static explicit operator Foo(string value)
            {
                return new Foo(null);
            }

            public static explicit operator string(Foo value)
            {
                return null;
            }

            public static Foo operator !(Foo value)
            {
                return new Foo(null);
            }

            public enum EnumName
            {

            }

            public interface InterfaceName<T>
            {
            }

            public struct StructName<T>
            {
            }

            public class ClassName<T>
            {
            }

            public delegate void DelegateName<T>(object parameter);
        }
    }
}