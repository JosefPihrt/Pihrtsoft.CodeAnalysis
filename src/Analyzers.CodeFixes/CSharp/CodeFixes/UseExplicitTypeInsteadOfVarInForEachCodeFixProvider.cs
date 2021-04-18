// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseExplicitTypeInsteadOfVarInForEachCodeFixProvider))]
    [Shared]
    public sealed class UseExplicitTypeInsteadOfVarInForEachCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.UseExplicitTypeInsteadOfVarInForEach); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(
                root,
                context.Span,
                out SyntaxNode node,
                predicate: f => f.IsKind(SyntaxKind.ForEachStatement, SyntaxKind.ForEachVariableStatement, SyntaxKind.DeclarationExpression)))
            {
                return;
            }

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];

            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

            TypeSyntax type;
            ITypeSymbol typeSymbol;

            switch (node)
            {
                case ForEachStatementSyntax forEachStatement:
                    {
                        type = forEachStatement.Type;

                        typeSymbol = semanticModel.GetForEachStatementInfo((CommonForEachStatementSyntax)node).ElementType;
                        break;
                    }
                case ForEachVariableStatementSyntax forEachVariableStatement:
                    {
                        var declarationExpression = (DeclarationExpressionSyntax)forEachVariableStatement.Variable;

                        type = declarationExpression.Type;

                        typeSymbol = semanticModel.GetForEachStatementInfo((CommonForEachStatementSyntax)node).ElementType;
                        break;
                    }
                case DeclarationExpressionSyntax declarationExpression:
                    {
                        type = declarationExpression.Type;

                        typeSymbol = semanticModel.GetTypeSymbol(declarationExpression, context.CancellationToken);
                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException();
                    }
            }

            CodeAction codeAction = CodeActionFactory.ChangeType(document, type, typeSymbol, semanticModel, equivalenceKey: GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
        }
    }
}