// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#pragma warning disable RCS1016, RCS1085, RCS1100

namespace Roslynator.CSharp.Analyzers.Tests
{
    public static class RemoveRedundantOverridingMember
    {
        public class Base
        {
            private string _property;

            private readonly Collection<string> _items;

            public virtual string Method() => null;

            public virtual string MethodWithDefaultValue(int parameter = 0) => null;

            public virtual string MethodWithParams(params int[] values) => null;

            public virtual string MethodWithParams2(params int[] values) => null;

            public virtual string MethodWithArray(int[] values) => null;

            public virtual void VoidMethod()
            {
            }

            public virtual string Property
            {
                get { return _property; }
                set { _property = value; }
            }

            public virtual string ReadOnlyProperty
            {
                get { return _property; }
            }

            public virtual string this[int index]
            {
                get { return _items[index]; }
                set { _items[index] = value; }
            }

            public virtual string this[int index, int index2 = 0]
            {
                get { return _items[index]; }
                set { _items[index] = value; }
            }
        }

        public class Derived : Base
        {
            public override string Method()
            {
                return base.Method();
            }

            public override string MethodWithDefaultValue(int parameter = 0) => base.MethodWithDefaultValue(parameter);

            public override string MethodWithParams(params int[] values) => base.MethodWithParams(values);

            public override string MethodWithParams2(int[] values) => base.MethodWithParams2(values);

            public override string MethodWithArray(params int[] values) => base.MethodWithArray(values);

            public override void VoidMethod() => base.VoidMethod();

            public override string Property
            {
                get { return base.Property; }
                set { base.Property = value; }
            }

            public override string ReadOnlyProperty
            {
                get { return base.ReadOnlyProperty; }
            }

            public override string this[int index]
            {
                get { return base[index]; }
                set { base[index] = value; }
            }

            //n

            public string MethodWithArray_() => MethodWithArray(1, 2, 3);

            public string MethodWithParams2_() => MethodWithParams2(1, 2, 3);

            public override string this[int index, int index2 = 1]
            {
                get => base[index, index2];
                set => base[index, index2] = value;
            }
        }

        public class Derived2 : Base
        {
            private string _property;
            private readonly Collection<string> _items;

            public override string Method() => null;

            public override string MethodWithDefaultValue(int parameter = 1) => base.MethodWithDefaultValue(parameter);

            public override void VoidMethod() => VoidMethod();

            public override string Property
            {
                get { return _property; }
                set { _property = value; }
            }

            public override string this[int index]
            {
                get { return _items[index]; }
                set { _items[index] = value; }
            }
        }

        public class Derived3 : Base
        {
            public sealed override string Method() => base.Method();

            public override string MethodWithDefaultValue(int parameter) => base.MethodWithDefaultValue(parameter);

            public sealed override void VoidMethod() => base.VoidMethod();

            public sealed override string Property
            {
                get { return base.Property; }
                set { base.Property = value; }
            }

            public sealed override string this[int index]
            {
                get { return base[index]; }
                set { base[index] = value; }
            }
        }

        public class Derived4 : Base
        {
            /// <summary>
            /// x
            /// </summary>
            public override string Method()
            {
                return base.Method();
            }

            /// <summary>
            /// x
            /// </summary>
            public override string Property
            {
                get { return base.Property; }
                set { base.Property = value; }
            }

            /// <summary>
            /// x
            /// </summary>
            /// <param name="index"></param>
            public override string this[int index]
            {
                get { return base[index]; }
                set { base[index] = value; }
            }
        }

        public class Derived5 : Base
        {
            /** <summary>x</summary> */
            public override string Method()
            {
                return base.Method();
            }

            /** <summary>x</summary> */
            public override string Property
            {
                get { return base.Property; }
                set { base.Property = value; }
            }

            /** <summary>x</summary> */
            public override string this[int index]
            {
                get { return base[index]; }
                set { base[index] = value; }
            }
        }

        public class Derived6 : Base
        {
            public override string Method()
            {
                // x
                return base.Method();
            }

            public override string Property
            {
                // x
                get { return base.Property; }
                set { base.Property = value; }
            }

            public override string this[int index]
            {
                // x
                get { return base[index]; }
                set { base[index] = value; }
            }
        }
    }
}
