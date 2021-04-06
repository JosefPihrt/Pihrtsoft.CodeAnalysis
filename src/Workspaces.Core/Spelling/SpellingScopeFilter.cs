// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.Spelling
{
    [Flags]
    internal enum SpellingScopeFilter
    {
        None = 0,
        DocumentationComment = 1,
        NonDocumentationComment = 1 << 1,
        Comment = DocumentationComment | NonDocumentationComment,
        RegionDirective = 1 << 2,
        NonSymbol = Comment | RegionDirective,
        Namespace = 1 << 3,
        Class = 1 << 4,
        Struct = 1 << 5,
        Delegate = 1 << 6,
        Interface = 1 << 7,
        Enum = 1 << 8,
        Record = 1 << 9,
        Type = Class | Struct | Delegate | Interface | Enum | Record,
        Method = 1 << 10,
        Property = 1 << 11,
        Indexer = 1 << 12,
        Field = 1 << 13,
        Event = 1 << 14,
        Constant = 1 << 15,
        Member = Method | Property | Indexer | Field | Event | Constant,
        LocalVariable = 1 << 16,
        LocalFunction = 1 << 17,
        Local = LocalVariable | LocalFunction,
        Parameter = 1 << 18,
        TypeParameter = 1 << 19,
        UsingAlias = 1 << 20,
        ReturnType = 1 << 21,
        Symbol = Namespace | Type | Member | Local | Parameter | TypeParameter | UsingAlias | ReturnType,
        All = Symbol | NonSymbol,
    }
}
