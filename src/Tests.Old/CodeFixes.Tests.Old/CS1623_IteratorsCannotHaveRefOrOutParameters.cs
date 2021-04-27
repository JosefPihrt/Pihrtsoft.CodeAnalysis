// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS1623_IteratorsCannotHaveRefOrOutParameters
    {
        private class Foo
        {
            private IEnumerable<object> GetValueAsync(ref object value, out object value2, out object value3)
            {
                yield return null;
            }

            private void Bar()
            {
                IEnumerable<object> LocalAsync(ref object value)
                {
                    yield return null;
                }
            }
        }
    }
}
