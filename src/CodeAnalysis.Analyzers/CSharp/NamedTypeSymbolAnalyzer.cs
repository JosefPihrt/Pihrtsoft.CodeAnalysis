// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;

namespace Roslynator.CodeAnalysis.CSharp
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
                {
                    Immutable.InterlockedInitialize(
                        ref _supportedDiagnostics,
                        DiagnosticRules.UnknownLanguageName,
                        DiagnosticRules.SpecifyExportCodeFixProviderAttributeName,
                        DiagnosticRules.SpecifyExportCodeRefactoringProviderAttributeName);
                }

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSymbolAction(f => AnalyzeNamedTypeSymbol(f), SymbolKind.NamedType);
        }

        private static void AnalyzeNamedTypeSymbol(SymbolAnalysisContext context)
        {
            var symbol = (INamedTypeSymbol)context.Symbol;

            if (symbol.TypeKind != TypeKind.Class)
                return;

            INamedTypeSymbol baseType = symbol.BaseType;

            while (baseType != null)
            {
                switch (baseType.Name)
                {
                    case "DiagnosticAnalyzer":
                        {
                            if (baseType.ContainingNamespace.HasMetadataName(RoslynMetadataNames.Microsoft_CodeAnalysis_Diagnostics))
                            {
                                AnalyzeDiagnosticAnalyzer(context, symbol);
                                return;
                            }

                            break;
                        }
                    case "CodeFixProvider":
                        {
                            if (baseType.ContainingNamespace.HasMetadataName(RoslynMetadataNames.Microsoft_CodeAnalysis_CodeFixes))
                            {
                                AnalyzeCodeFixProvider(context, symbol);
                                return;
                            }

                            break;
                        }
                    case "CodeRefactoringProvider":
                        {
                            if (baseType.ContainingNamespace.HasMetadataName(RoslynMetadataNames.Microsoft_CodeAnalysis_CodeRefactorings))
                            {
                                AnalyzeCodeRefactoringProvider(context, symbol);
                                return;
                            }

                            break;
                        }
                }

                baseType = baseType.BaseType;
            }
        }

        private static void AnalyzeDiagnosticAnalyzer(SymbolAnalysisContext context, INamedTypeSymbol symbol)
        {
            AttributeData attribute = symbol.GetAttribute(RoslynMetadataNames.Microsoft_CodeAnalysis_Diagnostics_DiagnosticAnalyzerAttribute);

            if (attribute == null)
                return;

            if (DiagnosticRules.UnknownLanguageName.IsEffective(context))
                AnalyzeLanguageName(context, attribute);
        }

        private static void AnalyzeCodeFixProvider(SymbolAnalysisContext context, INamedTypeSymbol symbol)
        {
            AttributeData attribute = symbol.GetAttribute(RoslynMetadataNames.Microsoft_CodeAnalysis_CodeFixes_ExportCodeFixProviderAttribute);

            if (attribute == null)
                return;

            if (DiagnosticRules.UnknownLanguageName.IsEffective(context))
                AnalyzeLanguageName(context, attribute);

            if (DiagnosticRules.SpecifyExportCodeFixProviderAttributeName.IsEffective(context)
                && !ContainsNamedArgument(attribute, "Name"))
            {
                ReportDiagnostic(context, attribute, DiagnosticRules.SpecifyExportCodeFixProviderAttributeName);
            }
        }

        private static void AnalyzeCodeRefactoringProvider(SymbolAnalysisContext context, INamedTypeSymbol symbol)
        {
            AttributeData attribute = symbol.GetAttribute(RoslynMetadataNames.Microsoft_CodeAnalysis_CodeRefactorings_ExportCodeRefactoringProviderAttribute);

            if (attribute == null)
                return;

            if (DiagnosticRules.UnknownLanguageName.IsEffective(context))
                AnalyzeLanguageName(context, attribute);

            if (DiagnosticRules.SpecifyExportCodeRefactoringProviderAttributeName.IsEffective(context)
                && !ContainsNamedArgument(attribute, "Name"))
            {
                ReportDiagnostic(context, attribute, DiagnosticRules.SpecifyExportCodeRefactoringProviderAttributeName);
            }
        }

        private static void AnalyzeLanguageName(SymbolAnalysisContext context, AttributeData attribute)
        {
            int argumentIndex = 0;

            foreach (TypedConstant constructorArgument in attribute.ConstructorArguments)
            {
                switch (constructorArgument.Kind)
                {
                    case TypedConstantKind.Primitive:
                        {
                            if (constructorArgument.Type.SpecialType == SpecialType.System_String
                                && !RoslynUtility.WellKnownLanguageNames.Contains((string)constructorArgument.Value))
                            {
                                ReportUnknownLanguageName(context, attribute, argumentIndex);
                            }

                            argumentIndex++;
                            break;
                        }
                    case TypedConstantKind.Array:
                        {
                            foreach (TypedConstant typedConstant in constructorArgument.Values)
                            {
                                if (typedConstant.Kind == TypedConstantKind.Primitive
                                    && typedConstant.Type.SpecialType == SpecialType.System_String
                                    && !RoslynUtility.WellKnownLanguageNames.Contains((string)typedConstant.Value))
                                {
                                    ReportUnknownLanguageName(context, attribute, argumentIndex);
                                }

                                argumentIndex++;
                            }

                            break;
                        }
                    default:
                        {
                            return;
                        }
                }
            }
        }

        private static bool ContainsNamedArgument(AttributeData attribute, string name)
        {
            foreach (KeyValuePair<string, TypedConstant> namedArgument in attribute.NamedArguments)
            {
                if (string.Equals(namedArgument.Key, name, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static void ReportUnknownLanguageName(SymbolAnalysisContext context, AttributeData attributeData, int argumentIndex)
        {
            var attribute = (AttributeSyntax)attributeData.ApplicationSyntaxReference.GetSyntax(context.CancellationToken);

            SeparatedSyntaxList<AttributeArgumentSyntax> arguments = attribute.ArgumentList.Arguments;

            for (int i = 0; i < arguments.Count; i++)
            {
                if (argumentIndex == i)
                {
                    DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UnknownLanguageName, arguments[i].Expression);
                    break;
                }
            }
        }

        private static void ReportDiagnostic(SymbolAnalysisContext context, AttributeData attributeData, DiagnosticDescriptor descriptor)
        {
            var attribute = (AttributeSyntax)attributeData.ApplicationSyntaxReference.GetSyntax(context.CancellationToken);

            DiagnosticHelpers.ReportDiagnostic(context, descriptor, attribute.Name);
        }
    }
}
