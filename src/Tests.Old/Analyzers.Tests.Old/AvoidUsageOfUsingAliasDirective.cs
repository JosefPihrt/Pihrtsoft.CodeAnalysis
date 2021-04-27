// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using x = System;
using xx = System.Collections.Generic;
using xxx = System.String;

namespace Roslynator.CSharp.Analyzers.Tests
{
    public class AvoidUsageOfUsingAliasDirective
    {
        public void MethodName()
        {
            xxx s1 = xxx.Empty;
            xxx s2 = xxx.Empty;

            xx.List<string> t1 = new xx.List<string>();
            xx.List<string> t2 = new xx.List<string>();

            string u1 = x.String.Empty;
            string u2 = x.String.Empty;
        }
    }
}
