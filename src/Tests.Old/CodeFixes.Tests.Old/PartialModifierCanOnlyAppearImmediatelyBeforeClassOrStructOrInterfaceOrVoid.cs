// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class PartialModifierCanOnlyAppearImmediatelyBeforeClassOrStructOrInterfaceOrVoid
    {
        partial private class ClassName
        {
            partial static void MethodName();

            partial static void MethodName()
            {
            }
        }

        partial public interface InterfaceName
        {
        }

        partial public struct StructName
        {
        }
    }
}
