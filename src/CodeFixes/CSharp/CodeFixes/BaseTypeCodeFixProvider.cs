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
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BaseTypeCodeFixProvider))]
    [Shared]
    public sealed class BaseTypeCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CompilerDiagnosticIdentifiers.TypeInInterfaceListIsNotInterface); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics[0];

            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.ReplaceStructWithClass))
                return;

            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out BaseTypeSyntax baseType))
                return;

            SyntaxNode parent = baseType.Parent;

            if (parent.Kind() != SyntaxKind.BaseList)
                return;

            parent = parent.Parent;

            if (!(parent is StructDeclarationSyntax structDeclaration))
                return;

            CodeAction codeAction = CodeAction.Create(
                "Replace 'struct' with 'class'",
                cancellationToken => context.Document.ReplaceNodeAsync(structDeclaration, ClassDeclaration(structDeclaration), cancellationToken),
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
        }
    }
}
