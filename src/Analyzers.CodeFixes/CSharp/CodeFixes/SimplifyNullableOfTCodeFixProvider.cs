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

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SimplifyNullableOfTCodeFixProvider))]
    [Shared]
    public sealed class SimplifyNullableOfTCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.SimplifyNullableOfT); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out TypeSyntax type, findInsideTrivia: true))
                return;

            TypeSyntax nullableType = GetNullableType(type);

            CodeAction codeAction = CodeAction.Create(
                $"Simplify name '{type}'",
                cancellationToken => SimplifyNullableOfTRefactoring.RefactorAsync(context.Document, type, nullableType, cancellationToken),
                GetEquivalenceKey(DiagnosticIdentifiers.SimplifyNullableOfT));

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }

        private static TypeSyntax GetNullableType(TypeSyntax type)
        {
            if (type.IsKind(SyntaxKind.QualifiedName))
                type = ((QualifiedNameSyntax)type).Right;

            return ((GenericNameSyntax)type).TypeArgumentList.Arguments[0];
        }
    }
}
