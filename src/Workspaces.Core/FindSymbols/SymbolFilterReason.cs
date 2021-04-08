// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.FindSymbols
{
    internal enum SymbolFilterReason
    {
        None = 0,
        Visibility = 1,
        SymbolGroup = 2,
        Ignored = 3,
        WithAttribute = 4,
        WithoutAttribute = 5,
        ImplicitlyDeclared = 6,
        Other = 7,
    }
}
