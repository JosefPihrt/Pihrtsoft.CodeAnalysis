// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslynator.Spelling;

namespace Roslynator.CSharp.Spelling
{
    //TODO: allow parameter names in comments
    internal class CSharpSpellingWalker : CSharpSyntaxWalker
    {
        private readonly SpellingAnalysisContext _analysisContext;

        private SpellingFixerOptions Options => _analysisContext.Options;

        public CSharpSpellingWalker(SpellingAnalysisContext analysisContext)
            : base(SyntaxWalkerDepth.StructuredTrivia)
        {
            _analysisContext = analysisContext;
        }

        private void AnalyzeText(string value, SyntaxTree syntaxTree, TextSpan textSpan)
        {
            _analysisContext.CancellationToken.ThrowIfCancellationRequested();

            _analysisContext.AnalyzeText(value, textSpan, syntaxTree);
        }

        private void AnalyzeIdentifier(SyntaxToken identifier, int prefixLength = 0)
        {
            _analysisContext.CancellationToken.ThrowIfCancellationRequested();

            _analysisContext.AnalyzeIdentifier(identifier, prefixLength);
        }

        public override void VisitTrivia(SyntaxTrivia trivia)
        {
            switch (trivia.Kind())
            {
                case SyntaxKind.SingleLineCommentTrivia:
                case SyntaxKind.MultiLineCommentTrivia:
                    {
                        if (Options.IncludeNonDocumentationComments)
                            AnalyzeText(trivia.ToString(), trivia.SyntaxTree, trivia.Span);

                        break;
                    }
                case SyntaxKind.SingleLineDocumentationCommentTrivia:
                case SyntaxKind.MultiLineDocumentationCommentTrivia:
                    {
                        if (Options.IncludeDocumentationComments)
                            base.VisitTrivia(trivia);

                        break;
                    }
                case SyntaxKind.RegionDirectiveTrivia:
                case SyntaxKind.EndRegionDirectiveTrivia:
                    {
                        if (Options.IncludeNonDocumentationComments)
                            base.VisitTrivia(trivia);

                        break;
                    }
                case SyntaxKind.PreprocessingMessageTrivia:
                    {
                        Debug.Assert(Options.IncludeNonDocumentationComments);

                        AnalyzeText(trivia.ToString(), trivia.SyntaxTree, trivia.Span);
                        break;
                    }
            }
        }

        public override void VisitTupleType(TupleTypeSyntax node)
        {
            if (!Options.IncludeLocalVariable
                && node.IsParentKind(SyntaxKind.VariableDeclaration)
                && node.Parent.IsParentKind(SyntaxKind.LocalDeclarationStatement, SyntaxKind.UsingStatement))
            {
                return;
            }

            base.VisitTupleType(node);
        }

        public override void VisitTupleElement(TupleElementSyntax node)
        {
            if (node.Identifier.Parent != null)
                AnalyzeIdentifier(node.Identifier);

            base.VisitTupleElement(node);
        }

        public override void VisitArgument(ArgumentSyntax node)
        {
            if (node.Expression is DeclarationExpressionSyntax declarationExpression)
                VisitDeclarationExpression(declarationExpression);

            base.VisitArgument(node);
        }

        public override void VisitAnonymousObjectMemberDeclarator(AnonymousObjectMemberDeclaratorSyntax node)
        {
            if (node.NameEquals != null)
                AnalyzeIdentifier(node.NameEquals.Name.Identifier);

            base.VisitAnonymousObjectMemberDeclarator(node);
        }

        public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
        {
            AnalyzeIdentifier(node.Identifier);
            base.VisitLocalFunctionStatement(node);
        }

        public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            if (ShouldAnalyze(node))
                AnalyzeIdentifier(node.Identifier);

            base.VisitVariableDeclarator(node);

            bool ShouldAnalyze(VariableDeclaratorSyntax node)
            {
                Debug.Assert(node.IsParentKind(SyntaxKind.VariableDeclaration), node.Parent.Kind().ToString());

                if (node.IsParentKind(SyntaxKind.VariableDeclaration))
                {
                    Debug.Assert(
                        node.Parent.IsParentKind(
                            SyntaxKind.LocalDeclarationStatement,
                            SyntaxKind.UsingStatement,
                            SyntaxKind.ForStatement,
                            SyntaxKind.FixedStatement,
                            SyntaxKind.FieldDeclaration,
                            SyntaxKind.EventFieldDeclaration),
                        node.Parent.Parent.Kind().ToString());

                    switch (node.Parent.Parent.Kind())
                    {
                        case SyntaxKind.LocalDeclarationStatement:
                        case SyntaxKind.UsingStatement:
                        case SyntaxKind.ForStatement:
                        case SyntaxKind.FixedStatement:
                            {
                                if (!Options.IncludeLocalVariable)
                                    return false;

                                break;
                            }
                        case SyntaxKind.FieldDeclaration:
                        case SyntaxKind.EventFieldDeclaration:
                            {
                                break;
                            }
                    }
                }

                return true;
            }
        }

        public override void VisitSingleVariableDesignation(SingleVariableDesignationSyntax node)
        {
            if (Options.IncludeLocalVariable)
                AnalyzeIdentifier(node.Identifier);

            base.VisitSingleVariableDesignation(node);
        }

        public override void VisitCatchDeclaration(CatchDeclarationSyntax node)
        {
            if (Options.IncludeLocalVariable)
                AnalyzeIdentifier(node.Identifier);

            base.VisitCatchDeclaration(node);
        }

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            if (node.Alias != null)
                AnalyzeIdentifier(node.Alias.Name.Identifier);

            base.VisitUsingDirective(node);
        }

        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            VisitName(node.Name);
            base.VisitNamespaceDeclaration(node);
        }

        private void VisitName(NameSyntax node)
        {
            switch (node)
            {
                case IdentifierNameSyntax identifierName:
                    {
                        AnalyzeIdentifier(identifierName.Identifier);
                        break;
                    }
                case QualifiedNameSyntax qualifiedName:
                    {
                        VisitName(qualifiedName.Left);

                        if (qualifiedName.Right is IdentifierNameSyntax identifierName)
                            AnalyzeIdentifier(identifierName.Identifier);

                        break;
                    }
            }
        }

        public override void VisitTypeParameter(TypeParameterSyntax node)
        {
            SyntaxToken identifier = node.Identifier;
            string value = identifier.ValueText;

            int prefixLength = 0;
            if (value.Length > 1
                && value[0] == 'T'
                && char.IsUpper(value[1]))
            {
                prefixLength = 1;
            }

            AnalyzeIdentifier(identifier, prefixLength: prefixLength);
            base.VisitTypeParameter(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            AnalyzeIdentifier(node.Identifier);
            base.VisitClassDeclaration(node);
        }

        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            AnalyzeIdentifier(node.Identifier);
            base.VisitStructDeclaration(node);
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            SyntaxToken identifier = node.Identifier;
            string value = identifier.ValueText;

            int prefixLength = 0;
            if (value.Length > 1
                && value[0] == 'I'
                && char.IsUpper(value[1]))
            {
                prefixLength = 1;
            }

            AnalyzeIdentifier(identifier, prefixLength: prefixLength);
            base.VisitInterfaceDeclaration(node);
        }

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            AnalyzeIdentifier(node.Identifier);
            base.VisitEnumDeclaration(node);
        }

        public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node)
        {
            AnalyzeIdentifier(node.Identifier);
            base.VisitDelegateDeclaration(node);
        }

        public override void VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
        {
            AnalyzeIdentifier(node.Identifier);
            base.VisitEnumMemberDeclaration(node);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            AnalyzeIdentifier(node.Identifier);
            base.VisitMethodDeclaration(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            AnalyzeIdentifier(node.Identifier);
            base.VisitPropertyDeclaration(node);
        }

        public override void VisitEventDeclaration(EventDeclarationSyntax node)
        {
            AnalyzeIdentifier(node.Identifier);
            base.VisitEventDeclaration(node);
        }

        public override void VisitParameter(ParameterSyntax node)
        {
            AnalyzeIdentifier(node.Identifier);
            base.VisitParameter(node);
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            if (Options.IncludeLocalVariable)
                AnalyzeIdentifier(node.Identifier);

            base.VisitForEachStatement(node);
        }

        public override void VisitXmlElement(XmlElementSyntax node)
        {
            switch (node.StartTag.Name.LocalName.ValueText)
            {
                case "c":
                case "code":
                    return;
            }

            base.VisitXmlElement(node);
        }

        public override void VisitXmlText(XmlTextSyntax node)
        {
            if (Options.IncludeDocumentationComments)
            {
                foreach (SyntaxToken token in node.TextTokens)
                {
                    if (token.IsKind(SyntaxKind.XmlTextLiteralToken))
                        AnalyzeText(token.ValueText, node.SyntaxTree, token.Span);
                }
            }

            base.VisitXmlText(node);
        }
    }
}
