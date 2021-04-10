// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis;
using Roslynator.CodeFixes;
using Roslynator.CSharp;

namespace Roslynator
{
    internal static class CodeFixMap
    {
        private static ReadOnlyDictionary<string, CodeFixDescriptor> _codeFixDescriptorsById;
        private static ReadOnlyDictionary<string, ReadOnlyCollection<string>> _codeFixDescriptorsByCompilerDiagnosticId;
        private static ReadOnlyDictionary<string, DiagnosticDescriptor> _compilerDiagnosticsById;

        public static IReadOnlyList<string> GetCompilerDiagnosticIds(string codeFixId)
        {
            if (CodeFixDescriptorsById.TryGetValue(codeFixId, out CodeFixDescriptor codeFixDescriptor))
            {
                return codeFixDescriptor.FixableDiagnosticIds;
            }

            return ImmutableArray<string>.Empty;
        }

        public static IReadOnlyList<string> GetCodeFixIds(string compilerDiagnosticId)
        {
            if (CodeFixIdsByCompilerDiagnosticId.TryGetValue(compilerDiagnosticId, out ReadOnlyCollection<string> codeFixDescriptors))
            {
                return codeFixDescriptors;
            }

            return Empty.ReadOnlyList<string>();
        }

        public static IEnumerable<CompilerDiagnosticFix> GetCompilerDiagnosticFixes()
        {
            foreach ((CodeFixDescriptor codeFixDescriptor, string compilerDiagnosticId) in CodeFixDescriptorsById
                .SelectMany(kvp => kvp.Value.FixableDiagnosticIds.Select(compilerDiagnosticId => (codeFixDescriptor: kvp.Value, compilerDiagnosticId))))
            {
                yield return new CompilerDiagnosticFix(
                    compilerDiagnosticId: compilerDiagnosticId,
                    compilerDiagnosticTitle: CompilerDiagnosticsById[compilerDiagnosticId].Title.ToString(),
                    codeFixId: codeFixDescriptor.Id,
                    codeFixTitle: codeFixDescriptor.Title);
            }
        }

        public static ReadOnlyDictionary<string, CodeFixDescriptor> CodeFixDescriptorsById
        {
            get
            {
                if (_codeFixDescriptorsById == null)
                    Interlocked.CompareExchange(ref _codeFixDescriptorsById, LoadCodeFixDescriptorsById(), null);

                return _codeFixDescriptorsById;

                static ReadOnlyDictionary<string, CodeFixDescriptor> LoadCodeFixDescriptorsById()
                {
                    Dictionary<string, CodeFixDescriptor> dic = typeof(CodeFixDescriptors)
                        .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                        .Select(f => (CodeFixDescriptor)f.GetValue(null))
                        .ToDictionary(f => f.Id, f => f);

                    return new ReadOnlyDictionary<string, CodeFixDescriptor>(dic);
                }
            }
        }

        public static ReadOnlyDictionary<string, ReadOnlyCollection<string>> CodeFixIdsByCompilerDiagnosticId
        {
            get
            {
                if (_codeFixDescriptorsByCompilerDiagnosticId == null)
                    Interlocked.CompareExchange(ref _codeFixDescriptorsByCompilerDiagnosticId, LoadCodeFixDescriptorsByCompilerDiagnosticId(), null);

                return _codeFixDescriptorsByCompilerDiagnosticId;

                ReadOnlyDictionary<string, ReadOnlyCollection<string>> LoadCodeFixDescriptorsByCompilerDiagnosticId()
                {
                    Dictionary<string, ReadOnlyCollection<string>> dic = typeof(CodeFixDescriptors)
                        .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                        .Select(f => (CodeFixDescriptor)f.GetValue(null))
                        .SelectMany(f => f.FixableDiagnosticIds.Select(id => (codeFixDescriptor: f, id)))
                        .GroupBy(f => f.id)
                        .ToDictionary(f => f.Key, f => new ReadOnlyCollection<string>(f.Select(ff => ff.codeFixDescriptor.Id).ToList()));

                    return new ReadOnlyDictionary<string, ReadOnlyCollection<string>>(dic);
                }
            }
        }

        public static ReadOnlyDictionary<string, DiagnosticDescriptor> CompilerDiagnosticsById
        {
            get
            {
                if (_compilerDiagnosticsById == null)
                    Interlocked.CompareExchange(ref _compilerDiagnosticsById, LoadCompilerDiagnosticsById(), null);

                return _compilerDiagnosticsById;

                static ReadOnlyDictionary<string, DiagnosticDescriptor> LoadCompilerDiagnosticsById()
                {
                    Dictionary<string, DiagnosticDescriptor> dic = typeof(CompilerDiagnosticRules)
                        .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                        .Select(f => (DiagnosticDescriptor)f.GetValue(null))
                        .ToDictionary(f => f.Id, f => f);

                    return new ReadOnlyDictionary<string, DiagnosticDescriptor>(dic);
                }
            }
        }
    }
}
