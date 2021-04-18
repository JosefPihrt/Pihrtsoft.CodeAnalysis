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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConstructorDeclarationCodeFixProvider))]
    [Shared]
    public sealed class ConstructorDeclarationCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.RemoveRedundantBaseConstructorCall,
                    DiagnosticIdentifiers.RemoveRedundantConstructor,
                    DiagnosticIdentifiers.AbstractTypeShouldNotHavePublicConstructors);
            }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out ConstructorDeclarationSyntax constructor))
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.RemoveRedundantBaseConstructorCall:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Remove redundant base constructor call",
                                cancellationToken => RemoveRedundantBaseConstructorCallRefactoring.RefactorAsync(context.Document, constructor, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.RemoveRedundantConstructor:
                        {
                            CodeAction codeAction = CodeActionFactory.RemoveMemberDeclaration(
                                context.Document,
                                constructor,
                                title: "Remove redundant constructor",
                                equivalenceKey: GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.AbstractTypeShouldNotHavePublicConstructors:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Change accessibility to 'protected'",
                                cancellationToken => AbstractTypeShouldNotHavePublicConstructorsRefactoring.RefactorAsync(context.Document, constructor, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }
        }
    }
}
