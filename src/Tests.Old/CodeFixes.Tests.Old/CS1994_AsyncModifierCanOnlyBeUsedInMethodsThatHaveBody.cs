// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS1994_AsyncModifierCanOnlyBeUsedInMethodsThatHaveBody
    {
        private abstract class Foo
        {
            protected abstract async Task<object> GetValueAsync();
        }
    }
}
