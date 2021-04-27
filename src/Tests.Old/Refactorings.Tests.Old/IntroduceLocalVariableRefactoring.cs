// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class IntroduceLocalVariableRefactoring
    {
        public static void Foo()
        {
            using (default(IDisposable))
            {
            }

            int i;
            i = 0;
            i++;

            Execute();

            var x = GetValue();

            GetValue();

            DateTime

            using (new StringReader(""))
            {

            }
        }

        public static async Task ExecuteAsync() => ExecuteAsync();

        private static async Task<Entity> GetValueAsync()
        {
            GetValueAsync();

            return null;
        }

        public static void Execute()
        {
        }

        public static string GetValue()
        {
            string s = null;

            return s;
        }

        private class Entity
        {
        }
    }
}
