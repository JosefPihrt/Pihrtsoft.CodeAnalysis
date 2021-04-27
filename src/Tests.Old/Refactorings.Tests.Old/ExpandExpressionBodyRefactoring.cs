// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class ExpandExpressionBodyRefactoring
    {
        public string Foo() => Foo();

        public void FooVoid() => FooVoid();

        public string FooThrow() => throw new Exception();

        private async Task FooAsync() => await FooAsync().ConfigureAwait(false);

        private async void FooVoidAsync() => await FooAsync().ConfigureAwait(false);

        private async Task<object> FooAsync(object parameter) => await FooAsync(parameter).ConfigureAwait(false);
    }
}
