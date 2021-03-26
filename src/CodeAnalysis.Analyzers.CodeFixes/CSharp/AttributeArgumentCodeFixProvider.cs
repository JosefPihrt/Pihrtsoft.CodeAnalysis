// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CodeAnalysis.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AttributeArgumentCodeFixProvider))]
    [Shared]
    public class AttributeArgumentCodeFixProvider : BaseCodeFixProvider
    {
        private static ImmutableDictionary<string, string> _languageNames;

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.UnknownLanguageName); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out AttributeArgumentSyntax attributeArgument))
                return;

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];

            switch (diagnostic.Id)
            {
                case DiagnosticIdentifiers.UnknownLanguageName:
                    {
                        foreach (string languageName in RoslynUtility.WellKnownLanguageNames)
                        {
                            CodeAction codeAction = CodeAction.Create(
                                $"Change language name to '{languageName}'",
                                ct => ChangeLanguageNameAsync(document, attributeArgument, languageName, ct),
                                GetEquivalenceKey(diagnostic, languageName));

                            context.RegisterCodeFix(codeAction, diagnostic);
                        }

                        break;
                    }
            }
        }

        private static Task<Document> ChangeLanguageNameAsync(
            Document document,
            AttributeArgumentSyntax attributeArgument,
            string languageName,
            CancellationToken cancellationToken)
        {
            if (_languageNames == null)
                Interlocked.CompareExchange(ref _languageNames, LoadLanguageNames(), null);

            AttributeArgumentSyntax newAttributeArgument = AttributeArgument(
                SimpleMemberAccessExpression(
                    ParseName("global::Microsoft.CodeAnalysis.LanguageNames").WithSimplifierAnnotation(),
                    IdentifierName(_languageNames[languageName])));

            return document.ReplaceNodeAsync(attributeArgument, newAttributeArgument, cancellationToken);

            static ImmutableDictionary<string, string> LoadLanguageNames()
            {
                return typeof(LanguageNames)
                    .GetRuntimeFields()
                    .Where(f => f.IsPublic)
                    .ToImmutableDictionary(f => (string)f.GetValue(null), f => f.Name);
            }
        }
    }
}
