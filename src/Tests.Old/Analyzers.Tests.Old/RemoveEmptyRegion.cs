// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class RemoveEmptyRegion
    {
        #region Methods
        public static void Foo()
        #endregion
        {
            #region Empty
            
            #endregion
        }
        #region Empty


        #endregion
    }
}
