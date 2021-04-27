// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Text;

#pragma warning disable RCS1163, RCS1176

namespace Roslynator.CSharp.Analyzers.Tests
{
    internal static class ReorderNamedArguments
    {
        private class Foo
        {
            private void Bar()
            {
                var foo = new Foo();

                string s = foo[value: 1, value3: 3, value2: 2];
            }

            private string this[int value, int value2, int value3] => "";
        }

        private static void Bar()
        {
            using (new StreamReader(
                stream: default(Stream),
                encoding: Encoding.UTF8,
                bufferSize: 0,
                leaveOpen: false,
                detectEncodingFromByteOrderMarks: true)) { }

            using (new StreamReader(
                default(Stream),
                Encoding.UTF8,
                true,
                leaveOpen: false,
                bufferSize: 0)) { }

            using (new StreamReader(
                leaveOpen: false,
                bufferSize: 0,
                detectEncodingFromByteOrderMarks: true,
                encoding: Encoding.UTF8,
                stream: default(Stream))) { }

            //n

            using (new StreamReader(default(Stream), Encoding.UTF8, true, 0, false)) { }

            using (new StreamReader(
                default(Stream),
                Encoding.UTF8,
                true,
                0,
                leaveOpen: false)) { }

            using (new StreamReader(
                default(Stream),
                Encoding.UTF8,
                true,
                bufferSize: 0,
                leaveOpen: false)) { }

            using (new StreamReader(
                default(Stream),
                Encoding.UTF8,
                true,
                bufferSize: 0,
                false)) { }

            using (new StreamReader(
                stream: default(Stream),
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 0,
                leaveOpen: false)) { }
        }
    }
}
