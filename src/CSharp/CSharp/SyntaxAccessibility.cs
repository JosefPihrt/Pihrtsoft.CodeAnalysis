// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp
{
    /// <summary>
    /// A set of static methods that are related to C# accessibility.
    /// </summary>
    public static class SyntaxAccessibility
    {
        /// <summary>
        /// Returns a default accessibility of the specified declaration.
        /// </summary>
        /// <param name="declaration"></param>
        public static Accessibility GetDefaultAccessibility(SyntaxNode declaration)
        {
            if (declaration == null)
                throw new ArgumentNullException(nameof(declaration));

            switch (declaration.Kind())
            {
                case SyntaxKind.ConstructorDeclaration:
                    return SyntaxAccessibility<ConstructorDeclarationSyntax>.Instance.GetDefaultAccessibility((ConstructorDeclarationSyntax)declaration);
                case SyntaxKind.DestructorDeclaration:
                    return SyntaxAccessibility<DestructorDeclarationSyntax>.Instance.GetDefaultAccessibility((DestructorDeclarationSyntax)declaration);
                case SyntaxKind.MethodDeclaration:
                    return SyntaxAccessibility<MethodDeclarationSyntax>.Instance.GetDefaultAccessibility((MethodDeclarationSyntax)declaration);
                case SyntaxKind.PropertyDeclaration:
                    return SyntaxAccessibility<PropertyDeclarationSyntax>.Instance.GetDefaultAccessibility((PropertyDeclarationSyntax)declaration);
                case SyntaxKind.IndexerDeclaration:
                    return SyntaxAccessibility<IndexerDeclarationSyntax>.Instance.GetDefaultAccessibility((IndexerDeclarationSyntax)declaration);
                case SyntaxKind.EventDeclaration:
                    return SyntaxAccessibility<EventDeclarationSyntax>.Instance.GetDefaultAccessibility((EventDeclarationSyntax)declaration);
                case SyntaxKind.EventFieldDeclaration:
                    return SyntaxAccessibility<EventFieldDeclarationSyntax>.Instance.GetDefaultAccessibility((EventFieldDeclarationSyntax)declaration);
                case SyntaxKind.FieldDeclaration:
                    return SyntaxAccessibility<FieldDeclarationSyntax>.Instance.GetDefaultAccessibility((FieldDeclarationSyntax)declaration);
                case SyntaxKind.OperatorDeclaration:
                    return SyntaxAccessibility<OperatorDeclarationSyntax>.Instance.GetDefaultAccessibility((OperatorDeclarationSyntax)declaration);
                case SyntaxKind.ConversionOperatorDeclaration:
                    return SyntaxAccessibility<ConversionOperatorDeclarationSyntax>.Instance.GetDefaultAccessibility((ConversionOperatorDeclarationSyntax)declaration);
                case SyntaxKind.ClassDeclaration:
                    return SyntaxAccessibility<ClassDeclarationSyntax>.Instance.GetDefaultAccessibility((ClassDeclarationSyntax)declaration);
                case SyntaxKind.RecordDeclaration:
                    return SyntaxAccessibility<RecordDeclarationSyntax>.Instance.GetDefaultAccessibility((RecordDeclarationSyntax)declaration);
                case SyntaxKind.StructDeclaration:
                    return SyntaxAccessibility<StructDeclarationSyntax>.Instance.GetDefaultAccessibility((StructDeclarationSyntax)declaration);
                case SyntaxKind.InterfaceDeclaration:
                    return SyntaxAccessibility<InterfaceDeclarationSyntax>.Instance.GetDefaultAccessibility((InterfaceDeclarationSyntax)declaration);
                case SyntaxKind.EnumDeclaration:
                    return SyntaxAccessibility<EnumDeclarationSyntax>.Instance.GetDefaultAccessibility((EnumDeclarationSyntax)declaration);
                case SyntaxKind.DelegateDeclaration:
                    return SyntaxAccessibility<DelegateDeclarationSyntax>.Instance.GetDefaultAccessibility((DelegateDeclarationSyntax)declaration);
                case SyntaxKind.EnumMemberDeclaration:
                    return SyntaxAccessibility<EnumMemberDeclarationSyntax>.Instance.GetDefaultAccessibility((EnumMemberDeclarationSyntax)declaration);
                case SyntaxKind.NamespaceDeclaration:
                    return SyntaxAccessibility<NamespaceDeclarationSyntax>.Instance.GetDefaultAccessibility((NamespaceDeclarationSyntax)declaration);
                case SyntaxKind.GetAccessorDeclaration:
                case SyntaxKind.SetAccessorDeclaration:
                case SyntaxKind.AddAccessorDeclaration:
                case SyntaxKind.RemoveAccessorDeclaration:
                case SyntaxKind.UnknownAccessorDeclaration:
                    return SyntaxAccessibility<AccessorDeclarationSyntax>.Instance.GetDefaultAccessibility((AccessorDeclarationSyntax)declaration);
                case SyntaxKind.IncompleteMember:
                    return SyntaxAccessibility<IncompleteMemberSyntax>.Instance.GetDefaultAccessibility((IncompleteMemberSyntax)declaration);
            }

            Debug.Fail(declaration.Kind().ToString());

            return Accessibility.NotApplicable;
        }

        /// <summary>
        /// Returns a default explicit accessibility of the specified declaration.
        /// </summary>
        /// <param name="declaration"></param>
        public static Accessibility GetDefaultExplicitAccessibility(SyntaxNode declaration)
        {
            if (declaration == null)
                throw new ArgumentNullException(nameof(declaration));

            switch (declaration.Kind())
            {
                case SyntaxKind.ConstructorDeclaration:
                    return SyntaxAccessibility<ConstructorDeclarationSyntax>.Instance.GetDefaultExplicitAccessibility((ConstructorDeclarationSyntax)declaration);
                case SyntaxKind.DestructorDeclaration:
                    return SyntaxAccessibility<DestructorDeclarationSyntax>.Instance.GetDefaultExplicitAccessibility((DestructorDeclarationSyntax)declaration);
                case SyntaxKind.MethodDeclaration:
                    return SyntaxAccessibility<MethodDeclarationSyntax>.Instance.GetDefaultExplicitAccessibility((MethodDeclarationSyntax)declaration);
                case SyntaxKind.PropertyDeclaration:
                    return SyntaxAccessibility<PropertyDeclarationSyntax>.Instance.GetDefaultExplicitAccessibility((PropertyDeclarationSyntax)declaration);
                case SyntaxKind.IndexerDeclaration:
                    return SyntaxAccessibility<IndexerDeclarationSyntax>.Instance.GetDefaultExplicitAccessibility((IndexerDeclarationSyntax)declaration);
                case SyntaxKind.EventDeclaration:
                    return SyntaxAccessibility<EventDeclarationSyntax>.Instance.GetDefaultExplicitAccessibility((EventDeclarationSyntax)declaration);
                case SyntaxKind.EventFieldDeclaration:
                    return SyntaxAccessibility<EventFieldDeclarationSyntax>.Instance.GetDefaultExplicitAccessibility((EventFieldDeclarationSyntax)declaration);
                case SyntaxKind.FieldDeclaration:
                    return SyntaxAccessibility<FieldDeclarationSyntax>.Instance.GetDefaultExplicitAccessibility((FieldDeclarationSyntax)declaration);
                case SyntaxKind.OperatorDeclaration:
                    return SyntaxAccessibility<OperatorDeclarationSyntax>.Instance.GetDefaultExplicitAccessibility((OperatorDeclarationSyntax)declaration);
                case SyntaxKind.ConversionOperatorDeclaration:
                    return SyntaxAccessibility<ConversionOperatorDeclarationSyntax>.Instance.GetDefaultExplicitAccessibility((ConversionOperatorDeclarationSyntax)declaration);
                case SyntaxKind.ClassDeclaration:
                    return SyntaxAccessibility<ClassDeclarationSyntax>.Instance.GetDefaultExplicitAccessibility((ClassDeclarationSyntax)declaration);
                case SyntaxKind.RecordDeclaration:
                    return SyntaxAccessibility<RecordDeclarationSyntax>.Instance.GetDefaultExplicitAccessibility((RecordDeclarationSyntax)declaration);
                case SyntaxKind.StructDeclaration:
                    return SyntaxAccessibility<StructDeclarationSyntax>.Instance.GetDefaultExplicitAccessibility((StructDeclarationSyntax)declaration);
                case SyntaxKind.InterfaceDeclaration:
                    return SyntaxAccessibility<InterfaceDeclarationSyntax>.Instance.GetDefaultExplicitAccessibility((InterfaceDeclarationSyntax)declaration);
                case SyntaxKind.EnumDeclaration:
                    return SyntaxAccessibility<EnumDeclarationSyntax>.Instance.GetDefaultExplicitAccessibility((EnumDeclarationSyntax)declaration);
                case SyntaxKind.DelegateDeclaration:
                    return SyntaxAccessibility<DelegateDeclarationSyntax>.Instance.GetDefaultExplicitAccessibility((DelegateDeclarationSyntax)declaration);
                case SyntaxKind.EnumMemberDeclaration:
                    return SyntaxAccessibility<EnumMemberDeclarationSyntax>.Instance.GetDefaultExplicitAccessibility((EnumMemberDeclarationSyntax)declaration);
                case SyntaxKind.NamespaceDeclaration:
                    return SyntaxAccessibility<NamespaceDeclarationSyntax>.Instance.GetDefaultExplicitAccessibility((NamespaceDeclarationSyntax)declaration);
                case SyntaxKind.GetAccessorDeclaration:
                case SyntaxKind.SetAccessorDeclaration:
                case SyntaxKind.AddAccessorDeclaration:
                case SyntaxKind.RemoveAccessorDeclaration:
                case SyntaxKind.UnknownAccessorDeclaration:
                    return SyntaxAccessibility<AccessorDeclarationSyntax>.Instance.GetDefaultExplicitAccessibility((AccessorDeclarationSyntax)declaration);
                case SyntaxKind.IncompleteMember:
                    return SyntaxAccessibility<IncompleteMemberSyntax>.Instance.GetDefaultExplicitAccessibility((IncompleteMemberSyntax)declaration);
            }

            Debug.Fail(declaration.Kind().ToString());

            return Accessibility.NotApplicable;
        }

        /// <summary>
        /// Returns an accessibility of the specified declaration.
        /// </summary>
        /// <param name="declaration"></param>
        public static Accessibility GetAccessibility(SyntaxNode declaration)
        {
            if (declaration == null)
                throw new ArgumentNullException(nameof(declaration));

            switch (declaration.Kind())
            {
                case SyntaxKind.ConstructorDeclaration:
                    return SyntaxAccessibility<ConstructorDeclarationSyntax>.Instance.GetAccessibility((ConstructorDeclarationSyntax)declaration);
                case SyntaxKind.DestructorDeclaration:
                    return SyntaxAccessibility<DestructorDeclarationSyntax>.Instance.GetAccessibility((DestructorDeclarationSyntax)declaration);
                case SyntaxKind.MethodDeclaration:
                    return SyntaxAccessibility<MethodDeclarationSyntax>.Instance.GetAccessibility((MethodDeclarationSyntax)declaration);
                case SyntaxKind.PropertyDeclaration:
                    return SyntaxAccessibility<PropertyDeclarationSyntax>.Instance.GetAccessibility((PropertyDeclarationSyntax)declaration);
                case SyntaxKind.IndexerDeclaration:
                    return SyntaxAccessibility<IndexerDeclarationSyntax>.Instance.GetAccessibility((IndexerDeclarationSyntax)declaration);
                case SyntaxKind.EventDeclaration:
                    return SyntaxAccessibility<EventDeclarationSyntax>.Instance.GetAccessibility((EventDeclarationSyntax)declaration);
                case SyntaxKind.EventFieldDeclaration:
                    return SyntaxAccessibility<EventFieldDeclarationSyntax>.Instance.GetAccessibility((EventFieldDeclarationSyntax)declaration);
                case SyntaxKind.FieldDeclaration:
                    return SyntaxAccessibility<FieldDeclarationSyntax>.Instance.GetAccessibility((FieldDeclarationSyntax)declaration);
                case SyntaxKind.OperatorDeclaration:
                    return SyntaxAccessibility<OperatorDeclarationSyntax>.Instance.GetAccessibility((OperatorDeclarationSyntax)declaration);
                case SyntaxKind.ConversionOperatorDeclaration:
                    return SyntaxAccessibility<ConversionOperatorDeclarationSyntax>.Instance.GetAccessibility((ConversionOperatorDeclarationSyntax)declaration);
                case SyntaxKind.ClassDeclaration:
                    return SyntaxAccessibility<ClassDeclarationSyntax>.Instance.GetAccessibility((ClassDeclarationSyntax)declaration);
                case SyntaxKind.RecordDeclaration:
                    return SyntaxAccessibility<RecordDeclarationSyntax>.Instance.GetAccessibility((RecordDeclarationSyntax)declaration);
                case SyntaxKind.StructDeclaration:
                    return SyntaxAccessibility<StructDeclarationSyntax>.Instance.GetAccessibility((StructDeclarationSyntax)declaration);
                case SyntaxKind.InterfaceDeclaration:
                    return SyntaxAccessibility<InterfaceDeclarationSyntax>.Instance.GetAccessibility((InterfaceDeclarationSyntax)declaration);
                case SyntaxKind.EnumDeclaration:
                    return SyntaxAccessibility<EnumDeclarationSyntax>.Instance.GetAccessibility((EnumDeclarationSyntax)declaration);
                case SyntaxKind.DelegateDeclaration:
                    return SyntaxAccessibility<DelegateDeclarationSyntax>.Instance.GetAccessibility((DelegateDeclarationSyntax)declaration);
                case SyntaxKind.EnumMemberDeclaration:
                    return SyntaxAccessibility<EnumMemberDeclarationSyntax>.Instance.GetAccessibility((EnumMemberDeclarationSyntax)declaration);
                case SyntaxKind.NamespaceDeclaration:
                    return SyntaxAccessibility<NamespaceDeclarationSyntax>.Instance.GetAccessibility((NamespaceDeclarationSyntax)declaration);
                case SyntaxKind.GetAccessorDeclaration:
                case SyntaxKind.SetAccessorDeclaration:
                case SyntaxKind.AddAccessorDeclaration:
                case SyntaxKind.RemoveAccessorDeclaration:
                case SyntaxKind.UnknownAccessorDeclaration:
                    return SyntaxAccessibility<AccessorDeclarationSyntax>.Instance.GetAccessibility((AccessorDeclarationSyntax)declaration);
                case SyntaxKind.IncompleteMember:
                    return SyntaxAccessibility<IncompleteMemberSyntax>.Instance.GetAccessibility((IncompleteMemberSyntax)declaration);
            }

            Debug.Fail(declaration.Kind().ToString());

            return Accessibility.NotApplicable;
        }

        /// <summary>
        /// Returns an explicit accessibility of the specified declaration.
        /// </summary>
        /// <param name="declaration"></param>
        public static Accessibility GetExplicitAccessibility(SyntaxNode declaration)
        {
            if (declaration == null)
                throw new ArgumentNullException(nameof(declaration));

            switch (declaration.Kind())
            {
                case SyntaxKind.ConstructorDeclaration:
                    return SyntaxAccessibility<ConstructorDeclarationSyntax>.Instance.GetExplicitAccessibility((ConstructorDeclarationSyntax)declaration);
                case SyntaxKind.DestructorDeclaration:
                    return SyntaxAccessibility<DestructorDeclarationSyntax>.Instance.GetExplicitAccessibility((DestructorDeclarationSyntax)declaration);
                case SyntaxKind.MethodDeclaration:
                    return SyntaxAccessibility<MethodDeclarationSyntax>.Instance.GetExplicitAccessibility((MethodDeclarationSyntax)declaration);
                case SyntaxKind.PropertyDeclaration:
                    return SyntaxAccessibility<PropertyDeclarationSyntax>.Instance.GetExplicitAccessibility((PropertyDeclarationSyntax)declaration);
                case SyntaxKind.IndexerDeclaration:
                    return SyntaxAccessibility<IndexerDeclarationSyntax>.Instance.GetExplicitAccessibility((IndexerDeclarationSyntax)declaration);
                case SyntaxKind.EventDeclaration:
                    return SyntaxAccessibility<EventDeclarationSyntax>.Instance.GetExplicitAccessibility((EventDeclarationSyntax)declaration);
                case SyntaxKind.EventFieldDeclaration:
                    return SyntaxAccessibility<EventFieldDeclarationSyntax>.Instance.GetExplicitAccessibility((EventFieldDeclarationSyntax)declaration);
                case SyntaxKind.FieldDeclaration:
                    return SyntaxAccessibility<FieldDeclarationSyntax>.Instance.GetExplicitAccessibility((FieldDeclarationSyntax)declaration);
                case SyntaxKind.OperatorDeclaration:
                    return SyntaxAccessibility<OperatorDeclarationSyntax>.Instance.GetExplicitAccessibility((OperatorDeclarationSyntax)declaration);
                case SyntaxKind.ConversionOperatorDeclaration:
                    return SyntaxAccessibility<ConversionOperatorDeclarationSyntax>.Instance.GetExplicitAccessibility((ConversionOperatorDeclarationSyntax)declaration);
                case SyntaxKind.ClassDeclaration:
                    return SyntaxAccessibility<ClassDeclarationSyntax>.Instance.GetExplicitAccessibility((ClassDeclarationSyntax)declaration);
                case SyntaxKind.RecordDeclaration:
                    return SyntaxAccessibility<RecordDeclarationSyntax>.Instance.GetExplicitAccessibility((RecordDeclarationSyntax)declaration);
                case SyntaxKind.StructDeclaration:
                    return SyntaxAccessibility<StructDeclarationSyntax>.Instance.GetExplicitAccessibility((StructDeclarationSyntax)declaration);
                case SyntaxKind.InterfaceDeclaration:
                    return SyntaxAccessibility<InterfaceDeclarationSyntax>.Instance.GetExplicitAccessibility((InterfaceDeclarationSyntax)declaration);
                case SyntaxKind.EnumDeclaration:
                    return SyntaxAccessibility<EnumDeclarationSyntax>.Instance.GetExplicitAccessibility((EnumDeclarationSyntax)declaration);
                case SyntaxKind.DelegateDeclaration:
                    return SyntaxAccessibility<DelegateDeclarationSyntax>.Instance.GetExplicitAccessibility((DelegateDeclarationSyntax)declaration);
                case SyntaxKind.EnumMemberDeclaration:
                    return SyntaxAccessibility<EnumMemberDeclarationSyntax>.Instance.GetExplicitAccessibility((EnumMemberDeclarationSyntax)declaration);
                case SyntaxKind.NamespaceDeclaration:
                    return SyntaxAccessibility<NamespaceDeclarationSyntax>.Instance.GetExplicitAccessibility((NamespaceDeclarationSyntax)declaration);
                case SyntaxKind.GetAccessorDeclaration:
                case SyntaxKind.SetAccessorDeclaration:
                case SyntaxKind.AddAccessorDeclaration:
                case SyntaxKind.RemoveAccessorDeclaration:
                case SyntaxKind.UnknownAccessorDeclaration:
                    return SyntaxAccessibility<AccessorDeclarationSyntax>.Instance.GetExplicitAccessibility((AccessorDeclarationSyntax)declaration);
                case SyntaxKind.IncompleteMember:
                    return SyntaxAccessibility<IncompleteMemberSyntax>.Instance.GetExplicitAccessibility((IncompleteMemberSyntax)declaration);
            }

            Debug.Fail(declaration.Kind().ToString());

            return Accessibility.NotApplicable;
        }

        /// <summary>
        /// Returns an explicit accessibility of the specified modifiers.
        /// </summary>
        /// <param name="modifiers"></param>
        public static Accessibility GetExplicitAccessibility(SyntaxTokenList modifiers)
        {
            int count = modifiers.Count;

            for (int i = 0; i < count; i++)
            {
                switch (modifiers[i].Kind())
                {
                    case SyntaxKind.PublicKeyword:
                        {
                            return Accessibility.Public;
                        }
                    case SyntaxKind.PrivateKeyword:
                        {
                            for (int j = i + 1; j < count; j++)
                            {
                                if (modifiers[j].Kind() == SyntaxKind.ProtectedKeyword)
                                    return Accessibility.ProtectedAndInternal;
                            }

                            return Accessibility.Private;
                        }
                    case SyntaxKind.InternalKeyword:
                        {
                            for (int j = i + 1; j < count; j++)
                            {
                                if (modifiers[j].Kind() == SyntaxKind.ProtectedKeyword)
                                    return Accessibility.ProtectedOrInternal;
                            }

                            return Accessibility.Internal;
                        }
                    case SyntaxKind.ProtectedKeyword:
                        {
                            for (int j = i + 1; j < count; j++)
                            {
                                switch (modifiers[j].Kind())
                                {
                                    case SyntaxKind.InternalKeyword:
                                        return Accessibility.ProtectedOrInternal;
                                    case SyntaxKind.PrivateKeyword:
                                        return Accessibility.ProtectedAndInternal;
                                }
                            }

                            return Accessibility.Protected;
                        }
                }
            }

            return Accessibility.NotApplicable;
        }

        /// <summary>
        /// Return true if the specified declaration is publicly visible.
        /// </summary>
        /// <param name="declaration"></param>
        public static bool IsPubliclyVisible(MemberDeclarationSyntax declaration)
        {
            if (declaration == null)
                throw new ArgumentNullException(nameof(declaration));

            do
            {
                if (declaration.IsKind(SyntaxKind.NamespaceDeclaration))
                    return true;

                if (!GetAccessibility(declaration).Is(
                    Accessibility.Public,
                    Accessibility.Protected,
                    Accessibility.ProtectedOrInternal))
                {
                    return false;
                }

                SyntaxNode parent = declaration.Parent;

                if (parent == null)
                    return true;

                if (parent is ICompilationUnitSyntax)
                    return true;

                Debug.Assert(parent is MemberDeclarationSyntax, parent.Kind().ToString());

                declaration = parent as MemberDeclarationSyntax;

            } while (declaration != null);

            return false;
        }

        /// <summary>
        /// Creates a new node with the explicit accessibility removed.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        public static TNode WithoutExplicitAccessibility<TNode>(TNode node) where TNode : SyntaxNode
        {
            return WithExplicitAccessibility(node, Accessibility.NotApplicable);
        }

        /// <summary>
        /// Creates a new node with the specified explicit accessibility updated.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        /// <param name="newAccessibility"></param>
        /// <param name="comparer"></param>
        public static TNode WithExplicitAccessibility<TNode>(
            TNode node,
            Accessibility newAccessibility,
            IComparer<SyntaxKind> comparer = null) where TNode : SyntaxNode
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            ModifierListInfo info = SyntaxInfo.ModifierListInfo(node);

            if (!info.Success)
                throw new ArgumentException($"'{node.Kind()}' cannot have modifiers.", nameof(node));

            ModifierListInfo newInfo = info.WithExplicitAccessibility(newAccessibility, comparer);

            return (TNode)newInfo.Parent;
        }

        /// <summary>
        /// Returns true if the node can have specified accessibility.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="accessibility"></param>
        /// <param name="ignoreOverride">Ignore "override" modifier.</param>
        public static bool IsValidAccessibility(SyntaxNode node, Accessibility accessibility, bool ignoreOverride = false)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            switch (node.Parent?.Kind())
            {
                case SyntaxKind.NamespaceDeclaration:
                case SyntaxKind.CompilationUnit:
                    {
                        return accessibility.Is(Accessibility.Public, Accessibility.Internal);
                    }
                case SyntaxKind.StructDeclaration:
                    {
                        if (accessibility.ContainsProtected())
                            return false;

                        break;
                    }
            }

            switch (node.Kind())
            {
                case SyntaxKind.ClassDeclaration:
                case SyntaxKind.InterfaceDeclaration:
                case SyntaxKind.RecordDeclaration:
                case SyntaxKind.StructDeclaration:
                case SyntaxKind.EnumDeclaration:
                    {
                        return true;
                    }
                case SyntaxKind.EventDeclaration:
                    {
                        var eventDeclaration = (EventDeclarationSyntax)node;

                        ModifierFilter filter = SyntaxInfo.ModifierListInfo(eventDeclaration).GetFilter();

                        return (ignoreOverride || !filter.HasAnyFlag(ModifierFilter.Override))
                            && (accessibility != Accessibility.Private || !filter.HasAnyFlag(ModifierFilter.AbstractVirtualOverride))
                            && CheckProtectedInStaticOrSealedClass(node)
                            && CheckAccessorAccessibility(eventDeclaration.AccessorList);
                    }
                case SyntaxKind.IndexerDeclaration:
                    {
                        var indexerDeclaration = (IndexerDeclarationSyntax)node;

                        ModifierFilter filter = SyntaxInfo.ModifierListInfo(indexerDeclaration).GetFilter();

                        return (ignoreOverride || !filter.HasAnyFlag(ModifierFilter.Override))
                            && (accessibility != Accessibility.Private || !filter.HasAnyFlag(ModifierFilter.AbstractVirtualOverride))
                            && CheckProtectedInStaticOrSealedClass(node)
                            && CheckAccessorAccessibility(indexerDeclaration.AccessorList);
                    }
                case SyntaxKind.PropertyDeclaration:
                    {
                        var propertyDeclaration = (PropertyDeclarationSyntax)node;

                        ModifierFilter filter = SyntaxInfo.ModifierListInfo(propertyDeclaration).GetFilter();

                        return (ignoreOverride || !filter.HasAnyFlag(ModifierFilter.Override))
                            && (accessibility != Accessibility.Private || !filter.HasAnyFlag(ModifierFilter.AbstractVirtualOverride))
                            && CheckProtectedInStaticOrSealedClass(node)
                            && CheckAccessorAccessibility(propertyDeclaration.AccessorList);
                    }
                case SyntaxKind.MethodDeclaration:
                    {
                        var methodDeclaration = (MethodDeclarationSyntax)node;

                        ModifierFilter filter = SyntaxInfo.ModifierListInfo(methodDeclaration).GetFilter();

                        return (ignoreOverride || !filter.HasAnyFlag(ModifierFilter.Override))
                            && (accessibility != Accessibility.Private || !filter.HasAnyFlag(ModifierFilter.AbstractVirtualOverride))
                            && CheckProtectedInStaticOrSealedClass(node);
                    }
                case SyntaxKind.EventFieldDeclaration:
                    {
                        var eventFieldDeclaration = (EventFieldDeclarationSyntax)node;

                        ModifierFilter filter = SyntaxInfo.ModifierListInfo(eventFieldDeclaration).GetFilter();

                        return (ignoreOverride || !filter.HasAnyFlag(ModifierFilter.Override))
                            && (accessibility != Accessibility.Private || !filter.HasAnyFlag(ModifierFilter.AbstractVirtualOverride))
                            && CheckProtectedInStaticOrSealedClass(node);
                    }
                case SyntaxKind.ConstructorDeclaration:
                case SyntaxKind.DelegateDeclaration:
                case SyntaxKind.FieldDeclaration:
                case SyntaxKind.IncompleteMember:
                    {
                        return CheckProtectedInStaticOrSealedClass(node);
                    }
                case SyntaxKind.OperatorDeclaration:
                case SyntaxKind.ConversionOperatorDeclaration:
                    {
                        return accessibility == Accessibility.Public;
                    }
                case SyntaxKind.GetAccessorDeclaration:
                case SyntaxKind.SetAccessorDeclaration:
                case SyntaxKind.AddAccessorDeclaration:
                case SyntaxKind.RemoveAccessorDeclaration:
                case SyntaxKind.UnknownAccessorDeclaration:
                    {
                        var memberDeclaration = node.Parent?.Parent as MemberDeclarationSyntax;

                        Debug.Assert(memberDeclaration != null, node.ToString());

                        if (memberDeclaration != null)
                        {
                            if (!CheckProtectedInStaticOrSealedClass(memberDeclaration))
                                return false;

                            return accessibility.IsMoreRestrictiveThan(GetAccessibility(memberDeclaration));
                        }

                        return false;
                    }
                case SyntaxKind.LocalFunctionStatement:
                    {
                        return false;
                    }
                default:
                    {
                        Debug.Fail(node.Kind().ToString());
                        return false;
                    }
            }

            bool CheckProtectedInStaticOrSealedClass(SyntaxNode declaration)
            {
                return !accessibility.ContainsProtected()
                    || (declaration.Parent as ClassDeclarationSyntax)?
                        .Modifiers
                        .ContainsAny(SyntaxKind.StaticKeyword, SyntaxKind.SealedKeyword) != true;
            }

            bool CheckAccessorAccessibility(AccessorListSyntax accessorList)
            {
                if (accessorList != null)
                {
                    foreach (AccessorDeclarationSyntax accessor in accessorList.Accessors)
                    {
                        Accessibility accessorAccessibility = GetExplicitAccessibility(accessor.Modifiers);

                        if (accessorAccessibility != Accessibility.NotApplicable)
                            return accessorAccessibility.IsMoreRestrictiveThan(accessibility);
                    }
                }

                return true;
            }
        }
    }
}
