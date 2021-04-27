// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using static System.Math;

#pragma warning disable CS0219, RCS1118, RCS1138, RCS1141, RCS1163, RCS1196, RCS1213

namespace Roslynator.CSharp.Refactorings.Tests
{
    public partial class InlineMethodRefactoring
    {
        public void Method()
        {
            Method();

            Entity x = null;
            Entity x2 = null;
            Entity y = null;
            Entity z = null;
            Entity item = null;
            Entity item2 = null;
            Entity tx = null;
            Entity ty = null;
            Entity tx2 = null;
            Entity ty2 = null;

            Entity.VoidMethod(x, y);

            x = Entity.Method(x, y);

            x = Entity.GenericMethod(x, y);

            x = Entity.MethodWithExpressionBody(x, y);

            Entity.VoidMethod2(x, y);

            x = Entity.Method2(x, y);

            x = Method3(x, y);

            x = Entity.Method(Entity.Method(x, y), y);

            x = Entity.Method2(Entity.Method2(x, y), y);

            x = z.ExtensionMethod(x, y).ExtensionMethod(x, y);

            x = z.ExtensionMethod2(x, y).ExtensionMethod2(x, y);

            z.InstanceMethod(x, y);

            switch (true)
            {
                case true:
                    Entity.VoidMethod(x, y);
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static Entity Method3(Entity p1, Entity p2)
        {
#if DEBUG
            return p1 + p2 + p2;
        }
#endif

        public partial class Entity
        {
            private readonly Entity _value;

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public static Entity Method(Entity p1, Entity p2)
            {
#if DEBUG
                return p1 + p2 + p2;
            }
#endif

            public static Entity GenericMethod<TEntity>(TEntity p1, TEntity p2) where TEntity : Entity
            {
                return p1 + p2 + p2;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public static void VoidMethod(Entity p1, Entity p2)
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

                var items = new List<string>();
                foreach (var item in items)
                {
                    var item2 = item;
                }

                var items2 = new List<(int, int)>();
                foreach ((var tx, var ty) in items2)
                {
                    var tx2 = tx;
                    var ty2 = ty;
                }

                (int tx, int ty) GetTuple()
                {
                    return (0, 0);
                }
            }
#endif
            public static Entity MethodWithExpressionBody(Entity p1, Entity p2) => p1 + p2 + p2;

            public Entity InstanceMethod(Entity p1, Entity p2)
            {

                return _value + p1 + p2;
            }

            public Entity InstanceMethod2()
            {
                return InstanceMethod(null, null);
            }

            public static Entity operator +(Entity left, Entity right)
            {
                return null;
            }
        }
    }

    public static partial class InlineMethodRefactoringExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static InlineMethodRefactoring.Entity ExtensionMethod(
            this InlineMethodRefactoring.Entity entity,
            InlineMethodRefactoring.Entity ep1,
            InlineMethodRefactoring.Entity ep2)
        {
#if DEBUG
            return entity + ep1 + ep2;
        }
#endif
    }

    public static class InlineMethodSingleVariableDesignation
    {
        private static void Foo()
        {
            string value = null;
            string value2 = null;
            string value3 = null;
            string value4 = null;
            string value5 = null;

            Foo(value, value2, value3, value4, value5);

            value.Foo(value2, value3, value4, value5);
        }

        private static void Foo(this string s1, string s2, string s3, string s4, string s5)
        {
            if (s1 is object value)
            {
                value = null;
            }

            var dic = new Dictionary<object, object>();

            if (dic.TryGetValue(s2, out object value2))
            {
                value2 = null;
            }

            switch (s3)
            {
                case object value3:
                    {
                        value3 = null;
                        break;
                    }
            }

            (int value4, int value5) = Foo(s4, s5);

            value4 = 0;
            value5 = 0;
        }

        private static (int x, int y) Foo(string s1, string s2)
        {
            return (0, 0);
        }
    }
}
