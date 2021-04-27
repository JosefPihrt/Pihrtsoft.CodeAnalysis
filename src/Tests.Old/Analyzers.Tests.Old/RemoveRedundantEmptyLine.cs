// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#pragma warning disable RCS1126

namespace Roslynator.CSharp.Analyzers.Tests
{

    public static class RemoveRedundantEmptyLine
    {

        private static string _propertyName;


        public static string PropertyName
        {

            get { return _propertyName; }
            set { _propertyName = value; }

        }

        public static void Foo()
        {

            var options = RegexOptions.None;

            switch (options)
            {

                case RegexOptions.CultureInvariant:
                    {

                        break;

                    }


                case RegexOptions.ECMAScript:

                    break;

                default:
                    break;

            }

            try
            {
            }

            catch (ArgumentNullException)
            {

                throw;

            }

            catch (ArgumentException)
            {
                throw;
            }

            finally
            {

                Foo();

            }

            bool f = false;
            var items = new List<object>();

            if (f)

                Foo();

            if (f)

                Foo();

            else

                Foo();

            foreach (object item in items)

                Foo();

            foreach ((string, string) item in Tuple.Values)

                Foo();

            for (int i = 0; i < items.Count; i++)

                Foo();

            using ((IDisposable)null)

                Foo();

            while (f)

                Foo();

            do

                Foo();
            while (f);

            lock (null)

                Foo();

            unsafe
            {

                fixed (char* p = "")

                    Foo();

            }

        }

    }

}
