// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class ReplaceMethodWithPropertyRefactoring
    {
#if DEBUG
        public ReplaceMethodWithPropertyRefactoring GetValue() /**/
        {
            var x = /*a*/ GetValue() /*b*/
                /*c*/ .GetValue() /*d*/
                /*e*/ .GetValue() /*f*/;

            return null;
        }
#endif

        public string Foo()
        {
            return "Foo";
        }

        public ReplaceMethodWithPropertyRefactoring()
        {
            var a = GetValue();
            var b = this.GetValue();
        }

        //n

        public async Task<bool> FooAsync()
        {
            return await Task.FromResult(false).ConfigureAwait(false);
        }
    }
}
