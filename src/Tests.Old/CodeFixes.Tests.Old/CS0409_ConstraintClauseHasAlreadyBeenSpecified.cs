// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.RegularExpressions;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS0409_ConstraintClauseHasAlreadyBeenSpecified
    {
        private interface IFoo
        {
        }

        private class Foo<T> where T : class where T : IFoo
        {
        }
    }
}
