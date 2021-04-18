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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ParameterCodeFixProvider))]
    [Shared]
    public sealed class ParameterCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.OverridingMemberShouldNotChangeParamsModifier,
                    DiagnosticIdentifiers.MakeParameterRefReadOnly,
                    DiagnosticIdentifiers.DoNotPassNonReadOnlyStructByReadOnlyReference);
            }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out ParameterSyntax parameter))
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.OverridingMemberShouldNotChangeParamsModifier:
                        {
                            if (parameter.IsParams())
                            {
                                ModifiersCodeFixRegistrator.RemoveModifier(context, diagnostic, parameter, SyntaxKind.ParamsKeyword);
                            }
                            else
                            {
                                ModifiersCodeFixRegistrator.AddModifier(context, diagnostic, parameter, SyntaxKind.ParamsKeyword);
                            }

                            break;
                        }
                    case DiagnosticIdentifiers.MakeParameterRefReadOnly:
                    case DiagnosticIdentifiers.DoNotPassNonReadOnlyStructByReadOnlyReference:
                        {
                            if (parameter.Modifiers.Contains(SyntaxKind.InKeyword))
                            {
                                ModifiersCodeFixRegistrator.RemoveModifier(context, diagnostic, parameter, SyntaxKind.InKeyword);
                            }
                            else
                            {
                                ModifiersCodeFixRegistrator.AddModifier(context, diagnostic, parameter, SyntaxKind.InKeyword);
                            }

                            break;
                        }
                }
            }
        }
    }
}
