// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

#pragma warning disable RCS1100, RCS1101

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class AddSummaryToDocumentationComment
    {
        /// <summary>
        /// x
        /// </summary>
        private static void Foo()
        {
        }

        /// <summary>x</summary>
        private static void Foo2()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        private static void Foo3()
        {
        }

        /// <summary>
        ///     
        /// </summary>
        private static void Foo4()
        {
        }

        /// <summary>
        /// </summary>
        private static void Foo5()
        {
        }

        /// <summary></summary>
        private static void Foo6()
        {
        }

        /// <summary> </summary>
        private static void Foo7()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        [Obsolete]
        private static void Foo8()
        {
        }

        private static class InheritDoc
        {
            /// <inheritdoc />
            /// <summary>
            /// 
            /// </summary>
            private static void Foo3()
            {
            }

            /// <inheritdoc />
            /// <summary>
            ///     
            /// </summary>
            private static void Foo4()
            {
            }

            /// <inheritdoc />
            /// <summary>
            /// </summary>
            private static void Foo5()
            {
            }

            /// <inheritdoc />
            /// <summary></summary>
            private static void Foo6()
            {
            }

            /// <inheritdoc />
            /// <summary> </summary>
            private static void Foo7()
            {
            }
        }

        private static class Include
        {
            /// <include file='' path='[@name=""]' />
            /// <summary>
            /// 
            /// </summary>
            private static void Foo3()
            {
            }

            /// <include file='' path='[@name=""]' />
            /// <summary>
            ///     
            /// </summary>
            private static void Foo4()
            {
            }

            /// <include file='' path='[@name=""]' />
            /// <summary>
            /// </summary>
            private static void Foo5()
            {
            }

            /// <include file='' path='[@name=""]' />
            /// <summary></summary>
            private static void Foo6()
            {
            }

            /// <include file='' path='[@name=""]' />
            /// <summary> </summary>
            private static void Foo7()
            {
            }
        }

        private static class Exclude
        {
            /// <exclude />
            /// <summary>
            /// 
            /// </summary>
            private static void Foo3()
            {
            }

            /// <exclude />
            /// <summary>
            ///     
            /// </summary>
            private static void Foo4()
            {
            }

            /// <exclude />
            /// <summary>
            /// </summary>
            private static void Foo5()
            {
            }

            /// <exclude />
            /// <summary></summary>
            private static void Foo6()
            {
            }

            /// <exclude />
            /// <summary> </summary>
            private static void Foo7()
            {
            }
        }
    }
}
