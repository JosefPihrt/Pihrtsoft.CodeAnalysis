// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class ReplaceStringEmptyWithEmptyStringLiteralRefactoring
    {
        public void Foo()
        {
            string s = string.Empty;

            s = String.Empty;
            s = System.String.Empty;
            s = global::System.String.Empty;
        }
    }
}
