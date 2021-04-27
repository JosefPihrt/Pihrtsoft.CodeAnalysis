// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// xxx

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using s = System;
using us = System.String;

/// <summary>
/// 
/// </summary>
namespace Roslynator.CSharp.Analyzers.Tests
{
    #region
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Test2;
    using Test2.Test3;
    using s;
    using static System.Nullable;
    using static Math;
    using static Regex;
    using static DeclareUsingDirectiveOnTopLevel<IEnumerable<object>>;
    using static Tests.DeclareUsingDirectiveOnTopLevel<IEnumerable<object>>;
    using static us;
    #endregion

    internal static class DeclareUsingDirectiveOnTopLevel<T>
    {
        public static void Foo()
        {
            var items = new List<string>();
        }

        private static Task<object> GetValueAsync()
        {
            return Task.FromResult(new object());
        }
    }

    namespace Test2
    {
        namespace Test3
        {
        }
    }
}
