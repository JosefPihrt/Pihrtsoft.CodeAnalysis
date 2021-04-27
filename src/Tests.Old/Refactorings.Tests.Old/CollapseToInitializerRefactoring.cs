// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class CollapseToInitializerRefactoring
    {
        public class Entity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Index { get; set; }

            public void MethodName()
            {
                var entity = new Entity();
                entity.Id = 1;
                entity.Name = "name";

                entity = new Entity();
                entity.Id = 1;
                entity.Name = "name";

                switch (0)
                {
                    case 0:
                        var entity2 = new Entity();
                        entity2.Id = 1;
                        entity2.Name = "name";
                        break;
                    case 2:
                        var entity3 = new Entity();
                        entity3.Id = 1;
                        entity3.Name = "name";
                        break;
                }










                var xx = new Entity() { Id = 1 };
                xx.Name = "name";
                xx.Index = 0;

                entity = new Entity();
                entity.Name = "name";
                entity.Index = 0;

                entity = new Entity() { Id = 1 };
                entity.Name = "name";
                entity.Index = 0;
            }
        }

    }
}
