// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.RegularExpressions;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class ReplaceHasFlagWithBitwiseOperationRefactoring
    {
        public void Do()
        {
            RegexOptions options = RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase;

            if (options.HasFlag(RegexOptions.IgnoreCase))
            {
            }

            if (!options.HasFlag(RegexOptions.IgnoreCase))
            {
            }
        }
    }
}
