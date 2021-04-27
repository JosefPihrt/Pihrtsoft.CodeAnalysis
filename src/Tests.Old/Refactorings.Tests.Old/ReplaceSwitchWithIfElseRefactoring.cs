// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class ReplaceSwitchWithIfElseRefactoring
    {
        public string SomeMethod()
        {
            object x = null;

            switch (x)
            {
                case null:
                    return "null";
                case 0:
                    return "0";
                case 1:
                case int i:
                    return "i";
                case long l:
                    return "l";
                default:
                    return "default";    
            }
        }
    }
}
