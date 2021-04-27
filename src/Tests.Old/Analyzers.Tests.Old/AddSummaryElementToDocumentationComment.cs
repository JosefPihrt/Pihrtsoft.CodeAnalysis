// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

#pragma warning disable RCS1100, RCS1101, RCS1163

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class AddSummaryElementToDocumentationComment
    {
        /// <param name="parameter"></param>
        /// <param name="parameter2"></param>
        /// <include />
        private static void Foo(object parameter)
        {
        }

        /// x
        private static void Foo2(object parameter)
        {
        }

        //n

        /// <summary>
        /// x
        /// </summary>
        /// <param name="parameter"></param>
        private static void Foo3(object parameter)
        {
            /// x
            if (parameter == null)
            {

            }
        }

        private static class InheritDoc
        {
            /// <inheritdoc />
            /// <param name="parameter"></param>
            /// <param name="parameter2"></param>
            private static void Foo(object parameter, object parameter2)
            {
            }
        }

        private static class Include
        {
            /// <include file='' path='[@name=""]' />
            private static void Foo()
            {
            }
        }

        private static class Exclude
        {
            /// <exclude />
            private static void Foo(object parameter, object parameter2)
            {
            }
        }
    }
}
