// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class ReplaceRegularStringLiteralWithVerbatimStringLiteralRefactoring
    {
        public string Foo()
        {

            string s = "\"1\"\r\n\"2\"";

            return s;
        }
    }
}
