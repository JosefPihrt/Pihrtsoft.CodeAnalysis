// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal class AddComparisonWithBooleanLiteralRefactoring
    {
        public bool SomeMethod()
        {
            string value = GetValueOrDefault();

            if (value?.StartsWith("a"))
            {
            }

            if (!value?.StartsWith("a"))
            {
            }

            do
            {
            } while (value?.StartsWith("a"));

            while (value?.StartsWith("a"))
            {
            }

            var x = Enumerable.Empty<string>().Where(f => f?.StartsWith("a"));

            bool x2 = value?.StartsWith("a");

            bool x3 = value?.StartsWith("a") || true;

            return value?.StartsWith("a");
        }

        private string GetValueOrDefault()
        {
            return null;
        }

        private bool GetValue()
        {
            return GetValueOrDefault()?.StartsWith("a");
        }

        private bool GetValue2() => GetValueOrDefault()?.StartsWith("a");
    }
}
