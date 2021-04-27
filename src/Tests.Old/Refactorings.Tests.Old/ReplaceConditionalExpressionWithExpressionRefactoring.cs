// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class ReplaceConditionalExpressionWithExpressionRefactoring
    {
        public static bool MethodName()
        {
            bool condition = false;

            return (condition) ? GetTrue() : GetFalse();
        }

        private static bool GetTrue()
        {
            throw new NotImplementedException();
        }

        private static bool GetFalse()
        {
            throw new NotImplementedException();
        }
    }
}
