// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class ReplacePropertyWithMethodRefactoring
    {
        public ReplacePropertyWithMethodRefactoring()
        {
            var x = Value;
            x = this.Value;
        }

#if DEBUG
        public ReplacePropertyWithMethodRefactoring Value /**/
        {
            get
            {
                var x = /*a*/ Value /*b*/
                    /*c*/ .Value /*d*/
                    /*e*/ .Value /*f*/;

                return null;
            }
        }
#endif

#if DEBUG
        public string Value4 { get; } = new string(' ', 1); /**/
#endif

        private class Foo
        {
            public bool IsFoo
            {
                get { return false; }
            }

            public bool IsFoo2
            {
                get => false;
            }

            public bool Bar()
            {
                var x = new Foo();

                if (x?.IsFoo == true)
                {
                }

                return false;
            }
        }

        //n

        private string _value2;

        public string Value2
        {
            get { return _value2; }
            set { _value2 = value; }
        }

        private string _value3;

        public string Value3
        {
            get => _value3;
            set => _value3 = value;
        }
    }
}
