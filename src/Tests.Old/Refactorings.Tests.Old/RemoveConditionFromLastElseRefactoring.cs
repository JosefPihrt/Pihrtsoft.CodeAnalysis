// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class RemoveConditionFromLastElseRefactoring
    {
        public bool SomeMethod()
        {
            bool condition = false;
            if (condition)
            {
                return true;
            }
            else if (condition)
            {
                return false;

            }
            else if (condition)
            {
                return false;
            }

            return false;
        }
    }
}
