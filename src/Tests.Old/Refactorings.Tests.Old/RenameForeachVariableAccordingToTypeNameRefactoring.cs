// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class RenameForEachVariableAccordingToTypeNameRefactoring
    {
        public void SomeMethod()
        {
            var entities = new List<Entity>();

            foreach (Entity item in entities)
            {
                var x = item;
            }

            Entity2 entity2 = null;

            foreach (Entity2 item in Enumerable.Empty<Entity2>())
            {
                var x = item;
            }
        }

        private class Entity
        {
        }

        private class Entity2
        {
        }
    }
}
