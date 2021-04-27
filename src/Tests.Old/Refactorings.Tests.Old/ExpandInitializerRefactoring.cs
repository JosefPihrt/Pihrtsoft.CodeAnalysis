// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class ExpandInitializerRefactoring
    {
        public void SomeMethod()
        {
            var entity = new Entity() { Name = "Name", Value = 0 };

            entity = new Entity() { Name = "Name", Value = 0 };

            entity = new Entity { Name = "Name", Value = 0 };

            var items = new List<string>() { "a", "b", "c" };

            items = new List<string>() { "a", "b", "c" };

            items = new List<string> { "a", "b", "c" };

            var dic = new Dictionary<string, int>() { ["key1"] = 1, ["key2"] = 2 };

            dic = new Dictionary<string, int>() { ["key1"] = 1, ["key2"] = 2 };

            dic = new Dictionary<string, int> { ["key1"] = 1, ["key2"] = 2 };

            var dic2 = new Dictionary<string, int>() { { "key1", 1 }, { "key2", 2 } };

            dic2 = new Dictionary<string, int>() { { "key1", 1 }, { "key2", 2 } };

            dic2 = new Dictionary<string, int> { { "key1", 1 }, { "key2", 2 } };

            var arr = new string[] { "a", "b", "c" };
        }

        private class Entity
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }
    }
}
