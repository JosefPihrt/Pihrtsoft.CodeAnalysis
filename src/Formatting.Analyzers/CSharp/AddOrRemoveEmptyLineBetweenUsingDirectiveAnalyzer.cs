// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp;

namespace Roslynator.Formatting.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AddOrRemoveEmptyLineBetweenUsingDirectiveAnalyzer : BaseDiagnosticAnalyzer
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
                        DiagnosticRules.RemoveEmptyLineBetweenUsingDirectivesWithSameRootNamespace,
                        DiagnosticRules.AddEmptyLineBetweenUsingDirectivesWithDifferentRootNamespaceOrViceVersa,
                        CommonDiagnosticRules.AnalyzerOptionIsObsolete);
                }

                return _supportedDiagnostics;
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);

            context.RegisterSyntaxNodeAction(f => AnalyzeCompilationUnit(f), SyntaxKind.CompilationUnit);
            context.RegisterSyntaxNodeAction(f => AnalyzeNamespaceDeclaration(f), SyntaxKind.NamespaceDeclaration);
        }

        private static void AnalyzeCompilationUnit(SyntaxNodeAnalysisContext context)
        {
            var compilationUnit = (CompilationUnitSyntax)context.Node;

            AnalyzeUsings(context, compilationUnit.Usings);
        }

        private static void AnalyzeNamespaceDeclaration(SyntaxNodeAnalysisContext context)
        {
            var namespaceDeclaration = (NamespaceDeclarationSyntax)context.Node;

            AnalyzeUsings(context, namespaceDeclaration.Usings);
        }

        private static void AnalyzeUsings(SyntaxNodeAnalysisContext context, SyntaxList<UsingDirectiveSyntax> usings)
        {
            int count = usings.Count;

            if (count <= 1)
                return;

            UsingDirectiveSyntax usingDirective1 = usings[0];

            for (int i = 1; i < count; i++, usingDirective1 = usings[i - 1])
            {
                if (usingDirective1.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
                    return;

                if (usingDirective1.Alias != null)
                    return;

                UsingDirectiveSyntax usingDirective2 = usings[i];

                if (usingDirective2.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
                    return;

                if (usingDirective2.Alias != null)
                    return;

                SyntaxTriviaList trailingTrivia = usingDirective1.GetTrailingTrivia();

                if (!SyntaxTriviaAnalysis.IsOptionalWhitespaceThenOptionalSingleLineCommentThenEndOfLineTrivia(trailingTrivia))
                    continue;

                IdentifierNameSyntax rootNamespace1 = usingDirective1.GetRootNamespace();

                if (rootNamespace1 == null)
                    continue;

                IdentifierNameSyntax rootNamespace2 = usingDirective2.GetRootNamespace();

                if (rootNamespace2 == null)
                    continue;

                SyntaxTriviaList leadingTrivia = usingDirective2.GetLeadingTrivia();

                bool isEmptyLine = SyntaxTriviaAnalysis.StartsWithOptionalWhitespaceThenEndOfLineTrivia(leadingTrivia);

                if (string.Equals(rootNamespace1.Identifier.ValueText, rootNamespace2.Identifier.ValueText, StringComparison.Ordinal))
                {
                    if (isEmptyLine)
                    {
                        DiagnosticHelpers.ReportDiagnosticIfEffective(
                            context,
                            DiagnosticRules.RemoveEmptyLineBetweenUsingDirectivesWithSameRootNamespace,
                            Location.Create(context.Node.SyntaxTree, leadingTrivia[0].Span.WithLength(0)));
                    }
                }
                else if (DiagnosticRules.AddEmptyLineBetweenUsingDirectivesWithDifferentRootNamespaceOrViceVersa.IsEffective(context))
                {
                    if (isEmptyLine)
                    {
                        if (AnalyzerOptions.RemoveEmptyLineBetweenUsingDirectivesWithDifferentRootNamespace.IsEnabled(context))
                        {
                            DiagnosticHelpers.ReportDiagnostic(
                                context,
                                DiagnosticRules.ReportOnly.RemoveEmptyLineBetweenUsingDirectivesWithDifferentRootNamespace,
                                Location.Create(context.Node.SyntaxTree, leadingTrivia[0].Span.WithLength(0)),
                                properties: DiagnosticProperties.AnalyzerOption_Invert,
                                AnalyzerOptions.RemoveEmptyLineBetweenUsingDirectivesWithDifferentRootNamespace);
                        }
                    }
                    else if (!AnalyzerOptions.RemoveEmptyLineBetweenUsingDirectivesWithDifferentRootNamespace.IsEnabled(context))
                    {
                        DiagnosticHelpers.ReportDiagnostic(
                            context,
                            DiagnosticRules.AddEmptyLineBetweenUsingDirectivesWithDifferentRootNamespaceOrViceVersa,
                            Location.Create(context.Node.SyntaxTree, trailingTrivia.Last().Span.WithLength(0)),
                            AnalyzerOptions.RemoveEmptyLineBetweenUsingDirectivesWithDifferentRootNamespace);
                    }
                }
            }
        }
    }
}
