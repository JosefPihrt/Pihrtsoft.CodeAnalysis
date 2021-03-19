// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp
{
    /// <summary>
    /// Defines a set of methods related to C# accessibility.
    /// </summary>
    internal abstract class SyntaxAccessibility<TNode> where TNode : SyntaxNode
    {
        public static SyntaxAccessibility<TNode> Instance { get; } = (SyntaxAccessibility<TNode>)GetInstance();

        private static object GetInstance()
        {
            if (typeof(TNode) == typeof(AccessorDeclarationSyntax))
                return new AccessorAccessibility();

            if (typeof(TNode) == typeof(ClassDeclarationSyntax))
                return new ClassAccessibility();

            if (typeof(TNode) == typeof(ConstructorDeclarationSyntax))
                return new ConstructorAccessibility();

            if (typeof(TNode) == typeof(ConversionOperatorDeclarationSyntax))
                return new ConversionOperatorAccessibility();

            if (typeof(TNode) == typeof(DelegateDeclarationSyntax))
                return new DelegateAccessibility();

            if (typeof(TNode) == typeof(DestructorDeclarationSyntax))
                return new DestructorAccessibility();

            if (typeof(TNode) == typeof(EnumDeclarationSyntax))
                return new EnumAccessibility();

            if (typeof(TNode) == typeof(EnumMemberDeclarationSyntax))
                return new EnumMemberAccessibility();

            if (typeof(TNode) == typeof(EventDeclarationSyntax))
                return new EventAccessibility();

            if (typeof(TNode) == typeof(EventFieldDeclarationSyntax))
                return new EventFieldAccessibility();

            if (typeof(TNode) == typeof(FieldDeclarationSyntax))
                return new FieldAccessibility();

            if (typeof(TNode) == typeof(IndexerDeclarationSyntax))
                return new IndexerAccessibility();

            if (typeof(TNode) == typeof(InterfaceDeclarationSyntax))
                return new InterfaceAccessibility();

            if (typeof(TNode) == typeof(MethodDeclarationSyntax))
                return new MethodAccessibility();

            if (typeof(TNode) == typeof(NamespaceDeclarationSyntax))
                return new NamespaceAccessibility();

            if (typeof(TNode) == typeof(OperatorDeclarationSyntax))
                return new OperatorAccessibility();

            if (typeof(TNode) == typeof(PropertyDeclarationSyntax))
                return new PropertyAccessibility();

            if (typeof(TNode) == typeof(StructDeclarationSyntax))
                return new StructAccessibility();

            if (typeof(TNode) == typeof(RecordDeclarationSyntax))
                return new RecordAccessibility();

            if (typeof(TNode) == typeof(IncompleteMemberSyntax))
                return new IncompleteMemberAccessibility();

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Returns an accessibility of the specified declaration.
        /// </summary>
        /// <param name="declaration"></param>
        public abstract Accessibility GetAccessibility(TNode declaration);

        /// <summary>
        /// Returns a default accessibility of the specified declaration.
        /// </summary>
        /// <param name="declaration"></param>
        public abstract Accessibility GetDefaultAccessibility(TNode declaration);

        /// <summary>
        /// Returns an explicit accessibility of the specified declaration.
        /// </summary>
        /// <param name="declaration"></param>
        public abstract Accessibility GetExplicitAccessibility(TNode declaration);

        /// <summary>
        /// Returns a default explicit accessibility of the specified declaration.
        /// </summary>
        /// <param name="declaration"></param>
        public abstract Accessibility GetDefaultExplicitAccessibility(TNode declaration);

        private class AccessorAccessibility : SyntaxAccessibility<AccessorDeclarationSyntax>
        {
            public override Accessibility GetDefaultAccessibility(AccessorDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                SyntaxNode containingDeclaration = declaration.Parent?.Parent;

                switch (containingDeclaration?.Kind())
                {
                    case SyntaxKind.PropertyDeclaration:
                        return SyntaxAccessibility<PropertyDeclarationSyntax>.Instance.GetDefaultAccessibility((PropertyDeclarationSyntax)containingDeclaration);
                    case SyntaxKind.IndexerDeclaration:
                        return SyntaxAccessibility<IndexerDeclarationSyntax>.Instance.GetDefaultAccessibility((IndexerDeclarationSyntax)containingDeclaration);
                    case SyntaxKind.EventDeclaration:
                        return SyntaxAccessibility<EventDeclarationSyntax>.Instance.GetDefaultAccessibility((EventDeclarationSyntax)containingDeclaration);
                }

                Debug.Assert(containingDeclaration == null, containingDeclaration.Kind().ToString());

                return Accessibility.NotApplicable;
            }

            public override Accessibility GetAccessibility(AccessorDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                Accessibility accessibility = SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);

                SyntaxNode containingDeclaration = declaration.Parent?.Parent;

                if (containingDeclaration == null)
                    return accessibility;

                Accessibility containingAccessibility = GetAccessibility();

                if (containingAccessibility == Accessibility.NotApplicable)
                    return accessibility;

                return (accessibility.IsMoreRestrictiveThan(containingAccessibility))
                    ? accessibility
                    : containingAccessibility;

                Accessibility GetAccessibility()
                {
                    switch (containingDeclaration.Kind())
                    {
                        case SyntaxKind.PropertyDeclaration:
                            return SyntaxAccessibility<PropertyDeclarationSyntax>.Instance.GetAccessibility((PropertyDeclarationSyntax)containingDeclaration);
                        case SyntaxKind.IndexerDeclaration:
                            return SyntaxAccessibility<IndexerDeclarationSyntax>.Instance.GetAccessibility((IndexerDeclarationSyntax)containingDeclaration);
                        case SyntaxKind.EventDeclaration:
                            return SyntaxAccessibility<EventDeclarationSyntax>.Instance.GetAccessibility((EventDeclarationSyntax)containingDeclaration);
                    }

                    Debug.Fail(containingDeclaration.Kind().ToString());

                    return Accessibility.NotApplicable;
                }
            }

            public override Accessibility GetExplicitAccessibility(AccessorDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);
            }

            public override Accessibility GetDefaultExplicitAccessibility(AccessorDeclarationSyntax declaration)
            {
                return Accessibility.NotApplicable;
            }
        }

        private static class BaseTypeAccessibility
        {
            public static Accessibility GetDefaultAccessibility(BaseTypeDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return (declaration.IsParentKind(SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration))
                    ? Accessibility.Private
                    : Accessibility.Internal;
            }

            public static Accessibility GetDefaultExplicitAccessibility(BaseTypeDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return (declaration.IsParentKind(SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration))
                    ? Accessibility.Private
                    : Accessibility.Internal;
            }
        }

        private class ClassAccessibility : SyntaxAccessibility<ClassDeclarationSyntax>
        {
            public override Accessibility GetDefaultAccessibility(ClassDeclarationSyntax declaration)
            {
                return BaseTypeAccessibility.GetDefaultAccessibility(declaration);
            }

            public override Accessibility GetDefaultExplicitAccessibility(ClassDeclarationSyntax declaration)
            {
                return BaseTypeAccessibility.GetDefaultExplicitAccessibility(declaration);
            }

            public override Accessibility GetAccessibility(ClassDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                Accessibility accessibility = SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);

                return (accessibility != Accessibility.NotApplicable)
                    ? accessibility
                    : GetDefaultAccessibility(declaration);
            }

            public override Accessibility GetExplicitAccessibility(ClassDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);
            }
        }

        private class ConstructorAccessibility : SyntaxAccessibility<ConstructorDeclarationSyntax>
        {
            public override Accessibility GetDefaultAccessibility(ConstructorDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return (declaration.Modifiers.Contains(SyntaxKind.StaticKeyword))
                    ? Accessibility.Public
                    : Accessibility.Private;
            }

            public override Accessibility GetDefaultExplicitAccessibility(ConstructorDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return (declaration.Modifiers.Contains(SyntaxKind.StaticKeyword))
                    ? Accessibility.NotApplicable
                    : Accessibility.Private;
            }

            public override Accessibility GetAccessibility(ConstructorDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                SyntaxTokenList modifiers = declaration.Modifiers;

                if (modifiers.Contains(SyntaxKind.StaticKeyword))
                    return Accessibility.Private;

                Accessibility accessibility = SyntaxAccessibility.GetExplicitAccessibility(modifiers);

                return (accessibility != Accessibility.NotApplicable)
                    ? accessibility
                    : GetDefaultAccessibility(declaration);
            }

            public override Accessibility GetExplicitAccessibility(ConstructorDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);
            }
        }

        private class ConversionOperatorAccessibility : SyntaxAccessibility<ConversionOperatorDeclarationSyntax>
        {
            public override Accessibility GetDefaultAccessibility(ConversionOperatorDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return Accessibility.Public;
            }

            public override Accessibility GetDefaultExplicitAccessibility(ConversionOperatorDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return Accessibility.Public;
            }

            public override Accessibility GetAccessibility(ConversionOperatorDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return Accessibility.Public;
            }

            public override Accessibility GetExplicitAccessibility(ConversionOperatorDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);
            }
        }

        private class DelegateAccessibility : SyntaxAccessibility<DelegateDeclarationSyntax>
        {
            public override Accessibility GetDefaultAccessibility(DelegateDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return (declaration.IsParentKind(SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration))
                    ? Accessibility.Private
                    : Accessibility.Internal;
            }

            public override Accessibility GetDefaultExplicitAccessibility(DelegateDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return (declaration.IsParentKind(SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration))
                    ? Accessibility.Private
                    : Accessibility.Internal;
            }

            public override Accessibility GetAccessibility(DelegateDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                Accessibility accessibility = SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);

                return (accessibility != Accessibility.NotApplicable)
                    ? accessibility
                    : GetDefaultAccessibility(declaration);
            }

            public override Accessibility GetExplicitAccessibility(DelegateDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);
            }
        }

        private class DestructorAccessibility : SyntaxAccessibility<DestructorDeclarationSyntax>
        {
            public override Accessibility GetDefaultAccessibility(DestructorDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return Accessibility.Public;
            }

            public override Accessibility GetDefaultExplicitAccessibility(DestructorDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return Accessibility.NotApplicable;
            }

            public override Accessibility GetAccessibility(DestructorDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return Accessibility.Public;
            }

            public override Accessibility GetExplicitAccessibility(DestructorDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);
            }
        }

        private class EnumAccessibility : SyntaxAccessibility<EnumDeclarationSyntax>
        {
            public override Accessibility GetDefaultAccessibility(EnumDeclarationSyntax declaration)
            {
                return BaseTypeAccessibility.GetDefaultAccessibility(declaration);
            }

            public override Accessibility GetDefaultExplicitAccessibility(EnumDeclarationSyntax declaration)
            {
                return BaseTypeAccessibility.GetDefaultExplicitAccessibility(declaration);
            }

            public override Accessibility GetAccessibility(EnumDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                Accessibility accessibility = SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);

                return (accessibility != Accessibility.NotApplicable)
                    ? accessibility
                    : GetDefaultAccessibility(declaration);
            }

            public override Accessibility GetExplicitAccessibility(EnumDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);
            }
        }

        private class EnumMemberAccessibility : SyntaxAccessibility<EnumMemberDeclarationSyntax>
        {
            public override Accessibility GetDefaultAccessibility(EnumMemberDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return Accessibility.Public;
            }

            public override Accessibility GetDefaultExplicitAccessibility(EnumMemberDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return Accessibility.NotApplicable;
            }

            public override Accessibility GetAccessibility(EnumMemberDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return Accessibility.Public;
            }

            public override Accessibility GetExplicitAccessibility(EnumMemberDeclarationSyntax declaration)
            {
                return Accessibility.NotApplicable;
            }
        }

        private class EventAccessibility : SyntaxAccessibility<EventDeclarationSyntax>
        {
            public override Accessibility GetDefaultAccessibility(EventDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return Accessibility.Private;
            }

            public override Accessibility GetDefaultExplicitAccessibility(EventDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return (declaration.ExplicitInterfaceSpecifier != null)
                    ? Accessibility.NotApplicable
                    : Accessibility.Private;
            }

            public override Accessibility GetAccessibility(EventDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                if (declaration.ExplicitInterfaceSpecifier != null)
                    return Accessibility.Private;

                Accessibility accessibility = SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);

                return (accessibility != Accessibility.NotApplicable)
                    ? accessibility
                    : GetDefaultAccessibility(declaration);
            }

            public override Accessibility GetExplicitAccessibility(EventDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);
            }
        }

        private class EventFieldAccessibility : SyntaxAccessibility<EventFieldDeclarationSyntax>
        {
            public override Accessibility GetDefaultAccessibility(EventFieldDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return (declaration.IsParentKind(SyntaxKind.InterfaceDeclaration))
                    ? Accessibility.Public
                    : Accessibility.Private;
            }

            public override Accessibility GetDefaultExplicitAccessibility(EventFieldDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return (declaration.IsParentKind(SyntaxKind.InterfaceDeclaration))
                    ? Accessibility.NotApplicable
                    : Accessibility.Private;
            }

            public override Accessibility GetAccessibility(EventFieldDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                if (declaration.IsParentKind(SyntaxKind.InterfaceDeclaration))
                    return Accessibility.Public;

                Accessibility accessibility = SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);

                return (accessibility != Accessibility.NotApplicable)
                    ? accessibility
                    : GetDefaultAccessibility(declaration);
            }

            public override Accessibility GetExplicitAccessibility(EventFieldDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);
            }
        }

        private class FieldAccessibility : SyntaxAccessibility<FieldDeclarationSyntax>
        {
            public override Accessibility GetDefaultAccessibility(FieldDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return Accessibility.Private;
            }

            public override Accessibility GetDefaultExplicitAccessibility(FieldDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return Accessibility.Private;
            }

            public override Accessibility GetAccessibility(FieldDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                Accessibility accessibility = SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);

                return (accessibility != Accessibility.NotApplicable)
                    ? accessibility
                    : GetDefaultAccessibility(declaration);
            }

            public override Accessibility GetExplicitAccessibility(FieldDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);
            }
        }

        private class IndexerAccessibility : SyntaxAccessibility<IndexerDeclarationSyntax>
        {
            public override Accessibility GetDefaultAccessibility(IndexerDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return (declaration.IsParentKind(SyntaxKind.InterfaceDeclaration))
                    ? Accessibility.Public
                    : Accessibility.Private;
            }

            public override Accessibility GetDefaultExplicitAccessibility(IndexerDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                if (declaration.ExplicitInterfaceSpecifier != null
                    || declaration.IsParentKind(SyntaxKind.InterfaceDeclaration))
                {
                    return Accessibility.NotApplicable;
                }
                else
                {
                    return Accessibility.Private;
                }
            }

            public override Accessibility GetAccessibility(IndexerDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                if (declaration.ExplicitInterfaceSpecifier != null)
                    return Accessibility.Private;

                if (declaration.IsParentKind(SyntaxKind.InterfaceDeclaration))
                    return Accessibility.Public;

                Accessibility accessibility = SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);

                return (accessibility != Accessibility.NotApplicable)
                    ? accessibility
                    : GetDefaultAccessibility(declaration);
            }

            public override Accessibility GetExplicitAccessibility(IndexerDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);
            }
        }

        private class InterfaceAccessibility : SyntaxAccessibility<InterfaceDeclarationSyntax>
        {
            public override Accessibility GetDefaultAccessibility(InterfaceDeclarationSyntax declaration)
            {
                return BaseTypeAccessibility.GetDefaultAccessibility(declaration);
            }

            public override Accessibility GetDefaultExplicitAccessibility(InterfaceDeclarationSyntax declaration)
            {
                return BaseTypeAccessibility.GetDefaultExplicitAccessibility(declaration);
            }

            public override Accessibility GetAccessibility(InterfaceDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                Accessibility accessibility = SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);

                return (accessibility != Accessibility.NotApplicable)
                    ? accessibility
                    : GetDefaultAccessibility(declaration);
            }

            public override Accessibility GetExplicitAccessibility(InterfaceDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);
            }
        }

        private class MethodAccessibility : SyntaxAccessibility<MethodDeclarationSyntax>
        {
            public override Accessibility GetDefaultAccessibility(MethodDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return (declaration.IsParentKind(SyntaxKind.InterfaceDeclaration))
                    ? Accessibility.Public
                    : Accessibility.Private;
            }

            public override Accessibility GetDefaultExplicitAccessibility(MethodDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                if (declaration.Modifiers.Contains(SyntaxKind.PartialKeyword)
                    || declaration.ExplicitInterfaceSpecifier != null
                    || declaration.IsParentKind(SyntaxKind.InterfaceDeclaration))
                {
                    return Accessibility.NotApplicable;
                }
                else
                {
                    return Accessibility.Private;
                }
            }

            public override Accessibility GetAccessibility(MethodDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                SyntaxTokenList modifiers = declaration.Modifiers;

                if (modifiers.Contains(SyntaxKind.PartialKeyword))
                    return Accessibility.Private;

                if (declaration.ExplicitInterfaceSpecifier != null)
                    return Accessibility.Private;

                if (declaration.IsParentKind(SyntaxKind.InterfaceDeclaration))
                    return Accessibility.Public;

                Accessibility accessibility = SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);

                return (accessibility != Accessibility.NotApplicable)
                    ? accessibility
                    : GetDefaultAccessibility(declaration);
            }

            public override Accessibility GetExplicitAccessibility(MethodDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);
            }
        }

        private class NamespaceAccessibility : SyntaxAccessibility<NamespaceDeclarationSyntax>
        {
            public override Accessibility GetDefaultAccessibility(NamespaceDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return Accessibility.Public;
            }

            public override Accessibility GetDefaultExplicitAccessibility(NamespaceDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return Accessibility.NotApplicable;
            }

            public override Accessibility GetAccessibility(NamespaceDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return Accessibility.Public;
            }

            public override Accessibility GetExplicitAccessibility(NamespaceDeclarationSyntax declaration)
            {
                return Accessibility.NotApplicable;
            }
        }

        private class OperatorAccessibility : SyntaxAccessibility<OperatorDeclarationSyntax>
        {
            public override Accessibility GetDefaultAccessibility(OperatorDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return Accessibility.Public;
            }

            public override Accessibility GetDefaultExplicitAccessibility(OperatorDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return Accessibility.Public;
            }

            public override Accessibility GetAccessibility(OperatorDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return Accessibility.Public;
            }

            public override Accessibility GetExplicitAccessibility(OperatorDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);
            }
        }

        private class PropertyAccessibility : SyntaxAccessibility<PropertyDeclarationSyntax>
        {
            public override Accessibility GetDefaultAccessibility(PropertyDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return (declaration.IsParentKind(SyntaxKind.InterfaceDeclaration))
                    ? Accessibility.Public
                    : Accessibility.Private;
            }

            public override Accessibility GetDefaultExplicitAccessibility(PropertyDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                if (declaration.ExplicitInterfaceSpecifier != null
                    || declaration.IsParentKind(SyntaxKind.InterfaceDeclaration))
                {
                    return Accessibility.NotApplicable;
                }
                else
                {
                    return Accessibility.Private;
                }
            }

            public override Accessibility GetAccessibility(PropertyDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                if (declaration.ExplicitInterfaceSpecifier != null)
                    return Accessibility.Private;

                if (declaration.IsParentKind(SyntaxKind.InterfaceDeclaration))
                    return Accessibility.Public;

                Accessibility accessibility = SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);

                return (accessibility != Accessibility.NotApplicable)
                    ? accessibility
                    : GetDefaultAccessibility(declaration);
            }

            public override Accessibility GetExplicitAccessibility(PropertyDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);
            }
        }

        private class StructAccessibility : SyntaxAccessibility<StructDeclarationSyntax>
        {
            public override Accessibility GetDefaultAccessibility(StructDeclarationSyntax declaration)
            {
                return BaseTypeAccessibility.GetDefaultAccessibility(declaration);
            }

            public override Accessibility GetDefaultExplicitAccessibility(StructDeclarationSyntax declaration)
            {
                return BaseTypeAccessibility.GetDefaultExplicitAccessibility(declaration);
            }

            public override Accessibility GetAccessibility(StructDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                Accessibility accessibility = SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);

                return (accessibility != Accessibility.NotApplicable)
                    ? accessibility
                    : GetDefaultAccessibility(declaration);
            }

            public override Accessibility GetExplicitAccessibility(StructDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);
            }
        }

        private class RecordAccessibility : SyntaxAccessibility<RecordDeclarationSyntax>
        {
            public override Accessibility GetDefaultAccessibility(RecordDeclarationSyntax declaration)
            {
                return BaseTypeAccessibility.GetDefaultAccessibility(declaration);
            }

            public override Accessibility GetDefaultExplicitAccessibility(RecordDeclarationSyntax declaration)
            {
                return BaseTypeAccessibility.GetDefaultExplicitAccessibility(declaration);
            }

            public override Accessibility GetAccessibility(RecordDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                Accessibility accessibility = SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);

                return (accessibility != Accessibility.NotApplicable)
                    ? accessibility
                    : GetDefaultAccessibility(declaration);
            }

            public override Accessibility GetExplicitAccessibility(RecordDeclarationSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);
            }
        }

        private class IncompleteMemberAccessibility : SyntaxAccessibility<IncompleteMemberSyntax>
        {
            public override Accessibility GetDefaultAccessibility(IncompleteMemberSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return (declaration.IsParentKind(SyntaxKind.InterfaceDeclaration))
                    ? Accessibility.Public
                    : Accessibility.Private;
            }

            public override Accessibility GetDefaultExplicitAccessibility(IncompleteMemberSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return Accessibility.NotApplicable;
            }

            public override Accessibility GetAccessibility(IncompleteMemberSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                if (declaration.IsParentKind(SyntaxKind.InterfaceDeclaration))
                    return Accessibility.Public;

                Accessibility accessibility = SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);

                return (accessibility != Accessibility.NotApplicable)
                    ? accessibility
                    : GetDefaultAccessibility(declaration);
            }

            public override Accessibility GetExplicitAccessibility(IncompleteMemberSyntax declaration)
            {
                if (declaration == null)
                    throw new ArgumentNullException(nameof(declaration));

                return SyntaxAccessibility.GetExplicitAccessibility(declaration.Modifiers);
            }
        }
    }
}
