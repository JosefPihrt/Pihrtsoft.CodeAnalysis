// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
using Roslynator.CSharp;
using Roslynator.Formatting.CSharp;

namespace Roslynator.Formatting.CodeFixes.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EnumDeclarationCodeFixProvider))]
    [Shared]
    public sealed class EnumDeclarationCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.AddNewLineBeforeEnumMember); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out EnumDeclarationSyntax enumDeclaration))
                return;

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];

            CodeAction codeAction = CodeAction.Create(
                CodeFixTitles.AddNewLine,
                ct => AddNewLineBeforeEnumMemberAsync(document, enumDeclaration, ct),
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        private static Task<Document> AddNewLineBeforeEnumMemberAsync(
            Document document,
            EnumDeclarationSyntax enumDeclaration,
            CancellationToken cancellationToken)
        {
            var rewriter = new AddNewLineBeforeEnumMemberAsyncRewriter(enumDeclaration);

            SyntaxNode newNode = rewriter.Visit(enumDeclaration).WithFormatterAnnotation();

            return document.ReplaceNodeAsync(enumDeclaration, newNode, cancellationToken);
        }

        private class AddNewLineBeforeEnumMemberAsyncRewriter : CSharpSyntaxRewriter
        {
            private readonly SyntaxToken[] _separators;

            public AddNewLineBeforeEnumMemberAsyncRewriter(EnumDeclarationSyntax enumDeclaration)
            {
                _separators = enumDeclaration.Members.GetSeparators().ToArray();
            }

            public override SyntaxToken VisitToken(SyntaxToken token)
            {
                if (_separators.Contains(token)
                    && !token.TrailingTrivia.Contains(SyntaxKind.EndOfLineTrivia))
                {
                    return token.TrimTrailingTrivia().AppendEndOfLineToTrailingTrivia();
                }

                return base.VisitToken(token);
            }
        }
    }
}
