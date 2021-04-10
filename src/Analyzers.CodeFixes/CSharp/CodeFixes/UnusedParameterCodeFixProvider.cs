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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UnusedParameterCodeFixProvider))]
    [Shared]
    public sealed class UnusedParameterCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.UnusedParameter); }
        }

        public override FixAllProvider GetFixAllProvider()
        {
            return null;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out ParameterSyntax parameter))
                return;

            SyntaxNode parent = parameter.Parent;

            if (parent is BaseParameterListSyntax)
                parent = parent.Parent;

            if (!CSharpFacts.IsAnonymousFunctionExpression(parent.Kind()))
                return;

            Diagnostic diagnostic = context.Diagnostics[0];

            CodeAction codeAction = CodeAction.Create(
                $"Rename '{parameter.Identifier.ValueText}' to '_'",
                ct => RefactorAsync(context.Document, parameter, (AnonymousFunctionExpressionSyntax)parent, ct),
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        private static async Task<Solution> RefactorAsync(
            Document document,
            ParameterSyntax parameter,
            AnonymousFunctionExpressionSyntax anonymousFunction,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            IParameterSymbol parameterSymbol = semanticModel.GetDeclaredSymbol(parameter, cancellationToken);

            ISymbol anonymousFunctionSymbol = semanticModel.GetSymbol(anonymousFunction, cancellationToken);

            string newName = NameGenerators.UnderscoreSuffix.EnsureUniqueParameterName("_", anonymousFunctionSymbol, semanticModel, cancellationToken: cancellationToken);

            DocumentOptionSet options = await document.GetOptionsAsync(cancellationToken).ConfigureAwait(false);

            return await Renamer.RenameSymbolAsync(document.Solution(), parameterSymbol, newName, options, cancellationToken).ConfigureAwait(false);
        }
    }
}
