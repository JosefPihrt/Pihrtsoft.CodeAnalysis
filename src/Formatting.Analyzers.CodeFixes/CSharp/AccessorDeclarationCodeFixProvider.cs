// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp;
using Roslynator.Formatting.CSharp;

namespace Roslynator.Formatting.CodeFixes.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AccessorDeclarationCodeFixProvider))]
    [Shared]
    public sealed class AccessorDeclarationCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.RemoveNewLinesFromAccessorWithSingleLineExpression,
                    DiagnosticIdentifiers.AddNewLineBeforeAccessorOfFullProperty);
            }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out AccessorDeclarationSyntax accessorDeclaration))
                return;

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];

            switch (diagnostic.Id)
            {
                case DiagnosticIdentifiers.RemoveNewLinesFromAccessorWithSingleLineExpression:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            CodeFixTitles.RemoveNewLines,
                            ct => RemoveNewLinesFromAccessorWithSingleLineExpressionAsync(document, accessorDeclaration, ct),
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
                case DiagnosticIdentifiers.AddNewLineBeforeAccessorOfFullProperty:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            CodeFixTitles.AddNewLine,
                            ct => AddNewLineBeforeAccessorOfFullPropertyAsync(accessorDeclaration, document, ct),
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
            }
        }

        private static Task<Document> RemoveNewLinesFromAccessorWithSingleLineExpressionAsync(
            Document document,
            AccessorDeclarationSyntax accessorDeclaration,
            CancellationToken cancellationToken)
        {
            AccessorDeclarationSyntax newAccessorDeclaration = accessorDeclaration
                .RemoveWhitespace(accessorDeclaration.Span)
                .WithFormatterAnnotation();

            return document.ReplaceNodeAsync(accessorDeclaration, newAccessorDeclaration, cancellationToken);
        }

        private static Task<Document> AddNewLineBeforeAccessorOfFullPropertyAsync(
            AccessorDeclarationSyntax accessorDeclaration,
            Document document,
            CancellationToken cancellationToken)
        {
            AccessorDeclarationSyntax newAccessorDeclaration = accessorDeclaration.AppendEndOfLineToLeadingTrivia();

            var accessorList = (AccessorListSyntax)accessorDeclaration.Parent;

            AccessorListSyntax newAccessorList = accessorList
                .WithAccessors(accessorList.Accessors.Replace(accessorDeclaration, newAccessorDeclaration))
                .WithFormatterAnnotation();

            return document.ReplaceNodeAsync(accessorList, newAccessorList, cancellationToken);
        }
    }
}
