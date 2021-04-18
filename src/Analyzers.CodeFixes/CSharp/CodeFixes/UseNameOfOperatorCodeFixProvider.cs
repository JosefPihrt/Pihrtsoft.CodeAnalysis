// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Refactorings;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseNameOfOperatorCodeFixProvider))]
    [Shared]
    public sealed class UseNameOfOperatorCodeFixProvider : BaseCodeFixProvider
    {
        private const string Title = "Use nameof operator";

        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.UseNameOfOperator); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindNode(root, context.Span, out SyntaxNode node))
                return;

            Diagnostic diagnostic = context.Diagnostics[0];

            if (diagnostic.Properties.TryGetValue("Identifier", out string identifier))
            {
                CodeAction codeAction = CodeAction.Create(
                    Title,
                    cancellationToken => UseNameOfOperatorRefactoring.RefactorAsync(context.Document, (LiteralExpressionSyntax)node, identifier, cancellationToken),
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
            else
            {
                CodeAction codeAction = CodeAction.Create(
                    Title,
                    cancellationToken => UseNameOfOperatorRefactoring.RefactorAsync(context.Document, (InvocationExpressionSyntax)node, cancellationToken),
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
        }
    }
}
