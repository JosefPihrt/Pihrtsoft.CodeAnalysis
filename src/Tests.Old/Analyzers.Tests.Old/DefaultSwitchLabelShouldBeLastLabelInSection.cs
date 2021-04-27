// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.RegularExpressions;

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class DefaultSwitchLabelShouldBeLastLabelInSection
    {
        public static void Foo()
        {
            RegexOptions options = RegexOptions.None;

            switch (options)
            {
                case RegexOptions.CultureInvariant:
                    break;
#if DEBUG
                default:
                case RegexOptions.Multiline:
                case RegexOptions.Singleline:
#endif
                    {
                        break;
                    }
            }

            switch (options)
            {
                case RegexOptions.CultureInvariant:
                    break;
                default:
                case RegexOptions.Multiline:
                case RegexOptions.Singleline:
#if DEBUG
                    {
                        break;
                    }
#endif
            }

            switch (options)
            {
                case RegexOptions.CultureInvariant:
                    break;
                default:
#if DEBUG
                case RegexOptions.Multiline:
                case RegexOptions.Singleline:
#endif
                    {
                        break;
                    }
            }
        }
    }
}
