// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class ReplaceFieldWithConstantRefactoring
    {
        public static readonly string Value = "Value";

        /// <summary>.</summary>
        static readonly string Value2 = "Value2";
    }
}
