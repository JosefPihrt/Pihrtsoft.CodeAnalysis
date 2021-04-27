// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

#pragma warning disable CS0168

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class AvoidEmptyCatchClauseThatCatchesSystemException
    {
        private static void Foo()
        {
            try
            {
                Foo();
            }
            catch (Exception ex)
            {
            }

            //n

            try
            {
                Foo();
            }
            catch (Exception ex)
            {
                throw;
            }

            try
            {
                Foo();
            }
            catch (FormatException ex)
            {
                throw;
            }

            try
            {
                Foo();
            }
            catch (Exception ex) when (LogException(ex))
            {
            }

            try
            {
                Foo();
            }
            catch (object ex)
            {
            }

            try
            {
                Foo();
            }
            catch (Exception2 ex)
            {
            }
        }

        private static bool LogException(Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }
}
