// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UsePredefinedTypeAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.UsePredefinedType);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeQualifiedName(f), SyntaxKind.QualifiedName);
            context.RegisterSyntaxNodeAction(f => AnalyzeIdentifierName(f), SyntaxKind.IdentifierName);
            context.RegisterSyntaxNodeAction(f => AnalyzeXmlCrefAttribute(f), SyntaxKind.XmlCrefAttribute);
            context.RegisterSyntaxNodeAction(f => AnalyzeSimpleMemberAccessExpression(f), SyntaxKind.SimpleMemberAccessExpression);
        }

        private static void AnalyzeIdentifierName(SyntaxNodeAnalysisContext context)
        {
            var identifierName = (IdentifierNameSyntax)context.Node;

            if (identifierName.IsVar)
                return;

            if (identifierName.IsParentKind(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxKind.QualifiedName,
                SyntaxKind.UsingDirective))
            {
                return;
            }

            if (!SupportsPredefinedType(identifierName))
                return;

            if (identifierName.IsPartOfDocumentationComment())
                return;

            if (IsArgumentExpressionOfNameOfExpression(context, identifierName))
                return;

            if (!(context.SemanticModel.GetSymbol(identifierName, context.CancellationToken) is ITypeSymbol typeSymbol))
                return;

            if (!CSharpFacts.IsPredefinedType(typeSymbol.SpecialType))
                return;

            IAliasSymbol aliasSymbol = context.SemanticModel.GetAliasInfo(identifierName, context.CancellationToken);

            if (aliasSymbol != null)
                return;

            ReportDiagnostic(context, identifierName);
        }

        private static void AnalyzeXmlCrefAttribute(SyntaxNodeAnalysisContext context)
        {
            var xmlCrefAttribute = (XmlCrefAttributeSyntax)context.Node;

            CrefSyntax cref = xmlCrefAttribute.Cref;

            switch (cref?.Kind())
            {
                case SyntaxKind.NameMemberCref:
                    {
                        Analyze(context, cref, (NameMemberCrefSyntax)cref);
                        break;
                    }
                case SyntaxKind.QualifiedCref:
                    {
                        var qualifiedCref = (QualifiedCrefSyntax)cref;

                        MemberCrefSyntax memberCref = qualifiedCref.Member;

                        if (memberCref?.IsKind(SyntaxKind.NameMemberCref) != true)
                            break;

                        Analyze(context, cref, (NameMemberCrefSyntax)memberCref);
                        break;
                    }
            }
        }

        private static void Analyze(SyntaxNodeAnalysisContext context, CrefSyntax cref, NameMemberCrefSyntax nameMemberCref)
        {
            if (!(nameMemberCref.Name is IdentifierNameSyntax identifierName))
                return;

            if (!SupportsPredefinedType(identifierName))
                return;

            if (!(context.SemanticModel.GetSymbol(identifierName, context.CancellationToken) is ITypeSymbol typeSymbol))
                return;

            if (!CSharpFacts.IsPredefinedType(typeSymbol.SpecialType))
                return;

            IAliasSymbol aliasSymbol = context.SemanticModel.GetAliasInfo(identifierName, context.CancellationToken);

            if (aliasSymbol != null)
                return;

            ReportDiagnostic(context, cref);
        }

        private static void AnalyzeQualifiedName(SyntaxNodeAnalysisContext context)
        {
            var qualifiedName = (QualifiedNameSyntax)context.Node;

            if (qualifiedName.IsParentKind(SyntaxKind.UsingDirective))
                return;

            if (!(qualifiedName.Right is IdentifierNameSyntax identifierName))
                return;

            if (!SupportsPredefinedType(identifierName))
                return;

            if (IsArgumentExpressionOfNameOfExpression(context, qualifiedName))
                return;

            if (!(context.SemanticModel.GetSymbol(qualifiedName, context.CancellationToken) is ITypeSymbol typeSymbol))
                return;

            if (!CSharpFacts.IsPredefinedType(typeSymbol.SpecialType))
                return;

            ReportDiagnostic(context, qualifiedName);
        }

        private static void AnalyzeSimpleMemberAccessExpression(SyntaxNodeAnalysisContext context)
        {
            var memberAccess = (MemberAccessExpressionSyntax)context.Node;

            if (memberAccess.IsParentKind(SyntaxKind.SimpleMemberAccessExpression))
                return;

            ExpressionSyntax expression = memberAccess.Expression;

            if (expression == null)
                return;

            SyntaxKind kind = expression.Kind();

            if (kind == SyntaxKind.IdentifierName)
            {
                if (!SupportsPredefinedType((IdentifierNameSyntax)expression))
                    return;
            }
            else if (kind == SyntaxKind.SimpleMemberAccessExpression)
            {
                memberAccess = (MemberAccessExpressionSyntax)expression;

                if (!(memberAccess.Name is IdentifierNameSyntax identifierName))
                    return;

                if (!SupportsPredefinedType(identifierName))
                    return;
            }
            else
            {
                return;
            }

            if (!(context.SemanticModel.GetSymbol(expression, context.CancellationToken) is ITypeSymbol typeSymbol))
                return;

            if (!CSharpFacts.IsPredefinedType(typeSymbol.SpecialType))
                return;

            IAliasSymbol aliasSymbol = context.SemanticModel.GetAliasInfo(expression, context.CancellationToken);

            if (aliasSymbol != null)
                return;

            ReportDiagnostic(context, expression);
        }

        private static bool IsArgumentExpressionOfNameOfExpression(SyntaxNodeAnalysisContext context, SyntaxNode node)
        {
            SyntaxNode parent = node.Parent;

            if (parent?.IsKind(SyntaxKind.Argument) != true)
                return false;

            parent = parent.Parent;

            if (parent?.IsKind(SyntaxKind.ArgumentList) != true)
                return false;

            parent = parent.Parent;

            return parent != null
                && CSharpUtility.IsNameOfExpression(parent, context.SemanticModel, context.CancellationToken);
        }

        private static bool SupportsPredefinedType(IdentifierNameSyntax identifierName)
        {
            if (identifierName == null)
                return false;

            switch (identifierName.Identifier.ValueText)
            {
                case "Object":
                case "Boolean":
                case "Char":
                case "SByte":
                case "Byte":
                case "Int16":
                case "UInt16":
                case "Int32":
                case "UInt32":
                case "Int64":
                case "UInt64":
                case "Decimal":
                case "Single":
                case "Double":
                case "String":
                    return true;
                default:
                    return false;
            }
        }

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, SyntaxNode node)
        {
            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.UsePredefinedType, node);
        }
    }
}
