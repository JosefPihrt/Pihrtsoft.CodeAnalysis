// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CodeFixes;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ObjectCreationExpressionCodeFixProvider))]
    [Shared]
    public sealed class ObjectCreationExpressionCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CompilerDiagnosticIdentifiers.ThereIsNoArgumentGivenThatCorrespondsToRequiredFormalParameter); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics[0];

            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.MoveInitializerExpressionsToConstructor))
                return;

            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            ObjectCreationExpressionSyntax objectCreationExpression = root
                .FindNode(context.Span, getInnermostNodeForTie: true)?
                .FirstAncestorOrSelf<ObjectCreationExpressionSyntax>();

            if (objectCreationExpression == null)
                return;

            switch (diagnostic.Id)
            {
                case CompilerDiagnosticIdentifiers.ThereIsNoArgumentGivenThatCorrespondsToRequiredFormalParameter:
                    {
                        if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.MoveInitializerExpressionsToConstructor))
                            break;

                        if (!objectCreationExpression.Type.Span.Contains(diagnostic.Location.SourceSpan))
                            return;

                        ArgumentListSyntax argumentList = objectCreationExpression.ArgumentList;

                        if (argumentList?.Arguments.Any() == true)
                            return;

                        InitializerExpressionSyntax initializer = objectCreationExpression.Initializer;

                        if (initializer == null)
                            return;

                        SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                        List<ExpressionSyntax> expressions = null;

                        foreach (ExpressionSyntax expression in initializer.Expressions)
                        {
                            if (expression is AssignmentExpressionSyntax assignment
                                && semanticModel.GetDiagnostic(
                                    CompilerDiagnosticIdentifiers.PropertyOrIndexerCannotBeUsedInThisContextBecauseSetAccessorIsAccessible,
                                    assignment.Left.Span,
                                    context.CancellationToken) != null)
                            {
                                (expressions ??= new List<ExpressionSyntax>()).Add(expression);
                            }
                        }

                        if (expressions == null)
                            return;

                        TypeSyntax type = objectCreationExpression.Type;

                        if (argumentList == null)
                        {
                            argumentList = ArgumentList().WithTrailingTrivia(type.GetTrailingTrivia());
                            type = type.WithoutTrailingTrivia();
                        }

                        SeparatedSyntaxList<ArgumentSyntax> arguments = expressions
                            .Select(f => Argument(((AssignmentExpressionSyntax)f).Right))
                            .ToSeparatedSyntaxList();

                        argumentList = argumentList.WithArguments(arguments);

                        ObjectCreationExpressionSyntax newObjectCreationExpression = objectCreationExpression.Update(
                            objectCreationExpression.NewKeyword,
                            type,
                            argumentList,
                            initializer);

                        SymbolInfo symbolInfo = semanticModel.GetSpeculativeSymbolInfo(
                            objectCreationExpression.SpanStart,
                            newObjectCreationExpression,
                            SpeculativeBindingOption.BindAsExpression);

                        if (symbolInfo.Symbol is IMethodSymbol methodSymbol
                            && methodSymbol.MethodKind == MethodKind.Constructor)
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Move initializer expressions to constructor",
                                ct =>
                                {
                                    InitializerExpressionSyntax newInitializer = initializer.RemoveNodes(expressions, SyntaxRefactorings.DefaultRemoveOptions);

                                    if (newInitializer.Expressions.Count == 0
                                        && newInitializer
                                            .DescendantTrivia(TextSpan.FromBounds(newInitializer.OpenBraceToken.SpanStart, newInitializer.CloseBraceToken.SpanStart))
                                            .All(f => f.IsWhitespaceOrEndOfLineTrivia()))
                                    {
                                        newInitializer = null;

                                        ArgumentListSyntax newArgumentList = newObjectCreationExpression
                                            .ArgumentList
                                            .TrimTrailingTrivia()
                                            .AppendToTrailingTrivia(initializer.GetTrailingTrivia());

                                        newObjectCreationExpression = newObjectCreationExpression
                                            .WithArgumentList(newArgumentList);
                                    }

                                    newObjectCreationExpression = newObjectCreationExpression
                                        .WithInitializer(newInitializer)
                                        .WithFormatterAnnotation();

                                    return context.Document.ReplaceNodeAsync(objectCreationExpression, newObjectCreationExpression, ct);
                                },
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                        }

                        break;
                    }
            }
        }
    }
}
