// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class ReplaceForWithForeachRefactoring
    {
        public static void Foo()
        {
            string item = null;

            var items = new List<string>();

            for (int i = 0; i < items.Count; i++)
            {
                string value = items[i];
                string value2 = items[i];
            }

            //n

            for (int i = 1; i < items.Count; i++)
            {
                string value = items[i];
            }

            for (int i = 0; i < items.Count - 1; i++)
            {
                string value = items[i];
            }

            for (int i = 0; i < items.Count; i++)
            {
                string value = items[i + 1];
            }

            for (int i = 0; i < items2.Count; i++)
            {
                var x = items2[i + 1];
            }
        }
    }
}
