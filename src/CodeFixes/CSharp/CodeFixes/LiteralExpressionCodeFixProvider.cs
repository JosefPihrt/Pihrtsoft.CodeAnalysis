// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(LiteralExpressionCodeFixProvider))]
    [Shared]
    public sealed class LiteralExpressionCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CompilerDiagnosticIdentifiers.TooManyCharactersInCharacterLiteral); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics[0];

            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.ReplaceCharacterLiteralWithStringLiteral))
                return;

            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out LiteralExpressionSyntax literalExpression))
                return;

            string text = literalExpression.Token.Text;

            if (text.Length > 2
                && text[0] == '\''
                && text[text.Length - 1] == '\'')
            {
                CodeAction codeAction = CodeAction.Create(
                    "Replace character literal with string literal",
                    ct =>
                    {
                        ExpressionSyntax newExpression = ParseExpression("\"" + text.Substring(1, text.Length - 2) + "\"")
                            .WithTriviaFrom(literalExpression);

                        return context.Document.ReplaceNodeAsync(literalExpression, newExpression, ct);
                    },
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
        }
    }
}
