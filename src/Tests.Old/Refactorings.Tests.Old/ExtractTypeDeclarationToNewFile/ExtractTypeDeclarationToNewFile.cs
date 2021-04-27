// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class Foo
    {
    }

    public class Bar
    {
    }

    public struct FooStruct
    {
    }

    public interface FooInterface
    {
    }

    public enum FooEnum
    {
        None
    }

    public delegate void FooDelegate();

    internal class ExtractTypeDeclarationToNewFile
    {
        public class FooNested
        {
        }
    }
}
