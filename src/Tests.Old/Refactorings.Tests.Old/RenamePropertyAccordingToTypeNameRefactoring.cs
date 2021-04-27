// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using System.Xml;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class RenamePropertyAccordingToTypeNameRefactoring
    {
        public Entity Value => null;

        public XmlWriter Bar => null;
        public StringBuilder Builder => null;
        public StringBuilder StringBuilder => null;

        public class Entity
        {
        }
    }
}
