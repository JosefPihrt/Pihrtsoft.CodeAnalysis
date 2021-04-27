// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class FormatAttributeArgumentListRefactoring
    {
        [MyAttribute(null, null, null)]
        private class MyAttribute : Attribute
        {
            public MyAttribute(object parameter1, object parameter2, object parameter3)
            {
            }
        }

        [My2Attribute(
            null,
            null,
            null)]
        private class My2Attribute : Attribute
        {
            public My2Attribute(object parameter1, object parameter2, object parameter3)
            {
            }
        }

        [My3Attribute(
            null, //x
            null,
            null)]
        private class My3Attribute : Attribute
        {
            public My3Attribute(object parameter1, object parameter2, object parameter3)
            {
            }
        }
    }
}
