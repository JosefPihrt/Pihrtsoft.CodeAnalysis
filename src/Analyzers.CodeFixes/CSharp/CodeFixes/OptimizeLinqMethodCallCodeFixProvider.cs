// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Analysis;
using Roslynator.CSharp.Refactorings;
using Roslynator.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;
using static Roslynator.CSharp.SyntaxRefactorings;
using static Roslynator.CSharp.SyntaxInfo;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OptimizeLinqMethodCallCodeFixProvider))]
    [Shared]
    public class OptimizeLinqMethodCallCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.OptimizeLinqMethodCall,
                    DiagnosticIdentifiers.UseElementAccess);
            }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(
                root,
                context.Span,
                out SyntaxNode node,
                predicate: f => f.IsKind(
                    SyntaxKind.InvocationExpression,
                    SyntaxKind.EqualsExpression,
                    SyntaxKind.NotEqualsExpression,
                    SyntaxKind.IsPatternExpression,
                    SyntaxKind.ConditionalExpression)))
            {
                return;
            }

            Diagnostic diagnostic = context.Diagnostics[0];
            Document document = context.Document;
            CancellationToken cancellationToken = context.CancellationToken;

            SyntaxKind kind = node.Kind();

            if (kind == SyntaxKind.InvocationExpression)
            {
                var invocation = (InvocationExpressionSyntax)node;

                SimpleMemberInvocationExpressionInfo invocationInfo = SimpleMemberInvocationExpressionInfo(invocation);

                if (diagnostic.Properties.TryGetValue("Name", out string name)
                    && name == "SimplifyLinqMethodChain")
                {
                    SimpleMemberInvocationExpressionInfo invocationInfo2 = SimpleMemberInvocationExpressionInfo(invocationInfo.Expression);

                    CodeAction codeAction = CodeAction.Create(
                        $"Combine '{invocationInfo2.NameText}' and '{invocationInfo.NameText}'",
                        ct => SimplifyLinqMethodChainAsync(document, invocationInfo, ct),
                        GetEquivalenceKey(diagnostic, "SimplifyLinqMethodChain"));

                    context.RegisterCodeFix(codeAction, diagnostic);
                    return;
                }

                switch (invocationInfo.NameText)
                {
                    case "Cast":
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Call 'OfType' instead of 'Where' and 'Cast'",
                                ct => CallOfTypeInsteadOfWhereAndCastAsync(document, invocationInfo, ct),
                                GetEquivalenceKey(diagnostic, "CallOfTypeInsteadOfWhereAndCast"));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            return;
                        }
                    case "Any":
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Combine 'Where' and 'Any'",
                                ct => CombineWhereAndAnyAsync(document, invocationInfo, ct),
                                GetEquivalenceKey(diagnostic, "CombineWhereAndAny"));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            return;
                        }
                    case "ToList":
                    case "ToImmutableArray":
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Call 'ConvertAll'",
                                ct => CallConvertAllInsteadOfSelectAsync(document, invocationInfo, ct),
                                GetEquivalenceKey(diagnostic, "CallConvertAllInsteadOfSelect"));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            return;
                        }
                    case "OfType":
                        {
                            TypeSyntax typeArgument = ((GenericNameSyntax)invocationInfo.Name).TypeArgumentList.Arguments.Single();

                            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

                            if (semanticModel.GetTypeSymbol(typeArgument, cancellationToken).IsValueType)
                            {
                                CodeAction codeAction = CodeAction.Create(
                                    "Remove redundant 'OfType' call",
                                    ct =>
                                    {
                                        ExpressionSyntax newNode = invocationInfo.Expression.WithTrailingTrivia(invocation.GetTrailingTrivia());
                                        return document.ReplaceNodeAsync(invocation, newNode, ct);
                                    },
                                    GetEquivalenceKey(diagnostic, "RemoveRedundantOfTypeCall"));

                                context.RegisterCodeFix(codeAction, diagnostic);
                            }
                            else
                            {
                                CodeAction codeAction = CodeAction.Create(
                                    "Call 'Where' instead of 'OfType'",
                                    ct => CallWhereInsteadOfOfTypeAsync(document, invocationInfo, ct),
                                    GetEquivalenceKey(diagnostic, "CallWhereInsteadOfOfType"));

                                context.RegisterCodeFix(codeAction, diagnostic);
                            }

                            return;
                        }
                    case "Reverse":
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Call 'OrderByDescending'",
                                ct => CallOrderByDescendingInsteadOfOrderByAndReverseAsync(document, invocationInfo, ct),
                                GetEquivalenceKey(diagnostic, "CallOrderByDescendingInsteadOfOrderByAndReverse"));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            return;
                        }
                    case "Where":
                        {
                            SimpleMemberInvocationExpressionInfo invocationInfo2 = SimpleMemberInvocationExpressionInfo(
                                invocationInfo.Expression);

                            CodeAction codeAction = CodeAction.Create(
                                $"Call '{invocationInfo2.NameText}' and 'Where' in reverse order",
                                ct => CallOrderByAndWhereInReverseOrderAsync(document, invocationInfo, invocationInfo2, ct),
                                GetEquivalenceKey(diagnostic, "CallOrderByAndWhereInReverseOrder"));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            return;
                        }
                    case "Select":
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Call 'Cast' instead of 'Select'",
                                ct => CallCastInsteadOfSelectAsync(document, invocation, ct),
                                GetEquivalenceKey(diagnostic, "CallCastInsteadOfSelect"));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            return;
                        }
                    case "FirstOrDefault":
                        {
                            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

                            CodeAction codeAction = CodeAction.Create(
                                "Call 'Find' instead of 'FirstOrDefault'",
                                ct => CallFindInsteadOfFirstOrDefaultAsync(document, invocationInfo, semanticModel, ct),
                                GetEquivalenceKey(diagnostic, "CallFindInsteadOfFirstOrDefault"));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            return;
                        }
                    case "First":
                        {
                            if (diagnostic.Properties.TryGetValue("MethodName", out string methodName)
                                && methodName == "Peek")
                            {
                                CodeAction codeAction = CodeAction.Create(
                                    "Call 'Peek' instead of 'First'",
                                    ct => document.ReplaceNodeAsync(invocation, ChangeInvokedMethodName(invocation, "Peek"), ct),
                                    GetEquivalenceKey(diagnostic, "CallPeekInsteadOfFirst"));

                                context.RegisterCodeFix(codeAction, diagnostic);
                            }
                            else
                            {
                                CodeAction codeAction = CodeAction.Create(
                                    "Use [] instead of calling 'First'",
                                    ct => UseElementAccessInsteadOfEnumerableMethodRefactoring.UseElementAccessInsteadOfFirstAsync(context.Document, invocation, ct),
                                    GetEquivalenceKey(diagnostic, "UseElementAccessInsteadOfFirst"));

                                context.RegisterCodeFix(codeAction, diagnostic);
                            }

                            return;
                        }
                    case "Count":
                        {
                            if (diagnostic.Properties.TryGetValue("PropertyName", out string propertyName))
                            {
                                if (diagnostic.Properties.TryGetValue("MethodName", out string methodName)
                                    && methodName == "Sum")
                                {
                                    CodeAction codeAction = CodeAction.Create(
                                        "Call 'Sum'",
                                        ct => CallSumInsteadOfSelectManyAndCountAsync(document, invocationInfo, propertyName, ct),
                                        GetEquivalenceKey(diagnostic, "CallSumInsteadOfSelectManyAndCount"));

                                    context.RegisterCodeFix(codeAction, diagnostic);
                                }
                                else
                                {
                                    CodeAction codeAction = CodeAction.Create(
                                        $"Use '{propertyName}' property instead of calling 'Count'",
                                        ct => UseCountOrLengthPropertyInsteadOfCountMethodAsync(document, invocation, diagnostic.Properties["PropertyName"], ct),
                                        GetEquivalenceKey(diagnostic, "UseCountOrLengthPropertyInsteadOfCountMethod"));

                                    context.RegisterCodeFix(codeAction, diagnostic);
                                }
                            }
                            else if (invocation.Parent is BinaryExpressionSyntax binaryExpression)
                            {
                                CodeAction codeAction = CodeAction.Create(
                                    "Call 'Any' instead of 'Count'",
                                    ct => CallAnyInsteadOfCountAsync(document, invocation, binaryExpression, ct),
                                    GetEquivalenceKey(diagnostic, "CallAnyInsteadOfCount"));

                                context.RegisterCodeFix(codeAction, diagnostic);
                            }

                            return;
                        }
                    case "ElementAt":
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Use [] instead of calling 'ElementAt'",
                                ct => UseElementAccessInsteadOfEnumerableMethodRefactoring.UseElementAccessInsteadOfElementAtAsync(document, invocation, ct),
                                GetEquivalenceKey(diagnostic, "UseElementAccessInsteadOfElementAt"));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            return;
                        }
                }
            }
            else if (kind == SyntaxKind.ConditionalExpression)
            {
                CodeAction codeAction = CodeAction.Create(
                    "Call 'FirstOrDefault' instead of ?:",
                    ct => CallFirstOrDefaultInsteadOfConditionalExpressionAsync(document, (ConditionalExpressionSyntax)node, ct),
                    GetEquivalenceKey(diagnostic, "CallFirstOrDefaultInsteadOfConditionalExpression"));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
            else if (kind.Is(
                SyntaxKind.EqualsExpression,
                SyntaxKind.NotEqualsExpression,
                SyntaxKind.IsPatternExpression))
            {
                CodeAction codeAction = CodeAction.Create(
                    "Call 'Any' instead of 'FirstOrDefault'",
                    ct => CallAnyInsteadOfFirstOrDefaultAsync(document, node, ct),
                    GetEquivalenceKey(diagnostic, "CallAnyInsteadOfFirstOrDefault"));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
        }

        private static Task<Document> CallOfTypeInsteadOfWhereAndCastAsync(
            Document document,
            in SimpleMemberInvocationExpressionInfo invocationInfo,
            CancellationToken cancellationToken)
        {
            SimpleMemberInvocationExpressionInfo invocationInfo2 = SimpleMemberInvocationExpressionInfo(invocationInfo.Expression);

            var genericName = (GenericNameSyntax)invocationInfo.Name;

            InvocationExpressionSyntax newInvocation = invocationInfo2.InvocationExpression.Update(
                invocationInfo2.MemberAccessExpression.WithName(genericName.WithIdentifier(Identifier("OfType"))),
                invocationInfo.ArgumentList.WithArguments(SeparatedList<ArgumentSyntax>()));

            return document.ReplaceNodeAsync(invocationInfo.InvocationExpression, newInvocation, cancellationToken);
        }

        private static Task<Document> CombineWhereAndAnyAsync(
            Document document,
            in SimpleMemberInvocationExpressionInfo invocationInfo,
            CancellationToken cancellationToken)
        {
            SimpleMemberInvocationExpressionInfo invocationInfo2 = SimpleMemberInvocationExpressionInfo(invocationInfo.Expression);

            SingleParameterLambdaExpressionInfo lambda = SingleParameterLambdaExpressionInfo((LambdaExpressionSyntax)invocationInfo.Arguments[0].Expression);
            SingleParameterLambdaExpressionInfo lambda2 = SingleParameterLambdaExpressionInfo((LambdaExpressionSyntax)invocationInfo2.Arguments[0].Expression);

            BinaryExpressionSyntax logicalAnd = LogicalAndExpression(
                ((ExpressionSyntax)lambda2.Body).Parenthesize(),
                ((ExpressionSyntax)lambda.Body).Parenthesize());

            InvocationExpressionSyntax newNode = invocationInfo2.InvocationExpression
                .ReplaceNode(invocationInfo2.Name, invocationInfo.Name.WithTriviaFrom(invocationInfo2.Name))
                .WithArgumentList(invocationInfo2.ArgumentList.ReplaceNode((ExpressionSyntax)lambda2.Body, logicalAnd));

            return document.ReplaceNodeAsync(invocationInfo.InvocationExpression, newNode, cancellationToken);
        }

        private static Task<Document> SimplifyLinqMethodChainAsync(
            Document document,
            in SimpleMemberInvocationExpressionInfo invocationInfo,
            CancellationToken cancellationToken = default)
        {
            SimpleMemberInvocationExpressionInfo invocationInfo2 = SimpleMemberInvocationExpressionInfo(invocationInfo.Expression);

            InvocationExpressionSyntax invocation = invocationInfo.InvocationExpression;
            InvocationExpressionSyntax invocation2 = invocationInfo2.InvocationExpression;

            InvocationExpressionSyntax newNode = invocation2.WithExpression(
                invocationInfo2.MemberAccessExpression.WithName(invocationInfo.Name.WithTriviaFrom(invocationInfo2.Name)));

            IEnumerable<SyntaxTrivia> trivia = invocation.DescendantTrivia(TextSpan.FromBounds(invocation2.Span.End, invocation.Span.End));

            if (trivia.Any(f => !f.IsWhitespaceOrEndOfLineTrivia()))
            {
                newNode = newNode.WithTrailingTrivia(trivia.Concat(invocation.GetTrailingTrivia()));
            }
            else
            {
                newNode = newNode.WithTrailingTrivia(invocation.GetTrailingTrivia());
            }

            return document.ReplaceNodeAsync(invocation, newNode, cancellationToken);
        }

        private static Task<Document> CallAnyInsteadOfFirstOrDefaultAsync(
            Document document,
            SyntaxNode node,
            CancellationToken cancellationToken)
        {
            NullCheckExpressionInfo nullCheck = NullCheckExpressionInfo(node, NullCheckStyles.ComparisonToNull | NullCheckStyles.IsNull);

            var invocation = (InvocationExpressionSyntax)nullCheck.Expression;

            ExpressionSyntax newNode = ChangeInvokedMethodName(invocation, "Any");

            if (node.IsKind(SyntaxKind.EqualsExpression, SyntaxKind.IsPatternExpression))
                newNode = LogicalNotExpression(newNode.TrimTrivia().Parenthesize());

            newNode = newNode.WithTriviaFrom(node);

            return document.ReplaceNodeAsync(node, newNode, cancellationToken);
        }

        private static Task<Document> CallWhereInsteadOfOfTypeAsync(
            Document document,
            in SimpleMemberInvocationExpressionInfo invocationInfo,
            CancellationToken cancellationToken)
        {
            InvocationExpressionSyntax invocationExpression = invocationInfo.InvocationExpression;

            ExpressionSyntax newNode = invocationExpression
                .WithExpression(invocationInfo.MemberAccessExpression.WithName(IdentifierName("Where").WithTriviaFrom(invocationInfo.Name)))
                .AddArgumentListArguments(
                    Argument(
                        SimpleLambdaExpression(
                            Parameter(Identifier(DefaultNames.LambdaParameter)),
                            NotEqualsExpression(
                                IdentifierName(DefaultNames.LambdaParameter),
                                NullLiteralExpression()))
                            .WithFormatterAnnotation()
                    )
                );

            return document.ReplaceNodeAsync(invocationExpression, newNode, cancellationToken);
        }

        private static Task<Document> CallCastInsteadOfSelectAsync(
            Document document,
            InvocationExpressionSyntax invocationExpression,
            CancellationToken cancellationToken)
        {
            var memberAccessExpression = (MemberAccessExpressionSyntax)invocationExpression.Expression;

            ArgumentSyntax lastArgument = invocationExpression.ArgumentList.Arguments.Last();

            var lambdaExpression = (LambdaExpressionSyntax)lastArgument.Expression;

            GenericNameSyntax newName = GenericName(
                Identifier("Cast"),
                CallCastInsteadOfSelectAnalysis.GetCastExpression(lambdaExpression.Body).Type);

            InvocationExpressionSyntax newInvocationExpression = invocationExpression
                .RemoveNode(lastArgument)
                .WithExpression(memberAccessExpression.WithName(newName));

            return document.ReplaceNodeAsync(invocationExpression, newInvocationExpression, cancellationToken);
        }

        private static Task<Document> CallFindInsteadOfFirstOrDefaultAsync(
            Document document,
            in SimpleMemberInvocationExpressionInfo invocationInfo,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(invocationInfo.Expression, cancellationToken);

            if ((typeSymbol as IArrayTypeSymbol)?.Rank == 1)
            {
                NameSyntax arrayName = ParseName("global::System.Array")
                    .WithLeadingTrivia(invocationInfo.InvocationExpression.GetLeadingTrivia())
                    .WithSimplifierAnnotation();

                MemberAccessExpressionSyntax newMemberAccess = SimpleMemberAccessExpression(
                    arrayName,
                    invocationInfo.OperatorToken,
                    IdentifierName("Find").WithTriviaFrom(invocationInfo.Name));

                ArgumentListSyntax argumentList = invocationInfo.ArgumentList;

                InvocationExpressionSyntax newInvocation = InvocationExpression(
                    newMemberAccess,
                    ArgumentList(
                        Argument(invocationInfo.Expression.WithoutTrivia()),
                        argumentList.Arguments[0])
                        .WithTriviaFrom(argumentList));

                return document.ReplaceNodeAsync(invocationInfo.InvocationExpression, newInvocation, cancellationToken);
            }
            else
            {
                IdentifierNameSyntax newName = IdentifierName("Find").WithTriviaFrom(invocationInfo.Name);

                return document.ReplaceNodeAsync(invocationInfo.Name, newName, cancellationToken);
            }
        }

        public static Task<Document> UseCountOrLengthPropertyInsteadOfCountMethodAsync(
            Document document,
            InvocationExpressionSyntax invocation,
            string propertyName,
            CancellationToken cancellationToken = default)
        {
            var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

            IEnumerable<SyntaxTrivia> trailingTrivia = memberAccess.Name.GetTrailingTrivia().Where(f => !f.IsWhitespaceOrEndOfLineTrivia())
                .Concat(invocation.ArgumentList.DescendantTrivia().Where(f => !f.IsWhitespaceOrEndOfLineTrivia()))
                .Concat(invocation.GetTrailingTrivia());

            IdentifierNameSyntax newName = IdentifierName(propertyName)
                .WithLeadingTrivia(memberAccess.Name.GetLeadingTrivia())
                .WithTrailingTrivia(trailingTrivia);

            MemberAccessExpressionSyntax newNode = memberAccess.WithName(newName);

            return document.ReplaceNodeAsync(invocation, newNode, cancellationToken);
        }

        private static Task<Document> CallAnyInsteadOfCountAsync(
            Document document,
            InvocationExpressionSyntax invocationExpression,
            BinaryExpressionSyntax binaryExpression,
            CancellationToken cancellationToken = default)
        {
            ExpressionSyntax left = binaryExpression.Left;

            ExpressionSyntax newNode = null;

            switch (binaryExpression.Kind())
            {
                case SyntaxKind.EqualsExpression:
                    {
                        // Count() == 0 >>> !Any()
                        newNode = ChangeInvokedMethodName(invocationExpression, "Any");
                        newNode = LogicalNotExpression(newNode.Parenthesize());
                        break;
                    }
                case SyntaxKind.NotEqualsExpression:
                    {
                        // Count() != 0 >>> Any()
                        newNode = ChangeInvokedMethodName(invocationExpression, "Any");
                        break;
                    }
                case SyntaxKind.LessThanExpression:
                case SyntaxKind.LessThanOrEqualExpression:
                    {
                        if (invocationExpression == left)
                        {
                            // Count() < 1 >>> !Any()
                            // Count() <= 0 >>> !Any()
                            newNode = ChangeInvokedMethodName(invocationExpression, "Any");
                            newNode = LogicalNotExpression(newNode.Parenthesize());
                        }
                        else
                        {
                            // 0 < Count() >>> Any()
                            // 1 <= Count() >>> Any()
                            newNode = ChangeInvokedMethodName(invocationExpression, "Any");
                        }

                        break;
                    }
                case SyntaxKind.GreaterThanExpression:
                case SyntaxKind.GreaterThanOrEqualExpression:
                    {
                        if (invocationExpression == left)
                        {
                            // Count() > 0 >>> Any()
                            // Count() >= 1 >>> Any()
                            newNode = ChangeInvokedMethodName(invocationExpression, "Any");
                        }
                        else
                        {
                            // 1 > Count() >>> !Any()
                            // 0 >= Count() >>> !Any()
                            newNode = ChangeInvokedMethodName(invocationExpression, "Any");
                            newNode = LogicalNotExpression(newNode.Parenthesize());
                        }

                        break;
                    }
            }

            newNode = newNode
                .WithTriviaFrom(binaryExpression)
                .WithFormatterAnnotation();

            return document.ReplaceNodeAsync(binaryExpression, newNode, cancellationToken);
        }

        private static Task<Document> CallFirstOrDefaultInsteadOfConditionalExpressionAsync(
            Document document,
            ConditionalExpressionSyntax conditionalExpression,
            CancellationToken cancellationToken)
        {
            ExpressionSyntax expression = conditionalExpression.Condition.WalkDownParentheses();

            if (expression.IsKind(SyntaxKind.LogicalNotExpression))
            {
                var logicalNot = (PrefixUnaryExpressionSyntax)expression;

                expression = logicalNot.Operand.WalkDownParentheses();
            }

            SimpleMemberInvocationExpressionInfo invocationInfo = SimpleMemberInvocationExpressionInfo(expression);

            InvocationExpressionSyntax invocationExpression = invocationInfo.InvocationExpression;

            InvocationExpressionSyntax newInvocationExpression = ChangeInvokedMethodName(invocationExpression, "FirstOrDefault")
                .WithTriviaFrom(conditionalExpression);

            return document.ReplaceNodeAsync(conditionalExpression, newInvocationExpression, cancellationToken);
        }

        private static Task<Document> CallOrderByDescendingInsteadOfOrderByAndReverseAsync(
            Document document,
            in SimpleMemberInvocationExpressionInfo invocationInfo,
            CancellationToken cancellationToken)
        {
            InvocationExpressionSyntax invocationExpression2 = SimpleMemberInvocationExpressionInfo(invocationInfo.Expression).InvocationExpression;

            InvocationExpressionSyntax newInvocationExpression = ChangeInvokedMethodName(invocationExpression2, "OrderByDescending");

            return document.ReplaceNodeAsync(invocationInfo.InvocationExpression, newInvocationExpression, cancellationToken);
        }

        private static Task<Document> CallOrderByAndWhereInReverseOrderAsync(
            Document document,
            in SimpleMemberInvocationExpressionInfo invocationInfo,
            in SimpleMemberInvocationExpressionInfo invocationInfo2,
            CancellationToken cancellationToken)
        {
            TextSpan span1 = TextSpan.FromBounds(invocationInfo2.OperatorToken.SpanStart, invocationInfo2.ArgumentList.Span.End);

            TextSpan span2 = TextSpan.FromBounds(invocationInfo.OperatorToken.SpanStart, invocationInfo.ArgumentList.Span.End);

            var textChange1 = new TextChange(span1, invocationInfo.InvocationExpression.ToString(span2));

            var textChange2 = new TextChange(span2, invocationInfo.InvocationExpression.ToString(span1));

            return document.WithTextChangesAsync(new TextChange[] { textChange1, textChange2 }, cancellationToken);
        }

        private static Task<Document> CallConvertAllInsteadOfSelectAsync(
            Document document,
            in SimpleMemberInvocationExpressionInfo invocationInfo,
            CancellationToken cancellationToken)
        {
            InvocationExpressionSyntax invocationExpression2 = SimpleMemberInvocationExpressionInfo(invocationInfo.Expression).InvocationExpression;

            InvocationExpressionSyntax newInvocationExpression = ChangeInvokedMethodName(invocationExpression2, "ConvertAll");

            return document.ReplaceNodeAsync(invocationInfo.InvocationExpression, newInvocationExpression, cancellationToken);
        }

        private static Task<Document> CallSumInsteadOfSelectManyAndCountAsync(
            Document document,
            in SimpleMemberInvocationExpressionInfo invocationInfo,
            string propertyName,
            CancellationToken cancellationToken)
        {
            SimpleMemberInvocationExpressionInfo invocationInfo2 = SimpleMemberInvocationExpressionInfo(invocationInfo.Expression);

            ArgumentSyntax argument = invocationInfo2.Arguments.Single();

            var lambdaExpression = (LambdaExpressionSyntax)argument.Expression;

            ExpressionSyntax expression;
            if (lambdaExpression.Body is BlockSyntax block)
            {
                var returnStatement = (ReturnStatementSyntax)block.Statements.Single();

                expression = returnStatement.Expression;
            }
            else
            {
                expression = (ExpressionSyntax)lambdaExpression.Body;
            }

            MemberAccessExpressionSyntax memberAccessExpression = SimpleMemberAccessExpression(
                expression.WithoutTrivia(),
                IdentifierName(propertyName))
                .WithTriviaFrom(expression);

            LambdaExpressionSyntax newLambdaExpression = lambdaExpression.ReplaceNode(expression, memberAccessExpression);

            ArgumentSyntax newArgument = argument.WithExpression(newLambdaExpression);

            InvocationExpressionSyntax newInvocationExpression = invocationInfo2.InvocationExpression.ReplaceNode(argument, newArgument);

            newInvocationExpression = ChangeInvokedMethodName(newInvocationExpression, "Sum");

            return document.ReplaceNodeAsync(invocationInfo.InvocationExpression, newInvocationExpression, cancellationToken);
        }
    }
}
