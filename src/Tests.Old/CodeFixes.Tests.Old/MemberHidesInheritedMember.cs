// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class MemberHidesInheritedMember
    {
        public class Foo
        {
            public string ToString()
            {
                return null;
            }

            public Type GetType()
            {
                return null;
            }
        }
    }
}
