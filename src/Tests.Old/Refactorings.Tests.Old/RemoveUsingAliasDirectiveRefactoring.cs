// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using listOfString = System.Collections.Generic.List<string>;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class RemoveUsingAliasDirectiveRefactoring
    {
        public static void Foo()
        {
            var items = new listOfString();
        }
    }
}
