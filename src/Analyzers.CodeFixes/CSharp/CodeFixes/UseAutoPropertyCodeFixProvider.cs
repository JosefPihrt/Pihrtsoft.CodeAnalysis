// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CodeFixes;
using Roslynator.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseAutoPropertyCodeFixProvider))]
    [Shared]
    public sealed class UseAutoPropertyCodeFixProvider : BaseCodeFixProvider
    {
        private static readonly SyntaxAnnotation _removeAnnotation = new SyntaxAnnotation();

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIdentifiers.UseAutoProperty); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf(root, context.Span, out PropertyDeclarationSyntax property))
                return;

            Diagnostic diagnostic = context.Diagnostics[0];

            CodeAction codeAction = CodeAction.Create(
                "Use auto-property",
                cancellationToken => RefactorAsync(context.Document, property, cancellationToken),
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);
        }

        public static async Task<Document> RefactorAsync(
            Document document,
            PropertyDeclarationSyntax propertyDeclaration,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            SyntaxToken propertyIdentifier = propertyDeclaration.Identifier.WithoutTrivia();

            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            IPropertySymbol propertySymbol = semanticModel.GetDeclaredSymbol(propertyDeclaration, cancellationToken);

            ISymbol fieldSymbol = GetFieldSymbol(propertyDeclaration, semanticModel, cancellationToken);

            var variableDeclarator = (VariableDeclaratorSyntax)await fieldSymbol.GetSyntaxAsync(cancellationToken).ConfigureAwait(false);

            var variableDeclaration = (VariableDeclarationSyntax)variableDeclarator.Parent;

            var fieldDeclaration = (FieldDeclarationSyntax)variableDeclaration.Parent;

            bool isSingleDeclarator = variableDeclaration.Variables.Count == 1;

            ImmutableArray<SyntaxNode> nodes = await SyntaxFinder.FindReferencesAsync(fieldSymbol, document, allowCandidate: false, cancellationToken: cancellationToken).ConfigureAwait(false);

            nodes = nodes.Add(propertyDeclaration);

            if (isSingleDeclarator)
            {
                nodes = nodes.Add(fieldDeclaration);
            }
            else
            {
                nodes = nodes.Add(variableDeclarator);
            }

            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            SyntaxNode newRoot = root.ReplaceNodes(
                nodes,
                (node, _) =>
                {
                    switch (node.Kind())
                    {
                        case SyntaxKind.IdentifierName:
                            {
                                SyntaxNode newNode = null;

                                if (node.IsParentKind(SyntaxKind.SimpleMemberAccessExpression)
                                    && ((MemberAccessExpressionSyntax)node.Parent).Name == node)
                                {
                                    newNode = IdentifierName(propertyIdentifier);
                                }
                                else if (node.IsParentKind(SyntaxKind.NameMemberCref))
                                {
                                    newNode = IdentifierName(propertyIdentifier);
                                }
                                else if (propertySymbol.IsStatic)
                                {
                                    newNode = SimpleMemberAccessExpression(
                                        propertySymbol.ContainingType.ToTypeSyntax(),
                                        (SimpleNameSyntax)ParseName(propertySymbol.ToDisplayString(SymbolDisplayFormats.DisplayName)))
                                        .WithSimplifierAnnotation();
                                }
                                else
                                {
                                    newNode = IdentifierName(propertyIdentifier).QualifyWithThis();
                                }

                                return newNode.WithTriviaFrom(node);
                            }
                        case SyntaxKind.PropertyDeclaration:
                            {
                                return CreateAutoProperty(propertyDeclaration, variableDeclarator.Initializer);
                            }
                        case SyntaxKind.VariableDeclarator:
                        case SyntaxKind.FieldDeclaration:
                            {
                                return node.WithAdditionalAnnotations(_removeAnnotation);
                            }
                        default:
                            {
                                Debug.Fail(node.ToString());
                                return node;
                            }
                    }
                });

            SyntaxNode nodeToRemove = newRoot.GetAnnotatedNodes(_removeAnnotation).FirstOrDefault();

            if (nodeToRemove != null)
                newRoot = newRoot.RemoveNode(nodeToRemove, SyntaxRemoveOptions.KeepUnbalancedDirectives);

            return document.WithSyntaxRoot(newRoot);
        }

        private static ISymbol GetFieldSymbol(PropertyDeclarationSyntax property, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            ArrowExpressionClauseSyntax expressionBody = property.ExpressionBody;

            if (expressionBody != null)
            {
                return semanticModel.GetSymbol(expressionBody.Expression, cancellationToken);
            }
            else
            {
                AccessorDeclarationSyntax getter = property.Getter();

                BlockSyntax body = getter.Body;

                if (body != null)
                {
                    var returnStatement = (ReturnStatementSyntax)body.Statements[0];

                    return semanticModel.GetSymbol(returnStatement.Expression, cancellationToken);
                }
                else
                {
                    return semanticModel.GetSymbol(getter.ExpressionBody.Expression, cancellationToken);
                }
            }
        }

        public static PropertyDeclarationSyntax CreateAutoProperty(PropertyDeclarationSyntax property, EqualsValueClauseSyntax initializer)
        {
            AccessorListSyntax accessorList = property.AccessorList;

            if (accessorList != null)
            {
                SyntaxList<AccessorDeclarationSyntax> newAccessors = accessorList
                    .Accessors
                    .Select(accessor =>
                    {
                        accessor = accessor.Update(
                            attributeLists: accessor.AttributeLists,
                            modifiers: accessor.Modifiers,
                            keyword: accessor.Keyword,
                            body: default(BlockSyntax),
                            expressionBody: default(ArrowExpressionClauseSyntax),
                            semicolonToken: Token(default, SyntaxKind.SemicolonToken, TriviaList(NewLine())));

                        return accessor.WithTriviaFrom(accessor);
                    })
                    .ToSyntaxList();

                accessorList = accessorList.WithAccessors(newAccessors);
            }
            else
            {
                accessorList = AccessorList(AutoGetAccessorDeclaration().WithTrailingTrivia(NewLine()))
                    .WithTriviaFrom(property.ExpressionBody);
            }

            if (accessorList.Accessors.All(f => !f.AttributeLists.Any())
                && accessorList.DescendantTrivia().All(f => f.IsWhitespaceOrEndOfLineTrivia()))
            {
                accessorList = accessorList.RemoveWhitespace();
            }

            PropertyDeclarationSyntax newProperty = property.Update(
                attributeLists: property.AttributeLists,
                modifiers: property.Modifiers,
                type: property.Type,
                explicitInterfaceSpecifier: property.ExplicitInterfaceSpecifier,
                identifier: property.Identifier.WithTrailingTrivia(Space),
                accessorList: accessorList,
                expressionBody: default(ArrowExpressionClauseSyntax),
                initializer: initializer,
                semicolonToken: (initializer != null) ? SemicolonToken() : default);

            return newProperty
                .WithTriviaFrom(property)
                .WithFormatterAnnotation();
        }
    }
}
