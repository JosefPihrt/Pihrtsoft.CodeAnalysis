// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TypeDoesNotContainDefinitionCodeFixProvider))]
    [Shared]
    public sealed class TypeDoesNotContainDefinitionCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CompilerDiagnosticIdentifiers.TypeDoesNotContainDefinitionAndNoExtensionMethodCouldBeFound); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out ExpressionSyntax expression))
                return;

            Diagnostic diagnostic = context.Diagnostics[0];
            Document document = context.Document;

            switch (expression)
            {
                case SimpleNameSyntax simpleName:
                    {
                        Debug.Assert(expression.IsKind(SyntaxKind.IdentifierName, SyntaxKind.GenericName), expression.Kind().ToString());

                        if (simpleName.IsParentKind(SyntaxKind.SimpleMemberAccessExpression))
                        {
                            var memberAccessExpression = (MemberAccessExpressionSyntax)simpleName.Parent;

                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            if (memberAccessExpression.IsParentKind(SyntaxKind.InvocationExpression))
                            {
                                if (!memberAccessExpression.Parent.IsParentKind(SyntaxKind.ConditionalAccessExpression)
                                    && Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.ReplaceInvocationWithMemberAccessOrViceVersa))
                                {
                                    var invocationExpression = (InvocationExpressionSyntax)memberAccessExpression.Parent;

                                    if (!invocationExpression.ArgumentList.Arguments.Any())
                                    {
                                        ReplaceInvocationWithMemberAccess(context, diagnostic, memberAccessExpression, invocationExpression, semanticModel);
                                    }
                                }
                            }
                            else
                            {
                                if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.FixMemberAccessName))
                                {
                                    CodeFixRegistrationResult result = ReplaceCountWithLengthOrViceVersa(context, diagnostic, memberAccessExpression.Expression, simpleName, semanticModel);

                                    if (result.Success)
                                        break;
                                }

                                if (!memberAccessExpression.IsParentKind(SyntaxKind.ConditionalAccessExpression)
                                    && Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.ReplaceInvocationWithMemberAccessOrViceVersa))
                                {
                                    ReplaceMemberAccessWithInvocation(context, diagnostic, memberAccessExpression, semanticModel);
                                }
                            }
                        }

                        break;
                    }
                case MemberBindingExpressionSyntax memberBindingExpression:
                    {
                        if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.FixMemberAccessName))
                            break;

                        if (!(memberBindingExpression.Parent is ConditionalAccessExpressionSyntax conditionalAccessExpression))
                            break;

                        SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                        CodeFixRegistrationResult result = ReplaceCountWithLengthOrViceVersa(context, diagnostic, conditionalAccessExpression.Expression, memberBindingExpression.Name, semanticModel);

                        break;
                    }
                case AwaitExpressionSyntax awaitExpression:
                    {
                        if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveAwaitKeyword))
                            break;

                        CodeAction codeAction = CodeAction.Create(
                            "Remove 'await'",
                            cancellationToken =>
                            {
                                ExpressionSyntax expression2 = awaitExpression.Expression;

                                SyntaxTriviaList leadingTrivia = awaitExpression
                                    .GetLeadingTrivia()
                                    .AddRange(awaitExpression.AwaitKeyword.TrailingTrivia.EmptyIfWhitespace())
                                    .AddRange(expression2.GetLeadingTrivia().EmptyIfWhitespace());

                                ExpressionSyntax newNode = expression2.WithLeadingTrivia(leadingTrivia);

                                return document.ReplaceNodeAsync(awaitExpression, newNode, cancellationToken);
                            },
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
            }
        }

        private CodeFixRegistrationResult ReplaceCountWithLengthOrViceVersa(
            CodeFixContext context,
            Diagnostic diagnostic,
            ExpressionSyntax expression,
            SimpleNameSyntax simpleName,
            SemanticModel semanticModel)
        {
            string name = simpleName.Identifier.ValueText;

            string newName = GetNewName();

            if (newName != null)
            {
                ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(expression, context.CancellationToken);

                if (typeSymbol != null)
                {
                    if (typeSymbol is IArrayTypeSymbol arrayType)
                        typeSymbol = arrayType.ElementType;

                    foreach (ISymbol symbol in typeSymbol.GetMembers(newName))
                    {
                        if (!symbol.IsStatic
                            && symbol.Kind == SymbolKind.Property)
                        {
                            var propertySymbol = (IPropertySymbol)symbol;

                            if (!propertySymbol.IsIndexer
                                && propertySymbol.IsReadOnly
                                && semanticModel.IsAccessible(expression.SpanStart, symbol))
                            {
                                Document document = context.Document;

                                CodeAction codeAction = CodeAction.Create(
                                    $"Use '{newName}' instead of '{name}'",
                                    ct =>
                                    {
                                        SimpleNameSyntax newNode = simpleName.WithIdentifier(Identifier(newName).WithTriviaFrom(simpleName.Identifier));

                                        return document.ReplaceNodeAsync(simpleName, newNode, ct);
                                    },
                                    GetEquivalenceKey(diagnostic));

                                context.RegisterCodeFix(codeAction, diagnostic);
                                return new CodeFixRegistrationResult(true);
                            }
                        }
                    }
                }
            }

            return default;

            string GetNewName()
            {
                switch (name)
                {
                    case "Count":
                        return "Length";
                    case "Length":
                        return "Count";
                    default:
                        return null;
                }
            }
        }

        private void ReplaceInvocationWithMemberAccess(
            CodeFixContext context,
            Diagnostic diagnostic,
            MemberAccessExpressionSyntax memberAccess,
            InvocationExpressionSyntax invocationExpression,
            SemanticModel semanticModel)
        {
            SimpleNameSyntax simpleName = memberAccess.Name;
            SyntaxToken identifier = simpleName.Identifier;
            string name = identifier.ValueText;

            if (name.Length > 3
                && name.StartsWith("Get", StringComparison.Ordinal)
                && char.IsUpper(name, 3))
            {
                SyntaxToken newIdentifier = Identifier(
                    identifier.LeadingTrivia,
                    name.Substring(3),
                    invocationExpression.ArgumentList.CloseParenToken.TrailingTrivia);

                SimpleNameSyntax newSimpleName = simpleName.WithIdentifier(newIdentifier);
                MemberAccessExpressionSyntax newMemberAccessExpression = memberAccess.WithName(newSimpleName);

                if (semanticModel
                    .GetSpeculativeSymbolInfo(invocationExpression.SpanStart, newMemberAccessExpression, SpeculativeBindingOption.BindAsExpression)
                    .Symbol
                    .IsKind(SymbolKind.Property))
                {
                    Document document = context.Document;

                    CodeAction codeAction = CodeAction.Create(
                        $"Use '{newIdentifier.ValueText}' instead of '{identifier.ValueText}'",
                        ct => document.ReplaceNodeAsync(invocationExpression, newMemberAccessExpression, ct),
                        GetEquivalenceKey(diagnostic));

                    context.RegisterCodeFix(codeAction, diagnostic);
                }
            }
        }

        private void ReplaceMemberAccessWithInvocation(
            CodeFixContext context,
            Diagnostic diagnostic,
            MemberAccessExpressionSyntax memberAccessExpression,
            SemanticModel semanticModel)
        {
            SimpleNameSyntax simpleName = memberAccessExpression.Name;
            SyntaxToken identifier = simpleName.Identifier;
            string name = identifier.ValueText;

            SyntaxToken newIdentifier = Identifier(
                identifier.LeadingTrivia,
                "Get" + name,
                SyntaxTriviaList.Empty);

            SimpleNameSyntax newSimpleName = simpleName.WithIdentifier(newIdentifier);
            MemberAccessExpressionSyntax newMemberAccessExpression = memberAccessExpression.WithName(newSimpleName);
            InvocationExpressionSyntax invocationExpression = InvocationExpression(newMemberAccessExpression, ArgumentList());

            if (semanticModel
                .GetSpeculativeSymbolInfo(memberAccessExpression.SpanStart, invocationExpression, SpeculativeBindingOption.BindAsExpression)
                .Symbol
                .IsKind(SymbolKind.Method))
            {
                Document document = context.Document;

                CodeAction codeAction = CodeAction.Create(
                    $"Use '{newIdentifier.ValueText}' instead of '{identifier.ValueText}'",
                    ct => document.ReplaceNodeAsync(memberAccessExpression, invocationExpression, ct),
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
        }
    }
}
