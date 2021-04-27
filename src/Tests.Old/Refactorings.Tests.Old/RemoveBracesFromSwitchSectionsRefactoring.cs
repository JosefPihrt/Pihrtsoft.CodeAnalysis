// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class RemoveBracesFromSwitchSectionsRefactoring
    {
        public bool SomeMethod()
        {
            StringReader sr = null;

            switch (sr.Read())
            {
                case 10:
                    {
                        return true;
                    }
                case 13:
                    {
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }
    }
}
