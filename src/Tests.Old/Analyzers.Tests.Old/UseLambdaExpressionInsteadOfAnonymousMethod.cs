// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class UseLambdaExpressionInsteadOfAnonymousMethod
    {
        private static void Foo()
        {
            Func<object, bool> func = delegate(object parameter)
            {
                return false;
            };

            func = (Func<object, bool>)delegate(object parameter)
            {
                return false;
            };

            //n

            func = (Func<object, bool>)delegate
            {
                return false;
            };
        }
    }
}
