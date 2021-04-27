// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace Roslynator.CSharp.CodeFixes.Tests
{

    internal static class CS1689_AttributeIsNotValidOnThisDeclarationType
    {
        [Conditional("DEBUG")]
        private class Foo
        {
        }

        [Obsolete, Conditional("DEBUG")]
        private class Foo2
        {
        }
    }
}
