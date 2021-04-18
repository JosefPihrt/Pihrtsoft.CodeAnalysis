// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
using Roslynator.CSharp.Refactorings;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InvocationExpressionCodeFixProvider))]
    [Shared]
    public sealed class InvocationExpressionCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.UseCountOrLengthPropertyInsteadOfAnyMethod,
                    DiagnosticIdentifiers.RemoveRedundantToStringCall,
                    DiagnosticIdentifiers.RemoveRedundantStringToCharArrayCall,
                    DiagnosticIdentifiers.CombineEnumerableWhereMethodChain,
                    DiagnosticIdentifiers.CallExtensionMethodAsInstanceMethod,
                    DiagnosticIdentifiers.CallThenByInsteadOfOrderBy);
            }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out InvocationExpressionSyntax invocation))
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.CombineEnumerableWhereMethodChain:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Combine 'Where' method chain",
                                cancellationToken => CombineEnumerableWhereMethodChainRefactoring.RefactorAsync(context.Document, invocation, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.UseCountOrLengthPropertyInsteadOfAnyMethod:
                        {
                            string propertyName = diagnostic.Properties["PropertyName"];

                            CodeAction codeAction = CodeAction.Create(
                                $"Use '{propertyName}' property instead of calling 'Any'",
                                cancellationToken => UseCountOrLengthPropertyInsteadOfAnyMethodRefactoring.RefactorAsync(context.Document, invocation, propertyName, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.RemoveRedundantToStringCall:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Remove redundant 'ToString' call",
                                cancellationToken => context.Document.ReplaceNodeAsync(invocation, RemoveInvocation(invocation).WithFormatterAnnotation(), cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.RemoveRedundantStringToCharArrayCall:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Remove redundant 'ToCharArray' call",
                                cancellationToken => context.Document.ReplaceNodeAsync(invocation, RemoveInvocation(invocation).WithFormatterAnnotation(), cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.CallExtensionMethodAsInstanceMethod:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                CallExtensionMethodAsInstanceMethodRefactoring.Title,
                                cancellationToken => CallExtensionMethodAsInstanceMethodRefactoring.RefactorAsync(context.Document, invocation, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case DiagnosticIdentifiers.CallThenByInsteadOfOrderBy:
                        {
                            SimpleMemberInvocationExpressionInfo invocationInfo = SyntaxInfo.SimpleMemberInvocationExpressionInfo(invocation);

                            string oldName = invocationInfo.NameText;

                            string newName = (string.Equals(oldName, "OrderBy", StringComparison.Ordinal))
                                ? "ThenBy"
                                : "ThenByDescending";

                            CodeAction codeAction = CodeAction.Create(
                                $"Call '{newName}' instead of '{oldName}'",
                                cancellationToken => CallThenByInsteadOfOrderByRefactoring.RefactorAsync(context.Document, invocation, newName, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }
        }

        private static ExpressionSyntax RemoveInvocation(InvocationExpressionSyntax invocation)
        {
            var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

            ArgumentListSyntax argumentList = invocation.ArgumentList;

            SyntaxToken closeParen = argumentList.CloseParenToken;

            return memberAccess.Expression
                .AppendToTrailingTrivia(
                    memberAccess.OperatorToken.GetAllTrivia()
                        .Concat(memberAccess.Name.GetLeadingAndTrailingTrivia())
                        .Concat(argumentList.OpenParenToken.GetAllTrivia())
                        .Concat(closeParen.LeadingTrivia)
                        .ToSyntaxTriviaList()
                        .EmptyIfWhitespace()
                        .AddRange(closeParen.TrailingTrivia));
        }
    }
}
