// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SimplifyNullableOfTAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.SimplifyNullableOfT);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeQualifiedName(f), SyntaxKind.QualifiedName);
            context.RegisterSyntaxNodeAction(f => AnalyzeGenericName(f), SyntaxKind.GenericName);
        }

        private static void AnalyzeGenericName(SyntaxNodeAnalysisContext context)
        {
            var genericName = (GenericNameSyntax)context.Node;

            if (genericName.IsParentKind(
                SyntaxKind.QualifiedName,
                SyntaxKind.UsingDirective,
                SyntaxKind.NameMemberCref,
                SyntaxKind.QualifiedCref))
            {
                return;
            }

            if (IsWithinNameOfExpression(genericName, context.SemanticModel, context.CancellationToken))
                return;

            if (genericName
                .TypeArgumentList?
                .Arguments
                .SingleOrDefault(shouldThrow: false)?
                .IsKind(SyntaxKind.OmittedTypeArgument) != false)
            {
                return;
            }

            if (!(context.SemanticModel.GetSymbol(genericName, context.CancellationToken) is INamedTypeSymbol namedTypeSymbol))
                return;

            if (!namedTypeSymbol.IsNullableType())
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.SimplifyNullableOfT, genericName);
        }

        private static void AnalyzeQualifiedName(SyntaxNodeAnalysisContext context)
        {
            var qualifiedName = (QualifiedNameSyntax)context.Node;

            if (qualifiedName.IsParentKind(SyntaxKind.UsingDirective, SyntaxKind.QualifiedCref))
                return;

            if (!(qualifiedName.Right is GenericNameSyntax genericName))
                return;

            if (genericName
                .TypeArgumentList?
                .Arguments
                .SingleOrDefault(shouldThrow: false)?
                .IsKind(SyntaxKind.OmittedTypeArgument) != false)
            {
                return;
            }

            if (IsWithinNameOfExpression(qualifiedName, context.SemanticModel, context.CancellationToken))
                return;

            if (!(context.SemanticModel.GetSymbol(qualifiedName, context.CancellationToken) is INamedTypeSymbol typeSymbol))
                return;

            if (CSharpFacts.IsPredefinedType(typeSymbol.SpecialType))
                return;

            if (!typeSymbol.IsNullableType())
                return;

            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticRules.SimplifyNullableOfT, qualifiedName);
        }

        private static bool IsWithinNameOfExpression(
            SyntaxNode node,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default)
        {
            for (node = node.Parent; node != null; node = node.Parent)
            {
                SyntaxKind kind = node.Kind();

                if (kind == SyntaxKind.InvocationExpression)
                {
                    if (CSharpUtility.IsNameOfExpression((InvocationExpressionSyntax)node, semanticModel, cancellationToken))
                        return true;
                }
                else if (kind == SyntaxKind.TypeArgumentList)
                {
                    break;
                }

                if (node is StatementSyntax
                    || node is MemberDeclarationSyntax)
                {
                    break;
                }
            }

            return false;
        }
    }
}
