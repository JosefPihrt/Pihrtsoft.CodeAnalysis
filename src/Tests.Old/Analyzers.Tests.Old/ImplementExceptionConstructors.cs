// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class ImplementExceptionConstructors
    {
        internal class FooException : Exception
        {
        }

        internal class FooException2 : FooException
        {
        }

        internal class FooException3 : FooException2
        {
        }
    }
}
