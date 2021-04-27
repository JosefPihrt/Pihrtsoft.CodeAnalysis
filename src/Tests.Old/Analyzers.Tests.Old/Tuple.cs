// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class Tuple
    {
        public static IEnumerable<(string, string)> Values
        {
            get
            {
                yield return Value;
            }
        }

        public static (string, string) Value
        {
            get
            {
                string first = null;
                string second = null;

                return (first, second);
            }
        }
    }
}
