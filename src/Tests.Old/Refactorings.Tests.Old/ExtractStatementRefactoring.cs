// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class ExtractStatementRefactoring
    {
        public bool GetValue()
        {
            bool condition = false;

            if (condition)
            {
                return true;
            }

            //
            if (condition)
            {
                //...
                return true;
            }
            else
            {
                //...
                return true;
            }

            foreach ((string, string) item in Tuple.Values)
                GetValue();

            return false;
        }
    }
}
