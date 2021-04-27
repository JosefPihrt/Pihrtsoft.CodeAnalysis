// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class MoveBaseClassBeforeAnyInterface
    {
        private class Foo : IDisposable, Base
        {
            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }

        private class Base
        {
        }
    }
}
