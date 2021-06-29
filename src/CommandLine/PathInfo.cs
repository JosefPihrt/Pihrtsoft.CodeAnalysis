// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

namespace Roslynator.CommandLine
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal readonly struct PathInfo
    {
        public PathInfo(string path, PathOrigin origin)
        {
            Path = path;
            Origin = origin;
        }

        public string Path { get; }

        public PathOrigin Origin { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{Origin}  {Path}";
    }
}
