// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public class AvoidLockingOnPubliclyAIntroduceFieldToLockOnRefactoring
    {
        private const string FooConst = "";

        public void Foo()
        {
            lock ()
            {
            }

            lock (this)
            {
            }

            var items = new List<object>();

            IEnumerable<object> q = items.Select(f =>
            {
                lock ()
                {
                }

                return f;
            });
        }

        public static void StaticFoo()
        {
            lock ()
            {
            }
        }
    }
}
