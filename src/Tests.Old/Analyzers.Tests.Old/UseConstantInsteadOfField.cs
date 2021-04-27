// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable CS0109, RCS1016, RCS1018, RCS1045, RCS1081, RCS1101, RCS1169

using System;

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal class UseConstantInsteadOfField
    {
        /// <summary>This value is constant.</summary>
        static readonly bool Foo2 = false;

        private static readonly int Foo3 = 0;

        private static readonly string Foo4 = Constant;

        static readonly char Foo5 = 'a', Foo6 = 'b';

        private static readonly StringSplitOptions Foo7 = StringSplitOptions.None, Foo8 = StringSplitOptions.RemoveEmptyEntries;

        private static readonly string Foo9 = Constant, Foo10 = "";

        //n

        private const string Constant = "";

        new private static readonly string Foo11 = "";

        public static readonly string Foo12 = "";

        internal static readonly string Foo13 = "";

        protected static readonly string Foo14 = "";

        private readonly string Foo15 = "";

        private static string Foo16 = "";

        private static readonly xxx Foo17 = "";

        private static readonly Foo Foo18 = new Foo();

        private static readonly string Foo19;

        private static readonly string Foo20 = xxx;

        private static readonly string Foo21 = GetValue();

        private static readonly string Foo22 = "", Foo23 = GetValue();

        private static string GetValue()
        {
            return null;
        }

        private class Foo
        {
        }
    }
}
