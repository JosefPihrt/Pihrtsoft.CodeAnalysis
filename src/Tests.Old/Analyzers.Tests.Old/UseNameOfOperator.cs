// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

#pragma warning disable CS0168, RCS1007, RCS1016, RCS1048, RCS1163, RCS1176, RCS1208

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class UseNameOfOperator
    {
        private class Foo
        {
            public Foo(object parameter)
            {
                if (parameter == null)
                {
                    throw new ArgumentNullException(
                       "parameter",
                       "message");
                }
            }

            private static void FooMethod(object parameter)
            {
                if (parameter == null)
                    throw new ArgumentNullException("parameter", "message");

                CheckParam("parameter");

                var items = new List<string>();

                IEnumerable<string> q = items.Select(item =>
                {
                    if (item == null)
                        throw new ArgumentNullException("item", "message");

                    return item;
                });

                IEnumerable<string> q2 = items.Select((item) =>
                {
                    if (item == null)
                        throw new ArgumentNullException("item", "message");

                    return item;
                });

                IEnumerable<string> q3 = items.Select(delegate (string item)
                {
                    if (item == null)
                        throw new ArgumentNullException("item", "message");

                    return item;
                });

                IEnumerable<string> LocalFunction(object parameter2)
                {
                    if (parameter == null)
                        throw new ArgumentNullException("parameter2", "message");

                    yield break;
                }
            }

            private static void CheckParam(string parameterName)
            {
            }

            private object this[int index]
            {
                get { return Items[index]; }
                set
                {
                    if (index < 0)
                        throw new ArgumentOutOfRangeException("index");

                    Items[index] = value;
                }
            }

            private Collection<object> Items { get; } = new Collection<object>();

            private class Bar : INotifyPropertyChanged
            {
                private string _value;

                public event PropertyChangedEventHandler PropertyChanged;

                public string Value
                {
                    get { return _value; }
                    set
                    {
                        if (_value != value)
                            PropertyChanged(this, new PropertyChangedEventArgs("Value"));

                        _value = value;
                    }
                }
            }
        }

        private class Foo2
        {
            public Foo2(object @namespace)
            {
                if (@namespace == null)
                {
                    throw new ArgumentNullException(
                       "namespace",
                       "message");
                }
            }

            private static void FooMethod(object @namespace)
            {
                if (@namespace == null)
                    throw new ArgumentNullException("namespace", "message");

                CheckParam("namespace");

                var items = new List<string>();

                IEnumerable<string> q = items.Select(@class =>
                {
                    if (@class == null)
                        throw new ArgumentNullException("class", "message");

                    return @class;
                });

                IEnumerable<string> q2 = items.Select((@class) =>
                {
                    if (@class == null)
                        throw new ArgumentNullException("class", "message");

                    return @class;
                });

                IEnumerable<string> q3 = items.Select(delegate (string @class)
                {
                    if (@class == null)
                        throw new ArgumentNullException("class", "message");

                    return @class;
                });

                IEnumerable<string> LocalFunction(object @class)
                {
                    if (@class == null)
                        throw new ArgumentNullException("class", "message");

                    yield break;
                }
            }

            private static void CheckParam(string parameterName)
            {
            }

            private object this[int @class]
            {
                get { return Items[@class]; }
                set
                {
                    if (@class < 0)
                        throw new ArgumentOutOfRangeException("class");

                    Items[@class] = value;
                }
            }

            private Collection<object> Items { get; } = new Collection<object>();

            private class Bar : INotifyPropertyChanged
            {
                private string _value;

                public event PropertyChangedEventHandler PropertyChanged;

                public string @namespace
                {
                    get { return _value; }
                    set
                    {
                        if (_value != value)
                            PropertyChanged(this, new PropertyChangedEventArgs("namespace"));

                        _value = value;
                    }
                }
            }
        }

        private static class FooEnum
        {
            private static void Bar(string s)
            {
                s = DayOfWeek.Monday.ToString();

                // n

                s = DayOfWeek.Monday.ToString("f");

                s = StringSplitOptions.None.ToString();
                s = StringSplitOptions.None.ToString("");
            }
        }
    }
}
