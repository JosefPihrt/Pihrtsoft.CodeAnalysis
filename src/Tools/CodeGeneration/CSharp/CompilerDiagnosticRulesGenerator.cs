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
    public static class CompilerDiagnosticRulesGenerator
    {
        public static CompilationUnitSyntax Generate(
            IEnumerable<CompilerDiagnosticMetadata> compilerDiagnostics,
            IComparer<string> comparer,
            string @namespace)
        {
            CompilationUnitSyntax compilationUnit = CompilationUnit(
                UsingDirectives("Microsoft.CodeAnalysis"),
                NamespaceDeclaration(
                    @namespace,
                    ClassDeclaration(
                        Modifiers.Public_Static_Partial(),
                        "CompilerDiagnosticRules",
                        List(
                            CreateMembers(
                                compilerDiagnostics
                                    .OrderBy(f => f.Id, comparer))))));

            compilationUnit = compilationUnit.NormalizeWhitespace();

            return (CompilationUnitSyntax)Rewriter.Instance.Visit(compilationUnit);
        }

        private static IEnumerable<MemberDeclarationSyntax> CreateMembers(IEnumerable<CompilerDiagnosticMetadata> compilerDiagnostics)
        {
            foreach (CompilerDiagnosticMetadata compilerDiagnostic in compilerDiagnostics)
            {
                FieldDeclarationSyntax fieldDeclaration = FieldDeclaration(
                    Modifiers.Public_Static_ReadOnly(),
                    IdentifierName("DiagnosticDescriptor"),
                    compilerDiagnostic.Identifier,
                    ObjectCreationExpression(
                        IdentifierName("DiagnosticDescriptor"),
                        ArgumentList(
                            Argument(
                                NameColon("id"),
                                SimpleMemberAccessExpression(IdentifierName("CompilerDiagnosticIdentifiers"), IdentifierName(compilerDiagnostic.Identifier))),
                            Argument(
                                NameColon("title"),
                                StringLiteralExpression(compilerDiagnostic.Title)),
                            Argument(
                                NameColon("messageFormat"),
                                StringLiteralExpression(compilerDiagnostic.MessageFormat)),
                            Argument(
                                NameColon("category"),
                                StringLiteralExpression("Compiler")),
                            Argument(
                                NameColon("defaultSeverity"),
                                SimpleMemberAccessExpression(IdentifierName("DiagnosticSeverity"), IdentifierName(compilerDiagnostic.Severity))),
                            Argument(
                                NameColon("isEnabledByDefault"),
                                BooleanLiteralExpression(true)),
                            Argument(
                                NameColon("description"),
                                NullLiteralExpression()),
                            Argument(
                                NameColon("helpLinkUri"),
                                StringLiteralExpression(compilerDiagnostic.HelpUrl)),
                            Argument(
                                NameColon("customTags"),
                                SimpleMemberAccessExpression(IdentifierName("WellKnownDiagnosticTags"), IdentifierName("Compiler")))
                            )));

                var settings = new DocumentationCommentGeneratorSettings(
                    summary: new string[] { compilerDiagnostic.Id },
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
                return node
                    .WithNameColon(node.NameColon.AppendToLeadingTrivia(TriviaList(NewLine(), Whitespace("            "))))
                    .WithExpression(node.Expression.PrependToLeadingTrivia(Whitespace(new string(' ', 18 - node.NameColon.Name.Identifier.ValueText.Length))));
            }

            public override SyntaxNode VisitAttribute(AttributeSyntax node)
            {
                return node;
            }
        }
    }
}
