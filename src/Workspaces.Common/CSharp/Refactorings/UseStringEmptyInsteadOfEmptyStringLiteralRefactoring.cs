// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpTypeFactory;

namespace Roslynator.CSharp.Refactorings
{
    internal static class UseStringEmptyInsteadOfEmptyStringLiteralRefactoring {
        internal static Task<Document> RefactorAsync(
            Document document,
            LiteralExpressionSyntax literalExpressionSyntax,
            CancellationToken cancellationToken)
        {
            ExpressionSyntax newNode = CSharpFactory.SimpleMemberAccessExpression(
                StringType(),
                IdentifierName("Empty")
                );

            return document.ReplaceNodeAsync(literalExpressionSyntax, newNode, cancellationToken);
        }
    }
}
