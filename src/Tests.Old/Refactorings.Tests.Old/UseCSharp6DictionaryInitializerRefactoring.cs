// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class UseCSharp6DictionaryInitializerRefactoring
    {
        private static readonly Dictionary<string, string> _dic = new Dictionary<string, string>()
        {
            { "A", "A" }, //a
            { "B", "B" }, //b
            { "C", "C" }, //c
        };

        public static void GetValue()
        {
            var dic = new Dictionary<int, string>() { { 0, "0" } };

            dic = new Dictionary<int, string>()
            {
                { 0, "0" },
                { 1, "1" }
            };

            dic = new Dictionary<int, string>()
            {
                {
                    0, "0"
                },
                {
                    1, "1"
                }
            };

            var dic2 = new DerivedDictionary<string, string>() { { "a", "b" } };

            //n

            dic = new Dictionary<int, string>() { [0] = null };

            var items = new List<string>() { { null } };

            var q1 = new Foo<int, int>() { { "key", "value" } };

            var q2 = new Foo<int, string>() { { "key", "value" } };

            var q3 = new Foo<string, string>() { { "key", "value" } };
        }

        private class Foo<TKey, TItem> : IEnumerable<int>
        {
            public TItem this[TKey key]
            {
                get { return default(TItem); }
                //private set { }
            }

            public void Add(string key, string value)
            {
            }

            public IEnumerator<int> GetEnumerator() => null;
            IEnumerator IEnumerable.GetEnumerator() => null;
        }

        public class DerivedDictionary<TKey, TValue> : Dictionary<TKey, TValue>
        {
        }
    }
}
