// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class IntroduceConstructorRefactoring
    {
        private class Entity
        {
            private readonly string name;

            private int _property;
            private int _property2;
            private int _property3;
            private int _property4;

            public string Name => name;

            public int AutoProperty { get; set; }

            public int Property
            {
                get { return _property; }
                set { _property = value; }
            }

            public int Property2
            {
                get { return this._property2; }
                set { this._property2 = value; }
            }

            public int Property3
            {
                get => _property3;
                set => _property3 = value;
            }

            public int Property4
            {
                get => this._property4;
                set => this._property4 = value;
            }
        }
    }
}
