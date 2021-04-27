// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal static class UseListInsteadOfYield
    {
        private static IEnumerable<string> Foo()
        {
            yield return "a";
            yield return "b";
            yield return "c";
        }

        private static IEnumerable<string> Foo(object parameter1, object parameter2)
        {
            if (parameter1 == null)
                throw new ArgumentNullException(nameof(parameter1));

            if (parameter2 == null)
                throw new ArgumentNullException(nameof(parameter2));

            List<string> items = null;

            yield return "a";

            if (parameter1 != null)
            {
                yield return "b";

                if (parameter2 != null)
                {
                    yield return "c";
                    yield break;
                }
            }

            IEnumerable<string> Local()
            {
                yield return "";
            }
        }
    }
}
