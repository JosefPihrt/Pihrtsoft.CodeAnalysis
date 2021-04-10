// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ChangeOverridingMemberAccessibilityCodeFixProvider))]
    [Shared]
    public sealed class ChangeOverridingMemberAccessibilityCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CompilerDiagnosticIdentifiers.CannotChangeAccessModifiersWhenOverridingInheritedMember); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics[0];

            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.ChangeAccessibility))
                return;

            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(
                root,
                context.Span,
                out SyntaxNode node,
                predicate: f => CSharpOverriddenSymbolInfo.CanCreate(f)))
            {
                return;
            }

            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

            OverriddenSymbolInfo overrideInfo = CSharpOverriddenSymbolInfo.Create(node, semanticModel, context.CancellationToken);

            if (!overrideInfo.Success)
                return;

            Accessibility newAccessibility = overrideInfo.OverriddenSymbol.DeclaredAccessibility;

            CodeAction codeAction = CodeAction.Create(
                $"Change accessibility to '{SyntaxFacts.GetText(newAccessibility)}'",
                cancellationToken =>
                {
                    if (node.Kind() == SyntaxKind.VariableDeclarator)
                    {
                        node = node.Parent.Parent;
                    }

                    SyntaxNode newNode;

                    if (newAccessibility == Accessibility.Public
                        && node is AccessorDeclarationSyntax)
                    {
                        newNode = SyntaxAccessibility.WithoutExplicitAccessibility(node);
                    }
                    else
                    {
                        newNode = SyntaxAccessibility.WithExplicitAccessibility(node, newAccessibility);
                    }

                    return context.Document.ReplaceNodeAsync(node, newNode, cancellationToken);
                },
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
        }
    }
}
