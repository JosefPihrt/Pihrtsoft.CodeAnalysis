// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class CodeFixOptionAttribute : Attribute
    {
        public CodeFixOptionAttribute(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }
}
