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
using Roslynator.CSharp;
using Roslynator.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;
using static Roslynator.CSharp.CSharpTypeFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseStringComparisonCodeFixProvider))]
    [Shared]
    public sealed class UseStringComparisonCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.UseStringComparison); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(
                root,
                context.Span,
                out SyntaxNode node,
                predicate: f => f.IsKind(
                    SyntaxKind.EqualsExpression,
                    SyntaxKind.NotEqualsExpression,
                    SyntaxKind.InvocationExpression)))
            {
                return;
            }

            Diagnostic diagnostic = context.Diagnostics[0];

            switch (node.Kind())
            {
                case SyntaxKind.EqualsExpression:
                case SyntaxKind.NotEqualsExpression:
                    {
                        var binaryExpression = (BinaryExpressionSyntax)node;

                        SimpleMemberInvocationExpressionInfo invocationInfo = SyntaxInfo.SimpleMemberInvocationExpressionInfo(binaryExpression.Left.WalkDownParentheses());

                        if (!invocationInfo.Success)
                            invocationInfo = SyntaxInfo.SimpleMemberInvocationExpressionInfo((InvocationExpressionSyntax)binaryExpression.Right.WalkDownParentheses());

                        SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                        INamedTypeSymbol comparisonSymbol = semanticModel.GetTypeByMetadataName("System.StringComparison");

                        if (!invocationInfo.NameText.EndsWith("Invariant", StringComparison.Ordinal)
                            || !RegisterCodeFix(context, diagnostic, binaryExpression, comparisonSymbol, "InvariantCultureIgnoreCase"))
                        {
                            RegisterCodeFix(context, diagnostic, binaryExpression, comparisonSymbol, "OrdinalIgnoreCase");
                            RegisterCodeFix(context, diagnostic, binaryExpression, comparisonSymbol, "CurrentCultureIgnoreCase");
                        }

                        break;
                    }
                case SyntaxKind.InvocationExpression:
                    {
                        var invocationExpression = (InvocationExpressionSyntax)node;

                        SimpleMemberInvocationExpressionInfo invocationInfo = SyntaxInfo.SimpleMemberInvocationExpressionInfo(invocationExpression);

                        SeparatedSyntaxList<ArgumentSyntax> arguments = invocationInfo.Arguments;

                        InvocationExpressionSyntax invocationExpression2;

                        if (arguments.Count == 1)
                        {
                            invocationExpression2 = (InvocationExpressionSyntax)invocationInfo.Expression;
                        }
                        else
                        {
                            invocationExpression2 = (arguments[0].Expression.WalkDownParentheses() as InvocationExpressionSyntax)
                                ?? (InvocationExpressionSyntax)arguments[1].Expression.WalkDownParentheses();
                        }

                        SimpleMemberInvocationExpressionInfo invocationInfo2 = SyntaxInfo.SimpleMemberInvocationExpressionInfo(invocationExpression2);

                        SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                        INamedTypeSymbol comparisonSymbol = semanticModel.GetTypeByMetadataName("System.StringComparison");

                        if (!invocationInfo2.NameText.EndsWith("Invariant", StringComparison.Ordinal)
                            || !RegisterCodeFix(context, diagnostic, invocationInfo, comparisonSymbol, "InvariantCultureIgnoreCase", semanticModel))
                        {
                            RegisterCodeFix(context, diagnostic, invocationInfo, comparisonSymbol, "OrdinalIgnoreCase", semanticModel);
                            RegisterCodeFix(context, diagnostic, invocationInfo, comparisonSymbol, "CurrentCultureIgnoreCase", semanticModel);
                        }

                        break;
                    }
            }
        }

        private bool RegisterCodeFix(
            CodeFixContext context,
            Diagnostic diagnostic,
            BinaryExpressionSyntax binaryExpression,
            INamedTypeSymbol comparisonSymbol,
            string comparisonName)
        {
            if (!comparisonSymbol.ContainsMember<IFieldSymbol>(comparisonName))
                return false;

            CodeAction codeAction = CodeAction.Create(
                GetTitle(comparisonName),
                cancellationToken => RefactorAsync(context.Document, binaryExpression, comparisonName, cancellationToken),
                GetEquivalenceKey(diagnostic, (comparisonName != "InvariantCultureIgnoreCase") ? comparisonName : null));

            context.RegisterCodeFix(codeAction, diagnostic);
            return true;
        }

        private static Task<Document> RefactorAsync(
            Document document,
            BinaryExpressionSyntax binaryExpression,
            string comparisonName,
            CancellationToken cancellationToken)
        {
            ExpressionSyntax left = binaryExpression.Left.WalkDownParentheses();
            ExpressionSyntax right = binaryExpression.Right.WalkDownParentheses();

            ExpressionSyntax newNode = SimpleMemberInvocationExpression(
                StringType(),
                IdentifierName("Equals"),
                ArgumentList(
                    CreateArgument(left),
                    CreateArgument(right),
                    Argument(CreateStringComparison(comparisonName))));

            if (binaryExpression.IsKind(SyntaxKind.NotEqualsExpression))
                newNode = LogicalNotExpression(newNode);

            newNode = newNode
                .WithTriviaFrom(binaryExpression)
                .WithFormatterAnnotation();

            return document.ReplaceNodeAsync(binaryExpression, newNode, cancellationToken);
        }

        private bool RegisterCodeFix(
            CodeFixContext context,
            Diagnostic diagnostic,
            SimpleMemberInvocationExpressionInfo invocationInfo,
            INamedTypeSymbol comparisonSymbol,
            string comparisonName,
            SemanticModel semanticModel)
        {
            if (!comparisonSymbol.ContainsMember<IFieldSymbol>(comparisonName))
                return false;

            CodeAction codeAction = CodeAction.Create(
                GetTitle(comparisonName),
                cancellationToken => RefactorAsync(context.Document, invocationInfo, comparisonName, semanticModel, cancellationToken),
                GetEquivalenceKey(diagnostic, (comparisonName != "InvariantCultureIgnoreCase") ? comparisonName : null));

            context.RegisterCodeFix(codeAction, diagnostic);
            return true;
        }

        private static string GetTitle(string stringComparison)
        {
            return $"Use 'StringComparison.{stringComparison}'";
        }

        private static Task<Document> RefactorAsync(
            Document document,
            in SimpleMemberInvocationExpressionInfo invocationInfo,
            string comparisonName,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            SeparatedSyntaxList<ArgumentSyntax> arguments = invocationInfo.Arguments;

            InvocationExpressionSyntax invocation = invocationInfo.InvocationExpression;

            if (arguments.Count == 2)
            {
                ArgumentListSyntax newArgumentList = ArgumentList(
                    CreateArgument(arguments[0]),
                    CreateArgument(arguments[1]),
                    Argument(CreateStringComparison(comparisonName)));

                InvocationExpressionSyntax newNode = invocation.WithArgumentList(newArgumentList);

                return document.ReplaceNodeAsync(invocation, newNode, cancellationToken);
            }
            else
            {
                var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
                var invocation2 = (InvocationExpressionSyntax)memberAccess.Expression;
                var memberAccess2 = (MemberAccessExpressionSyntax)invocation2.Expression;

                MemberAccessExpressionSyntax newMemberAccess = memberAccess.WithExpression(memberAccess2.Expression);

                bool useIndexOf = memberAccess.Name.Identifier.ValueText == "Contains"
                    && !ExistsStringContainsWithStringComparison();

                if (useIndexOf)
                    newMemberAccess = newMemberAccess.WithName(newMemberAccess.Name.WithIdentifier(Identifier("IndexOf").WithTriviaFrom(newMemberAccess.Name.Identifier)));

                ArgumentListSyntax newArgumentList = ArgumentList(
                    CreateArgument(arguments[0]),
                    Argument(CreateStringComparison(comparisonName)));

                ExpressionSyntax newNode = invocation
                    .WithExpression(newMemberAccess)
                    .WithArgumentList(newArgumentList);

                if (useIndexOf)
                    newNode = GreaterThanOrEqualExpression(newNode.Parenthesize(), NumericLiteralExpression(0)).Parenthesize();

                return document.ReplaceNodeAsync(invocation, newNode, cancellationToken);
            }

            bool ExistsStringContainsWithStringComparison()
            {
                foreach (ISymbol symbol in semanticModel
                    .Compilation
                    .GetSpecialType(SpecialType.System_String)
                    .GetMembers("Contains"))
                {
                    if (!symbol.IsStatic
                        && symbol.DeclaredAccessibility == Accessibility.Public
                        && symbol.Kind == SymbolKind.Method)
                    {
                        var methodSymbol = (IMethodSymbol)symbol;

                        ImmutableArray<IParameterSymbol> parameters = methodSymbol.Parameters;

                        if (parameters.Length == 2
                            && parameters[0].Type.SpecialType == SpecialType.System_String
                            && parameters[1].Type.HasMetadataName(MetadataNames.System_StringComparison))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        private static NameSyntax CreateStringComparison(string comparisonName)
        {
            return ParseName($"global::System.StringComparison.{comparisonName}").WithSimplifierAnnotation();
        }

        private static ArgumentSyntax CreateArgument(ExpressionSyntax expression)
        {
            switch (expression.Kind())
            {
                case SyntaxKind.StringLiteralExpression:
                    {
                        return Argument(expression);
                    }
                case SyntaxKind.InvocationExpression:
                    {
                        var invocation = (InvocationExpressionSyntax)expression;

                        var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

                        return Argument(memberAccess.Expression).WithTriviaFrom(expression);
                    }
                default:
                    {
                        Debug.Fail(expression.Kind().ToString());
                        return Argument(expression);
                    }
            }
        }

        private static ArgumentSyntax CreateArgument(ArgumentSyntax argument)
        {
            ExpressionSyntax expression = argument.Expression.WalkDownParentheses();

            switch (expression.Kind())
            {
                case SyntaxKind.StringLiteralExpression:
                    {
                        return argument;
                    }
                case SyntaxKind.InvocationExpression:
                    {
                        var invocation = (InvocationExpressionSyntax)expression;

                        var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

                        return Argument(memberAccess.Expression).WithTriviaFrom(argument);
                    }
                default:
                    {
                        Debug.Fail(expression.Kind().ToString());
                        return argument;
                    }
            }
        }
    }
}