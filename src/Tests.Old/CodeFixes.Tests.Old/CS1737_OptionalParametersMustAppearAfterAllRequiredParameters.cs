// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS1737_OptionalParametersMustAppearAfterAllRequiredParameters
    {
        private class Foo
        {
            public void Bar(
                object value = null,
                int i,
                string s,
                CancellationToken ct)
            {
            }
        }
    }
}
