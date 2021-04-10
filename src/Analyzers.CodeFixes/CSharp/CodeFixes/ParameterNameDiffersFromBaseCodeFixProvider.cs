// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Rename;
using Roslynator.CodeFixes;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ParameterNameDiffersFromBaseCodeFixProvider))]
    [Shared]
    public sealed class ParameterNameDiffersFromBaseCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.ParameterNameDiffersFromBase); }
        }

        public override FixAllProvider GetFixAllProvider()
        {
            return null;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out ParameterSyntax parameter))
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.ParameterNameDiffersFromBase:
                        {
                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            var parameterSymbol = (IParameterSymbol)semanticModel.GetDeclaredSymbol(parameter, context.CancellationToken);

                            ISymbol containingSymbol = parameterSymbol.ContainingSymbol;

                            ISymbol baseParameterSymbol = null;

                            switch (containingSymbol.Kind)
                            {
                                case SymbolKind.Method:
                                    {
                                        var methodSymbol = (IMethodSymbol)containingSymbol;

                                        IMethodSymbol baseSymbol = methodSymbol.OverriddenMethod ?? methodSymbol.FindFirstImplementedInterfaceMember<IMethodSymbol>();

                                        baseParameterSymbol = baseSymbol.Parameters[parameterSymbol.Ordinal];
                                        break;
                                    }
                                case SymbolKind.Property:
                                    {
                                        var propertySymbol = (IPropertySymbol)containingSymbol;

                                        IPropertySymbol baseSymbol = propertySymbol.OverriddenProperty ?? propertySymbol.FindFirstImplementedInterfaceMember<IPropertySymbol>();

                                        baseParameterSymbol = baseSymbol.Parameters[parameterSymbol.Ordinal];
                                        break;
                                    }
                            }

                            string oldName = parameterSymbol.Name;

                            string newName = NameGenerator.Default.EnsureUniqueParameterName(
                                baseParameterSymbol.Name,
                                containingSymbol,
                                semanticModel,
                                cancellationToken: context.CancellationToken);

                            if (!string.Equals(newName, baseParameterSymbol.Name, StringComparison.Ordinal))
                                break;

                            CodeAction codeAction = CodeAction.Create(
                                $"Rename '{oldName}' to '{newName}'",
                                cancellationToken => Renamer.RenameSymbolAsync(context.Document.Solution(), parameterSymbol, newName, default(OptionSet), cancellationToken),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }
        }
    }
}
