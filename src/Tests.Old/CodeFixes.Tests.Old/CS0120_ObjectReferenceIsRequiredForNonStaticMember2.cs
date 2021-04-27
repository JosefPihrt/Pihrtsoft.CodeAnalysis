// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static partial class CS0120_ObjectReferenceIsRequiredForNonStaticMember
    {
        private partial class Foo
        {
            public string MethodInOtherFile()
            {
                return null;
            }
        }
    }
}
