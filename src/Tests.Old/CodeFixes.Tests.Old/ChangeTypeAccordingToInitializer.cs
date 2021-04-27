// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal class ChangeTypeAccordingToInitializer
    {
        private static class Foo
        {
            public static void Bar()
            {
                int x = GetAsync();
            }

            public static async Task BarAsync()
            {
                int x = GetAsync();
            }

            public static async Task Bar2Async()
            {
                int x = GetAsync();

                string s = await GetAsync();
            }
        }

        private static class Foo2
        {
            public static Task Bar()
            {
                int x = GetAsync();

                return Task.CompletedTask;
            }

            public static Task<string> Bar2()
            {
                int x = GetAsync();

                return Task.FromResult(default(string));
            }

            public static Task<string> Bar3()
            {
                int x = GetAsync();

                return Task.FromResult(default(string));
            }
        }

        private static Task<string> GetAsync()
        {
            return Task.FromResult(default(string));
        }
    }
}
