// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

#pragma warning disable CS0168, CS8321, RCS1100, RCS1016, RCS1021, RCS1023, RCS1048, RCS1079, RCS1140, RCS1176, RCS1185

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class UnusedParameter
    {
        private static void ExtensionMethod(this Foo foo, object parameter)
        {
        }

        private abstract class Foo : IFoo
        {
            protected Foo(object parameter)
            {
                object LocalFunction(object value) => null;
            }

            public string this[int index] => "";

            public string this[int index, string value]
            {
                get { return value; }
            }

            public void Bar5<T>(object parameter)
            {
                T x;
            }

            public object Bar6(object parameter1, object parameter2) => parameter2;

            /// <summary>
            /// ...
            /// </summary>
            /// <param name="parameter1"></param>
            /// <param name="parameter2"></param>
            public void Bar7(object parameter1, object parameter2) { }

            public void Bar8(object parameter1, object parameter2) { Bar7(parameter1: null, parameter2: null); }

            public void SimpleLambda()
            {
                Func<string, string> func1 = f => { return ""; };
                Func<string, string> func2 = f => "";
            }

            public void ParenthesizedLambda()
            {
                Func<string, string, string> func1 = (f, f2) => { return f2; };
                Func<string, string, string> func2 = (f, f2) => f2;
            }

            public void AnonymousMethod()
            {
                Func<string, string, string> func = delegate (string f, string f2) { return f2; };
            }

            public static Foo operator +(Foo left, Foo right)
            {
                return right;
            }

            public static explicit operator Foo(string value)
            {
                return null;
            }

            // n

            public void Bar(object parameter) { }

            public virtual void Bar2(object parameter) { }

            public abstract void Bar3(object parameter);

            public void EventHandlerMethod(object sender, EventArgs args) { }

            public void EventHandlerMethod2(object sender, ConsoleCancelEventArgs args) { }

            public void Bar4(object parameter)
            {
                Bar4(parameter);
            }

            public void SimpleLambda2()
            {
                Func<string, string> func1 = f => { return f; };
                Func<string, string> func2 = f => f;
            }

            public void ParenthesizedLambda2()
            {
                Func<string, string, string> func1 = (f, f2) => { return f + f2; };
                Func<string, string, string> func2 = (f, f2) => f + f2;
            }

            public void AnonymousMethod2()
            {
                Func<string, string, string> func = delegate (string f, string f2) { return f + f2; };
            }

            public void AnonymousEventHandler()
            {
                Action<object, EventArgs> func1 = (sender, e) => { return; };

                Action<object, EventArgs> func2 = delegate (object sender, EventArgs e) { return; };
            }
        }

        private static void ExtensionMethod2(this Foo foo, object parameter)
        {
            throw new NotImplementedException();
        }

        private abstract class Foo2
        {
            protected Foo2(object parameter)
            {
                throw new NotImplementedException();
            }

            protected Foo2(object parameter, object parameter2) => throw new NotImplementedException();

            public string this[int index] => throw new NotImplementedException();

            public string this[string index]
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public string this[string index, string index2]
            {
                get => throw new NotImplementedException();
                set => throw new NotImplementedException();
            }

            public void Bar(object parameter) { throw new NotImplementedException(); }

            public object Bar(object parameter1, object parameter2) => throw new NotImplementedException();

            /// <summary>
            /// ...
            /// </summary>
            /// <param name="parameter1"></param>
            /// <param name="parameter2"></param>
            public void Bar2(object parameter1, object parameter2) { throw new NotImplementedException(); }

            public static Foo2 operator +(Foo2 left, Foo2 right)
            {
                throw new NotImplementedException();
            }

            public static explicit operator Foo2(string value)
            {
                throw new NotImplementedException();
            }
        }

        private interface IFoo
        {
            void Bar(object parameter);
        }

        private class ParameterNameConsistsOfUnderscore
        {
            public void Bar()
            {
                IEnumerable<string> items1 = new List<string>().OrderBy(_ => 0);

                IEnumerable<string> items2 = new List<string>().Select((_, __) => "");
            }

            public void Bar(string _)
            {

            }

            public void Bar(string _, object __)
            {
            }
        }

        private class FooSerializable : ISerializable
        {
            protected FooSerializable(SerializationInfo info, StreamingContext context)
            {
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                throw new NotImplementedException();
            }
        }

        private static class FooInheritedInterfaceImplementation
        {
            private interface IFoo
            {
                string this[int index] { get; }

                bool Bar(string value);
            }

            private interface IFoo2 : IFoo
            {
            }

            private class Foo2 : IFoo2
            {
                public string this[int index] => null;

                public bool Bar(string value) => true;
            }
        }
    }
}
