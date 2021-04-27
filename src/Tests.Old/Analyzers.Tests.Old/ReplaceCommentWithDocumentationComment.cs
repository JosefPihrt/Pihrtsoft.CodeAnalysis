// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

#pragma warning disable RCS1007, RCS1016, RCS1071, RCS1185, RCS1100, RCS1106, RCS1138, RCS1163, RCS1164, RCS1176

// x
namespace Roslynator.CSharp.Analyzers.Tests
{
    // x
    public static class ReplaceCommentWithDocumentationComment
    {
        // x1
        // x2
        private class Leading
        {
            // x
            public string FieldName;

            // x
            public const string ConstantName = null;

            // x
            public Leading(object parameter)
            {
            }

            // x
            ~Leading()
            {
            }

            // x
            public event EventHandler EventName;

            // x
            public event EventHandler<EventArgs> EventName2
            {
                add { }
                remove { }
            }

            // x
            public string PropertyName { get; set; }

            // x
            public string this[int index]
            {
                get { return null; }
                set { }
            }

            // x
            public void MethodName<T>(object parameter)
            {
            }

            // x
            public static explicit operator Leading(string value)
            {
                return new Leading(null);
            }

            // x
            public static explicit operator string(Leading value)
            {
                return null;
            }

            // x
            public static Leading operator !(Leading value)
            {
                return new Leading(null);
            }

            // x
            public enum EnumName
            {
                // x
                None
            }

            // x
            public interface InterfaceName<T>
            {
            }

            // x
            public struct StructName<T>
            {
            }

            // x
            public class ClassName<T>
            {
            }

            // x
            public delegate void DelegateName<T>(object parameter);

            // n

            /// <summary>
            /// x
            /// </summary>
            public class Foo2
            {
            }
        }

        public class Trailing // x
        {
            public string FieldName; // x

            public const string ConstantName = null; // x

            public Trailing() // x
            {
            }

            public Trailing(object x) : base() // x
            {
            }

            public Trailing(string x) => MethodName(); // x

            public Trailing(int x) => // x
                MethodName();

            public Trailing(long x) // x
                => MethodName();

            ~Trailing() // x
            {
            }

            ~Trailing() => MethodName(); // x

            ~Trailing() => // x
                MethodName();

            ~Trailing() // x
                => MethodName();

            public event EventHandler EventName; // x

            public event EventHandler<EventArgs> EventName2 // x
            {
                add { }
                remove { }
            }

            public event EventHandler<EventArgs> EventName3 { add { } remove { } } // x

            public string PropertyName  // x
            {
                get { return null; }
                set { }
            }

            public string PropertyName2 { get; set; } // x

            public string PropertyName3 { get { return null; } set { } } // x

            public string PropertyName4 => null; // x

            public string PropertyName5 => // x
                null;

            public string PropertyName6 // x
                => null;

            public string this[object index] // x
            {
                get { return null; }
                set { }
            }

            public string this[string index] { get { return null; } set { } } // x

            public string this[int index] => null; // x

            public string this[long index] => // x
                null;

            public string this[double index] // x
                => null;

            public void MethodName() // x
            {
            }

            public void MethodName2() { } // x

            public void MethodName3() => MethodName(); // x

            public void MethodName4() => // x
                MethodName();

            public void MethodName5() // x
                => MethodName();

            public void MethodName<T>() where T : class // x
            {
            }

            public void MethodName2<T>() where T : class { } // x

            public void MethodName3<T>() where T : class => MethodName(); // x

            public void MethodName4<T>() where T : class => // x
                MethodName();

            public void MethodName5<T>() where T : class // x
                => MethodName();

            public static explicit operator Trailing(string value) // x
            {
                return null;
            }

            public static explicit operator Trailing(string value) { return null; } // x

            public static explicit operator Trailing(string value) => null; // x

            public static explicit operator Trailing(string value) => // x
                null;

            public static explicit operator Trailing(string value) // x
                => null;

            public static Trailing operator !(Trailing value) // x
            {
                return null;
            }

            public static Trailing operator !(Trailing value) { return null; } // x

            public static Trailing operator !(Trailing value) => null; // x

            public static Trailing operator !(Trailing value) => // x
                null;

            public static Trailing operator !(Trailing value) // x
                => null;

            public class ClassName // x
            {
            }

            public class ClassName2 : IFoo // x
            {
            }

            public class ClassName3 // x
                : IFoo
            {
            }

            public class ClassName<T> // x
            {
            }

            public class ClassName2<T> : IFoo // x
            {
            }

            public class ClassName3<T> // x
                : IFoo
            {
            }

            public class ClassName4<T> where T : class // x
            {
            }

            public class ClassName5<T> : IFoo // x
                where T : class
            {
            }

            public class ClassName6<T> // x
                : IFoo where T : class
            {
            }

            public struct StructName // x
            {
            }

            public struct StructName2 : IFoo // x
            {
            }

            public struct StructName3 // x
                : IFoo
            {
            }

            public struct StructName<T> // x
            {
            }

            public struct StructName2<T> : IFoo // x
            {
            }

            public struct StructName3<T> // x
                : IFoo
            {
            }

            public struct StructName4<T> where T : class // x
            {
            }

            public struct StructName5<T> : IFoo // x
                where T : class
            {
            }

            public struct StructName6<T> // x
                : IFoo where T : class
            {
            }

            public interface IInterfaceName // x
            {
            }

            public interface IInterfaceName2 : IFoo // x
            {
            }

            public interface IInterfaceName3 // x
                : IFoo
            {
            }

            public interface IInterfaceName<T> // x
            {
            }

            public interface IInterfaceName2<T> : IFoo // x
            {
            }

            public interface IInterfaceName3<T> // x
                : IFoo
            {
            }

            public interface IInterfaceName4<T> where T : class // x
            {
            }

            public interface IInterfaceName5<T> : IFoo // x
                where T : class
            {
            }

            public interface IInterfaceName6<T> // x
                : IFoo where T : class
            {
            }

            public enum EnumName // x
            {
                None // x
            }

            public delegate void DelegateName<T>(object parameter); // x
        }

        // n

        /// <summary>
        /// x
        /// </summary>
        public interface IFoo
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public class Trailing2 // x
        {
            /// <summary>
            /// 
            /// </summary>
            public string FieldName; // x

            /// <summary>
            /// 
            /// </summary>
            public const string ConstantName = null; // x

            /// <summary>
            /// 
            /// </summary>
            public Trailing2() // x
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="x"></param>
            public Trailing2(object x) : base() // x
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="x"></param>
            public Trailing2(string x) => MethodName(); // x

            /// <summary>
            /// 
            /// </summary>
            /// <param name="x"></param>
            public Trailing2(int x) => // x
                MethodName();

            /// <summary>
            /// 
            /// </summary>
            /// <param name="x"></param>
            public Trailing2(long x) // x
                => MethodName();

            /// <summary>
            /// 
            /// </summary>
            ~Trailing2() // x
            {
            }

            /// <summary>
            /// 
            /// </summary>
            ~Trailing2() => MethodName(); // x

            /// <summary>
            /// 
            /// </summary>
            ~Trailing2() => // x
                MethodName();

            /// <summary>
            /// 
            /// </summary>
            ~Trailing2() // x
                => MethodName();

            /// <summary>
            /// 
            /// </summary>
            public event EventHandler EventName; // x

            /// <summary>
            /// 
            /// </summary>
            public event EventHandler<EventArgs> EventName2 // x
            {
                add { }
                remove { }
            }

            /// <summary>
            /// 
            /// </summary>
            public event EventHandler<EventArgs> EventName3 { add { } remove { } } // x

            /// <summary>
            /// 
            /// </summary>
            public string PropertyName  // x
            {
                get { return null; }
                set { }
            }

            /// <summary>
            /// 
            /// </summary>
            public string PropertyName2 { get; set; } // x

            /// <summary>
            /// 
            /// </summary>
            public string PropertyName3 { get { return null; } set { } } // x

            /// <summary>
            /// 
            /// </summary>
            public string PropertyName4 => null; // x

            /// <summary>
            /// 
            /// </summary>
            public string PropertyName5 => // x
                null;

            /// <summary>
            /// 
            /// </summary>
            public string PropertyName6 // x
                => null;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public string this[object index] // x
            {
                get { return null; }
                set { }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public string this[string index] { get { return null; } set { } } // x

            /// <summary>
            /// 
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public string this[int index] => null; // x

            /// <summary>
            /// 
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public string this[long index] => // x
                null;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public string this[double index] // x
                => null;

            /// <summary>
            /// 
            /// </summary>
            public void MethodName() // x
            {
            }

            /// <summary>
            /// 
            /// </summary>
            public void MethodName2() { } // x

            /// <summary>
            /// 
            /// </summary>
            public void MethodName3() => MethodName(); // x

            /// <summary>
            /// 
            /// </summary>
            public void MethodName4() => // x
                MethodName();

            /// <summary>
            /// 
            /// </summary>
            public void MethodName5() // x
                => MethodName();

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public void MethodName<T>() where T : class // x
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public void MethodName2<T>() where T : class { } // x

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public void MethodName3<T>() where T : class => MethodName(); // x

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public void MethodName4<T>() where T : class => // x
                MethodName();

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public void MethodName5<T>() where T : class // x
                => MethodName();

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            public static explicit operator Trailing2(string value) // x
            {
                return null;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            public static explicit operator Trailing2(string value) { return null; } // x

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            public static explicit operator Trailing2(string value) => null; // x

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            public static explicit operator Trailing2(string value) => // x
                null;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            public static explicit operator Trailing2(string value) // x
                => null;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static Trailing2 operator !(Trailing2 value) // x
            {
                return null;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static Trailing2 operator !(Trailing2 value) { return null; } // x

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static Trailing2 operator !(Trailing2 value) => null; // x

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static Trailing2 operator !(Trailing2 value) => // x
                null;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static Trailing2 operator !(Trailing2 value) // x
                => null;

            /// <summary>
            /// 
            /// </summary>
            public class ClassName // x
            {
            }

            /// <summary>
            /// 
            /// </summary>
            public class ClassName2 : IFoo // x
            {
            }

            /// <summary>
            /// 
            /// </summary>
            public class ClassName3 // x
                : IFoo
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public class ClassName<T> // x
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public class ClassName2<T> : IFoo // x
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public class ClassName3<T> // x
                : IFoo
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public class ClassName4<T> where T : class // x
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public class ClassName5<T> : IFoo // x
                where T : class
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public class ClassName6<T> // x
                : IFoo where T : class
            {
            }

            /// <summary>
            /// 
            /// </summary>
            public struct StructName // x
            {
            }

            /// <summary>
            /// 
            /// </summary>
            public struct StructName2 : IFoo // x
            {
            }

            /// <summary>
            /// 
            /// </summary>
            public struct StructName3 // x
                : IFoo
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public struct StructName<T> // x
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public struct StructName2<T> : IFoo // x
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public struct StructName3<T> // x
                : IFoo
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public struct StructName4<T> where T : class // x
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public struct StructName5<T> : IFoo // x
                where T : class
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public struct StructName6<T> // x
                : IFoo where T : class
            {
            }

            /// <summary>
            /// 
            /// </summary>
            public interface IInterfaceName // x
            {
            }

            /// <summary>
            /// 
            /// </summary>
            public interface IInterfaceName2 : IFoo // x
            {
            }

            /// <summary>
            /// 
            /// </summary>
            public interface IInterfaceName3 // x
                : IFoo
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public interface IInterfaceName<T> // x
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public interface IInterfaceName2<T> : IFoo // x
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public interface IInterfaceName3<T> // x
                : IFoo
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public interface IInterfaceName4<T> where T : class // x
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public interface IInterfaceName5<T> : IFoo // x
                where T : class
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            public interface IInterfaceName6<T> // x
                : IFoo where T : class
            {
            }

            /// <summary>
            /// 
            /// </summary>
            public enum EnumName // x
            {
                /// <summary>
                /// 
                /// </summary>
                None // x
            }

            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="parameter"></param>
            public delegate void DelegateName<T>(object parameter); // x
        }
    }
}
