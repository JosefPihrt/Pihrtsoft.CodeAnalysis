// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class CallConfigureAwaitRefactoring
    {
        public async Task<string> GetValueAsync()
        {
            string s = await GetStringAsync();

            return "";
        }

        public async Task<string> GetValue()
        {
            var x = await this.GetStringAsync() /**/;

            var xx = Get2Async() /**/;

            var q = Enumerable.Range(1, 1).Select(async f =>
            {
                var x2 = await GetStringAsync() /**/;

                var xx2 = Get2Async() /**/;

                return 0;
            });

            return "";
        }

        public Task<string> GetStringAsync()
        {
            var s =  Task.FromResult<string>(null);

            return s;
        }

        public Task Get2Async()
        {
            return Task.FromResult<string>(null);
        }
    }
}
