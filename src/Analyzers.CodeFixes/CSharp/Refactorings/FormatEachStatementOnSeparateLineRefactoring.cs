// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Refactorings
{
    internal static class AddNewLineBeforeStatementRefactoring
    {
        public static async Task<Document> RefactorAsync(
            Document document,
            StatementSyntax statement,
            CancellationToken cancellationToken)
        {
            StatementSyntax newStatement = statement
                .WithLeadingTrivia(statement.GetLeadingTrivia().Insert(0, CSharpFactory.NewLine()))
                .WithFormatterAnnotation();

            if (statement.IsParentKind(SyntaxKind.Block))
            {
                var block = (BlockSyntax)statement.Parent;

                if (block.IsSingleLine(includeExteriorTrivia: false, cancellationToken: cancellationToken))
                {
                    SyntaxTriviaList triviaList = block.CloseBraceToken.LeadingTrivia
                        .Add(CSharpFactory.NewLine());

                    BlockSyntax newBlock = block
                        .WithCloseBraceToken(block.CloseBraceToken.WithLeadingTrivia(triviaList))
                        .WithStatements(block.Statements.Replace(statement, newStatement))
                        .WithFormatterAnnotation();

                    return await document.ReplaceNodeAsync(block, newBlock, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    return await document.ReplaceNodeAsync(statement, newStatement, cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                return await document.ReplaceNodeAsync(statement, newStatement, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
