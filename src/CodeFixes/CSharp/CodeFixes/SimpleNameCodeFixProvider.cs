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
using Roslynator.CSharp.Refactorings;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SimpleNameCodeFixProvider))]
    [Shared]
    public sealed class SimpleNameCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    CompilerDiagnosticIdentifiers.CannotConvertMethodGroupToNonDelegateType,
                    CompilerDiagnosticIdentifiers.TypeOrNamespaceNameCouldNotBeFound,
                    CompilerDiagnosticIdentifiers.NameIsNotValidInGivenContext);
            }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out SimpleNameSyntax simpleName))
                return;

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];
            string diagnosticId = diagnostic.Id;

            if (diagnosticId == CompilerDiagnosticIdentifiers.CannotConvertMethodGroupToNonDelegateType
                || diagnosticId == CompilerDiagnosticIdentifiers.NameIsNotValidInGivenContext)
            {
                if (!Settings.IsEnabled(diagnosticId, CodeFixIdentifiers.AddArgumentList))
                    return;

                if (!simpleName.IsParentKind(SyntaxKind.SimpleMemberAccessExpression))
                    return;

                var memberAccess = (MemberAccessExpressionSyntax)simpleName.Parent;

                CodeAction codeAction = CodeAction.Create(
                    "Add argument list",
                    cancellationToken =>
                    {
                        InvocationExpressionSyntax invocationExpression = InvocationExpression(
                            memberAccess.WithoutTrailingTrivia(),
                            ArgumentList().WithTrailingTrivia(memberAccess.GetTrailingTrivia()));

                        return document.ReplaceNodeAsync(memberAccess, invocationExpression, cancellationToken);
                    },
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
            else if (diagnosticId == CompilerDiagnosticIdentifiers.TypeOrNamespaceNameCouldNotBeFound)
            {
                if (Settings.IsEnabled(diagnosticId, CodeFixIdentifiers.ChangeArrayType)
                    && (simpleName.Parent is ArrayTypeSyntax arrayType)
                    && (arrayType.Parent is ArrayCreationExpressionSyntax arrayCreation)
                    && object.ReferenceEquals(simpleName, arrayType.ElementType))
                {
                    ExpressionSyntax expression = arrayCreation.Initializer?.Expressions.FirstOrDefault();

                    if (expression != null)
                    {
                        SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                        ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(expression, context.CancellationToken);

                        if (typeSymbol?.SupportsExplicitDeclaration() == true)
                        {
                            TypeSyntax newType = typeSymbol.ToTypeSyntax()
                                .WithSimplifierAnnotation()
                                .WithTriviaFrom(simpleName);

                            CodeAction codeAction = CodeAction.Create(
                                $"Change element type to '{SymbolDisplay.ToMinimalDisplayString(typeSymbol, semanticModel, simpleName.SpanStart, SymbolDisplayFormats.DisplayName)}'",
                                cancellationToken => document.ReplaceNodeAsync(simpleName, newType, cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            return;
                        }
                    }
                }

                if (Settings.IsEnabled(diagnosticId, CodeFixIdentifiers.ChangeMemberTypeAccordingToReturnExpression))
                {
                    ExpressionSyntax expression = GetReturnExpression(simpleName);

                    if (expression != null)
                    {
                        SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                        ChangeMemberTypeRefactoring.ComputeCodeFix(context, diagnostic, expression, semanticModel);
                    }
                }
            }
        }

        private static ExpressionSyntax GetReturnExpression(SyntaxNode node)
        {
            switch (node.Parent)
            {
                case MethodDeclarationSyntax methodDeclaration:
                    {
                        if (object.ReferenceEquals(node, methodDeclaration.ReturnType))
                        {
                            ExpressionSyntax expression = methodDeclaration.ExpressionBody?.Expression;

                            if (expression != null)
                                return expression;

                            StatementSyntax statement = methodDeclaration.Body?.SingleNonBlockStatementOrDefault();

                            return (statement as ReturnStatementSyntax)?.Expression;
                        }

                        break;
                    }
                case LocalFunctionStatementSyntax localFunction:
                    {
                        if (object.ReferenceEquals(node, localFunction.ReturnType))
                        {
                            ExpressionSyntax expression = localFunction.ExpressionBody?.Expression;

                            if (expression != null)
                                return expression;

                            StatementSyntax statement = localFunction.Body?.SingleNonBlockStatementOrDefault();

                            return (statement as ReturnStatementSyntax)?.Expression;
                        }

                        break;
                    }
                case VariableDeclarationSyntax variableDeclaration:
                    {
                        if (object.ReferenceEquals(node, variableDeclaration.Type)
                            && node.Parent.IsParentKind(SyntaxKind.FieldDeclaration))
                        {
                            return variableDeclaration
                                .Variables
                                .SingleOrDefault(shouldThrow: false)?
                                .Initializer?
                                .Value;
                        }

                        break;
                    }
            }

            return null;
        }
    }
}
