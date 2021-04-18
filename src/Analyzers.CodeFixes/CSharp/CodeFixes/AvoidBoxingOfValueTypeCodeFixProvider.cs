// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AvoidBoxingOfValueTypeCodeFixProvider))]
    [Shared]
    public sealed class AvoidBoxingOfValueTypeCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.AvoidBoxingOfValueType); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out ExpressionSyntax expression))
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.AvoidBoxingOfValueType:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                (expression.IsKind(SyntaxKind.CharacterLiteralExpression))
                                    ? "Use string literal instead of character literal"
                                    : "Call 'ToString'",
                                cancellationToken => RefactorAsync(context.Document, expression, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }
        }

        private static async Task<Document> RefactorAsync(
            Document document,
            ExpressionSyntax expression,
            CancellationToken cancellationToken)
        {
            ExpressionSyntax newNode;

            if (expression.Kind() == SyntaxKind.CharacterLiteralExpression)
            {
                var literalExpression = (LiteralExpressionSyntax)expression;

                newNode = StringLiteralExpression(literalExpression.Token.ValueText);
            }
            else
            {
                ParenthesizedExpressionSyntax newExpression = expression
                    .WithoutTrivia()
                    .Parenthesize();

                SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

                if (ShouldAddConditionalAccess(semanticModel))
                {
                    newNode = ConditionalAccessExpression(
                        newExpression,
                        InvocationExpression(MemberBindingExpression(IdentifierName("ToString")), ArgumentList()));
                }
                else
                {
                    newNode = SimpleMemberInvocationExpression(
                        newExpression,
                        IdentifierName("ToString"),
                        ArgumentList());
                }
            }

            newNode = newNode.WithTriviaFrom(expression);

            return await document.ReplaceNodeAsync(expression, newNode, cancellationToken).ConfigureAwait(false);

            bool ShouldAddConditionalAccess(SemanticModel semanticModel)
            {
                if (!semanticModel.GetTypeSymbol(expression, cancellationToken).IsNullableType())
                    return false;

                if (!(expression is ConditionalAccessExpressionSyntax conditionalAccess))
                    return true;

                return semanticModel
                    .GetTypeSymbol(conditionalAccess.WhenNotNull, cancellationToken)
                    .IsNullableType();
            }
        }
    }
}