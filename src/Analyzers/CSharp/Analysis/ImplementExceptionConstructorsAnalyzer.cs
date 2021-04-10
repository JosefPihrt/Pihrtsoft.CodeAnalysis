// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ImplementExceptionConstructorsAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.ImplementExceptionConstructors);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterCompilationStartAction(startContext =>
            {
                INamedTypeSymbol exceptionSymbol = startContext.Compilation.GetTypeByMetadataName("System.Exception");

                if (exceptionSymbol == null)
                    return;

                startContext.RegisterSymbolAction(f => AnalyzeNamedType(f, exceptionSymbol), SymbolKind.NamedType);
            });
        }

        private static void AnalyzeNamedType(SymbolAnalysisContext context, INamedTypeSymbol exceptionSymbol)
        {
            var symbol = (INamedTypeSymbol)context.Symbol;

            if (symbol.TypeKind != TypeKind.Class)
                return;

            if (symbol.IsStatic)
                return;

            if (symbol.IsImplicitClass)
                return;

            if (symbol.IsImplicitlyDeclared)
                return;

            INamedTypeSymbol baseType = symbol.BaseType;

            if (baseType?.IsObject() != false)
                return;

            if (!baseType.EqualsOrInheritsFrom(exceptionSymbol))
                return;

            if (!GenerateBaseConstructorsAnalysis.IsAnyBaseConstructorMissing(symbol, baseType, f => !IsSerializationConstructor(f)))
                return;

            var classDeclaration = (ClassDeclarationSyntax)symbol.GetSyntax(context.CancellationToken);

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.ImplementExceptionConstructors, classDeclaration.Identifier);
        }

        private static bool IsSerializationConstructor(IMethodSymbol methodSymbol)
        {
            ImmutableArray<IParameterSymbol> parameters = methodSymbol.Parameters;

            return parameters.Length == 2
                && parameters[0].Type.HasMetadataName(MetadataNames.System_Runtime_Serialization_SerializationInfo)
                && parameters[1].Type.HasMetadataName(MetadataNames.System_Runtime_Serialization_StreamingContext);
        }
    }
}
