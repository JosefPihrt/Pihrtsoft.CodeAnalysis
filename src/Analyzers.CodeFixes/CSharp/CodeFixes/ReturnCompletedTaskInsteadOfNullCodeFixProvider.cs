// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
using Roslynator.CodeFixes;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ReturnCompletedTaskInsteadOfNullCodeFixProvider))]
    [Shared]
    public sealed class ReturnCompletedTaskInsteadOfNullCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.ReturnCompletedTaskInsteadOfNull); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindNode(root, context.Span, out ExpressionSyntax expression))
                return;

            if (expression.IsKind(SyntaxKind.ConditionalAccessExpression))
                return;

            expression = expression.WalkUpParentheses();

            Diagnostic diagnostic = context.Diagnostics[0];

            CodeAction codeAction = CodeAction.Create(
                "Use completed task",
                ct => RefactorAsync(context.Document, expression, ct),
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        private static async Task<Document> RefactorAsync(
            Document document,
            ExpressionSyntax expression,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            ExpressionSyntax newExpression = CreateCompletedTaskExpression(document, expression, semanticModel, cancellationToken);

            return await document.ReplaceNodeAsync(expression, newExpression, cancellationToken).ConfigureAwait(false);
        }

        internal static ExpressionSyntax CreateCompletedTaskExpression(
            Document document,
            ExpressionSyntax expression,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            var typeSymbol = (INamedTypeSymbol)semanticModel.GetTypeInfo(expression, cancellationToken).ConvertedType;

            TypeSyntax taskType = typeSymbol
                .BaseTypesAndSelf()
                .First(f => f.HasMetadataName(MetadataNames.System_Threading_Tasks_Task))
                .ToTypeSyntax()
                .WithSimplifierAnnotation();

            if (typeSymbol.HasMetadataName(MetadataNames.System_Threading_Tasks_Task))
            {
                MemberAccessExpressionSyntax newNode = SimpleMemberAccessExpression(
                    taskType,
                    IdentifierName("CompletedTask"));

                return newNode.WithTriviaFrom(expression);
            }
            else
            {
                ITypeSymbol typeArgument = typeSymbol.TypeArguments[0];

                TypeSyntax type = typeArgument.ToTypeSyntax().WithSimplifierAnnotation();

                ExpressionSyntax defaultValue = typeArgument.GetDefaultValueSyntax(type, document.GetDefaultSyntaxOptions());

                SimpleNameSyntax name;
                if (defaultValue.IsKind(
                    SyntaxKind.TrueLiteralExpression,
                    SyntaxKind.FalseLiteralExpression,
                    SyntaxKind.CharacterLiteralExpression,
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxKind.DefaultExpression))
                {
                    name = IdentifierName("FromResult");
                }
                else
                {
                    name = GenericName("FromResult", type);
                }

                InvocationExpressionSyntax newNode = SimpleMemberInvocationExpression(
                    taskType,
                    name,
                    Argument(defaultValue));

                return newNode.WithTriviaFrom(expression);
            }
        }
    }
}
