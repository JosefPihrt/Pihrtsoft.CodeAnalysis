// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class RenameFieldIdentifierAccordingToTypeNameRefactoring
    {
        private readonly Entity _value = new Entity();

        private readonly KeywordCollection _value2 = new KeywordCollection();
        private readonly Collection<Entity> _value3 = new Collection<Entity>();

        public class Entity
        {
        }

        private class KeywordCollection : Collection<string>
        {
            public KeywordCollection()
            {
            }
        }
    }
}
