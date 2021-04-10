// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MemberHidesInheritedMemberCodeFixProvider))]
    [Shared]
    public sealed class MemberHidesInheritedMemberCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    CompilerDiagnosticIdentifiers.MemberHidesInheritedMemberUseNewKeywordIfHidingWasIntended,
                    CompilerDiagnosticIdentifiers.MemberHidesInheritedMemberToMakeCurrentMethodOverrideThatImplementationAddOverrideKeyword);
            }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out MemberDeclarationSyntax memberDeclaration))
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case CompilerDiagnosticIdentifiers.MemberHidesInheritedMemberUseNewKeywordIfHidingWasIntended:
                        {
                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.AddNewModifier))
                                ModifiersCodeFixRegistrator.AddModifier(context, diagnostic, memberDeclaration, SyntaxKind.NewKeyword, additionalKey: nameof(SyntaxKind.NewKeyword));

                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveMemberDeclaration))
                                CodeFixRegistrator.RemoveMemberDeclaration(context, diagnostic, memberDeclaration);

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.MemberHidesInheritedMemberToMakeCurrentMethodOverrideThatImplementationAddOverrideKeyword:
                        {
                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.AddOverrideModifier)
                                && !SyntaxInfo.ModifierListInfo(memberDeclaration).IsStatic)
                            {
                                ModifiersCodeFixRegistrator.AddModifier(context, diagnostic, memberDeclaration, SyntaxKind.OverrideKeyword, additionalKey: nameof(SyntaxKind.OverrideKeyword));
                            }

                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.AddNewModifier))
                                ModifiersCodeFixRegistrator.AddModifier(context, diagnostic, memberDeclaration, SyntaxKind.NewKeyword, additionalKey: nameof(SyntaxKind.NewKeyword));

                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveMemberDeclaration))
                                CodeFixRegistrator.RemoveMemberDeclaration(context, diagnostic, memberDeclaration);

                            break;
                        }
                }
            }
        }
    }
}
