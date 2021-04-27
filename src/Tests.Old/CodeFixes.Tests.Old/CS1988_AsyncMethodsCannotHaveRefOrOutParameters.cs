// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

#pragma warning disable CS0168

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS1988_AsyncMethodsCannotHaveRefOrOutParameters
    {
        private class Foo
        {
            private async Task<object> GetValueAsync(ref object value, out object value2, out object value3)
            {
                value2 = null;
                value3 = null;

                return await Task.FromResult(default(object));
            }

            private void Bar()
            {
                async Task<object> LocalAsync(ref object value, out object value2, out object value3)
                {
                    value2 = null;
                    value3 = null;

                    return await Task.FromResult(default(object));
                }
            }
        }
    }
}
