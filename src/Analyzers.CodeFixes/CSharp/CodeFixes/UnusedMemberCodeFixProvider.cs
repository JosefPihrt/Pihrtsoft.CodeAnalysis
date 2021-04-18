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

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UnusedMemberCodeFixProvider))]
    [Shared]
    public sealed class UnusedMemberCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.RemoveUnusedMemberDeclaration); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindNode(root, context.Span, out SyntaxNode node, predicate: f => Predicate(f)))
            {
                return;
            }

            Diagnostic diagnostic = context.Diagnostics[0];

            CodeAction codeAction = CodeAction.Create(
                $"Remove '{CSharpUtility.GetIdentifier(node).ValueText}'",
                cancellationToken => RefactorAsync(context.Document, node, cancellationToken),
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        private static Task<Document> RefactorAsync(
            Document document,
            SyntaxNode node,
            CancellationToken cancellationToken)
        {
            if (node is MemberDeclarationSyntax memberDeclaration)
                return document.RemoveMemberAsync(memberDeclaration, cancellationToken);

            if (node.Kind() == SyntaxKind.VariableDeclarator
                && node.Parent is VariableDeclarationSyntax variableDeclaration
                && variableDeclaration.Variables.Count == 1)
            {
                return document.RemoveMemberAsync((MemberDeclarationSyntax)variableDeclaration.Parent, cancellationToken);
            }

            return document.RemoveNodeAsync(node, cancellationToken);
        }

        private static bool Predicate(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.DelegateDeclaration:
                case SyntaxKind.EventDeclaration:
                case SyntaxKind.EventFieldDeclaration:
                case SyntaxKind.FieldDeclaration:
                case SyntaxKind.MethodDeclaration:
                case SyntaxKind.PropertyDeclaration:
                case SyntaxKind.VariableDeclarator:
                    return true;
                default:
                    return false;
            }
        }
    }
}
