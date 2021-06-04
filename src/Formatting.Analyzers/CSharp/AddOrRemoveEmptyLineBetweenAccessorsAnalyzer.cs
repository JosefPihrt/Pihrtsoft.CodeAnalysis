// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp;

namespace Roslynator.Formatting.CSharp
{
    //TODO: sloučit s AccessorListAnalyzer
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AddOrRemoveEmptyLineBetweenAccessorsAnalyzer : BaseDiagnosticAnalyzer
    {
        private static ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                {
                    Immutable.InterlockedInitialize(
                        ref _supportedDiagnostics,
                        DiagnosticRules.AddEmptyLineBetweenAccessors,
                        DiagnosticRules.AddEmptyLineBetweenSingleLineAccessorsOrViceVersa,
                        CommonDiagnosticRules.AnalyzerOptionIsObsolete);
                }

                return _supportedDiagnostics;
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

            if (accessors.Count <= 1)
                return;

            Debug.Assert(accessors.Count == 2, accessors.Count.ToString());

            AccessorDeclarationSyntax accessor1 = accessors[0];

            if (accessor1.BodyOrExpressionBody() == null)
                return;

            AccessorDeclarationSyntax accessor2 = accessors[1];

            if (accessor2.BodyOrExpressionBody() == null)
                return;

            SyntaxTriviaList trailingTrivia = accessor1.GetTrailingTrivia();

            if (!SyntaxTriviaAnalysis.IsOptionalWhitespaceThenOptionalSingleLineCommentThenEndOfLineTrivia(trailingTrivia))
                return;

            SyntaxTriviaList leadingTrivia = accessor2.GetLeadingTrivia();

            bool isEmptyLine = SyntaxTriviaAnalysis.StartsWithOptionalWhitespaceThenEndOfLineTrivia(leadingTrivia);

            if (accessorList.SyntaxTree.IsSingleLineSpan(accessor1.Span, context.CancellationToken)
                && accessorList.SyntaxTree.IsSingleLineSpan(accessor2.Span, context.CancellationToken))
            {
                if (DiagnosticRules.AddEmptyLineBetweenSingleLineAccessorsOrViceVersa.IsEffective(context))
                {
                    if (isEmptyLine)
                    {
                        if (AnalyzerOptions.RemoveEmptyLineBetweenSingleLineAccessors.IsEnabled(context))
                        {
                            DiagnosticHelpers.ReportDiagnostic(
                                context,
                                DiagnosticRules.ReportOnly.RemoveEmptyLineBetweenSingleLineAccessors,
                                Location.Create(context.Node.SyntaxTree, leadingTrivia[0].Span.WithLength(0)),
                                properties: DiagnosticProperties.AnalyzerOption_Invert,
                                AnalyzerOptions.RemoveEmptyLineBetweenSingleLineAccessors);
                        }
                    }
                    else if (!AnalyzerOptions.RemoveEmptyLineBetweenSingleLineAccessors.IsEnabled(context))
                    {
                        DiagnosticHelpers.ReportDiagnostic(
                            context,
                            DiagnosticRules.AddEmptyLineBetweenSingleLineAccessorsOrViceVersa,
                            Location.Create(context.Node.SyntaxTree, trailingTrivia.Last().Span.WithLength(0)),
                            AnalyzerOptions.RemoveEmptyLineBetweenSingleLineAccessors);
                    }
                }
            }
            else if (!isEmptyLine)
            {
                DiagnosticHelpers.ReportDiagnosticIfNotSuppressed(
                    context,
                    DiagnosticRules.AddEmptyLineBetweenAccessors,
                    Location.Create(context.Node.SyntaxTree, trailingTrivia.Last().Span.WithLength(0)));
            }
        }
    }
}
