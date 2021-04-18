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
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MarkOperatorAsPublicAndStaticCodeFixProvider))]
    [Shared]
    public sealed class MarkOperatorAsPublicAndStaticCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CompilerDiagnosticIdentifiers.UserDefinedOperatorMustBeDeclaredStaticAndPublic); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics[0];

            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.MarkOperatorAsPublicAndStatic))
                return;

            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out MemberDeclarationSyntax memberDeclaration))
                return;

            ModifierListInfo info = SyntaxInfo.ModifierListInfo(memberDeclaration);

            var title = "Add ";

            if (info.ExplicitAccessibility == Accessibility.Public)
            {
                title += "modifier 'static'";
            }
            else if (info.IsStatic)
            {
                title += "modifier 'public'";
            }
            else
            {
                title += "modifiers 'public static'";
            }

            CodeAction codeAction = CodeAction.Create(
                title,
                cancellationToken =>
                {
                    SyntaxNode newNode = memberDeclaration;

                    if (info.Modifiers.ContainsAny(SyntaxKind.InternalKeyword, SyntaxKind.ProtectedKeyword, SyntaxKind.PrivateKeyword))
                        newNode = SyntaxAccessibility.WithoutExplicitAccessibility(newNode);

                    if (!info.Modifiers.Contains(SyntaxKind.PublicKeyword))
                        newNode = ModifierList.Insert(newNode, SyntaxKind.PublicKeyword);

                    if (!info.IsStatic)
                        newNode = ModifierList.Insert(newNode, SyntaxKind.StaticKeyword);

                    return context.Document.ReplaceNodeAsync(memberDeclaration, newNode, cancellationToken);
                },
                base.GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
        }
    }
}
