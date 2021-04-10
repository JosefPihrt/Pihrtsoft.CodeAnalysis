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
    public class DiagnosticRulesGenerator
    {
        public CompilationUnitSyntax Generate(
            IEnumerable<AnalyzerMetadata> analyzers,
            bool obsolete,
            IComparer<string> comparer,
            string @namespace,
            string className,
            string identifiersClassName)
        {
            analyzers = analyzers
                .Where(f => f.IsObsolete == obsolete)
                .OrderBy(f => f.Id, comparer);

            ClassDeclarationSyntax classDeclaration = CreateClassDeclaration(
                analyzers,
                className,
                identifiersClassName);

            CompilationUnitSyntax compilationUnit = CompilationUnit(
                UsingDirectives("System", "Microsoft.CodeAnalysis"),
                NamespaceDeclaration(
                    @namespace,
                    classDeclaration));

            compilationUnit = compilationUnit.NormalizeWhitespace();

            return (CompilationUnitSyntax)Rewriter.Instance.Visit(compilationUnit);
        }

        private IEnumerable<MemberDeclarationSyntax> CreateMembers(IEnumerable<AnalyzerMetadata> analyzers, string identifiersClassName, bool useParentProperties = false)
        {
            foreach (AnalyzerMetadata analyzer in analyzers)
            {
                if (analyzer.Id == null)
                    continue;

                string identifier = analyzer.Identifier;
                string title = analyzer.Title;
                string messageFormat = analyzer.MessageFormat;
                bool isEnabledByDefault = analyzer.IsEnabledByDefault;

                yield return CreateMember(
                    analyzer,
                    identifiersClassName,
                    useParentProperties);

                if (analyzer.SupportsFadeOutAnalyzer)
                {
                    yield return FieldDeclaration(
                        Modifiers.Public_Static_ReadOnly(),
                        IdentifierName("DiagnosticDescriptor"),
                        identifier + "FadeOut",
                        SimpleMemberInvocationExpression(
                            IdentifierName("DiagnosticDescriptorFactory"),
                            IdentifierName("CreateFadeOut"),
                            ArgumentList(Argument(IdentifierName(identifier)))))
                        .AddObsoleteAttributeIf(analyzer.IsObsolete, error: true);
                }
            }

            IEnumerable<AnalyzerMetadata> optionAnalyzers = analyzers.SelectMany(f => f.OptionAnalyzers.Where(f => f.Kind == AnalyzerOptionKind.Change || f.Kind == AnalyzerOptionKind.Invert));

            if (optionAnalyzers.Any())
            {
                yield return CreateClassDeclaration(optionAnalyzers, "ReportOnly", identifiersClassName, useParentProperties = true);
            }
        }

        protected virtual ClassDeclarationSyntax CreateClassDeclaration(
            IEnumerable<AnalyzerMetadata> analyzers,
            string className,
            string identifiersClassName,
            bool useParentProperties = false)
        {
            return ClassDeclaration(
                Modifiers.Public_Static_Partial(),
                className,
                List(
                    CreateMembers(
                        analyzers,
                        identifiersClassName,
                        useParentProperties)));
        }

        private MemberDeclarationSyntax CreateMember(
            AnalyzerMetadata analyzer,
            string identifiersClassName,
            bool useParentProperties = false)
        {
            AnalyzerMetadata parent = (useParentProperties) ? analyzer.Parent : null;

            ExpressionSyntax idExpression = SimpleMemberAccessExpression(IdentifierName(identifiersClassName), IdentifierName(parent?.Identifier ?? analyzer.Identifier));

            idExpression = ModifyIdExpression(idExpression);

            FieldDeclarationSyntax fieldDeclaration = FieldDeclaration(
                (analyzer.IsObsolete) ? Modifiers.Internal_Static_ReadOnly() : Modifiers.Public_Static_ReadOnly(),
                IdentifierName("DiagnosticDescriptor"),
                analyzer.Identifier,
                SimpleMemberInvocationExpression(
                    SimpleMemberAccessExpression(IdentifierName("DiagnosticDescriptorFactory"), IdentifierName("Default")),
                    IdentifierName("Create"),
                    ArgumentList(
                        Argument(
                            NameColon("id"),
                            idExpression),
                        Argument(
                            NameColon("title"),
                            StringLiteralExpression(parent?.Title ?? analyzer.Title)),
                        Argument(
                            NameColon("messageFormat"),
                            StringLiteralExpression(analyzer.MessageFormat)),
                        Argument(
                            NameColon("category"),
                            SimpleMemberAccessExpression(IdentifierName("DiagnosticCategories"), IdentifierName(parent?.Category ?? analyzer.Category))),
                        Argument(
                            NameColon("defaultSeverity"),
                            SimpleMemberAccessExpression(IdentifierName("DiagnosticSeverity"), IdentifierName(parent?.DefaultSeverity ?? analyzer.DefaultSeverity))),
                        Argument(
                            NameColon("isEnabledByDefault"),
                            BooleanLiteralExpression(parent?.IsEnabledByDefault ?? analyzer.IsEnabledByDefault)),
                        Argument(
                            NameColon("description"),
                            NullLiteralExpression()),
                        Argument(
                            NameColon("helpLinkUri"),
                            idExpression),
                        Argument(
                            NameColon("customTags"),
                            (analyzer.SupportsFadeOut)
                                ? SimpleMemberAccessExpression(IdentifierName("WellKnownDiagnosticTags"), IdentifierName(WellKnownDiagnosticTags.Unnecessary))
                                : ParseExpression("Array.Empty<string>()"))
                        )))
                .AddObsoleteAttributeIf(analyzer.IsObsolete, error: true);

            if (!analyzer.IsObsolete)
            {
                var settings = new DocumentationCommentGeneratorSettings(
                    summary: new string[] { analyzer.Id },
                    ignoredTags: new[] { "returns", "value" },
                    indentation: "        ",
                    singleLineSummary: true);

                fieldDeclaration = fieldDeclaration.WithNewSingleLineDocumentationComment(settings);
            }

            return fieldDeclaration;
        }

        protected virtual ExpressionSyntax ModifyIdExpression(ExpressionSyntax expression)
        {
            return expression;
        }

        private class Rewriter : CSharpSyntaxRewriter
        {
            private int _classDeclarationDepth;

            public static Rewriter Instance { get; } = new Rewriter();

            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                _classDeclarationDepth++;
                SyntaxNode result = base.VisitClassDeclaration(node);
                _classDeclarationDepth--;

                return result;
            }

            public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
            {
                node = (FieldDeclarationSyntax)base.VisitFieldDeclaration(node);

                return node.AppendToTrailingTrivia(NewLine());
            }

            public override SyntaxNode VisitArgument(ArgumentSyntax node)
            {
                if (node.NameColon != null)
                {
                    return node
                        .WithNameColon(node.NameColon.AppendToLeadingTrivia(TriviaList(NewLine(), Whitespace(new string(' ', 4 * (2 + _classDeclarationDepth))))))
                        .WithExpression(node.Expression.PrependToLeadingTrivia(Whitespace(new string(' ', 18 - node.NameColon.Name.Identifier.ValueText.Length))));
                }

                return node;
            }

            public override SyntaxNode VisitAttribute(AttributeSyntax node)
            {
                return node;
            }
        }
    }
}
