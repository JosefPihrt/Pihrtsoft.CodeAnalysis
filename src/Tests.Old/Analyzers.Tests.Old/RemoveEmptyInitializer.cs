// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;

#pragma warning disable RCS1050, RCS1067

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class RemoveEmptyInitializer
    {
        private static void Foo(StringBuilder x)
        {
            x = new StringBuilder() { };

            x = new StringBuilder { };

            x = new StringBuilder()
            {
            };

            x = new StringBuilder
            {
            };

            x = new StringBuilder() /**/ { }; //x

            x = new StringBuilder /**/ { }; //x

            x = new StringBuilder() //x
            {
            }; //x

            x = new StringBuilder //x
            {
            }; //x
        }
    }
}
