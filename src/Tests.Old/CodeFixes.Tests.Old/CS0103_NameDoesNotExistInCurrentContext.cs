// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS0103_NameDoesNotExistInCurrentContext
    {
        private static class Foo
        {
            private static void Bar()
            {
                var dic = new Dictionary<object, object>();

                if (dic.TryGetValue("key", out value))
                {

                }
            }
        }
    }
}
