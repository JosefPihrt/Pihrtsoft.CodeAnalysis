// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class RemovePropertyInitializerRefactoring
    {
        private class Entity
        {
            public string Value { get; set; } = InitializeValue();

            private static string InitializeValue()
            {
                throw new NotImplementedException();
            }
        }
    }
}
