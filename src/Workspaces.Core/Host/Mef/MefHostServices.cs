// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Roslynator.Host.Mef
{
    internal class MefHostServices
    {
        private static MefHostServices _default;
        private static ImmutableArray<Assembly> _defaultAssemblies;

        private readonly CompositionContext _compositionContext;

        public MefHostServices(CompositionContext compositionContext)
        {
            _compositionContext = compositionContext;
        }

        public static MefHostServices Default
        {
            get
            {
                if (_default == null)
                {
                    MefHostServices services = Create(DefaultAssemblies);
                    Interlocked.CompareExchange(ref _default, services, null);
                }

                return _default;
            }
        }

        public static ImmutableArray<Assembly> DefaultAssemblies
        {
            get
            {
                if (_defaultAssemblies.IsDefault)
                    ImmutableInterlocked.InterlockedInitialize(ref _defaultAssemblies, LoadDefaultAssemblies());

                return _defaultAssemblies;
            }
        }

        public static MefHostServices Create(CompositionContext compositionContext)
        {
            if (compositionContext == null)
                throw new ArgumentNullException(nameof(compositionContext));

            return new MefHostServices(compositionContext);
        }

        public static MefHostServices Create(IEnumerable<Assembly> assemblies)
        {
            if (assemblies == null)
                throw new ArgumentNullException(nameof(assemblies));

            ContainerConfiguration compositionConfiguration = new ContainerConfiguration().WithAssemblies(assemblies);

            CompositionHost container = compositionConfiguration.CreateContainer();

            return new MefHostServices(container);
        }

        private static ImmutableArray<Assembly> LoadDefaultAssemblies()
        {
            return GetAssemblyNames()
                .Select(f => TryLoadAssembly(f))
                .Where(f => f != null)
                .ToImmutableArray();

            static IEnumerable<string> GetAssemblyNames()
            {
                AssemblyName assemblyName = typeof(MefHostServices).GetTypeInfo().Assembly.GetName();
                Version assemblyVersion = assemblyName.Version;
                string publicKeyToken = assemblyName.GetPublicKeyToken().Aggregate("", (s, b) => s + b.ToString("x2"));

                string prefix = Regex.Match(assemblyName.Name, @"\A.*Roslynator(?=\..*\z)", RegexOptions.RightToLeft).Value;

                yield return $"{prefix}.CSharp.Workspaces, Version={assemblyVersion}, Culture=neutral, PublicKeyToken={publicKeyToken}";
                yield return $"{prefix}.VisualBasic.Workspaces, Version={assemblyVersion}, Culture=neutral, PublicKeyToken={publicKeyToken}";
            }
        }

        private static Assembly TryLoadAssembly(string assemblyName)
        {
            try
            {
                return Assembly.Load(new AssemblyName(assemblyName));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public IEnumerable<Lazy<TExtension>> GetExports<TExtension>()
        {
            return _compositionContext.GetExports<TExtension>().Select(e => new Lazy<TExtension>(() => e));
        }

        public IEnumerable<Lazy<TExtension, TMetadata>> GetExports<TExtension, TMetadata>()
        {
            var importer = new WithMetadataImporter<TExtension, TMetadata>();
            _compositionContext.SatisfyImports(importer);
            return importer.Exports;
        }

        private class WithMetadataImporter<TExtension, TMetadata>
        {
            [ImportMany]
            public IEnumerable<Lazy<TExtension, TMetadata>> Exports { get; set; }
        }
    }
}

