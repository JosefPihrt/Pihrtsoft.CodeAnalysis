// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CodeFixes;

namespace Roslynator.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SyntaxErrorCharExpectedCodeFixProvider))]
    [Shared]
    public sealed class SyntaxErrorCharExpectedCodeFixProvider : BaseCodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(CompilerDiagnosticIdentifiers.SyntaxErrorCharExpected); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            Diagnostic diagnostic = context.Diagnostics[0];

            if (!Settings.IsEnabled(diagnostic.Id, CodeFixIdentifiers.AddMissingComma))
                return;

            SyntaxNode root = await context.GetSyntaxRootAsync().ConfigureAwait(false);

            ExpressionSyntax expression = root.FindNode(context.Span).FirstAncestorOrSelf<ExpressionSyntax>();

            if (expression == null)
                return;

            int position = -1;

            switch (expression.Parent.Kind())
            {
                case SyntaxKind.ArrayInitializerExpression:
                    {
                        var initializer = (InitializerExpressionSyntax)expression.Parent;

                        SeparatedSyntaxList<ExpressionSyntax> expressions = initializer.Expressions;

                        position = FindMissingCommaPosition(expressions, expression);
                        break;
                    }
                case SyntaxKind.EqualsValueClause:
                    {
                        var equalsValueClause = (EqualsValueClauseSyntax)expression.Parent;

                        if (equalsValueClause.Parent is EnumMemberDeclarationSyntax enumMemberDeclaration)
                        {
                            var enumDeclaration = (EnumDeclarationSyntax)enumMemberDeclaration.Parent;

                            position = FindMissingCommaPosition(enumDeclaration.Members, enumMemberDeclaration);
                        }

                        break;
                    }
            }

            if (position == -1)
                return;

            CodeAction codeAction = CodeAction.Create(
                "Add missing comma",
                ct => context.Document.WithTextChangeAsync(new TextSpan(position, 0), ",", ct),
                GetEquivalenceKey(diagnostic));

            context.RegisterCodeFix(codeAction, diagnostic);

            static int FindMissingCommaPosition<TNode>(SeparatedSyntaxList<TNode> nodes, TNode node) where TNode : SyntaxNode
            {
                int index = nodes.IndexOf(node);

                Debug.Assert(index > 0);

                if (index <= 0)
                    return -1;

                if (nodes.GetSeparator(index - 1).IsMissing)
                {
                    return nodes[index - 1].Span.End;
                }
                else
                {
                    Debug.Assert(index < nodes.Count - 1);

                    if (index == nodes.Count - 1)
                        return -1;

                    Debug.Assert(nodes.GetSeparator(index).IsMissing);

                    if (!nodes.GetSeparator(index).IsMissing)
                        return -1;

                    return node.Span.End;
                }
            }
        }
    }
}
