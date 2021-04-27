// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class MergeLocalDeclarationsRefactoring
    {
        public void MethodName()
        {
#if DEBUG
            bool f = false;
            bool f2 = false;
            bool f3 = false;
#endif
            string s = null;
            bool f4 = f = f2 = f3;
        }

        public void MethodName2()
        {
            switch (0)
            {
                case 0:
                    bool f = false;
                    bool f2 = false;
                    bool f3 = false;
                    string s = null;
                    bool f4 = f = f2 = f3;
                    break;
            }
        }
    }
}
