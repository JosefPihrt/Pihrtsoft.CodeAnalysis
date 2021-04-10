// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    internal static class CallThenByInsteadOfOrderByAnalysis
    {
        public static void Analyze(SyntaxNodeAnalysisContext context, in SimpleMemberInvocationExpressionInfo invocationInfo)
        {
            ExpressionSyntax expression = invocationInfo.Expression;

            if (expression.Kind() != SyntaxKind.InvocationExpression)
                return;

            SimpleMemberInvocationExpressionInfo invocationInfo2 = SyntaxInfo.SimpleMemberInvocationExpressionInfo((InvocationExpressionSyntax)expression);

            if (!invocationInfo2.Success)
                return;

            if (!StringUtility.Equals(invocationInfo2.NameText, "OrderBy", "OrderByDescending"))
                return;

            if (IsOrderByOrOrderByDescending(invocationInfo.InvocationExpression, context.SemanticModel, context.CancellationToken)
                && IsOrderByOrOrderByDescending(invocationInfo2.InvocationExpression, context.SemanticModel, context.CancellationToken))
            {
                DiagnosticHelpers.ReportDiagnostic(
                    context,
                    DiagnosticRules.CallThenByInsteadOfOrderBy,
                    invocationInfo.Name,
                    (invocationInfo.NameText == "OrderByDescending") ? "Descending" : null);
            }
        }

        private static bool IsOrderByOrOrderByDescending(InvocationExpressionSyntax invocationExpression, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            IMethodSymbol methodSymbol = semanticModel.GetExtensionMethodInfo(invocationExpression, cancellationToken).Symbol;

            return methodSymbol?.IsName("OrderBy", "OrderByDescending") == true
                && SymbolUtility.IsLinqExtensionOfIEnumerableOfT(methodSymbol);
        }
    }
}
