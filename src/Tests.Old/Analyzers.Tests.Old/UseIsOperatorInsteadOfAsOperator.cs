// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable RCS1023

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class UseIsOperatorInsteadOfAsOperator
    {
        public abstract class Foo
        {
            protected Foo()
            {
                object x = null;

                if (x as string == null) { }
                if ((x as string) == null) { }

                if (x as string is null) { }
                if ((x as string) is null) { }

                if (x as string != null) { }
                if ((x as string) != null) { }
            }
        }
    }
}
