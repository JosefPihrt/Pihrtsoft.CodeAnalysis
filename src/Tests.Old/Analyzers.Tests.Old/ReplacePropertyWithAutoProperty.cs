// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class ReplacePropertyWithAutoProperty
    {
        private class PropertyWithExpressionBodiedMember
        {
#if DEBUG //1
            private readonly string _field = null; //x
#endif
            public PropertyWithExpressionBodiedMember()
            {
                _field = null;
            }

#if DEBUG //2
            public string Property1 => _field;
#endif
            public object Property2 => _field;
        }

        private class PropertyWithGetterAndSetter
        {
#if DEBUG //3
            private string _field = null;
#endif

            public PropertyWithGetterAndSetter()
            {
                _field = null;
            }

            public string Property1
            {
#if DEBUG //4
                get { return _field; }
#endif
#if DEBUG //5
                set { _field = value; }
#endif
            }

            public object Property2
            {
                get { return _field; }
                set { _field = (string)value; }
            }
        }

        private class PropertyWithGetter
        {
            private readonly string _x = null,
#if DEBUG //6
                _field = null;
#endif

            public PropertyWithGetter()
            {
                _field = null;
            }

#if DEBUG //7
            public string Property1
            {
                get { return _field; }
            }
#endif

            public object Property2
            {
                get { return _field; }
            }
        }
    }
}
