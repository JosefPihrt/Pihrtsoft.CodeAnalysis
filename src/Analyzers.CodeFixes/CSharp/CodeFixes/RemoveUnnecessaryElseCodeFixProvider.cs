// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
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
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveUnnecessaryElseCodeFixProvider))]
    [Shared]
    public sealed class RemoveUnnecessaryElseCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.RemoveUnnecessaryElse); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out ElseClauseSyntax elseClause))
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.RemoveUnnecessaryElse:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Remove 'else'",
                                ct => RefactorAsync(context.Document, elseClause, ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }
        }

        private static Task<Document> RefactorAsync(
            Document document,
            ElseClauseSyntax elseClause,
            CancellationToken cancellationToken = default)
        {
            var ifStatement = (IfStatementSyntax)elseClause.Parent;

            List<StatementSyntax> newStatements;

            if (elseClause.Statement is BlockSyntax block)
            {
                SyntaxList<StatementSyntax> statements = block.Statements;

                if (!statements.Any())
                    return document.RemoveNodeAsync(elseClause, SyntaxRemoveOptions.KeepExteriorTrivia, cancellationToken);

                newStatements = new List<StatementSyntax>() { ifStatement.WithElse(null) };

                StatementSyntax lastStatement = statements.Last();

                SyntaxTriviaList leadingTrivia = lastStatement.GetLeadingTrivia();

                if (!leadingTrivia.Contains(SyntaxKind.EndOfLineTrivia))
                    statements = statements.Replace(lastStatement, lastStatement.WithLeadingTrivia(leadingTrivia.Insert(0, NewLine())));

                foreach (StatementSyntax statement in statements)
                    newStatements.Add(statement.WithFormatterAnnotation());
            }
            else
            {
                StatementSyntax statement = elseClause.Statement;

                SyntaxTriviaList leadingTrivia = statement.GetLeadingTrivia();

                if (!leadingTrivia.Contains(SyntaxKind.EndOfLineTrivia))
                    statement = statement.WithLeadingTrivia(leadingTrivia.Insert(0, NewLine()));

                statement = statement.WithFormatterAnnotation();

                newStatements = new List<StatementSyntax>() { ifStatement.WithElse(null), statement };
            }

            if (ifStatement.IsEmbedded())
            {
                BlockSyntax newBlock = Block(newStatements).WithFormatterAnnotation();

                return document.ReplaceNodeAsync(ifStatement, newBlock, cancellationToken);
            }
            else
            {
                return document.ReplaceNodeAsync(ifStatement, newStatements, cancellationToken);
            }
        }
    }
}
