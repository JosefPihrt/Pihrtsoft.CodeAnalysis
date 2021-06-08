// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;

namespace Roslynator
{
    internal readonly struct AnalyzerOptionDescriptor
    {
        public AnalyzerOptionDescriptor(
            DiagnosticDescriptor descriptor,
            DiagnosticDescriptor parent,
            string optionKey)
        {
            Descriptor = descriptor;
            Parent = parent;
            OptionKey = optionKey;
        }

        public DiagnosticDescriptor Descriptor { get; }

        public DiagnosticDescriptor Parent { get; }

        public string OptionKey { get; }

        public bool IsDefault
        {
            get
            {
                return Descriptor == null
                    && Parent == null
                    && OptionKey == null;
            }
        }
    }
}
