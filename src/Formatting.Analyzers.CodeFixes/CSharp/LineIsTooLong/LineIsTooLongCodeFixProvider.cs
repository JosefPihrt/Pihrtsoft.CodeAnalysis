// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp;
using Roslynator.CSharp.Syntax;
using Roslynator.Formatting.CodeFixes.CSharp;
using Roslynator.Formatting.CSharp;
using static Roslynator.Formatting.CodeFixes.CSharp.CodeFixHelpers;

namespace Roslynator.Formatting.CodeFixes.LineIsTooLong
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(LineIsTooLongCodeFixProvider))]
    [Shared]
    public sealed class LineIsTooLongCodeFixProvider : BaseCodeFixProvider
    {
        private const string Title = "Wrap line";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(DiagnosticIdentifiers.LineIsTooLong);

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];

            SyntaxTree syntaxTree = await document.GetSyntaxTreeAsync(context.CancellationToken).ConfigureAwait(false);

            int maxLength = AnalyzerOptions.MaxLineLength.GetInt32Value(syntaxTree, document.Project.AnalyzerOptions, AnalyzerSettings.Current.MaxLineLength);

            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

            var wrapLineNodeFinder = new WrapLineNodeFinder(document, semanticModel, context.Span, maxLength);

            SyntaxNode nodeToFix = wrapLineNodeFinder.FindNodeToFix(root);

            if (nodeToFix == null)
                return;

            CodeAction codeAction = CodeAction.Create(
                Title,
                GetCreateChangedDocument(document, nodeToFix),
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
            return;
        }

        private static Func<CancellationToken, Task<Document>> GetCreateChangedDocument(Document document, SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.ArrowExpressionClause:
                    return ct => AddNewLineBeforeOrAfterArrowAsync(document, (ArrowExpressionClauseSyntax)node, ct);
                case SyntaxKind.EqualsValueClause:
                    return ct => AddNewLineBeforeOrAfterEqualsSignAsync(
                        document,
                        ((EqualsValueClauseSyntax)node).EqualsToken,
                        ct);
                case SyntaxKind.AttributeList:
                    return ct => FixListAsync(document, (AttributeListSyntax)node, ListFixMode.Wrap, ct);
                case SyntaxKind.BaseList:
                    return ct => WrapBaseListAsync(document, (BaseListSyntax)node, ct);
                case SyntaxKind.ForStatement:
                    return ct => WrapForStatementAsync(document, (ForStatementSyntax)node, ct);
                case SyntaxKind.ParameterList:
                    return ct => FixListAsync(document, (ParameterListSyntax)node, ListFixMode.Wrap, ct);
                case SyntaxKind.BracketedParameterList:
                    return ct => FixListAsync(document, (BracketedParameterListSyntax)node, ListFixMode.Wrap, ct);
                case SyntaxKind.SimpleMemberAccessExpression:
                    return ct =>
                    {
                        var memberAccess = (MemberAccessExpressionSyntax)node;
                        ExpressionSyntax topExpression = CSharpUtility.GetTopmostExpressionInCallChain(memberAccess);

                        return FixCallChainAsync(
                            document,
                            topExpression,
                            TextSpan.FromBounds(memberAccess.OperatorToken.SpanStart, topExpression.Span.End),
                            ct);
                    };
                case SyntaxKind.MemberBindingExpression:
                    return ct =>
                    {
                        var memberBinding = (MemberBindingExpressionSyntax)node;
                        ExpressionSyntax topExpression = CSharpUtility.GetTopmostExpressionInCallChain(memberBinding);

                        return FixCallChainAsync(
                            document,
                            topExpression,
                            TextSpan.FromBounds(memberBinding.OperatorToken.SpanStart, topExpression.Span.End),
                            ct);
                    };
                case SyntaxKind.ArgumentList:
                    return ct => FixListAsync(document, (ArgumentListSyntax)node, ListFixMode.Wrap, ct);
                case SyntaxKind.BracketedArgumentList:
                    return ct => FixListAsync(document, (BracketedArgumentListSyntax)node, ListFixMode.Wrap, ct);
                case SyntaxKind.AttributeArgumentList:
                    return ct => FixListAsync(document, (AttributeArgumentListSyntax)node, ListFixMode.Wrap, ct);
                case SyntaxKind.ConditionalExpression:
                    return ct => AddNewLineBeforeOrAfterConditionalOperatorAsync(
                        document,
                        (ConditionalExpressionSyntax)node,
                        ct);
                case SyntaxKind.ArrayInitializerExpression:
                case SyntaxKind.CollectionInitializerExpression:
                case SyntaxKind.ComplexElementInitializerExpression:
                case SyntaxKind.ObjectInitializerExpression:
                    return ct => FixListAsync(document, (InitializerExpressionSyntax)node, ListFixMode.Wrap, ct);
                case SyntaxKind.AddExpression:
                case SyntaxKind.SubtractExpression:
                case SyntaxKind.MultiplyExpression:
                case SyntaxKind.DivideExpression:
                case SyntaxKind.ModuloExpression:
                case SyntaxKind.LeftShiftExpression:
                case SyntaxKind.RightShiftExpression:
                case SyntaxKind.LogicalOrExpression:
                case SyntaxKind.LogicalAndExpression:
                case SyntaxKind.BitwiseOrExpression:
                case SyntaxKind.BitwiseAndExpression:
                case SyntaxKind.ExclusiveOrExpression:
                case SyntaxKind.CoalesceExpression:
                case SyntaxKind.IsExpression:
                case SyntaxKind.AsExpression:
                case SyntaxKind.EqualsExpression:
                case SyntaxKind.NotEqualsExpression:
                case SyntaxKind.LessThanExpression:
                case SyntaxKind.LessThanOrEqualExpression:
                case SyntaxKind.GreaterThanExpression:
                case SyntaxKind.GreaterThanOrEqualExpression:
                    return ct =>
                    {
                        var binaryExpression = (BinaryExpressionSyntax)node;
                        var binaryExpression2 = (BinaryExpressionSyntax)binaryExpression
                            .WalkUp(f => f.IsKind(binaryExpression.Kind()));

                        return FixBinaryExpressionAsync(
                            document,
                            binaryExpression2,
                            TextSpan.FromBounds(
                                binaryExpression.OperatorToken.SpanStart,
                                binaryExpression2.OperatorToken.Span.End),
                            ct);
                    };
                case SyntaxKind.AddAssignmentExpression:
                case SyntaxKind.AndAssignmentExpression:
                case SyntaxKind.CoalesceAssignmentExpression:
                case SyntaxKind.DivideAssignmentExpression:
                case SyntaxKind.ExclusiveOrAssignmentExpression:
                case SyntaxKind.LeftShiftAssignmentExpression:
                case SyntaxKind.ModuloAssignmentExpression:
                case SyntaxKind.MultiplyAssignmentExpression:
                case SyntaxKind.OrAssignmentExpression:
                case SyntaxKind.RightShiftAssignmentExpression:
                case SyntaxKind.SimpleAssignmentExpression:
                case SyntaxKind.SubtractAssignmentExpression:
                    return ct => AddNewLineBeforeOrAfterEqualsSignAsync(
                        document,
                        ((AssignmentExpressionSyntax)node).OperatorToken,
                        ct);
                default:
                    throw new InvalidOperationException();
            }
        }

        private static Task<Document> AddNewLineBeforeOrAfterArrowAsync(
            Document document,
            ArrowExpressionClauseSyntax arrowExpressionClause,
            CancellationToken cancellationToken = default)
        {
            return AddNewLineBeforeOrAfterAsync(
                document,
                arrowExpressionClause.ArrowToken,
                AnalyzerOptions.AddNewLineAfterExpressionBodyArrowInsteadOfBeforeIt.IsEnabled(document, arrowExpressionClause),
                cancellationToken);
        }

        private static Task<Document> AddNewLineBeforeOrAfterEqualsSignAsync(
            Document document,
            SyntaxToken operatorToken,
            CancellationToken cancellationToken = default)
        {
            return AddNewLineBeforeOrAfterAsync(
                document,
                operatorToken,
                AnalyzerOptions.AddNewLineAfterEqualsSignInsteadOfBeforeIt.IsEnabled(document, operatorToken),
                cancellationToken);
        }

        private static Task<Document> AddNewLineBeforeOrAfterConditionalOperatorAsync(
            Document document,
            ConditionalExpressionSyntax conditionalExpression,
            CancellationToken cancellationToken = default)
        {
            string indentation = SyntaxTriviaAnalysis.GetIncreasedIndentation(conditionalExpression, cancellationToken);

            if (AnalyzerOptions.AddNewLineAfterEqualsSignInsteadOfBeforeIt.IsEnabled(document, conditionalExpression))
            {
                return document.WithTextChangesAsync(
                    new TextChange[]
                    {
                        GetNewLineAfterTextChange(conditionalExpression.QuestionToken, indentation),
                        GetNewLineAfterTextChange(conditionalExpression.ColonToken, indentation),
                    },
                    cancellationToken);
            }
            else
            {
                return document.WithTextChangesAsync(
                    new TextChange[]
                    {
                        GetNewLineBeforeTextChange(conditionalExpression.QuestionToken, indentation),
                        GetNewLineBeforeTextChange(conditionalExpression.ColonToken, indentation),
                    },
                    cancellationToken);
            }
        }

        private static Task<Document> AddNewLineBeforeOrAfterAsync(
            Document document,
            SyntaxToken token,
            bool addNewLineAfter,
            CancellationToken cancellationToken = default)
        {
            string indentation = SyntaxTriviaAnalysis.GetIncreasedIndentation(token.Parent, cancellationToken);

            return (addNewLineAfter)
                ? AddNewLineAfterAsync(document, token, indentation, cancellationToken)
                : AddNewLineBeforeAsync(document, token, indentation, cancellationToken);
        }

        private static Task<Document> WrapForStatementAsync(
            Document document,
            ForStatementSyntax forStatement,
            CancellationToken cancellationToken = default)
        {
            string indentation = SyntaxTriviaAnalysis.GetIncreasedIndentation(forStatement, cancellationToken);

            return document.WithTextChangesAsync(
                new TextChange[]
                {
                    GetNewLineAfterTextChange(forStatement.OpenParenToken, indentation),
                    GetNewLineAfterTextChange(forStatement.FirstSemicolonToken, indentation),
                    GetNewLineAfterTextChange(forStatement.SecondSemicolonToken, indentation),
                },
                cancellationToken);
        }

        private static Task<Document> WrapBaseListAsync(
            Document document,
            BaseListSyntax baseList,
            CancellationToken cancellationToken = default)
        {
            List<TextChange> textChanges = GetFixListChanges(
                baseList,
                baseList.ColonToken,
                baseList.Types,
                ListFixMode.Wrap,
                cancellationToken);

            if (baseList.Types.Count > 1)
            {
                GenericInfo genericInfo = SyntaxInfo.GenericInfo(baseList.Parent);

                SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses = genericInfo.ConstraintClauses;

                if (constraintClauses.Any())
                {
                    List<TextChange> textChanges2 = GetFixListChanges(
                        genericInfo.Node,
                        constraintClauses.First().WhereKeyword.GetPreviousToken(),
                        constraintClauses,
                        ListFixMode.Wrap,
                        cancellationToken);

                    textChanges.AddRange(textChanges2);
                }
            }

            return document.WithTextChangesAsync(textChanges, cancellationToken);
        }
    }
}

