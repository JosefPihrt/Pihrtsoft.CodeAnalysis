// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseVarInsteadOfExplicitTypeCodeFixProvider))]
    [Shared]
    public sealed class UseVarInsteadOfExplicitTypeCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.UseVarInsteadOfExplicitTypeWhenTypeIsObvious,
                    DiagnosticIdentifiers.UseVarInsteadOfExplicitTypeWhenTypeIsNotObvious,
                    DiagnosticIdentifiers.UseVarInsteadOfExplicitTypeInForEach);
            }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(
                root,
                context.Span,
                out SyntaxNode node,
                predicate: f => f is TypeSyntax || f.IsKind(SyntaxKind.TupleExpression)))
            {
                return;
            }

            Diagnostic diagnostic = context.Diagnostics[0];
            Document document = context.Document;

            switch (node)
            {
                case TypeSyntax type:
                    {
                        CodeAction codeAction = CodeActionFactory.ChangeTypeToVar(document, type, equivalenceKey: GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
                case TupleExpressionSyntax tupleExpression:
                    {
                        CodeAction codeAction = CodeActionFactory.ChangeTypeToVar(document, tupleExpression, equivalenceKey: GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException();
                    }
            }
        }
    }
}