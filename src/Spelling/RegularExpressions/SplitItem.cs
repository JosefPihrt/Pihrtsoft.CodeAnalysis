// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;

namespace Roslynator.RegularExpressions
{
    public class SplitItem
    {
        public SplitItem(string value)
        {
            Value = value;
            Number = 0;
        }

        public SplitItem(string value, int index, int number)
        {
            Value = value;
            Index = index;
            Number = number;
        }

        public string Value { get; }

        public int Index { get; }

        public int Length => Value.Length;

        public string Name => Number.ToString(CultureInfo.CurrentCulture);

        public int Number { get; }

        public bool IsGroup => false;

        public override string ToString() => Value;
    }
}
