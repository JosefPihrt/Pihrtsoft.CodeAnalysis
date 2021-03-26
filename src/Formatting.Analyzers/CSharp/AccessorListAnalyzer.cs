// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp;

namespace Roslynator.Formatting.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AccessorListAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticDescriptors.RemoveNewLinesFromAccessorListOfAutoProperty,
                    DiagnosticDescriptors.AddNewLineBeforeAccessorOfFullProperty,
                    DiagnosticDescriptors.RemoveNewLinesFromAccessorWithSingleLineExpression);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeAccessorList(f), SyntaxKind.AccessorList);
        }

        private static void AnalyzeAccessorList(SyntaxNodeAnalysisContext context)
        {
            var accessorList = (AccessorListSyntax)context.Node;

            SyntaxList<AccessorDeclarationSyntax> accessors = accessorList.Accessors;

            if (accessors.Any(f => f.BodyOrExpressionBody() != null))
            {
                if (DiagnosticDescriptors.AddNewLineBeforeAccessorOfFullProperty.IsEffective(context))
                {
                    SyntaxToken token = accessorList.OpenBraceToken;

                    foreach (AccessorDeclarationSyntax accessor in accessors)
                    {
                        if (accessor.BodyOrExpressionBody() != null
                            && accessor.SyntaxTree.IsSingleLineSpan(TextSpan.FromBounds(token.Span.End, accessor.SpanStart)))
                        {
                            DiagnosticHelpers.ReportDiagnostic(
                                context,
                                DiagnosticDescriptors.AddNewLineBeforeAccessorOfFullProperty,
                                Location.Create(accessor.SyntaxTree, new TextSpan(accessor.SpanStart, 0)));

                            break;
                        }

                        token = accessor.Body?.CloseBraceToken ?? accessor.SemicolonToken;

                        if (!token.Equals(accessor.GetLastToken()))
                            break;
                    }
                }

                if (DiagnosticDescriptors.RemoveNewLinesFromAccessorWithSingleLineExpression.IsEffective(context)
                    && !accessorList.IsSingleLine(includeExteriorTrivia: false))
                {
                    foreach (AccessorDeclarationSyntax accessor in accessors)
                    {
                        if (CanRemoveNewLinesFromAccessor(accessor))
                            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.RemoveNewLinesFromAccessorWithSingleLineExpression, accessor);
                    }
                }
            }
            else if (DiagnosticDescriptors.RemoveNewLinesFromAccessorListOfAutoProperty.IsEffective(context))
            {
                SyntaxNode parent = accessorList.Parent;

                switch (parent?.Kind())
                {
                    case SyntaxKind.PropertyDeclaration:
                        {
                            if (accessors.All(f => !f.AttributeLists.Any())
                                && !accessorList.IsSingleLine(includeExteriorTrivia: false))
                            {
                                var propertyDeclaration = (PropertyDeclarationSyntax)parent;
                                SyntaxToken identifier = propertyDeclaration.Identifier;

                                if (!identifier.IsMissing)
                                {
                                    SyntaxToken closeBrace = accessorList.CloseBraceToken;

                                    if (!closeBrace.IsMissing)
                                    {
                                        TextSpan span = TextSpan.FromBounds(identifier.Span.End, closeBrace.SpanStart);

                                        if (propertyDeclaration
                                            .DescendantTrivia(span)
                                            .All(f => f.IsWhitespaceOrEndOfLineTrivia()))
                                        {
                                            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.RemoveNewLinesFromAccessorListOfAutoProperty, accessorList);
                                        }
                                    }
                                }
                            }

                            break;
                        }
                    case SyntaxKind.IndexerDeclaration:
                        {
                            if (accessors.All(f => !f.AttributeLists.Any())
                                && !accessorList.IsSingleLine(includeExteriorTrivia: false))
                            {
                                var indexerDeclaration = (IndexerDeclarationSyntax)parent;

                                BracketedParameterListSyntax parameterList = indexerDeclaration.ParameterList;

                                if (parameterList != null)
                                {
                                    SyntaxToken closeBracket = parameterList.CloseBracketToken;

                                    if (!closeBracket.IsMissing)
                                    {
                                        SyntaxToken closeBrace = accessorList.CloseBraceToken;

                                        if (!closeBrace.IsMissing)
                                        {
                                            TextSpan span = TextSpan.FromBounds(closeBracket.Span.End, closeBrace.SpanStart);

                                            if (indexerDeclaration
                                                .DescendantTrivia(span)
                                                .All(f => f.IsWhitespaceOrEndOfLineTrivia()))
                                            {
                                                DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.RemoveNewLinesFromAccessorListOfAutoProperty, accessorList);
                                            }
                                        }
                                    }
                                }
                            }

                            break;
                        }
                }
            }
        }

        private static bool CanRemoveNewLinesFromAccessor(AccessorDeclarationSyntax accessor)
        {
            BlockSyntax body = accessor.Body;

            if (body != null)
            {
                SyntaxList<StatementSyntax> statements = body.Statements;

                if (statements.Count <= 1
                    && accessor.SyntaxTree.IsMultiLineSpan(TextSpan.FromBounds(accessor.Keyword.SpanStart, accessor.Span.End))
                    && (!statements.Any() || statements[0].IsSingleLine()))
                {
                    return accessor
                        .DescendantTrivia(accessor.Span, descendIntoTrivia: true)
                        .All(f => f.IsWhitespaceOrEndOfLineTrivia());
                }
            }
            else
            {
                ArrowExpressionClauseSyntax expressionBody = accessor.ExpressionBody;

                if (expressionBody != null
                    && accessor.SyntaxTree.IsMultiLineSpan(TextSpan.FromBounds(accessor.Keyword.SpanStart, accessor.Span.End))
                    && expressionBody.Expression?.IsSingleLine() == true)
                {
                    return accessor
                        .DescendantTrivia(accessor.Span, descendIntoTrivia: true)
                        .All(f => f.IsWhitespaceOrEndOfLineTrivia());
                }
            }

            return false;
        }
    }
}
