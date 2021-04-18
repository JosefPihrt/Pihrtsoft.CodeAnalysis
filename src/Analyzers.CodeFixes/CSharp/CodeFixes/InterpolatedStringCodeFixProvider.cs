// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using Roslynator.CSharp.Analysis;
using Roslynator.CSharp.Refactorings;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InterpolatedStringCodeFixProvider))]
    [Shared]
    public sealed class InterpolatedStringCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.UnnecessaryInterpolatedString,
                    DiagnosticIdentifiers.ConvertInterpolatedStringToConcatenation);
            }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out InterpolatedStringExpressionSyntax interpolatedString))
                return;

            Document document = context.Document;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case DiagnosticIdentifiers.UnnecessaryInterpolatedString:
                        {
                            if (ConvertInterpolatedStringToStringLiteralAnalysis.IsFixable(interpolatedString))
                            {
                                CodeAction codeAction = CodeAction.Create(
                                    "Remove '$'",
                                    ct => ConvertInterpolatedStringToStringLiteralRefactoring.RefactorAsync(document, interpolatedString, ct),
                                    GetEquivalenceKey(diagnostic.Id));

                                context.RegisterCodeFix(codeAction, diagnostic);
                            }
                            else
                            {
                                var interpolation = (InterpolationSyntax)interpolatedString.Contents[0];

                                CodeAction codeAction = CodeAction.Create(
                                    $"Replace interpolated string with '{interpolation.Expression}'",
                                    ct => UnnecessaryInterpolatedStringRefactoring.RefactorAsync(document, interpolatedString, ct),
                                    GetEquivalenceKey(diagnostic.Id));

                                context.RegisterCodeFix(codeAction, diagnostic);
                            }

                            break;
                        }
                    case DiagnosticIdentifiers.ConvertInterpolatedStringToConcatenation:
                        {
                            CodeAction codeAction = CodeAction.Create(
                                "Convert to concatenation",
                                cancellationToken => ConvertInterpolatedStringToConcatenationRefactoring.RefactorAsync(document, interpolatedString, cancellationToken),
                                GetEquivalenceKey(diagnostic.Id));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                }
            }
        }
    }
}
