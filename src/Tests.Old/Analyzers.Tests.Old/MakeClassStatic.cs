// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

#pragma warning disable RCS1016, RCS1018

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal partial class MakeClassStatic
    {
        public class Foo
        {
            public const string FooConst = "";

            private static readonly string _fooField;

            public static string FooProperty { get; set; }

            public static event EventHandler FooEvent;

            public static void FooMethod()
            {
            }

            public class FooClass
            {
            }

            public struct FooStruct
            {
            }

            public interface FooInterface
            {
            }

            public delegate void FooDelegate();

            public enum FooEnum
            {
            }
        }

        class Foo2
        {
            public static void Foo()
            {
            }
        }

        public partial class FooPartial
        {
            public static void Foo()
            {
            }
        }

        public partial class FooPartial
        {
        }

        public sealed class FooSealed
        {
            public static void Foo()
            {
            }
        }

        //n

        public sealed partial class FooSealedPartial
        {
        }

        public class Foo5
        {
            protected static void FooMethod()
            {
            }
        }

        public class Foo6
        {
            protected class FooClass
            {
            }
        }

        public class Foo7
        {
            protected struct FooStruct
            {
            }
        }

        public class Foo8
        {
            protected interface FooInterface
            {
            }
        }

        public class Foo9
        {
            protected delegate void FooDelegate();
        }

        public class Foo10
        {
            protected enum FooEnum
            {
            }
        }

        public class Foo11
        {
            protected internal static void FooMethod()
            {
            }
        }

        public class Foo12
        {
            protected internal class FooClass
            {
            }
        }

        public class Foo13
        {
            protected internal struct FooStruct
            {
            }
        }

        public class Foo14
        {
            protected internal interface FooInterface
            {
            }
        }

        public class Foo15
        {
            protected internal delegate void FooDelegate();
        }

        public class Foo16
        {
            protected internal enum FooEnum
            {
            }
        }

        public class Foo17
        {
            public static Foo17 operator !(Foo17 value)
            {
                return null;
            }
        }

        public class Foo18
        {
            public static implicit operator Foo18(string value)
            {
                return null;
            }
        }

        public class Foo19
        {
            public static explicit operator Foo19(string value)
            {
                return null;
            }
        }
    }
}
