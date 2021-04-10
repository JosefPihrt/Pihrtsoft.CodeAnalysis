// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    internal static class CallCastInsteadOfSelectAnalysis
    {
        public static void Analyze(
            SyntaxNodeAnalysisContext context,
            in SimpleMemberInvocationExpressionInfo invocationInfo)
        {
            InvocationExpressionSyntax invocationExpression = invocationInfo.InvocationExpression;

            if (invocationInfo.Name.SpanOrTrailingTriviaContainsDirectives())
                return;

            if (invocationInfo.ArgumentList.SpanOrLeadingTriviaContainsDirectives())
                return;

            SemanticModel semanticModel = context.SemanticModel;
            CancellationToken cancellationToken = context.CancellationToken;

            ExtensionMethodSymbolInfo extensionInfo = semanticModel.GetExtensionMethodInfo(invocationExpression, cancellationToken);

            if (extensionInfo.Symbol == null)
                return;

            if (!SymbolUtility.IsLinqSelect(extensionInfo.Symbol, allowImmutableArrayExtension: true))
                return;

            ITypeSymbol typeArgument = extensionInfo.ReducedSymbolOrSymbol.TypeArguments[0];

            if (!typeArgument.IsReferenceType)
                return;

            if (typeArgument.SpecialType == SpecialType.System_Object)
                return;

            ExpressionSyntax expression = invocationExpression.ArgumentList?.Arguments.Last().Expression;

            SingleParameterLambdaExpressionInfo lambdaInfo = SyntaxInfo.SingleParameterLambdaExpressionInfo(expression);

            if (!lambdaInfo.Success)
                return;

            CastExpressionSyntax castExpression = GetCastExpression(lambdaInfo.Body);

            if (castExpression == null)
                return;

            if (!(castExpression.Expression is IdentifierNameSyntax identifierName))
                return;

            if (!string.Equals(lambdaInfo.Parameter.Identifier.ValueText, identifierName.Identifier.ValueText, StringComparison.Ordinal))
                return;

            if (semanticModel.GetMethodSymbol(castExpression, cancellationToken)?.MethodKind == MethodKind.Conversion)
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.OptimizeLinqMethodCall,
                Location.Create(invocationExpression.SyntaxTree, TextSpan.FromBounds(invocationInfo.Name.SpanStart, invocationExpression.Span.End)));
        }

        internal static CastExpressionSyntax GetCastExpression(CSharpSyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.CastExpression:
                    {
                        return (CastExpressionSyntax)node;
                    }
                case SyntaxKind.Block:
                    {
                        var block = (BlockSyntax)node;

                        var returnStatement = block.Statements.SingleOrDefault(shouldThrow: false) as ReturnStatementSyntax;

                        return returnStatement?.Expression as CastExpressionSyntax;
                    }
            }

            return null;
        }
    }
}
