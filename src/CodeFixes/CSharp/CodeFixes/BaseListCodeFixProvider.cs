// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BaseListCodeFixProvider))]
    [Shared]
    public sealed class BaseListCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    CompilerDiagnosticIdentifiers.BaseClassMustComeBeforeAnyInterface,
                    CompilerDiagnosticIdentifiers.StaticClassCannotDeriveFromType,
                    CompilerDiagnosticIdentifiers.StaticClassCannotImplementInterfaces);
            }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out BaseListSyntax baseList))
                return;

            if (baseList.ContainsDiagnostics)
                return;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case CompilerDiagnosticIdentifiers.BaseClassMustComeBeforeAnyInterface:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.MoveBaseClassBeforeAnyInterface))
                                return;

                            SeparatedSyntaxList<BaseTypeSyntax> types = baseList.Types;

                            if (types.Count > 1)
                            {
                                BaseTypeSyntax baseType = types.First(f => context.Span.Contains(f.Span));

                                CodeAction codeAction = CodeAction.Create(
                                    $"Move '{baseType.Type}' before any interface",
                                    cancellationToken =>
                                    {
                                        BaseTypeSyntax firstType = types[0];

                                        SeparatedSyntaxList<BaseTypeSyntax> newTypes = types
                                            .Replace(baseType, firstType.WithTriviaFrom(baseType))
                                            .ReplaceAt(0, baseType.WithTriviaFrom(firstType));

                                        BaseListSyntax newBaseList = baseList.WithTypes(newTypes);

                                        return context.Document.ReplaceNodeAsync(baseList, newBaseList, cancellationToken);
                                    },
                                    GetEquivalenceKey(diagnostic));

                                context.RegisterCodeFix(codeAction, diagnostic);
                            }

                            break;
                        }
                    case CompilerDiagnosticIdentifiers.StaticClassCannotDeriveFromType:
                    case CompilerDiagnosticIdentifiers.StaticClassCannotImplementInterfaces:
                        {
                            if (!(baseList.Parent is ClassDeclarationSyntax classDeclaration))
                                break;

                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.MakeClassNonStatic))
                            {
                                ModifiersCodeFixRegistrator.RemoveModifier(
                                    context,
                                    diagnostic,
                                    classDeclaration,
                                    SyntaxKind.StaticKeyword,
                                    title: "Make class non-static",
                                    additionalKey: CodeFixIdentifiers.MakeClassNonStatic);
                            }

                            if (Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.RemoveBaseList))
                            {
                                CodeAction codeAction = CodeAction.Create(
                                    "Remove base list",
                                    cancellationToken =>
                                    {
                                        SyntaxToken token = baseList.GetFirstToken().GetPreviousToken();

                                        SyntaxTriviaList trivia = token.TrailingTrivia.EmptyIfWhitespace()
                                            .AddRange(baseList.GetLeadingTrivia().EmptyIfWhitespace())
                                            .AddRange(baseList.GetTrailingTrivia());

                                        ClassDeclarationSyntax newNode = classDeclaration
                                            .ReplaceToken(token, token.WithTrailingTrivia(trivia))
                                            .WithBaseList(null);

                                        return context.Document.ReplaceNodeAsync(classDeclaration, newNode, cancellationToken);
                                    },
                                    base.GetEquivalenceKey(diagnostic, CodeFixIdentifiers.RemoveBaseList));

                                context.RegisterCodeFix(codeAction, diagnostic);
                            }

                            break;
                        }
                }
            }
        }
    }
}
