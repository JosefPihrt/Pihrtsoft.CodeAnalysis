// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NamedTypeSymbolAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.ImplementNonGenericCounterpart);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSymbolAction(f => AnalyzeNamedType(f), SymbolKind.NamedType);
        }

        private static void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            if (context.Symbol.IsImplicitlyDeclared)
                return;

            var symbol = (INamedTypeSymbol)context.Symbol;

            TypeKind typeKind = symbol.TypeKind;

            if (typeKind == TypeKind.Class
                || typeKind == TypeKind.Struct)
            {
                if (symbol.IsPubliclyVisible())
                {
                    ImmutableArray<INamedTypeSymbol> interfaces = symbol.Interfaces;

                    if (interfaces.Any())
                    {
                        var fIComparable = false;
                        var fIComparableOfT = false;
                        var fIComparer = false;
                        var fIComparerOfT = false;
                        var fIEqualityComparer = false;
                        var fIEqualityComparerOfT = false;

                        foreach (INamedTypeSymbol interfaceSymbol in interfaces)
                        {
                            switch (interfaceSymbol.MetadataName)
                            {
                                case "IComparable":
                                    {
                                        if (interfaceSymbol.HasMetadataName(MetadataNames.System_IComparable))
                                            fIComparable = true;

                                        break;
                                    }
                                case "IComparable`1":
                                    {
                                        if (interfaceSymbol.HasMetadataName(MetadataNames.System_IComparable_T))
                                            fIComparableOfT = true;

                                        break;
                                    }
                                case "IComparer":
                                    {
                                        if (interfaceSymbol.HasMetadataName(MetadataNames.System_Collections_IComparer))
                                            fIComparer = true;

                                        break;
                                    }
                                case "IComparer`1":
                                    {
                                        if (interfaceSymbol.HasMetadataName(MetadataNames.System_Collections_Generic_IComparer_T))
                                            fIComparerOfT = true;

                                        break;
                                    }
                                case "IEqualityComparer":
                                    {
                                        if (interfaceSymbol.HasMetadataName(MetadataNames.System_Collections_IEqualityComparer))
                                            fIEqualityComparer = true;

                                        break;
                                    }
                                case "IEqualityComparer`1":
                                    {
                                        if (interfaceSymbol.HasMetadataName(MetadataNames.System_Collections_Generic_IEqualityComparer_T))
                                            fIEqualityComparerOfT = true;

                                        break;
                                    }
                            }
                        }

                        if (fIComparableOfT
                            && !fIComparable)
                        {
                            ReportDiagnostic(context, symbol, "IComparable");
                        }

                        if (fIComparerOfT
                            && !fIComparer)
                        {
                            ReportDiagnostic(context, symbol, "IComparer");
                        }

                        if (fIEqualityComparerOfT
                            && !fIEqualityComparer)
                        {
                            ReportDiagnostic(context, symbol, "IEqualityComparer");
                        }
                    }
                }
            }
        }

        private static void ReportDiagnostic(SymbolAnalysisContext context, INamedTypeSymbol symbol, string interfaceName)
        {
            SyntaxToken identifier = default;

            if (symbol.TypeKind == TypeKind.Class)
            {
                var classDeclaration = (ClassDeclarationSyntax)symbol.GetSyntax(context.CancellationToken);

                identifier = classDeclaration.Identifier;
            }
            else if (symbol.TypeKind == TypeKind.Struct)
            {
                var structDeclaration = (StructDeclarationSyntax)symbol.GetSyntax(context.CancellationToken);

                identifier = structDeclaration.Identifier;
            }

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.ImplementNonGenericCounterpart,
                identifier.GetLocation(),
                ImmutableDictionary.CreateRange(new[] { new KeyValuePair<string, string>("InterfaceName", interfaceName) }),
                interfaceName);
        }
    }
}
