// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Refactorings;
using Roslynator.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OptimizeStringBuilderAppendCallCodeFixProvider))]
    [Shared]
    public sealed class OptimizeStringBuilderAppendCallCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.OptimizeStringBuilderAppendCall); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(
                root,
                context.Span,
                out SyntaxNode node,
                getInnermostNodeForTie: false,
                predicate: f => f.IsKind(SyntaxKind.Argument, SyntaxKind.InvocationExpression)))
            {
                return;
            }

            Diagnostic diagnostic = context.Diagnostics[0];
            Document document = context.Document;

            if (node is ArgumentSyntax argument)
            {
                SimpleMemberInvocationExpressionInfo invocationInfo = SyntaxInfo.SimpleMemberInvocationExpressionInfo((InvocationExpressionSyntax)argument.Parent.Parent);

                CodeAction codeAction = CodeAction.Create(
                    $"Optimize '{invocationInfo.NameText}' call",
                    ct => RefactorAsync(document, argument, invocationInfo, ct),
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
            else if (node is InvocationExpressionSyntax invocationExpression)
            {
                SimpleMemberInvocationExpressionInfo invocationInfo = SyntaxInfo.SimpleMemberInvocationExpressionInfo(invocationExpression);

                CodeAction codeAction = CodeAction.Create(
                    $"Optimize '{invocationInfo.NameText}' call",
                    ct => RefactorAsync(document, invocationInfo, ct),
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
        }

        public static Task<Document> RefactorAsync(
            Document document,
            in SimpleMemberInvocationExpressionInfo invocationInfo,
            CancellationToken cancellationToken)
        {
            SimpleMemberInvocationExpressionInfo invocationInfo2 = SyntaxInfo.SimpleMemberInvocationExpressionInfo(invocationInfo.Expression);

            SyntaxTriviaList trivia = invocationInfo2.InvocationExpression
                .GetTrailingTrivia()
                .EmptyIfWhitespace()
                .AddRange(invocationInfo.InvocationExpression.GetTrailingTrivia());

            InvocationExpressionSyntax newNode = invocationInfo2
                .WithName("AppendLine")
                .InvocationExpression
                .WithTrailingTrivia(trivia);

            return document.ReplaceNodeAsync(invocationInfo.InvocationExpression, newNode, cancellationToken);
        }

        public static async Task<Document> RefactorAsync(
            Document document,
            ArgumentSyntax argument,
            SimpleMemberInvocationExpressionInfo invocationInfo,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            InvocationExpressionSyntax invocation = invocationInfo.InvocationExpression;
            InvocationExpressionSyntax newInvocation;

            bool isAppendLine = string.Equals(invocationInfo.NameText, "AppendLine", StringComparison.Ordinal);

            ExpressionSyntax expression = argument.Expression;

            switch (expression.Kind())
            {
                case SyntaxKind.InterpolatedStringExpression:
                    {
                        newInvocation = ConvertInterpolatedStringExpressionToInvocationExpression((InterpolatedStringExpressionSyntax)expression, invocationInfo, semanticModel);
                        break;
                    }
                case SyntaxKind.AddExpression:
                    {
                        ImmutableArray<ExpressionSyntax> expressions = SyntaxInfo.BinaryExpressionInfo((BinaryExpressionSyntax)expression)
                            .AsChain()
                            .ToImmutableArray();

                        newInvocation = invocation
                            .ReplaceNode(invocationInfo.Name, IdentifierName("Append").WithTriviaFrom(invocationInfo.Name))
                            .WithArgumentList(invocation.ArgumentList.WithArguments(SingletonSeparatedList(Argument(ReplaceStringLiteralWithCharacterLiteral(expressions[0])))).WithoutTrailingTrivia());

                        for (int i = 1; i < expressions.Length; i++)
                        {
                            ExpressionSyntax argumentExpression = expressions[i];

                            string methodName;
                            if (i == expressions.Length - 1
                                && isAppendLine
                                && semanticModel
                                    .GetTypeInfo(argumentExpression, cancellationToken)
                                    .ConvertedType?
                                    .SpecialType == SpecialType.System_String)
                            {
                                methodName = "AppendLine";
                            }
                            else
                            {
                                methodName = "Append";

                                argumentExpression = ReplaceStringLiteralWithCharacterLiteral(argumentExpression);
                            }

                            newInvocation = SimpleMemberInvocationExpression(
                                newInvocation,
                                IdentifierName(methodName),
                                ArgumentList(Argument(argumentExpression)));

                            if (i == expressions.Length - 1
                                && isAppendLine
                                && !string.Equals(methodName, "AppendLine", StringComparison.Ordinal))
                            {
                                newInvocation = SimpleMemberInvocationExpression(
                                    newInvocation,
                                    IdentifierName("AppendLine"),
                                    ArgumentList());
                            }
                        }

                        break;
                    }
                default:
                    {
                        newInvocation = CreateInvocationExpression(
                            (InvocationExpressionSyntax)expression,
                            invocation);

                        if (isAppendLine)
                            newInvocation = SimpleMemberInvocationExpression(newInvocation, IdentifierName("AppendLine"), ArgumentList());

                        break;
                    }
            }

            newInvocation = newInvocation
                .WithTriviaFrom(invocation)
                .WithFormatterAnnotation();

            return await document.ReplaceNodeAsync(invocation, newInvocation, cancellationToken).ConfigureAwait(false);
        }

        private static InvocationExpressionSyntax ConvertInterpolatedStringExpressionToInvocationExpression(
            InterpolatedStringExpressionSyntax interpolatedString,
            in SimpleMemberInvocationExpressionInfo invocationInfo,
            SemanticModel semanticModel)
        {
            bool isVerbatim = interpolatedString.IsVerbatim();

            bool isAppendLine = string.Equals(invocationInfo.NameText, "AppendLine", StringComparison.Ordinal);

            InvocationExpressionSyntax invocation = invocationInfo.InvocationExpression;

            InvocationExpressionSyntax newExpression = null;

            SyntaxList<InterpolatedStringContentSyntax> contents = interpolatedString.Contents;

            for (int i = 0; i < contents.Count; i++)
            {
                (SyntaxKind contentKind, string methodName, ImmutableArray<ArgumentSyntax> arguments) = ConvertInterpolatedStringToStringBuilderMethodRefactoring.Refactor(contents[i], isVerbatim);

                if (i == contents.Count - 1
                    && isAppendLine
                    && string.Equals(methodName, "Append", StringComparison.Ordinal)
                    && (contentKind == SyntaxKind.InterpolatedStringText
                        || semanticModel.IsImplicitConversion(((InterpolationSyntax)contents[i]).Expression, semanticModel.Compilation.GetSpecialType(SpecialType.System_String))))
                {
                    methodName = "AppendLine";
                }
                else if (methodName == "Append")
                {
                    arguments = ReplaceStringLiteralWithCharacterLiteral(arguments);
                }

                if (newExpression == null)
                {
                    arguments = arguments.Replace(arguments[0], arguments[0].WithLeadingTrivia(interpolatedString.GetLeadingTrivia()));

                    newExpression = invocation
                        .ReplaceNode(invocationInfo.Name, IdentifierName(methodName).WithTriviaFrom(invocationInfo.Name))
                        .WithArgumentList(invocation.ArgumentList.WithArguments(arguments.ToSeparatedSyntaxList()).WithoutTrailingTrivia());
                }
                else
                {
                    newExpression = SimpleMemberInvocationExpression(
                        newExpression,
                        IdentifierName(methodName),
                        ArgumentList(arguments.ToSeparatedSyntaxList()));
                }

                if (i == contents.Count - 1
                    && isAppendLine
                    && !string.Equals(methodName, "AppendLine", StringComparison.Ordinal))
                {
                    newExpression = SimpleMemberInvocationExpression(
                        newExpression,
                        IdentifierName("AppendLine"),
                        ArgumentList());
                }
            }

            return newExpression;
        }

        private static InvocationExpressionSyntax CreateInvocationExpression(
            InvocationExpressionSyntax innerInvocationExpression,
            InvocationExpressionSyntax outerInvocationExpression)
        {
            SimpleMemberInvocationExpressionInfo invocationInfo = SyntaxInfo.SimpleMemberInvocationExpressionInfo(innerInvocationExpression);

            switch (invocationInfo.NameText)
            {
                case "Substring":
                    {
                        SeparatedSyntaxList<ArgumentSyntax> arguments = invocationInfo.Arguments;

                        switch (arguments.Count)
                        {
                            case 1:
                                {
                                    ArgumentListSyntax argumentList = ArgumentList(
                                        Argument(invocationInfo.Expression),
                                        arguments[0],
                                        Argument(
                                            SubtractExpression(
                                                SimpleMemberAccessExpression(invocationInfo.Expression, IdentifierName("Length")),
                                                arguments[0].Expression)));

                                    return CreateNewInvocationExpression(outerInvocationExpression, "Append", argumentList);
                                }
                            case 2:
                                {
                                    ArgumentListSyntax argumentList = ArgumentList(
                                        Argument(invocationInfo.Expression),
                                        arguments[0],
                                        arguments[1]
                                    );

                                    return CreateNewInvocationExpression(outerInvocationExpression, "Append", argumentList);
                                }
                            default:
                                {
                                    throw new InvalidOperationException();
                                }
                        }
                    }
                case "Remove":
                    {
                        SeparatedSyntaxList<ArgumentSyntax> arguments = invocationInfo.Arguments;

                        ArgumentListSyntax argumentList = ArgumentList(
                            Argument(invocationInfo.Expression),
                            Argument(NumericLiteralExpression(0)),
                            arguments[0]
                        );

                        return CreateNewInvocationExpression(outerInvocationExpression, "Append", argumentList);
                    }
                case "Format":
                    {
                        return CreateNewInvocationExpression(outerInvocationExpression, "AppendFormat", invocationInfo.ArgumentList);
                    }
                case "Join":
                    {
                        return CreateNewInvocationExpression(outerInvocationExpression, "AppendJoin", invocationInfo.ArgumentList);
                    }
            }

            Debug.Fail(innerInvocationExpression.ToString());
            return outerInvocationExpression;
        }

        private static InvocationExpressionSyntax CreateNewInvocationExpression(InvocationExpressionSyntax invocationExpression, string methodName, ArgumentListSyntax argumentList)
        {
            var memberAccess = (MemberAccessExpressionSyntax)invocationExpression.Expression;

            return invocationExpression
                .WithExpression(memberAccess.WithName(IdentifierName(methodName).WithTriviaFrom(memberAccess.Name)))
                .WithArgumentList(argumentList);
        }

        private static ExpressionSyntax ReplaceStringLiteralWithCharacterLiteral(ExpressionSyntax expression)
        {
            if (expression.IsKind(SyntaxKind.StringLiteralExpression))
            {
                var literalExpression = (LiteralExpressionSyntax)expression;

                if (literalExpression.Token.ValueText.Length == 1)
                    return SyntaxRefactorings.ReplaceStringLiteralWithCharacterLiteral(literalExpression);
            }

            return expression;
        }

        private static ImmutableArray<ArgumentSyntax> ReplaceStringLiteralWithCharacterLiteral(ImmutableArray<ArgumentSyntax> arguments)
        {
            ArgumentSyntax argument = arguments.SingleOrDefault(shouldThrow: false);

            if (argument != null)
            {
                ExpressionSyntax expression = argument.Expression;

                if (expression != null)
                {
                    ExpressionSyntax newExpression = ReplaceStringLiteralWithCharacterLiteral(expression);

                    if (newExpression != expression)
                        arguments = arguments.Replace(argument, argument.WithExpression(newExpression));
                }
            }

            return arguments;
        }
    }
}
