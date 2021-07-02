// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Roslynator
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal readonly struct SimpleProjectInfo
    {
        public SimpleProjectInfo(string name, string filePath)
        {
            Name = name;
            FilePath = filePath;
        }

        public string Name { get; }

        public string FilePath { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{Name}  {FilePath}";

        public static SimpleProjectInfo Create(Project project)
        {
            return new SimpleProjectInfo(project.Name, project.FilePath);
        }
    }
}
