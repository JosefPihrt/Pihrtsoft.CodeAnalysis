// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;

namespace Roslynator
{
    internal static class Immutable
    {
        public static bool InterlockedInitialize<T>(ref ImmutableArray<T> location, T item)
        {
            return ImmutableInterlocked.InterlockedInitialize(ref location, ImmutableArray.Create(item));
        }

        public static bool InterlockedInitialize<T>(ref ImmutableArray<T> location, T item1, T item2)
        {
            return ImmutableInterlocked.InterlockedInitialize(ref location, ImmutableArray.Create(item1, item2));
        }

        public static bool InterlockedInitialize<T>(ref ImmutableArray<T> location, T item1, T item2, T item3)
        {
            return ImmutableInterlocked.InterlockedInitialize(ref location, ImmutableArray.Create(item1, item2, item3));
        }

        public static bool InterlockedInitialize<T>(ref ImmutableArray<T> location, T item1, T item2, T item3, T item4)
        {
            return ImmutableInterlocked.InterlockedInitialize(ref location, ImmutableArray.Create(item1, item2, item3, item4));
        }

        public static bool InterlockedInitialize<T>(ref ImmutableArray<T> location, params T[] items)
        {
            return ImmutableInterlocked.InterlockedInitialize(ref location, ImmutableArray.Create(items));
        }
    }
}
