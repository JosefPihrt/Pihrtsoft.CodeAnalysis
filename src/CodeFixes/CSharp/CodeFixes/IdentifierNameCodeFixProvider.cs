// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(IdentifierNameCodeFixProvider))]
    [Shared]
    public sealed class IdentifierNameCodeFixProvider : BaseCodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(
                    CompilerDiagnosticIdentifiers.UseOfUnassignedLocalVariable,
                    CompilerDiagnosticIdentifiers.NameDoesNotExistInCurrentContext);
            }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out IdentifierNameSyntax identifierName))
                return;

            Document document = context.Document;
            CancellationToken cancellationToken = context.CancellationToken;

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                switch (diagnostic.Id)
                {
                    case CompilerDiagnosticIdentifiers.UseOfUnassignedLocalVariable:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.InitializeLocalVariableWithDefaultValue))
                                return;

                            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

                            if (!(semanticModel.GetSymbol(identifierName, cancellationToken) is ILocalSymbol localSymbol))
                                break;

                            ITypeSymbol typeSymbol = localSymbol.Type;

                            if (typeSymbol.Kind == SymbolKind.ErrorType)
                                break;

                            if (!(localSymbol.GetSyntax(cancellationToken) is VariableDeclaratorSyntax variableDeclarator))
                                break;

                            CodeAction codeAction = CodeAction.Create(
                                $"Initialize '{identifierName.Identifier.ValueText}' with default value",
                                ct =>
                                {
                                    SyntaxToken identifier = variableDeclarator.Identifier;

                                    var variableDeclaration = (VariableDeclarationSyntax)variableDeclarator.Parent;

                                    ExpressionSyntax value = typeSymbol.GetDefaultValueSyntax(variableDeclaration.Type.WithoutTrivia(), document.GetDefaultSyntaxOptions());

                                    if (value.IsKind(SyntaxKind.DefaultExpression)
                                        && document.SupportsLanguageFeature(CSharpLanguageFeature.DefaultLiteral))
                                    {
                                        value = CSharpFactory.DefaultLiteralExpression().WithTriviaFrom(value);
                                    }

                                    EqualsValueClauseSyntax newEqualsValue = EqualsValueClause(value)
                                        .WithLeadingTrivia(TriviaList(Space))
                                        .WithTrailingTrivia(identifier.TrailingTrivia);

                                    VariableDeclaratorSyntax newNode = variableDeclarator
                                        .WithInitializer(newEqualsValue)
                                        .WithIdentifier(identifier.WithoutTrailingTrivia());

                                    return document.ReplaceNodeAsync(variableDeclarator, newNode, ct);
                                },
                                GetEquivalenceKey(diagnostic));

                            context.RegisterCodeFix(codeAction, diagnostic);
                            break;
                        }
                    case CompilerDiagnosticIdentifiers.NameDoesNotExistInCurrentContext:
                        {
                            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.AddVariableType))
                                return;

                            if (!(identifierName.Parent is ArgumentSyntax argument))
                                break;

                            if (argument.RefOrOutKeyword.Kind() != SyntaxKind.OutKeyword)
                                break;

                            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                            foreach (ITypeSymbol typeSymbol in DetermineParameterTypeHelper.DetermineParameterTypes(argument, semanticModel, cancellationToken))
                            {
                                if (typeSymbol.Kind == SymbolKind.TypeParameter)
                                    continue;

                                string typeName = SymbolDisplay.ToMinimalDisplayString(typeSymbol, semanticModel, identifierName.SpanStart, SymbolDisplayFormats.DisplayName);

                                CodeAction codeAction = CodeAction.Create(
                                    $"Add variable type '{typeName}'",
                                    ct =>
                                    {
                                        DeclarationExpressionSyntax newNode = DeclarationExpression(
                                            typeSymbol.ToTypeSyntax(),
                                            SingleVariableDesignation(identifierName.Identifier.WithoutTrivia()).WithLeadingTrivia(Space));

                                        newNode = newNode
                                            .WithTriviaFrom(identifierName)
                                            .WithFormatterAnnotation();

                                        return document.ReplaceNodeAsync(identifierName, newNode, ct);
                                    },
                                    GetEquivalenceKey(diagnostic, typeName));

                                context.RegisterCodeFix(codeAction, diagnostic);
                                break;
                            }

                            break;
                        }
                }
            }
        }
    }
}
