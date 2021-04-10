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
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DeclareEnumMemberWithZeroValueCodeFixProvider))]
    [Shared]
    public sealed class DeclareEnumMemberWithZeroValueCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.DeclareEnumMemberWithZeroValue); }
        }

        public override FixAllProvider GetFixAllProvider() => null;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out EnumDeclarationSyntax enumDeclaration))
                return;

            CodeAction codeAction = CodeAction.Create(
                "Declare enum member with zero value",
                ct => RefactorAsync(context.Document, enumDeclaration, ct),
                GetEquivalenceKey(DiagnosticIdentifiers.DeclareEnumMemberWithZeroValue));

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }

        private static async Task<Document> RefactorAsync(
            Document document,
            EnumDeclarationSyntax enumDeclaration,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            INamedTypeSymbol symbol = semanticModel.GetDeclaredSymbol(enumDeclaration, cancellationToken);

            string name = NameGenerator.Default.EnsureUniqueEnumMemberName("None", symbol);

            EnumMemberDeclarationSyntax enumMember = EnumMemberDeclaration(
                Identifier(name).WithRenameAnnotation(),
                NumericLiteralExpression(0));

            enumMember = enumMember.WithTrailingTrivia(NewLine());

            EnumDeclarationSyntax newNode = enumDeclaration.WithMembers(enumDeclaration.Members.Insert(0, enumMember));

            return await document.ReplaceNodeAsync(enumDeclaration, newNode, cancellationToken).ConfigureAwait(false);
        }
    }
}
