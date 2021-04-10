// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UseAttributeUsageAttributeAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.UseAttributeUsageAttribute);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterCompilationStartAction(startContext =>
            {
                INamedTypeSymbol attributeSymbol = startContext.Compilation.GetTypeByMetadataName("System.Attribute");
                INamedTypeSymbol attributeUsageAttributeSymbol = startContext.Compilation.GetTypeByMetadataName("System.AttributeUsageAttribute");

                if (attributeSymbol != null
                    && attributeUsageAttributeSymbol != null)
                {
                    startContext.RegisterSymbolAction(
                        nodeContext => AnalyzerNamedTypeSymbol(nodeContext, attributeSymbol, attributeUsageAttributeSymbol),
                        SymbolKind.NamedType);
                }
            });
        }

        public static void AnalyzerNamedTypeSymbol(
            SymbolAnalysisContext context,
            INamedTypeSymbol attributeSymbol,
            INamedTypeSymbol attributeUsageAttributeSymbol)
        {
            var typeSymbol = (INamedTypeSymbol)context.Symbol;

            if (typeSymbol.IsImplicitlyDeclared)
                return;

            if (typeSymbol.TypeKind != TypeKind.Class)
                return;

            if (!typeSymbol.Name.EndsWith("Attribute", StringComparison.Ordinal))
                return;

            if (typeSymbol.HasAttribute(attributeUsageAttributeSymbol))
                return;

            INamedTypeSymbol baseType = typeSymbol.BaseType;

            while (baseType?.SpecialType == SpecialType.None)
            {
                if (SymbolEqualityComparer.Default.Equals(baseType, attributeSymbol))
                {
                    var classDeclaration = (ClassDeclarationSyntax)typeSymbol.GetSyntax(context.CancellationToken);

                    DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UseAttributeUsageAttribute, classDeclaration.Identifier);

                    return;
                }

                if (baseType.HasAttribute(attributeUsageAttributeSymbol))
                    return;

                baseType = baseType.BaseType;
            }
        }
    }
}
