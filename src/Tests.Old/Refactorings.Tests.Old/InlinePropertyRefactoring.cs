// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable IDE0002, IDE0003

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class InlinePropertyRefactoring
    {
        private class Foo
        {
            public void Bar(string s)
            {
                s = Property;
                s = this.Property;

                s = Property2;
                s = this.Property2;

                s = StaticProperty;
                s = Foo.StaticProperty;

                s = StaticProperty2;
                s = Foo.StaticProperty2;
            }

            public string Property => nameof(Property);

            public string Property2
            {
                get { return nameof(Property2); }
            }

            public static string StaticProperty => nameof(StaticProperty);

            public static string StaticProperty2
            {
                get { return nameof(StaticProperty2); }
            }
        }
    }
}
