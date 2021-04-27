// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class SplitLocalDeclarationRefactoring
    {
        private readonly string _value4 = "";
        private readonly string _value = "", _value2 = "", _value3 = "";

        public event EventHandler OnChanged4;
        public event EventHandler OnChanged, OnChanged2, OnChanged3;

        public void Do()
        {
            string value4 = "";

            string value = "", value2 = "", value3 = "";

            value = value2 = value3 = value4;
        }
    }
}
