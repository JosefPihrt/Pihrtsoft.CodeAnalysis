// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class RemoveAllRegionDirectivesRefactoring
    {
        #region Methods
        public object GetValue() => null;
        #endregion

        #region Properties
        public string Value { get; set; }
        #endregion
    }
}
