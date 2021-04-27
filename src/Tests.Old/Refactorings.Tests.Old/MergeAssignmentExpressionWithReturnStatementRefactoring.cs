// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class MergeAssignmentExpressionWithReturnStatementRefactoring
    {
        public int GetValue()
        {
            int value = 0;

            value = 0;
            return value;
        }
    }
}
