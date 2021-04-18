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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UnsafeCodeFixProvider))]
    [Shared]
    public sealed class UnsafeCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CompilerDiagnosticIdentifiers.PointersAndFixedSizeBuffersMayOnlyBeUsedInUnsafeContext); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindNode(root, context.Span, out SyntaxNode node))
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case CompilerDiagnosticIdentifiers.PointersAndFixedSizeBuffersMayOnlyBeUsedInUnsafeContext:
                        {
                            var fStatement = false;
                            var fMemberDeclaration = false;

                            foreach (SyntaxNode ancestor in node.AncestorsAndSelf())
                            {
                                if (fStatement
                                    && fMemberDeclaration)
                                {
                                    break;
                                }

                                if (!fStatement
                                    && ancestor is StatementSyntax)
                                {
                                    fStatement = true;

                                    if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.WrapInUnsafeStatement))
                                        continue;

                                    var statement = (StatementSyntax)ancestor;

                                    if (statement.IsKind(SyntaxKind.Block)
                                        && statement.Parent is StatementSyntax)
                                    {
                                        statement = (StatementSyntax)statement.Parent;
                                    }

                                    if (statement.IsKind(SyntaxKind.UnsafeStatement))
                                        break;

                                    CodeAction codeAction = CodeAction.Create(
                                        "Wrap in unsafe block",
                                        cancellationToken =>
                                        {
                                            BlockSyntax block = (statement.IsKind(SyntaxKind.Block))
                                                ? (BlockSyntax)statement
                                                : SyntaxFactory.Block(statement);

                                            UnsafeStatementSyntax unsafeStatement = SyntaxFactory.UnsafeStatement(block).WithFormatterAnnotation();

                                            return context.Document.ReplaceNodeAsync(statement, unsafeStatement, cancellationToken);
                                        },
                                        GetEquivalenceKey(diagnostic, CodeFixIdentifiers.WrapInUnsafeStatement));

                                    context.RegisterCodeFix(codeAction, diagnostic);
                                }
                                else if (!fMemberDeclaration
                                    && ancestor is MemberDeclarationSyntax)
                                {
                                    fMemberDeclaration = true;

                                    if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.MakeContainingDeclarationUnsafe))
                                        continue;

                                    if (!CSharpFacts.CanHaveModifiers(ancestor.Kind()))
                                        continue;

                                    ModifiersCodeFixRegistrator.AddModifier(
                                        context,
                                        diagnostic,
                                        ancestor,
                                        SyntaxKind.UnsafeKeyword,
                                        title: "Make containing declaration unsafe",
                                        additionalKey: CodeFixIdentifiers.MakeContainingDeclarationUnsafe);
                                }
                            }

                            break;
                        }
                }
            }
        }
    }
}
