// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

#pragma warning disable RCS1074, RCS1102

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class StaticMemberInGenericTypeShouldUseTypeParameter
    {
        public class Foo<T>
        {
            public static readonly string _field;

            public static object Method() => null;

            public static string Property { get; set; }

            public static event EventHandler Event;

            public static event EventHandler Event2
            {
                add { }
                remove { }
            }
        }

        public class Foo2<T, T2>
        {
            private static readonly T _field;
            private static readonly T2 _field2;
            private static readonly List<List<Dictionary<string, T>>> _field3;
            private static readonly List<List<Dictionary<string, T2>>> _field4;

            public static T Method() => default(T);
            public static T2 Method2() => default(T2);
            public static List<List<Dictionary<string, T>>> Method3() => null;
            public static List<List<Dictionary<string, T2>>> Method4() => null;
            public static object Method5(T parameter) => parameter;
            public static object Method6(T2 parameter) => parameter;
            public static object Method7(List<List<Dictionary<string, T>>> parameter) => parameter;
            public static object Method8(List<List<Dictionary<string, T2>>> parameter) => parameter;

            public static T Property { get; set; }
            public static T2 Property2 { get; set; }
            public static List<List<Dictionary<string, T>>> Property3 { get; set; }
            public static List<List<Dictionary<string, T2>>> Property4 { get; set; }

            public static event EventHandler<T> Event;
            public static event EventHandler<T2> Event2;
            public static event EventHandler<List<List<Dictionary<string, T>>>> Event3;
            public static event EventHandler<List<List<Dictionary<string, T2>>>> Event4;

            public static event EventHandler<T> Event5
            {
                add { }
                remove { }
            }
        }
    }
}
