// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConditionalExpressionClauseCodeFixProvider))]
    [Shared]
    public sealed class ConditionalExpressionClauseCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CompilerDiagnosticIdentifiers.TypeOfConditionalExpressionCannotBeDetermined); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics[0];

            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.AddCastExpression))
                return;

            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out ConditionalExpressionSyntax conditionalExpression))
                return;

            ExpressionSyntax whenTrue = conditionalExpression.WhenTrue;

            ExpressionSyntax whenFalse = conditionalExpression.WhenFalse;

            if (whenTrue?.IsMissing != false)
                return;

            if (whenFalse?.IsMissing != false)
                return;

            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

            ITypeSymbol falseType = semanticModel.GetTypeSymbol(whenFalse, context.CancellationToken);

            if (falseType?.IsErrorType() != false)
                return;

            ITypeSymbol destinationType = FindDestinationType(whenTrue, falseType.BaseType, semanticModel);

            if (destinationType == null)
                return;

            CodeFixRegistrator.AddCastExpression(context, diagnostic, whenTrue, destinationType, semanticModel);
        }

        private static ITypeSymbol FindDestinationType(ExpressionSyntax expression, ITypeSymbol type, SemanticModel semanticModel)
        {
            while (type != null)
            {
                if (semanticModel.IsImplicitConversion(expression, type))
                    return type;

                type = type.BaseType;
            }

            return null;
        }
    }
}
