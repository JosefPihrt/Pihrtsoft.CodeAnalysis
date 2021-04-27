// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator.CSharp.CodeFixes.Tests
{
    /// <summary>
    /// 
    /// </summary>
    public static class AddDocumentationComment
    {
        public class Foo
        {
            public string FieldName;

            public const string ConstantName = null;

            public Foo(object parameter)
            {
            }

            ~Foo()
            {
            }

            public event EventHandler EventName;

            protected virtual void OnEventName(EventArgs e)
            {
                EventName?.Invoke(this, e);
            }

            public event EventHandler<EventArgs> EventName2;

            protected virtual void OnEventName2(EventArgs e)
            {
                EventHandler<EventArgs> handler = EventName2;
                if (handler != null)
                    handler(this, e);
            }

            public string PropertyName { get; set; }

            public string this[int index]
            {
                get { return null; }
                set { }
            }

            public void MethodNameVoid<T>(object parameter)
            {
            }

            public string MethodName<T>(object parameter)
            {
                return null;
            }

            public static explicit operator Foo(string value)
            {
                return new Foo(null);
            }

            public static explicit operator string(Foo value)
            {
                return null;
            }

            public static Foo operator !(Foo value)
            {
                return new Foo(null);
            }

            public enum EnumName
            {

            }

            public interface InterfaceName<T>
            {
            }

            public struct StructName<T>
            {
            }

            public class ClassName<T>
            {
            }

            public delegate void DelegateName<T>(object parameter);

            public class BaseClass
            {
                /// <summary>
                /// virtual method
                /// </summary>
                public virtual void VirtualMethod()
                {
                }
            }

            public class DerivedClass : BaseClass
            {
                public override void VirtualMethod()
                {
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class Foo2
        {
            /// <summary>
            /// 
            /// </summary>
            public string FieldName;

            /// <summary>
            /// 
            /// </summary>
            public const string ConstantName = null;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parameter"></param>
            public Foo2(object parameter)
            {
            }

            /// <summary>
            /// 
            /// </summary>
            ~Foo2()
            {
            }

            /// <summary>
            /// 
            /// </summary>
            public event EventHandler EventName;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="e"></param>
            protected virtual void OnEventName(EventArgs e)
            {
                EventName?.Invoke(this, e);
            }

            /// <summary>
            /// 
            /// </summary>
            public event EventHandler<EventArgs> EventName2;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="e"></param>
            protected virtual void OnEventName2(EventArgs e)
            {
                EventHandler<EventArgs> handler = EventName2;
                if (handler != null)
                    handler(this, e);
            }

            /// <summary>
            /// 
            /// </summary>
            public string PropertyName { get; set; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public string this[int index]
            {
                get { return null; }
                set { }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="parameter"></param>
            public void MethodNameVoid<T>(object parameter)
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="parameter"></param>
            /// <returns></returns>
            public string MethodName<T>(object parameter)
            {
                return null;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            public static explicit operator Foo2(string value)
            {
                return new Foo2(null);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            public static explicit operator string(Foo2 value)
            {
                return null;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static Foo2 operator !(Foo2 value)
            {
                return new Foo2(null);
            }

            /// <summary>
            /// 
            /// </summary>
            public enum EnumName
            {

            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public interface InterfaceName<T>
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public struct StructName<T>
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public class ClassName<T>
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="parameter"></param>
            public delegate void DelegateName<T>(object parameter);
        }
    }
}
