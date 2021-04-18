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
using Roslynator.CodeFixes;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddBreakStatementToSwitchSectionCodeFixProvider))]
    [Shared]
    public sealed class AddBreakStatementToSwitchSectionCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    CompilerDiagnosticIdentifiers.ControlCannotFallThroughFromOneCaseLabelToAnother,
                    CompilerDiagnosticIdentifiers.ControlCannotFallOutOfSwitchFromFinalCaseLabel);
            }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics[0];

            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.AddBreakStatementToSwitchSection))
                return;

            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out SwitchSectionSyntax switchSection))
                return;

            CodeAction codeAction = CodeAction.Create(
                "Add break;",
                cancellationToken => RefactorAsync(context.Document, switchSection, cancellationToken),
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        public static Task<Document> RefactorAsync(
            Document document,
            SwitchSectionSyntax switchSection,
            CancellationToken cancellationToken)
        {
            SwitchSectionSyntax newNode = switchSection;

            SyntaxList<StatementSyntax> statements = switchSection.Statements;

            if (statements.Count == 1
                && statements[0].IsKind(SyntaxKind.Block))
            {
                var block = (BlockSyntax)statements[0];
                newNode = newNode.ReplaceNode(block, block.AddStatements(BreakStatement()));
            }
            else
            {
                newNode = switchSection.AddStatements(BreakStatement());
            }

            return document.ReplaceNodeAsync(switchSection, newNode, cancellationToken);
        }
    }
}
