// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis.MakeMemberReadOnly
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MakeMemberReadOnlyAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticDescriptors.MakeFieldReadOnly,
                    DiagnosticDescriptors.UseReadOnlyAutoProperty);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeTypeDeclaration(f), SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeTypeDeclaration(f), SyntaxKind.StructDeclaration);
        }

        private static void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
        {
            var typeDeclaration = (TypeDeclarationSyntax)context.Node;

            if (typeDeclaration.Modifiers.Contains(SyntaxKind.PartialKeyword))
                return;

            bool skipField = !DiagnosticDescriptors.MakeFieldReadOnly.IsEffective(context);

            bool skipProperty = !DiagnosticDescriptors.UseReadOnlyAutoProperty.IsEffective(context)
                || ((CSharpCompilation)context.Compilation).LanguageVersion < LanguageVersion.CSharp6;

            MakeMemberReadOnlyWalker walker = MakeMemberReadOnlyWalker.GetInstance();

            walker.SemanticModel = context.SemanticModel;
            walker.CancellationToken = context.CancellationToken;

            Dictionary<string, (SyntaxNode node, ISymbol symbol)> symbols = walker.Symbols;

            foreach (MemberDeclarationSyntax memberDeclaration in typeDeclaration.Members)
            {
                switch (memberDeclaration.Kind())
                {
                    case SyntaxKind.PropertyDeclaration:
                        {
                            if (skipProperty)
                                break;

                            var propertyDeclaration = (PropertyDeclarationSyntax)memberDeclaration;

                            AccessorDeclarationSyntax setter = propertyDeclaration.Setter();

                            if (setter?.IsKind(SyntaxKind.InitAccessorDeclaration) == false
                                && setter.BodyOrExpressionBody() == null
                                && !setter.AttributeLists.Any()
                                && !setter.SpanContainsDirectives())
                            {
                                IPropertySymbol propertySymbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration, context.CancellationToken);

                                IMethodSymbol setMethod = propertySymbol.SetMethod;

                                if (setMethod?.DeclaredAccessibility == Accessibility.Private
                                    && setMethod.GetAttributes().IsEmpty
                                    && !propertySymbol.IsIndexer
                                    && !propertySymbol.IsReadOnly
                                    && ValidateType(propertySymbol.Type)
                                    && propertySymbol.ExplicitInterfaceImplementations.IsDefaultOrEmpty
                                    && AnalyzePropertyAttributes(propertySymbol))
                                {
                                    symbols[propertySymbol.Name] = (propertyDeclaration, propertySymbol);
                                }
                            }

                            break;
                        }
                    case SyntaxKind.FieldDeclaration:
                        {
                            if (skipField)
                                break;

                            var fieldDeclaration = (FieldDeclarationSyntax)memberDeclaration;

                            foreach (VariableDeclaratorSyntax declarator in fieldDeclaration.Declaration.Variables)
                            {
                                if (context.SemanticModel.GetDeclaredSymbol(declarator, context.CancellationToken) is IFieldSymbol fieldSymbol
                                    && !fieldSymbol.IsConst
                                    && fieldSymbol.DeclaredAccessibility == Accessibility.Private
                                    && !fieldSymbol.IsReadOnly
                                    && !fieldSymbol.IsVolatile
                                    && ValidateType(fieldSymbol.Type))
                                {
                                    symbols[fieldSymbol.Name] = (declarator, fieldSymbol);
                                }
                            }

                            break;
                        }
                }
            }

            if (symbols.Count > 0)
            {
                foreach (MemberDeclarationSyntax memberDeclaration in typeDeclaration.Members)
                {
                    walker.Visit(memberDeclaration);

                    if (symbols.Count == 0)
                        break;
                }

                if (symbols.Count > 0)
                {
                    foreach (KeyValuePair<string, (SyntaxNode node, ISymbol symbol)> kvp in symbols)
                    {
                        if (kvp.Value.node is PropertyDeclarationSyntax propertyDeclaration)
                        {
                            AccessorDeclarationSyntax setter = propertyDeclaration.Setter();

                            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.UseReadOnlyAutoProperty, setter);
                        }
                    }

                    foreach (IGrouping<VariableDeclarationSyntax, SyntaxNode> grouping in symbols
                        .Select(f => f.Value.node)
                        .Where(f => f.IsKind(SyntaxKind.VariableDeclarator))
                        .GroupBy(f => (VariableDeclarationSyntax)f.Parent))
                    {
                        int count = grouping.Key.Variables.Count;

                        if (count == 1
                            || count == grouping.Count())
                        {
                            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.MakeFieldReadOnly, grouping.Key.Parent);
                        }
                    }
                }
            }

            MakeMemberReadOnlyWalker.Free(walker);

            static bool AnalyzePropertyAttributes(IPropertySymbol propertySymbol)
            {
                foreach (AttributeData attributeData in propertySymbol.GetAttributes())
                {
                    INamedTypeSymbol attributeClass = attributeData.AttributeClass;

                    if (string.Equals(attributeClass.Name, "DependencyAttribute", StringComparison.Ordinal))
                        return false;

                    if (attributeClass.HasMetadataName(MetadataNames.System_Runtime_Serialization_DataMemberAttribute))
                        return false;
                }

                return true;
            }
        }

        private static bool ValidateType(ITypeSymbol type)
        {
            if (type.Kind == SymbolKind.ErrorType)
                return false;

            return type.IsReferenceType
                || type.TypeKind == TypeKind.Enum
                || CSharpFacts.IsSimpleType(type.SpecialType)
                || type.IsReadOnlyStruct();
        }
    }
}
