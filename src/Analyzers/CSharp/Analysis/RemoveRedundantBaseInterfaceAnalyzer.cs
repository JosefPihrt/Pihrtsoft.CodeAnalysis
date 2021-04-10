// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RemoveRedundantBaseInterfaceAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.RemoveRedundantBaseInterface);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeBaseList(f), SyntaxKind.BaseList);
        }

        private static void AnalyzeBaseList(SyntaxNodeAnalysisContext context)
        {
            var baseList = (BaseListSyntax)context.Node;

            if (!baseList.IsParentKind(SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration, SyntaxKind.InterfaceDeclaration))
                return;

            if (baseList.ContainsDiagnostics)
                return;

            if (baseList.SpanContainsDirectives())
                return;

            SeparatedSyntaxList<BaseTypeSyntax> baseTypes = baseList.Types;

            if (baseTypes.Count <= 1)
                return;

            var isFirst = true;
            INamedTypeSymbol typeSymbol = null;
            SymbolInterfaceInfo baseClassInfo = default;
            List<SymbolInterfaceInfo> baseInterfaceInfos = null;

            foreach (BaseTypeSyntax baseType in baseTypes)
            {
                TypeSyntax type = baseType.Type;

                if (type?.IsMissing == false
                    && (context.SemanticModel.GetSymbol(type, context.CancellationToken) is INamedTypeSymbol baseSymbol))
                {
                    TypeKind typeKind = baseSymbol.TypeKind;

                    ImmutableArray<INamedTypeSymbol> allInterfaces = baseSymbol.AllInterfaces;

                    if (typeKind == TypeKind.Class)
                    {
                        if (!isFirst)
                            break;

                        if (allInterfaces.Any())
                            baseClassInfo = new SymbolInterfaceInfo(baseType, baseSymbol, allInterfaces);
                    }
                    else if (typeKind == TypeKind.Interface)
                    {
                        var baseInterfaceInfo = new SymbolInterfaceInfo(baseType, baseSymbol, allInterfaces);

                        if (baseInterfaceInfos == null)
                        {
                            if (allInterfaces.Any())
                                baseInterfaceInfos = new List<SymbolInterfaceInfo>() { baseInterfaceInfo };
                        }
                        else
                        {
                            foreach (SymbolInterfaceInfo baseInterfaceInfo2 in baseInterfaceInfos)
                            {
                                Analyze(baseInterfaceInfo, baseInterfaceInfo2);
                                Analyze(baseInterfaceInfo2, baseInterfaceInfo);
                            }
                        }

                        if (baseClassInfo.Symbol != null)
                        {
                            if (typeSymbol == null)
                                typeSymbol = context.SemanticModel.GetDeclaredSymbol((TypeDeclarationSyntax)baseList.Parent, context.CancellationToken);

                            Analyze(baseInterfaceInfo, baseClassInfo);
                        }
                    }
                }

                if (isFirst)
                    isFirst = false;
            }

            void Analyze(
                in SymbolInterfaceInfo interfaceInfo,
                in SymbolInterfaceInfo interfaceInfo2)
            {
                ImmutableArray<ISymbol> members = default;

                foreach (INamedTypeSymbol interfaceSymbol in interfaceInfo2.Interfaces)
                {
                    if (SymbolEqualityComparer.Default.Equals(interfaceInfo.Symbol, interfaceSymbol))
                    {
                        if (typeSymbol != null)
                        {
                            if (members.IsDefault)
                                members = typeSymbol.GetMembers();

                            if (IsExplicitlyImplemented(interfaceInfo, members))
                                continue;

                            if (IsImplementedWithNewKeyword(interfaceInfo, typeSymbol, context.CancellationToken))
                                continue;
                        }

                        BaseTypeSyntax baseType = interfaceInfo.BaseType;

                        DiagnosticHelpers.ReportDiagnostic(
                            context,
                            DiagnosticRules.RemoveRedundantBaseInterface,
                            baseType,
                            SymbolDisplay.ToMinimalDisplayString(interfaceInfo.Symbol, context.SemanticModel, baseType.SpanStart, SymbolDisplayFormats.DisplayName),
                            SymbolDisplay.ToMinimalDisplayString(interfaceInfo2.Symbol, context.SemanticModel, baseType.SpanStart, SymbolDisplayFormats.DisplayName));

                        return;
                    }
                }
            }
        }

        private static bool IsExplicitlyImplemented(in SymbolInterfaceInfo interfaceInfo, ImmutableArray<ISymbol> members)
        {
            if (IsExplicitlyImplemented(interfaceInfo.Symbol))
                return true;

            foreach (INamedTypeSymbol interfaceSymbol in interfaceInfo.Interfaces)
            {
                if (IsExplicitlyImplemented(interfaceSymbol))
                    return true;
            }

            return false;

            bool IsExplicitlyImplemented(ISymbol interfaceSymbol)
            {
                foreach (ISymbol member in members)
                {
                    switch (member.Kind)
                    {
                        case SymbolKind.Event:
                            {
                                foreach (IEventSymbol eventSymbol in ((IEventSymbol)member).ExplicitInterfaceImplementations)
                                {
                                    if (SymbolEqualityComparer.Default.Equals(eventSymbol.ContainingType, interfaceSymbol))
                                        return true;
                                }

                                break;
                            }
                        case SymbolKind.Method:
                            {
                                foreach (IMethodSymbol methodSymbol in ((IMethodSymbol)member).ExplicitInterfaceImplementations)
                                {
                                    if (SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType, interfaceSymbol))
                                        return true;
                                }

                                break;
                            }
                        case SymbolKind.Property:
                            {
                                foreach (IPropertySymbol propertySymbol in ((IPropertySymbol)member).ExplicitInterfaceImplementations)
                                {
                                    if (SymbolEqualityComparer.Default.Equals(propertySymbol.ContainingType, interfaceSymbol))
                                        return true;
                                }

                                break;
                            }
                    }
                }

                return false;
            }
        }

        private static bool IsImplementedWithNewKeyword(
            in SymbolInterfaceInfo interfaceInfo,
            INamedTypeSymbol typeSymbol,
            CancellationToken cancellationToken)
        {
            foreach (ISymbol member in interfaceInfo.Symbol.GetMembers())
            {
                string name = member.Name;

                if (name.StartsWith("get_", StringComparison.Ordinal)
                    || name.StartsWith("set_", StringComparison.Ordinal)
                    || name.StartsWith("add_", StringComparison.Ordinal)
                    || name.StartsWith("remove_", StringComparison.Ordinal))
                {
                    continue;
                }

                ISymbol symbol = typeSymbol.FindImplementationForInterfaceMember(member);

                if (symbol != null)
                {
                    foreach (SyntaxReference syntaxReference in symbol.DeclaringSyntaxReferences)
                    {
                        SyntaxNode node = syntaxReference.GetSyntax(cancellationToken);

                        switch (node.Kind())
                        {
                            case SyntaxKind.MethodDeclaration:
                                {
                                    var methodDeclaration = (MethodDeclarationSyntax)node;

                                    if (methodDeclaration.Modifiers.Contains(SyntaxKind.NewKeyword))
                                        return true;

                                    break;
                                }
                            case SyntaxKind.PropertyDeclaration:
                                {
                                    var propertyDeclaration = (PropertyDeclarationSyntax)node;

                                    if (propertyDeclaration.Modifiers.Contains(SyntaxKind.NewKeyword))
                                        return true;

                                    break;
                                }
                            case SyntaxKind.IndexerDeclaration:
                                {
                                    var indexerDeclaration = (IndexerDeclarationSyntax)node;

                                    if (indexerDeclaration.Modifiers.Contains(SyntaxKind.NewKeyword))
                                        return true;

                                    break;
                                }
                            case SyntaxKind.EventDeclaration:
                                {
                                    var eventDeclaration = (EventDeclarationSyntax)node;

                                    if (eventDeclaration.Modifiers.Contains(SyntaxKind.NewKeyword))
                                        return true;

                                    break;
                                }
                            case SyntaxKind.VariableDeclarator:
                                {
                                    if (node.IsParentKind(SyntaxKind.VariableDeclaration)
                                        && node.Parent.IsParentKind(SyntaxKind.EventFieldDeclaration))
                                    {
                                        var eventFieldDeclaration = (EventFieldDeclarationSyntax)node.Parent.Parent;

                                        if (eventFieldDeclaration.Modifiers.Contains(SyntaxKind.NewKeyword))
                                            return true;
                                    }

                                    break;
                                }
                            default:
                                {
                                    Debug.Fail(node.Kind().ToString());
                                    return true;
                                }
                        }
                    }
                }
            }

            return false;
        }

        private readonly struct SymbolInterfaceInfo
        {
            public SymbolInterfaceInfo(BaseTypeSyntax baseType, INamedTypeSymbol symbol, ImmutableArray<INamedTypeSymbol> interfaces)
            {
                BaseType = baseType;
                Symbol = symbol;
                Interfaces = interfaces;
            }

            public BaseTypeSyntax BaseType { get; }

            public INamedTypeSymbol Symbol { get; }

            public ImmutableArray<INamedTypeSymbol> Interfaces { get; }
        }
    }
}
