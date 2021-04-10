// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BitwiseOperatorAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    Immutable.InterlockedInitialize(ref _supportedDiagnostics, DiagnosticRules.BitwiseOperationOnEnumWithoutFlagsAttribute);

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterCompilationStartAction(startContext =>
            {
                INamedTypeSymbol flagsAttribute = startContext.Compilation.GetTypeByMetadataName("System.FlagsAttribute");

                if (flagsAttribute == null)
                    return;

                startContext.RegisterSyntaxNodeAction(nodeContext => AnalyzeBinaryExpression(nodeContext, flagsAttribute), SyntaxKind.BitwiseAndExpression);
                startContext.RegisterSyntaxNodeAction(nodeContext => AnalyzeBinaryExpression(nodeContext, flagsAttribute), SyntaxKind.BitwiseOrExpression);
                startContext.RegisterSyntaxNodeAction(nodeContext => AnalyzeBinaryExpression(nodeContext, flagsAttribute), SyntaxKind.ExclusiveOrExpression);
                startContext.RegisterSyntaxNodeAction(nodeContext => AnalyzeBitwiseNotExpression(nodeContext, flagsAttribute), SyntaxKind.BitwiseNotExpression);
            });
        }

        private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context, INamedTypeSymbol flagsAttribute)
        {
            var binaryExpression = (BinaryExpressionSyntax)context.Node;

            if (IsEnumWithoutFlags(binaryExpression.Left, flagsAttribute, context.SemanticModel, context.CancellationToken)
                || IsEnumWithoutFlags(binaryExpression.Right, flagsAttribute, context.SemanticModel, context.CancellationToken))
            {
                DiagnosticHelpers.ReportDiagnostic(
                    context,
                    DiagnosticRules.BitwiseOperationOnEnumWithoutFlagsAttribute,
                    binaryExpression);
            }
        }

        private static void AnalyzeBitwiseNotExpression(SyntaxNodeAnalysisContext context, INamedTypeSymbol flagsAttribute)
        {
            var prefixUnaryExpression = (PrefixUnaryExpressionSyntax)context.Node;

            if (IsEnumWithoutFlags(prefixUnaryExpression.Operand, flagsAttribute, context.SemanticModel, context.CancellationToken))
            {
                DiagnosticHelpers.ReportDiagnostic(
                    context,
                    DiagnosticRules.BitwiseOperationOnEnumWithoutFlagsAttribute,
                    prefixUnaryExpression);
            }
        }

        private static bool IsEnumWithoutFlags(
            ExpressionSyntax expression,
            INamedTypeSymbol flagsAttribute,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            if (expression?.IsMissing == false)
            {
                ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(expression, cancellationToken);

                return typeSymbol?.TypeKind == TypeKind.Enum
                    && !typeSymbol.HasAttribute(flagsAttribute);
            }

            return false;
        }
    }
}
