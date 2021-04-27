// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

#pragma warning disable RCS1100, RCS1131, RCS1139, RCS1163, RCS1164

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal class AddParameterToDocumentationComment
    {
        /// <summary>
        /// x
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="parameter3"></param>
        /// <param name="parameter5"></param>
        public void Foo(object parameter, object parameter2, object parameter3, object parameter4, object parameter5)
        {
        }

        /// <param name="parameter2"></param>
        public void Foo2(object parameter, object parameter2, object parameter3)
        {
        }

        /// <summary>
        /// x
        /// </summary>
        public void Foo(object parameter, object parameter2, object parameter3)
        {
        }

        /// <typeparam name="T"></typeparam>
        /// <include file='' path='[@name=""]' />
        public void Foo3<T>(object parameter)
        {
        }

        /// <summary>
        /// x
        /// </summary>
        /// <returns></returns>
        public string this[int index] => "";

        private class InheritDoc
        {
            /// <inheritdoc />
            /// <summary>
            /// x
            /// </summary>
            /// <param name="parameter"></param>
            /// <param name="parameter3"></param>
            /// <param name="parameter5"></param>
            public void Foo(object parameter, object parameter2, object parameter3, object parameter4, object parameter5)
            {
            }

            /// <inheritdoc />
            /// <param name="parameter2"></param>
            public void Foo2(object parameter, object parameter2, object parameter3)
            {
            }

            /// <inheritdoc />
            /// <summary>
            /// x
            /// </summary>
            public void Foo(object parameter, object parameter2, object parameter3)
            {
            }

            /// <inheritdoc />
            /// <typeparam name="T"></typeparam>
            public void Foo3<T>(object parameter)
            {
            }
        }

        private class Include
        {
            /// <include file='' path='[@name=""]' />
            public void Foo(object parameter)
            {
            }

            /// <include file='' path='[@name=""]' />
            public void Foo(object parameter, object parameter2)
            {
            }
        }

        private class Exclude
        {
            /// <exclude />
            public void Foo(object parameter)
            {
            }

            /// <exclude />
            public void Foo(object parameter, object parameter2)
            {
            }
        }
    }
}
