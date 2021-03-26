// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Roslynator
{
    internal static class RuntimeMetadataReference
    {
        internal static readonly MetadataReference CorLibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

        private static ImmutableDictionary<string, string> _trustedPlatformAssemblyMap;
        private static ImmutableDictionary<string, MetadataReference> _metadataReferences;

        internal static ImmutableDictionary<string, string> TrustedPlatformAssemblyMap
        {
            get
            {
                if (_trustedPlatformAssemblyMap == null)
                    Interlocked.CompareExchange(ref _trustedPlatformAssemblyMap, CreateTrustedPlatformAssemblies(), null);

                return _trustedPlatformAssemblyMap;

                static ImmutableDictionary<string, string> CreateTrustedPlatformAssemblies()
                {
                    return AppContext
                        .GetData("TRUSTED_PLATFORM_ASSEMBLIES")
                        .ToString()
                        .Split(';')
                        .ToImmutableDictionary(f => Path.GetFileName(f));
                }
            }
        }

        internal static ImmutableDictionary<string, MetadataReference> DefaultMetadataReferences
        {
            get
            {
                if (_metadataReferences == null)
                    Interlocked.CompareExchange(ref _metadataReferences, CreateMetadataReferences(), null);

                return _metadataReferences;

                static ImmutableDictionary<string, MetadataReference> CreateMetadataReferences()
                {
                    return TrustedPlatformAssemblyMap
                        .Where(f =>
                        {
                            return f.Key.StartsWith("Microsoft.", StringComparison.Ordinal)
                                || f.Key.StartsWith("System.", StringComparison.Ordinal)
                                || string.Equals(f.Key, "netstandard.dll", StringComparison.Ordinal)
                                || string.Equals(f.Key, "mscorlib.dll", StringComparison.Ordinal);
                        })
                        .ToImmutableDictionary(f => f.Key, f => (MetadataReference)MetadataReference.CreateFromFile(f.Value));
                }
            }
        }
    }
}
