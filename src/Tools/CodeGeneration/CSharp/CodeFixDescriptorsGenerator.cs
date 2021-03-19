// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp;
using Roslynator.Documentation;
using Roslynator.Metadata;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CodeGeneration.CSharp
{
    public static class CodeFixDescriptorsGenerator
    {
        public static CompilationUnitSyntax Generate(
            IEnumerable<CodeFixMetadata> codeFixes,
            IComparer<string> comparer,
            string @namespace)
        {
            CompilationUnitSyntax compilationUnit = CompilationUnit(
                UsingDirectives("Roslynator.CodeFixes"),
                NamespaceDeclaration(
                    @namespace,
                    ClassDeclaration(
                        Modifiers.Public_Static_Partial(),
                        "CodeFixDescriptors",
                        List(
                            CreateMembers(
                                codeFixes
                                    .OrderBy(f => f.Id, comparer))))));

            compilationUnit = compilationUnit.NormalizeWhitespace();

            return (CompilationUnitSyntax)Rewriter.Instance.Visit(compilationUnit);
        }

        private static IEnumerable<MemberDeclarationSyntax> CreateMembers(IEnumerable<CodeFixMetadata> codeFixes)
        {
            foreach (CodeFixMetadata codeFix in codeFixes)
            {
                var arguments = new List<ArgumentSyntax>()
                {
                    Argument(
                        NameColon("id"),
                        SimpleMemberAccessExpression(IdentifierName("CodeFixIdentifiers"), IdentifierName(codeFix.Identifier))),

                    Argument(
                        NameColon("title"),
                        StringLiteralExpression(codeFix.Title)),

                    Argument(
                        NameColon("isEnabledByDefault"),
                        BooleanLiteralExpression(true))
                };

                foreach (string diagnosticId in codeFix.FixableDiagnosticIds.OrderBy(f => f))
                    arguments.Add(Argument(StringLiteralExpression(diagnosticId)));

                FieldDeclarationSyntax fieldDeclaration = FieldDeclaration(
                    Modifiers.Public_Static_ReadOnly(),
                    IdentifierName("CodeFixDescriptor"),
                    codeFix.Identifier,
                    ObjectCreationExpression(
                        IdentifierName("CodeFixDescriptor"),
                        ArgumentList(arguments.ToSeparatedSyntaxList())));

                var settings = new DocumentationCommentGeneratorSettings(
                    summary: new string[] { $"{codeFix.Id} (fixes {string.Join(", ", codeFix.FixableDiagnosticIds.OrderBy(f => f))})" },
                    ignoredTags: new[] { "returns", "value" },
                    indentation: "        ",
                    singleLineSummary: true);

                fieldDeclaration = fieldDeclaration.WithNewSingleLineDocumentationComment(settings);

                yield return fieldDeclaration;
            }
        }

        private class Rewriter : CSharpSyntaxRewriter
        {
            public static Rewriter Instance { get; } = new Rewriter();

            public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
            {
                node = (FieldDeclarationSyntax)base.VisitFieldDeclaration(node);

                return node.AppendToTrailingTrivia(NewLine());
            }

            public override SyntaxNode VisitArgument(ArgumentSyntax node)
            {
                ExpressionSyntax newExpression = node.Expression.PrependToLeadingTrivia(Whitespace(new string(' ', 18 - node.NameColon?.Name.Identifier.ValueText.Length ?? 0)));

                if (node.NameColon != null)
                {
                    node = node.WithNameColon(node.NameColon.AppendToLeadingTrivia(TriviaList(NewLine(), Whitespace("            "))));
                }
                else
                {
                    newExpression = newExpression.AppendToLeadingTrivia(TriviaList(NewLine(), Whitespace("            ")));
                }

                return node.WithExpression(newExpression);
            }

            public override SyntaxNode VisitAttribute(AttributeSyntax node)
            {
                return node;
            }
        }
    }
}
