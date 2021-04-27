// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

#pragma warning disable RCS1016, RCS1039, RCS1081, RCS1100, RCS1163, RCS1164, RCS1175, RCS1176

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class UnusedMemberDeclaration
    {
        private static class Foo
        {
            private const string _f = "";
            private static readonly string _f2, _f3;

            /// <summary>
            /// ...
            /// </summary>
            private static void FooMethod()
            {
                string s = _f2;

                EventHandler eh = FooEvent3;

                FooMethod<string>();

                s.FooExtensionMethod<string>();
            }

            private static void FooMethod<T>()
            {
            }

            private static string FooProperty { get; } = _f3;

            private static event EventHandler FooEvent;
            private static event EventHandler FooEvent2, FooEvent3;

            private delegate void FooDelegate();
        }

        private static void FooExtensionMethod<T>(this T value)
        {
        }

        // n

        private partial class FooPartial
        {
            private void FooMethod()
            {
            }
        }

        private partial class FooPartial
        {
        }

        private class Base
        {
            public Base(string value)
            {
            }
        }

        private class Derived : Base
        {
            public Derived(string value)
                : base(Bar())
            {
            }

            private static string Bar()
            {
                return null;
            }
        }

        [FooAttribute(A)]
        public class Foo2
        {
            private const string A = "";
            private const string B = "";
            private const string C = "";
            private const int D = 0;

            [FooAttribute(B)]
            public Foo2(string x = C)
            {
            }

            public enum FooEnum
            {
                None = D,
            }
        }

        [AttributeUsageAttribute(AttributeTargets.All, AllowMultiple = false)]
        private sealed class FooAttribute : Attribute
        {
            public FooAttribute(string value)
            {
            }
        }

        private static class Program
        {
            private static void Main(string[] args)
            {
            }

            private static void Main()
            {
            }
        }

        [DebuggerDisplay(@"\\ \{ \} {GetDebuggerDisplay()} \\ \{ \} {DebuggerDisplay}")]
        private class FooDebuggerDisplay
        {
            private string GetDebuggerDisplay()
            {
                return "";
            }

            private string DebuggerDisplay
            {
                get { return ""; }
            }

            [DebuggerDisplay]
            private class FooDebuggerDisplay2
            {
            }

            [DebuggerDisplay()]
            private class FooDebuggerDisplay3
            {
            }

            [DebuggerDisplay("")]
            private class FooDebuggerDisplay4
            {
            }

            [DebuggerDisplay("}")]
            private class FooDebuggerDisplay5
            {
            }
        }

        private static class FooDelegateAsParameterType
        {
            private delegate void FooDelegate();

#pragma warning disable RCS1213
            private static void Foo(FooDelegate callback)
#pragma warning restore RCS1213
            {
            }
        }

        public class FooNameOf
        {
            public static string Bar1 { get; }

            public static string Bar2 { get; }

            public static string Bar3 { get; }

            [Obsolete(nameof(Bar1))]
            public void Test(string s = nameof(Bar2)) => Test(s);

            public string this[string s = nameof(Bar3)] => s;
        }
    }
}
