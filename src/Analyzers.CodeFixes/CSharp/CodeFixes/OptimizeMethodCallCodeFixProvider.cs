// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
using Roslynator.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OptimizeMethodCallCodeFixProvider))]
    [Shared]
    public sealed class OptimizeMethodCallCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.OptimizeMethodCall); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out SyntaxNode node, predicate: f => f.IsKind(SyntaxKind.InvocationExpression, SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression, SyntaxKind.IfStatement)))
                return;

            Document document = context.Document;

            Diagnostic diagnostic = context.Diagnostics[0];

            switch (node.Kind())
            {
                case SyntaxKind.InvocationExpression:
                    {
                        var invocationExpression = (InvocationExpressionSyntax)node;

                        SimpleMemberInvocationExpressionInfo invocationInfo = SyntaxInfo.SimpleMemberInvocationExpressionInfo(invocationExpression);

                        switch (invocationInfo.NameText)
                        {
                            case "Compare":
                                {
                                    CodeAction codeAction = CodeAction.Create(
                                        "Call 'CompareOrdinal' instead of 'Compare'",
                                        ct => CallCompareOrdinalInsteadOfCompareAsync(document, invocationInfo, ct),
                                        GetEquivalenceKey(diagnostic, "CallCompareOrdinalInsteadOfCompare"));

                                    context.RegisterCodeFix(codeAction, diagnostic);
                                    break;
                                }
                            case "Join":
                                {
                                    CodeAction codeAction = CodeAction.Create(
                                        "Call 'Concat' instead of 'Join'",
                                        cancellationToken => CallStringConcatInsteadOfStringJoinAsync(document, invocationExpression, cancellationToken),
                                        GetEquivalenceKey(diagnostic, "CallConcatInsteadOfJoin"));

                                    context.RegisterCodeFix(codeAction, diagnostic);
                                    break;
                                }
                            case "Assert":
                                {
                                    CodeAction codeAction = CodeAction.Create(
                                        "Call 'Fail' instead of 'Assert'",
                                        ct => CallDebugFailInsteadOfDebugAssertAsync(context.Document, invocationExpression, ct),
                                        GetEquivalenceKey(diagnostic, "CallFailInsteadOfAssert"));

                                    context.RegisterCodeFix(codeAction, diagnostic);
                                    break;
                                }
                        }

                        break;
                    }
                case SyntaxKind.EqualsExpression:
                case SyntaxKind.NotEqualsExpression:
                    {
                        var equalityExpression = (BinaryExpressionSyntax)node;

                        CodeAction codeAction = CodeAction.Create(
                            "Call 'Equals' instead of 'Compare'",
                            ct => CallEqualsInsteadOfCompareAsync(document, equalityExpression, ct),
                            GetEquivalenceKey(diagnostic, "CallEqualsInsteadOfCompare"));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
                case SyntaxKind.IfStatement:
                    {
                        var ifStatement = (IfStatementSyntax)node;

                        CodeAction codeAction = CodeAction.Create(
                            "Use [] instead of 'ContainsKey'",
                            ct => UseElementAccessInsteadOfContainsKeyAsync(document, ifStatement, ct),
                            GetEquivalenceKey(diagnostic, "UseElementAccessInsteadOfContainsKey"));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
            }
        }

        private static Task<Document> CallCompareOrdinalInsteadOfCompareAsync(
            Document document,
            in SimpleMemberInvocationExpressionInfo invocationInfo,
            CancellationToken cancellationToken)
        {
            InvocationExpressionSyntax invocationExpression = invocationInfo.InvocationExpression;

            MemberAccessExpressionSyntax memberAccessExpression = invocationInfo.MemberAccessExpression;

            MemberAccessExpressionSyntax newMemberAccessExpression = memberAccessExpression.WithName(IdentifierName("CompareOrdinal").WithTriviaFrom(memberAccessExpression.Name));

            ArgumentListSyntax argumentList = invocationExpression.ArgumentList;

            ArgumentListSyntax newArgumentList = argumentList.WithArguments(argumentList.Arguments.RemoveAt(2));

            InvocationExpressionSyntax newInvocationExpression = invocationExpression.Update(newMemberAccessExpression, newArgumentList);

            return document.ReplaceNodeAsync(invocationExpression, newInvocationExpression, cancellationToken);
        }

        private static Task<Document> CallEqualsInsteadOfCompareAsync(
            Document document,
            BinaryExpressionSyntax equalityExpression,
            CancellationToken cancellationToken)
        {
            if (!(equalityExpression.Left.WalkDownParentheses() is InvocationExpressionSyntax invocationExpression))
                invocationExpression = (InvocationExpressionSyntax)equalityExpression.Right.WalkDownParentheses();

            ExpressionSyntax newExpression = SyntaxRefactorings.ChangeInvokedMethodName(invocationExpression, "Equals");

            if (equalityExpression.IsKind(SyntaxKind.NotEqualsExpression))
                newExpression = LogicalNotExpression(newExpression.WithoutTrivia());

            newExpression = newExpression.WithTriviaFrom(equalityExpression);

            return document.ReplaceNodeAsync(equalityExpression, newExpression, cancellationToken);
        }

        private static Task<Document> CallStringConcatInsteadOfStringJoinAsync(
            Document document,
            InvocationExpressionSyntax invocation,
            CancellationToken cancellationToken)
        {
            var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

            MemberAccessExpressionSyntax newMemberAccess = memberAccess.WithName(IdentifierName("Concat").WithTriviaFrom(memberAccess.Name));

            ArgumentListSyntax argumentList = invocation.ArgumentList;
            SeparatedSyntaxList<ArgumentSyntax> arguments = argumentList.Arguments;

            ArgumentListSyntax newArgumentList = argumentList
                .WithArguments(arguments.RemoveAt(0))
                .WithOpenParenToken(argumentList.OpenParenToken.AppendToTrailingTrivia(arguments[0].GetLeadingAndTrailingTrivia()));

            InvocationExpressionSyntax newInvocation = invocation
                .WithExpression(newMemberAccess)
                .WithArgumentList(newArgumentList);

            return document.ReplaceNodeAsync(invocation, newInvocation, cancellationToken);
        }

        private static Task<Document> CallDebugFailInsteadOfDebugAssertAsync(
            Document document,
            InvocationExpressionSyntax invocation,
            CancellationToken cancellationToken)
        {
            ArgumentListSyntax argumentList = invocation.ArgumentList;
            SeparatedSyntaxList<ArgumentSyntax> arguments = argumentList.Arguments;

            if (arguments.Count == 1)
            {
                ArgumentSyntax argument = arguments[0];
                arguments = arguments.ReplaceAt(0, argument.WithExpression(StringLiteralExpression("").WithTriviaFrom(argument.Expression)));
            }
            else
            {
                arguments = arguments.RemoveAt(0);
            }

            InvocationExpressionSyntax newInvocation = SyntaxRefactorings.ChangeInvokedMethodName(invocation, "Fail")
                .WithArgumentList(argumentList.WithArguments(arguments))
                .WithFormatterAnnotation();

            return document.ReplaceNodeAsync(invocation, newInvocation, cancellationToken);
        }

        private static Task<Document> UseElementAccessInsteadOfContainsKeyAsync(
            Document document,
            IfStatementSyntax ifStatement,
            CancellationToken cancellationToken)
        {
            StatementSyntax statement = (ifStatement.Condition.IsKind(SyntaxKind.LogicalNotExpression))
                ? ifStatement.Else.Statement
                : ifStatement.Statement;

            statement = statement
                .SingleNonBlockStatementOrDefault()
                .WithTriviaFrom(ifStatement);

            return document.ReplaceNodeAsync(ifStatement, statement, cancellationToken);
        }
    }
}
