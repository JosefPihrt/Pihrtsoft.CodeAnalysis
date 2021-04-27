// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#pragma warning disable RCS1132, RCS1163

namespace Roslynator.CSharp.Refactorings.Tests
{
    internal class CopyDocumentationCommentFromBaseMemberRefactoring
    {
        private class Comparer : IComparer<object>
        {
            public int Compare(object x, object y)
            {
                throw new NotImplementedException();
            }
        }

        private class ObjectCollection : Collection<List<object>>
        {
            public ObjectCollection(IList<List<object>> list) : base(list)
            {
            }
        }

        public class BaseClass
        {
            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="value"></param>
            public BaseClass(string value)
            {
            }

            /// <summary>
            /// method
            /// </summary>
            /// <returns></returns>
            public virtual string FooMethod()
            {
                return null;
            }

            /// <summary>
            /// property
            /// </summary>
            public virtual string FooProperty { get; set; }

            /// <summary>
            /// indexer
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public virtual string this[int index]
            {
                get { return null; }
            }

            /// <summary>
            /// event field
            /// </summary>
            public virtual event EventHandler FooEventField;

            /// <summary>
            /// event
            /// </summary>
            public virtual event EventHandler FooEvent
            {
                add { }
                remove { }
            }
        }

        public class DerivedClass : BaseClass
        {
            public DerivedClass(string value)
                : base(value)
            {
            }

            public override string FooMethod()
            {
                return base.FooMethod();
            }

            public override string this[int index]
            {
                get { return base[index]; }
            }

            public override string FooProperty
            {
                get { return base.FooProperty; }

                set { base.FooProperty = value; }
            }

            public override event EventHandler FooEventField;

            public override event EventHandler FooEvent;
        }

        public class DerivedClass2 : DerivedClass
        {
            public DerivedClass2(string value)
                : base(value)
            {
            }

            public override string FooMethod()
            {
                return base.FooMethod();
            }

            public override string this[int index]
            {
                get { return base[index]; }
            }

            public override string FooProperty
            {
                get { return base.FooProperty; }

                set { base.FooProperty = value; }
            }

            public override event EventHandler FooEventField;

            public override event EventHandler FooEvent;
        }

        public interface IFoo
        {
            /// <summary>
            /// method
            /// </summary>
            void Bar();

            /// <summary>
            /// property
            /// </summary>
            string PropertyName { get; set; }

            /// <summary>
            /// indexer
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            string this[int index] { get; set; }

            /// <summary>
            /// event
            /// </summary>
            event EventHandler EventName;

            /// <summary>
            /// event 2
            /// </summary>
            event EventHandler EventName2;
        }

        public abstract class Foo : IFoo
        {
            public abstract string this[int index] { get; set; }

            public string PropertyName { get; set; }

            public event EventHandler EventName;

            public event EventHandler EventName2
            {
                add { }
                remove { }
            }

            public void Bar()
            {
            }
        }
    }
}
