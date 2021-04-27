// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

#pragma warning disable CA1822

namespace Roslynator.CSharp.CodeFixes.Tests
{
    internal static class CS0238_MemberCannotBeSealedBecauseItIsNotOverride
    {
        public class ClassName
        {
            public sealed string MethodName()
            {
                return null;
            }

            public sealed string PropertyName
            {
                get { return null; }
            }

            public sealed event EventHandler EventName;

            public sealed string this[int index]
            {
                get { return null; }
            }

            // n

            public sealed string ToString()
            {
                return null;
            }
        }
    }
}
