// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class AddStaticModifierToAllPartialClassDeclarations
    {
        public static partial class Foo
        {
        }

        public partial class Foo
        {
        }

        public partial class Foo
        {
        }
    }
}
