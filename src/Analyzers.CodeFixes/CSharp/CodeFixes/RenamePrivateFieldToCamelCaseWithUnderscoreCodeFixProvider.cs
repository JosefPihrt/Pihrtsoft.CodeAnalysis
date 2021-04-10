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
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Rename;
using Roslynator.CodeFixes;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RenamePrivateFieldToCamelCaseWithUnderscoreCodeFixProvider))]
    [Shared]
    public sealed class RenamePrivateFieldToCamelCaseWithUnderscoreCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.RenamePrivateFieldToCamelCaseWithUnderscore); }
        }

        public override FixAllProvider GetFixAllProvider() => null;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out VariableDeclaratorSyntax declarator))
                return;

            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

            ISymbol symbol = semanticModel.GetDeclaredSymbol(declarator, context.CancellationToken);

            string oldName = declarator.Identifier.ValueText;
            string newName = StringUtility.ToCamelCase(oldName, prefixWithUnderscore: true);

            newName = NameGenerator.Default.EnsureUniqueName(
                newName,
                semanticModel,
                declarator.Identifier.SpanStart);

            CodeAction codeAction = CodeAction.Create(
                $"Rename '{oldName}' to '{newName}'",
                cancellationToken => RefactorAsync(context.Document, symbol, newName, cancellationToken),
                DiagnosticIdentifiers.RenamePrivateFieldToCamelCaseWithUnderscore);

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }

        private static Task<Solution> RefactorAsync(
            Document document,
            ISymbol symbol,
            string newName,
            CancellationToken cancellationToken)
        {
            return Renamer.RenameSymbolAsync(
                document.Solution(),
                symbol,
                newName,
                default(OptionSet),
                cancellationToken);
        }
    }
}