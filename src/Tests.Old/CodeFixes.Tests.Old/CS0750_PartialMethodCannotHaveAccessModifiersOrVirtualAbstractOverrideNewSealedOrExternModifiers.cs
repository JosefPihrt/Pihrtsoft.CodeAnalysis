// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS0750_PartialMethodCannotHaveAccessModifiersOrVirtualAbstractOverrideNewSealedOrExternModifiers
    {
        private partial class Foo
        {
            public virtual partial void Bar()
            {
            }

            partial void Bar();

            public partial void Bar2()
            {
            }

            partial void Bar2();
        }
    }
}
