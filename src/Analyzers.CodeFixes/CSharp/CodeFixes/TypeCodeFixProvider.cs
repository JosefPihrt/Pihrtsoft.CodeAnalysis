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

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TypeCodeFixProvider))]
    [Shared]
    public sealed class TypeCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.UseGenericEventHandler); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out TypeSyntax type))
                return;

            Diagnostic diagnostic = context.Diagnostics[0];

            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

            var delegateSymbol = (INamedTypeSymbol)semanticModel.GetTypeSymbol(type, context.CancellationToken);

            ITypeSymbol typeSymbol = delegateSymbol.DelegateInvokeMethod.Parameters[1].Type;

            INamedTypeSymbol eventHandlerSymbol = semanticModel
                .GetTypeByMetadataName("System.EventHandler`1")
                .Construct(typeSymbol);

            CodeAction codeAction = CodeAction.Create(
                $"Use '{SymbolDisplay.ToMinimalDisplayString(eventHandlerSymbol, semanticModel, type.SpanStart, SymbolDisplayFormats.DisplayName)}'",
                ct =>
                {
                    TypeSyntax newType = eventHandlerSymbol.ToTypeSyntax()
                        .WithSimplifierAnnotation()
                        .WithTriviaFrom(type);

                    return context.Document.ReplaceNodeAsync(type, newType, ct);
                },
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
        }
    }
}