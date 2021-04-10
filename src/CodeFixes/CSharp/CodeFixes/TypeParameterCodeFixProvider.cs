// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TypeParameterCodeFixProvider))]
    [Shared]
    public sealed class TypeParameterCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CompilerDiagnosticIdentifiers.TypeParameterHasSameNameAsTypeParameterFromOuterType); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics[0];

            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveTypeParameter))
                return;

            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out TypeParameterSyntax typeParameter))
                return;

            string name = typeParameter.Identifier.ValueText;

            if (string.IsNullOrEmpty(name))
                return;

            CodeAction codeAction = CodeAction.Create(
                $"Remove type parameter '{name}'",
                cancellationToken =>
                {
                    GenericInfo genericInfo = SyntaxInfo.GenericInfo(typeParameter);

                    GenericInfo newGenericInfo = genericInfo.RemoveTypeParameter(typeParameter);

                    TypeParameterConstraintClauseSyntax constraintClause = genericInfo.FindConstraintClause(name);

                    if (constraintClause != null)
                        newGenericInfo = newGenericInfo.RemoveConstraintClause(constraintClause);

                    return context.Document.ReplaceNodeAsync(genericInfo.Node, newGenericInfo.Node, cancellationToken);
                },
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
        }
    }
}
