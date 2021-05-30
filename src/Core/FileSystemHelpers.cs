// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Roslynator
{
    internal static class FileSystemHelpers
    {
        public static bool IsCaseSensitive { get; } = GetIsCaseSensitive();

        public static StringComparer Comparer { get; } = (IsCaseSensitive) ? StringComparer.CurrentCulture : StringComparer.CurrentCultureIgnoreCase;

        public static StringComparison Comparison { get; } = (IsCaseSensitive) ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;

        private static bool GetIsCaseSensitive()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return false;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return true;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return true;

            Debug.Fail(RuntimeInformation.OSDescription);

            return true;
        }

        public static bool IsDirectorySeparator(char ch)
        {
            return ch == Path.DirectorySeparatorChar
                || ch == Path.AltDirectorySeparatorChar;
        }

        public static bool TryGetNormalizedFullPath(string path, out string result)
        {
            return TryGetNormalizedFullPath(path, basePath: null, out result);
        }

        public static bool TryGetNormalizedFullPath(string path, string basePath, out string result)
        {
            try
            {
                if (basePath != null
                    && !Path.IsPathRooted(path))
                {
                    path = Path.Combine(basePath, path);
                }

                path = Path.GetFullPath(path);

                result = path;
                return true;
            }
            catch (ArgumentException)
            {
                result = null;
                return false;
            }
        }
    }
}
