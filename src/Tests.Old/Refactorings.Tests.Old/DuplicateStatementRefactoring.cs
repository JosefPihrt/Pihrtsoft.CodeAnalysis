// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class DuplicateStatementRefactoring
    {
        public bool GetValue()
        {
            bool condition = false;

            if (condition)
            {
                // ...
            }

            string s = null;

            switch (s)
            {
                case "":
                    if (condition)
                    {
                        return false;
                    }
                    break;
                default:
                    break;
            }

            if (condition)
                if (condition)
                {
                    return false;
                }

            using (var sr = new StringReader(""))
            using (var sr2 = new StringReader(""))
            {
            }

            if (condition)
            {
                return true;
            }
            else if (condition)
            {
                return false;
            }

            return false;
        }
    }
}
