// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS0126_ObjectOfTypeConvertibleToTypeIsRequired
    {
        private class Foo
        {
            public DateTime GetDateTime()
            {
                return;
            }

            public string GetValue()
            {
                var items = new List<string>();

                var q = items.Select<string, bool>(f =>
                {
                    return;
                });

                q = items.Select<string, bool>(delegate(string value)
                {
                    return;
                });

                return;
            }

            public Task<bool> GetAsync()
            {
                return;
            }

            public async Task<bool> GetWithAwaitAsync()
            {
                var x = await GetAsync();

                return;
            }

            public DateTime PropertyDateTime
            {
                get { return; }
            }

            public string PropertyString
            {
                get { return; }
            }

            public static explicit operator DateTime(Foo foo)
            {
                return;
            }
        }
    }
}
