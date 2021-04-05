// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.Spelling
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal abstract class SpellingDiagnostic
    {
        private string _valueLower;
        private TextCasing? _casing;

        protected SpellingDiagnostic(
            Diagnostic diagnostic,
            string value,
            int index,
            string parent,
            int parentIndex,
            SyntaxToken identifier = default)
        {
            Diagnostic = diagnostic ?? throw new ArgumentNullException(nameof(diagnostic));
            Value = value;
            Index = index;
            Parent = parent;
            ParentIndex = parentIndex;
            Identifier = identifier;
        }

        public Diagnostic Diagnostic { get; }

        public string Value { get; }

        public int Index { get; }

        public int Length => Value.Length;

        public int EndIndex => Index + Value.Length;

        public string Parent { get; }

        public int ParentIndex { get; }

        public int Offset => (Parent != null) ? Index - ParentIndex : 0;

        public Location Location => Diagnostic.Location;

        public SyntaxTree SyntaxTree => Location.SourceTree;

        public string FilePath => SyntaxTree?.FilePath;

        public TextSpan Span => Location.SourceSpan;

        public SyntaxToken Identifier { get; }

        public bool IsSymbol => Identifier.Parent != null;

        public string ValueLower => _valueLower ??= Value.ToLowerInvariant();

        public TextCasing Casing => _casing ??= TextUtility.GetTextCasing(Value);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{Value}  {Parent}";

        public abstract bool IsApplicableFix(string fix);
    }
}
