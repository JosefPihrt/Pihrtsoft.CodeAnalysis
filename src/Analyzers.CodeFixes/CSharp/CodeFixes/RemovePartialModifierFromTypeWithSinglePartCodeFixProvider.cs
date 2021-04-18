// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemovePartialModifierFromTypeWithSinglePartCodeFixProvider))]
    [Shared]
    public sealed class RemovePartialModifierFromTypeWithSinglePartCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.RemovePartialModifierFromTypeWithSinglePart); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out TypeDeclarationSyntax typeDeclaration))
                return;

            Diagnostic diagnostic = context.Diagnostics[0];
            Document document = context.Document;

            CodeAction codeAction = CodeAction.Create(
                ModifiersCodeFixRegistrator.GetRemoveModifierTitle(SyntaxKind.PartialKeyword),
                ct =>
                {
                    TypeDeclarationSyntax newTypeDeclaration = typeDeclaration.ReplaceNodes(
                        typeDeclaration.Members.OfType<MethodDeclarationSyntax>().Where(f => f.Modifiers.Contains(SyntaxKind.PartialKeyword) && f.BodyOrExpressionBody() != null),
                        (f, _) => f.RemoveModifier(SyntaxKind.PartialKeyword));

                    int count = newTypeDeclaration.Members.Count;

                    for (int i = count - 1; i >= 0; i--)
                    {
                        if (newTypeDeclaration.Members[i] is MethodDeclarationSyntax method
                            && method.Modifiers.Contains(SyntaxKind.PartialKeyword))
                        {
                            newTypeDeclaration = SyntaxRefactorings.RemoveMember(newTypeDeclaration, method);
                        }
                    }

                    newTypeDeclaration = newTypeDeclaration.RemoveModifier(SyntaxKind.PartialKeyword);

                    return document.ReplaceNodeAsync(typeDeclaration, newTypeDeclaration, ct);
                },
                GetEquivalenceKey(DiagnosticIdentifiers.RemovePartialModifierFromTypeWithSinglePart));

            context.RegisterCodeFix(codeAction, diagnostic);
        }
    }
}
