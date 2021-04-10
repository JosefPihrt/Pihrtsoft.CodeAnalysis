// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp;
using Roslynator.CSharp.SyntaxWalkers;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UseAutoPropertyAnalyzer : BaseDiagnosticAnalyzer
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
                        DiagnosticRules.UseAutoProperty,
                        DiagnosticRules.UseAutoPropertyFadeOut);
                }

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    if (DiagnosticRules.UseAutoProperty.IsEffective(c))
                        AnalyzePropertyDeclaration(c);
                },
                SyntaxKind.PropertyDeclaration);
        }

        private static void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
        {
            var property = (PropertyDeclarationSyntax)context.Node;

            if (property.ContainsDiagnostics)
                return;

            SemanticModel semanticModel = context.SemanticModel;
            CancellationToken cancellationToken = context.CancellationToken;

            IFieldSymbol fieldSymbol = null;

            AccessorDeclarationSyntax getter = null;
            AccessorDeclarationSyntax setter = null;

            ArrowExpressionClauseSyntax expressionBody = property.ExpressionBody;

            if (expressionBody != null)
            {
                IdentifierNameSyntax identifierName = GetIdentifierNameFromExpression(expressionBody.Expression);

                if (identifierName != null)
                    fieldSymbol = GetBackingFieldSymbol(identifierName, semanticModel, cancellationToken);
            }
            else
            {
                getter = property.Getter();

                if (getter != null)
                {
                    setter = property.Setter();

                    if (setter != null)
                    {
                        fieldSymbol = GetBackingFieldSymbol(getter, setter, semanticModel, cancellationToken);
                    }
                    else
                    {
                        IdentifierNameSyntax identifierName = GetIdentifierNameFromGetter(getter);

                        if (identifierName != null)
                            fieldSymbol = GetBackingFieldSymbol(identifierName, semanticModel, cancellationToken);
                    }
                }
            }

            if (fieldSymbol == null)
                return;

            var variableDeclarator = (VariableDeclaratorSyntax)fieldSymbol.GetSyntax(cancellationToken);

            if (variableDeclarator.SyntaxTree != property.SyntaxTree)
                return;

            if (!CheckPreprocessorDirectives(property))
                return;

            if (!CheckPreprocessorDirectives(variableDeclarator))
                return;

            IPropertySymbol propertySymbol = semanticModel.GetDeclaredSymbol(property, cancellationToken);

            if (propertySymbol?.IsStatic != fieldSymbol.IsStatic)
                return;

            if (!propertySymbol.ExplicitInterfaceImplementations.IsDefaultOrEmpty)
                return;

            if (!SymbolEqualityComparer.Default.Equals(propertySymbol.Type, fieldSymbol.Type))
                return;

            if (!SymbolEqualityComparer.Default.Equals(propertySymbol.ContainingType, fieldSymbol.ContainingType))
                return;

            if (setter == null
                && propertySymbol.IsOverride
                && propertySymbol.OverriddenProperty?.SetMethod != null)
            {
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();

            foreach (AttributeData attributeData in fieldSymbol.GetAttributes())
            {
                if (attributeData.AttributeClass.HasMetadataName(MetadataNames.System_NonSerializedAttribute))
                    return;
            }

            if (HasStructLayoutAttributeWithExplicitKind(propertySymbol.ContainingType))
                return;

            if (!IsFixableBackingField(property, propertySymbol, fieldSymbol, semanticModel, cancellationToken))
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UseAutoProperty, property.Identifier);

            if (property.ExpressionBody != null)
            {
                DiagnosticHelpers.ReportNode(context, DiagnosticRules.UseAutoPropertyFadeOut, property.ExpressionBody);
            }
            else
            {
                if (getter != null)
                    FadeOut(getter);

                if (setter != null)
                    FadeOut(setter);
            }

            void FadeOut(AccessorDeclarationSyntax accessor)
            {
                BlockSyntax body = accessor.Body;

                if (body != null)
                {
                    switch (body.Statements[0])
                    {
                        case ReturnStatementSyntax returnStatement:
                            {
                                DiagnosticHelpers.ReportToken(context, DiagnosticRules.UseAutoPropertyFadeOut, returnStatement.ReturnKeyword);
                                DiagnosticHelpers.ReportNode(context, DiagnosticRules.UseAutoPropertyFadeOut, returnStatement.Expression);
                                break;
                            }
                        case ExpressionStatementSyntax expressionStatement:
                            {
                                DiagnosticHelpers.ReportNode(context, DiagnosticRules.UseAutoPropertyFadeOut, expressionStatement.Expression);
                                break;
                            }
                    }

                    CSharpDiagnosticHelpers.ReportBraces(context, DiagnosticRules.UseAutoPropertyFadeOut, body);
                }
                else
                {
                    DiagnosticHelpers.ReportNode(context, DiagnosticRules.UseAutoPropertyFadeOut, accessor.ExpressionBody);
                }
            }
        }

        private static bool IsFixableBackingField(
            PropertyDeclarationSyntax propertyDeclaration,
            IPropertySymbol propertySymbol,
            IFieldSymbol fieldSymbol,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            INamedTypeSymbol containingType = fieldSymbol.ContainingType;

            bool shouldSearchForReferenceInInstanceConstructor = !containingType.IsSealed
                && !propertySymbol.IsStatic
                && (propertySymbol.IsVirtual || propertySymbol.IsOverride);

            UseAutoPropertyWalker walker = UseAutoPropertyWalker.GetInstance();

            var isFixable = false;

            ImmutableArray<SyntaxReference> syntaxReferences = containingType.DeclaringSyntaxReferences;

            if (syntaxReferences.Length == 1)
            {
                walker.SetValues(fieldSymbol, shouldSearchForReferenceInInstanceConstructor, semanticModel, cancellationToken);

                walker.Visit(propertyDeclaration.Parent);

                isFixable = walker.Success;
            }
            else
            {
                foreach (SyntaxReference syntaxReference in syntaxReferences)
                {
                    SyntaxNode typeDeclaration = syntaxReference.GetSyntax(cancellationToken);

                    if (typeDeclaration.SyntaxTree != semanticModel.SyntaxTree)
                    {
                        isFixable = false;
                        break;
                    }

                    walker.SetValues(fieldSymbol, shouldSearchForReferenceInInstanceConstructor, semanticModel, cancellationToken);

                    walker.Visit(typeDeclaration);

                    isFixable = walker.Success;

                    if (!isFixable)
                        break;
                }
            }

            UseAutoPropertyWalker.Free(walker);

            return isFixable;
        }

        private static IFieldSymbol GetBackingFieldSymbol(
            IdentifierNameSyntax identifierName,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            ISymbol symbol = semanticModel.GetSymbol(identifierName, cancellationToken);

            if (symbol?.DeclaredAccessibility == Accessibility.Private
                && symbol.Kind == SymbolKind.Field)
            {
                var fieldSymbol = (IFieldSymbol)symbol;

                if (fieldSymbol.IsReadOnly
                    && !fieldSymbol.IsVolatile)
                {
                    return fieldSymbol;
                }
            }

            return null;
        }

        private static IFieldSymbol GetBackingFieldSymbol(
            AccessorDeclarationSyntax getter,
            AccessorDeclarationSyntax setter,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            IdentifierNameSyntax getterName = GetIdentifierNameFromGetter(getter);

            if (getterName == null)
                return null;

            IdentifierNameSyntax setterName = GetIdentifierNameFromSetter(setter);

            if (setterName == null)
                return null;

            ISymbol getterSymbol = semanticModel.GetSymbol(getterName, cancellationToken);

            if (getterSymbol?.DeclaredAccessibility != Accessibility.Private)
                return null;

            if (getterSymbol.Kind != SymbolKind.Field)
                return null;

            var fieldSymbol = (IFieldSymbol)getterSymbol;

            if (fieldSymbol.IsVolatile)
                return null;

            ISymbol setterSymbol = semanticModel.GetSymbol(setterName, cancellationToken);

            if (SymbolEqualityComparer.Default.Equals(fieldSymbol, setterSymbol))
                return fieldSymbol;

            return null;
        }

        private static IdentifierNameSyntax GetIdentifierNameFromGetter(AccessorDeclarationSyntax getter)
        {
            if (getter != null)
            {
                BlockSyntax body = getter.Body;

                if (body != null)
                {
                    if (body.Statements.SingleOrDefault(shouldThrow: false) is ReturnStatementSyntax returnStatement)
                    {
                        return GetIdentifierNameFromExpression(returnStatement.Expression);
                    }
                }
                else
                {
                    return GetIdentifierNameFromExpression(getter.ExpressionBody?.Expression);
                }
            }

            return null;
        }

        private static IdentifierNameSyntax GetIdentifierNameFromSetter(AccessorDeclarationSyntax setter)
        {
            if (setter != null)
            {
                BlockSyntax body = setter.Body;

                if (body != null)
                {
                    if (body.Statements.SingleOrDefault(shouldThrow: false) is ExpressionStatementSyntax expressionStatement)
                    {
                        return GetIdentifierName(expressionStatement.Expression);
                    }
                }
                else
                {
                    return GetIdentifierName(setter.ExpressionBody?.Expression);
                }
            }

            return null;

            static IdentifierNameSyntax GetIdentifierName(ExpressionSyntax expression)
            {
                if (expression?.Kind() == SyntaxKind.SimpleAssignmentExpression)
                {
                    var assignment = (AssignmentExpressionSyntax)expression;
                    ExpressionSyntax right = assignment.Right;

                    if (right.IsKind(SyntaxKind.IdentifierName)
                        && ((IdentifierNameSyntax)right).Identifier.ValueText == "value")
                    {
                        return GetIdentifierNameFromExpression(assignment.Left);
                    }
                }

                return null;
            }
        }

        private static IdentifierNameSyntax GetIdentifierNameFromExpression(ExpressionSyntax expression)
        {
            switch (expression?.Kind())
            {
                case SyntaxKind.IdentifierName:
                    {
                        return (IdentifierNameSyntax)expression;
                    }
                case SyntaxKind.SimpleMemberAccessExpression:
                    {
                        var memberAccess = (MemberAccessExpressionSyntax)expression;

                        if (memberAccess.Expression?.Kind() == SyntaxKind.ThisExpression)
                        {
                            SimpleNameSyntax name = memberAccess.Name;

                            if (name.IsKind(SyntaxKind.IdentifierName))
                                return (IdentifierNameSyntax)name;
                        }

                        break;
                    }
            }

            return null;
        }

        private static bool HasStructLayoutAttributeWithExplicitKind(INamedTypeSymbol typeSymbol)
        {
            AttributeData attribute = typeSymbol.GetAttribute(MetadataNames.System_Runtime_InteropServices_StructLayoutAttribute);

            if (attribute != null)
            {
                TypedConstant typedConstant = attribute.ConstructorArguments.SingleOrDefault(shouldThrow: false);

                return typedConstant.Type?.HasMetadataName(MetadataNames.System_Runtime_InteropServices_LayoutKind) == true
                    && (((LayoutKind)typedConstant.Value) == LayoutKind.Explicit);
            }

            return false;
        }

        private static bool CheckPreprocessorDirectives(PropertyDeclarationSyntax property)
        {
            ArrowExpressionClauseSyntax expressionBody = property.ExpressionBody;

            if (expressionBody != null)
            {
                if (expressionBody.SpanContainsDirectives())
                    return false;
            }
            else if (property.AccessorList.Accessors.Any(f => f.SpanContainsDirectives()))
            {
                return false;
            }

            return true;
        }

        private static bool CheckPreprocessorDirectives(VariableDeclaratorSyntax declarator)
        {
            var variableDeclaration = (VariableDeclarationSyntax)declarator.Parent;

            if (variableDeclaration.Variables.Count == 1)
            {
                if (variableDeclaration.Parent.SpanContainsDirectives())
                    return false;
            }
            else if (declarator.SpanContainsDirectives())
            {
                return false;
            }

            return true;
        }

        private class UseAutoPropertyWalker : CSharpSyntaxNodeWalker
        {
            private bool _isInInstanceConstructor;

            public IFieldSymbol FieldSymbol { get; private set; }

            public bool ShouldSearchForReferenceInInstanceConstructor { get; private set; }

            public SemanticModel SemanticModel { get; private set; }

            public CancellationToken CancellationToken { get; private set; }

            public bool Success { get; set; } = true;

            protected override bool ShouldVisit => Success;

            public void SetValues(
                IFieldSymbol fieldSymbol,
                bool shouldSearchForReferenceInInstanceConstructor,
                SemanticModel semanticModel,
                CancellationToken cancellationToken)
            {
                FieldSymbol = fieldSymbol;
                ShouldSearchForReferenceInInstanceConstructor = shouldSearchForReferenceInInstanceConstructor;
                SemanticModel = semanticModel;
                CancellationToken = cancellationToken;
                Success = true;
            }

            public override void VisitArgument(ArgumentSyntax node)
            {
                CancellationToken.ThrowIfCancellationRequested();

                if (node.RefOrOutKeyword.IsKind(SyntaxKind.RefKeyword, SyntaxKind.OutKeyword))
                {
                    ExpressionSyntax expression = node.Expression?.WalkDownParentheses();

                    switch (expression?.Kind())
                    {
                        case SyntaxKind.IdentifierName:
                            {
                                if (IsBackingFieldReference((IdentifierNameSyntax)expression))
                                    Success = false;

                                return;
                            }
                        case SyntaxKind.SimpleMemberAccessExpression:
                            {
                                var memberAccessExpression = (MemberAccessExpressionSyntax)expression;

                                if (memberAccessExpression.Expression.IsKind(SyntaxKind.ThisExpression))
                                {
                                    SimpleNameSyntax name = memberAccessExpression.Name;

                                    if (name.IsKind(SyntaxKind.IdentifierName))
                                    {
                                        if (IsBackingFieldReference((IdentifierNameSyntax)name))
                                            Success = false;

                                        return;
                                    }
                                }

                                break;
                            }
                    }
                }

                base.VisitArgument(node);
            }

            public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
            {
                CancellationToken.ThrowIfCancellationRequested();

                Debug.Assert(!_isInInstanceConstructor);

                if (ShouldSearchForReferenceInInstanceConstructor
                    && !node.Modifiers.Contains(SyntaxKind.StaticKeyword))
                {
                    _isInInstanceConstructor = true;
                }

                base.VisitConstructorDeclaration(node);
                _isInInstanceConstructor = false;
            }

            public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
            {
                if (FieldSymbol.Type.TypeKind == TypeKind.Struct
                    && IsAssigned(node.Left))
                {
                    Success = false;
                }
                else
                {
                    base.VisitAssignmentExpression(node);
                }
            }

            public override void VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
            {
                if (node.IsKind(SyntaxKind.PreIncrementExpression, SyntaxKind.PreDecrementExpression)
                    && FieldSymbol.Type.TypeKind == TypeKind.Struct
                    && IsAssigned(node.Operand))
                {
                    Success = false;
                }
                else
                {
                    base.VisitPrefixUnaryExpression(node);
                }
            }

            public override void VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
            {
                if (node.IsKind(SyntaxKind.PostIncrementExpression, SyntaxKind.PostDecrementExpression)
                    && FieldSymbol.Type.TypeKind == TypeKind.Struct
                    && IsAssigned(node.Operand))
                {
                    Success = false;
                }
                else
                {
                    base.VisitPostfixUnaryExpression(node);
                }
            }

            private bool IsAssigned(ExpressionSyntax expression)
            {
                switch (expression.Kind())
                {
                    case SyntaxKind.SimpleMemberAccessExpression:
                        {
                            var memberAccessExpression = (MemberAccessExpressionSyntax)expression;
                            expression = memberAccessExpression.Expression;

                            break;
                        }
                    case SyntaxKind.ElementAccessExpression:
                        {
                            var elementAccessExpression = (ElementAccessExpressionSyntax)expression;
                            expression = elementAccessExpression.Expression;

                            break;
                        }
                    default:
                        {
                            return false;
                        }
                }

                switch (expression.Kind())
                {
                    case SyntaxKind.SimpleMemberAccessExpression:
                        {
                            var memberAccessExpression = ((MemberAccessExpressionSyntax)expression);

                            if (memberAccessExpression.Expression.IsKind(SyntaxKind.ThisExpression)
                                && memberAccessExpression.Name.IsKind(SyntaxKind.IdentifierName)
                                && IsBackingFieldReference((IdentifierNameSyntax)memberAccessExpression.Name))
                            {
                                return true;
                            }

                            break;
                        }
                    case SyntaxKind.IdentifierName:
                        {
                            if (IsBackingFieldReference((IdentifierNameSyntax)expression))
                                return true;

                            break;
                        }
                }

                return false;
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
                if (_isInInstanceConstructor
                    && IsBackingFieldReference(node))
                {
                    Success = false;
                }
                else
                {
                    base.VisitIdentifierName(node);
                }
            }

            private bool IsBackingFieldReference(IdentifierNameSyntax identifierName)
            {
                return string.Equals(identifierName.Identifier.ValueText, FieldSymbol.Name, StringComparison.Ordinal)
                    && SymbolEqualityComparer.Default.Equals(SemanticModel.GetSymbol(identifierName, CancellationToken), FieldSymbol);
            }

            [ThreadStatic]
            private static UseAutoPropertyWalker _cachedInstance;

            public static UseAutoPropertyWalker GetInstance()
            {
                UseAutoPropertyWalker walker = _cachedInstance;

                if (walker != null)
                {
                    Debug.Assert(walker.FieldSymbol == null);
                    Debug.Assert(walker.SemanticModel == null);
                    Debug.Assert(walker.CancellationToken == default);

                    _cachedInstance = null;
                    return walker;
                }

                return new UseAutoPropertyWalker();
            }

            public static void Free(UseAutoPropertyWalker walker)
            {
                walker.SetValues(
                    default(IFieldSymbol),
                    false,
                    default(SemanticModel),
                    default(CancellationToken));

                _cachedInstance = walker;
            }
        }
    }
}
