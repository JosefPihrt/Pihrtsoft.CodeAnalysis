// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class StaticMemberInGenericTypeShouldUseTypeParameterAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.StaticMemberInGenericTypeShouldUseTypeParameter);

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
            var namedType = (INamedTypeSymbol)context.Symbol;

            if (!namedType.TypeKind.Is(TypeKind.Class, TypeKind.Struct))
                return;

            if (namedType.Arity == 0)
                return;

            if (namedType.IsStatic)
                return;

            if (namedType.IsImplicitClass)
                return;

            if (namedType.IsImplicitlyDeclared)
                return;

            var typeParameters = default(ImmutableArray<ITypeParameterSymbol>);

            foreach (ISymbol member in namedType.GetMembers())
            {
                if (member.IsImplicitlyDeclared)
                    continue;

                if (!member.IsStatic)
                    continue;

                if (!member.DeclaredAccessibility.Is(Accessibility.Public, Accessibility.Internal, Accessibility.ProtectedOrInternal))
                    continue;

                switch (member.Kind)
                {
                    case SymbolKind.Event:
                        {
                            var eventSymbol = (IEventSymbol)member;

                            if (typeParameters.IsDefault)
                                typeParameters = namedType.TypeParameters;

                            if (!ContainsAnyTypeParameter(typeParameters, eventSymbol.Type))
                                ReportDiagnostic(context, eventSymbol);

                            break;
                        }
                    case SymbolKind.Field:
                        {
                            var fieldSymbol = (IFieldSymbol)member;

                            if (typeParameters.IsDefault)
                                typeParameters = namedType.TypeParameters;

                            if (!ContainsAnyTypeParameter(typeParameters, fieldSymbol.Type))
                                ReportDiagnostic(context, fieldSymbol);

                            break;
                        }
                    case SymbolKind.Method:
                        {
                            var methodsymbol = (IMethodSymbol)member;

                            if (methodsymbol.MethodKind == MethodKind.Ordinary)
                            {
                                if (typeParameters.IsDefault)
                                    typeParameters = namedType.TypeParameters;

                                if (!ContainsAnyTypeParameter(typeParameters, methodsymbol.ReturnType)
                                    && !ContainsAnyTypeParameter(typeParameters, methodsymbol.Parameters))
                                {
                                    ReportDiagnostic(context, methodsymbol);
                                }
                            }

                            break;
                        }
                    case SymbolKind.Property:
                        {
                            var propertySymbol = (IPropertySymbol)member;

                            if (!propertySymbol.IsIndexer)
                            {
                                if (typeParameters.IsDefault)
                                    typeParameters = namedType.TypeParameters;

                                if (!ContainsAnyTypeParameter(typeParameters, propertySymbol.Type))
                                    ReportDiagnostic(context, propertySymbol);
                            }

                            break;
                        }
                }
            }
        }

        private static bool ContainsAnyTypeParameter(ImmutableArray<ITypeParameterSymbol> typeParameters, ImmutableArray<IParameterSymbol> parameters)
        {
            foreach (IParameterSymbol parameter in parameters)
            {
                if (ContainsAnyTypeParameter(typeParameters, parameter.Type))
                    return true;
            }

            return false;
        }

        private static bool ContainsAnyTypeParameter(
            ImmutableArray<ITypeParameterSymbol> typeParameters,
            ITypeSymbol typeSymbol)
        {
            switch (typeSymbol.Kind)
            {
                case SymbolKind.TypeParameter:
                    {
                        foreach (ITypeParameterSymbol typeParameter in typeParameters)
                        {
                            if (SymbolEqualityComparer.Default.Equals(typeParameter, typeSymbol))
                                return true;
                        }

                        return false;
                    }
                case SymbolKind.ArrayType:
                    {
                        return ContainsAnyTypeParameter(typeParameters, ((IArrayTypeSymbol)typeSymbol).ElementType);
                    }
                case SymbolKind.NamedType:
                    {
                        return ContainsAnyTypeParameter(typeParameters, ((INamedTypeSymbol)typeSymbol).TypeArguments);
                    }
            }

            Debug.Assert(typeSymbol.Kind == SymbolKind.ErrorType, typeSymbol.Kind.ToString());

            return true;
        }

        private static bool ContainsAnyTypeParameter(
            ImmutableArray<ITypeParameterSymbol> typeParameters,
            ImmutableArray<ITypeSymbol> typeArguments)
        {
            foreach (ITypeSymbol typeArgument in typeArguments)
            {
                SymbolKind kind = typeArgument.Kind;

                if (kind == SymbolKind.TypeParameter)
                {
                    foreach (ITypeParameterSymbol typeParameter in typeParameters)
                    {
                        if (SymbolEqualityComparer.Default.Equals(typeParameter, typeArgument))
                            return true;
                    }
                }
                else if (kind == SymbolKind.NamedType)
                {
                    if (ContainsAnyTypeParameter(typeParameters, ((INamedTypeSymbol)typeArgument).TypeArguments))
                        return true;
                }
            }

            return false;
        }

        private static void ReportDiagnostic(SymbolAnalysisContext context, ISymbol member)
        {
            SyntaxNode node = member.GetSyntaxOrDefault(context.CancellationToken);

            Debug.Assert(node != null, member.ToString());

            if (node == null)
                return;

            SyntaxToken identifier = CSharpUtility.GetIdentifier(node);

            Debug.Assert(!identifier.IsKind(SyntaxKind.None), node.ToString());

            if (identifier.Kind() == SyntaxKind.None)
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.StaticMemberInGenericTypeShouldUseTypeParameter,
                identifier);
        }
    }
}
