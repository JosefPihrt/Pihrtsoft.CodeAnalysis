// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.CSharp.Analysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FormatAccessorListAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.FormatAccessorList); }
        }

        public override void Initialize(AnalysisContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            base.Initialize(context);

            context.RegisterSyntaxNodeAction(AnalyzeAccessorList, SyntaxKind.AccessorList);
        }

        public static void AnalyzeAccessorList(SyntaxNodeAnalysisContext context)
        {
            var accessorList = (AccessorListSyntax)context.Node;

            SyntaxList<AccessorDeclarationSyntax> accessors = accessorList.Accessors;

            if (accessors.Any(f => f.BodyOrExpressionBody() != null))
            {
                if (accessorList.IsSingleLine(includeExteriorTrivia: false, cancellationToken: context.CancellationToken))
                {
                    ReportDiagnostic(context, accessorList);
                }
                else
                {
                    foreach (AccessorDeclarationSyntax accessor in accessors)
                    {
                        if (ShouldBeFormatted(accessor))
                            ReportDiagnostic(context, accessor);
                    }
                }
            }
            else
            {
                SyntaxNode parent = accessorList.Parent;

                switch (parent?.Kind())
                {
                    case SyntaxKind.PropertyDeclaration:
                        {
                            if (accessors.All(f => !f.AttributeLists.Any())
                                && !accessorList.IsSingleLine(includeExteriorTrivia: false, cancellationToken: context.CancellationToken))
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
                                            ReportDiagnostic(context, accessorList);
                                        }
                                    }
                                }
                            }

                            break;
                        }
                    case SyntaxKind.IndexerDeclaration:
                        {
                            if (accessors.All(f => !f.AttributeLists.Any())
                                && !accessorList.IsSingleLine(includeExteriorTrivia: false, cancellationToken: context.CancellationToken))
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
                                                ReportDiagnostic(context, accessorList);
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

        internal static bool ShouldBeFormatted(AccessorDeclarationSyntax accessor)
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

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, SyntaxNode node)
        {
            DiagnosticHelpers.ReportDiagnostic(context, DiagnosticDescriptors.FormatAccessorList, node);
        }
    }
}
