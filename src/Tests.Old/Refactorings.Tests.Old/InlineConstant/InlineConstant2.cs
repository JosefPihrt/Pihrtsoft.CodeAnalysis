// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable RCS1127

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class InlineConstant2
    {
        public void MethodName()
        {
            string x = null;

            x = InlineConstant.Constant;
            x = InlineConstant.Constant2;
            x = Roslynator.CSharp.Refactorings.Tests.InlineConstant.Constant;
            x = global::Roslynator.CSharp.Refactorings.Tests.InlineConstant.Constant;
        }
    }
}
