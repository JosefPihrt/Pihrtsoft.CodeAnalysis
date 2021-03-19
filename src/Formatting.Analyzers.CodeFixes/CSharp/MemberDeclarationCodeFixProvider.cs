// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp;
using Roslynator.Formatting.CSharp;

namespace Roslynator.Formatting.CodeFixes.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MemberDeclarationCodeFixProvider))]
    [Shared]
    public class MemberDeclarationCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticIdentifiers.AddNewLineAfterOpeningBraceOfTypeDeclaration,
                    DiagnosticIdentifiers.AddNewLineBeforeConstructorInitializer);
            }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out MemberDeclarationSyntax memberDeclaration))
                return;

            Document document = context.Document;
            Diagnostic diagnostic = context.Diagnostics[0];

            switch (diagnostic.Id)
            {
                case DiagnosticIdentifiers.AddNewLineAfterOpeningBraceOfTypeDeclaration:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            CodeFixTitles.AddNewLine,
                            ct => AddNewLineAfterOpeningBraceOfTypeDeclarationAsync(document, memberDeclaration, ct),
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
                case DiagnosticIdentifiers.AddNewLineBeforeConstructorInitializer:
                    {
                        CodeAction codeAction = CodeAction.Create(
                            CodeFixTitles.AddNewLine,
                            ct =>
                            {
                                return CodeFixHelpers.AddNewLineBeforeAndIncreaseIndentationAsync(
                                    document,
                                    ((ConstructorDeclarationSyntax)memberDeclaration).Initializer.ColonToken,
                                    SyntaxTriviaAnalysis.AnalyzeIndentation(memberDeclaration, ct),
                                    ct);
                            },
                            GetEquivalenceKey(diagnostic));

                        context.RegisterCodeFix(codeAction, diagnostic);
                        break;
                    }
            }
        }

        private static Task<Document> AddNewLineAfterOpeningBraceOfTypeDeclarationAsync(
            Document document,
            MemberDeclarationSyntax declaration,
            CancellationToken cancellationToken)
        {
            MemberDeclarationSyntax newDeclaration = GetNewDeclaration().WithFormatterAnnotation();

            return document.ReplaceNodeAsync(declaration, newDeclaration, cancellationToken);

            MemberDeclarationSyntax GetNewDeclaration()
            {
                switch (declaration)
                {
                    case ClassDeclarationSyntax classDeclaration:
                        {
                            return classDeclaration.WithOpenBraceToken(classDeclaration.OpenBraceToken.AppendEndOfLineToTrailingTrivia());
                        }
                    case StructDeclarationSyntax structDeclaration:
                        {
                            return structDeclaration.WithOpenBraceToken(structDeclaration.OpenBraceToken.AppendEndOfLineToTrailingTrivia());
                        }
                    case InterfaceDeclarationSyntax interfaceDeclaration:
                        {
                            return interfaceDeclaration.WithOpenBraceToken(interfaceDeclaration.OpenBraceToken.AppendEndOfLineToTrailingTrivia());
                        }
                    default:
                        {
                            throw new InvalidOperationException();
                        }
                }
            }
        }
    }
}
