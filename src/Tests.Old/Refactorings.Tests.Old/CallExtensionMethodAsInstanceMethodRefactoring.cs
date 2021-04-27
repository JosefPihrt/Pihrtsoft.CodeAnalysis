// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Collections.Generic;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class CallExtensionMethodAsInstanceMethodRefactoring
    {
        public static void ExtensionMethod(this string parameter1, string parameter2)
        {
            ExtensionMethod(parameter1, parameter2);
            ExtensionMethod(parameter1 ?? "", parameter2);
            CallExtensionMethodAsInstanceMethodRefactoring.ExtensionMethod(parameter1, parameter2);
            Roslynator.CSharp.Refactorings.Tests.CallExtensionMethodAsInstanceMethodRefactoring.ExtensionMethod(parameter1, parameter2);

            //n

            parameter1.ExtensionMethod(parameter2);

            var items = Enumerable.Select(Enumerable.Range(0, 1), f => f);
        }

        private static IEnumerable<TResult> Select<TResult, TSource>(this IEnumerable<TSource> items, Func<TSource, TResult> selector)
        {
            foreach (var item in items)
                yield return selector(item);
        }
    }
}
