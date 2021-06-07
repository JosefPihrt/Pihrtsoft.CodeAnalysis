// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Roslynator.Spelling
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal class NewWord
    {
        public NewWord(
            string value,
            string line,
            FileLinePositionSpan lineSpan,
            string containingValue = null)
        {
            Value = value;
            Line = line;
            ContainingValue = containingValue;
            LineSpan = lineSpan;
        }

        public string Value { get; }

        public string Line { get; }

        public FileLinePositionSpan LineSpan { get; }

        public string ContainingValue { get; }

        public string FilePath => LineSpan.Path;

        public int LineNumber => LineSpan.StartLine() + 1;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{Value}  {LineSpan}";
    }
}
