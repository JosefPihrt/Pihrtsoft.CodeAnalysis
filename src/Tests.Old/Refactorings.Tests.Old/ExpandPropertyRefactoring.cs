// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class ExpandPropertyRefactoring
    {
        private ExpandPropertyRefactoring()
        {
            Value3 = null;
        }

        public string Value { get; set; }

        public string Value2 { get; set; } = null;

        public string Value3 { get; } = null;
    }
}
