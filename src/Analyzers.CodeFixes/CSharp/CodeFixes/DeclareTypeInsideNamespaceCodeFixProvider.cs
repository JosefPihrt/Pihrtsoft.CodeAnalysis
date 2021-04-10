// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DeclareTypeInsideNamespaceCodeFixProvider))]
    [Shared]
    public sealed class DeclareTypeInsideNamespaceCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.DeclareTypeInsideNamespace); }
        }

        public override FixAllProvider GetFixAllProvider() => null;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindToken(root, context.Span.Start, out SyntaxToken identifier))
                return;

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];

            CodeAction codeAction = CodeAction.Create(
                $"Declare '{identifier.ValueText}' inside namespace",
                ct => DeclareTypeInsideNamespaceAsync(document, (MemberDeclarationSyntax)identifier.Parent, ct),
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        private static async Task<Document> DeclareTypeInsideNamespaceAsync(
            Document document,
            MemberDeclarationSyntax member,
            CancellationToken cancellationToken)
        {
            Debug.Assert(member.IsParentKind(SyntaxKind.CompilationUnit), member.Parent?.Kind().ToString());

            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            string name = NameGenerator.Default.EnsureUniqueName(
                DefaultNames.Namespace,
                semanticModel.LookupNamespacesAndTypes(member.SpanStart));

            NamespaceDeclarationSyntax namespaceDeclaration = NamespaceDeclaration(
                IdentifierName(Identifier(name).WithRenameAnnotation()),
                default(SyntaxList<ExternAliasDirectiveSyntax>),
                default(SyntaxList<UsingDirectiveSyntax>),
                SingletonList(member));

            return await document.ReplaceNodeAsync(member, namespaceDeclaration.WithFormatterAnnotation(), cancellationToken).ConfigureAwait(false);
        }
    }
}