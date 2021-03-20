// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.Spelling
{
    public enum SpellingFixKind
    {
        None = 0,
        Predefined = 1,
        Swap = 2,
        Fuzzy = 3,
        Split = 4,
        User = 5,
    }
}
