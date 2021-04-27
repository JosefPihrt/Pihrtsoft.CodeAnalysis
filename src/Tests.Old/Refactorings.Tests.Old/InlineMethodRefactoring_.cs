// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using static System.Math;

namespace Roslynator.CSharp.Refactorings.Tests
{
    public partial class InlineMethodRefactoring
    {
        public partial class Entity
        {
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public static Entity Method2(Entity p1, Entity p2)
            {
#if DEBUG
                return p1 + p2 + p2;
            }
#endif

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public static void VoidMethod2(Entity p1, Entity p2)
            {
#if DEBUG
                var a = p1 + p2 + p2;
                var b = p1 + p2 + p2;
                Entity.Method(p1, p2);
                Method(p1, p2);
                Method3(p1, p2);
                InlineMethodRefactoringExtensions.ExtensionMethod(p1, p1, p2);
                var x = Sqrt(2);
                var y = Sqrt(2);
                Action<int> z = (f) =>
                {
                    var x2 = x;
                    var y2 = y;
                };
            }
#endif
        }
    }

    public static partial class InlineMethodRefactoringExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static InlineMethodRefactoring.Entity ExtensionMethod2(
            this InlineMethodRefactoring.Entity entity,
            InlineMethodRefactoring.Entity ep1,
            InlineMethodRefactoring.Entity ep2)
        {
#if DEBUG
            return entity + ep1 + ep2;
        }
#endif
    }
}
