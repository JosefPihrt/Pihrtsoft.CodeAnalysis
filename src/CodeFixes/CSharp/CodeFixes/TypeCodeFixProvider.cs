// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TypeCodeFixProvider))]
    [Shared]
    public sealed class TypeCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CompilerDiagnosticIdentifiers.UsingGenericTypeRequiresNumberOfTypeArguments); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics[0];

            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.AddTypeArgument))
                return;

            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out TypeSyntax type))
                return;

            if (!type.IsKind(SyntaxKind.IdentifierName))
                return;

            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

            SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(type, context.CancellationToken);

            foreach (ISymbol symbol in symbolInfo.CandidateSymbols)
            {
                if (symbol is INamedTypeSymbol namedTypeSymbol)
                {
                    ImmutableArray<ITypeParameterSymbol> typeParameters = namedTypeSymbol.TypeParameters;

                    if (typeParameters.Any())
                    {
                        CodeAction codeAction = CodeAction.Create(
                            GetTitle(typeParameters),
                            cancellationToken =>
                            {
                                SeparatedSyntaxList<TypeSyntax> typeArguments = CreateTypeArguments(typeParameters, type.SpanStart, semanticModel).ToSeparatedSyntaxList();

                                var identifierName = (IdentifierNameSyntax)type;

                                GenericNameSyntax newNode = SyntaxFactory.GenericName(identifierName.Identifier, SyntaxFactory.TypeArgumentList(typeArguments));

                                return context.Document.ReplaceNodeAsync(type, newNode, cancellationToken);
                            },
                            GetEquivalenceKey(diagnostic, SymbolDisplay.ToDisplayString(namedTypeSymbol, SymbolDisplayFormats.DisplayName)));

                        context.RegisterCodeFix(codeAction, diagnostic);
                    }
                }
            }
        }

        private static string GetTitle(ImmutableArray<ITypeParameterSymbol> typeParameters)
        {
            if (typeParameters.Length == 1)
            {
                return $"Add type argument {typeParameters[0].Name}";
            }
            else
            {
                return $"Add type arguments {string.Join(", ", typeParameters.Select(f => f.Name))}";
            }
        }

        private static IEnumerable<TypeSyntax> CreateTypeArguments(
            ImmutableArray<ITypeParameterSymbol> typeParameters,
            int position,
            SemanticModel semanticModel)
        {
            var isFirst = true;

            ImmutableArray<ISymbol> symbols = semanticModel.LookupSymbols(position);

            foreach (ITypeParameterSymbol typeParameter in typeParameters)
            {
                string name = NameGenerator.Default.EnsureUniqueName(
                    typeParameter.Name,
                    symbols);

                SyntaxToken identifier = SyntaxFactory.Identifier(name);

                if (isFirst)
                {
                    identifier = identifier.WithRenameAnnotation();
                    isFirst = false;
                }

                yield return SyntaxFactory.IdentifierName(identifier);
            }
        }
    }
}
