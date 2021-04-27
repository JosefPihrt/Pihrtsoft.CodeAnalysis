// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class ThrowingOfNewNotImplementedException
    {
        public static object Value { get; private set; }

        public static void Foo(object value)
        {
            Value = value ?? throw new NotImplementedException();

            throw new NotImplementedException();
        }
    }
}
