// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AttributeCodeFixProvider))]
    [Shared]
    public sealed class AttributeCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    CompilerDiagnosticIdentifiers.AttributeIsNotValidOnThisDeclarationType,
                    CompilerDiagnosticIdentifiers.AttributeIsOnlyValidOnMethodsOrAttributeClasses);
            }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics[0];

            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveAttribute))
                return;

            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out AttributeSyntax attribute))
                return;

            CodeAction codeAction = CodeAction.Create(
                $"Remove attribute '{attribute.Name}'",
                cancellationToken =>
                {
                    var attributeList = (AttributeListSyntax)attribute.Parent;

                    SeparatedSyntaxList<AttributeSyntax> attributes = attributeList.Attributes;

                    if (attributes.Count == 1)
                    {
                        return context.Document.RemoveNodeAsync(attributeList, cancellationToken);
                    }
                    else
                    {
                        return context.Document.RemoveNodeAsync(attribute, cancellationToken);
                    }
                },
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
        }
    }
}
