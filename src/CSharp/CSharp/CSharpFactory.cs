// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

#pragma warning disable CS1591

namespace Roslynator.CSharp
{
    /// <summary>
    /// A factory for syntax nodes, tokens and trivia. This class is built on top of <see cref="SyntaxFactory"/> members.
    /// </summary>
    public static class CSharpFactory
    {
        #region Trivia
        public static SyntaxTrivia EmptyWhitespace()
        {
            return SyntaxTrivia(SyntaxKind.WhitespaceTrivia, "");
        }

        public static SyntaxTrivia NewLine()
        {
            switch (Environment.NewLine)
            {
                case "\r":
                    return CarriageReturn;
                case "\n":
                    return LineFeed;
                default:
                    return CarriageReturnLineFeed;
            }
        }
        #endregion Trivia

        #region Token
        internal static SyntaxToken OpenParenToken()
        {
            return Token(SyntaxKind.OpenParenToken);
        }

        internal static SyntaxToken CloseParenToken()
        {
            return Token(SyntaxKind.CloseParenToken);
        }

        internal static SyntaxToken OpenBraceToken()
        {
            return Token(SyntaxKind.OpenBraceToken);
        }

        internal static SyntaxToken CloseBraceToken()
        {
            return Token(SyntaxKind.CloseBraceToken);
        }

        internal static SyntaxToken OpenBracketToken()
        {
            return Token(SyntaxKind.OpenBracketToken);
        }

        internal static SyntaxToken CloseBracketToken()
        {
            return Token(SyntaxKind.CloseBracketToken);
        }

        internal static SyntaxToken SemicolonToken()
        {
            return Token(SyntaxKind.SemicolonToken);
        }

        internal static SyntaxToken CommaToken()
        {
            return Token(SyntaxKind.CommaToken);
        }
        #endregion Token

        #region Type
        public static PredefinedTypeSyntax PredefinedBoolType()
        {
            return PredefinedType(SyntaxKind.BoolKeyword);
        }

        public static PredefinedTypeSyntax PredefinedByteType()
        {
            return PredefinedType(SyntaxKind.ByteKeyword);
        }

        public static PredefinedTypeSyntax PredefinedSByteType()
        {
            return PredefinedType(SyntaxKind.SByteKeyword);
        }

        public static PredefinedTypeSyntax PredefinedIntType()
        {
            return PredefinedType(SyntaxKind.IntKeyword);
        }

        public static PredefinedTypeSyntax PredefinedUIntType()
        {
            return PredefinedType(SyntaxKind.UIntKeyword);
        }

        public static PredefinedTypeSyntax PredefinedShortType()
        {
            return PredefinedType(SyntaxKind.ShortKeyword);
        }

        public static PredefinedTypeSyntax PredefinedUShortType()
        {
            return PredefinedType(SyntaxKind.UShortKeyword);
        }

        public static PredefinedTypeSyntax PredefinedLongType()
        {
            return PredefinedType(SyntaxKind.LongKeyword);
        }

        public static PredefinedTypeSyntax PredefinedULongType()
        {
            return PredefinedType(SyntaxKind.ULongKeyword);
        }

        public static PredefinedTypeSyntax PredefinedFloatType()
        {
            return PredefinedType(SyntaxKind.FloatKeyword);
        }

        public static PredefinedTypeSyntax PredefinedDoubleType()
        {
            return PredefinedType(SyntaxKind.DoubleKeyword);
        }

        public static PredefinedTypeSyntax PredefinedDecimalType()
        {
            return PredefinedType(SyntaxKind.DecimalKeyword);
        }

        public static PredefinedTypeSyntax PredefinedStringType()
        {
            return PredefinedType(SyntaxKind.StringKeyword);
        }

        public static PredefinedTypeSyntax PredefinedCharType()
        {
            return PredefinedType(SyntaxKind.CharKeyword);
        }

        public static PredefinedTypeSyntax PredefinedObjectType()
        {
            return PredefinedType(SyntaxKind.ObjectKeyword);
        }

        public static PredefinedTypeSyntax VoidType()
        {
            return PredefinedType(SyntaxKind.VoidKeyword);
        }

        private static PredefinedTypeSyntax PredefinedType(SyntaxKind syntaxKind)
        {
            return SyntaxFactory.PredefinedType(Token(syntaxKind));
        }

        #endregion Type

        #region List
        /// <summary>
        /// Creates a list of modifiers from the specified accessibility.
        /// </summary>
        /// <param name="accessibility"></param>
        public static SyntaxTokenList TokenList(Accessibility accessibility)
        {
            switch (accessibility)
            {
                case Accessibility.Public:
                    return Modifiers.Public();
                case Accessibility.Internal:
                    return Modifiers.Internal();
                case Accessibility.ProtectedOrInternal:
                    return Modifiers.Protected_Internal();
                case Accessibility.Protected:
                    return Modifiers.Protected();
                case Accessibility.ProtectedAndInternal:
                    return Modifiers.Private_Protected();
                case Accessibility.Private:
                    return Modifiers.Private();
                case Accessibility.NotApplicable:
                    return default;
                default:
                    throw new ArgumentException($"Unknown accessibility '{accessibility}'.");
            }
        }

        public static SyntaxTokenList TokenList(SyntaxKind kind)
        {
            return SyntaxFactory.TokenList(Token(kind));
        }

        public static SyntaxTokenList TokenList(SyntaxKind kind1, SyntaxKind kind2)
        {
            return SyntaxFactory.TokenList(Token(kind1), Token(kind2));
        }

        public static SyntaxTokenList TokenList(SyntaxKind kind1, SyntaxKind kind2, SyntaxKind kind3)
        {
            return SyntaxFactory.TokenList(Token(kind1), Token(kind2), Token(kind3));
        }

        public static ArgumentListSyntax ArgumentList(params ArgumentSyntax[] arguments)
        {
            return SyntaxFactory.ArgumentList(SeparatedList(arguments));
        }

        public static ArgumentListSyntax ArgumentList(ArgumentSyntax argument)
        {
            return SyntaxFactory.ArgumentList(SingletonSeparatedList(argument));
        }

        public static BracketedArgumentListSyntax BracketedArgumentList(params ArgumentSyntax[] arguments)
        {
            return SyntaxFactory.BracketedArgumentList(SeparatedList(arguments));
        }

        public static BracketedArgumentListSyntax BracketedArgumentList(ArgumentSyntax argument)
        {
            return SyntaxFactory.BracketedArgumentList(SingletonSeparatedList(argument));
        }

        public static AttributeListSyntax AttributeList(AttributeSyntax attribute)
        {
            return SyntaxFactory.AttributeList(SingletonSeparatedList(attribute));
        }

        public static AttributeListSyntax AttributeList(params AttributeSyntax[] attributes)
        {
            return SyntaxFactory.AttributeList(SeparatedList(attributes));
        }

        public static AttributeArgumentListSyntax AttributeArgumentList(params AttributeArgumentSyntax[] attributeArguments)
        {
            return SyntaxFactory.AttributeArgumentList(SeparatedList(attributeArguments));
        }

        public static AttributeArgumentListSyntax AttributeArgumentList(AttributeArgumentSyntax attributeArgument)
        {
            return SyntaxFactory.AttributeArgumentList(SingletonSeparatedList(attributeArgument));
        }

        public static AccessorListSyntax AccessorList(params AccessorDeclarationSyntax[] accessors)
        {
            return SyntaxFactory.AccessorList(List(accessors));
        }

        public static AccessorListSyntax AccessorList(AccessorDeclarationSyntax accessor)
        {
            return SyntaxFactory.AccessorList(SingletonList(accessor));
        }

        public static ParameterListSyntax ParameterList(ParameterSyntax parameter)
        {
            return SyntaxFactory.ParameterList(SingletonSeparatedList(parameter));
        }

        public static ParameterListSyntax ParameterList(params ParameterSyntax[] parameters)
        {
            return SyntaxFactory.ParameterList(SeparatedList(parameters));
        }

        public static BracketedParameterListSyntax BracketedParameterList(ParameterSyntax parameter)
        {
            return SyntaxFactory.BracketedParameterList(SingletonSeparatedList(parameter));
        }

        public static BracketedParameterListSyntax BracketedParameterList(params ParameterSyntax[] parameters)
        {
            return SyntaxFactory.BracketedParameterList(SeparatedList(parameters));
        }

        public static TypeArgumentListSyntax TypeArgumentList(TypeSyntax argument)
        {
            return SyntaxFactory.TypeArgumentList(SingletonSeparatedList(argument));
        }

        public static TypeArgumentListSyntax TypeArgumentList(params TypeSyntax[] arguments)
        {
            return SyntaxFactory.TypeArgumentList(SeparatedList(arguments));
        }

        public static TypeParameterListSyntax TypeParameterList(TypeParameterSyntax parameter)
        {
            return SyntaxFactory.TypeParameterList(SingletonSeparatedList(parameter));
        }

        public static TypeParameterListSyntax TypeParameterList(params TypeParameterSyntax[] parameters)
        {
            return SyntaxFactory.TypeParameterList(SeparatedList(parameters));
        }

        public static BaseListSyntax BaseList(BaseTypeSyntax type)
        {
            return SyntaxFactory.BaseList(SingletonSeparatedList(type));
        }

        public static BaseListSyntax BaseList(params BaseTypeSyntax[] types)
        {
            return SyntaxFactory.BaseList(SeparatedList(types));
        }

        public static BaseListSyntax BaseList(SyntaxToken colonToken, BaseTypeSyntax baseType)
        {
            return SyntaxFactory.BaseList(colonToken, SingletonSeparatedList(baseType));
        }

        public static BaseListSyntax BaseList(SyntaxToken colonToken, params BaseTypeSyntax[] types)
        {
            return SyntaxFactory.BaseList(colonToken, SeparatedList(types));
        }
        #endregion List

        #region MemberDeclaration

        internal static NamespaceDeclarationSyntax NamespaceDeclaration(string name, MemberDeclarationSyntax member)
        {
            return NamespaceDeclaration(ParseName(name), member);
        }

        public static NamespaceDeclarationSyntax NamespaceDeclaration(NameSyntax name, MemberDeclarationSyntax member)
        {
            return NamespaceDeclaration(name, SingletonList(member));
        }

        internal static NamespaceDeclarationSyntax NamespaceDeclaration(string name, SyntaxList<MemberDeclarationSyntax> members)
        {
            return NamespaceDeclaration(ParseName(name), members);
        }

        public static NamespaceDeclarationSyntax NamespaceDeclaration(NameSyntax name, SyntaxList<MemberDeclarationSyntax> members)
        {
            return SyntaxFactory.NamespaceDeclaration(
                name,
                default(SyntaxList<ExternAliasDirectiveSyntax>),
                default(SyntaxList<UsingDirectiveSyntax>),
                members);
        }

        public static ClassDeclarationSyntax ClassDeclaration(SyntaxTokenList modifiers, string identifier, SyntaxList<MemberDeclarationSyntax> members = default)
        {
            return ClassDeclaration(modifiers, Identifier(identifier), members);
        }

        public static ClassDeclarationSyntax ClassDeclaration(SyntaxTokenList modifiers, SyntaxToken identifier, SyntaxList<MemberDeclarationSyntax> members = default)
        {
            return SyntaxFactory.ClassDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                identifier,
                default(TypeParameterListSyntax),
                default(BaseListSyntax),
                default(SyntaxList<TypeParameterConstraintClauseSyntax>),
                members);
        }

        internal static ClassDeclarationSyntax ClassDeclaration(StructDeclarationSyntax structDeclaration)
        {
            if (structDeclaration == null)
                throw new ArgumentNullException(nameof(structDeclaration));

            SyntaxToken keyword = structDeclaration.Keyword;

            return SyntaxFactory.ClassDeclaration(
                structDeclaration.AttributeLists,
                structDeclaration.Modifiers,
                SyntaxFactory.Token(keyword.LeadingTrivia, SyntaxKind.ClassKeyword, keyword.TrailingTrivia),
                structDeclaration.Identifier,
                structDeclaration.TypeParameterList,
                structDeclaration.BaseList,
                structDeclaration.ConstraintClauses,
                structDeclaration.OpenBraceToken,
                structDeclaration.Members,
                structDeclaration.CloseBraceToken,
                structDeclaration.SemicolonToken);
        }

        public static StructDeclarationSyntax StructDeclaration(SyntaxTokenList modifiers, string identifier, SyntaxList<MemberDeclarationSyntax> members = default)
        {
            return StructDeclaration(modifiers, Identifier(identifier), members);
        }

        public static StructDeclarationSyntax StructDeclaration(SyntaxTokenList modifiers, SyntaxToken identifier, SyntaxList<MemberDeclarationSyntax> members = default)
        {
            return SyntaxFactory.StructDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                identifier,
                default(TypeParameterListSyntax),
                default(BaseListSyntax),
                default(SyntaxList<TypeParameterConstraintClauseSyntax>),
                members);
        }

        internal static StructDeclarationSyntax StructDeclaration(ClassDeclarationSyntax classDeclaration)
        {
            if (classDeclaration == null)
                throw new ArgumentNullException(nameof(classDeclaration));

            SyntaxToken keyword = classDeclaration.Keyword;

            return SyntaxFactory.StructDeclaration(
                classDeclaration.AttributeLists,
                classDeclaration.Modifiers,
                SyntaxFactory.Token(keyword.LeadingTrivia, SyntaxKind.StructKeyword, keyword.TrailingTrivia),
                classDeclaration.Identifier,
                classDeclaration.TypeParameterList,
                classDeclaration.BaseList,
                classDeclaration.ConstraintClauses,
                classDeclaration.OpenBraceToken,
                classDeclaration.Members,
                classDeclaration.CloseBraceToken,
                classDeclaration.SemicolonToken);
        }

        public static InterfaceDeclarationSyntax InterfaceDeclaration(SyntaxTokenList modifiers, string identifier, SyntaxList<MemberDeclarationSyntax> members = default)
        {
            return InterfaceDeclaration(modifiers, Identifier(identifier), members);
        }

        public static InterfaceDeclarationSyntax InterfaceDeclaration(SyntaxTokenList modifiers, SyntaxToken identifier, SyntaxList<MemberDeclarationSyntax> members = default)
        {
            return SyntaxFactory.InterfaceDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                identifier,
                default(TypeParameterListSyntax),
                default(BaseListSyntax),
                default(SyntaxList<TypeParameterConstraintClauseSyntax>),
                members);
        }

        public static ConstructorDeclarationSyntax ConstructorDeclaration(SyntaxTokenList modifiers, SyntaxToken identifier, ParameterListSyntax parameterList, BlockSyntax body)
        {
            return SyntaxFactory.ConstructorDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                identifier,
                parameterList,
                default(ConstructorInitializerSyntax),
                body);
        }

        public static ConstructorDeclarationSyntax ConstructorDeclaration(SyntaxTokenList modifiers, SyntaxToken identifier, ParameterListSyntax parameterList, ArrowExpressionClauseSyntax expressionBody)
        {
            return SyntaxFactory.ConstructorDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                identifier,
                parameterList,
                default(ConstructorInitializerSyntax),
                expressionBody,
                SemicolonToken());
        }

        public static EnumDeclarationSyntax EnumDeclaration(SyntaxTokenList modifiers, SyntaxToken identifier, SeparatedSyntaxList<EnumMemberDeclarationSyntax> members)
        {
            return SyntaxFactory.EnumDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                identifier,
                default(BaseListSyntax),
                members);
        }

        public static EnumMemberDeclarationSyntax EnumMemberDeclaration(string name, ExpressionSyntax value)
        {
            return EnumMemberDeclaration(Identifier(name), value);
        }

        public static EnumMemberDeclarationSyntax EnumMemberDeclaration(SyntaxToken identifier, ExpressionSyntax value)
        {
            return EnumMemberDeclaration(identifier, EqualsValueClause(value));
        }

        public static EnumMemberDeclarationSyntax EnumMemberDeclaration(string name, EqualsValueClauseSyntax value)
        {
            return EnumMemberDeclaration(Identifier(name), value);
        }

        public static EnumMemberDeclarationSyntax EnumMemberDeclaration(SyntaxToken identifier, EqualsValueClauseSyntax value)
        {
            return SyntaxFactory.EnumMemberDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                identifier,
                value);
        }

        public static FieldDeclarationSyntax FieldDeclaration(SyntaxTokenList modifiers, TypeSyntax type, string identifier, ExpressionSyntax value = null)
        {
            return FieldDeclaration(
                modifiers,
                type,
                Identifier(identifier),
                value);
        }

        public static FieldDeclarationSyntax FieldDeclaration(SyntaxTokenList modifiers, TypeSyntax type, string identifier, EqualsValueClauseSyntax initializer)
        {
            return FieldDeclaration(
                modifiers,
                type,
                Identifier(identifier),
                initializer);
        }

        public static FieldDeclarationSyntax FieldDeclaration(SyntaxTokenList modifiers, TypeSyntax type, SyntaxToken identifier, ExpressionSyntax value = null)
        {
            return FieldDeclaration(
                modifiers,
                type,
                identifier,
                (value != null) ? EqualsValueClause(value) : default);
        }

        public static FieldDeclarationSyntax FieldDeclaration(SyntaxTokenList modifiers, TypeSyntax type, SyntaxToken identifier, EqualsValueClauseSyntax initializer)
        {
            return SyntaxFactory.FieldDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                VariableDeclaration(
                    type,
                    identifier,
                    initializer));
        }

        public static EventFieldDeclarationSyntax EventFieldDeclaration(SyntaxTokenList modifiers, TypeSyntax type, string identifier)
        {
            return EventFieldDeclaration(
                modifiers,
                type,
                Identifier(identifier));
        }

        public static EventFieldDeclarationSyntax EventFieldDeclaration(SyntaxTokenList modifiers, TypeSyntax type, SyntaxToken identifier)
        {
            return SyntaxFactory.EventFieldDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                VariableDeclaration(type, identifier));
        }

        public static EventDeclarationSyntax EventDeclaration(SyntaxTokenList modifiers, TypeSyntax type, string identifier, AccessorListSyntax accessorList)
        {
            return EventDeclaration(
                modifiers,
                type,
                Identifier(identifier),
                accessorList);
        }

        public static EventDeclarationSyntax EventDeclaration(SyntaxTokenList modifiers, TypeSyntax type, SyntaxToken identifier, AccessorListSyntax accessorList)
        {
            return SyntaxFactory.EventDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                type,
                default(ExplicitInterfaceSpecifierSyntax),
                identifier,
                accessorList);
        }

        public static DelegateDeclarationSyntax DelegateDeclaration(SyntaxTokenList modifiers, TypeSyntax returnType, string identifier, ParameterListSyntax parameterList)
        {
            return DelegateDeclaration(
                modifiers,
                returnType,
                Identifier(identifier),
                parameterList);
        }

        public static DelegateDeclarationSyntax DelegateDeclaration(SyntaxTokenList modifiers, TypeSyntax returnType, SyntaxToken identifier, ParameterListSyntax parameterList)
        {
            return SyntaxFactory.DelegateDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                returnType,
                identifier,
                default(TypeParameterListSyntax),
                parameterList,
                default(SyntaxList<TypeParameterConstraintClauseSyntax>));
        }

        public static MethodDeclarationSyntax MethodDeclaration(
            SyntaxTokenList modifiers,
            TypeSyntax returnType,
            SyntaxToken identifier,
            ParameterListSyntax parameterList,
            BlockSyntax body)
        {
            return SyntaxFactory.MethodDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                returnType,
                default(ExplicitInterfaceSpecifierSyntax),
                identifier,
                default(TypeParameterListSyntax),
                parameterList,
                default(SyntaxList<TypeParameterConstraintClauseSyntax>),
                body,
                default(ArrowExpressionClauseSyntax));
        }

        public static MethodDeclarationSyntax MethodDeclaration(
            SyntaxTokenList modifiers,
            TypeSyntax returnType,
            SyntaxToken identifier,
            ParameterListSyntax parameterList,
            ArrowExpressionClauseSyntax expressionBody)
        {
            return SyntaxFactory.MethodDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                returnType,
                default(ExplicitInterfaceSpecifierSyntax),
                identifier,
                default(TypeParameterListSyntax),
                parameterList,
                default(SyntaxList<TypeParameterConstraintClauseSyntax>),
                default(BlockSyntax),
                expressionBody,
                SemicolonToken());
        }

        public static OperatorDeclarationSyntax OperatorDeclaration(
            SyntaxTokenList modifiers,
            TypeSyntax returnType,
            SyntaxToken operatorToken,
            ParameterListSyntax parameterList,
            BlockSyntax body)
        {
            return SyntaxFactory.OperatorDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                returnType,
                operatorToken,
                parameterList,
                body,
                default(ArrowExpressionClauseSyntax));
        }

        public static OperatorDeclarationSyntax OperatorDeclaration(
            SyntaxTokenList modifiers,
            TypeSyntax returnType,
            SyntaxToken operatorToken,
            ParameterListSyntax parameterList,
            ArrowExpressionClauseSyntax expressionBody)
        {
            return SyntaxFactory.OperatorDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                returnType,
                Token(SyntaxKind.OperatorKeyword),
                operatorToken,
                parameterList,
                default(BlockSyntax),
                expressionBody,
                SemicolonToken());
        }

        public static ConversionOperatorDeclarationSyntax ImplicitConversionOperatorDeclaration(
            SyntaxTokenList modifiers,
            TypeSyntax type,
            ParameterListSyntax parameterList,
            BlockSyntax body)
        {
            return SyntaxFactory.ConversionOperatorDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                Token(SyntaxKind.ImplicitKeyword),
                type,
                parameterList,
                body,
                default(ArrowExpressionClauseSyntax));
        }

        public static ConversionOperatorDeclarationSyntax ImplicitConversionOperatorDeclaration(
            SyntaxTokenList modifiers,
            TypeSyntax type,
            ParameterListSyntax parameterList,
            ArrowExpressionClauseSyntax expressionBody)
        {
            return SyntaxFactory.ConversionOperatorDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                Token(SyntaxKind.ImplicitKeyword),
                Token(SyntaxKind.OperatorKeyword),
                type,
                parameterList,
                default(BlockSyntax),
                expressionBody,
                SemicolonToken());
        }

        public static ConversionOperatorDeclarationSyntax ExplicitConversionOperatorDeclaration(
            SyntaxTokenList modifiers,
            TypeSyntax type,
            ParameterListSyntax parameterList,
            BlockSyntax body)
        {
            return SyntaxFactory.ConversionOperatorDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                Token(SyntaxKind.ExplicitKeyword),
                type,
                parameterList,
                body,
                default(ArrowExpressionClauseSyntax));
        }

        public static ConversionOperatorDeclarationSyntax ExplicitConversionOperatorDeclaration(
            SyntaxTokenList modifiers,
            TypeSyntax type,
            ParameterListSyntax parameterList,
            ArrowExpressionClauseSyntax expressionBody)
        {
            return SyntaxFactory.ConversionOperatorDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                Token(SyntaxKind.ExplicitKeyword),
                Token(SyntaxKind.OperatorKeyword),
                type,
                parameterList,
                default(BlockSyntax),
                expressionBody,
                SemicolonToken());
        }

        public static PropertyDeclarationSyntax PropertyDeclaration(
            SyntaxTokenList modifiers,
            TypeSyntax type,
            SyntaxToken identifier,
            AccessorListSyntax accessorList,
            ExpressionSyntax value = null)
        {
            return SyntaxFactory.PropertyDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                type,
                default(ExplicitInterfaceSpecifierSyntax),
                identifier,
                accessorList,
                default(ArrowExpressionClauseSyntax),
                (value != null) ? EqualsValueClause(value) : default,
                (value != null) ? SemicolonToken() : default);
        }

        public static PropertyDeclarationSyntax PropertyDeclaration(
            SyntaxTokenList modifiers,
            TypeSyntax type,
            SyntaxToken identifier,
            ArrowExpressionClauseSyntax expressionBody)
        {
            return SyntaxFactory.PropertyDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                type,
                default(ExplicitInterfaceSpecifierSyntax),
                identifier,
                default(AccessorListSyntax),
                expressionBody,
                default(EqualsValueClauseSyntax),
                SemicolonToken());
        }

        public static IndexerDeclarationSyntax IndexerDeclaration(
            SyntaxTokenList modifiers,
            TypeSyntax type,
            BracketedParameterListSyntax parameterList,
            AccessorListSyntax accessorList)
        {
            return SyntaxFactory.IndexerDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                type,
                default(ExplicitInterfaceSpecifierSyntax),
                parameterList,
                accessorList);
        }

        public static IndexerDeclarationSyntax IndexerDeclaration(
            SyntaxTokenList modifiers,
            TypeSyntax type,
            BracketedParameterListSyntax parameterList,
            ArrowExpressionClauseSyntax expressionBody)
        {
            return SyntaxFactory.IndexerDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                type,
                default(ExplicitInterfaceSpecifierSyntax),
                Token(SyntaxKind.ThisKeyword),
                parameterList,
                default(AccessorListSyntax),
                expressionBody,
                SemicolonToken());
        }
        #endregion MemberDeclaration

        #region AccessorDeclaration
        public static AccessorDeclarationSyntax GetAccessorDeclaration(BlockSyntax body)
        {
            return GetAccessorDeclaration(default(SyntaxTokenList), body);
        }

        public static AccessorDeclarationSyntax GetAccessorDeclaration(SyntaxTokenList modifiers, BlockSyntax body)
        {
            return AccessorDeclaration(
                SyntaxKind.GetAccessorDeclaration,
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                body);
        }

        public static AccessorDeclarationSyntax GetAccessorDeclaration(ArrowExpressionClauseSyntax expressionBody)
        {
            return GetAccessorDeclaration(default(SyntaxTokenList), expressionBody);
        }

        public static AccessorDeclarationSyntax GetAccessorDeclaration(SyntaxTokenList modifiers, ArrowExpressionClauseSyntax expressionBody)
        {
            return AccessorDeclaration(
                SyntaxKind.GetAccessorDeclaration,
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                Token(SyntaxKind.GetKeyword),
                expressionBody,
                SemicolonToken());
        }

        public static AccessorDeclarationSyntax SetAccessorDeclaration(BlockSyntax body)
        {
            return SetAccessorDeclaration(default(SyntaxTokenList), body);
        }

        public static AccessorDeclarationSyntax SetAccessorDeclaration(SyntaxTokenList modifiers, BlockSyntax body)
        {
            return AccessorDeclaration(
                SyntaxKind.SetAccessorDeclaration,
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                body);
        }

        public static AccessorDeclarationSyntax SetAccessorDeclaration(ArrowExpressionClauseSyntax expressionBody)
        {
            return SetAccessorDeclaration(default(SyntaxTokenList), expressionBody);
        }

        public static AccessorDeclarationSyntax SetAccessorDeclaration(SyntaxTokenList modifiers, ArrowExpressionClauseSyntax expressionBody)
        {
            return AccessorDeclaration(
                SyntaxKind.SetAccessorDeclaration,
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                Token(SyntaxKind.SetKeyword),
                expressionBody,
                SemicolonToken());
        }

        public static AccessorDeclarationSyntax AddAccessorDeclaration(BlockSyntax body)
        {
            return AddAccessorDeclaration(default(SyntaxTokenList), body);
        }

        public static AccessorDeclarationSyntax AddAccessorDeclaration(SyntaxTokenList modifiers, BlockSyntax body)
        {
            return AccessorDeclaration(
                SyntaxKind.AddAccessorDeclaration,
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                body);
        }

        public static AccessorDeclarationSyntax AddAccessorDeclaration(ArrowExpressionClauseSyntax expressionBody)
        {
            return AddAccessorDeclaration(default(SyntaxTokenList), expressionBody);
        }

        public static AccessorDeclarationSyntax AddAccessorDeclaration(SyntaxTokenList modifiers, ArrowExpressionClauseSyntax expressionBody)
        {
            return AccessorDeclaration(
                SyntaxKind.AddAccessorDeclaration,
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                Token(SyntaxKind.AddKeyword),
                expressionBody,
                SemicolonToken());
        }

        public static AccessorDeclarationSyntax RemoveAccessorDeclaration(BlockSyntax body)
        {
            return RemoveAccessorDeclaration(default(SyntaxTokenList), body);
        }

        public static AccessorDeclarationSyntax RemoveAccessorDeclaration(SyntaxTokenList modifiers, BlockSyntax body)
        {
            return AccessorDeclaration(
                SyntaxKind.RemoveAccessorDeclaration,
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                body);
        }

        public static AccessorDeclarationSyntax RemoveAccessorDeclaration(ArrowExpressionClauseSyntax expressionBody)
        {
            return RemoveAccessorDeclaration(default(SyntaxTokenList), expressionBody);
        }

        public static AccessorDeclarationSyntax RemoveAccessorDeclaration(SyntaxTokenList modifiers, ArrowExpressionClauseSyntax expressionBody)
        {
            return AccessorDeclaration(
                SyntaxKind.RemoveAccessorDeclaration,
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                Token(SyntaxKind.RemoveKeyword),
                expressionBody,
                SemicolonToken());
        }

        public static AccessorDeclarationSyntax AutoGetAccessorDeclaration(SyntaxTokenList modifiers = default)
        {
            return AutoAccessorDeclaration(SyntaxKind.GetAccessorDeclaration, modifiers);
        }

        public static AccessorDeclarationSyntax AutoSetAccessorDeclaration(SyntaxTokenList modifiers = default)
        {
            return AutoAccessorDeclaration(SyntaxKind.SetAccessorDeclaration, modifiers);
        }

        private static AccessorDeclarationSyntax AutoAccessorDeclaration(SyntaxKind kind, SyntaxTokenList modifiers = default)
        {
            return AccessorDeclaration(
                kind,
                default(SyntaxList<AttributeListSyntax>),
                modifiers,
                Token(AccessorDeclarationKeywordKind(kind)),
                default(BlockSyntax),
                SemicolonToken());
        }

        private static SyntaxKind AccessorDeclarationKeywordKind(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.GetAccessorDeclaration:
                    return SyntaxKind.GetKeyword;
                case SyntaxKind.SetAccessorDeclaration:
                    return SyntaxKind.SetKeyword;
                case SyntaxKind.AddAccessorDeclaration:
                    return SyntaxKind.AddKeyword;
                case SyntaxKind.RemoveAccessorDeclaration:
                    return SyntaxKind.RemoveKeyword;
                case SyntaxKind.UnknownAccessorDeclaration:
                    return SyntaxKind.IdentifierToken;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }
        #endregion AccessorDeclaration

        #region Statement
        public static LocalDeclarationStatementSyntax LocalDeclarationStatement(TypeSyntax type, string identifier, ExpressionSyntax value = null)
        {
            return LocalDeclarationStatement(type, Identifier(identifier), value);
        }

        public static LocalDeclarationStatementSyntax LocalDeclarationStatement(TypeSyntax type, SyntaxToken identifier, ExpressionSyntax value = null)
        {
            VariableDeclaratorSyntax variableDeclarator = (value != null)
                ? VariableDeclarator(identifier, EqualsValueClause(value))
                : SyntaxFactory.VariableDeclarator(identifier);

            return SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(
                    type,
                    SingletonSeparatedList(variableDeclarator)));
        }

        public static LocalDeclarationStatementSyntax LocalDeclarationStatement(TypeSyntax type, string identifier, EqualsValueClauseSyntax initializer)
        {
            return LocalDeclarationStatement(type, Identifier(identifier), initializer);
        }

        public static LocalDeclarationStatementSyntax LocalDeclarationStatement(TypeSyntax type, SyntaxToken identifier, EqualsValueClauseSyntax initializer)
        {
            return SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(
                    type,
                    SingletonSeparatedList(VariableDeclarator(identifier, initializer))));
        }

        public static YieldStatementSyntax YieldReturnStatement(ExpressionSyntax expression)
        {
            return YieldStatement(SyntaxKind.YieldReturnStatement, expression);
        }

        public static YieldStatementSyntax YieldBreakStatement()
        {
            return YieldStatement(SyntaxKind.YieldBreakStatement);
        }

        public static TryStatementSyntax TryStatement(BlockSyntax block, CatchClauseSyntax @catch, FinallyClauseSyntax @finally = null)
        {
            return SyntaxFactory.TryStatement(block, SingletonList(@catch), @finally);
        }

        public static ExpressionStatementSyntax SimpleAssignmentStatement(ExpressionSyntax left, ExpressionSyntax right)
        {
            return ExpressionStatement(SimpleAssignmentExpression(left, right));
        }

        public static ExpressionStatementSyntax SimpleAssignmentStatement(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return ExpressionStatement(SimpleAssignmentExpression(left, operatorToken, right));
        }

        public static BlockSyntax Block(StatementSyntax statement)
        {
            return SyntaxFactory.Block(SingletonList(statement));
        }

        public static BlockSyntax Block(SyntaxToken openBrace, StatementSyntax statement, SyntaxToken closeBrace)
        {
            return SyntaxFactory.Block(openBrace, SingletonList(statement), closeBrace);
        }

        public static LocalFunctionStatementSyntax LocalFunctionStatement(
            SyntaxTokenList modifiers,
            TypeSyntax returnType,
            SyntaxToken identifier,
            ParameterListSyntax parameterList,
            BlockSyntax body)
        {
            return SyntaxFactory.LocalFunctionStatement(
                modifiers,
                returnType,
                identifier,
                default(TypeParameterListSyntax),
                parameterList,
                default(SyntaxList<TypeParameterConstraintClauseSyntax>),
                body,
                default(ArrowExpressionClauseSyntax));
        }

        public static LocalFunctionStatementSyntax LocalFunctionStatement(
            SyntaxTokenList modifiers,
            TypeSyntax returnType,
            SyntaxToken identifier,
            ParameterListSyntax parameterList,
            ArrowExpressionClauseSyntax expressionBody)
        {
            return SyntaxFactory.LocalFunctionStatement(
                modifiers,
                returnType,
                identifier,
                default(TypeParameterListSyntax),
                parameterList,
                default(SyntaxList<TypeParameterConstraintClauseSyntax>),
                default(BlockSyntax),
                expressionBody,
                SemicolonToken());
        }

        internal static ThrowStatementSyntax ThrowNewStatement(TypeSyntax exceptionType)
        {
            return ThrowStatement(ObjectCreationExpression(exceptionType, ArgumentList()));
        }

        public static ForStatementSyntax ForStatement(
            VariableDeclarationSyntax declaration,
            ExpressionSyntax condition,
            ExpressionSyntax incrementor,
            StatementSyntax statement)
        {
            return SyntaxFactory.ForStatement(
                declaration: declaration,
                initializers: default(SeparatedSyntaxList<ExpressionSyntax>),
                condition: condition,
                incrementors: SingletonSeparatedList(incrementor),
                statement: statement);
        }
        #endregion Statement

        #region BinaryExpression
        public static BinaryExpressionSyntax AddExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.AddExpression, left, right);
        }

        public static BinaryExpressionSyntax AddExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.AddExpression, left, operatorToken, right);
        }

        public static BinaryExpressionSyntax SubtractExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.SubtractExpression, left, right);
        }

        public static BinaryExpressionSyntax SubtractExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.SubtractExpression, left, operatorToken, right);
        }

        public static BinaryExpressionSyntax MultiplyExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.MultiplyExpression, left, right);
        }

        public static BinaryExpressionSyntax MultiplyExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.MultiplyExpression, left, operatorToken, right);
        }

        public static BinaryExpressionSyntax DivideExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.DivideExpression, left, right);
        }

        public static BinaryExpressionSyntax DivideExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.DivideExpression, left, operatorToken, right);
        }

        public static BinaryExpressionSyntax ModuloExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.ModuloExpression, left, right);
        }

        public static BinaryExpressionSyntax ModuloExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.ModuloExpression, left, operatorToken, right);
        }

        public static BinaryExpressionSyntax LeftShiftExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.LeftShiftExpression, left, right);
        }

        public static BinaryExpressionSyntax LeftShiftExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.LeftShiftExpression, left, operatorToken, right);
        }

        public static BinaryExpressionSyntax RightShiftExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.RightShiftExpression, left, right);
        }

        public static BinaryExpressionSyntax RightShiftExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.RightShiftExpression, left, operatorToken, right);
        }

        public static BinaryExpressionSyntax LogicalOrExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.LogicalOrExpression, left, right);
        }

        public static BinaryExpressionSyntax LogicalOrExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.LogicalOrExpression, left, operatorToken, right);
        }

        public static BinaryExpressionSyntax LogicalAndExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.LogicalAndExpression, left, right);
        }

        public static BinaryExpressionSyntax LogicalAndExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.LogicalAndExpression, left, operatorToken, right);
        }

        public static BinaryExpressionSyntax BitwiseOrExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.BitwiseOrExpression, left, right);
        }

        public static BinaryExpressionSyntax BitwiseOrExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.BitwiseOrExpression, left, operatorToken, right);
        }

        public static BinaryExpressionSyntax BitwiseAndExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.BitwiseAndExpression, left, right);
        }

        public static BinaryExpressionSyntax BitwiseAndExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.BitwiseAndExpression, left, operatorToken, right);
        }

        public static BinaryExpressionSyntax ExclusiveOrExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.ExclusiveOrExpression, left, right);
        }

        public static BinaryExpressionSyntax ExclusiveOrExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.ExclusiveOrExpression, left, operatorToken, right);
        }

        public static BinaryExpressionSyntax EqualsExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.EqualsExpression, left, right);
        }

        public static BinaryExpressionSyntax EqualsExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.EqualsExpression, left, operatorToken, right);
        }

        public static BinaryExpressionSyntax NotEqualsExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.NotEqualsExpression, left, right);
        }

        public static BinaryExpressionSyntax NotEqualsExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.NotEqualsExpression, left, operatorToken, right);
        }

        public static BinaryExpressionSyntax LessThanExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.LessThanExpression, left, right);
        }

        public static BinaryExpressionSyntax LessThanExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.LessThanExpression, left, operatorToken, right);
        }

        public static BinaryExpressionSyntax LessThanOrEqualExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.LessThanOrEqualExpression, left, right);
        }

        public static BinaryExpressionSyntax LessThanOrEqualExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.LessThanOrEqualExpression, left, operatorToken, right);
        }

        public static BinaryExpressionSyntax GreaterThanExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.GreaterThanExpression, left, right);
        }

        public static BinaryExpressionSyntax GreaterThanExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.GreaterThanExpression, left, operatorToken, right);
        }

        public static BinaryExpressionSyntax GreaterThanOrEqualExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.GreaterThanOrEqualExpression, left, right);
        }

        public static BinaryExpressionSyntax GreaterThanOrEqualExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.GreaterThanOrEqualExpression, left, operatorToken, right);
        }

        public static BinaryExpressionSyntax IsExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.IsExpression, left, right);
        }

        public static BinaryExpressionSyntax IsExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.IsExpression, left, operatorToken, right);
        }

        public static BinaryExpressionSyntax AsExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.AsExpression, left, right);
        }

        public static BinaryExpressionSyntax AsExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.AsExpression, left, operatorToken, right);
        }

        public static BinaryExpressionSyntax CoalesceExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.CoalesceExpression, left, right);
        }

        public static BinaryExpressionSyntax CoalesceExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return BinaryExpression(SyntaxKind.CoalesceExpression, left, operatorToken, right);
        }
        #endregion BinaryExpression

        #region PrefixUnaryExpression
        public static PrefixUnaryExpressionSyntax UnaryPlusExpression(ExpressionSyntax operand)
        {
            return PrefixUnaryExpression(SyntaxKind.UnaryPlusExpression, operand);
        }

        public static PrefixUnaryExpressionSyntax UnaryPlusExpression(ExpressionSyntax operand, SyntaxToken operatorToken)
        {
            return PrefixUnaryExpression(SyntaxKind.UnaryPlusExpression, operatorToken, operand);
        }

        public static PrefixUnaryExpressionSyntax UnaryMinusExpression(ExpressionSyntax operand)
        {
            return PrefixUnaryExpression(SyntaxKind.UnaryMinusExpression, operand);
        }

        public static PrefixUnaryExpressionSyntax UnaryMinusExpression(ExpressionSyntax operand, SyntaxToken operatorToken)
        {
            return PrefixUnaryExpression(SyntaxKind.UnaryMinusExpression, operatorToken, operand);
        }

        public static PrefixUnaryExpressionSyntax BitwiseNotExpression(ExpressionSyntax operand)
        {
            return PrefixUnaryExpression(SyntaxKind.BitwiseNotExpression, operand);
        }

        public static PrefixUnaryExpressionSyntax BitwiseNotExpression(ExpressionSyntax operand, SyntaxToken operatorToken)
        {
            return PrefixUnaryExpression(SyntaxKind.BitwiseNotExpression, operatorToken, operand);
        }

        public static PrefixUnaryExpressionSyntax LogicalNotExpression(ExpressionSyntax operand)
        {
            return PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, operand);
        }

        public static PrefixUnaryExpressionSyntax LogicalNotExpression(ExpressionSyntax operand, SyntaxToken operatorToken)
        {
            return PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, operatorToken, operand);
        }

        public static PrefixUnaryExpressionSyntax PreIncrementExpression(ExpressionSyntax operand)
        {
            return PrefixUnaryExpression(SyntaxKind.PreIncrementExpression, operand);
        }

        public static PrefixUnaryExpressionSyntax PreIncrementExpression(ExpressionSyntax operand, SyntaxToken operatorToken)
        {
            return PrefixUnaryExpression(SyntaxKind.PreIncrementExpression, operatorToken, operand);
        }

        public static PrefixUnaryExpressionSyntax PreDecrementExpression(ExpressionSyntax operand)
        {
            return PrefixUnaryExpression(SyntaxKind.PreDecrementExpression, operand);
        }

        public static PrefixUnaryExpressionSyntax PreDecrementExpression(ExpressionSyntax operand, SyntaxToken operatorToken)
        {
            return PrefixUnaryExpression(SyntaxKind.PreDecrementExpression, operatorToken, operand);
        }

        public static PrefixUnaryExpressionSyntax AddressOfExpression(ExpressionSyntax operand)
        {
            return PrefixUnaryExpression(SyntaxKind.AddressOfExpression, operand);
        }

        public static PrefixUnaryExpressionSyntax AddressOfExpression(ExpressionSyntax operand, SyntaxToken operatorToken)
        {
            return PrefixUnaryExpression(SyntaxKind.AddressOfExpression, operatorToken, operand);
        }

        public static PrefixUnaryExpressionSyntax PointerIndirectionExpression(ExpressionSyntax operand)
        {
            return PrefixUnaryExpression(SyntaxKind.PointerIndirectionExpression, operand);
        }

        public static PrefixUnaryExpressionSyntax PointerIndirectionExpression(ExpressionSyntax operand, SyntaxToken operatorToken)
        {
            return PrefixUnaryExpression(SyntaxKind.PointerIndirectionExpression, operatorToken, operand);
        }
        #endregion PrefixUnaryExpression

        #region PostfixUnaryExpression
        public static PostfixUnaryExpressionSyntax PostIncrementExpression(ExpressionSyntax operand)
        {
            return PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, operand);
        }

        public static PostfixUnaryExpressionSyntax PostIncrementExpression(ExpressionSyntax operand, SyntaxToken operatorToken)
        {
            return PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, operand, operatorToken);
        }

        public static PostfixUnaryExpressionSyntax PostDecrementExpression(ExpressionSyntax operand)
        {
            return PostfixUnaryExpression(SyntaxKind.PostDecrementExpression, operand);
        }

        public static PostfixUnaryExpressionSyntax PostDecrementExpression(ExpressionSyntax operand, SyntaxToken operatorToken)
        {
            return PostfixUnaryExpression(SyntaxKind.PostDecrementExpression, operand, operatorToken);
        }
        #endregion PostfixUnaryExpression

        #region AssignmentExpression
        public static AssignmentExpressionSyntax SimpleAssignmentExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, right);
        }

        public static AssignmentExpressionSyntax SimpleAssignmentExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, operatorToken, right);
        }

        public static AssignmentExpressionSyntax AddAssignmentExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return AssignmentExpression(SyntaxKind.AddAssignmentExpression, left, right);
        }

        public static AssignmentExpressionSyntax AddAssignmentExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return AssignmentExpression(SyntaxKind.AddAssignmentExpression, left, operatorToken, right);
        }

        public static AssignmentExpressionSyntax SubtractAssignmentExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return AssignmentExpression(SyntaxKind.SubtractAssignmentExpression, left, right);
        }

        public static AssignmentExpressionSyntax SubtractAssignmentExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return AssignmentExpression(SyntaxKind.SubtractAssignmentExpression, left, operatorToken, right);
        }

        public static AssignmentExpressionSyntax MultiplyAssignmentExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return AssignmentExpression(SyntaxKind.MultiplyAssignmentExpression, left, right);
        }

        public static AssignmentExpressionSyntax MultiplyAssignmentExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return AssignmentExpression(SyntaxKind.MultiplyAssignmentExpression, left, operatorToken, right);
        }

        public static AssignmentExpressionSyntax DivideAssignmentExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return AssignmentExpression(SyntaxKind.DivideAssignmentExpression, left, right);
        }

        public static AssignmentExpressionSyntax DivideAssignmentExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return AssignmentExpression(SyntaxKind.DivideAssignmentExpression, left, operatorToken, right);
        }

        public static AssignmentExpressionSyntax ModuloAssignmentExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return AssignmentExpression(SyntaxKind.ModuloAssignmentExpression, left, right);
        }

        public static AssignmentExpressionSyntax ModuloAssignmentExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return AssignmentExpression(SyntaxKind.ModuloAssignmentExpression, left, operatorToken, right);
        }

        public static AssignmentExpressionSyntax AndAssignmentExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return AssignmentExpression(SyntaxKind.AndAssignmentExpression, left, right);
        }

        public static AssignmentExpressionSyntax AndAssignmentExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return AssignmentExpression(SyntaxKind.AndAssignmentExpression, left, operatorToken, right);
        }

        public static AssignmentExpressionSyntax ExclusiveOrAssignmentExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return AssignmentExpression(SyntaxKind.ExclusiveOrAssignmentExpression, left, right);
        }

        public static AssignmentExpressionSyntax ExclusiveOrAssignmentExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return AssignmentExpression(SyntaxKind.ExclusiveOrAssignmentExpression, left, operatorToken, right);
        }

        public static AssignmentExpressionSyntax OrAssignmentExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return AssignmentExpression(SyntaxKind.OrAssignmentExpression, left, right);
        }

        public static AssignmentExpressionSyntax OrAssignmentExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return AssignmentExpression(SyntaxKind.OrAssignmentExpression, left, operatorToken, right);
        }

        public static AssignmentExpressionSyntax LeftShiftAssignmentExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return AssignmentExpression(SyntaxKind.LeftShiftAssignmentExpression, left, right);
        }

        public static AssignmentExpressionSyntax LeftShiftAssignmentExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return AssignmentExpression(SyntaxKind.LeftShiftAssignmentExpression, left, operatorToken, right);
        }

        public static AssignmentExpressionSyntax RightShiftAssignmentExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return AssignmentExpression(SyntaxKind.RightShiftAssignmentExpression, left, right);
        }

        public static AssignmentExpressionSyntax RightShiftAssignmentExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return AssignmentExpression(SyntaxKind.RightShiftAssignmentExpression, left, operatorToken, right);
        }

        public static AssignmentExpressionSyntax CoalesceAssignmentExpression(ExpressionSyntax left, ExpressionSyntax right)
        {
            return AssignmentExpression(SyntaxKind.CoalesceAssignmentExpression, left, right);
        }

        public static AssignmentExpressionSyntax CoalesceAssignmentExpression(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            return AssignmentExpression(SyntaxKind.CoalesceAssignmentExpression, left, operatorToken, right);
        }
        #endregion AssignmentExpression

        #region LiteralExpression
        public static LiteralExpressionSyntax StringLiteralExpression(string value)
        {
            return SyntaxFactory.LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                Literal(value));
        }

        public static LiteralExpressionSyntax CharacterLiteralExpression(char value)
        {
            return SyntaxFactory.LiteralExpression(
                SyntaxKind.CharacterLiteralExpression,
                Literal(value));
        }

        public static LiteralExpressionSyntax NumericLiteralExpression(int value)
        {
            return SyntaxFactory.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                Literal(value));
        }

        public static LiteralExpressionSyntax NumericLiteralExpression(uint value)
        {
            return SyntaxFactory.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                Literal(value));
        }

        public static LiteralExpressionSyntax NumericLiteralExpression(sbyte value)
        {
            return SyntaxFactory.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                Literal(value));
        }

        public static LiteralExpressionSyntax NumericLiteralExpression(decimal value)
        {
            return SyntaxFactory.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                Literal(value));
        }

        public static LiteralExpressionSyntax NumericLiteralExpression(double value)
        {
            return SyntaxFactory.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                Literal(value));
        }

        public static LiteralExpressionSyntax NumericLiteralExpression(float value)
        {
            return SyntaxFactory.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                Literal(value));
        }

        public static LiteralExpressionSyntax NumericLiteralExpression(long value)
        {
            return SyntaxFactory.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                Literal(value));
        }

        public static LiteralExpressionSyntax NumericLiteralExpression(ulong value)
        {
            return SyntaxFactory.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                Literal(value));
        }

        internal static LiteralExpressionSyntax NumericLiteralExpression(ulong value, SpecialType numericType)
        {
            return LiteralExpression(ConvertHelpers.ConvertFromUInt64(value, numericType));
        }

        public static LiteralExpressionSyntax TrueLiteralExpression()
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression);
        }

        public static LiteralExpressionSyntax FalseLiteralExpression()
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression);
        }

        public static LiteralExpressionSyntax BooleanLiteralExpression(bool value)
        {
            return (value) ? TrueLiteralExpression() : FalseLiteralExpression();
        }

        public static LiteralExpressionSyntax NullLiteralExpression()
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
        }

        public static LiteralExpressionSyntax DefaultLiteralExpression()
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression);
        }

        public static LiteralExpressionSyntax LiteralExpression(object value)
        {
            if (value == null)
                return NullLiteralExpression();

            if (value is string stringValue)
                return StringLiteralExpression(stringValue);

            if (value is bool boolValue)
                return (boolValue) ? TrueLiteralExpression() : FalseLiteralExpression();

            if (value is char charValue)
                return CharacterLiteralExpression(charValue);

            if (value is sbyte sbyteValue)
                return NumericLiteralExpression(sbyteValue);

            if (value is byte byteValue)
                return NumericLiteralExpression(byteValue);

            if (value is short shortValue)
                return NumericLiteralExpression(shortValue);

            if (value is ushort ushortValue)
                return NumericLiteralExpression(ushortValue);

            if (value is int intValue)
                return NumericLiteralExpression(intValue);

            if (value is uint uintValue)
                return NumericLiteralExpression(uintValue);

            if (value is long longValue)
                return NumericLiteralExpression(longValue);

            if (value is ulong ulongValue)
                return NumericLiteralExpression(ulongValue);

            if (value is decimal decimalValue)
                return NumericLiteralExpression(decimalValue);

            if (value is float floatValue)
                return NumericLiteralExpression(floatValue);

            if (value is double doubleValue)
                return NumericLiteralExpression(doubleValue);

            return StringLiteralExpression(value.ToString());
        }
        #endregion LiteralExpression

        #region Expression
        public static ObjectCreationExpressionSyntax ObjectCreationExpression(TypeSyntax type, ArgumentListSyntax argumentList)
        {
            return SyntaxFactory.ObjectCreationExpression(type, argumentList, default(InitializerExpressionSyntax));
        }

        public static MemberAccessExpressionSyntax SimpleMemberAccessExpression(ExpressionSyntax expression, SimpleNameSyntax name)
        {
            return MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression, name);
        }

        public static MemberAccessExpressionSyntax SimpleMemberAccessExpression(ExpressionSyntax expression, SyntaxToken operatorToken, SimpleNameSyntax name)
        {
            return MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression, operatorToken, name);
        }

        public static InvocationExpressionSyntax SimpleMemberInvocationExpression(ExpressionSyntax expression, SimpleNameSyntax name)
        {
            return InvocationExpression(SimpleMemberAccessExpression(expression, name));
        }

        public static InvocationExpressionSyntax SimpleMemberInvocationExpression(ExpressionSyntax expression, SimpleNameSyntax name, ArgumentSyntax argument)
        {
            return SimpleMemberInvocationExpression(expression, name, ArgumentList(argument));
        }

        public static InvocationExpressionSyntax SimpleMemberInvocationExpression(ExpressionSyntax expression, SimpleNameSyntax name, ArgumentListSyntax argumentList)
        {
            return InvocationExpression(SimpleMemberAccessExpression(expression, name), argumentList);
        }

        public static InvocationExpressionSyntax NameOfExpression(string identifier)
        {
            return NameOfExpression(IdentifierName(identifier));
        }

        public static InvocationExpressionSyntax NameOfExpression(ExpressionSyntax expression)
        {
            string text = SyntaxFacts.GetText(SyntaxKind.NameOfKeyword);

            SyntaxToken identifier = Identifier(
                leading: default(SyntaxTriviaList),
                contextualKind: SyntaxKind.NameOfKeyword,
                text: text,
                valueText: text,
                trailing: default(SyntaxTriviaList));

            return InvocationExpression(
                IdentifierName(identifier),
                ArgumentList(SyntaxFactory.Argument(expression)));
        }

        public static InitializerExpressionSyntax ArrayInitializerExpression(SeparatedSyntaxList<ExpressionSyntax> expressions = default)
        {
            return InitializerExpression(SyntaxKind.ArrayInitializerExpression, expressions);
        }

        public static InitializerExpressionSyntax ArrayInitializerExpression(SyntaxToken openBraceToken, SeparatedSyntaxList<ExpressionSyntax> expressions, SyntaxToken closeBraceToken)
        {
            return InitializerExpression(SyntaxKind.ArrayInitializerExpression, openBraceToken, expressions, closeBraceToken);
        }

        public static InitializerExpressionSyntax CollectionInitializerExpression(SeparatedSyntaxList<ExpressionSyntax> expressions = default)
        {
            return InitializerExpression(SyntaxKind.CollectionInitializerExpression, expressions);
        }

        public static InitializerExpressionSyntax CollectionInitializerExpression(SyntaxToken openBraceToken, SeparatedSyntaxList<ExpressionSyntax> expressions, SyntaxToken closeBraceToken)
        {
            return InitializerExpression(SyntaxKind.CollectionInitializerExpression, openBraceToken, expressions, closeBraceToken);
        }

        public static InitializerExpressionSyntax ComplexElementInitializerExpression(SeparatedSyntaxList<ExpressionSyntax> expressions = default)
        {
            return InitializerExpression(SyntaxKind.ComplexElementInitializerExpression, expressions);
        }

        public static InitializerExpressionSyntax ComplexElementInitializerExpression(SyntaxToken openBraceToken, SeparatedSyntaxList<ExpressionSyntax> expressions, SyntaxToken closeBraceToken)
        {
            return InitializerExpression(SyntaxKind.ComplexElementInitializerExpression, openBraceToken, expressions, closeBraceToken);
        }

        public static InitializerExpressionSyntax ObjectInitializerExpression(SeparatedSyntaxList<ExpressionSyntax> expressions = default)
        {
            return InitializerExpression(SyntaxKind.ObjectInitializerExpression, expressions);
        }

        public static InitializerExpressionSyntax ObjectInitializerExpression(SyntaxToken openBraceToken, SeparatedSyntaxList<ExpressionSyntax> expressions, SyntaxToken closeBraceToken)
        {
            return InitializerExpression(SyntaxKind.ObjectInitializerExpression, openBraceToken, expressions, closeBraceToken);
        }

        public static CheckedExpressionSyntax CheckedExpression(ExpressionSyntax expression)
        {
            return SyntaxFactory.CheckedExpression(SyntaxKind.CheckedExpression, expression);
        }

        public static CheckedExpressionSyntax CheckedExpression(SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken)
        {
            return SyntaxFactory.CheckedExpression(SyntaxKind.CheckedExpression, Token(SyntaxKind.CheckedKeyword), openParenToken, expression, closeParenToken);
        }

        public static CheckedExpressionSyntax UncheckedExpression(ExpressionSyntax expression)
        {
            return SyntaxFactory.CheckedExpression(SyntaxKind.UncheckedExpression, expression);
        }

        public static CheckedExpressionSyntax UncheckedExpression(SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken)
        {
            return SyntaxFactory.CheckedExpression(SyntaxKind.UncheckedExpression, Token(SyntaxKind.UncheckedKeyword), openParenToken, expression, closeParenToken);
        }

        internal static ThrowExpressionSyntax ThrowNewExpression(TypeSyntax exceptionType)
        {
            return ThrowExpression(ObjectCreationExpression(exceptionType, ArgumentList()));
        }
        #endregion Expression

        public static IdentifierNameSyntax VarType()
        {
            return IdentifierName("var");
        }

        public static GenericNameSyntax GenericName(string identifier, TypeSyntax typeArgument)
        {
            return GenericName(Identifier(identifier), typeArgument);
        }

        public static GenericNameSyntax GenericName(SyntaxToken identifier, TypeSyntax typeArgument)
        {
            return SyntaxFactory.GenericName(identifier, TypeArgumentList(typeArgument));
        }

        public static VariableDeclaratorSyntax VariableDeclarator(string identifier, EqualsValueClauseSyntax initializer)
        {
            return VariableDeclarator(Identifier(identifier), initializer);
        }

        public static VariableDeclaratorSyntax VariableDeclarator(SyntaxToken identifier, EqualsValueClauseSyntax initializer)
        {
            return SyntaxFactory.VariableDeclarator(identifier, default(BracketedArgumentListSyntax), initializer);
        }

        public static VariableDeclarationSyntax VariableDeclaration(TypeSyntax type, string identifier, ExpressionSyntax value = null)
        {
            return VariableDeclaration(type, Identifier(identifier), value);
        }

        public static VariableDeclarationSyntax VariableDeclaration(TypeSyntax type, SyntaxToken identifier, ExpressionSyntax value = null)
        {
            if (value != null)
            {
                return VariableDeclaration(type, identifier, EqualsValueClause(value));
            }
            else
            {
                return VariableDeclaration(type, SyntaxFactory.VariableDeclarator(identifier));
            }
        }

        public static VariableDeclarationSyntax VariableDeclaration(TypeSyntax type, SyntaxToken identifier, EqualsValueClauseSyntax initializer)
        {
            return VariableDeclaration(
                type,
                VariableDeclarator(identifier, initializer));
        }

        public static VariableDeclarationSyntax VariableDeclaration(TypeSyntax type, VariableDeclaratorSyntax variable)
        {
            return SyntaxFactory.VariableDeclaration(type, SingletonSeparatedList(variable));
        }

        public static UsingDirectiveSyntax UsingStaticDirective(NameSyntax name)
        {
            return UsingDirective(
                Token(SyntaxKind.StaticKeyword),
                default(NameEqualsSyntax),
                name);
        }

        public static UsingDirectiveSyntax UsingStaticDirective(SyntaxToken usingKeyword, SyntaxToken staticKeyword, NameSyntax name, SyntaxToken semicolonToken)
        {
            return UsingDirective(
                usingKeyword,
                staticKeyword,
                default(NameEqualsSyntax),
                name,
                semicolonToken);
        }

        public static AttributeSyntax Attribute(NameSyntax name, AttributeArgumentSyntax argument)
        {
            return SyntaxFactory.Attribute(
                name,
                AttributeArgumentList(argument));
        }

        public static AttributeArgumentSyntax AttributeArgument(NameEqualsSyntax nameEquals, ExpressionSyntax expression)
        {
            return SyntaxFactory.AttributeArgument(
                nameEquals: nameEquals,
                nameColon: default(NameColonSyntax),
                expression: expression);
        }

        public static AttributeArgumentSyntax AttributeArgument(NameColonSyntax nameColon, ExpressionSyntax expression)
        {
            return SyntaxFactory.AttributeArgument(
                nameEquals: default(NameEqualsSyntax),
                nameColon: nameColon,
                expression: expression);
        }

        public static ArgumentSyntax Argument(NameColonSyntax nameColon, ExpressionSyntax expression)
        {
            return SyntaxFactory.Argument(nameColon, default(SyntaxToken), expression);
        }

        public static ParameterSyntax Parameter(TypeSyntax type, string identifier, ExpressionSyntax @default = null)
        {
            return Parameter(type, Identifier(identifier), @default);
        }

        public static ParameterSyntax Parameter(TypeSyntax type, SyntaxToken identifier, ExpressionSyntax @default = null)
        {
            if (@default != null)
            {
                return Parameter(type, identifier, EqualsValueClause(@default));
            }
            else
            {
                return SyntaxFactory.Parameter(
                    default(SyntaxList<AttributeListSyntax>),
                    default(SyntaxTokenList),
                    type,
                    identifier,
                    default(EqualsValueClauseSyntax));
            }
        }

        public static ParameterSyntax Parameter(TypeSyntax type, SyntaxToken identifier, EqualsValueClauseSyntax @default)
        {
            return SyntaxFactory.Parameter(
                default(SyntaxList<AttributeListSyntax>),
                default(SyntaxTokenList),
                type,
                identifier,
                @default);
        }

        public static TypeParameterConstraintClauseSyntax TypeParameterConstraintClause(
            string name,
            TypeParameterConstraintSyntax typeParameterConstraint)
        {
            return TypeParameterConstraintClause(IdentifierName(name), typeParameterConstraint);
        }

        public static TypeParameterConstraintClauseSyntax TypeParameterConstraintClause(
            IdentifierNameSyntax identifierName,
            TypeParameterConstraintSyntax typeParameterConstraint)
        {
            return SyntaxFactory.TypeParameterConstraintClause(identifierName, SingletonSeparatedList(typeParameterConstraint));
        }

        public static ClassOrStructConstraintSyntax ClassConstraint()
        {
            return ClassOrStructConstraint(SyntaxKind.ClassConstraint, Token(SyntaxKind.ClassKeyword));
        }

        public static ClassOrStructConstraintSyntax StructConstraint()
        {
            return ClassOrStructConstraint(SyntaxKind.StructConstraint, Token(SyntaxKind.StructKeyword));
        }

        public static ConstructorInitializerSyntax BaseConstructorInitializer(ArgumentListSyntax argumentList = null)
        {
            return ConstructorInitializer(SyntaxKind.BaseConstructorInitializer, argumentList);
        }

        public static ConstructorInitializerSyntax BaseConstructorInitializer(SyntaxToken semicolonToken, ArgumentListSyntax argumentList)
        {
            return ConstructorInitializer(SyntaxKind.BaseConstructorInitializer, semicolonToken, Token(SyntaxKind.BaseKeyword), argumentList);
        }

        public static ConstructorInitializerSyntax ThisConstructorInitializer(ArgumentListSyntax argumentList = null)
        {
            return ConstructorInitializer(SyntaxKind.ThisConstructorInitializer, argumentList);
        }

        public static ConstructorInitializerSyntax ThisConstructorInitializer(SyntaxToken semicolonToken, ArgumentListSyntax argumentList)
        {
            return ConstructorInitializer(SyntaxKind.ThisConstructorInitializer, semicolonToken, Token(SyntaxKind.ThisKeyword), argumentList);
        }

        public static SwitchSectionSyntax SwitchSection(SwitchLabelSyntax switchLabel, StatementSyntax statement)
        {
            return SwitchSection(switchLabel, SingletonList(statement));
        }

        public static SwitchSectionSyntax SwitchSection(SwitchLabelSyntax switchLabel, SyntaxList<StatementSyntax> statements)
        {
            return SyntaxFactory.SwitchSection(SingletonList(switchLabel), statements);
        }

        public static SwitchSectionSyntax SwitchSection(SyntaxList<SwitchLabelSyntax> switchLabels, StatementSyntax statement)
        {
            return SyntaxFactory.SwitchSection(switchLabels, SingletonList(statement));
        }

        public static SwitchSectionSyntax DefaultSwitchSection(StatementSyntax statement)
        {
            return DefaultSwitchSection(SingletonList(statement));
        }

        public static SwitchSectionSyntax DefaultSwitchSection(SyntaxList<StatementSyntax> statements)
        {
            return SwitchSection(DefaultSwitchLabel(), statements);
        }

        public static CompilationUnitSyntax CompilationUnit(MemberDeclarationSyntax member)
        {
            return CompilationUnit(
                default(SyntaxList<UsingDirectiveSyntax>),
                member);
        }

        public static CompilationUnitSyntax CompilationUnit(SyntaxList<UsingDirectiveSyntax> usings, MemberDeclarationSyntax member)
        {
            return CompilationUnit(
                usings,
                SingletonList(member));
        }

        public static CompilationUnitSyntax CompilationUnit(SyntaxList<UsingDirectiveSyntax> usings, SyntaxList<MemberDeclarationSyntax> members)
        {
            return SyntaxFactory.CompilationUnit(
                default(SyntaxList<ExternAliasDirectiveSyntax>),
                usings,
                default(SyntaxList<AttributeListSyntax>),
                members);
        }

        internal static SyntaxList<UsingDirectiveSyntax> UsingDirectives(string name)
        {
            return SingletonList(UsingDirective(ParseName(name)));
        }

        internal static SyntaxList<UsingDirectiveSyntax> UsingDirectives(params string[] names)
        {
            return List(names.Select(f => UsingDirective(ParseName(f))));
        }

        public static UnaryPatternSyntax NotPattern(PatternSyntax pattern)
        {
            return UnaryPattern(Token(SyntaxKind.NotKeyword), pattern);
        }

        internal static bool AreEquivalent(
            SyntaxNode node1,
            SyntaxNode node2,
            bool topLevel = false)
        {
            return SyntaxFactory.AreEquivalent(node1, node2, topLevel: topLevel);
        }

        internal static bool AreEquivalent(
            SyntaxNode node1,
            SyntaxNode node2,
            SyntaxNode node3,
            bool topLevel = false)
        {
            return AreEquivalent(node1, node2, topLevel: topLevel)
                && AreEquivalent(node1, node3, topLevel: topLevel);
        }
    }
}
