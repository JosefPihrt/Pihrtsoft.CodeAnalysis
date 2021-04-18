// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UnnecessaryExplicitUseOfEnumeratorCodeFixProvider))]
    [Shared]
    public sealed class UnnecessaryExplicitUseOfEnumeratorCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.UnnecessaryExplicitUseOfEnumerator); }
        }

        public override FixAllProvider GetFixAllProvider() => null;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out UsingStatementSyntax usingStatement))
                return;

            Diagnostic diagnostic = context.Diagnostics[0];

            CodeAction codeAction = CodeAction.Create(
                "Use foreach instead of enumerator",
                ct => RefactorAsync(context.Document, usingStatement, ct),
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        private static async Task<Document> RefactorAsync(
            Document document,
            UsingStatementSyntax usingStatement,
            CancellationToken cancellationToken)
        {
            VariableDeclaratorSyntax declarator = usingStatement.Declaration.Variables.Single();

            var whileStatement = (WhileStatementSyntax)usingStatement.Statement.SingleNonBlockStatementOrDefault();

            SimpleMemberInvocationExpressionInfo invocationInfo = SyntaxInfo.SimpleMemberInvocationExpressionInfo(declarator.Initializer.Value);

            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            string name = NameGenerator.Default.EnsureUniqueLocalName(DefaultNames.ForEachVariable, semanticModel, usingStatement.SpanStart, cancellationToken: cancellationToken);

            StatementSyntax statement = whileStatement.Statement;

            var rewriter = new Rewriter(
                semanticModel.GetDeclaredSymbol(declarator, cancellationToken),
                declarator.Identifier.ValueText,
                name,
                semanticModel,
                cancellationToken);

            var newStatement = (StatementSyntax)rewriter.Visit(statement);

            ForEachStatementSyntax forEachStatement = ForEachStatement(
                VarType(),
                Identifier(name).WithRenameAnnotation(),
                invocationInfo.Expression,
                newStatement);

            forEachStatement = forEachStatement
                .WithTriviaFrom(usingStatement)
                .WithFormatterAnnotation();

            return await document.ReplaceNodeAsync(usingStatement, forEachStatement, cancellationToken).ConfigureAwait(false);
        }

        private class Rewriter : CSharpSyntaxRewriter
        {
            private readonly ISymbol _symbol;
            private readonly string _name;
            private readonly IdentifierNameSyntax _newName;
            private readonly SemanticModel _semanticModel;
            private readonly CancellationToken _cancellationToken;

            public Rewriter(ISymbol symbol, string name, string newName, SemanticModel semanticModel, CancellationToken cancellationToken)
            {
                _symbol = symbol;
                _name = name;
                _newName = IdentifierName(newName);
                _semanticModel = semanticModel;
                _cancellationToken = cancellationToken;
            }

            public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
            {
                node = (MemberAccessExpressionSyntax)base.VisitMemberAccessExpression(node);

                if (node.IsKind(SyntaxKind.SimpleMemberAccessExpression)
                    && string.Equals((node.Expression as IdentifierNameSyntax)?.Identifier.ValueText, _name, StringComparison.Ordinal)
                    && string.Equals((node.Name as IdentifierNameSyntax)?.Identifier.ValueText, "Current", StringComparison.Ordinal)
                    && SymbolEqualityComparer.Default.Equals(_symbol, _semanticModel.GetSymbol(node.Expression, _cancellationToken)))
                {
                    return _newName.WithTriviaFrom(node);
                }

                return node;
            }
        }
    }
}
