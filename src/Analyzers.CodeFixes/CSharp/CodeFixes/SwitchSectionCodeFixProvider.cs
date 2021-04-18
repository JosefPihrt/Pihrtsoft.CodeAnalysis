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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SwitchSectionCodeFixProvider))]
    [Shared]
    public sealed class SwitchSectionCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.RemoveRedundantDefaultSwitchSection,
                    DiagnosticIdentifiers.DefaultLabelShouldBeLastLabelInSwitchSection,
                    DiagnosticIdentifiers.AddBracesToSwitchSectionWithMultipleStatements,
                    DiagnosticIdentifiers.MergeSwitchSectionsWithEquivalentContent);
            }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out SwitchSectionSyntax switchSection))
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.RemoveRedundantDefaultSwitchSection:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Remove redundant switch section",
                                cancellationToken => RemoveRedundantDefaultSwitchSectionRefactoring.RefactorAsync(context.Document, switchSection, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.DefaultLabelShouldBeLastLabelInSwitchSection:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Move default label to the last position",
                                cancellationToken => DefaultLabelShouldBeLastLabelInSwitchSectionRefactoring.RefactorAsync(context.Document, switchSection, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.AddBracesToSwitchSectionWithMultipleStatements:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                AddBracesToSwitchSectionRefactoring.Title,
                                cancellationToken => AddBracesToSwitchSectionRefactoring.RefactorAsync(context.Document, switchSection, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.MergeSwitchSectionsWithEquivalentContent:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Merge sections",
                                cancellationToken => MergeSwitchSectionsRefactoring.RefactorAsync(context.Document, switchSection, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }
        }
    }
}
