// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

#pragma warning disable CS0168, RCS1007, RCS1016, RCS1021, RCS1100, RCS1101, RCS1118, RCS1138, RCS1139, RCS1141, RCS1163, RCS1164, RCS1176, RCS1194

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal class AddExceptionToDocumentationComment
    {
        /// <summary></summary>
        /// <param name="parameter"></param>
        public void Foo(object parameter, object parameter2, object parameter3)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            if (parameter2 == null)
                throw new ArgumentNullException(nameof(parameter2));

            if (parameter == null)
                throw new ArgumentException(nameof(parameter));

            if (parameter == null)
                throw new Exception<string>();

            object x = parameter3 ?? throw new InvalidOperationException(nameof(parameter3));
        }

        /// <typeparam name="T"></typeparam>
        public void Foo<T>()
        {
            throw new Exception<T>();
        }

        /// <typeparam name="T"></typeparam>
        public void FooLocalFunction()
        {
            void Local()
            {
                throw new InvalidOperationException();
            }

            void Local2()
            {
                void Local3()
                {
                    throw new Exception<string>();
                }
            }

            void Local4() => throw new ArgumentException();
        }

        /// <summary></summary>
        public string FooProperty
        {
            get
            {
                string s = null;

                if (s == null)
                    throw new ArgumentNullException(nameof(s));

                return s;
            }
        }

        /// <summary></summary>
        public string FooProperty2 => throw new InvalidOperationException();

        public class Exception<T> : Exception
        {
            /// <summary></summary>
            public void Foo()
            {
                throw new Exception<string>();
            }

            /// <summary></summary>
            public void Foo2()
            {
                throw new Exception<T>();
            }
        }

        //n

        private class InheritDoc
        {
            /// <inheritdoc />
            /// <summary></summary>
            /// <param name="parameter"></param>
            public void Foo(object parameter, object parameter2, object parameter3)
            {
                if (parameter == null)
                    throw new ArgumentNullException(nameof(parameter));
            }
        }

        private class Include
        {
            /// <include file='' path='[@name=""]' />
            public void Foo(object parameter, object parameter2, object parameter3)
            {
                if (parameter == null)
                    throw new ArgumentNullException(nameof(parameter));
            }
        }

        private class Exclude
        {
            /// <exclude />
            public void Foo(object parameter, object parameter2, object parameter3)
            {
                if (parameter == null)
                    throw new ArgumentNullException(nameof(parameter));
            }
        }

        /// <summary></summary>
        public void Foo3()
        {
            Func<object, object> func = f =>
            {
                throw new InvalidOperationException();
            };
        }

        /// <summary></summary>
        public void Foo4()
        {
            Func<object, object> func2 = f => throw new InvalidOperationException();
        }
    }
}
