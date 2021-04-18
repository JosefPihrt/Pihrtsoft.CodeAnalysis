// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using Roslynator.Formatting.CSharp;

namespace Roslynator.Formatting.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic, Name = nameof(NewLineCodeFixProvider))]
    [Shared]
    public sealed class NewLineCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.UseLinefeedAsNewLine,
                    DiagnosticIdentifiers.UseCarriageReturnAndLinefeedAsNewLine);
            }
        }

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];
            TextSpan span = context.Span;

            switch (diagnostic.Id)
            {
                case DiagnosticIdentifiers.UseLinefeedAsNewLine:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            "Use linefeed as newline",
                            ct => document.WithTextChangeAsync(span, "\n", ct),
                            base.GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
                case DiagnosticIdentifiers.UseCarriageReturnAndLinefeedAsNewLine:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            "Use carriage return + linefeed as newline",
                            ct => document.WithTextChangeAsync(span, "\r\n", ct),
                            base.GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
            }

            return Task.CompletedTask;
        }
    }
}

