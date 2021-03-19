// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp
{
    internal static class CSharpTypeAnalysis
    {
        public static TypeAnalysis AnalyzeType(
            VariableDeclarationSyntax variableDeclaration,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default)
        {
            TypeSyntax type = variableDeclaration.Type;

            Debug.Assert(type != null);

            if (type == null)
                return default;

            SeparatedSyntaxList<VariableDeclaratorSyntax> variables = variableDeclaration.Variables;

            Debug.Assert(variables.Any());

            if (!variables.Any())
                return default;

            if (variableDeclaration.IsParentKind(SyntaxKind.FieldDeclaration, SyntaxKind.EventFieldDeclaration))
                return default;

            ExpressionSyntax expression = variables[0].Initializer?.Value?.WalkDownParentheses();

            if (expression == null)
                return default;

            ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(type, cancellationToken);

            if (typeSymbol == null)
                return default;

            SymbolKind kind = typeSymbol.Kind;

            if (kind == SymbolKind.ErrorType)
                return default;

            if (kind == SymbolKind.DynamicType)
                return new TypeAnalysis(typeSymbol, TypeAnalysisFlags.Dynamic);

            var flags = TypeAnalysisFlags.None;

            if (type.IsVar)
            {
                flags |= TypeAnalysisFlags.Implicit;

                if (typeSymbol.SupportsExplicitDeclaration())
                    flags |= TypeAnalysisFlags.SupportsExplicit;
            }
            else
            {
                flags |= TypeAnalysisFlags.Explicit;

                if (variables.Count == 1
                    && (variableDeclaration.Parent as LocalDeclarationStatementSyntax)?.IsConst != true
                    && !expression.IsKind(SyntaxKind.NullLiteralExpression, SyntaxKind.DefaultLiteralExpression))
                {
                    flags |= TypeAnalysisFlags.SupportsImplicit;
                }
            }

            switch (expression.Kind())
            {
                case SyntaxKind.ObjectCreationExpression:
                case SyntaxKind.ArrayCreationExpression:
                case SyntaxKind.CastExpression:
                case SyntaxKind.AsExpression:
                case SyntaxKind.ThisExpression:
                case SyntaxKind.DefaultExpression:
                    {
                        flags |= TypeAnalysisFlags.TypeObvious;
                        break;
                    }
                case SyntaxKind.SimpleMemberAccessExpression:
                    {
                        ISymbol symbol = semanticModel.GetSymbol(expression, cancellationToken);

                        if (symbol?.Kind == SymbolKind.Field
                            && symbol.ContainingType?.TypeKind == TypeKind.Enum)
                        {
                            flags |= TypeAnalysisFlags.TypeObvious;
                        }

                        break;
                    }
            }

            return new TypeAnalysis(typeSymbol, flags);
        }

        public static bool IsImplicitThatCanBeExplicit(
            VariableDeclarationSyntax variableDeclaration,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default)
        {
            return IsImplicitThatCanBeExplicit(variableDeclaration, semanticModel, TypeAppearance.None, cancellationToken);
        }

        public static bool IsImplicitThatCanBeExplicit(
            VariableDeclarationSyntax variableDeclaration,
            SemanticModel semanticModel,
            TypeAppearance typeAppearance,
            CancellationToken cancellationToken = default)
        {
            TypeSyntax type = variableDeclaration.Type;

            Debug.Assert(type != null);

            if (type == null)
                return false;

            if (!type.IsVar)
                return false;

            if (variableDeclaration.IsParentKind(SyntaxKind.FieldDeclaration, SyntaxKind.EventFieldDeclaration))
                return false;

            Debug.Assert(variableDeclaration.Variables.Any());

            ExpressionSyntax expression = variableDeclaration
                .Variables
                .FirstOrDefault()?
                .Initializer?
                .Value?
                .WalkDownParentheses();

            if (expression == null)
                return false;

            ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(type, cancellationToken);

            if (typeSymbol == null)
                return false;

            if (typeSymbol.IsKind(SymbolKind.ErrorType, SymbolKind.DynamicType))
                return false;

            if (!typeSymbol.SupportsExplicitDeclaration())
                return false;

            switch (typeAppearance)
            {
                case TypeAppearance.Obvious:
                    return IsTypeObvious(expression, typeSymbol, semanticModel, cancellationToken);
                case TypeAppearance.NotObvious:
                    return !IsTypeObvious(expression, typeSymbol, semanticModel, cancellationToken);
            }

            Debug.Assert(typeAppearance == TypeAppearance.None, typeAppearance.ToString());

            return true;
        }

        public static bool IsExplicitThatCanBeImplicit(
            VariableDeclarationSyntax variableDeclaration,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default)
        {
            return IsExplicitThatCanBeImplicit(variableDeclaration, semanticModel, TypeAppearance.None, cancellationToken);
        }

        public static bool IsExplicitThatCanBeImplicit(
            VariableDeclarationSyntax variableDeclaration,
            SemanticModel semanticModel,
            TypeAppearance typeAppearance,
            CancellationToken cancellationToken = default)
        {
            TypeSyntax type = variableDeclaration.Type;

            Debug.Assert(type != null);

            if (type == null)
                return false;

            if (type.IsVar)
                return false;

            switch (variableDeclaration.Parent.Kind())
            {
                case SyntaxKind.FieldDeclaration:
                case SyntaxKind.EventFieldDeclaration:
                    {
                        return false;
                    }
                case SyntaxKind.LocalDeclarationStatement:
                    {
                        if (((LocalDeclarationStatementSyntax)variableDeclaration.Parent).IsConst)
                            return false;

                        break;
                    }
            }

            Debug.Assert(variableDeclaration.Variables.Any());

            ExpressionSyntax expression = variableDeclaration
                .Variables
                .SingleOrDefault(shouldThrow: false)?
                .Initializer?
                .Value?
                .WalkDownParentheses();

            if (expression == null)
                return false;

            if (expression.IsKind(SyntaxKind.NullLiteralExpression, SyntaxKind.DefaultLiteralExpression))
                return false;

            ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(type, cancellationToken);

            if (typeSymbol == null)
                return false;

            if (typeSymbol.IsKind(SymbolKind.ErrorType, SymbolKind.DynamicType))
                return false;

            if (!SymbolEqualityComparer.Default.Equals(typeSymbol, semanticModel.GetTypeSymbol(expression, cancellationToken)))
                return false;

            switch (typeAppearance)
            {
                case TypeAppearance.Obvious:
                    return IsTypeObvious(expression, typeSymbol, semanticModel, cancellationToken);
                case TypeAppearance.NotObvious:
                    return !IsTypeObvious(expression, typeSymbol, semanticModel, cancellationToken);
            }

            Debug.Assert(typeAppearance == TypeAppearance.None, typeAppearance.ToString());

            return true;
        }

        public static bool IsTypeObvious(ExpressionSyntax expression, ITypeSymbol typeSymbol, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            switch (expression.Kind())
            {
                case SyntaxKind.StringLiteralExpression:
                case SyntaxKind.CharacterLiteralExpression:
                case SyntaxKind.TrueLiteralExpression:
                case SyntaxKind.FalseLiteralExpression:
                case SyntaxKind.ObjectCreationExpression:
                case SyntaxKind.ArrayCreationExpression:
                case SyntaxKind.CastExpression:
                case SyntaxKind.AsExpression:
                case SyntaxKind.ThisExpression:
                case SyntaxKind.DefaultExpression:
                    {
                        return true;
                    }
                case SyntaxKind.ImplicitArrayCreationExpression:
                    {
                        var implicitArrayCreation = (ImplicitArrayCreationExpressionSyntax)expression;

                        SeparatedSyntaxList<ExpressionSyntax> expressions = implicitArrayCreation.Initializer?.Expressions ?? default;

                        if (!expressions.Any())
                            return false;

                        foreach (ExpressionSyntax expression2 in expressions)
                        {
                            if (!IsTypeObvious(expression2, typeSymbol, semanticModel, cancellationToken))
                                return false;
                        }

                        return true;
                    }
                case SyntaxKind.SimpleMemberAccessExpression:
                    {
                        ISymbol symbol = semanticModel.GetSymbol(expression, cancellationToken);

                        return symbol?.Kind == SymbolKind.Field
                            && symbol.ContainingType?.TypeKind == TypeKind.Enum;
                    }
                case SyntaxKind.InvocationExpression:
                    {
                        if (typeSymbol != null)
                        {
                            var invocationExpression = (InvocationExpressionSyntax)expression;
                            if (invocationExpression.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                            {
                                ISymbol symbol = semanticModel.GetSymbol(expression, cancellationToken);

                                if (symbol?.IsStatic == true
                                    && string.Equals(symbol.Name, "Parse", StringComparison.Ordinal)
                                    && SymbolEqualityComparer.IncludeNullability.Equals(
                                        ((IMethodSymbol)symbol).ReturnType,
                                        typeSymbol))
                                {
                                    var simpleMemberAccess = (MemberAccessExpressionSyntax)invocationExpression.Expression;

                                    ISymbol symbol2 = semanticModel.GetSymbol(simpleMemberAccess.Expression, cancellationToken);

                                    if (SymbolEqualityComparer.Default.Equals(symbol2, typeSymbol)
                                        && semanticModel.GetAliasInfo(simpleMemberAccess.Expression, cancellationToken) == null)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }

                        break;
                    }
            }

            return false;
        }

        public static TypeAnalysis AnalyzeType(
            DeclarationExpressionSyntax declarationExpression,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default)
        {
            TypeSyntax type = declarationExpression.Type;

            if (type == null)
                return default;

            if (!(declarationExpression.Designation is SingleVariableDesignationSyntax singleVariableDesignation))
                return default;

            if (!(semanticModel.GetDeclaredSymbol(singleVariableDesignation, cancellationToken) is ILocalSymbol localSymbol))
                return default;

            ITypeSymbol typeSymbol = localSymbol.Type;

            Debug.Assert(typeSymbol != null);

            if (typeSymbol == null)
                return default;

            SymbolKind kind = typeSymbol.Kind;

            if (kind == SymbolKind.ErrorType)
                return default;

            if (kind == SymbolKind.DynamicType)
                return new TypeAnalysis(typeSymbol, TypeAnalysisFlags.Dynamic);

            var flags = TypeAnalysisFlags.None;

            if (type.IsVar)
            {
                flags |= TypeAnalysisFlags.Implicit;

                if (typeSymbol.SupportsExplicitDeclaration())
                    flags |= TypeAnalysisFlags.SupportsExplicit;
            }
            else
            {
                flags |= TypeAnalysisFlags.Explicit;
                flags |= TypeAnalysisFlags.SupportsImplicit;
            }

            return new TypeAnalysis(typeSymbol, flags);
        }

        public static bool IsImplicitThatCanBeExplicit(
            DeclarationExpressionSyntax declarationExpression,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default)
        {
            TypeSyntax type = declarationExpression.Type;

            Debug.Assert(type != null);

            if (type == null)
                return false;

            if (!type.IsVar)
                return false;

            switch (declarationExpression.Designation)
            {
                case SingleVariableDesignationSyntax singleVariableDesignation:
                    {
                        return IsLocalThatSupportsExplicitDeclaration(singleVariableDesignation);
                    }
                case DiscardDesignationSyntax discardDesignation:
                    {
                        return IsLocalThatSupportsExplicitDeclaration(discardDesignation);
                    }
                case ParenthesizedVariableDesignationSyntax parenthesizedVariableDesignation:
                    {
                        foreach (VariableDesignationSyntax variableDesignation in parenthesizedVariableDesignation.Variables)
                        {
                            if (!(variableDesignation is SingleVariableDesignationSyntax singleVariableDesignation2))
                                return false;

                            if (!IsLocalThatSupportsExplicitDeclaration(singleVariableDesignation2))
                                return false;
                        }

                        return true;
                    }
                default:
                    {
                        Debug.Fail(declarationExpression.Designation.Kind().ToString());
                        return false;
                    }
            }

            bool IsLocalThatSupportsExplicitDeclaration(VariableDesignationSyntax variableDesignation)
            {
                if (!(semanticModel.GetDeclaredSymbol(variableDesignation, cancellationToken) is ILocalSymbol localSymbol))
                    return false;

                ITypeSymbol typeSymbol = localSymbol.Type;

                if (typeSymbol.IsKind(SymbolKind.ErrorType, SymbolKind.DynamicType))
                    return false;

                return typeSymbol.SupportsExplicitDeclaration();
            }
        }

        public static bool IsExplicitThatCanBeImplicit(
            DeclarationExpressionSyntax declarationExpression,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default)
        {
            TypeSyntax type = declarationExpression.Type;

            if (type == null)
                return false;

            if (type.IsVar)
                return false;

            if (!(declarationExpression.Designation is SingleVariableDesignationSyntax singleVariableDesignation))
                return false;

            if (!(semanticModel.GetDeclaredSymbol(singleVariableDesignation, cancellationToken) is ILocalSymbol localSymbol))
                return false;

            ITypeSymbol typeSymbol = localSymbol.Type;

            Debug.Assert(typeSymbol != null);

            if (typeSymbol == null)
                return false;

            if (typeSymbol.IsKind(SymbolKind.ErrorType, SymbolKind.DynamicType))
                return false;

            return true;
        }

        public static bool IsExplicitThatCanBeImplicit(
            TupleExpressionSyntax tupleExpression,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default)
        {
            return IsExplicitThatCanBeImplicit(tupleExpression, semanticModel, TypeAppearance.None, cancellationToken);
        }

        public static bool IsExplicitThatCanBeImplicit(
            TupleExpressionSyntax tupleExpression,
            SemanticModel semanticModel,
            TypeAppearance typeAppearance,
            CancellationToken cancellationToken = default)
        {
            switch (tupleExpression.Parent.Kind())
            {
                case SyntaxKind.SimpleAssignmentExpression:
                    {
                        var assignment = (AssignmentExpressionSyntax)tupleExpression.Parent;

                        return IsExplicitThatCanBeImplicit(tupleExpression, assignment, typeAppearance, semanticModel, cancellationToken);
                    }
                case SyntaxKind.ForEachVariableStatement:
                    {
                        var forEachStatement = (ForEachVariableStatementSyntax)tupleExpression.Parent;

                        return IsExplicitThatCanBeImplicit(tupleExpression, forEachStatement, semanticModel);
                    }
                case SyntaxKind.Argument:
                case SyntaxKind.ArrayInitializerExpression:
                case SyntaxKind.ReturnStatement:
                case SyntaxKind.YieldReturnStatement:
                case SyntaxKind.ArrowExpressionClause:
                case SyntaxKind.SimpleLambdaExpression:
                case SyntaxKind.ParenthesizedLambdaExpression:
                case SyntaxKind.CollectionInitializerExpression:
                case SyntaxKind.EqualsValueClause:
                case SyntaxKind.SimpleMemberAccessExpression:
                    {
                        Debug.Assert(!tupleExpression.Arguments.Any(f => f.Expression.IsKind(SyntaxKind.DeclarationExpression)), tupleExpression.ToString());
                        return false;
                    }
                default:
                    {
                        Debug.Fail(tupleExpression.Parent.Kind().ToString());
                        return false;
                    }
            }
        }

        private static bool IsExplicitThatCanBeImplicit(
            TupleExpressionSyntax tupleExpression,
            AssignmentExpressionSyntax assignment,
            TypeAppearance typeAppearance,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            ExpressionSyntax expression = assignment.Right.WalkDownParentheses();

            if (expression?.IsMissing != false)
                return false;

            if (expression.IsKind(SyntaxKind.NullLiteralExpression, SyntaxKind.DefaultLiteralExpression))
                return false;

            ITypeSymbol tupleTypeSymbol = semanticModel.GetTypeSymbol(tupleExpression, cancellationToken);

            if (tupleTypeSymbol == null)
                return false;

            ITypeSymbol expressionTypeSymbol = semanticModel.GetTypeSymbol(expression, cancellationToken);

            if (tupleTypeSymbol.IsTupleType
                && expressionTypeSymbol.IsTupleType)
            {
                var tupleNamedTypeSymbol = (INamedTypeSymbol)tupleTypeSymbol;
                var expressionNamedTypeSymbol = (INamedTypeSymbol)expressionTypeSymbol;

                if (!SymbolEqualityComparer.Default.Equals(
                    tupleNamedTypeSymbol.TupleUnderlyingType ?? tupleNamedTypeSymbol,
                    expressionNamedTypeSymbol.TupleUnderlyingType ?? expressionNamedTypeSymbol))
                {
                    return false;
                }
            }
            else if (!SymbolEqualityComparer.Default.Equals(tupleTypeSymbol, expressionTypeSymbol))
            {
                return false;
            }

            foreach (ArgumentSyntax argument in tupleExpression.Arguments)
            {
                if (!(argument.Expression is DeclarationExpressionSyntax declarationExpression))
                    return false;

                TypeSyntax type = declarationExpression.Type;

                if (type == null)
                    return false;

                if (!(declarationExpression.Designation is SingleVariableDesignationSyntax singleVariableDesignation))
                    return false;

                if (!(semanticModel.GetDeclaredSymbol(singleVariableDesignation, cancellationToken) is ILocalSymbol localSymbol))
                    return false;

                ITypeSymbol typeSymbol = localSymbol.Type;

                Debug.Assert(typeSymbol != null);

                if (typeSymbol == null)
                    return false;

                if (typeSymbol.IsKind(SymbolKind.ErrorType, SymbolKind.DynamicType))
                    return false;
            }

            switch (typeAppearance)
            {
                case TypeAppearance.Obvious:
                    return IsTypeObvious(expression, null, semanticModel, cancellationToken);
                case TypeAppearance.NotObvious:
                    return !IsTypeObvious(expression, null, semanticModel, cancellationToken);
            }

            Debug.Assert(typeAppearance == TypeAppearance.None, typeAppearance.ToString());

            return true;
        }

        public static TypeAnalysis AnalyzeType(ForEachStatementSyntax forEachStatement, SemanticModel semanticModel)
        {
            TypeSyntax type = forEachStatement.Type;

            Debug.Assert(type != null);

            if (type == null)
                return default;

            ForEachStatementInfo info = semanticModel.GetForEachStatementInfo(forEachStatement);

            ITypeSymbol typeSymbol = info.ElementType;

            if (typeSymbol == null)
                return default;

            SymbolKind kind = typeSymbol.Kind;

            if (kind == SymbolKind.ErrorType)
                return default;

            if (kind == SymbolKind.DynamicType)
                return new TypeAnalysis(typeSymbol, TypeAnalysisFlags.Dynamic);

            var flags = TypeAnalysisFlags.None;

            if (type.IsVar)
            {
                flags |= TypeAnalysisFlags.Implicit;

                if (typeSymbol.SupportsExplicitDeclaration())
                    flags |= TypeAnalysisFlags.SupportsExplicit;
            }
            else
            {
                flags |= TypeAnalysisFlags.Explicit;

                if (info.ElementConversion.IsIdentity)
                    flags |= TypeAnalysisFlags.SupportsImplicit;
            }

            return new TypeAnalysis(typeSymbol, flags);
        }

        public static TypeAnalysis AnalyzeType(ForEachVariableStatementSyntax forEachStatement, SemanticModel semanticModel)
        {
            var flags = TypeAnalysisFlags.None;

            switch (forEachStatement.Variable)
            {
                case DeclarationExpressionSyntax declarationExpression:
                    {
                        TypeSyntax type = declarationExpression.Type;

                        Debug.Assert(type != null);

                        if (type == null)
                            return default;

                        Debug.Assert(type.IsVar, type.Kind().ToString());

                        if (type.IsVar)
                            flags |= TypeAnalysisFlags.Implicit;

                        break;
                    }
                case TupleExpressionSyntax tupleExpression:
                    {
                        foreach (ArgumentSyntax argument in tupleExpression.Arguments)
                        {
                            Debug.Assert(argument.Expression.IsKind(SyntaxKind.DeclarationExpression), argument.Expression.Kind().ToString());

                            if (argument.Expression is DeclarationExpressionSyntax declarationExpression)
                            {
                                TypeSyntax type = declarationExpression.Type;

                                if (type.IsVar)
                                {
                                    flags |= TypeAnalysisFlags.Implicit;
                                }
                                else
                                {
                                    flags |= TypeAnalysisFlags.Explicit;
                                }
                            }
                        }

                        break;
                    }
                default:
                    {
                        Debug.Fail(forEachStatement.Variable.Kind().ToString());
                        return default;
                    }
            }

            ForEachStatementInfo info = semanticModel.GetForEachStatementInfo(forEachStatement);

            ITypeSymbol typeSymbol = info.ElementType;

            if (typeSymbol == null)
                return default;

            SymbolKind kind = typeSymbol.Kind;

            if (kind == SymbolKind.ErrorType)
                return default;

            if (kind == SymbolKind.DynamicType)
                return new TypeAnalysis(typeSymbol, TypeAnalysisFlags.Dynamic);

            if ((flags & TypeAnalysisFlags.Implicit) != 0
                && typeSymbol.SupportsExplicitDeclaration())
            {
                flags |= TypeAnalysisFlags.SupportsExplicit;
            }

            if ((flags & TypeAnalysisFlags.Explicit) != 0
                && info.ElementConversion.IsIdentity)
            {
                flags |= TypeAnalysisFlags.SupportsImplicit;
            }

            return new TypeAnalysis(typeSymbol, flags);
        }

        public static bool IsImplicitThatCanBeExplicit(ForEachStatementSyntax forEachStatement, SemanticModel semanticModel)
        {
            TypeSyntax type = forEachStatement.Type;

            Debug.Assert(type != null);

            if (type == null)
                return false;

            if (!type.IsVar)
                return false;

            ForEachStatementInfo info = semanticModel.GetForEachStatementInfo(forEachStatement);

            ITypeSymbol typeSymbol = info.ElementType;

            if (typeSymbol == null)
                return false;

            if (typeSymbol.IsKind(SymbolKind.ErrorType, SymbolKind.DynamicType))
                return false;

            return typeSymbol.SupportsExplicitDeclaration();
        }

        public static bool IsImplicitThatCanBeExplicit(ForEachVariableStatementSyntax forEachStatement, SemanticModel semanticModel)
        {
            ExpressionSyntax variable = forEachStatement.Variable;

            if (!(variable is DeclarationExpressionSyntax declarationExpression))
                return false;

            TypeSyntax type = declarationExpression.Type;

            Debug.Assert(type.IsVar, type.Kind().ToString());

            if (!type.IsVar)
                return false;

            ForEachStatementInfo info = semanticModel.GetForEachStatementInfo(forEachStatement);

            ITypeSymbol typeSymbol = info.ElementType;

            if (typeSymbol == null)
                return false;

            if (typeSymbol.IsKind(SymbolKind.ErrorType, SymbolKind.DynamicType))
                return false;

            return typeSymbol.SupportsExplicitDeclaration();
        }

        public static bool IsExplicitThatCanBeImplicit(ForEachStatementSyntax forEachStatement, SemanticModel semanticModel)
        {
            TypeSyntax type = forEachStatement.Type;

            Debug.Assert(type != null);

            if (type == null)
                return false;

            if (type.IsVar)
                return false;

            ForEachStatementInfo info = semanticModel.GetForEachStatementInfo(forEachStatement);

            ITypeSymbol typeSymbol = info.ElementType;

            if (typeSymbol == null)
                return false;

            if (typeSymbol.IsKind(SymbolKind.ErrorType, SymbolKind.DynamicType))
                return false;

            return info.ElementConversion.IsIdentity;
        }

        public static bool IsExplicitThatCanBeImplicit(ForEachVariableStatementSyntax forEachStatement, SemanticModel semanticModel)
        {
            ExpressionSyntax variable = forEachStatement.Variable;

            if (!(variable is TupleExpressionSyntax tupleExpression))
                return false;

            return IsExplicitThatCanBeImplicit(tupleExpression, forEachStatement, semanticModel);
        }

        private static bool IsExplicitThatCanBeImplicit(
            TupleExpressionSyntax tupleExpression,
            ForEachVariableStatementSyntax forEachStatement,
            SemanticModel semanticModel)
        {
            var isAllVar = true;

            foreach (ArgumentSyntax argument in tupleExpression.Arguments)
            {
                if (!(argument.Expression is DeclarationExpressionSyntax declarationExpression))
                    return false;

                TypeSyntax type = declarationExpression.Type;

                if (type == null)
                    return false;

                if (!type.IsVar)
                {
                    isAllVar = false;
                    break;
                }
            }

            if (isAllVar)
                return false;

            ForEachStatementInfo info = semanticModel.GetForEachStatementInfo(forEachStatement);

            ITypeSymbol typeSymbol = info.ElementType;

            if (typeSymbol == null)
                return false;

            if (typeSymbol.IsKind(SymbolKind.ErrorType, SymbolKind.DynamicType))
                return false;

            return info.ElementConversion.IsIdentity;
        }
    }
}
