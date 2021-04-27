// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class SplitAttributesRefactoring
    {

        [Required, StringLength(10)]
        public string Value { get; set; }

    }
}
