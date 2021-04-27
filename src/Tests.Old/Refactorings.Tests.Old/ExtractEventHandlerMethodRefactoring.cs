// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class ExtractEventHandlerMethodRefactoring
    {
        private class Foo
        {
            public void Bar()
            {
                var foo = new Foo();

                foo.Changed += (s, e) => Bar();

                foo.Changed += (object s, EventArgs e) =>
                {
                    Bar();
                };
            }

            public event EventHandler Changed;
        }
    }
}
