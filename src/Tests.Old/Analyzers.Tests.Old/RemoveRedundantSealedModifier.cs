// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#pragma warning disable RCS1016

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class RemoveRedundantSealedModifier
    {
        public sealed class SealedFoo : Base
        {
            public sealed override void FooMethod()
            {
            }

            public sealed override string FooProperty => null;

            public sealed override string this[int index] => null;
        }

        //n

        public class Foo : Base
        {
            public sealed override void FooMethod()
            {
            }

            public sealed override string FooProperty => null;

            public sealed override string this[int index] => null;
        }

        public sealed class SealedFoo2 : Base
        {
            public override void FooMethod()
            {
            }

            public override string FooProperty => null;

            public override string this[int index] => null;
        }

        public class Base
        {
            public virtual void FooMethod()
            {
            }

            public virtual string FooProperty { get; }

            public virtual string this[int index]
            {
                get { return null; }
            }
        }
    }
}