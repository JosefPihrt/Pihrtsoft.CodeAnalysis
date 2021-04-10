// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.Formatting.CSharp;

namespace Roslynator.Formatting.CodeFixes.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BinaryExpressionCodeFixProvider))]
    [Shared]
    public sealed class BinaryExpressionCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.AddNewLineBeforeBinaryOperatorInsteadOfAfterItOrViceVersa); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out BinaryExpressionSyntax binaryExpression))
                return;

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];

            if (DiagnosticProperties.ContainsInvert(diagnostic.Properties))
            {
                CodeAction codeAction = CodeAction.Create(
                    $"Add newline after '{binaryExpression.OperatorToken.ToString()}' instead of before it",
                    ct => CodeFixHelpers.AddNewLineAfterInsteadOfBeforeAsync(document, binaryExpression.Left, binaryExpression.OperatorToken, binaryExpression.Right, ct),
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
            else
            {
                CodeAction codeAction = CodeAction.Create(
                    $"Add newline before '{binaryExpression.OperatorToken.ToString()}' instead of after it",
                    ct => CodeFixHelpers.AddNewLineBeforeInsteadOfAfterAsync(document, binaryExpression.Left, binaryExpression.OperatorToken, binaryExpression.Right, ct),
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
        }
    }
}