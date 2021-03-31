// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Roslynator.Host.Mef;

namespace Roslynator.FindSymbols
{
    internal static class SymbolFinder
    {
        internal static async Task<ImmutableArray<ISymbol>> FindSymbolsAsync(
            Project project,
            SymbolFinderOptions options = null,
            IFindSymbolsProgress progress = null,
            CancellationToken cancellationToken = default)
        {
            Compilation compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);

            INamedTypeSymbol generatedCodeAttribute = compilation.GetTypeByMetadataName("System.CodeDom.Compiler.GeneratedCodeAttribute");

            ImmutableArray<ISymbol>.Builder symbols = null;

            var namespaceOrTypeSymbols = new Stack<INamespaceOrTypeSymbol>();

            namespaceOrTypeSymbols.Push(compilation.Assembly.GlobalNamespace);

            while (namespaceOrTypeSymbols.Count > 0)
            {
                INamespaceOrTypeSymbol namespaceOrTypeSymbol = namespaceOrTypeSymbols.Pop();

                foreach (ISymbol symbol in namespaceOrTypeSymbol.GetMembers())
                {
                    SymbolKind kind = symbol.Kind;

                    if (kind == SymbolKind.Namespace)
                    {
                        var namespaceSymbol = (INamespaceSymbol)symbol;

                        SymbolFilterReason reason = options.GetReason(namespaceSymbol);

                        if (reason == SymbolFilterReason.None)
                            namespaceOrTypeSymbols.Push(namespaceSymbol);

                        continue;
                    }

                    var isUnused = false;

                    if (!options.UnusedOnly
                        || UnusedSymbolUtility.CanBeUnusedSymbol(symbol))
                    {
                        SymbolFilterReason reason = options.GetReason(symbol);

                        switch (reason)
                        {
                            case SymbolFilterReason.None:
                                {
                                    if (options.IgnoreGeneratedCode
                                        && GeneratedCodeUtility.IsGeneratedCode(symbol, generatedCodeAttribute, f => MefWorkspaceServices.Default.GetService<ISyntaxFactsService>(compilation.Language).IsComment(f), cancellationToken))
                                    {
                                        continue;
                                    }

                                    if (options.UnusedOnly
                                        && !symbol.IsImplicitlyDeclared)
                                    {
                                        isUnused = await UnusedSymbolUtility.IsUnusedSymbolAsync(symbol, project.Solution, cancellationToken).ConfigureAwait(false);
                                    }

                                    if (!options.UnusedOnly
                                        || isUnused)
                                    {
                                        progress?.OnSymbolFound(symbol);

                                        (symbols ??= ImmutableArray.CreateBuilder<ISymbol>()).Add(symbol);
                                    }

                                    break;
                                }
                            case SymbolFilterReason.Visibility:
                            case SymbolFilterReason.WithoutAttribute:
                            case SymbolFilterReason.ImplicitlyDeclared:
                                {
                                    continue;
                                }
                            case SymbolFilterReason.SymbolGroup:
                            case SymbolFilterReason.Ignored:
                            case SymbolFilterReason.WithAttribute:
                            case SymbolFilterReason.Other:
                                {
                                    break;
                                }
                            default:
                                {
                                    Debug.Fail(reason.ToString());
                                    break;
                                }
                        }
                    }

                    if (!isUnused
                        && kind == SymbolKind.NamedType)
                    {
                        namespaceOrTypeSymbols.Push((INamedTypeSymbol)symbol);
                    }
                }
            }

            return symbols?.ToImmutableArray() ?? ImmutableArray<ISymbol>.Empty;
        }
    }
}
