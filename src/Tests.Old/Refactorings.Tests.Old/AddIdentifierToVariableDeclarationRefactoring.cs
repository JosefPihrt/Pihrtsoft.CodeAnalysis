// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Xml;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class AddIdentifierToVariableDeclarationRefactoring
    {
        public static void GetValue(object value)
        {
            XmlReader;

            XmlReader

            object x = null;

            XmlReader xmlReader;
            XmlReader xmlReader2;

            // n

            xmlReader;

            value;
        }
    }
}
