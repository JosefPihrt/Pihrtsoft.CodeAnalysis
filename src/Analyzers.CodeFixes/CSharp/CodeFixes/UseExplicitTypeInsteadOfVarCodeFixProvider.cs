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
using Roslynator.CSharp;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseExplicitTypeInsteadOfVarCodeFixProvider))]
    [Shared]
    public sealed class UseExplicitTypeInsteadOfVarCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.UseExplicitTypeInsteadOfVarWhenTypeIsNotObvious,
                    DiagnosticIdentifiers.UseExplicitTypeInsteadOfVarWhenTypeIsObvious);
            }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out SyntaxNode node, predicate: f => f.IsKind(SyntaxKind.VariableDeclaration, SyntaxKind.DeclarationExpression)))
                return;

            if (node is VariableDeclarationSyntax variableDeclaration)
            {
                TypeSyntax type = variableDeclaration.Type;

                SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                ITypeSymbol typeSymbol = semanticModel.GetTypeSymbol(variableDeclaration.Variables[0].Initializer.Value, context.CancellationToken);

                RegisterCodeFix(context, type, typeSymbol, semanticModel);
            }
            else
            {
                var declarationExpression = (DeclarationExpressionSyntax)node;

                TypeSyntax type = declarationExpression.Type;

                SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                var localSymbol = semanticModel.GetDeclaredSymbol(declarationExpression.Designation, context.CancellationToken) as ILocalSymbol;

                ITypeSymbol typeSymbol = (localSymbol?.Type) ?? semanticModel.GetTypeSymbol(declarationExpression, context.CancellationToken);

                RegisterCodeFix(context, type, typeSymbol, semanticModel);
            }
        }

        private void RegisterCodeFix(CodeFixContext context, TypeSyntax type, ITypeSymbol typeSymbol, SemanticModel semanticModel)
        {
            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                CodeAction codeAction = CodeActionFactory.ChangeType(context.Document, type, typeSymbol, semanticModel, equivalenceKey: GetEquivalenceKey(diagnostic));

                context.RegisterCodeFix(codeAction, diagnostic);
            }
        }
    }
}