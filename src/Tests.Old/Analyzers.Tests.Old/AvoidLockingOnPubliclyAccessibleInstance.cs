// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Roslynator.CSharp.Analyzers.Tests
{
    public class AvoidLockingOnPubliclyAccessibleInstance
    {
        public void MethodName()
        {
            string _lockObject = "";

            lock (this)
            {
            }

            var items = new List<object>();

            IEnumerable<object> q = items.Select(f =>
            {
                lock (this)
                {
                }

                return f;
            });
        }

        public static void StaticMethodName()
        {
            lock (typeof(object))
            {
            }
        }
    }
}
