// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

#pragma warning disable RCS1002, RCS1016, RCS1040, RCS1079, RCS1163, RCS1207

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class SimplifyLambdaExpression
    {
        private static void Foo(object parameter)
        {
            Func<object, bool> func1 = f =>
            {
                return f != null;
            };

            Func<object, bool> func2 = (f) =>
            {
                return f != null;
            };

            Action<object> action1 = f =>
            {
                Foo(f);
            };

            Action<object> action2 = (f) =>
            {
                Foo(f);
            };

            Action<object> action3 = f =>
            {
                throw new NotImplementedException();
            };

            Action<object> action4 = (f) =>
            {
                throw new NotImplementedException();
            };
        }
    }
}
