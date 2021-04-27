// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS1621_YieldStatementCannotBeUsedInsideAnonymousMethodOrLambdaExpression
    {
        private static void Foo()
        {
            var x = Enumerable.Range(0, 1).Select(f =>
            {
                yield return f;
            });
        }
    }
}
