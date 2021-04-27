// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS0029_CannotImplicitlyConvertType
    {
        private class Foo : IDisposable
        {
            private Foo _f = new Bar();

            public void Method()
            {
                Foo x = new Bar();

                using (Foo resource = new Bar())
                {
                }
            }

            public async Task<Bar> MethodAsync()
            {
                Foo x = Method2Async();
            }

            public Task<Bar> Method2Async()
            {
                return Task.FromResult(new Bar());
            }

            public void Dispose()
            {
            }

            public Foo Property { get; } = new Bar();
        }

        private class Bar : IDisposable
        {
            public void Dispose()
            {
            }
        }

        // n

        private class Foo2
        {
            public const string Bar = new Bar();
        }
    }
}
