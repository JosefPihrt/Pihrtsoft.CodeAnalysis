// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class RenameMethodAccordingToTypeNameRefactoring
    {
        public Entity Foo() => Foo();

        public Entity Foo2() => Foo2();

        public Entity GetEntity2() => GetEntity2();

        public async Task<Entity> Foo(object parameter)
        {
            return await Task.FromResult(new Entity());
        }

        public async Task Foo3()
        {
            await Foo3();
        }

        public async void Foo4()
        {
            Foo4();
        }

        public class Entity
        {
            private IEnumerable<Entity> Bar()
            {
                yield break;
            }
        }

        public class Loss
        {
            private IEnumerable<Loss> Bar()
            {
                yield break;
            }
        }

        public class Vox
        {
            private IEnumerable<Vox> Bar()
            {
                yield break;
            }
        }

        public class Roxy
        {
            private IEnumerable<Roxy> Bar()
            {
                yield break;
            }
        }

        private class EntityCollection : Collection<Entity>
        {
            private EntityCollection Bar()
            {
                return null;
            }
        }
    }
}
