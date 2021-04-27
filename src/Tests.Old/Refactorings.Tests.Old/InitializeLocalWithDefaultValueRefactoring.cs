// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class InitializeLocalWithDefaultValueRefactoring
    {
        public string GetValue()
        {
            RegexOptions options 

            RegexOptions options2 , options3

            System.Text.RegularExpressions.RegexOptions options4, options5 ;

            bool condition 

            bool condition2 ;

            bool condition4 = ;

            EnumName enumName   

            char ch 

            int? ni

            int i

            long ii

            DateTime dt 

            string s

            StringBuilder sb 

            return null;
        }

        private enum EnumName
        {
            Value = 1
        }
    }
}
