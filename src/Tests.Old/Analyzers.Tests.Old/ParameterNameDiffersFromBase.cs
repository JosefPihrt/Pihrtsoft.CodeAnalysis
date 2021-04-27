// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

#pragma warning disable CS0219

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class ParameterNameDiffersFromBase
    {
        public interface Interface
        {
            string Method(object parameter);

            string this[int index] { get; }
        }

        public abstract class Foo
        {
            public abstract string Method(object parameter);

            public abstract string this[int index] { get; }
        }

        public class Foo2 : Foo
        {
            public override string this[int index2] => index2.ToString();

            public override string Method(object parameter2) => parameter2.ToString();
        }

        public class Foo3 : Interface
        {
            public string this[int index2] => index2.ToString();

            public string Method(object parameter2) => parameter2.ToString();
        }

        // no code fix

        public class Foo4 : Foo
        {
            public override string this[int index2]
            {
                get
                {
                    string index = null;

                    return index2.ToString();
                }
            }

            public override string Method(object parameter2)
            {
                string parameter = null;

                return parameter2.ToString();
            }
        }
    }
}
