// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddReturnStatementCodeFixProvider))]
    [Shared]
    public sealed class AddReturnStatementCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    CompilerDiagnosticIdentifiers.NotAllCodePathsReturnValue,
                    CompilerDiagnosticIdentifiers.NotAllCodePathsReturnValueInAnonymousFunction);
            }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics[0];

            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.AddReturnStatementThatReturnsDefaultValue))
                return;

            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            SyntaxNode node = root.FindNode(context.Span, getInnermostNodeForTie: true);

            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

            foreach (SyntaxNode ancestor in node.AncestorsAndSelf())
            {
                switch (ancestor.Kind())
                {
                    case SyntaxKind.MethodDeclaration:
                        {
                            var methodDeclaration = (MethodDeclarationSyntax)ancestor;

                            if (!methodDeclaration.Modifiers.Contains(SyntaxKind.PartialKeyword))
                                ComputeCodeFix(context, diagnostic, methodDeclaration.ReturnType, methodDeclaration.Body, semanticModel);

                            return;
                        }
                    case SyntaxKind.OperatorDeclaration:
                        {
                            var operatorDeclaration = (OperatorDeclarationSyntax)ancestor;

                            ComputeCodeFix(context, diagnostic, operatorDeclaration.ReturnType, operatorDeclaration.Body, semanticModel);
                            return;
                        }
                    case SyntaxKind.ConversionOperatorDeclaration:
                        {
                            var conversionOperatorDeclaration = (ConversionOperatorDeclarationSyntax)ancestor;

                            ComputeCodeFix(context, diagnostic, conversionOperatorDeclaration.Type, conversionOperatorDeclaration.Body, semanticModel);
                            return;
                        }
                    case SyntaxKind.LocalFunctionStatement:
                        {
                            var localFunction = (LocalFunctionStatementSyntax)ancestor;

                            ComputeCodeFix(context, diagnostic, localFunction.ReturnType, localFunction.Body, semanticModel);
                            return;
                        }
                    case SyntaxKind.GetAccessorDeclaration:
                        {
                            var accessor = (AccessorDeclarationSyntax)ancestor;

                            switch (accessor.Parent.Parent.Kind())
                            {
                                case SyntaxKind.PropertyDeclaration:
                                    {
                                        var propertyDeclaration = (PropertyDeclarationSyntax)accessor.Parent.Parent;

                                        ComputeCodeFix(context, diagnostic, propertyDeclaration.Type, accessor.Body, semanticModel);
                                        break;
                                    }
                                case SyntaxKind.IndexerDeclaration:
                                    {
                                        var indexerDeclaration = (IndexerDeclarationSyntax)accessor.Parent.Parent;

                                        ComputeCodeFix(context, diagnostic, indexerDeclaration.Type, accessor.Body, semanticModel);
                                        break;
                                    }
                            }

                            return;
                        }
                    case SyntaxKind.AnonymousMethodExpression:
                    case SyntaxKind.SimpleLambdaExpression:
                    case SyntaxKind.ParenthesizedLambdaExpression:
                        {
                            var anonymousFunction = (AnonymousFunctionExpressionSyntax)ancestor;

                            var body = anonymousFunction.Body as BlockSyntax;

                            if (body?.Statements.Count > 0)
                            {
                                var methodSymbol = semanticModel.GetSymbol(anonymousFunction, context.CancellationToken) as IMethodSymbol;

                                if (methodSymbol?.IsErrorType() == false)
                                    ComputeCodeFix(context, diagnostic, methodSymbol.ReturnType, body);
                            }

                            return;
                        }
                }
            }
        }

        private void ComputeCodeFix(
            CodeFixContext context,
            Diagnostic diagnostic,
            TypeSyntax type,
            BlockSyntax body,
            SemanticModel semanticModel)
        {
            if (type != null
                && body?.Statements.Count > 0)
            {
                ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(type, context.CancellationToken);

                ComputeCodeFix(context, diagnostic, typeSymbol, body);
            }
        }

        private void ComputeCodeFix(
            CodeFixContext context,
            Diagnostic diagnostic,
            ITypeSymbol typeSymbol,
            BlockSyntax body)
        {
            if (typeSymbol?.IsErrorType() == false
                && !typeSymbol.IsVoid()
                && !typeSymbol.OriginalDefinition.IsIEnumerableOrIEnumerableOfT())
            {
                CodeAction codeAction = CodeAction.Create(
                    "Add return statement that returns default value",
                    cancellationToken => RefactorAsync(context.Document, body, typeSymbol, cancellationToken),
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
        }

        private static Task<Document> RefactorAsync(
            Document document,
            BlockSyntax body,
            ITypeSymbol typeSymbol,
            CancellationToken cancellationToken)
        {
            ExpressionSyntax returnExpression = typeSymbol.GetDefaultValueSyntax(document.GetDefaultSyntaxOptions());

            ReturnStatementSyntax returnStatement = SyntaxFactory.ReturnStatement(returnExpression);

            SyntaxList<StatementSyntax> newStatements = body.Statements.Add(returnStatement, ignoreLocalFunctions: true);

            BlockSyntax newBody = body.WithStatements(newStatements);

            return document.ReplaceNodeAsync(body, newBody, cancellationToken);
        }
    }
}
