// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MarkTypeWithDebuggerDisplayAttributeAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.MarkTypeWithDebuggerDisplayAttribute); }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterCompilationStartAction(startContext =>
            {
                INamedTypeSymbol debuggerDisplayAttributeSymbol = startContext.Compilation.GetTypeByMetadataName("System.Diagnostics.DebuggerDisplayAttribute");

                if (debuggerDisplayAttributeSymbol != null)
                {
                    startContext.RegisterSymbolAction(
                        nodeContext => AnalyzerNamedTypeSymbol(nodeContext, debuggerDisplayAttributeSymbol),
                        SymbolKind.NamedType);
                }
            });
        }

        public static void AnalyzerNamedTypeSymbol(SymbolAnalysisContext context, INamedTypeSymbol debuggerDisplayAttributeSymbol)
        {
            var typeSymbol = (INamedTypeSymbol)context.Symbol;

            if (typeSymbol.IsImplicitlyDeclared)
                return;

            TypeKind typeKind = typeSymbol.TypeKind;

            if (typeKind == TypeKind.Class)
            {
                if (typeSymbol.IsImplicitClass)
                    return;

                if (typeSymbol.IsScriptClass)
                    return;

                if (typeSymbol.IsStatic)
                    return;

                if (typeSymbol.IsAbstract)
                    return;
            }
            else if (typeKind != TypeKind.Struct)
            {
                return;
            }

            if (!typeSymbol.IsPubliclyVisible())
                return;

            if (typeSymbol.OriginalDefinition.HasAttribute(debuggerDisplayAttributeSymbol, includeBaseTypes: true))
                return;

            SyntaxToken identifier;

            if (typeKind == TypeKind.Class)
            {
                var classDeclaration = (ClassDeclarationSyntax)typeSymbol.GetSyntax(context.CancellationToken);

                identifier = classDeclaration.Identifier;
            }
            else
            {
                var structDeclaration = (StructDeclarationSyntax)typeSymbol.GetSyntax(context.CancellationToken);

                identifier = structDeclaration.Identifier;
            }

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.MarkTypeWithDebuggerDisplayAttribute, identifier, identifier.ValueText);
        }
    }
}
