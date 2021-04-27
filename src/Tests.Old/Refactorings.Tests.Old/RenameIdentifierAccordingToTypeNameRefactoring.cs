// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal partial class RenameIdentifierAccordingToTypeNameRefactoring
    {
        partial void Foo();

        partial void Foo()
        {
            var q = Enumerable.Empty<Entity>().Select(f =>
            {
                var entities = new List<Entity>();

                return f;
            });

            var items = new List<Entity>();

            if (TryGet("", out var x))
            {
                x = null;
            }

            if (q is Entity e)
            {
                e = null;
            }
        }

        private static bool TryGet(string value, out Entity result)
        {
            result = null;
            return false;
        }

        private string this[Entity x]
        {
            get { return null; }
        }

        private class Entity
        {
        }
    }
}
