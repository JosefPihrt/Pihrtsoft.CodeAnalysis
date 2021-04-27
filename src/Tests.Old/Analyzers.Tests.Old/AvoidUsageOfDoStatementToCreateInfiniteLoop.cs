// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class AvoidUsageOfDoStatementToCreateInfiniteLoop
    {
        public static void Foo()
        {
            do
            {
                Foo();
            }
            while (true);
        }
    }
}
