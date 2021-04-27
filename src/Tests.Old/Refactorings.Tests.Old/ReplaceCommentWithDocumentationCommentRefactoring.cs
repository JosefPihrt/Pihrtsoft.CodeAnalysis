// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

#pragma warning disable RCS1016, RCS1100, RCS1106, RCS1138, RCS1163, RCS1164, RCS1176, RCS1181

// x
namespace Roslynator.CSharp.Refactorings.Tests
{
    // x
    public static class ReplaceCommentWithDocumentationCommentRefactoring
    {
        // x
        // xx
        public class Foo
        {
            // x
            public string FieldName;

            // x
            public const string ConstantName = null;

            // x
            public Foo(object parameter)
            {
            }

            // x
            ~Foo()
            {
            }

            // x
            public event EventHandler EventName;

            // x
            protected virtual void OnEventName(EventArgs e)
            {
                EventName?.Invoke(this, e);
            }

            // x
            public event EventHandler<EventArgs> EventName2;

            // x
            protected virtual void OnEventName2(EventArgs e)
            {
                EventName2?.Invoke(this, e);
            }

            // x
            public string PropertyName { get; set; }

            // x
            public string this[int index]
            {
                get { return null; }
                set { }
            }

            // x
            public void MethodNameVoid<T>(object parameter)
            {
            }

            // x
            public string MethodName<T>(object parameter)
            {
                return null;
            }

            // x
            public static explicit operator Foo(string value)
            {
                return new Foo(null);
            }

            // x
            public static explicit operator string(Foo value)
            {
                return null;
            }

            // x
            public static Foo operator !(Foo value)
            {
                return new Foo(null);
            }

            // x
            public enum EnumName
            {
                // x
                None
            }

            // x
            public interface IInterfaceName<T>
            {
            }

            // x
            public struct StructName<T>
            {
            }

            // x
            public class ClassName<T>
            {
            }

            // x
            public delegate void DelegateName<T>(object parameter);
        }
    }
}
