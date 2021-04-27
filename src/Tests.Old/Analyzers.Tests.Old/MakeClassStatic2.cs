// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal partial class MakeClassStatic
    {
        public partial class FooPartial
        {
        }

        //n

        public sealed partial class FooSealedPartial
        {
        }
    }
}
