// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analysis
{
    internal static class UseCountOrLengthPropertyInsteadOfAnyMethodAnalysis
    {
        public static void Analyze(SyntaxNodeAnalysisContext context, in SimpleMemberInvocationExpressionInfo invocationInfo)
        {
            InvocationExpressionSyntax invocationExpression = invocationInfo.InvocationExpression;

            if (invocationExpression.IsParentKind(SyntaxKind.SimpleMemberAccessExpression))
                return;

            SemanticModel semanticModel = context.SemanticModel;
            CancellationToken cancellationToken = context.CancellationToken;

            IMethodSymbol methodSymbol = semanticModel.GetReducedExtensionMethodInfo(invocationExpression, cancellationToken).Symbol;

            if (methodSymbol == null)
                return;

            if (!SymbolUtility.IsLinqExtensionOfIEnumerableOfTWithoutParameters(methodSymbol, "Any"))
                return;

            ExpressionSyntax expression = invocationInfo.Expression;

            ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(expression, cancellationToken);

            if (typeSymbol == null)
                return;

            string propertyName = SymbolUtility.GetCountOrLengthPropertyName(typeSymbol, semanticModel, expression.SpanStart);

            if (propertyName == null)
                return;

            DiagnosticHelpers.ReportDiagnostic(
                context,
                DiagnosticRules.UseCountOrLengthPropertyInsteadOfAnyMethod,
                Location.Create(context.Node.SyntaxTree, TextSpan.FromBounds(invocationInfo.Name.SpanStart, invocationExpression.Span.End)),
                ImmutableDictionary.CreateRange(new[] { new KeyValuePair<string, string>("PropertyName", propertyName) }),
                propertyName);
        }
    }
}
