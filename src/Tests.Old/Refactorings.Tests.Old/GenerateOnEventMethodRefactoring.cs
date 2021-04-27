// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class GenerateOnEventMethodRefactoring
    {
        private class Foo
        {
            public event EventHandler Changed;

            private void OnChanged()
            {
            }
        }

        private class Foo2
        {

            public event EventHandler Changed;

            public event EventHandler<CancelEventArgs> Changed2;

            public static event EventHandler StaticChanged;
        }

        private sealed class Foo3
        {
            public event EventHandler Changed;

            public event EventHandler<CancelEventArgs> Changed2;

            public static event EventHandler StaticChanged;

            private void Bar()
            {
            }
        }
    }
}
