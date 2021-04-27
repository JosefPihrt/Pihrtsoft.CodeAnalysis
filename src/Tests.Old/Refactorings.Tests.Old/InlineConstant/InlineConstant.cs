// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable RCS1127

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class InlineConstant
    {
        public const string ConstantValue = "ConstantValue";

        public string Foo()
        {
            string value = ConstantValue;

            string x = null;
            x = InlineConstant.Constant.ToString();
            x = InlineConstant.Constant2.ToString();
            x = Roslynator.CSharp.Refactorings.Tests.InlineConstant.Constant;
            x = global::Roslynator.CSharp.Refactorings.Tests.InlineConstant.Constant;

            return value;
        }

        public const string Constant = "ConstantValue";
        public const string Constant2 = "ConstantValue2", Constant3 = "ConstantValue3";

        public void Bar()
        {
            string x = null;

            x = InlineConstant.Constant.ToString();
            x = InlineConstant.Constant2.ToString();
            x = Roslynator.CSharp.Refactorings.Tests.InlineConstant.Constant;
            x = global::Roslynator.CSharp.Refactorings.Tests.InlineConstant.Constant;
        }
    }
}
