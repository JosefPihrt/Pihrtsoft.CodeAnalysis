// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

#pragma warning disable RCS1016, RCS1024, RCS1181

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class GeneratePropertyForDebuggerDisplayAttributeRefactoring
    {
        //" \\{\\} Value1: {Value1,nq} Value2: {Value2} \" \\"
        [DebuggerDisplay(" \\{\\} Value1: {Value1,nq} Value2: {Value2} \" \\")]
        private class Foo
        {
            public string Value1 { get; }

            public string Value2 { get; }
        }

        //@" \{\} Value1: {GetValue1(),nq} Value2: {GetValue2()} "" \\ "
        [DebuggerDisplay(@" \{\} Value1: {GetValue1(),nq} Value2: {GetValue2()} "" \\ ")]
        private class Foo2
        {
            public string GetValue1() => "";

            public string GetValue2() => "";
        }
    }
}
