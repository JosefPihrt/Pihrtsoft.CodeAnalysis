// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Roslynator.Testing.CSharp
{
    public static class WellKnownCSharpTestOptions
    {
        private static CSharpTestOptions _default_CSharp5;
        private static CSharpTestOptions _default_CSharp6;
        private static CSharpTestOptions _default_CSharp7;
        private static CSharpTestOptions _default_CSharp7_3;
        private static CSharpTestOptions _default_CSharp8;
        private static CSharpTestOptions _default_NullableReferenceTypes;

        public static CSharpTestOptions Default_CSharp5
        {
            get
            {
                if (_default_CSharp5 == null)
                    Interlocked.CompareExchange(ref _default_CSharp5, Create(), null);

                return _default_CSharp5;

                static CSharpTestOptions Create() => DefaultCSharpTestOptions.Value.WithParseOptions(DefaultCSharpTestOptions.Value.ParseOptions.WithLanguageVersion(LanguageVersion.CSharp5));
            }
        }

        public static CSharpTestOptions Default_CSharp6
        {
            get
            {
                if (_default_CSharp6 == null)
                    Interlocked.CompareExchange(ref _default_CSharp6, Create(), null);

                return _default_CSharp6;

                static CSharpTestOptions Create() => DefaultCSharpTestOptions.Value.WithParseOptions(DefaultCSharpTestOptions.Value.ParseOptions.WithLanguageVersion(LanguageVersion.CSharp6));
            }
        }

        public static CSharpTestOptions Default_CSharp7
        {
            get
            {
                if (_default_CSharp7 == null)
                    Interlocked.CompareExchange(ref _default_CSharp7, Create(), null);

                return _default_CSharp7;

                static CSharpTestOptions Create() => DefaultCSharpTestOptions.Value.WithParseOptions(DefaultCSharpTestOptions.Value.ParseOptions.WithLanguageVersion(LanguageVersion.CSharp7));
            }
        }

        public static CSharpTestOptions Default_CSharp7_3
        {
            get
            {
                if (_default_CSharp7_3 == null)
                    Interlocked.CompareExchange(ref _default_CSharp7_3, Create(), null);

                return _default_CSharp7_3;

                static CSharpTestOptions Create() => DefaultCSharpTestOptions.Value.WithParseOptions(DefaultCSharpTestOptions.Value.ParseOptions.WithLanguageVersion(LanguageVersion.CSharp7_3));
            }
        }

        public static CSharpTestOptions Default_CSharp8
        {
            get
            {
                if (_default_CSharp8 == null)
                    Interlocked.CompareExchange(ref _default_CSharp8, Create(), null);

                return _default_CSharp8;

                static CSharpTestOptions Create() => DefaultCSharpTestOptions.Value.WithParseOptions(DefaultCSharpTestOptions.Value.ParseOptions.WithLanguageVersion(LanguageVersion.CSharp8));
            }
        }

        public static CSharpTestOptions Default_NullableReferenceTypes
        {
            get
            {
                if (_default_NullableReferenceTypes == null)
                    Interlocked.CompareExchange(ref _default_NullableReferenceTypes, Create(), null);

                return _default_NullableReferenceTypes;

                static CSharpTestOptions Create() => DefaultCSharpTestOptions.Value.WithCompilationOptions(DefaultCSharpTestOptions.Value.CompilationOptions.WithNullableContextOptions(NullableContextOptions.Enable));
            }
        }
    }
}
