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
using Microsoft.CodeAnalysis.Text;
using Roslynator.CodeFixes;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AvoidNullReferenceExceptionCodeFixProvider))]
    [Shared]
    public sealed class AvoidNullReferenceExceptionCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.AvoidNullReferenceException); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out ExpressionSyntax expression, predicate: f => f.IsKind(SyntaxKind.SimpleMemberAccessExpression, SyntaxKind.ElementAccessExpression)))
                return;

            if (IsPartOfLeftSideOfAssignment())
                return;

            if (expression
                .WalkUp(f => f.IsKind(SyntaxKind.InvocationExpression, SyntaxKind.SimpleMemberAccessExpression, SyntaxKind.ElementAccessExpression, SyntaxKind.ParenthesizedExpression))
                .IsParentKind(SyntaxKind.AwaitExpression))
            {
                return;
            }

            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

            if (expression.IsInExpressionTree(semanticModel, context.CancellationToken))
                return;

            SyntaxKind kind = expression.Kind();

            if (kind == SyntaxKind.SimpleMemberAccessExpression)
            {
                expression = ((MemberAccessExpressionSyntax)expression).Expression;
            }
            else if (kind == SyntaxKind.ElementAccessExpression)
            {
                expression = ((ElementAccessExpressionSyntax)expression).Expression;
            }

            CodeAction codeAction = CodeAction.Create(
                "Use conditional access",
                cancellationToken => RefactorAsync(context.Document, expression, cancellationToken),
                GetEquivalenceKey(DiagnosticIdentifiers.AvoidNullReferenceException));

            context.RegisterCodeFix(codeAction, context.Diagnostics);

            bool IsPartOfLeftSideOfAssignment()
            {
                for (SyntaxNode node = expression; node != null; node = node.Parent)
                {
                    var assignmentExpression = node.Parent as AssignmentExpressionSyntax;

                    if (assignmentExpression?.Left == node)
                        return true;
                }

                return false;
            }
        }

        private static async Task<Document> RefactorAsync(
            Document document,
            ExpressionSyntax expression,
            CancellationToken cancellationToken)
        {
            var span = new TextSpan(expression.Span.End, 0);

            Document newDocument = await document.WithTextChangeAsync(span, "?", cancellationToken).ConfigureAwait(false);

            SyntaxNode root = await newDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var conditionalAccessExpression = (ConditionalAccessExpressionSyntax)root.FindNode(span, getInnermostNodeForTie: true);

            SemanticModel semanticModel = await newDocument.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            TypeInfo typeInfo = semanticModel.GetTypeInfo(conditionalAccessExpression, cancellationToken);

            ITypeSymbol type = typeInfo.Type;
            ITypeSymbol convertedType = typeInfo.ConvertedType;

            if (!SymbolEqualityComparer.Default.Equals(type, convertedType)
                && type.IsNullableType()
                && SymbolEqualityComparer.Default.Equals(((INamedTypeSymbol)type).TypeArguments[0], convertedType))
            {
                ExpressionSyntax defaultValue = convertedType.GetDefaultValueSyntax(document.GetDefaultSyntaxOptions());

                ExpressionSyntax coalesceExpression = CoalesceExpression(conditionalAccessExpression.WithoutTrivia(), defaultValue)
                    .WithTriviaFrom(conditionalAccessExpression)
                    .Parenthesize()
                    .WithFormatterAnnotation();

                return await newDocument.ReplaceNodeAsync(conditionalAccessExpression, coalesceExpression, cancellationToken).ConfigureAwait(false);
            }

            return newDocument;
        }
    }
}
