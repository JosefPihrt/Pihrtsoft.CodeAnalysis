// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ModifiersCodeFixProvider))]
    [Shared]
    public sealed class ModifiersCodeFixProvider : BaseCodeFixProvider
    {
        private static readonly Accessibility[] _publicOrInternal = new[]
        {
            Accessibility.Public,
            Accessibility.Internal
        };

        private static readonly Accessibility[] _publicOrInternalOrProtected = new[]
        {
            Accessibility.Public,
            Accessibility.Internal,
            Accessibility.Protected
        };

        private static readonly Accessibility[] _publicOrInternalOrPrivate = new[]
        {
            Accessibility.Public,
            Accessibility.Internal,
            Accessibility.Private
        };

        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    CompilerDiagnosticIdentifiers.ModifierIsNotValidForThisItem,
                    CompilerDiagnosticIdentifiers.MoreThanOneProtectionModifier,
                    CompilerDiagnosticIdentifiers.AccessibilityModifiersMayNotBeUsedOnAccessorsInInterface,
                    CompilerDiagnosticIdentifiers.ModifiersCannotBePlacedOnEventAccessorDeclarations,
                    CompilerDiagnosticIdentifiers.AccessModifiersAreNotAllowedOnStaticConstructors,
                    CompilerDiagnosticIdentifiers.OnlyMethodsClassesStructsOrInterfacesMayBePartial,
                    CompilerDiagnosticIdentifiers.ClassCannotBeBothStaticAndSealed,
                    CompilerDiagnosticIdentifiers.FieldCanNotBeBothVolatileAndReadOnly,
                    CompilerDiagnosticIdentifiers.NewProtectedMemberDeclaredInSealedClass,
                    CompilerDiagnosticIdentifiers.StaticClassesCannotContainProtectedMembers,
                    CompilerDiagnosticIdentifiers.VirtualOrAbstractMembersCannotBePrivate,
                    CompilerDiagnosticIdentifiers.AbstractPropertiesCannotHavePrivateAccessors,
                    CompilerDiagnosticIdentifiers.StaticMemberCannotBeMarkedOverrideVirtualOrAbstract,
                    CompilerDiagnosticIdentifiers.AsyncModifierCanOnlyBeUsedInMethodsThatHaveBody,
                    CompilerDiagnosticIdentifiers.PartialMethodCannotHaveAccessModifiersOrVirtualAbstractOverrideNewSealedOrExternModifiers,
                    CompilerDiagnosticIdentifiers.ExtensionMethodMustBeStatic,
                    CompilerDiagnosticIdentifiers.NoDefiningDeclarationFoundForImplementingDeclarationOfPartialMethod,
                    CompilerDiagnosticIdentifiers.MethodHasParameterModifierThisWhichIsNotOnFirstParameter,
                    CompilerDiagnosticIdentifiers.CannotDeclareInstanceMembersInStaticClass,
                    CompilerDiagnosticIdentifiers.StaticClassesCannotHaveInstanceConstructors,
                    CompilerDiagnosticIdentifiers.ElementsDefinedInNamespaceCannotBeExplicitlyDeclaredAsPrivateProtectedOrProtectedInternal,
                    CompilerDiagnosticIdentifiers.NamespaceAlreadyContainsDefinition,
                    CompilerDiagnosticIdentifiers.TypeAlreadyContainsDefinition,
                    CompilerDiagnosticIdentifiers.NoSuitableMethodFoundToOverride,
                    CompilerDiagnosticIdentifiers.ExtensionMethodMustBeDefinedInNonGenericStaticClass,
                    CompilerDiagnosticIdentifiers.AsyncMethodsCannotHaveRefOrOutParameters,
                    CompilerDiagnosticIdentifiers.IteratorsCannotHaveRefOrOutParameters,
                    CompilerDiagnosticIdentifiers.CannotHaveInstancePropertyOrFieldInitializersInStruct,
                    CompilerDiagnosticIdentifiers.ReadOnlyFieldCannotBePassedAsRefOrOutValue,
                    CompilerDiagnosticIdentifiers.MemberMustDeclareBodyBecauseItIsNotMarkedAbstractExternOrPartial,
                    CompilerDiagnosticIdentifiers.NewVirtualMemberInSealedClass,
                    CompilerDiagnosticIdentifiers.InstanceFieldsOfReadOnlyStructsMustBeReadOnly,
                    CompilerDiagnosticIdentifiers.MemberCannotBeSealedBecauseItIsNotOverride);
            }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindToken(root, context.Span.Start, out SyntaxToken token))
                return;

            SyntaxNode node = token.Parent;

            if (!CSharpFacts.CanHaveModifiers(node.Kind()))
                node = node.FirstAncestor(f => CSharpFacts.CanHaveModifiers(f.Kind()));

            Debug.Assert(node != null, $"{nameof(node)} is null");

            if (node == null)
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case CompilerDiagnosticIdentifiers.ModifierIsNotValidForThisItem:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveInvalidModifier))
                                break;

                            SyntaxTokenList modifiers = SyntaxInfo.ModifierListInfo(node).Modifiers;

                            if (modifiers.Contains(token))
                            {
                                ModifiersCodeFixRegistrator.RemoveModifier(context, diagnostic, node, token);
                                break;
                            }
                            else if (IsInterfaceMemberOrExplicitInterfaceImplementation(node))
                            {
                                ModifiersCodeFixRegistrator.RemoveModifiers(
                                    context,
                                    diagnostic,
                                    node,
                                    modifiers,
                                    f =>
                                    {
                                        switch (f.Kind())
                                        {
                                            case SyntaxKind.PublicKeyword:
                                            case SyntaxKind.ProtectedKeyword:
                                            case SyntaxKind.InternalKeyword:
                                            case SyntaxKind.PrivateKeyword:
                                            case SyntaxKind.StaticKeyword:
                                            case SyntaxKind.VirtualKeyword:
                                            case SyntaxKind.OverrideKeyword:
                                            case SyntaxKind.AbstractKeyword:
                                                {
                                                    return true;
                                                }
                                        }

                                        return false;
                                    });
                            }
                            else if (node.IsKind(SyntaxKind.MethodDeclaration, SyntaxKind.PropertyDeclaration, SyntaxKind.IndexerDeclaration, SyntaxKind.EventDeclaration, SyntaxKind.EventFieldDeclaration)
                                && node.IsParentKind(SyntaxKind.StructDeclaration)
                                && modifiers.Contains(SyntaxKind.VirtualKeyword))
                            {
                                ModifiersCodeFixRegistrator.RemoveModifier(context, diagnostic, node, SyntaxKind.VirtualKeyword);
                            }
                            else if (node.IsKind(SyntaxKind.IndexerDeclaration)
                                && modifiers.Contains(SyntaxKind.StaticKeyword))
                            {
                                ModifiersCodeFixRegistrator.RemoveModifier(context, diagnostic, node, SyntaxKind.StaticKeyword);
                            }
                            else if (node.IsKind(SyntaxKind.PropertyDeclaration, SyntaxKind.IndexerDeclaration, SyntaxKind.EventDeclaration, SyntaxKind.EventFieldDeclaration)
                                && modifiers.Contains(SyntaxKind.AsyncKeyword))
                            {
                                ModifiersCodeFixRegistrator.RemoveModifier(context, diagnostic, node, SyntaxKind.AsyncKeyword);
                            }

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.MoreThanOneProtectionModifier:
                        {
                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveInvalidModifier))
                                ModifiersCodeFixRegistrator.RemoveModifier(context, diagnostic, node, token);

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.AccessibilityModifiersMayNotBeUsedOnAccessorsInInterface:
                    case CompilerDiagnosticIdentifiers.AccessModifiersAreNotAllowedOnStaticConstructors:
                        {
                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveInvalidModifier))
                                ModifiersCodeFixRegistrator.RemoveAccessibility(context, diagnostic, node);

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.ModifiersCannotBePlacedOnEventAccessorDeclarations:
                        {
                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveInvalidModifier))
                                ModifiersCodeFixRegistrator.RemoveModifiers(context, diagnostic, node);

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.OnlyMethodsClassesStructsOrInterfacesMayBePartial:
                        {
                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveInvalidModifier))
                                ModifiersCodeFixRegistrator.RemoveModifier(context, diagnostic, node, SyntaxKind.PartialKeyword);

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.ClassCannotBeBothStaticAndSealed:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveInvalidModifier))
                                break;

                            ModifiersCodeFixRegistrator.RemoveModifier(context, diagnostic, node, SyntaxKind.StaticKeyword, additionalKey: nameof(SyntaxKind.StaticKeyword));
                            ModifiersCodeFixRegistrator.RemoveModifier(context, diagnostic, node, SyntaxKind.SealedKeyword, additionalKey: nameof(SyntaxKind.SealedKeyword));
                            break;
                        }
                    case CompilerDiagnosticIdentifiers.FieldCanNotBeBothVolatileAndReadOnly:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveInvalidModifier))
                                break;

                            var fieldDeclaration = (FieldDeclarationSyntax)node;

                            ModifiersCodeFixRegistrator.RemoveModifier(context, diagnostic, fieldDeclaration, SyntaxKind.VolatileKeyword, additionalKey: nameof(SyntaxKind.VolatileKeyword));
                            ModifiersCodeFixRegistrator.RemoveModifier(context, diagnostic, fieldDeclaration, SyntaxKind.ReadOnlyKeyword, additionalKey: nameof(SyntaxKind.ReadOnlyKeyword));
                            break;
                        }
                    case CompilerDiagnosticIdentifiers.NewProtectedMemberDeclaredInSealedClass:
                    case CompilerDiagnosticIdentifiers.StaticClassesCannotContainProtectedMembers:
                        {
                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.ChangeAccessibility))
                                ModifiersCodeFixRegistrator.ChangeAccessibility(context, diagnostic, node, _publicOrInternalOrPrivate);

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.VirtualOrAbstractMembersCannotBePrivate:
                        {
                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveVirtualModifier))
                                ModifiersCodeFixRegistrator.RemoveModifier(context, diagnostic, node, SyntaxKind.VirtualKeyword, additionalKey: nameof(SyntaxKind.VirtualKeyword));

                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.ChangeAccessibility))
                                ModifiersCodeFixRegistrator.ChangeAccessibility(context, diagnostic, node, _publicOrInternalOrProtected);

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.AbstractPropertiesCannotHavePrivateAccessors:
                        {
                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveInvalidModifier))
                                ModifiersCodeFixRegistrator.RemoveAccessibility(context, diagnostic, node, additionalKey: CodeFixIdentifiers.RemoveInvalidModifier);

                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.ChangeAccessibility))
                                ModifiersCodeFixRegistrator.ChangeAccessibility(context, diagnostic, node, _publicOrInternalOrProtected);

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.StaticMemberCannotBeMarkedOverrideVirtualOrAbstract:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveInvalidModifier))
                                break;

                            if (!node.IsParentKind(SyntaxKind.ClassDeclaration)
                                || !((ClassDeclarationSyntax)node.Parent).Modifiers.Contains(SyntaxKind.StaticKeyword))
                            {
                                ModifiersCodeFixRegistrator.RemoveModifier(context, diagnostic, node, SyntaxKind.StaticKeyword, additionalKey: nameof(SyntaxKind.StaticKeyword));
                            }

                            ModifiersCodeFixRegistrator.RemoveModifier(context, diagnostic, node, SyntaxKind.OverrideKeyword, additionalKey: nameof(SyntaxKind.OverrideKeyword));
                            ModifiersCodeFixRegistrator.RemoveModifier(context, diagnostic, node, SyntaxKind.VirtualKeyword, additionalKey: nameof(SyntaxKind.VirtualKeyword));
                            ModifiersCodeFixRegistrator.RemoveModifier(context, diagnostic, node, SyntaxKind.AbstractKeyword, additionalKey: nameof(SyntaxKind.AbstractKeyword));
                            break;
                        }
                    case CompilerDiagnosticIdentifiers.AsyncModifierCanOnlyBeUsedInMethodsThatHaveBody:
                        {
                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveInvalidModifier))
                                ModifiersCodeFixRegistrator.RemoveModifier(context, diagnostic, node, SyntaxKind.AsyncKeyword);

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.PartialMethodCannotHaveAccessModifiersOrVirtualAbstractOverrideNewSealedOrExternModifiers:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveInvalidModifier))
                                break;

                            ModifiersCodeFixRegistrator.RemoveModifiers(
                                context,
                                diagnostic,
                                node,
                                f =>
                                {
                                    switch (f.Kind())
                                    {
                                        case SyntaxKind.PublicKeyword:
                                        case SyntaxKind.ProtectedKeyword:
                                        case SyntaxKind.InternalKeyword:
                                        case SyntaxKind.PrivateKeyword:
                                        case SyntaxKind.VirtualKeyword:
                                        case SyntaxKind.AbstractKeyword:
                                        case SyntaxKind.OverrideKeyword:
                                        case SyntaxKind.NewKeyword:
                                        case SyntaxKind.SealedKeyword:
                                        case SyntaxKind.ExternKeyword:
                                            {
                                                return true;
                                            }
                                    }

                                    return false;
                                });

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.ExtensionMethodMustBeStatic:
                        {
                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.AddStaticModifier))
                                AddStaticModifier(context, diagnostic, node, CodeFixIdentifiers.AddStaticModifier);

                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveThisModifier))
                            {
                                var methodDeclaration = (MethodDeclarationSyntax)node;

                                ParameterSyntax parameter = methodDeclaration.ParameterList.Parameters[0];

                                SyntaxToken modifier = parameter.Modifiers.Find(SyntaxKind.ThisKeyword);

                                ModifiersCodeFixRegistrator.RemoveModifier(context, diagnostic, parameter, modifier, additionalKey: CodeFixIdentifiers.RemoveThisModifier);
                            }

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.ExtensionMethodMustBeDefinedInNonGenericStaticClass:
                        {
                            if (!(node is ClassDeclarationSyntax classDeclaration))
                                return;

                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.AddStaticModifier)
                                && !classDeclaration.Modifiers.Contains(SyntaxKind.StaticKeyword))
                            {
                                AddStaticModifier(context, diagnostic, node, CodeFixIdentifiers.AddStaticModifier);
                            }

                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveThisModifier))
                            {
                                IEnumerable<ParameterSyntax> thisParameters = classDeclaration.Members
                                    .Where(f => f.IsKind(SyntaxKind.MethodDeclaration))
                                    .Cast<MethodDeclarationSyntax>()
                                    .Select(f => f.ParameterList?.Parameters.FirstOrDefault())
                                    .Where(f => f?.Modifiers.Contains(SyntaxKind.ThisKeyword) == true);

                                ModifiersCodeFixRegistrator.RemoveModifier(
                                    context,
                                    diagnostic,
                                    thisParameters,
                                    SyntaxKind.ThisKeyword,
                                    title: "Remove 'this' modifier from extension methods",
                                    additionalKey: CodeFixIdentifiers.RemoveThisModifier);
                            }

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.NoDefiningDeclarationFoundForImplementingDeclarationOfPartialMethod:
                        {
                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveInvalidModifier))
                                ModifiersCodeFixRegistrator.RemoveModifier(context, diagnostic, node, SyntaxKind.PartialKeyword);

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.MethodHasParameterModifierThisWhichIsNotOnFirstParameter:
                        {
                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveThisModifier))
                                ModifiersCodeFixRegistrator.RemoveModifier(context, diagnostic, token.Parent, token);

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.CannotDeclareInstanceMembersInStaticClass:
                    case CompilerDiagnosticIdentifiers.StaticClassesCannotHaveInstanceConstructors:
                        {
                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.AddStaticModifier))
                                AddStaticModifier(context, diagnostic, node, CodeFixIdentifiers.AddStaticModifier);

                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.MakeContainingClassNonStatic))
                            {
                                var classDeclaration = (ClassDeclarationSyntax)node.Parent;

                                ModifiersCodeFixRegistrator.RemoveModifier(
                                    context,
                                    diagnostic,
                                    classDeclaration,
                                    classDeclaration.Modifiers.Find(SyntaxKind.StaticKeyword),
                                    title: "Make containing class non-static",
                                    additionalKey: CodeFixIdentifiers.MakeContainingClassNonStatic);
                            }

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.ElementsDefinedInNamespaceCannotBeExplicitlyDeclaredAsPrivateProtectedOrProtectedInternal:
                        {
                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.ChangeAccessibility))
                                ModifiersCodeFixRegistrator.ChangeAccessibility(context, diagnostic, node, _publicOrInternal);

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.NamespaceAlreadyContainsDefinition:
                    case CompilerDiagnosticIdentifiers.TypeAlreadyContainsDefinition:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.AddPartialModifier))
                                break;

                            if (!node.IsKind(
                                SyntaxKind.ClassDeclaration,
                                SyntaxKind.StructDeclaration,
                                SyntaxKind.InterfaceDeclaration,
                                SyntaxKind.MethodDeclaration))
                            {
                                return;
                            }

                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            ISymbol symbol = semanticModel.GetDeclaredSymbol(node, context.CancellationToken);

                            ImmutableArray<SyntaxReference> syntaxReferences = symbol.DeclaringSyntaxReferences;

                            if (syntaxReferences.Length <= 1)
                                break;

                            ModifiersCodeFixRegistrator.AddModifier(
                                context,
                                diagnostic,
                                ImmutableArray.CreateRange(syntaxReferences, f => f.GetSyntax(context.CancellationToken)),
                                SyntaxKind.PartialKeyword,
                                title: $"Make {CSharpFacts.GetTitle(node)} partial");

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.NoSuitableMethodFoundToOverride:
                        {
                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveInvalidModifier))
                                ModifiersCodeFixRegistrator.RemoveModifier(context, diagnostic, node, SyntaxKind.OverrideKeyword);

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.AsyncMethodsCannotHaveRefOrOutParameters:
                    case CompilerDiagnosticIdentifiers.IteratorsCannotHaveRefOrOutParameters:
                    case CompilerDiagnosticIdentifiers.ReadOnlyFieldCannotBePassedAsRefOrOutValue:
                        {
                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveRefModifier))
                                ModifiersCodeFixRegistrator.RemoveModifier(context, diagnostic, node, SyntaxKind.RefKeyword, additionalKey: nameof(SyntaxKind.RefKeyword));

                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveOutModifier))
                                ModifiersCodeFixRegistrator.RemoveModifier(context, diagnostic, node, SyntaxKind.OutKeyword, additionalKey: nameof(SyntaxKind.OutKeyword));

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.CannotHaveInstancePropertyOrFieldInitializersInStruct:
                        {
                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.AddStaticModifier))
                                AddStaticModifier(context, diagnostic, node);

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.MemberMustDeclareBodyBecauseItIsNotMarkedAbstractExternOrPartial:
                        {
                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.AddModifierAbstract)
                                && node.Kind() == SyntaxKind.MethodDeclaration
                                && (node.Parent as ClassDeclarationSyntax)?.Modifiers.Contains(SyntaxKind.AbstractKeyword) == true)
                            {
                                ModifiersCodeFixRegistrator.AddModifier(context, diagnostic, node, SyntaxKind.AbstractKeyword);
                            }

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.NewVirtualMemberInSealedClass:
                        {
                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveVirtualModifier))
                            {
                                if (node is AccessorDeclarationSyntax
                                    && SyntaxInfo.ModifierListInfo(node.Parent.Parent).IsVirtual)
                                {
                                    node = node.Parent.Parent;
                                }

                                ModifiersCodeFixRegistrator.RemoveModifier(
                                    context,
                                    diagnostic,
                                    node,
                                    SyntaxKind.VirtualKeyword,
                                    additionalKey: CodeFixIdentifiers.RemoveVirtualModifier);
                            }

                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.MakeContainingClassUnsealed)
                                && node.Parent is ClassDeclarationSyntax classDeclaration)
                            {
                                ModifiersCodeFixRegistrator.RemoveModifier(
                                    context,
                                    diagnostic,
                                    classDeclaration,
                                    SyntaxKind.SealedKeyword,
                                    title: "Make containing class unsealed",
                                    additionalKey: CodeFixIdentifiers.MakeContainingClassUnsealed);
                            }

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.InstanceFieldsOfReadOnlyStructsMustBeReadOnly:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.MakeMemberReadOnly))
                                break;

                            ModifiersCodeFixRegistrator.AddModifier(context, diagnostic, node, SyntaxKind.ReadOnlyKeyword);
                            break;
                        }
                    case CompilerDiagnosticIdentifiers.MemberCannotBeSealedBecauseItIsNotOverride:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveSealedModifier))
                                break;

                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            if (semanticModel.GetDiagnostic(
                                CompilerDiagnosticIdentifiers.MemberHidesInheritedMemberToMakeCurrentMethodOverrideThatImplementationAddOverrideKeyword,
                                CSharpUtility.GetIdentifier(node).Span,
                                context.CancellationToken) != null)
                            {
                                break;
                            }

                            ModifiersCodeFixRegistrator.RemoveModifier(context, diagnostic, node, SyntaxKind.SealedKeyword);
                            break;
                        }
                }
            }
        }

        private static void AddStaticModifier(CodeFixContext context, Diagnostic diagnostic, SyntaxNode node, string additionalKey = null)
        {
            if (node.IsKind(SyntaxKind.ConstructorDeclaration)
                && ((ConstructorDeclarationSyntax)node).ParameterList?.Parameters.Any() == true)
            {
                return;
            }

            ModifiersCodeFixRegistrator.AddModifier(context, diagnostic, node, SyntaxKind.StaticKeyword, additionalKey: additionalKey);
        }

        private static bool IsInterfaceMemberOrExplicitInterfaceImplementation(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.MethodDeclaration:
                    {
                        return node.IsParentKind(SyntaxKind.InterfaceDeclaration)
                            || ((MethodDeclarationSyntax)node).ExplicitInterfaceSpecifier != null;
                    }
                case SyntaxKind.PropertyDeclaration:
                    {
                        return node.IsParentKind(SyntaxKind.InterfaceDeclaration)
                            || ((PropertyDeclarationSyntax)node).ExplicitInterfaceSpecifier != null;
                    }
                case SyntaxKind.IndexerDeclaration:
                    {
                        return node.IsParentKind(SyntaxKind.InterfaceDeclaration)
                            || ((IndexerDeclarationSyntax)node).ExplicitInterfaceSpecifier != null;
                    }
                case SyntaxKind.EventFieldDeclaration:
                    {
                        return node.IsParentKind(SyntaxKind.InterfaceDeclaration);
                    }
                case SyntaxKind.EventDeclaration:
                    {
                        return ((EventDeclarationSyntax)node).ExplicitInterfaceSpecifier != null;
                    }
            }

            return false;
        }
    }
}
