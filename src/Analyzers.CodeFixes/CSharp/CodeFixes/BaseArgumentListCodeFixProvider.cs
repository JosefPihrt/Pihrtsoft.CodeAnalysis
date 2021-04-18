// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Analysis;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BaseArgumentListCodeFixProvider))]
    [Shared]
    public sealed class BaseArgumentListCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.OrderNamedArguments); }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out BaseArgumentListSyntax baseArgumentList))
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.OrderNamedArguments:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Order arguments",
                                ct => OrderNamedArgumentsAsync(context.Document, baseArgumentList, ct),
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }
        }

        private static async Task<Document> OrderNamedArgumentsAsync(
            Document document,
            BaseArgumentListSyntax argumentList,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            ImmutableArray<IParameterSymbol> parameters = semanticModel
                .GetSymbol(argumentList.Parent, cancellationToken)
                .ParametersOrDefault();

            SeparatedSyntaxList<ArgumentSyntax> arguments = argumentList.Arguments;

            int firstIndex = OrderNamedArgumentsAnalyzer.IndexOfFirstFixableParameter(argumentList, arguments, semanticModel, cancellationToken);

            SeparatedSyntaxList<ArgumentSyntax> newArguments = arguments;

            for (int i = firstIndex; i < arguments.Count; i++)
            {
                IParameterSymbol parameter = parameters[i];

                int index = arguments.IndexOf(f => f.NameColon?.Name.Identifier.ValueText == parameter.Name);

                Debug.Assert(index != -1, parameter.Name);

                if (index != -1
                    && index != i)
                {
                    newArguments = newArguments.ReplaceAt(i, arguments[index]);
                }
            }

            BaseArgumentListSyntax newNode = argumentList
                .WithArguments(newArguments)
                .WithFormatterAnnotation();

            return await document.ReplaceNodeAsync(argumentList, newNode, cancellationToken).ConfigureAwait(false);
        }
    }
}
