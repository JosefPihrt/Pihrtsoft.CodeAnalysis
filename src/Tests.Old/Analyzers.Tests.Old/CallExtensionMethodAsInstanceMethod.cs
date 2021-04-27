// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Collections.Generic;
using static Roslynator.CSharp.Analyzers.Tests.CallExtensionMethodAsInstanceMethodRefactoring;

#pragma warning disable RCS1007, RCS1032, RCS1060, RCS1016, RCS1176, RCS1177

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class CallExtensionMethodAsInstanceMethodRefactoring2
    {
        public static void Method(string parameter1, string parameter2)
        {
            CallExtensionMethodAsInstanceMethodRefactoring.ExtensionMethod(parameter1, parameter2);
            CallExtensionMethodAsInstanceMethodRefactoring.ExtensionMethod((parameter1), parameter2);
            CallExtensionMethodAsInstanceMethodRefactoring.ExtensionMethod((string)parameter1, parameter2);

            Roslynator.CSharp.Analyzers.Tests.CallExtensionMethodAsInstanceMethodRefactoring.ExtensionMethod(parameter1, parameter2);

            // n

            CallExtensionMethodAsInstanceMethodRefactoring.ExtensionMethod(parameter1 ?? "", parameter2);

            parameter1.ExtensionMethod(parameter2);

            IEnumerable<int> items = Enumerable.Select(Enumerable.Range(0, 1), f => f);

            string s = StringExtension("");
        }

        private static IEnumerable<TResult> Select<TResult, TSource>(this IEnumerable<TSource> items, Func<TSource, TResult> selector)
        {
            foreach (TSource item in items)
                yield return selector(item);
        }

        private static string StringExtension(this string value)
        {
            return value;
        }
    }

    internal static class CallExtensionMethodAsInstanceMethodRefactoring
    {
        public static void ExtensionMethod(this string parameter1, string parameter2)
        {
            ExtensionMethod(parameter1, parameter2);
            CallExtensionMethodAsInstanceMethodRefactoring.ExtensionMethod(parameter1, parameter2);
            Roslynator.CSharp.Analyzers.Tests.CallExtensionMethodAsInstanceMethodRefactoring.ExtensionMethod(parameter1, parameter2);
            parameter1.ExtensionMethod(parameter2);

            IEnumerable<int> items = Enumerable.Select(Enumerable.Range(0, 1), f => f);
        }

        private static IEnumerable<TResult> Select<TResult, TSource>(this IEnumerable<TSource> items, Func<TSource, TResult> selector)
        {
            foreach (TSource item in items)
                yield return selector(item);
        }
    }
}
