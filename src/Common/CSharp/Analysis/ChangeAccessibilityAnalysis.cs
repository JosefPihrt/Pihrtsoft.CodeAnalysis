// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    internal static class ChangeAccessibilityAnalysis
    {
        private static readonly ImmutableDictionary<Accessibility, ImmutableArray<Accessibility>> _accessibilityArrayMap = ImmutableDictionary.CreateRange(new[]
            {
                new KeyValuePair<Accessibility, ImmutableArray<Accessibility>>(Accessibility.Public, ImmutableArray.Create(Accessibility.Public)),
                new KeyValuePair<Accessibility, ImmutableArray<Accessibility>>(Accessibility.Internal, ImmutableArray.Create(Accessibility.Internal)),
                new KeyValuePair<Accessibility, ImmutableArray<Accessibility>>(Accessibility.Protected, ImmutableArray.Create(Accessibility.Protected)),
                new KeyValuePair<Accessibility, ImmutableArray<Accessibility>>(Accessibility.Private, ImmutableArray.Create(Accessibility.Private)),
            });

        private static ImmutableArray<Accessibility> AvailableAccessibilities { get; } = ImmutableArray.Create(
            Accessibility.Public,
            Accessibility.Internal,
            Accessibility.Protected,
            Accessibility.Private);

        public static AccessibilityFilter GetValidAccessibilityFilter(
            MemberDeclarationListSelection selectedMembers,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default)
        {
            if (selectedMembers.Count < 2)
                return AccessibilityFilter.None;

            ImmutableArray<Accessibility> availableAccessibilities = AvailableAccessibilities;

            var isAllExplicit = true;
            var all = AccessibilityFilter.None;

            AccessibilityFilter valid = AccessibilityFilter.Public
                | AccessibilityFilter.Internal
                | AccessibilityFilter.Protected
                | AccessibilityFilter.Private;

            foreach (MemberDeclarationSyntax member in selectedMembers)
            {
                Accessibility accessibility = SyntaxAccessibility.GetExplicitAccessibility(member);

                if (accessibility == Accessibility.NotApplicable)
                {
                    isAllExplicit = false;

                    accessibility = SyntaxAccessibility.GetDefaultExplicitAccessibility(member);

                    if (accessibility == Accessibility.NotApplicable)
                        return AccessibilityFilter.None;
                }

                switch (accessibility)
                {
                    case Accessibility.Private:
                    case Accessibility.Protected:
                    case Accessibility.ProtectedAndInternal:
                    case Accessibility.ProtectedOrInternal:
                    case Accessibility.Internal:
                    case Accessibility.Public:
                        {
                            all |= accessibility.GetAccessibilityFilter();
                            break;
                        }
                    default:
                        {
                            Debug.Fail(accessibility.ToString());
                            return AccessibilityFilter.None;
                        }
                }

                ModifierListInfo modifiersInfo = SyntaxInfo.ModifierListInfo(member);

                if (modifiersInfo.Modifiers.ContainsAny(
                    SyntaxKind.AbstractKeyword,
                    SyntaxKind.VirtualKeyword,
                    SyntaxKind.OverrideKeyword))
                {
                    valid &= ~AccessibilityFilter.Private;
                }

                if (modifiersInfo.IsOverride
                    && IsBaseDeclarationWithoutSource(member, semanticModel, cancellationToken))
                {
                    switch (accessibility)
                    {
                        case Accessibility.Private:
                        case Accessibility.Protected:
                        case Accessibility.Internal:
                        case Accessibility.Public:
                            {
                                valid &= accessibility.GetAccessibilityFilter();

                                if (valid == AccessibilityFilter.None)
                                    return AccessibilityFilter.None;

                                availableAccessibilities = _accessibilityArrayMap[accessibility];
                                continue;
                            }
                        default:
                            {
                                return AccessibilityFilter.None;
                            }
                    }
                }

                foreach (Accessibility accessibility2 in availableAccessibilities)
                {
                    if (accessibility != accessibility2
                        && !SyntaxAccessibility.IsValidAccessibility(member, accessibility2, ignoreOverride: true))
                    {
                        valid &= ~accessibility2.GetAccessibilityFilter();

                        if (valid == AccessibilityFilter.None)
                            return AccessibilityFilter.None;
                    }
                }
            }

            if (isAllExplicit)
            {
                switch (all)
                {
                    case AccessibilityFilter.Private:
                    case AccessibilityFilter.Protected:
                    case AccessibilityFilter.Internal:
                    case AccessibilityFilter.Public:
                        {
                            valid &= ~all;
                            break;
                        }
                }
            }

            return valid;
        }

        public static ISymbol GetBaseSymbolOrDefault(SyntaxNode node, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            ISymbol symbol = GetDeclaredSymbol();

            if (symbol != null)
            {
                if (!symbol.IsOverride)
                    return symbol;

                symbol = symbol.BaseOverriddenSymbol();

                if (symbol != null)
                {
                    SyntaxNode syntax = symbol.GetSyntaxOrDefault(cancellationToken);

                    if (syntax != null)
                    {
                        if (syntax is MemberDeclarationSyntax
                            || syntax.Kind() == SyntaxKind.VariableDeclarator)
                        {
                            return symbol;
                        }
                    }
                }
            }

            return null;

            ISymbol GetDeclaredSymbol()
            {
                if (node is EventFieldDeclarationSyntax eventFieldDeclaration)
                {
                    VariableDeclaratorSyntax declarator = eventFieldDeclaration.Declaration?.Variables.SingleOrDefault(shouldThrow: false);

                    if (declarator != null)
                        return semanticModel.GetDeclaredSymbol(declarator, cancellationToken);

                    return null;
                }
                else
                {
                    return semanticModel.GetDeclaredSymbol(node, cancellationToken);
                }
            }
        }

        private static bool IsBaseDeclarationWithoutSource(
            MemberDeclarationSyntax member,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            switch (member.Kind())
            {
                case SyntaxKind.EventFieldDeclaration:
                    {
                        var eventFieldDeclaration = (EventFieldDeclarationSyntax)member;

                        foreach (VariableDeclaratorSyntax declarator in eventFieldDeclaration.Declaration.Variables)
                        {
                            var symbol = (IEventSymbol)semanticModel.GetDeclaredSymbol(declarator, cancellationToken);

                            if (symbol?
                                .BaseOverriddenEvent()?
                                .Locations
                                .FirstOrDefault()?
                                .Kind != LocationKind.SourceFile)
                            {
                                return true;
                            }
                        }

                        break;
                    }
                case SyntaxKind.MethodDeclaration:
                    {
                        var methodDeclaration = (MethodDeclarationSyntax)member;

                        if (semanticModel
                            .GetDeclaredSymbol(methodDeclaration, cancellationToken)?
                            .BaseOverriddenMethod()?
                            .Locations
                            .FirstOrDefault()?
                            .Kind != LocationKind.SourceFile)
                        {
                            return true;
                        }

                        break;
                    }
                case SyntaxKind.PropertyDeclaration:
                    {
                        var propertyDeclaration = (PropertyDeclarationSyntax)member;

                        if (semanticModel
                            .GetDeclaredSymbol(propertyDeclaration, cancellationToken)?
                            .BaseOverriddenProperty()?
                            .Locations
                            .FirstOrDefault()?
                            .Kind != LocationKind.SourceFile)
                        {
                            return true;
                        }

                        break;
                    }
                case SyntaxKind.EventDeclaration:
                    {
                        var eventDeclaration = (EventDeclarationSyntax)member;

                        if (semanticModel
                            .GetDeclaredSymbol(eventDeclaration, cancellationToken)?
                            .BaseOverriddenEvent()?
                            .Locations
                            .FirstOrDefault()?
                            .Kind != LocationKind.SourceFile)
                        {
                            return true;
                        }

                        break;
                    }
                case SyntaxKind.IndexerDeclaration:
                    {
                        var indexerDeclaration = (IndexerDeclarationSyntax)member;

                        if (semanticModel
                            .GetDeclaredSymbol(indexerDeclaration, cancellationToken)?
                            .BaseOverriddenProperty()?
                            .Locations
                            .FirstOrDefault()?
                            .Kind != LocationKind.SourceFile)
                        {
                            return true;
                        }

                        break;
                    }
            }

            return false;
        }
    }
}
