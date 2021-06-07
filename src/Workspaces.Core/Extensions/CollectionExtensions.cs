// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Roslynator
{
    internal static class CollectionExtensions
    {
        public static T SingleOrDefault<T>(this IReadOnlyCollection<T> values, bool shouldThrow)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (shouldThrow)
            {
                return values.SingleOrDefault();
            }
            else
            {
                return (values.Count == 1) ? values.First() : default;
            }
        }

        public static T SingleOrDefault<T>(
            this IReadOnlyCollection<T> list,
            Func<T, bool> predicate,
            bool shouldThrow)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (shouldThrow)
                return list.SingleOrDefault(predicate);

            using (IEnumerator<T> en = list.GetEnumerator())
            {
                while (en.MoveNext())
                {
                    T item = en.Current;

                    if (predicate(item))
                    {
                        while (en.MoveNext())
                        {
                            if (predicate(en.Current))
                                return default;
                        }

                        return item;
                    }
                }
            }

            return default;
        }
    }
}
