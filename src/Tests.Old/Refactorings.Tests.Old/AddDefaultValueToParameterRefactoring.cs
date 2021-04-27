// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class AddDefaultValueToParameterRefactoring
    {
        public string GetValue(DateTime dateTime )
        {
            var items = new List<string>();

            var q = items.Select((string item ) =>
            {
                return item;
            });

            var q2 = items.Select(delegate(string item )
            {
                return item;
            });

            return null;
        }
    }
}
