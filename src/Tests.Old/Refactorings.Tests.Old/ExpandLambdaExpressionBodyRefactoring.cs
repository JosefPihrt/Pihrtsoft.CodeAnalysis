// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable RCS1174

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class ExpandLambdaExpressionBodyRefactoring
    {
        public object Foo()
        {
            var items = Enumerable.Range(0, 10).Select(i => i + 1);

            return null;
        }

        public void FooVoid()
        {
            Action<string> x = f => f.ToString();
        }

        private async Task FooAsync()
        {
            Func<Task> x = async () => await FooAsync().ConfigureAwait(false);

            await FooAsync().ConfigureAwait(false);
        }

        private async Task<object> FooAsync(object parameter)
        {
            Func<object, Task<object>> x = async (f) => await FooAsync(parameter).ConfigureAwait(false);

            return await FooAsync(parameter).ConfigureAwait(false);
        }
    }
}
