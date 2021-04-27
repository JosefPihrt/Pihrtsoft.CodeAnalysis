// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

#pragma warning disable CA1822

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS8340_InstanceFieldsOfReadOnlyStructsMustBeReadOnly
    {
        private readonly struct Foo
        {
            public string Field;

            public string Field2;

            public readonly string ReadOnlyField;
        }
    }
}
