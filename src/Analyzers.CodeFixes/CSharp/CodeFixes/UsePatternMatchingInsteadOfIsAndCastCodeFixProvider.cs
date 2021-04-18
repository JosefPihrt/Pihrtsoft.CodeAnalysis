// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UsePatternMatchingInsteadOfIsAndCastCodeFixProvider))]
    [Shared]
    public sealed class UsePatternMatchingInsteadOfIsAndCastCodeFixProvider : BaseCodeFixProvider
    {
        private const string Title = "Use pattern matching";

        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.UsePatternMatchingInsteadOfIsAndCast); }
        }

        public override FixAllProvider GetFixAllProvider()
        {
            return null;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out SyntaxNode node, predicate: f => f.IsKind(SyntaxKind.LogicalAndExpression, SyntaxKind.IfStatement)))
                return;

            Diagnostic diagnostic = context.Diagnostics[0];

            if (node is BinaryExpressionSyntax logicalAndExpression)
            {
                CodeAction codeAction = CodeAction.Create(
                    Title,
                    cancellationToken => RefactorAsync(context.Document, logicalAndExpression, cancellationToken),
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
            else
            {
                var ifStatement = (IfStatementSyntax)node;

                CodeAction codeAction = CodeAction.Create(
                    Title,
                    cancellationToken => RefactorAsync(context.Document, ifStatement, cancellationToken),
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
        }

        private static async Task<Document> RefactorAsync(
            Document document,
            IfStatementSyntax ifStatement,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            (IsPatternExpressionSyntax isPatternExpression, StatementSyntax newNode) = GetNewNodes(ifStatement, ifStatement.Condition, ifStatement.Statement, semanticModel, cancellationToken);

            IfStatementSyntax newIfStatement = ifStatement
                .WithCondition(isPatternExpression)
                .WithStatement(newNode);

            return await document.ReplaceNodeAsync(ifStatement, newIfStatement, cancellationToken).ConfigureAwait(false);
        }

        private static async Task<Document> RefactorAsync(
            Document document,
            BinaryExpressionSyntax binaryExpression,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            (IsPatternExpressionSyntax isPatternExpression, ExpressionSyntax newNode) = GetNewNodes(binaryExpression, binaryExpression.Left, binaryExpression.Right, semanticModel, cancellationToken);

            BinaryExpressionSyntax newBinaryExpression = binaryExpression
                .WithLeft(isPatternExpression)
                .WithRight(newNode);

            return await document.ReplaceNodeAsync(binaryExpression, newBinaryExpression, cancellationToken).ConfigureAwait(false);
        }

        private static (IsPatternExpressionSyntax isPatternExpression, TNode newNode) GetNewNodes<TNode>(
            SyntaxNode node,
            ExpressionSyntax expression,
            TNode nodeToRewrite,
            SemanticModel semanticModel,
            CancellationToken cancellationToken) where TNode : SyntaxNode
        {
            IsExpressionInfo isInfo = SyntaxInfo.IsExpressionInfo(expression);

            ISymbol symbol = semanticModel.GetSymbol(isInfo.Expression, cancellationToken);

            ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(isInfo.Type, cancellationToken);

            string name = NameGenerator.CreateName(typeSymbol, firstCharToLower: true) ?? DefaultNames.Variable;

            name = NameGenerator.Default.EnsureUniqueLocalName(name, semanticModel, node.SpanStart, cancellationToken: cancellationToken) ?? DefaultNames.Variable;

            IsPatternExpressionSyntax isPatternExpression = IsPatternExpression(
                isInfo.Expression,
                DeclarationPattern(
                    isInfo.Type.WithTrailingTrivia(ElasticSpace),
                    SingleVariableDesignation(Identifier(name).WithRenameAnnotation()).WithTrailingTrivia(isInfo.Type.GetTrailingTrivia())));

            IEnumerable<SyntaxNode> nodes = nodeToRewrite
                .DescendantNodes()
                .Where(f => f.IsKind(SyntaxKind.IdentifierName) && SymbolEqualityComparer.Default.Equals(symbol, semanticModel.GetSymbol(f, cancellationToken)))
                .Select(f =>
                {
                    if (f.IsParentKind(SyntaxKind.SimpleMemberAccessExpression)
                        && ((MemberAccessExpressionSyntax)f.Parent).Expression.IsKind(SyntaxKind.ThisExpression))
                    {
                        f = f.Parent;
                    }

                    return ((ExpressionSyntax)f).WalkUpParentheses().Parent;
                });

            IdentifierNameSyntax newIdentifierName = IdentifierName(name);

            TNode newRight = nodeToRewrite.ReplaceNodes(nodes, (f, _) => newIdentifierName.WithTriviaFrom(f));

            return (isPatternExpression, newRight);
        }
    }
}