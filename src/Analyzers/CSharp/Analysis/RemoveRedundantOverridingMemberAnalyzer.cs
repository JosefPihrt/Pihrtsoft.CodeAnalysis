// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RemoveRedundantOverridingMemberAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.RemoveRedundantOverridingMember);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeMethodDeclaration(f), SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzePropertyDeclaration(f), SyntaxKind.PropertyDeclaration);
            context.RegisterSyntaxNodeAction(f => AnalyzeIndexerDeclaration(f), SyntaxKind.IndexerDeclaration);
        }

        private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            if (methodDeclaration.ContainsDirectives)
                return;

            if (methodDeclaration.ContainsDiagnostics)
                return;

            if (methodDeclaration.AttributeLists.Any())
                return;

            if (!CheckModifiers(methodDeclaration.Modifiers))
                return;

            if (methodDeclaration.HasDocumentationComment())
                return;

            if (!methodDeclaration.DescendantTrivia(methodDeclaration.Span).All(f => f.IsWhitespaceOrEndOfLineTrivia()))
                return;

            ExpressionSyntax expression = GetMethodExpression(methodDeclaration);

            SimpleMemberInvocationExpressionInfo invocationInfo = SyntaxInfo.SimpleMemberInvocationExpressionInfo(expression);

            if (!invocationInfo.Success)
                return;

            if (invocationInfo.Expression.Kind() != SyntaxKind.BaseExpression)
                return;

            SemanticModel semanticModel = context.SemanticModel;
            CancellationToken cancellationToken = context.CancellationToken;

            IMethodSymbol methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration, cancellationToken);

            if (methodSymbol == null)
                return;

            IMethodSymbol overriddenMethod = methodSymbol.OverriddenMethod;

            if (overriddenMethod == null)
                return;

            ISymbol symbol = semanticModel.GetSymbol(invocationInfo.Name, cancellationToken);

            if (!SymbolEqualityComparer.Default.Equals(overriddenMethod, symbol))
                return;

            if (!CheckParameters(methodDeclaration.ParameterList, invocationInfo.ArgumentList, semanticModel, cancellationToken))
                return;

            if (!CheckDefaultValues(methodSymbol.Parameters, overriddenMethod.Parameters))
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.RemoveRedundantOverridingMember,
                methodDeclaration,
                CSharpFacts.GetTitle(methodDeclaration));
        }

        private static bool CheckModifiers(SyntaxTokenList modifiers)
        {
            var isOverride = false;

            foreach (SyntaxToken modifier in modifiers)
            {
                switch (modifier.Kind())
                {
                    case SyntaxKind.OverrideKeyword:
                        {
                            isOverride = true;
                            break;
                        }
                    case SyntaxKind.SealedKeyword:
                    case SyntaxKind.PartialKeyword:
                        {
                            return false;
                        }
                }
            }

            return isOverride;
        }

        private static bool CheckParameters(
            BaseParameterListSyntax parameterList,
            BaseArgumentListSyntax argumentList,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            SeparatedSyntaxList<ParameterSyntax> parameters = parameterList.Parameters;
            SeparatedSyntaxList<ArgumentSyntax> arguments = argumentList.Arguments;

            if (parameters.Count != arguments.Count)
                return false;

            for (int i = 0; i < parameters.Count; i++)
            {
                if (!SymbolEqualityComparer.Default.Equals(
                    semanticModel.GetDeclaredSymbol(parameters[i], cancellationToken),
                    GetParameterSymbol(arguments[i].Expression, semanticModel, cancellationToken)))
                {
                    return false;
                }
            }

            return true;
        }

        private static IParameterSymbol GetParameterSymbol(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (expression == null)
                return null;

            ISymbol symbol = semanticModel.GetSymbol(expression, cancellationToken);

            if (symbol?.Kind != SymbolKind.Parameter)
                return null;

            var parameterSymbol = (IParameterSymbol)symbol;

            ISymbol containingSymbol = parameterSymbol.ContainingSymbol;

            if (containingSymbol?.Kind == SymbolKind.Method)
            {
                var methodSymbol = (IMethodSymbol)containingSymbol;

                ISymbol associatedSymbol = methodSymbol.AssociatedSymbol;

                if (associatedSymbol?.Kind == SymbolKind.Property)
                {
                    var propertySymbol = (IPropertySymbol)associatedSymbol;

                    if (propertySymbol.IsIndexer)
                    {
                        ImmutableArray<IParameterSymbol> parameters = propertySymbol.Parameters;

                        if (parameters.Length > parameterSymbol.Ordinal)
                            return propertySymbol.Parameters[parameterSymbol.Ordinal];
                    }
                }
            }

            return parameterSymbol;
        }

        private static bool CheckDefaultValues(ImmutableArray<IParameterSymbol> parameters, ImmutableArray<IParameterSymbol> baseParameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].HasExplicitDefaultValue)
                {
                    if (baseParameters[i].HasExplicitDefaultValue)
                    {
                        if (!Equals(parameters[i].ExplicitDefaultValue, baseParameters[i].ExplicitDefaultValue))
                            return false;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (baseParameters[i].HasExplicitDefaultValue)
                {
                    return false;
                }
            }

            return true;
        }

        private static ExpressionSyntax GetMethodExpression(MethodDeclarationSyntax methodDeclaration)
        {
            BlockSyntax body = methodDeclaration.Body;

            if (body != null)
            {
                StatementSyntax statement = body.Statements.SingleOrDefault(shouldThrow: false);

                if (statement != null)
                {
                    if (methodDeclaration.ReturnsVoid())
                    {
                        if (statement.IsKind(SyntaxKind.ExpressionStatement))
                            return ((ExpressionStatementSyntax)statement).Expression;
                    }
                    else if (statement.IsKind(SyntaxKind.ReturnStatement))
                    {
                        return ((ReturnStatementSyntax)statement).Expression;
                    }
                }
            }
            else
            {
                return methodDeclaration.ExpressionBody?.Expression;
            }

            return null;
        }

        private static void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
        {
            var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

            if (propertyDeclaration.ContainsDirectives)
                return;

            if (propertyDeclaration.ContainsDiagnostics)
                return;

            if (propertyDeclaration.AttributeLists.Any())
                return;

            if (!CheckModifiers(propertyDeclaration.Modifiers))
                return;

            if (propertyDeclaration.HasDocumentationComment())
                return;

            if (!propertyDeclaration.DescendantTrivia(propertyDeclaration.Span).All(f => f.IsWhitespaceOrEndOfLineTrivia()))
                return;

            AccessorListSyntax accessorList = propertyDeclaration.AccessorList;

            if (accessorList == null)
                return;

            foreach (AccessorDeclarationSyntax accessor in accessorList.Accessors)
            {
                if (!IsFixable(propertyDeclaration, accessor, context.SemanticModel, context.CancellationToken))
                    return;
            }

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.RemoveRedundantOverridingMember,
                propertyDeclaration,
                CSharpFacts.GetTitle(propertyDeclaration));
        }

        internal static bool IsFixable(
            PropertyDeclarationSyntax propertyDeclaration,
            AccessorDeclarationSyntax accessor,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            switch (accessor.Kind())
            {
                case SyntaxKind.GetAccessorDeclaration:
                    {
                        ExpressionSyntax expression = GetGetAccessorExpression(accessor);

                        if (expression?.IsKind(SyntaxKind.SimpleMemberAccessExpression) != true)
                            return false;

                        var memberAccess = (MemberAccessExpressionSyntax)expression;

                        if (memberAccess.Expression?.IsKind(SyntaxKind.BaseExpression) != true)
                            return false;

                        SimpleNameSyntax simpleName = memberAccess.Name;

                        IPropertySymbol propertySymbol = semanticModel.GetDeclaredSymbol(propertyDeclaration, cancellationToken);

                        if (propertySymbol == null)
                            return false;

                        IPropertySymbol overriddenProperty = propertySymbol.OverriddenProperty;

                        if (overriddenProperty == null)
                            return false;

                        ISymbol symbol = semanticModel.GetSymbol(simpleName, cancellationToken);

                        return SymbolEqualityComparer.Default.Equals(overriddenProperty, symbol);
                    }
                case SyntaxKind.SetAccessorDeclaration:
                    {
                        ExpressionSyntax expression = GetSetAccessorExpression(accessor);

                        SimpleAssignmentExpressionInfo assignment = SyntaxInfo.SimpleAssignmentExpressionInfo(expression);

                        if (!assignment.Success)
                            return false;

                        if (assignment.Left.Kind() != SyntaxKind.SimpleMemberAccessExpression)
                            return false;

                        var memberAccess = (MemberAccessExpressionSyntax)assignment.Left;

                        if (memberAccess.Expression?.IsKind(SyntaxKind.BaseExpression) != true)
                            return false;

                        if (assignment.Right.Kind() != SyntaxKind.IdentifierName)
                            return false;

                        var identifierName = (IdentifierNameSyntax)assignment.Right;

                        if (identifierName.Identifier.ValueText != "value")
                            return false;

                        SimpleNameSyntax simpleName = memberAccess.Name;

                        if (simpleName == null)
                            return false;

                        IPropertySymbol propertySymbol = semanticModel.GetDeclaredSymbol(propertyDeclaration, cancellationToken);

                        if (propertySymbol == null)
                            return false;

                        IPropertySymbol overriddenProperty = propertySymbol.OverriddenProperty;

                        if (overriddenProperty == null)
                            return false;

                        ISymbol symbol = semanticModel.GetSymbol(simpleName, cancellationToken);

                        return SymbolEqualityComparer.Default.Equals(overriddenProperty, symbol);
                    }
                case SyntaxKind.UnknownAccessorDeclaration:
                    {
                        return false;
                    }
                default:
                    {
                        Debug.Fail(accessor.Kind().ToString());
                        return false;
                    }
            }
        }

        private static void AnalyzeIndexerDeclaration(SyntaxNodeAnalysisContext context)
        {
            var indexerDeclaration = (IndexerDeclarationSyntax)context.Node;

            if (indexerDeclaration.ContainsDirectives)
                return;

            if (indexerDeclaration.ContainsDiagnostics)
                return;

            if (indexerDeclaration.AttributeLists.Any())
                return;

            if (!CheckModifiers(indexerDeclaration.Modifiers))
                return;

            if (indexerDeclaration.HasDocumentationComment())
                return;

            if (!indexerDeclaration.DescendantTrivia(indexerDeclaration.Span).All(f => f.IsWhitespaceOrEndOfLineTrivia()))
                return;

            AccessorListSyntax accessorList = indexerDeclaration.AccessorList;

            if (accessorList == null)
                return;

            foreach (AccessorDeclarationSyntax accessor in accessorList.Accessors)
            {
                if (!IsFixable(indexerDeclaration, accessor, context.SemanticModel, context.CancellationToken))
                    return;
            }

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.RemoveRedundantOverridingMember,
                indexerDeclaration,
                CSharpFacts.GetTitle(indexerDeclaration));
        }

        internal static bool IsFixable(
            IndexerDeclarationSyntax indexerDeclaration,
            AccessorDeclarationSyntax accessor,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            switch (accessor.Kind())
            {
                case SyntaxKind.GetAccessorDeclaration:
                    {
                        ExpressionSyntax expression = GetGetAccessorExpression(accessor);

                        if (!(expression is ElementAccessExpressionSyntax elementAccess))
                            return false;

                        if (elementAccess.Expression?.IsKind(SyntaxKind.BaseExpression) != true)
                            return false;

                        if (elementAccess.ArgumentList == null)
                            return false;

                        IPropertySymbol propertySymbol = semanticModel.GetDeclaredSymbol(indexerDeclaration, cancellationToken);

                        if (propertySymbol == null)
                            return false;

                        IPropertySymbol overriddenProperty = propertySymbol.OverriddenProperty;

                        if (overriddenProperty == null)
                            return false;

                        ISymbol symbol = semanticModel.GetSymbol(elementAccess, cancellationToken);

                        return SymbolEqualityComparer.Default.Equals(overriddenProperty, symbol)
                            && CheckParameters(indexerDeclaration.ParameterList, elementAccess.ArgumentList, semanticModel, cancellationToken)
                            && CheckDefaultValues(propertySymbol.Parameters, overriddenProperty.Parameters);
                    }
                case SyntaxKind.SetAccessorDeclaration:
                    {
                        ExpressionSyntax expression = GetSetAccessorExpression(accessor);

                        SimpleAssignmentExpressionInfo assignment = SyntaxInfo.SimpleAssignmentExpressionInfo(expression);

                        if (!assignment.Success)
                            return false;

                        if (assignment.Left.Kind() != SyntaxKind.ElementAccessExpression)
                            return false;

                        var elementAccess = (ElementAccessExpressionSyntax)assignment.Left;

                        if (elementAccess.Expression?.IsKind(SyntaxKind.BaseExpression) != true)
                            return false;

                        if (elementAccess.ArgumentList == null)
                            return false;

                        if (assignment.Right.Kind() != SyntaxKind.IdentifierName)
                            return false;

                        var identifierName = (IdentifierNameSyntax)assignment.Right;

                        if (identifierName.Identifier.ValueText != "value")
                            return false;

                        IPropertySymbol propertySymbol = semanticModel.GetDeclaredSymbol(indexerDeclaration, cancellationToken);

                        if (propertySymbol == null)
                            return false;

                        IPropertySymbol overriddenProperty = propertySymbol.OverriddenProperty;

                        if (overriddenProperty == null)
                            return false;

                        ISymbol symbol = semanticModel.GetSymbol(elementAccess, cancellationToken);

                        return SymbolEqualityComparer.Default.Equals(overriddenProperty, symbol)
                            && CheckParameters(indexerDeclaration.ParameterList, elementAccess.ArgumentList, semanticModel, cancellationToken)
                            && CheckDefaultValues(propertySymbol.Parameters, overriddenProperty.Parameters);
                    }
                case SyntaxKind.UnknownAccessorDeclaration:
                    {
                        return false;
                    }
                default:
                    {
                        Debug.Fail(accessor.Kind().ToString());
                        return false;
                    }
            }
        }

        private static ExpressionSyntax GetGetAccessorExpression(AccessorDeclarationSyntax accessor)
        {
            BlockSyntax body = accessor.Body;

            if (body != null)
            {
                StatementSyntax statement = body.Statements.SingleOrDefault(shouldThrow: false);

                if (statement?.Kind() == SyntaxKind.ReturnStatement)
                    return ((ReturnStatementSyntax)statement).Expression;
            }
            else
            {
                return accessor.ExpressionBody?.Expression;
            }

            return null;
        }

        private static ExpressionSyntax GetSetAccessorExpression(AccessorDeclarationSyntax accessor)
        {
            BlockSyntax body = accessor.Body;

            if (body != null)
            {
                StatementSyntax statement = body.Statements.SingleOrDefault(shouldThrow: false);

                if (statement?.Kind() == SyntaxKind.ExpressionStatement)
                    return ((ExpressionStatementSyntax)statement).Expression;
            }
            else
            {
                return accessor.ExpressionBody?.Expression;
            }

            return null;
        }
    }
}
