// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp.Analysis;
using System.Threading.Tasks;

namespace Roslynator.CSharp.Refactorings
{
    internal static class SelectedMemberDeclarationsRefactoring
    {
        public static ImmutableDictionary<Accessibility, string> _accessibilityIdentifierMap = ImmutableDictionary.CreateRange(new[]
            {
                new KeyValuePair<Accessibility, string>(Accessibility.Public, EquivalenceKey.Join(RefactoringIdentifiers.ChangeAccessibility, nameof(Accessibility.Public))),
                new KeyValuePair<Accessibility, string>(Accessibility.Internal, EquivalenceKey.Join(RefactoringIdentifiers.ChangeAccessibility, nameof(Accessibility.Internal))),
                new KeyValuePair<Accessibility, string>(Accessibility.Protected, EquivalenceKey.Join(RefactoringIdentifiers.ChangeAccessibility, nameof(Accessibility.Protected))),
                new KeyValuePair<Accessibility, string>(Accessibility.Private, EquivalenceKey.Join(RefactoringIdentifiers.ChangeAccessibility, nameof(Accessibility.Private)))
            });

        public static async Task ComputeRefactoringAsync(RefactoringContext context, MemberDeclarationListSelection selectedMembers)
        {
            if (context.IsRefactoringEnabled(RefactoringIdentifiers.ChangeAccessibility)
                && !selectedMembers.Parent.IsKind(SyntaxKind.InterfaceDeclaration))
            {
                SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                AccessibilityFilter validAccessibilities = ChangeAccessibilityAnalysis.GetValidAccessibilityFilter(selectedMembers, semanticModel, context.CancellationToken);

                if (validAccessibilities != AccessibilityFilter.None)
                {
                    bool canHaveMultipleDeclarations = CanHaveMultipleDeclarations();

                    TryRegisterRefactoring(validAccessibilities, Accessibility.Public, canHaveMultipleDeclarations);
                    TryRegisterRefactoring(validAccessibilities, Accessibility.Internal, canHaveMultipleDeclarations);
                    TryRegisterRefactoring(validAccessibilities, Accessibility.Protected, canHaveMultipleDeclarations);
                    TryRegisterRefactoring(validAccessibilities, Accessibility.Private, canHaveMultipleDeclarations);
                }
            }

            if (context.IsAnyRefactoringEnabled(
                RefactoringIdentifiers.ConvertBlockBodyToExpressionBody,
                RefactoringIdentifiers.ConvertExpressionBodyToBlockBody))
            {
                ConvertBodyAndExpressionBodyRefactoring.ComputeRefactoring(context, selectedMembers);
            }

            if (context.IsRefactoringEnabled(RefactoringIdentifiers.InitializeFieldFromConstructor)
                && !selectedMembers.Parent.IsKind(SyntaxKind.InterfaceDeclaration))
            {
                InitializeFieldFromConstructorRefactoring.ComputeRefactoring(context, selectedMembers);
            }

            if (context.IsRefactoringEnabled(RefactoringIdentifiers.AddEmptyLineBetweenDeclarations))
            {
                AddEmptyLineBetweenDeclarationsRefactoring.ComputeRefactoring(context, selectedMembers);
            }

            void TryRegisterRefactoring(AccessibilityFilter accessibilities, Accessibility accessibility, bool canHaveMultipleDeclarations)
            {
                if ((accessibilities & accessibility.GetAccessibilityFilter()) != 0)
                {
                    if (canHaveMultipleDeclarations)
                    {
                        context.RegisterRefactoring(
                            ChangeAccessibilityRefactoring.GetTitle(accessibility),
                            async cancellationToken =>
                            {
                                SemanticModel semanticModel = await context.Document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
                                return await ChangeAccessibilityRefactoring.RefactorAsync(context.Document.Solution(), selectedMembers, accessibility, semanticModel, cancellationToken).ConfigureAwait(false);
                            },
                            _accessibilityIdentifierMap[accessibility]);
                    }
                    else
                    {
                        context.RegisterRefactoring(
                            ChangeAccessibilityRefactoring.GetTitle(accessibility),
                            cancellationToken => ChangeAccessibilityRefactoring.RefactorAsync(context.Document, selectedMembers, accessibility, cancellationToken),
                            EquivalenceKey.Join(RefactoringIdentifiers.ChangeAccessibility, accessibility.ToString()));
                    }
                }
            }

            bool CanHaveMultipleDeclarations()
            {
                foreach (MemberDeclarationSyntax member in selectedMembers)
                {
                    switch (member.Kind())
                    {
                        case SyntaxKind.ClassDeclaration:
                            {
                                if (((ClassDeclarationSyntax)member).Modifiers.Contains(SyntaxKind.PartialKeyword))
                                    return true;

                                break;
                            }
                        case SyntaxKind.InterfaceDeclaration:
                            {
                                if (((InterfaceDeclarationSyntax)member).Modifiers.Contains(SyntaxKind.PartialKeyword))
                                    return true;

                                break;
                            }
                        case SyntaxKind.RecordDeclaration:
                            {
                                if (((RecordDeclarationSyntax)member).Modifiers.Contains(SyntaxKind.PartialKeyword))
                                    return true;

                                break;
                            }
                        case SyntaxKind.StructDeclaration:
                            {
                                if (((StructDeclarationSyntax)member).Modifiers.Contains(SyntaxKind.PartialKeyword))
                                    return true;

                                break;
                            }
                        case SyntaxKind.MethodDeclaration:
                            {
                                if (((MethodDeclarationSyntax)member).Modifiers.ContainsAny(SyntaxKind.PartialKeyword, SyntaxKind.AbstractKeyword, SyntaxKind.VirtualKeyword, SyntaxKind.OverrideKeyword))
                                    return true;

                                break;
                            }
                        case SyntaxKind.PropertyDeclaration:
                            {
                                if (((PropertyDeclarationSyntax)member).Modifiers.ContainsAny(SyntaxKind.AbstractKeyword, SyntaxKind.VirtualKeyword, SyntaxKind.OverrideKeyword))
                                    return true;

                                break;
                            }
                        case SyntaxKind.IndexerDeclaration:
                            {
                                if (((IndexerDeclarationSyntax)member).Modifiers.ContainsAny(SyntaxKind.AbstractKeyword, SyntaxKind.VirtualKeyword, SyntaxKind.OverrideKeyword))
                                    return true;

                                break;
                            }
                        case SyntaxKind.EventDeclaration:
                            {
                                if (((EventDeclarationSyntax)member).Modifiers.ContainsAny(SyntaxKind.AbstractKeyword, SyntaxKind.VirtualKeyword, SyntaxKind.OverrideKeyword))
                                    return true;

                                break;
                            }
                        case SyntaxKind.EventFieldDeclaration:
                            {
                                if (((EventFieldDeclarationSyntax)member).Modifiers.ContainsAny(SyntaxKind.AbstractKeyword, SyntaxKind.VirtualKeyword, SyntaxKind.OverrideKeyword))
                                    return true;

                                break;
                            }
                    }
                }

                return false;
            }
        }
    }
}
