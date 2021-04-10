// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Refactorings;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OperatorCannotBeAppliedToOperandsCodeFixProvider))]
    [Shared]
    public sealed class OperatorCannotBeAppliedToOperandsCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CompilerDiagnosticIdentifiers.OperatorCannotBeAppliedToOperands); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics[0];

            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.AddComparisonWithBooleanLiteral))
                return;

            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindNode(root, context.Span, out BinaryExpressionSyntax binaryExpression))
                return;

            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

            bool success = RegisterCodeFix(context, binaryExpression.Left, diagnostic, semanticModel);

            if (!success)
                RegisterCodeFix(context, binaryExpression.Right, diagnostic, semanticModel);
        }

        private bool RegisterCodeFix(
            CodeFixContext context,
            ExpressionSyntax expression,
            Diagnostic diagnostic,
            SemanticModel semanticModel)
        {
            if (expression?.IsMissing == false
                && semanticModel.GetTypeSymbol(expression, context.CancellationToken)?.IsNullableOf(SpecialType.System_Boolean) == true)
            {
                CodeAction codeAction = CodeAction.Create(
                    AddComparisonWithBooleanLiteralRefactoring.GetTitle(expression),
                    cancellationToken => AddComparisonWithBooleanLiteralRefactoring.RefactorAsync(context.Document, expression, cancellationToken),
                    GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
                return true;
            }

            return false;
        }
    }
}
